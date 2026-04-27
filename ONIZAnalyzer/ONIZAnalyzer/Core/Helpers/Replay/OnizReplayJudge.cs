using OhNoItsZombiesAnalyzer.Core.Contexts;
using Sc2ReplayAnalyzer.Decoder.APIModel;
using System.Security.Cryptography;
using System.Text;

namespace ONIZAnalyzer.Core.Helpers.Replay;

public class OnizReplayJudge(Dictionary<string, OnizReplayContext> replays)
{
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

    private string Hash(Sc2Replay replay)
    {
        long randomValue = replay.InitData.GameDescription.RandomValue;
        long startTimeUtc = replay.Details.Time;
        string input = $"{randomValue}:{startTimeUtc}";
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return Convert.ToHexString(hashBytes);
    }
}
