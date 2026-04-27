using OhNoItsZombiesAnalyzer.Core;
using OhNoItsZombiesAnalyzer.Core.Contexts;
using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Core.Helpers.Replay;
using ONIZAnalyzer.Core.Serializer;
using Sc2ReplayAnalyzer.Decoder;
using Sc2ReplayAnalyzer.Decoder.APIModel;
using System.Diagnostics;

namespace OhNoItsZombiesAnalyzer.Services;

public class ReplayService
{
    private const string Sc2Extension = "*.SC2Replay";

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

    private OnizReplayContext GetReplayContext(Sc2Replay replay)
    {
        var replayHandler = new OnizReplayHandler(replay);
        var context = replayHandler.GetFullContext();

        return context;
    }

    private async Task<ICollection<OnizReplayContext>> DecodeUniqueReplayContexts(Func<int, TimeSpan, Task> progressCallback)
    {
        var uniqueContexts = new Dictionary<string, OnizReplayContext>();
        var replayJudge = new OnizReplayJudge(uniqueContexts);
        var files = Directory.GetFiles(_replayPathRetriever.TrueReplaysPath, Sc2Extension, SearchOption.AllDirectories);
        var counter = 0;
        var progressTask = Task.CompletedTask;
        var watch = Stopwatch.StartNew();

        foreach (var file in files)
        {
            try
            {
                var replay = _replayDecoder.DecodeReplay(file);
                counter++;

                if (progressTask.IsCompleted)
                {
                    var averageElapsed = watch.Elapsed / counter;
                    var averageTimeLeftForAnalyze = averageElapsed * (files.Length - counter);

                    progressTask = progressCallback(counter, averageTimeLeftForAnalyze);
                }

                replayJudge.Challenge(replay);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        await progressTask;
        await progressCallback(counter, TimeSpan.Zero);

        return uniqueContexts.Values;
    }
}
