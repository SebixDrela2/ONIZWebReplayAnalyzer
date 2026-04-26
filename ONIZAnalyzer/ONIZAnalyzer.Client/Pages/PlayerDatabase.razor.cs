using ONIZAnalyzer.Common;
using System.Net.Http.Json;

namespace ONIZAnalyzer.Client.Pages;

public partial class PlayerDatabase
{
    private string RecordResult { get; set; } = string.Empty;
    private string ProfileImageSrc { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (RendererInfo.Name is "Static")
        {
            return;
        }

        await LoadRecords();
    }

    private async Task LoadProfileImage(string handle)
    {
        using var httpClient = new HttpClient();
        var response = await Client.GetAsync($"api/records/profile-img/{handle}");
        var src = await response.Content.ReadAsStringAsync();

        ProfileImageSrc = src;
        StateHasChanged();
    }

    private async Task SelectRecord(string handle)
    {
        DeselectAllItems(Folders);
        SetSelectedRecord(handle);

        await LoadProfileImage(handle);
        await SetSelectedRecordData(handle);
    }

    private void SetSelectedRecord(string handle)
    {
        var record = FindRecord(handle);
        record.IsSelected = true;
    }

    private async Task SetSelectedRecordData(string handle)
    {
        var response = await Client.GetAsync($"/api/records/handle/{handle}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"HTTP error! status: {response.StatusCode}");
        }

        RecordResult = await response.Content.ReadAsStringAsync();
        StateHasChanged();
    }

    private FileItem FindRecord(string handle) => Folders
        .First()
        .Items
        .Single((folder => folder.Path == handle));

    private async Task LoadRecords()
    {
        try
        {
            ResetState();

            var response = await Client.GetAsync("/api/records/handle");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"HTTP error! status: {response.StatusCode}");
            }

            var folderDto = await response.Content.ReadFromJsonAsync<CustomFolderDto>();

            if (folderDto != null)
            {
                SetFoldersFromDto([folderDto]);
                AutoExpandFoldersWithContent();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading records: {ex.Message}";
            Console.Error.WriteLine($"Error loading records: {ex}");
        }
        finally
        {
            Loading = false;
            StateHasChanged();
        }
    }
}
