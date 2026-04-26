using OhNoItsZombiesAnalyzer.Core.Context;
using Sc2ReplayAnalyzer.Decoder.APIModel;

namespace ONIZAnalyzer.Core.Helpers.Replay;

public static class OnizMarineUtils
{
    public static void AddPurchases(HashSet<SpanValuePlayer> purchasesSet, HashSet<string> purchasesChallenger, Sc2Replay replay, int playerId)
    {
        var upgrades = replay.TrackerEvents.SUpgradeEvents
                    .Where(e => purchasesChallenger.Contains(e.UpgradeTypeName) && e.PlayerId - 1 == playerId)
                    .Distinct();

        foreach (var upgrade in upgrades)
        {
            var playerName = replay.Details.Players.Single(e => e.Slot == upgrade.PlayerId - 1);
            purchasesSet.Add(new SpanValuePlayer(upgrade.Gameloop, upgrade.UpgradeTypeName, playerName.Name));
        }
    }
}
