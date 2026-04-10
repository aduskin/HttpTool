using System.Windows;
using AduSkin.Controls;
using HttpTool.Desktop.ViewModels;

namespace HttpTool.Desktop.Views;

/// <summary>
/// ProxySettingsWindow.xaml 的交互逻辑
/// </summary>
public partial class ProxySettingsWindow : AduWindow
{
    public bool ProxyEnabled { get; private set; }
    public string ProxyType { get; private set; } = "HTTP";
    public string ProxyHost { get; private set; } = "";
    public int ProxyPort { get; private set; }
    public string Username { get; private set; } = "";
    public string Password { get; private set; } = "";

    public ProxySettingsWindow()
    {
        InitializeComponent();
    }

    public ProxySettingsWindow(bool isEnabled, string proxyType, string host, int port, string username, string password) : this()
    {
        if (DataContext is ProxySettingsWindowViewModel vm)
        {
            vm.ProxyEnabled = isEnabled;
            vm.ProxyType = proxyType;
            vm.ProxyHost = host;
            vm.ProxyPort = port > 0 ? port.ToString() : "";
            vm.Username = username;
            vm.Password = password;
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProxySettingsWindowViewModel vm)
        {
            ProxyEnabled = vm.ProxyEnabled;
            ProxyType = vm.ProxyType;
            ProxyHost = vm.ProxyHost;
            ProxyPort = vm.ProxyPortValue;
            Username = vm.Username;
            Password = vm.Password;

            DialogResult = true;
            Close();
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
