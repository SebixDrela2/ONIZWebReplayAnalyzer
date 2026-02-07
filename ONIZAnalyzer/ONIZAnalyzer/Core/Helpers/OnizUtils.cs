using Sc2ReplayAnalyzer.Decoder.Events.GameEvents;
using Sc2ReplayAnalyzer.Decoder.Models.Details;
using System.Text;
using System.Text.RegularExpressions;

namespace OhNoItsZombiesAnalyzer.Core.Helpers;

public static class OnizUtils
{

    private static readonly Regex _digitRegex = new(@"\d+");

    public static IEnumerable<T> OfEventType<T>(this GameEvents e)
        where T : GameEvent => e.Gameevents.OfType<T>();

    public static double GetDoubleData(string data) => Math.Round(double.Parse(_digitRegex.Match(data).Value), 2);

    public static string GetHandle(this Toon toon) => $"{toon.Region}-S2-{toon.Realm}-{toon.Id}";

    public static TimeSpan GetTimeSpan(this int milliseconds) => TimeSpan.FromMilliseconds(milliseconds);

    public static bool IsCorrectSlot(int controlPlayerId, int slot) => controlPlayerId - 1 == slot;

    public static long GameLoopToMilliseconds(long gameLoops)
    {
        const double loopsPerSecond = 16.0;
        const double fasterMultiplier = 1.4;
        const double multiplier = loopsPerSecond * fasterMultiplier;
        const double milisecondMultiplier = 1000 / multiplier;

        double ms = gameLoops * milisecondMultiplier;

        return (long)Math.Round(ms);
    }

    public static void AppendHeaderLine(this StringBuilder builder, string content)
    {
        builder.AppendLine($"<span class=\"analysis-textbox-header-line\">{content}</span><br>");
    }
}
