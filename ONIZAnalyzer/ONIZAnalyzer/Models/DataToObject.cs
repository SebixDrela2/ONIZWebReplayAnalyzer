using ONIZAnalyzer.Common.Models.Record;
using ONIZAnalyzer.Core.Helpers.Record;
using ONIZAnalyzer.Core.Serializer.CareerData;

namespace OhNoItsZombiesAnalyzer.Models;

public class FolderDto
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public List<FileDto> Items { get; set; } = new();
    public List<FolderDto> SubFolders { get; set; } = new();
}

public class FileDto
{
    public string Name { get; init; } = default!;
    public string Path { get; init; } = default!;
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

public class OnizPlayerCareerDataComparer(Func<OnizPlayerCareerData, OnizPlayerCareerData, int> func) : IComparer<OnizPlayerCareerData>
{  
    public int Compare(OnizPlayerCareerData? x, OnizPlayerCareerData? y) => func(x, y);
}