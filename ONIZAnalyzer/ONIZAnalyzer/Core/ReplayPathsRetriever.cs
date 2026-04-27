using OhNoItsZombiesAnalyzer.Models;

namespace OhNoItsZombiesAnalyzer.Core;

public class ReplayPathsRetriever
{
    private const bool UseCustomPath = true;
    private const string CustomReplayPath = @"C:\Users\Sebastian\replays";

    private const string StarcraftII = "Starcraft II";
    private const string Accounts = "Accounts";
    private const string HandleIdentifier = "S2-";
    private const string SubTerreneanPrefix = "Oh No It's Zombies Subterranean";
    private const string ArcticPrefix = "Oh No It's Zombies Arctic";
    private const string OnizMapPrefix = "Oh No It's Zombies";

    private const string ReplaysPath = @"Replays\Multiplayer";
    private const string OnizHandleFolder = "ONIZHandles";

    public readonly string HandleDataFolderPath = Path.Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.LocalApplicationData), OnizHandleFolder);

    public readonly string AccountsPath = Path.Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.MyDocuments), StarcraftII, Accounts);

    public string TrueReplaysPath => GetTrueReplaysPath();

    internal IReadOnlyList<FolderDto> GetOnizReplayFolders()
    {
        if (UseCustomPath)
        {
            return [BuildFolderTree(CustomReplayPath)];
        }

        var handleFolders = Directory.GetDirectories(AccountsPath)
            .SelectMany(Directory.GetDirectories)
            .Where(dir => dir.Contains(HandleIdentifier));

        var result = new List<FolderDto>();

        foreach (var handle in handleFolders)
        {
            var replayRoot = Path.Combine(handle, ReplaysPath);

            if (!Directory.Exists(replayRoot))
            {
                continue;
            }

            result.Add(BuildFolderTree(replayRoot));
        }

        return result;
    }

    private string GetTrueReplaysPath()
    {
        if (UseCustomPath)
        {
            return CustomReplayPath;
        }

        return AccountsPath;
    }

    private FolderDto BuildFolderTree(string folderPath)
    {
        var folderDto = new FolderDto
        {
            Name = Path.GetFileName(folderPath),
            Path = folderPath,
            Items = [.. Directory.GetFiles(folderPath)
                .Where(IsOnizReplay)
                .Select(path => new FileInfo(path))
                .OrderByDescending(fi => fi.LastWriteTime)
                .Select(fi => new FileDto
                {
                    Name = fi.Name,
                    Path = fi.FullName
                })]
        };

        foreach (var subDir in Directory.GetDirectories(folderPath))
        {
            folderDto.SubFolders.Add(BuildFolderTree(subDir));
        }

        return folderDto;
    }
    private static bool IsOnizReplay(string replayPath)
    {
        var fileName = Path.GetFileName(replayPath);

        return fileName.StartsWith(ArcticPrefix) 
            || fileName.StartsWith(SubTerreneanPrefix) 
            || fileName.StartsWith(OnizMapPrefix);
    }
}
