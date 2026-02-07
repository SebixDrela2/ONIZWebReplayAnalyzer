using OhNoItsZombiesAnalyzer.Core.Context;
using OhNoItsZombiesAnalyzer.Core.Contexts;
using OhNoItsZombiesAnalyzer.Core.Enums;
using OhNoItsZombiesAnalyzer.Core.Helpers;
using ONIZAnalyzer.Core.Context;
using Sc2ReplayAnalyzer.Decoder.APIModel;
using Sc2ReplayAnalyzer.Decoder.Events.GameEvents;
using Sc2ReplayAnalyzer.Decoder.Events.TrackerEvents;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;

namespace OhNoItsZombiesAnalyzer.Core;

public class OnizContextAnalyzer(Sc2Replay replay)
{
    private const string MarineRank = "HumanRank";
    private const string ZombieRank = "ZombieRank";
    private const string MarineLossIndicator = "ColonistShipFlying";
    private const string Marine = "WarPig";
    private const string NoAlpha = "No Alpha";
    private const string OverMind = "Overmind";
    private const string ZombieRace = "Zerg";

    private const int GrandMasterBarrier = 24;
    private const int VespeneStartLoop = 640;

    private readonly OnizTranslator _translator = new(replay.Details);

    public OnizReplayContext GetReplayContext()
    {
        var replayContext = GetGeneralReplayContext();

        if (!replayContext.IsValidContext)
        {
            return replayContext;
        }

        AsignZombieContext(replayContext);
        AsignMarineContext(replayContext);
        AsignBankContext(replayContext.PlayersBankContext);

        return replayContext;
    }

    private OnizReplayContext GetGeneralReplayContext()
    {
        if (!replay.Details.Players.Any(player => player.Race is ZombieRace)
            || replay.Details.Players.Count < 3)
        {
            // Single player game, or some test game
            return new OnizReplayContext();
        }

        var replayContext = new OnizReplayContext();
        var overMindBornEvent = replay.TrackerEvents.SUnitBornEvents.Single(e => e.UnitTypeName.Contains(OverMind));
        replayContext.OverMindIndex = replay.TrackerEvents.SUnitBornEvents.First(e => e.UnitTypeName.Contains(OverMind)).UnitTagIndex;

        var playerCount = replay.Details.Players.Count - 1;
        var zombieId = replay.TrackerEvents.SUnitOwnerChangeEvents.First(e => e.UnitTagIndex == replayContext.OverMindIndex).ControlPlayerId - 1;
        var zombieName = _translator.GetPlayerName(zombieId);
        var bankKeyEvents = replay.GameEvents.OfEventType<SBankKeyEvent>();
        var (marineRankAvg, zombieRank) = GetOnizRaceRanks(bankKeyEvents, zombieId, playerCount);

        replayContext.TimeGameStarted = DateTime.FromFileTime(replay.Details.Time);
        replayContext.GameLength = replay.Header.ElapsedGameLoops.GetTimeSpan();
        replayContext.PlayerCount = playerCount;
        replayContext.MatchResult = GetMatchResult(replay.TrackerEvents.SUnitTypeChangeEvents);

        replayContext.MarineGrandMasterCount = bankKeyEvents
            .Where(e => e.Name is MarineRank)
            .Count(e => OnizUtils.GetDoubleData(e.Data) > GrandMasterBarrier);

        replayContext.AverageMarineRank = marineRankAvg;
        replayContext.ZombieRank = zombieRank;
        replayContext.Advantage = GetOnizAdvantage(replay.TrackerEvents.SPlayerStatsEvents, zombieName);
        replayContext.MarineContext = [.. replay.Details.Players

            .Where(x => x.Name != zombieName)
            .Select(idx => new MarineContext{ Slot = (int)idx.Slot })];
        replayContext.ZombieContext = new ZombieContext { Slot = zombieId };
        replayContext.PlayersBankContext = [.. replay.Details.Players
            .Select(x => new BankContext { Slot = (int)x.Slot, Handle = x.Toon.GetHandle(), Name = x.Name })];
        replayContext.IsValidContext = true;

        return replayContext;
    }

    private void AsignBankContext(IReadOnlyList<BankContext> bankContexts)
    {
        var bankEntries = replay.GameEvents.OfEventType<SBankKeyEvent>();

        foreach(var bankContext in bankContexts)
        {
            var playerBankEntries = bankEntries.Where(entry => entry.UserId == bankContext.Slot);

            AsignBankValues(bankContext.BankValues, playerBankEntries);
        }
    }

    private void AsignBankValues(HashSet<NameValue> bankValues, IEnumerable<SBankKeyEvent> bankKeyEvents)
    {
        var translator = _translator.GetTranslator(OnizTranslatorType.BankEntries);
        var validBankEntries = bankKeyEvents.Where(e => translator.ContainsKey(e.Name));

        foreach(var entry in validBankEntries)
        {
            var translatedEntry = _translator.Translate(entry.Name, OnizTranslatorType.BankEntries);
            bankValues.Add(new NameValue(translatedEntry, int.Parse(entry.Data)));
        }
    }

    private void AsignZombieContext(OnizReplayContext gameContext)
    {
        var context = gameContext.ZombieContext;
        var zombieDetails = replay.Details.Players.Single(detail => detail.Slot == context.Slot);
        var zombieName = _translator.GetPlayerName(context.Slot);

        context.FirstAlpha = replay.TrackerEvents.SUnitBornEvents
            .FirstOrDefault(e => OnizZombieUtils.T1Alphas.Contains(e.UnitTypeName))?.UnitTypeName ?? NoAlpha;
        context.ZombieName = zombieName;
        context.Handle = zombieDetails.Toon.GetHandle();
        context.IsWinner = gameContext.MatchResult is OnizMatchResult.ZombieWin;

        AddPurchases(context.Upgrades, context.Slot, OnizTranslatorType.ZombieUpgrades);
        AddKillsSpecific(context.StrainKills, context.Slot, OnizTranslatorType.ZombieStrains);
        AddKillsSpecific(context.AlphaKills, context.Slot, OnizTranslatorType.ZombieAlphas);
    }

    private void AsignMarineContext(OnizReplayContext gameContext)
    {
        var unitDiedEvents = replay.TrackerEvents.SUnitDiedEvents;
        var unitBornEvents = replay.TrackerEvents.SUnitBornEvents;

        foreach (var context in gameContext.MarineContext)
        {
            AddPurchases(context.Upgrades, context.Slot, OnizTranslatorType.MarineUpgrades);

            var player = replay.Details.Players.Single(e => e.Slot == context.Slot);

            context.Name = player.Name;
            context.Handle = player.Toon.GetHandle();
            context.Kills = GetKills(context.Slot);

            context.AlphaKills = unitBornEvents
                .Count(e => OnizZombieUtils.Alphas.Contains(e.UnitTypeName)
                && e.SUnitDiedEvent?.KillerUnitBornEvent?.UnitTypeName == Marine 
                && OnizUtils.IsCorrectSlot(e.SUnitDiedEvent.KillerUnitBornEvent.ControlPlayerId, context.Slot));

            context.Deaths = unitBornEvents
                .Count(e => e.UnitTypeName is Marine && OnizUtils.IsCorrectSlot(e.ControlPlayerId, context.Slot) 
                && e.SUnitDiedEvent is { });

            context.IsWinner = gameContext.MatchResult is OnizMatchResult.MarineWin;
        }
    }

    private (double, int) GetOnizRaceRanks(IEnumerable<SBankKeyEvent> bankKeyEvents, int zombieId, int playerCount)
    {
        var marineCount = playerCount - 1;
        var marineRankAvg = Math.Round(bankKeyEvents
            .Where(x => x.Name is MarineRank)
            .Sum(e => double.Parse(e.Data, CultureInfo.InvariantCulture))/marineCount, 2);

        var zombieRank = (int)OnizUtils.GetDoubleData(
            bankKeyEvents.SingleOrDefault(x => x.Name is ZombieRank && x.UserId == zombieId)?.Data ?? "0");

        return (marineRankAvg, zombieRank);
    }

    private OnizMatchResult GetMatchResult(IReadOnlyList<SUnitTypeChangeEvent> unitTypeChangeEvents)
    {
        var result = replay.TrackerEvents.SUnitTypeChangeEvents.FirstOrDefault(e => e.UnitTypeName == MarineLossIndicator);

        if (result is not null)
        {
            return OnizMatchResult.MarineWin;
        }

        return OnizMatchResult.ZombieWin;
    }

    private OnizAdvantage GetOnizAdvantage(IReadOnlyList<SPlayerStatsEvent> playerStatsEvents, string zombieName)
    {
        var marineVespene = playerStatsEvents
            .Where(e => e.Gameloop is VespeneStartLoop && e.ScoreValueVespeneCurrent > 200 && _translator.GetPlayerName(e.PlayerId - 1) != zombieName)
            .OrderBy(e => e.ScoreValueVespeneCurrent)
            .FirstOrDefault()?.ScoreValueVespeneCurrent;

        if (marineVespene is not null)
        {
            return OnizAdvantage.NotFullGame;
        }

        return marineVespene switch
        {
            400 => OnizAdvantage.ExtremeMarineAdvantage,
            375 => OnizAdvantage.MajorMarineAdvantage,
            350 => OnizAdvantage.RegularMarineAdvantage,
            325 => OnizAdvantage.MinorMarineAdvantage,
            300 => OnizAdvantage.NoAdvantage,
            275 => OnizAdvantage.MinorZombieAdvantage,
            250 => OnizAdvantage.RegularZombieAdvantage,
            225 => OnizAdvantage.MajorZombieAdvantage,
            220 => OnizAdvantage.ExtremeZombieAdvantage,
            var x => OnizAdvantage.NotFullGame
        };
    }

    private void AddPurchases(HashSet<SpanValuePlayer> purchasesSet, int playerId, OnizTranslatorType translatorType)
    {
        var translator = _translator.GetTranslator(translatorType);
        var upgrades = replay.TrackerEvents.SUpgradeEvents
                    .Where(e => translator.Keys.Contains(e.UpgradeTypeName) && e.PlayerId - 1 == playerId)
                    .Distinct();

        foreach (var upgrade in upgrades)
        {
            var playerName = replay.Details.Players.Single(e => e.Slot == upgrade.PlayerId - 1);
            var translatedUpgrade = _translator.Translate(upgrade.UpgradeTypeName, translatorType);

            if (IsValidSpanValuePlayer(purchasesSet, translatedUpgrade, upgrade.Gameloop))
            {
                purchasesSet.Add(new SpanValuePlayer(upgrade.Gameloop, translatedUpgrade, playerName.Name));
            }
        }

        bool IsValidSpanValuePlayer(HashSet<SpanValuePlayer> purchasesSet, string translatedUpgrade, long gameLoop)
            => !purchasesSet.Any(set => translatedUpgrade == set.Value && Math.Abs(gameLoop - set.GameLoop) < 10);
    }

    private void AddKillsSpecific(HashSet<NameValue> killsDict, int playerId, OnizTranslatorType translatorType)
    {
        var translator = _translator.GetTranslator(translatorType);
        var validDiedEvents = replay.TrackerEvents.SUnitDiedEvents
            .Where(unitDiedEvent => unitDiedEvent.KillerUnitBornEvent is { } killerEvent
                && OnizUtils.IsCorrectSlot(killerEvent.ControlPlayerId, playerId)
                && translator.ContainsKey(killerEvent.UnitTypeName));

        foreach (var unitDiedEvent in validDiedEvents)
        {
            var translatedKiller = _translator.Translate(unitDiedEvent.KillerUnitBornEvent!.UnitTypeName, translatorType);

            var killEntry = killsDict.FirstOrDefault(entry => entry.Name == translatedKiller)
                            ?? new NameValue(translatedKiller, 0);
            killEntry.Value += 1;

            if (killEntry.Value is 1)
            {
                killsDict.Add(killEntry);
            }
        }
    }

    private int GetKills(int playerId)
    {
        var unitDiedEvents = replay.TrackerEvents.SUnitDiedEvents;

        var killCount = unitDiedEvents
                .Count(e => e.KillerUnitBornEvent is { } killerEvent
                && OnizUtils.IsCorrectSlot(killerEvent.ControlPlayerId, playerId));

        return killCount;
    }
}
