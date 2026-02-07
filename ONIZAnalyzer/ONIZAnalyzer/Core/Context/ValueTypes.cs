using OhNoItsZombiesAnalyzer.Core.Helpers;

namespace OhNoItsZombiesAnalyzer.Core.Context;

public record ValueTypes(long GameLoop, string Value)
{
    private TimeSpan Span { get; } = TimeSpan.FromMilliseconds(OnizUtils.GameLoopToMilliseconds(GameLoop));

    public string MinutesSeconds => $"{Span.Minutes:D2}:{Span.Seconds:D2}";
}

public record SpanValuePlayer(long GameLoop, string Value, string PlayerName) : ValueTypes(GameLoop, Value);

public sealed class NameValue : IEquatable<NameValue>
{
    public string Name { get; init; }
    public int Value { get; set; }

    public NameValue(string name, int value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
    }

    public bool Equals(NameValue? other) => other is not null && string.Equals(Name, other.Name, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is NameValue other && Equals(other);

    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Name);
}