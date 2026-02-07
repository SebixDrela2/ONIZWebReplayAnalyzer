using Sc2ReplayAnalyzer.Decoder.APIModel;
using System.Security.Cryptography;
using System.Text;

namespace ONIZAnalyzer.Core.Helpers;

public class OnizReplayJudge(Dictionary<string, Sc2Replay> replays)
{   
    public void Challenge(Sc2Replay replay)
    {
        var hash = Hash(replay);

        if (!replays.TryGetValue(hash, out var existing) || replay.Header.ElapsedGameLoops > existing.Header.ElapsedGameLoops)
        {
            replays[hash] = replay;
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
