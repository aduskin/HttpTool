using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// OAuth 2.0 配置
/// </summary>
public partial class OAuthConfig : ObservableObject
{
    [ObservableProperty]
    private string _tokenUrl = string.Empty;

    [ObservableProperty]
    private string _clientId = string.Empty;

    [ObservableProperty]
    private string _clientSecret = string.Empty;

    [ObservableProperty]
    private string _scope = string.Empty;

    [ObservableProperty]
    private string _authorizationUrl = string.Empty;

    [ObservableProperty]
    private string _accessToken = string.Empty;

    [ObservableProperty]
    private string _refreshToken = string.Empty;

    [ObservableProperty]
    private string _tokenType = "Bearer";

    [ObservableProperty]
    private DateTime _expiresAt = DateTime.Now;
}
