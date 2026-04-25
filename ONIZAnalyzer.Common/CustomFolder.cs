namespace ONIZAnalyzer.Common;

public class CustomFolder : CustomFolderDto
{
    public bool IsOpen { get; set; }
    public bool IsVisible { get; set; } = true;
    public new List<CustomFolder> SubFolders { get; set; } = new();
    public new List<FileItem> Items { get; set; } = new();

    public int TotalCount => Items.Count + SubFolders.Sum(x => x.TotalCount);
}
