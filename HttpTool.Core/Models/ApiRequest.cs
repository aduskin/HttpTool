using CommunityToolkit.Mvvm.ComponentModel;
using HttpTool.Core.Enums;

namespace HttpTool.Core.Models;

/// <summary>
/// API 请求配置
/// </summary>
public partial class ApiRequest : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = "New Request";

    [ObservableProperty]
    private HttpMethodType _method = HttpMethodType.GET;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private List<KeyValueItem> _headers = new();

    [ObservableProperty]
    private List<KeyValueItem> _queryParams = new();

    [ObservableProperty]
    private BodyType _bodyType = BodyType.None;

    [ObservableProperty]
    private string _body = string.Empty;

    [ObservableProperty]
    private AuthType _authType = AuthType.None;

    [ObservableProperty]
    private string _authValue = string.Empty;

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.Now;

    /// <summary>
    /// 请求超时时间（秒）
    /// </summary>
    [ObservableProperty]
    private int _timeoutSeconds = 30;

    /// <summary>
    /// OAuth 2.0 配置
    /// </summary>
    [ObservableProperty]
    private OAuthConfig _oAuthConfig = new();

    /// <summary>
    /// 二进制文件路径（用于文件上传）
    /// </summary>
    [ObservableProperty]
    private string _binaryFilePath = string.Empty;

    /// <summary>
    /// 创建默认请求
    /// </summary>
    public static ApiRequest CreateDefault()
    {
        return new ApiRequest
        {
            Headers = new List<KeyValueItem>
            {
                new("Content-Type", "application/json")
            }
        };
    }
}
