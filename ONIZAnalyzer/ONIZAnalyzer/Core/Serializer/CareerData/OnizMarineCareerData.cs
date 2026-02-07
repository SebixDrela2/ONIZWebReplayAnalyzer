using System.Text.Json.Serialization;

namespace ONIZAnalyzer.Core.Serializer.CareerData;

public class OnizMarineCareerData : OnizCareerDataBase
{
    public required string FavouriteUpgrade { get; init; }
    public required string FavouriteWeapon { get; init; }

    public required int TotalKills { get; init; }
    public required int TotalAlphaKills { get; init; }

    [JsonIgnore]
    public double AverageAlphaKillsPerGame => TotalAlphaKills / GamesPlayed;
}
