using OhNoItsZombiesAnalyzer.Core.Contexts;
using ONIZAnalyzer.Core.Context;
using ONIZAnalyzer.Core.Serializer.CareerData;
using System.Text.Json;

namespace ONIZAnalyzer.Core.Serializer;

public class ReplaySerializer(string directoryPath)
{
    private const string HandlesData = nameof(HandlesData);
    private const string HandlesMap = nameof(HandlesMap);

    private readonly OnizPlayerCareerContextHandler _careerContextSerializer = new(directoryPath);
    private readonly JsonSerializerOptions _options = new() 
    {
        WriteIndented = true, 
        IncludeFields = true 
    };

    public void SerializeContexts(OnizReplayContext[] allReplayContexts)
    {
        var handleNames = allReplayContexts
            .SelectMany(replayContext => replayContext.NameHandles)
            .DistinctBy(item => item.Handle);

        var dict = new Dictionary<string, OnizPlayerCareerData>();

        foreach (var (_, handle) in handleNames)
        {
            dict.Add(handle, GetOnizPlayerCareerData(allReplayContexts, handle));
        }

        var serializedData = JsonSerializer.Serialize(dict, _options);
        var serializedHelperData = JsonSerializer.Serialize(handleNames, _options);

        File.WriteAllText(@$"{directoryPath}\{HandlesData}.json", serializedData);
        File.WriteAllText(@$"{directoryPath}\{HandlesMap}.json", serializedHelperData);
    }

    private OnizPlayerCareerData GetOnizPlayerCareerData(OnizReplayContext[] allReplayContexts, string handle)
    {
        var allGamesContext = allReplayContexts
            .Where(replayContext => replayContext.Handles.Contains(handle))
            .ToList();

        var handleZombieContext = allGamesContext
            .Where(replayContext => replayContext.ZombieContext.Handle == handle)
            .Select(replayContext => replayContext.ZombieContext)
            .ToList();

        var handleMarineContext = allGamesContext       
            .Where(marineContext => marineContext.MarineContext.Any(context => context.Handle == handle))
            .SelectMany(replayContext => replayContext.MarineContext)
            .ToList();

        var playerCareerContext = new PlayerCareerContext(allGamesContext, handleMarineContext, handleZombieContext);

        return _careerContextSerializer.GetOnizCareerData(playerCareerContext);
    }
}
