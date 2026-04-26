using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using ONIZAnalyzer.Common.Models;
using ONIZAnalyzer.Common.WebSocket;
using System.Net.Http.Json;
using System.Web;

namespace ONIZAnalyzer.Client.Pages;

public partial class Home
{
    public string AnalysisResult { get; set; } = string.Empty;
    public bool Analyzing { get; set; } = false;
    public int MassAnalyzedReplays { get; set; } = 0;

    protected string? SelectedReplayPath;

    public TimeSpan AverageTimeLeftForMassAnalyze = default;

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
            ResetState();

            var response = await Client.GetAsync("/api/replay/list");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error! status: {response.StatusCode}");
            }

            var folderDtos = await response.Content.ReadFromJsonAsync<List<CustomFolderDto>>();

            if (folderDtos != null)
            {
                SetFoldersFromDto(folderDtos);
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

    private void SelectReplay(string fullPath)
    {
        DeselectAllItems(Folders);

        var replay = FindReplay(Folders, fullPath);

        if (replay is { })
        {
            replay.IsSelected = true;
            SelectedReplayPath = fullPath;
            AnalysisResult = string.Empty;
        }

        StateHasChanged();
    }

    private FileItem? FindReplay(IReadOnlyList<CustomFolder> folderList, string fullPath)
    {
        foreach (var folder in folderList)
        {
            var replay = folder.Items.FirstOrDefault(r => r.Path == fullPath);

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