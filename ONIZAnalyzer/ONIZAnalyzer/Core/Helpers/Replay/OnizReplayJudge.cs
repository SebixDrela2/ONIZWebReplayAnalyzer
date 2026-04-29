using OhNoItsZombiesAnalyzer.Core.Contexts;
using Sc2ReplayAnalyzer.Decoder.APIModel;
namespace ONIZAnalyzer.Core.Helpers.Replay;

public class OnizReplayJudge(Dictionary<OnizHash, OnizReplayContext> replays)
{
    public void Challenge(Sc2Replay[] replays)
    {
        foreach(var replay in replays)
        {
            Challenge(replay);
        }
    } 

    public void Challenge(Sc2Replay replay)
    {
        var hash = Hash(replay);

        if (!replays.TryGetValue(hash, out var existing) || replay.Header.ElapsedGameLoops > existing.ElapsedGameLoops)
        {
            var replayHandler = new OnizReplayHandler(replay);
            var context = replayHandler.GetFullContext();

            if (!context.IsValidContext)
            {               
                return;
            }
            
            replays[hash] = context;
        }
    }

    private OnizHash Hash(Sc2Replay replay)
    {
        long randomValue = replay.InitData.GameDescription.RandomValue;
        long startTimeUtc = replay.Details.Time;

        return new OnizHash(randomValue, startTimeUtc);
    }
}

public class OnizHash(long RandomValue, long Time);
