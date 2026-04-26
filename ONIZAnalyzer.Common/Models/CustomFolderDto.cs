namespace ONIZAnalyzer.Common.Models;

public class CustomFolderDto
{
    public string FolderName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public List<FileItemDto> Items { get; set; } = [];
    public List<CustomFolderDto> SubFolders { get; set; } = [];
}
