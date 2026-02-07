namespace OhNoItsZombiesAnalyzer.Models;

public class ReplayFolderDto
{
    public string FolderName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public List<ReplayFileDto> Replays { get; set; } = new();
    public List<ReplayFolderDto> SubFolders { get; set; } = new();
}

public class ReplayFileDto
{
    public string FileName { get; init; } = default!;
    public string FullPath { get; init; } = default!;
}

public class ReplayFolderUi : ReplayFolderDto
{
    public bool IsOpen { get; set; }
    public bool IsVisible { get; set; } = true;
    public new List<ReplayFolderUi> SubFolders { get; set; } = new();
    public new List<ReplayFileUi> Replays { get; set; } = new();
}

public class ReplayFileUi : ReplayFileDto
{
    public bool IsVisible { get; set; } = true;
    public bool IsSelected { get; set; }
}