using CommunityToolkit.Mvvm.ComponentModel;
using HttpTool.Core.Enums;

namespace HttpTool.Core.Models;

/// <summary>
/// 请求历史记录
/// </summary>
public partial class RequestHistory : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _projectId = string.Empty;

    [ObservableProperty]
    private string _apiName = string.Empty;

    [ObservableProperty]
    private HttpMethodType _method;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private DateTime _executedAt = DateTime.Now;

    [ObservableProperty]
    private int _statusCode;

    [ObservableProperty]
    private TimeSpan _elapsed;

    [ObservableProperty]
    private bool _isSuccess;
}
