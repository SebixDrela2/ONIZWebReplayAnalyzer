using ONIZAnalyzer.Common.Models.Record;
using ONIZAnalyzer.Core.Serializer.CareerData;

namespace ONIZAnalyzer.Core.Helpers.Record;

public class OnizRecordProvider
{
    private const string None = nameof(None);

    private static readonly OnizRecordSortOption[] _sortOptions = [
        new(OnizRecordOptionType.None, None),
        new(OnizRecordOptionType.General, nameof(OnizGeneralCareerData.LastGamePlayed)),
        new(OnizRecordOptionType.General, nameof(OnizGeneralCareerData.MonthsOfService)),
        new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.GamesWon)),
        new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.GamesPlayed)),
        new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.GamesWinPercentage)),
        new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.TotalKills)),
        new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.TotalAlphaKills)),
        new(OnizRecordOptionType.Marine, nameof(OnizMarineCareerData.AverageAlphaKillsPerGame)),
        new(OnizRecordOptionType.Zombie, nameof(OnizZombieCareerData.GamesWon)),
        new(OnizRecordOptionType.Zombie, nameof(OnizZombieCareerData.GamesPlayed)),
        new(OnizRecordOptionType.Zombie, nameof(OnizZombieCareerData.GamesWinPercentage)),
    ];

    public OnizRecordSortOption[] ProvideSortOptions() => _sortOptions;
}
