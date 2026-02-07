namespace ONIZAnalyzer.Common;

public class ReplayFolderUi : ReplayFolderDto
{
    public bool IsOpen { get; set; }
    public bool IsVisible { get; set; } = true;
    public new List<ReplayFolderUi> SubFolders { get; set; } = new();
    public new List<ReplayFileUi> Replays { get; set; } = new();

    public int TotalCount => Replays.Count + SubFolders.Sum(x => x.TotalCount);
}
