using ONIZAnalyzer.Core.Serializer.CareerData;

namespace OhNoItsZombiesAnalyzer.Models;

public class FolderDto
{
    public string FolderName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public List<FileDto> Replays { get; set; } = new();
    public List<FolderDto> SubFolders { get; set; } = new();
}

public class FileDto
{
    public string FileName { get; init; } = default!;
    public string FullPath { get; init; } = default!;
}

public class FolderUi : FolderDto
{
    public bool IsOpen { get; set; }
    public bool IsVisible { get; set; } = true;
    public new List<FolderUi> SubFolders { get; set; } = new();
    public new List<FileUi> Replays { get; set; } = new();
}

public class FileUi : FileDto
{
    public bool IsVisible { get; set; } = true;
    public bool IsSelected { get; set; }
}

public record class NameHandle(string Name, string Handle);
public record class OnizHandleCareerData(NameHandle Tuple, OnizPlayerCareerData Data);