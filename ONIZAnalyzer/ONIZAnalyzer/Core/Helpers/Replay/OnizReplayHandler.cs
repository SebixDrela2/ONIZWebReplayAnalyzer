using OhNoItsZombiesAnalyzer.Core;
using OhNoItsZombiesAnalyzer.Core.Context;
using OhNoItsZombiesAnalyzer.Core.Contexts;
using OhNoItsZombiesAnalyzer.Core.Enums;
using Sc2ReplayAnalyzer.Decoder.APIModel;
using System.Text;

namespace ONIZAnalyzer.Core.Helpers.Replay;

public class OnizReplayHandler(Sc2Replay replay)
{
    private readonly OnizTranslator _translator = new(replay.Details);
    private readonly OnizContextAnalyzer _analyzer = new(replay);

    public OnizReplayContext GetFullContext()
    {
        replay.Details.Players.RemoveAt(replay.Details.Players.Count - 1);

        return _analyzer.GetReplayContext();
    }

    public string GetAnalyzeText()
    {
        var builder = new StringBuilder();
        var context = GetFullContext();

        PrepareDetails(builder, context);
        PrepareCondition(builder, context);
        PreparePurchases(builder, context);
        PrepareMessages(builder, context);
        PrepareEndGame(builder, context);
        PrepareMarineKills(builder, context);
        PrepareAlphaRatio(builder, context);
        PrepareStrainRatio(builder, context);

        return builder.ToString();
    }

    private void PrepareCondition(StringBuilder builder, OnizReplayContext context)
    {
        var condition = context.MatchResult;
        var result = condition is OnizMatchResult.ZombieWin ? "Zombie Win" : "Marine Win";

        builder.AppendHeaderLine($"Game Result: {result}");
    }

    private void PrepareDetails(StringBuilder builder, OnizReplayContext context)
    {
        builder.AppendHeaderLine($"Players");

        var players = replay.Details.Players;

        foreach (var detail in players)
        {
            var onizRace = _translator.GetOnizRace(detail.Race);
            builder.AppendLine($"{detail.Slot + 1}. {detail.Toon.GetHandle()} [{onizRace}] Name: {detail.Name}");
        }

        builder.AppendLine();
    }

    private void PreparePurchases(StringBuilder builder, OnizReplayContext context)
    {
        var orderedUpgrades = context.MarineContext
            .SelectMany(e => e.Upgrades).Concat(context.ZombieContext.Upgrades)
            .OrderBy(e => e.MinutesSeconds);

        foreach(var purchase in orderedUpgrades)
        {
            builder.AppendLine(GetBuildText(purchase));
        }

        string GetBuildText(SpanValuePlayer spanValuePlayer)
        {
            return $"[{spanValuePlayer.MinutesSeconds}] {spanValuePlayer.PlayerName} Bought {spanValuePlayer.Value}";
        }

        builder.AppendLine();
    }

    private void PrepareMessages(StringBuilder builder, OnizReplayContext context)
    {
        var messages = replay.ChatMessages;

        if (messages.Count is 0)
        {
            return;
        }

        builder.AppendHeaderLine("Messages");

        foreach(var message in messages)
        {
            var time = _translator.GetTimeFromGameLoop(message.Loop);
            var playerName = _translator.GetPlayerName(message.Id);

            var content = $"[{time.Minutes:D2}:{time.Seconds:D2}] [{message.Recipient}] {playerName}: {message.Msg}";

            builder.AppendLine(content);
        }

        builder.AppendLine();
    }
    
    private void PrepareEndGame(StringBuilder builder, OnizReplayContext context)
    {
        builder.AppendHeaderLine("Game ended.");
    }

    private void PrepareMarineKills(StringBuilder builder, OnizReplayContext context)
    {
        var marineContext = context.MarineContext;

        if (marineContext.Count is 0)
        {
            return;
        }

        builder.AppendHeaderLine($"Marine Kills");

        foreach(var playerContext in marineContext)
        {
            builder.AppendLine($"{playerContext.Name}: {playerContext.Kills}");
        }

        builder.AppendLine();
    }

    private void PrepareAlphaRatio(StringBuilder builder, OnizReplayContext context)
    {
        var alphas = context.ZombieContext.AlphaKills;

        if (alphas.Count is 0)
        {
            return;
        }

        builder.AppendHeaderLine("Alpha Kills");

        foreach (var alpha in alphas)
        {
            builder.AppendLine($"{alpha.Name}: {alpha.Value}");
        }

        builder.AppendLine();
    }

    private void PrepareStrainRatio(StringBuilder builder, OnizReplayContext context)
    {
        var strains = context.ZombieContext.StrainKills
            .OrderBy(x => x.Name)
            .ToList();

        if (strains.Count is 0)
        {
            return;
        }

        builder.AppendHeaderLine("Strain Kills");

        foreach(var strain in strains)
        {
            builder.AppendLine($"{strain.Name}: {strain.Value}");
        }

        builder.AppendLine();
    }
}
