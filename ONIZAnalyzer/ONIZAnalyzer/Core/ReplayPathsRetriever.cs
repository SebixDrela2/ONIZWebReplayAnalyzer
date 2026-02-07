using OhNoItsZombiesAnalyzer.Models;

namespace OhNoItsZombiesAnalyzer.Core;

public class ReplayPathsRetriever
{
    private const string StarcraftII = "Starcraft II";
    private const string Accounts = "Accounts";
    private const string HandleIdentifier = "S2-";
    private const string SubTerreneanPrefix = "Oh No It's Zombies Subterranean";
    private const string ArcticPrefix = "Oh No It's Zombies Arctic";
    private const string ReplaysPath = @"Replays\Multiplayer";
    private const string OnizHandleFolder = "ONIZHandles";

    public readonly string HandleDataFolderPath = Path.Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.LocalApplicationData), OnizHandleFolder);

    public readonly string AccountsPath = Path.Combine(Environment
        .GetFolderPath(Environment.SpecialFolder.MyDocuments), StarcraftII, Accounts);

    internal IReadOnlyList<ReplayFolderDto> GetOnizReplayFolders()
    {
        var handleFolders = Directory.GetDirectories(AccountsPath)
            .SelectMany(Directory.GetDirectories)
            .Where(dir => dir.Contains(HandleIdentifier));

        var result = new List<ReplayFolderDto>();

        foreach (var handle in handleFolders)
        {
            var replayRoot = Path.Combine(handle, ReplaysPath);
            if (!Directory.Exists(replayRoot))
                continue;

            result.Add(BuildFolderTree(replayRoot));
        }

        return result;
    }

    private ReplayFolderDto BuildFolderTree(string folderPath)
    {
        var folderDto = new ReplayFolderDto
        {
            FolderName = Path.GetFileName(folderPath),
            FullPath = folderPath,
            Replays = [.. Directory.GetFiles(folderPath)
                .Where(IsOnizReplay)
                .Select(path => new FileInfo(path))
                .OrderByDescending(fi => fi.LastWriteTime)
                .Select(fi => new ReplayFileDto
                {
                    FileName = fi.Name,
                    FullPath = fi.FullName
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
        return fileName.StartsWith(ArcticPrefix) || fileName.StartsWith(SubTerreneanPrefix);
    }
}
