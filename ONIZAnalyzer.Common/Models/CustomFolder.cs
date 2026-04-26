namespace ONIZAnalyzer.Common.Models;

public class CustomFolder : CustomFolderDto
{
    public bool IsOpen { get; set; }
    public bool IsVisible { get; set; } = true;
    public new IReadOnlyList<CustomFolder> SubFolders { get; set; } = [];
    public new IReadOnlyList<FileItem> Items { get; set; } = [];

    public int TotalCount => Items.Count + SubFolders.Sum(x => x.TotalCount);
}
