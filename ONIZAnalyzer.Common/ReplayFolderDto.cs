namespace ONIZAnalyzer.Common;

public class ReplayFolderDto
{
    public string FolderName { get; set; } = "";
    public string FullPath { get; set; } = "";
    public List<ReplayFileDto> Replays { get; set; } = new();
    public List<ReplayFolderDto> SubFolders { get; set; } = new();
}
