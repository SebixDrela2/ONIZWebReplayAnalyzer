using OhNoItsZombiesAnalyzer.Core;
using OhNoItsZombiesAnalyzer.Core.Contexts;
using OhNoItsZombiesAnalyzer.Core.Helpers;
using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Core.Helpers;
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

    public IReadOnlyList<ReplayFolderDto> GetReplayPathTuples() => _replayPathRetriever.GetOnizReplayFolders();

    public string GetReplayAnalyzeContent(string fullPath)
    {
        var replay = _replayDecoder.DecodeReplay(fullPath);
        var replayHanlder = new OnizReplayHandler(replay);

        return replayHanlder.GetSingleReplayAnalyzeText();
    }

    public async Task MassAnalyzeReplays(Func<int, TimeSpan, Task> progressCallBack)
    {
        var handleDataFolderPath = _replayPathRetriever.HandleDataFolderPath;

        EnsureCleanDirectoryExists(handleDataFolderPath);

        var replaySerializer = new ReplaySerializer(_replayPathRetriever.HandleDataFolderPath);
        var replays = await DecodeUniqueReplays(progressCallBack);

        var replayFiles = replays.Select(x => x.FileName).ToList();
        var contexts = replays
            .Select(GetReplayContext)
            .Where(context => context.IsValidContext)
            .ToList();
        
        foreach(var context in contexts)
        {
            replaySerializer.SerializeContexts(contexts);
        }
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

    private async Task<ICollection<Sc2Replay>> DecodeUniqueReplays(Func<int, TimeSpan, Task> progressCallback)
    {
        var uniqueReplays = new Dictionary<string, Sc2Replay>();
        var replayJudge = new OnizReplayJudge(uniqueReplays);
        var files = Directory.GetFiles(_replayPathRetriever.AccountsPath, Sc2Extension, SearchOption.AllDirectories);
        var counter = 0;
        var progressTask = Task.CompletedTask;
        var watch = Stopwatch.StartNew();
        
        foreach (var file in files)
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

        await progressTask;
        await progressCallback(counter, TimeSpan.Zero);

        return uniqueReplays.Values;
    }
}
