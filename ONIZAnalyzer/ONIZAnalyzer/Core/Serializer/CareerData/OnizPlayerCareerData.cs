namespace ONIZAnalyzer.Core.Serializer.CareerData;

public class OnizPlayerCareerData
{
    public required OnizGeneralCareerData GeneralCareerData { get; init; }

    public required OnizMarineCareerData MarineCareerData { get; init; }

    public required OnizZombieCareerData ZombieCareerData { get; init; }

    public void Deconstruct(out OnizGeneralCareerData generalCareerData, out OnizMarineCareerData marineCareerData, out OnizZombieCareerData zombieCareerData)
    {
        (generalCareerData, marineCareerData, zombieCareerData) = (GeneralCareerData, MarineCareerData, ZombieCareerData);
    }
}
