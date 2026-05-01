using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Common;
using ONIZAnalyzer.Common.Models;
using ONIZAnalyzer.Common.Models.Record;
using ONIZAnalyzer.Core.Helpers.Record;
using ONIZAnalyzer.Core.Serializer.CareerData;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace ONIZAnalyzer.Services;

public partial class RecordsService
{
    private const string ArcadeWebSiteProfile = "https://sc2arcade.com/api/profiles";
    private const string ArcadeWebSitePotraits = "https://sc2arcade.com/media/portraits";

    private const string OnizHandleFolder = "ONIZHandles";
    private const string HandlesData = "HandlesData.json";
    private const string HandlesMap = "HandlesMap.json";
    private const string Avatar = "avatar";

    private const string HandleData = nameof(HandleData);

    private static readonly string HandleDataFolderPath = Path.Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.LocalApplicationData), OnizHandleFolder);

    private static readonly string CareerDataFolderPath = Path.Combine(HandleDataFolderPath, HandlesData);
    private static readonly string HandleMapFolderPath = Path.Combine(HandleDataFolderPath, HandlesMap);

    private static readonly OnizRecordProvider _recordProvider = new();

    [GeneratedRegex(@"(\d+)-S2-(\d+)-(\d+)")]
    private static partial Regex HandleRegex();

    public CustomFolderDto GetHandleDataFolder()
    {
        var serializedNameHandleData = GetSerializedNameHandleData();

        return GetFolderDto(serializedNameHandleData);
    }

    public CustomFolderDto GetHandleDataFolderSorted(OnizRecordSortOption sortOption)
    {
        if (sortOption.OptionType is OnizRecordOptionType.None)
        {
            return GetHandleDataFolder();
        }

        var comparer = _recordProvider.GetCareerComparer(sortOption);
        var careerDatas = GetOnizHandleCareerDatas()
            .Where(careerData =>
            {
                if (sortOption.OptionType is OnizRecordOptionType.Marine)
                {
                    return careerData.Data.MarineCareerData.GamesPlayed > 50;
                }

                return careerData.Data.ZombieCareerData.GamesPlayed > 50;
            })
            .OrderByDescending(x => x.Data, comparer);

        var nameHandles = careerDatas.Select(careerData => careerData.Tuple);

        return GetFolderDto(nameHandles);
    }

    public async Task<string> GetProfileImageAsync(string handle)
    {
        var handlePath = GetHandlePath(handle);
        var profileLink = $"{ArcadeWebSiteProfile}/{handlePath}";

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(profileLink);
        var content = await response.Content.ReadAsStringAsync();

        var json = JsonNode.Parse(content)!;
        var avatarUrl = json[Avatar];
        var link = $"{ArcadeWebSitePotraits}/{avatarUrl}.png";

        return link;
    }

    public string GetRecordTextData(string handle)
    {
        var careerData = GetOnizHandleCareerData(handle);
        var recordHandler = new OnizRecordHandler(careerData);

        return recordHandler.GetRecordText();
    }

    public OnizRecordSortOption[] GetSortOptions() => _recordProvider.ProvideSortOptions();

    private CustomFolderDto GetFolderDto(IEnumerable<NameHandle> nameHandles)
    {
        return new CustomFolderDto
        {
            FolderName = HandleData,
            SubFolders = [],
            FullPath = HandleData,
            Items = [.. nameHandles.Select(handleData => new FileItemDto
            {
                Name = $"{handleData.Handle} {handleData.Name}",
                Path = handleData.Handle
            })]
        };
    }

    private IReadOnlyList<NameHandle> GetSerializedNameHandleData()
    {
        var handleMapJson = File.ReadAllText(HandleMapFolderPath);
        var nameHandles = JsonSerializer
            .Deserialize<IReadOnlyList<NameHandle>>(handleMapJson, new JsonSerializerOptions { IncludeFields = true })!
            .ToList();

        return nameHandles;
    }

    private OnizHandleCareerData[] GetOnizHandleCareerDatas()
    {
        var (nameHandles, careerDatas) = GetNameHandlesWithCareerDatas();

        var onizHandleCareerData = nameHandles.Select(nameHandle =>
        {
            var careerData = careerDatas.Single(careerData => careerData.Key == nameHandle.Handle);

            return new OnizHandleCareerData(nameHandle, careerData.Value);

        }).ToArray();

        return onizHandleCareerData;
    }

    private OnizHandleCareerData GetOnizHandleCareerData(string handle)
    {
        var (nameHandles, careerDatas) = GetNameHandlesWithCareerDatas();

        var playerCareerData = careerDatas.Single(careerData => careerData.Key == handle).Value;
        var nameHandle = nameHandles.Single(nameHandle => nameHandle.Handle == handle);

        return new OnizHandleCareerData(nameHandle, playerCareerData);
    }

    private (IReadOnlyList<NameHandle> HandleMap, Dictionary<string, OnizPlayerCareerData> CareerData) GetNameHandlesWithCareerDatas()
    {
        var careerDataJson = File.ReadAllText(CareerDataFolderPath);
        var handleMapJson = File.ReadAllText(HandleMapFolderPath);

        var onizCareerData = JsonSerializer.Deserialize<Dictionary<string, OnizPlayerCareerData>>(careerDataJson)!;
        var handleMapData = JsonSerializer.Deserialize<IReadOnlyList<NameHandle>>(handleMapJson, new JsonSerializerOptions { IncludeFields = true })!;

        return (handleMapData, onizCareerData);
    }

    private string GetHandlePath(string handle)
    {
        var regex = HandleRegex();

        return regex.Replace(handle, @"$1/$2/$3");
    }
}
