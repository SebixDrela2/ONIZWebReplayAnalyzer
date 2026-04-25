namespace ONIZAnalyzer.Common;

public class CustomFolderDto
{
    public string FolderName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public List<FileItemDto> Items { get; set; } = new();
    public List<CustomFolderDto> SubFolders { get; set; } = new();
}
