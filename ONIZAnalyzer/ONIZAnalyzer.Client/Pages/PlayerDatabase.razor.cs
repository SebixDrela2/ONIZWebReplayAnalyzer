using ONIZAnalyzer.Common;
using System.Net.Http.Json;

namespace ONIZAnalyzer.Client.Pages;

public partial class PlayerDatabase
{
    protected override async Task OnInitializedAsync()
    {
        if (RendererInfo.Name is "Static")
        {
            return;
        }

        await LoadRecords();
    }

    private void SelectRecord()
    {

    }

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
