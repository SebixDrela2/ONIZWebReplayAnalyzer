using OhNoItsZombiesAnalyzer.Core.Enums;
using OhNoItsZombiesAnalyzer.Services;
using Sc2ReplayAnalyzer.Decoder.Models.Details;
using System.Reflection;

namespace ONIZAnalyzer.Core.Helpers.Replay;

public class OnizTranslator
{
    private readonly Assembly _assembly = typeof(ReplayService).Assembly;
    private readonly Dictionary<string, string> _onizTranslations = new Dictionary<string, string>()
    {
        {"Terran Mengsk", "Marine" },
        {"Terran", "Marine" },
        {"Zerg", "Zombie" },
    };

    private readonly Details _details;
    private Dictionary<string, string> MarineUpgradeTranslations { get; }
    private Dictionary<string, string> ZombieUpgradeTranslations { get; }
    private Dictionary<string, string> ZombieStrainsTranslations { get; }
    private Dictionary<string, string> ZombieAlphaTranslations { get; }

    private Dictionary<string, string> BankTranslations { get; }

    public OnizTranslator(Details details)
    {
        _details = details;

        MarineUpgradeTranslations = GetTranslations("Marine.Upgrades.txt");
        ZombieUpgradeTranslations = GetTranslations("Zombie.Upgrades.txt");
        ZombieStrainsTranslations = GetTranslations("Zombie.StrainUnits.txt");
        ZombieAlphaTranslations = GetTranslations("Zombie.AlphaUnits.txt");
        BankTranslations = GetTranslations("Bank.Entries.txt");
    }

    public string Translate(string nnetValue, OnizTranslatorType translatorType)
    {
        var translator = GetTranslator(translatorType);

        return translator[nnetValue];
    }

    public Dictionary<string, string> GetTranslator(OnizTranslatorType translatorType) => translatorType switch
    {
        OnizTranslatorType.MarineUpgrades => MarineUpgradeTranslations,
        OnizTranslatorType.ZombieUpgrades => ZombieUpgradeTranslations,
        OnizTranslatorType.ZombieStrains => ZombieStrainsTranslations,
        OnizTranslatorType.ZombieAlphas => ZombieAlphaTranslations,
        OnizTranslatorType.BankEntries => BankTranslations,
        _ => throw new InvalidOperationException($"Invalid translator: {translatorType}")
    };

    public string GetPlayerName(int playerId)
    {
        var players = _details.Players;

        return players.Single(x => x.Slot == playerId).Name;
    }

    public string GetOnizRace(string race)
    {
        _onizTranslations.TryGetValue(race, out var onizRace);

        return onizRace ?? "Zombie";
    }

    public TimeSpan GetTimeFromGameLoop(long gameLoop) => TimeSpan.FromMilliseconds(OnizUtils.GameLoopToMilliseconds(gameLoop));

    private Dictionary<string, string> GetTranslations(string relativePath)
    {
        var resource = GetResourceList(relativePath);
        var splitResource = SplitResource(resource);

        return splitResource;
    }

    private static Dictionary<string, string> SplitResource(List<string> resource)
    {
        var dict = new Dictionary<string, string>();

        foreach(var line in resource)
        {
            var split = line.Split("=");

            var key = split[0].Trim();
            var value = split[1].Trim();

            dict.Add(key, value);
        }

        return dict;
    }

    private List<string> GetResourceList(string relativePath)
    {
        var result = new List<string>();
        var resources = _assembly.GetManifestResourceNames();
        var resource = resources.First(r => r.Contains(relativePath, StringComparison.OrdinalIgnoreCase));

        using var stream = _assembly.GetManifestResourceStream(resource);
        using var reader = new StreamReader(stream!);
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            result.Add(line);
        }

        return result;
    }

}
