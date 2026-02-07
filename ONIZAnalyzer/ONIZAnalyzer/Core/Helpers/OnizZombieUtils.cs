namespace OhNoItsZombiesAnalyzer.Core.Helpers;

public static class OnizZombieUtils
{
    public static HashSet<string> StrainUpgrades = [
       "StrainDamage", "StrainHealth", "StrainSpeed", "StrainVolatile", "PureStrainSpeed" , "PureStrainDamage" ,
        "PureStrainHealth","PureStrainVolatile" , "HunterlingStrain" ,  "DefilerStrain" , "TankStrain" , "KaboomerStrain"];

    public static HashSet<string> StrainUnits =
        ["Zergling", "HotSRaptor", "Hunterling", "InfestedCivilian", "InfestedCivilianBurrowed",
         "InfestorTerran", "DehakaMirrorImage", "Dehaka", "RoachVile", "RoachCorpser",
        "DefilerMP", "InfestedExploder", "Baneling", "Kaboomer","WarPig"];

    public static HashSet<string> ZombieUpgrades = ["AdvancedInfestationSecurity" , "SiphonFuel" , "UltimateInfestationResearched" ,
        "SpawnHiveQueen" ,"DropshipsFueled" , "BuildExtractor" , "SpawnNydusWorm" ,
        "TrackHumans" , "AdvancedInfestationContainment" , "SunkenColonyBuildCounter","EvolveDropPods", "CreepSpeed"];

    public static HashSet<string> ZombieBuildings = ["BuildExtractorProcessingFacilities",
        "BuildExtractorManufacturingSector", "BuildExtractorExcavationZone",
        "BuildVirophageDelta" , "BuildVirophageAlpha" , "BuildVirophageBeta", "LesserNydusWormBuildCounter"];

    public static HashSet<string> InfestationLevels = ["InfestationLevel2", "InfestationLevel3", "InfestationLevel4"
        ,"InfestationLevel5", "InfestationLevel6"];

    public static HashSet<string> T1Alphas = ["Abomination", "Lurker"
        ,"Anubalisk", "PrimalTownHallUprooted",  "Hunter" ,"Underseer", "Overseer"];

    public static HashSet<string> T2Alphas = ["InfestedAbomination", "LegionnaireZombie", "Anubalight", 
        "GenesplicerUprooted", "Predator2", "Saboteur"];

    public static HashSet<string> Alphas => [.. T1Alphas, .. T2Alphas];
}
