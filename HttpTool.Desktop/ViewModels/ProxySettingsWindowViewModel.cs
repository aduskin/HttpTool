using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 代理设置窗口 ViewModel
/// </summary>
public partial class ProxySettingsWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _proxyEnabled;

    [ObservableProperty]
    private string _proxyType = "HTTP";

    [ObservableProperty]
    private string _proxyHost = "";

    [ObservableProperty]
    private string _proxyPort = "";

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    public int ProxyPortValue => int.TryParse(ProxyPort, out var port) ? port : 0;

    public static string[] ProxyTypes => new[] { "HTTP", "HTTPS", "SOCKS5" };
}
