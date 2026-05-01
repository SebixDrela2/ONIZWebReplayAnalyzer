using OhNoItsZombiesAnalyzer.Core.Context;

namespace OhNoItsZombiesAnalyzer.Core.Contexts;

public class MarineContext
{
    public int Slot { get; set; }
    public string Handle { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Kills { get; set; }
    public int AlphaKills { get; set; }

    public bool IsWinner { get; set; }
    public int Deaths { get; set; }

    public Dictionary<string, int> AlphaKillsDict { get; set; } = [];

    public Dictionary<string, int> StrainKillsDict { get; set; } = [];

    public HashSet<SpanValuePlayer> Upgrades { get; set; } = [];
}
