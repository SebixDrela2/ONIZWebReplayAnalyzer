using OhNoItsZombiesAnalyzer.Core.Enums;
using ONIZAnalyzer.Core.Context;
using ONIZAnalyzer.Core.Helpers.Replay;
using System.Text.Json.Serialization;

namespace OhNoItsZombiesAnalyzer.Core.Contexts;

public class OnizReplayContext
{
    public IReadOnlyList<MarineContext> MarineContext { get; set; } = [];
    public IReadOnlyList<BankContext> PlayersBankContext { get; set; } = [];
    public ZombieContext ZombieContext { get; set; } = new ZombieContext();

    public bool IsValidContext { get; set; } = false;

    public int OverMindIndex { get; set; }
    public string? ZombiePlayer { get; set; }
    public OnizMatchResult MatchResult { get; set; }
    public OnizAdvantage Advantage { get; set; }
    public TimeSpan GameLength { get; set; }

    public DateTime TimeGameStarted { get; set; }

    public int PlayerCount { get; set; }
    public int MarineGrandMasterCount { get; set; }
    public double AverageMarineRank { get; set; }
    public int ZombieRank { get; set; }

    public IReadOnlyList<(string Name , string Handle)> HandleNames 
        => [.. MarineContext.Select(context 
            => (context.Name!, context.Handle!)), (ZombieContext.ZombieName, ZombieContext.Handle)!];

    public int MarineCount => PlayerCount - 1;
    public bool IsProfessionalGame => MarineGrandMasterCount > 4;
    public MarineContext GetPlayerContext(int slotId) => MarineContext.Single(context => context.Slot == slotId);


    [JsonIgnore]
    public int ElapsedGameLoops;

    [JsonIgnore]
    public OnizHash? Hash;
}
