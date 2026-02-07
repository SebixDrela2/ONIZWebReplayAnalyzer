namespace ONIZAnalyzer.Core.Serializer.CareerData;

public abstract class OnizCareerDataBase
{
    public required int GamesPlayed { get; init; }
    public required int GamesWon { get; init; }
    public required double GamesWinPercentage { get; init; }
}
