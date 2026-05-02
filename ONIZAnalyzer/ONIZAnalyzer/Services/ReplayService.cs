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
    private static readonly int DegreeOfPararellism = Environment.ProcessorCount;

    private readonly ReplayPathsRetriever _replayPathRetriever = new();
    private readonly ReplayDecoder _replayDecoder = new();

    public IReadOnlyList<FolderDto> GetReplayPathTuples() => _replayPathRetriever.GetOnizReplayFolders();

    public string GetReplayAnalyzeContent(string fullPath)
    {
        var replay = _replayDecoder.DecodeReplay(fullPath);
        var replayHanlder = new OnizReplayHandler(replay);

        return replayHanlder.GetAnalyzeText();
    }

    public async Task MassAnalyzeReplaysAsync(Func<int, TimeSpan, Task> progressCallBack)
    {
        var handleDataFolderPath = _replayPathRetriever.HandleDataFolderPath;

        EnsureDirectoryExists(handleDataFolderPath);

        var replaySerializer = new ReplaySerializer(_replayPathRetriever.HandleDataFolderPath);
        var contexts = await DecodeUniqueReplayContextsAsync(progressCallBack);

        replaySerializer.SerializeContexts(contexts);
    }

    private void EnsureDirectoryExists(string handleDataFolderPath)
    {
        if (!Directory.Exists(handleDataFolderPath))
        {
            Directory.CreateDirectory(handleDataFolderPath);
        }
    }

    private async Task<OnizReplayContext[]> DecodeUniqueReplayContextsAsync(Func<int, TimeSpan, Task> progressCallback)
    {
        var contexts = new ConcurrentBag<OnizReplayContext>();
        var files = Directory.GetFiles(_replayPathRetriever.TrueReplaysPath, Sc2Extension, SearchOption.AllDirectories);
        var length = files.Length;
        var counter = 1;
        var watch = Stopwatch.StartNew();

        var task = Parallel.ForEachAsync(files, new ParallelOptions { MaxDegreeOfParallelism = DegreeOfPararellism }, async (file, cancellationToken) =>
        {
            try
            {
                var replayDecoder = new ReplayDecoder();
                var replay = replayDecoder.DecodeReplay(file);
                Interlocked.Increment(ref counter);
                
                var replayHandler = new OnizReplayHandler(replay);
                var context = replayHandler.GetFullContext();

                if (context.IsValidContext)
                {
                    context.Hash = Hash(replay);
                    contexts.Add(context);
                }
            }
            catch (MPQException ex) 
            {
                Console.Error.WriteLine($"MPQ EXCEPTION: {ex.Message} {ex.StackTrace}");
            }
            catch (Sc2TagException ex) 
            {
                Console.Error.WriteLine($"TAG EXCEPTION: {ex.Message} {ex.StackTrace}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        });

        while(!task.IsCompleted)
        {
            var currentCounter = Volatile.Read(ref counter);

            var averageElapsed = watch.Elapsed / currentCounter;
            var averageTimeLeftForAnalyze = averageElapsed * (length - currentCounter);

            if (task.IsCompleted)
            {
                break;
            }

            await progressCallback(currentCounter, averageTimeLeftForAnalyze);
            await Task.Delay(500);
        }

        await progressCallback(Volatile.Read(ref counter), TimeSpan.Zero);

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
