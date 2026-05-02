using OhNoItsZombiesAnalyzer.Core.Context;

namespace OhNoItsZombiesAnalyzer.Core.Contexts;

public class ZombieContext
{
    public int Slot { get; set; }
    public int MarinesCaptured { get; set; } // WORK ON
    public string? Handle { get; set; }
    public string? ZombieName { get; set; }
    public string? FirstAlpha { get; set; }
    public bool IsWinner { get; set; }

    public HashSet<NameValue> StrainKills { get; set; } = [];
    public HashSet<NameValue> AlphaKills { get; set; } = [];
    public HashSet<NameValue> UnitBorns { get; set; } = [];
    public HashSet<SpanValuePlayer> Upgrades { get; set; } = [];
}
