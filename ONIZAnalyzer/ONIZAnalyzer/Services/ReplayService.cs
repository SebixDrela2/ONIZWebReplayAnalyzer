using MPQArchive.MPQ.Utils;
using OhNoItsZombiesAnalyzer.Core;
using OhNoItsZombiesAnalyzer.Core.Contexts;
using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Core.Helpers.Replay;
using ONIZAnalyzer.Core.Serializer;
using Sc2ReplayAnalyzer.Decoder;
using Sc2ReplayAnalyzer.Decoder.APIModel;
using Sc2ReplayAnalyzer.Decoder.Exceptions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace OhNoItsZombiesAnalyzer.Services;

public class ReplayService
{
    private const string Sc2Extension = "*.SC2Replay";
    private const int ConcurrentReplays = 20;

    private readonly ReplayPathsRetriever _replayPathRetriever = new();
    private readonly ReplayDecoder _replayDecoder = new();

    public IReadOnlyList<FolderDto> GetReplayPathTuples() => _replayPathRetriever.GetOnizReplayFolders();

    public string GetReplayAnalyzeContent(string fullPath)
    {
        var replay = _replayDecoder.DecodeReplay(fullPath);
        var replayHanlder = new OnizReplayHandler(replay);

        return replayHanlder.GetAnalyzeText();
    }

    public async Task MassAnalyzeReplays(Func<int, TimeSpan, Task> progressCallBack)
    {
        var handleDataFolderPath = _replayPathRetriever.HandleDataFolderPath;

        EnsureCleanDirectoryExists(handleDataFolderPath);

        var replaySerializer = new ReplaySerializer(_replayPathRetriever.HandleDataFolderPath);
        var contexts = await DecodeUniqueReplayContexts(progressCallBack);

        replaySerializer.SerializeContexts(contexts);
    }

    private void EnsureCleanDirectoryExists(string handleDataFolderPath)
    {
        if (Directory.Exists(handleDataFolderPath))
        {
            Directory.Delete(handleDataFolderPath, recursive: true);
        }

        Directory.CreateDirectory(handleDataFolderPath);
    }

    private async Task<ICollection<OnizReplayContext>> DecodeUniqueReplayContexts(Func<int, TimeSpan, Task> progressCallback)
    {
        var contexts = new ConcurrentBag<OnizReplayContext>();
        var files = Directory.GetFiles(_replayPathRetriever.TrueReplaysPath, Sc2Extension, SearchOption.AllDirectories);
        var length = files.Length;
        var counter = 0;
        var watch = Stopwatch.StartNew();

        // Track the progress task with Interlocked for thread safety
        var lastProgressTask = Task.CompletedTask;

        await Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = ConcurrentReplays }, async (file, cancellationToken) =>
        {
            try
            {
                var replayDecoder = new ReplayDecoder();
                var replay = replayDecoder.DecodeReplay(file);
                var incrementedCounter = Interlocked.Increment(ref counter);

                var averageElapsed = watch.Elapsed / incrementedCounter;
                var averageTimeLeftForAnalyze = averageElapsed * (length - incrementedCounter);

                var newProgressTask = progressCallback(incrementedCounter, averageTimeLeftForAnalyze);
                var oldTask = Interlocked.Exchange(ref lastProgressTask, newProgressTask);

                var replayHandler = new OnizReplayHandler(replay);
                var context = replayHandler.GetFullContext();

                if (context.IsValidContext)
                {
                    context.Hash = Hash(replay);
                    contexts.Add(context);
                }
            }
            catch (Exception ex) when (ex is MPQException or Sc2TagException)
            {
                // Invalid MPQHeader, corrupted replay.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        });

        await lastProgressTask;
        await progressCallback(counter, TimeSpan.Zero);

        var uniqueContexts = contexts
            .GroupBy(x => x.Hash, (x, y) => y.MaxBy(z => z.ElapsedGameLoops))
            .ToArray();

        return uniqueContexts!;
    }

    private static OnizHash Hash(Sc2Replay replay)
    {
        long randomValue = replay.InitData.GameDescription.RandomValue;
        long startTimeUtc = replay.Details.Time;

        return new OnizHash(randomValue, startTimeUtc);
    }
}
