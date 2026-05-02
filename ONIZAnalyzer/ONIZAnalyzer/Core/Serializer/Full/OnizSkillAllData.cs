using OhNoItsZombiesAnalyzer.Core.Context;
using OhNoItsZombiesAnalyzer.Core.Enums;

namespace ONIZAnalyzer.Core.Serializer.Full;

public class OnizAllData
{
    public required OnizSkillAllData? PrivateGames { get; init; }
    public required OnizSkillAllData? PublicGames { get; init; }
}

public class OnizSkillAllData
{
    public required int? AllGames { get; init; }
    public required OnizAllTypeData? Zombie { get; init; }
    public required OnizAllTypeData? Marine { get; init; }
    public required OnizAllAverageData? Average { get; init; }
}

public class OnizAllTypeData
{
    public required int? Wins { get; init; }
    public required OnizAdvantageMap[]? Advantages { get; init; }
    public required NameValue[]? Upgrades { get; init; }
    public required NameValue[]? UnitsBorn { get; init; }
    public required NameValue[]? StructuresBorn { get; init; }
}

public class OnizAllAverageData
{
    public required double? Diverts { get; init; }
    public required OnizAllAverageTypeData? Zombie { get; init; }
    public required OnizAllAverageTypeData? Marine { get; init; }
}

public class OnizAllAverageTypeData
{
    public required double? Rank { get; init; }
    public required double? Kills { get; init; }
    public required double? SpecialKills { get; init; }
}

public record class OnizAdvantageMap(OnizAdvantage? OnizAdvantage, int? GamesPlayed, int? GamesWon);
