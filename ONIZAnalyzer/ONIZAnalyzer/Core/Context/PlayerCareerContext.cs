using OhNoItsZombiesAnalyzer.Core.Contexts;

namespace ONIZAnalyzer.Core.Context;

public record class PlayerCareerContext(
    IReadOnlyList<OnizReplayContext> AllGamesContext,
    IReadOnlyList<MarineContext> MarineCareerContext, 
    IReadOnlyList<ZombieContext> ZombieCareerContext);
