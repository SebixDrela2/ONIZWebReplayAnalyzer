using OhNoItsZombiesAnalyzer.Core.Context;

namespace ONIZAnalyzer.Core.Context;

public class BankContext
{
    public int Slot { get; set; }
    public required string Name { get; set; }
    public required string Handle { get; set; }

    public HashSet<NameValue> BankValues { get; set; } = [];
}
