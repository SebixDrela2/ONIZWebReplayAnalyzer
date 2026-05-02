namespace OhNoItsZombiesAnalyzer.Core.Enums;

[Flags]
public enum OnizAdvantage
{   
    NoAdvantage = 0,
    ExtremeMarineAdvantage = 1 << 0,
    MajorMarineAdvantage = 1 << 1,
    RegularMarineAdvantage = 1 << 2,
    MinorMarineAdvantage = 1 << 3,
    MinorZombieAdvantage = 1 << 4,
    RegularZombieAdvantage = 1 << 5,
    MajorZombieAdvantage = 1 << 6,
    ExtremeZombieAdvantage = 1 << 7,
    NotFullGame = 1 << 8,

    MarineAdvantage = ExtremeMarineAdvantage | MajorMarineAdvantage | RegularMarineAdvantage | MinorMarineAdvantage,
    ZombieAdvantage = ExtremeZombieAdvantage | MajorZombieAdvantage | RegularZombieAdvantage | MinorZombieAdvantage,
}
