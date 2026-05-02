using ONIZAnalyzer.Core.Context;
using ONIZAnalyzer.Core.Serializer.CareerData;

namespace ONIZAnalyzer.Core.Serializer;

public class OnizPlayerCareerContextHandler(string directoryPath)
{
    public OnizPlayerCareerData GetOnizCareerData(PlayerCareerContext careerContext)
    {
        var zombieCareerData = GetZombieCareerData(careerContext);
        var marineCareerData = GetMarineCareerData(careerContext);
        var generalCareerData = GetGeneralCareerData(careerContext);

        return new OnizPlayerCareerData
        {
            GeneralCareerData = generalCareerData,
            MarineCareerData = marineCareerData,
            ZombieCareerData = zombieCareerData
        };

    }

    private OnizGeneralCareerData GetGeneralCareerData(PlayerCareerContext careerContext)
    {
        var orderedGames = careerContext.AllGamesContext.OrderBy(x => x.TimeGameStarted);

        var firstDayOfService = orderedGames.First().TimeGameStarted;
        var lastDayOfService = orderedGames.Last().TimeGameStarted;
        var monthsService =
            ((lastDayOfService.Year - firstDayOfService.Year) * 12) +
            lastDayOfService.Month - firstDayOfService.Month;
       
        if (lastDayOfService.Day < firstDayOfService.Day)
        {
            monthsService--;
        }

        return new OnizGeneralCareerData
        {
            MonthsOfService = monthsService,
            LastGamePlayed = lastDayOfService,
        };
    }

    private OnizZombieCareerData GetZombieCareerData(PlayerCareerContext careerContext)
    {
        var careerZombieContext = careerContext.ZombieCareerContext;
        var zombieGamesPlayed = careerZombieContext.Count;
        var zombieGamesWon = careerZombieContext.Count(context => context.IsWinner);

        var zombieWinPercentage = zombieGamesPlayed is not 0
            ? Math.Round((double)zombieGamesWon / zombieGamesPlayed, 2)
            : 0;

        var favouriteAlphaOpener = careerZombieContext
           .GroupBy(replayContext => replayContext.FirstAlpha)
           .OrderByDescending(x => x.Count())
           .Select(g => g.Key)
           .FirstOrDefault();

        var favouriteZombieUpgrade = careerZombieContext
            .SelectMany(context => context.Upgrades)
            .GroupBy(x => x.Value)
            .OrderByDescending(x => x.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        return new OnizZombieCareerData
        {
            GamesPlayed = zombieGamesPlayed,
            GamesWon = zombieGamesWon,
            GamesWinPercentage = zombieWinPercentage,
            FavouriteAlphaOpener = favouriteAlphaOpener ?? "No alphas on record",
            FavouriteZombieUpgrade = favouriteZombieUpgrade ?? "No upgrade on record"
        };
    }

    private OnizMarineCareerData GetMarineCareerData(PlayerCareerContext careerContext)
    {
        var careerMarineContext = careerContext.MarineCareerContext;

        var allUpgradesGroupedOrdered = careerMarineContext
           .SelectMany(context => context.Upgrades)
           .GroupBy(x => x.Value)
           .OrderByDescending(x => x.Count())
           .Select(g => g.Key);


        var marineGamesPlayed = careerMarineContext.Count;
        var marineGamesWon = careerMarineContext.Count(context => context.IsWinner);

        var marineWinPercentage = marineGamesPlayed is not 0
           ? Math.Round((double)marineGamesWon / marineGamesPlayed, 2)
           : 0;

        var favouriteMarineUpgrade = allUpgradesGroupedOrdered
           .FirstOrDefault();

        var favouriteWeapon = allUpgradesGroupedOrdered
            .FirstOrDefault(OnizTranslationGroups.FirstWeaponUpgrades.Contains);

        var totalKills = careerMarineContext.Sum(replayContext => replayContext.Kills);
        var totalAlphaKills = careerMarineContext.Sum(replayContext => replayContext.AlphaKills);

        return new OnizMarineCareerData
        {
            GamesPlayed = marineGamesPlayed,
            GamesWon = marineGamesWon,
            GamesWinPercentage = marineWinPercentage,
            FavouriteUpgrade = favouriteMarineUpgrade ?? "No upgrade on record",
            FavouriteWeapon = favouriteWeapon ?? "No weapon on record",
            TotalKills = totalKills,
            TotalAlphaKills = totalAlphaKills,
        };
    }
}
