using ONIZAnalyzer.Common;

namespace ONIZAnalyzer.Client.Layout;

public partial class CommonLayout
{
    public static object SideBarSection { get; } = new object();
    public List<CustomFolder> Folders { get; set; } = [];
    public List<CustomFolder> AllFolders { get; set; } = [];

    public string SearchTerm { get; set; } = string.Empty;

    private void UpdateVisibility()
    {
        if (string.IsNullOrEmpty(SearchTerm))
        {
            ResetVisibility(AllFolders);
        }
        else
        {
            ApplySearchFilter(AllFolders, SearchTerm.ToLower());
        }

        UpdateFolderList();
    }

    private void ResetVisibility(List<CustomFolder> folderList)
    {
        foreach (var folder in folderList)
        {
            folder.IsVisible = true;

            foreach (var replay in folder.Items)
            {
                replay.IsVisible = true;
            }

            if (folder.SubFolders != null)
            {
                ResetVisibility(folder.SubFolders);
            }
        }
    }

    private bool ApplySearchFilter(List<CustomFolder> folderList, string searchTerm)
    {
        bool anyVisible = false;

        foreach (var folder in folderList)
        {
            var hasVisibleReplay = false;

            foreach (var replay in folder.Items)
            {
                replay.IsVisible = replay.FileName.ToLower().Contains(searchTerm);

                if (replay.IsVisible)
                {
                    hasVisibleReplay = true;
                    folder.IsOpen = true;
                }
            }

            var hasVisibleSubfolder = false;

            if (folder.SubFolders != null)
            {
                hasVisibleSubfolder = ApplySearchFilter(folder.SubFolders, searchTerm);
            }

            folder.IsVisible = hasVisibleReplay || hasVisibleSubfolder;
            anyVisible = anyVisible || folder.IsVisible;
        }

        return anyVisible;
    }

    private void UpdateFolderList()
    {
        Folders = AllFolders.Where(f => f.IsVisible).ToList();
    }

    private void ClearSearch()
    {
        SearchTerm = string.Empty;
        UpdateVisibility();
        StateHasChanged();
    }
}