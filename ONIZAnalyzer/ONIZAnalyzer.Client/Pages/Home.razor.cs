using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ONIZAnalyzer.Common;
using ONIZAnalyzer.Common.WebSocket;
using System.Net.Http.Json;
using System.Web;

namespace ONIZAnalyzer.Client.Pages;

public partial class Home
{
    public List<ReplayFolderUi> Folders { get; set; } = [];
    public List<ReplayFolderUi> AllFolders { get; set; } = [];

    public string? SelectedReplayPath;
    public string SearchTerm { get; set; } = string.Empty;
    public string AnalysisResult { get; set; } = string.Empty;
    public bool Loading { get; set; } = true;
    public bool Analyzing { get; set; } = false;
    public int MassAnalyzedReplays { get; set; } = 0;
    public int TotalReplayCount => Folders.Count + Folders.Sum(x => x.TotalCount);

    public TimeSpan AverageTimeLeftForMassAnalyze = default;
    public string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (RendererInfo.Name is "Static")
        {
            return;
        }
        
        await LoadReplays();
    }
    
    private async Task LoadReplays()
    {
        try
        {
            Loading = true;
            ErrorMessage = null;
            StateHasChanged();

            var response = await Client.GetAsync("/api/replay/list");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error! status: {response.StatusCode}");
            }

            var folderDtos = await response.Content.ReadFromJsonAsync<List<ReplayFolderDto>>();

            if (folderDtos != null)
            {
                AllFolders = ConvertToUiModels(folderDtos);
                Folders = AllFolders.ToList();
                AutoExpandFoldersWithContent();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading replays: {ex.Message}";
            Console.Error.WriteLine($"Error loading replays: {ex}");
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }
    }

    private List<ReplayFolderUi> ConvertToUiModels(List<ReplayFolderDto> dtos)
    {
        var result = new List<ReplayFolderUi>();

        foreach (var dto in dtos)
        {
            var uiFolder = new ReplayFolderUi
            {
                FolderName = dto.FolderName,
                FullPath = dto.FullPath,
                IsOpen = false
            };

            if (dto.Replays is { })
            {
                uiFolder.Replays = dto.Replays.Select(r => new ReplayFileUi
                {
                    FileName = r.FileName,
                    FullPath = r.FullPath,
                    IsVisible = true
                }).ToList();
            }

            if (dto.SubFolders is { Count: > 0 })
            {
                uiFolder.SubFolders = ConvertToUiModels(dto.SubFolders);
            }

            result.Add(uiFolder);
        }

        return result;
    }

    private void AutoExpandFoldersWithContent()
    {
        foreach (var folder in AllFolders)
        {
            SetAutoExpand(folder);
        }
    }

    private void SetAutoExpand(ReplayFolderUi folder)
    {
        var hasReplays = folder.Replays?.Any() == true;
        var hasSubfolders = folder.SubFolders?.Any() == true;

        folder.IsOpen = hasReplays || hasSubfolders;

        if (folder.SubFolders is { })
        {
            foreach (var subFolder in folder.SubFolders)
            {
                SetAutoExpand(subFolder);
            }
        }
    }

    private void ToggleFolder(ReplayFolderUi folder)
    {
        folder.IsOpen = !folder.IsOpen;
        StateHasChanged();
    }

    private void SelectReplay(string fullPath)
    {
        DeselectAllReplays(AllFolders);

        var replay = FindReplay(AllFolders, fullPath);

        if (replay is { })
        {
            replay.IsSelected = true;
            SelectedReplayPath = fullPath;
            AnalysisResult = string.Empty;
        }

        StateHasChanged();
    }

    private void DeselectAllReplays(List<ReplayFolderUi> folderList)
    {
        foreach (var folder in folderList)
        {
            foreach (var replay in folder.Replays)
            {
                replay.IsSelected = false;
            }

            if (folder.SubFolders != null)
            {
                DeselectAllReplays(folder.SubFolders);
            }
        }
    }

    private ReplayFileUi? FindReplay(List<ReplayFolderUi> folderList, string fullPath)
    {
        foreach (var folder in folderList)
        {
            var replay = folder.Replays.FirstOrDefault(r => r.FullPath == fullPath);

            if (replay is { })
            {
                return replay;
            }

            if (folder.SubFolders is { })
            {
                var subReplay = FindReplay(folder.SubFolders, fullPath);

                if (subReplay is { })
                {
                    return subReplay;
                }
            }
        }

        return null;
    }

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

    private void ResetVisibility(List<ReplayFolderUi> folderList)
    {
        foreach (var folder in folderList)
        {
            folder.IsVisible = true;

            foreach (var replay in folder.Replays)
            {
                replay.IsVisible = true;
            }

            if (folder.SubFolders != null)
            {
                ResetVisibility(folder.SubFolders);
            }
        }
    }

    private bool ApplySearchFilter(List<ReplayFolderUi> folderList, string searchTerm)
    {
        bool anyVisible = false;

        foreach (var folder in folderList)
        {
            var hasVisibleReplay = false;

            foreach (var replay in folder.Replays)
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

    private async Task AnalyzeReplay()
    {
        if (string.IsNullOrEmpty(SelectedReplayPath))
        {
            AnalysisResult = "Please select a replay first.";
            StateHasChanged();
            return;
        }

        try
        {
            Analyzing = true;
            AnalysisResult = "Analyzing...";
            StateHasChanged();

            var encodedPath = HttpUtility.UrlEncode(SelectedReplayPath);
            var response = await Client.GetAsync($"/api/replay/analyze?path={encodedPath}");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error! status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            AnalysisResult = content;
        }
        catch (Exception ex)
        {
            AnalysisResult = $"Error analyzing replay: {ex.Message}";
            Console.Error.WriteLine($"Error analyzing replay: {ex}");
        }
        finally
        {
            Analyzing = false;
            StateHasChanged();
        }
    }

    private async Task MassAnalyzeTwoWayConnection()
    {
        Analyzing = true;
        AnalysisResult = "Mass analyzing all replays...";

        StateHasChanged();

        var hubConnectionBuilder = new HubConnectionBuilder()
            .WithUrl(Navigation.ToAbsoluteUri(OnizMassAnalysisConstants.MassReplayAnalysisEndPoint));
        await using var hubConnection = hubConnectionBuilder.Build();

        hubConnection.On<int, TimeSpan>(OnizMassAnalysisConstants.ResponseMassReplayAnalysisMethodName, (value, avgTimeLeft) =>
        {
            MassAnalyzedReplays = value;
            AverageTimeLeftForMassAnalyze = avgTimeLeft;

            StateHasChanged();
        });

        try
        {
            await hubConnection.StartAsync();
            await hubConnection.InvokeAsync(OnizMassAnalysisConstants.RequestMassReplayAnalysisMethodName);

            AnalysisResult = "Finished mass analyze.";
        }
        catch(Exception ex)
        {
            AnalysisResult = $"Error analyzing replay: {ex.Message}";
            Console.Error.WriteLine($"Error analyzing replay: {ex}");
        }
        finally
        {
            Analyzing = false;
            StateHasChanged();
        }
    }
}