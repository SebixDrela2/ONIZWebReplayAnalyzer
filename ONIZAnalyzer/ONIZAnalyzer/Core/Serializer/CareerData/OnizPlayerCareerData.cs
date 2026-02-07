namespace ONIZAnalyzer.Core.Serializer.CareerData;

public class OnizPlayerCareerData
{
    public required OnizGeneralCareerData GeneralCareerData { get; init; }

    public required OnizMarineCareerData MarineCareerData { get; init; }

    public required OnizZombieCareerData ZombieCareerData { get; init; }
}
