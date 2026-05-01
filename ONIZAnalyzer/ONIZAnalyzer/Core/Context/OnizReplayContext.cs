using OhNoItsZombiesAnalyzer.Core.Enums;
using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Core.Context;
using ONIZAnalyzer.Core.Helpers.Replay;
using System.Text.Json.Serialization;

namespace OhNoItsZombiesAnalyzer.Core.Contexts;

public class OnizReplayContext
{
    public IReadOnlyList<MarineContext> MarineContext { get; set; } = [];
    public IReadOnlyList<BankContext> PlayersBankContext { get; set; } = [];
    public ZombieContext ZombieContext { get; set; } = new ZombieContext();

    public HashSet<string> MarineHandles = [];
    public HashSet<NameHandle> NameHandles { get; set; } = [];
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

    public int MarineCount => PlayerCount - 1;
    public bool IsProfessionalGame => MarineGrandMasterCount > 4;

    public IEnumerable<string> Handles => NameHandles.Select(x => x.Handle);
    public MarineContext GetPlayerContext(int slotId) => MarineContext.Single(context => context.Slot == slotId);


    [JsonIgnore]
    public int ElapsedGameLoops;

    [JsonIgnore]
    public OnizHash? Hash;
}
