namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 最近项目项
/// </summary>
public class RecentProjectItem
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName => System.IO.Path.GetFileName(FilePath);
    public string Directory => System.IO.Path.GetDirectoryName(FilePath) ?? string.Empty;
}