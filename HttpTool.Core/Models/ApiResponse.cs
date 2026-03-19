using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// API 响应结果
/// </summary>
public partial class ApiResponse : ObservableObject
{
    [ObservableProperty]
    private int _statusCode;

    [ObservableProperty]
    private string _statusDescription = string.Empty;

    [ObservableProperty]
    private Dictionary<string, string> _headers = new();

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private long _contentLength;

    [ObservableProperty]
    private TimeSpan _elapsed;

    [ObservableProperty]
    private string _contentType = string.Empty;

    [ObservableProperty]
    private bool _isSuccess;

    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// 响应耗时（毫秒）
    /// </summary>
    public double ElapsedMilliseconds => Elapsed.TotalMilliseconds;

    /// <summary>
    /// 响应大小格式化
    /// </summary>
    public string ContentLengthFormatted => FormatBytes(ContentLength);

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
