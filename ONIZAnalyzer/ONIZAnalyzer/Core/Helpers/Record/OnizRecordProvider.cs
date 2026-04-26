using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Common.Models.Record;
using ONIZAnalyzer.Core.Serializer.CareerData;

namespace ONIZAnalyzer.Core.Helpers.Record;

public class OnizRecordProvider
{
    private const string None = nameof(None);

    private static readonly Dictionary<OnizRecordSortOption, OnizPlayerCareerDataComparer> _sortMap = new()
    {
        { new(OnizRecordOptionType.General, nameof(OnizGeneralCareerData.LastGamePlayed)), Create(x => x.GeneralCareerData.LastGamePlayed)},
        { new(OnizRecordOptionType.General, nameof(OnizGeneralCareerData.MonthsOfService)), Create(x => x.GeneralCareerData.MonthsOfService)},
        { new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.GamesWon)), Create(x => x.MarineCareerData.GamesWon)},
        { new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.GamesPlayed)), Create(x => x.MarineCareerData.GamesPlayed)},
        { new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.GamesWinPercentage)), Create(x => x.MarineCareerData.GamesWinPercentage)},
        { new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.TotalKills)), Create(x => x.MarineCareerData.TotalKills)},
        { new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.TotalAlphaKills)), Create(x => x.MarineCareerData.TotalAlphaKills)},
        { new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.AverageAlphaKillsPerGame)), Create(x => x.MarineCareerData.AverageAlphaKillsPerGame)},
        { new(OnizRecordOptionType.Zombie, nameof(OnizZombieCareerData.GamesWon)), Create(x => x.ZombieCareerData.GamesWon)},
        { new(OnizRecordOptionType.Zombie, nameof(OnizZombieCareerData.GamesPlayed)), Create(x => x.ZombieCareerData.GamesPlayed)},
        { new(OnizRecordOptionType.Zombie, nameof(OnizZombieCareerData.GamesWinPercentage)), Create(x => x.ZombieCareerData.GamesWinPercentage) },
    };

    private static readonly OnizRecordSortOption[] _sortOptions = [
        new(OnizRecordOptionType.None, None),
        .._sortMap.Keys,
    ];

    public OnizRecordSortOption[] ProvideSortOptions() => _sortOptions;

    public OnizPlayerCareerDataComparer GetCareerComparer(OnizRecordSortOption sortOption) => _sortMap[sortOption];

    private static OnizPlayerCareerDataComparer Create<T>(Func<OnizPlayerCareerData, T> selector)
        where T : IComparable<T> => new((x, y) => selector(x).CompareTo(selector(y)));
}
