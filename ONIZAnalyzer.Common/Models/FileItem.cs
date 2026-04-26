namespace ONIZAnalyzer.Common.Models;

public class FileItem : FileItemDto
{
    public bool IsVisible { get; set; } = true;
    public bool IsSelected { get; set; }
}
