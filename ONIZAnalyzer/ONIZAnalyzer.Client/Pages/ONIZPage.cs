using Microsoft.AspNetCore.Components;
using ONIZAnalyzer.Common;

namespace ONIZAnalyzer.Client.Pages;

public abstract partial class ONIZPage : ComponentBase
{
    public IReadOnlyList<CustomFolder> Folders { get; set; } = [];

    protected int TotalItemsCount => Folders.Count + Folders.Sum(x => x.TotalCount);
    protected string? ErrorMessage { get; set; }
    protected bool Loading { get; set; } = true;
    protected string SearchTerm { get; set; } = string.Empty;

    protected void SetFoldersFromDto(IReadOnlyList<CustomFolderDto> dtos)
    {
        Folders = FoldersFromDto(dtos);
    }

    private IReadOnlyList<CustomFolder> FoldersFromDto(IReadOnlyList<CustomFolderDto> dtos)
    {
        var result = new List<CustomFolder>();

        foreach (var dto in dtos)
        {
            var uiFolder = new CustomFolder
            {
                FolderName = dto.FolderName,
                FullPath = dto.FullPath,
                IsOpen = false
            };

            if (dto.Items is { })
            {
                uiFolder.Items = dto.Items.Select(r => new FileItem
                {
                    Name = r.Name,
                    Path = r.Path,
                    IsVisible = true
                }).ToList();
            }

            if (dto.SubFolders is { Count: > 0 })
            {
                uiFolder.SubFolders = FoldersFromDto(dto.SubFolders);
            }

            result.Add(uiFolder);
        }

        return result;
    }


    protected void AutoExpandFoldersWithContent()
    {
        foreach (var folder in Folders)
        {
            SetAutoExpand(folder);
        }
    }

    protected void SetAutoExpand(CustomFolder folder)
    {
        var hasItems = folder.Items?.Any() == true;
        var hasSubfolders = folder.SubFolders?.Any() == true;

        folder.IsOpen = hasItems || hasSubfolders;

        if (folder.SubFolders is { })
        {
            foreach (var subFolder in folder.SubFolders)
            {
                SetAutoExpand(subFolder);
            }
        }
    }

    protected void DeselectAllItems(IReadOnlyList<CustomFolder> folderList)
    {
        foreach (var folder in folderList)
        {
            foreach (var replay in folder.Items)
            {
                replay.IsSelected = false;
            }

            if (folder.SubFolders != null)
            {
                DeselectAllItems(folder.SubFolders);
            }
        }
    }

    protected void ResetState()
    {
        Loading = true;
        ErrorMessage = null;
        StateHasChanged();
    }

    protected void UpdateVisibility()
    {
        if (string.IsNullOrEmpty(SearchTerm))
        {
            ResetVisibility(Folders);
        }
        else
        {
            ApplySearchFilter(Folders, SearchTerm.ToLower());
        }

        UpdateFolderList();
        StateHasChanged();
    }

    protected void ClearSearch()
    {
        SearchTerm = string.Empty;
        UpdateVisibility();
        StateHasChanged();
    }

    private void ResetVisibility(IReadOnlyList<CustomFolder> folderList)
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

    private bool ApplySearchFilter(IReadOnlyList<CustomFolder> folderList, string searchTerm)
    {
        bool anyVisible = false;

        foreach (var folder in folderList)
        {
            var hasVisibleReplay = false;

            foreach (var replay in folder.Items)
            {
                replay.IsVisible = replay.Name.ToLower().Contains(searchTerm);

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
        Folders = Folders.Where(f => f.IsVisible).ToList();
    }
}
