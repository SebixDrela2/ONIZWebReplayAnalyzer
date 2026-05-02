using OhNoItsZombiesAnalyzer.Core.Context;
using OhNoItsZombiesAnalyzer.Core.Contexts;
using OhNoItsZombiesAnalyzer.Core.Enums;
using ONIZAnalyzer.Core.Helpers.Replay;
using ONIZAnalyzer.Core.Serializer.Full;

namespace ONIZAnalyzer.Core.Serializer;

public class OnizAllDataContextHandler
{
    private readonly OnizTranslator _translator = new();
    public OnizAllData GetOnizAllData(OnizReplayContext[] allReplays)
    {
        var privateGamesAllData = GetOnizSkillAllData(allReplays, true);
        var publicGamesAllData = GetOnizSkillAllData(allReplays, false);

        return new OnizAllData
        {
            PrivateGames = privateGamesAllData,
            PublicGames = publicGamesAllData
        };
    }

    private OnizSkillAllData GetOnizSkillAllData(OnizReplayContext[] allReplays, bool isPrivate)
    {
        var replays = allReplays
            .Where(replay => isPrivate
                ? (replay.IsProfessionalGame == isPrivate && !replay.IsPublic)
                : !replay.IsProfessionalGame)
            .ToArray();
            
        var gamesCount = replays.Length;

        var zombieAllType = gamesCount is 0 ? null : GetOnizAllTypeData(replays, true);
        var marineAllType = gamesCount is 0 ? null : GetOnizAllTypeData(replays, false);
        var averageData = gamesCount is 0 ? null : GetOnizAverageData(replays);

        return new OnizSkillAllData
        {
            AllGames = gamesCount,
            Zombie = zombieAllType,
            Marine = marineAllType,
            Average = averageData
        };
    }

    private OnizAllTypeData GetOnizAllTypeData(OnizReplayContext[] replays, bool isZombie)
    {
        var advantages = GetOnizAdvantageMap(replays, isZombie);
        var upgrades = GetOnizUpgrades(replays, isZombie);
        var unitsBorn = GetOnizUnitsBorn(replays, isZombie);
        var winCount = advantages.Sum(advantage => advantage.GamesWon);

        return new OnizAllTypeData
        {
            Wins = winCount,
            Advantages = advantages,
            Upgrades = upgrades,
            UnitsBorn = unitsBorn,
            StructuresBorn = []
        };
    }

    private OnizAdvantageMap[] GetOnizAdvantageMap(OnizReplayContext[] replays, bool isZombie)
    {
        var typeAdvantage = isZombie ? OnizAdvantage.ZombieAdvantage : OnizAdvantage.MarineAdvantage;
        var winMatchResult = isZombie ? OnizMatchResult.ZombieWin : OnizMatchResult.MarineWin;

        var replaysByAdvantage = replays
            .Where(replay => (replay.Advantage | typeAdvantage) is not 0)
            .GroupBy(replay => replay.Advantage, (advantage, groupedReplays) =>
            {
                var count = groupedReplays.Count();
                var wins = groupedReplays.Count(replay => replay.MatchResult == winMatchResult);

                return new OnizAdvantageMap(advantage, count, wins);
            })
            .ToArray();

        return replaysByAdvantage;
    }

    private NameValue[] GetOnizUpgrades(OnizReplayContext[] replays, bool isZombie)
    {
        var translateType = isZombie ? OnizTranslatorType.ZombieUpgrades : OnizTranslatorType.MarineUpgrades;
        var ugpradesTranslates = _translator.GetTranslator(translateType);

        Func<OnizReplayContext, IEnumerable<SpanValuePlayer>> upgradeSelector = isZombie
            ? replay => replay.ZombieContext.Upgrades
            : replay => replay.MarineContext.SelectMany(x => x.Upgrades);

        var upgrades = replays
            .SelectMany(upgradeSelector)
            .Where(upgrade => ugpradesTranslates.ContainsValue(upgrade.Value));

        var dict = new Dictionary<string, int>();

        foreach(var upgrade in upgrades)
        {
            if (dict.TryGetValue(upgrade.Value, out _))
            {
                ++dict[upgrade.Value];
            }
            else
            {
                dict.Add(upgrade.Value, 1);
            }
        }

        var result = dict
            .Select(kvp => new NameValue(kvp.Key, kvp.Value))
            .ToArray();

        return result;
    }

    private NameValue[] GetOnizUnitsBorn(OnizReplayContext[] replays, bool isZombie)
    {
        if (isZombie)
        {
            return [.. replays
                .SelectMany(replay => replay.ZombieContext.UnitBorns)
                .GroupBy(replay => replay.Name, (key, values) 
                    => new NameValue(key, values.Sum(value => value.Value)))];
        }

        return [];
    }

    private OnizAllAverageData GetOnizAverageData(OnizReplayContext[] replays)
    {
        var zombieAverage = GetOnizAllAverageTypeData(replays, true);
        var marineAverage = GetOnizAllAverageTypeData(replays, false);

        return new OnizAllAverageData
        {
            Zombie = zombieAverage,
            Marine = marineAverage
        };
    }

    private OnizAllAverageTypeData GetOnizAllAverageTypeData(OnizReplayContext[] replays, bool isZombie)
    {
        var averageRank = isZombie
            ? replays.Average(replay => replay.ZombieRank)
            : replays.Average(replay => replay.AverageMarineRank);

        var kills = isZombie 
            ? 0 
            : replays.SelectMany(replay => replay.MarineContext).Average(context => context.Kills);

        var specialKills = isZombie
            ? replays.Select(replay => replay.ZombieContext).Average(context => context.MarinesCaptured)
            : replays.SelectMany(replay => replay.MarineContext).Average(context => context.AlphaKills);


        return new OnizAllAverageTypeData
        {
            Rank = averageRank,
            Kills = kills,
            SpecialKills = specialKills,
        };
    }
}
