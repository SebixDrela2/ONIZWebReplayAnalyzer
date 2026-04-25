using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Common;
using ONIZAnalyzer.Core.Serializer.CareerData;
using System.Text.Json;

namespace ONIZAnalyzer.Services;

public class RecordsService
{
    private const string OnizHandleFolder = "ONIZHandles";
    private const string HandlesData = "HandlesData.json";
    private const string HandlesMap = "HandlesMap.json";

    private const string HandleData = nameof(HandleData);

    private static readonly string HandleDataFolderPath = Path.Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.LocalApplicationData), OnizHandleFolder);

    private static readonly string CareerDataFolderPath = Path.Combine(HandleDataFolderPath, HandlesData);
    private static readonly string HandleMapFolderPath = Path.Combine(HandleDataFolderPath, HandlesMap);

    public CustomFolderDto GetHandleDataFolder()
    {
        var serializedNameHandleData = GetSerializedNameHandleData();

        return new CustomFolderDto
        {
            FolderName = HandleData,
            SubFolders = [],
            FullPath = HandleData,
            Items = [.. serializedNameHandleData.Select(handleData => new FileItemDto 
            {
                FileName = $"{handleData.Handle} {handleData.Name}",
                FullPath = string.Empty
            })]
        };
    }

    private IReadOnlyList<NameHandle> GetSerializedNameHandleData()
    {
        var handleMapJson = File.ReadAllText(HandleMapFolderPath);
        var handleMapData = JsonSerializer.Deserialize<IReadOnlyList<(string Name, string Handle)>>(handleMapJson)!;

        return [..handleMapData.Select(handleMap => new NameHandle(handleMap.Name, handleMap.Handle))];
    }

    public OnizHandleCareerData? GetSerializedCareerData(string handle)
    {
        if (!Directory.Exists(HandleDataFolderPath))
        {
            return null;
        }

        var careerDataJson = File.ReadAllText(CareerDataFolderPath);
        var handleMapJson = File.ReadAllText(HandleMapFolderPath);

        var onizCareerData = JsonSerializer
            .Deserialize<Dictionary<string, OnizPlayerCareerData>>(careerDataJson)!
            .Single(careerData => careerData.Key == handle)
            .Value;

        var handleMapData = JsonSerializer.Deserialize<IReadOnlyList<(string Name, string Handle)>>(handleMapJson)!;
        var handleMap = handleMapData.Single(handleMap => handleMap.Handle == handle);

        return new OnizHandleCareerData(new NameHandle(handleMap.Name, handleMap.Handle), onizCareerData);
    }
}
