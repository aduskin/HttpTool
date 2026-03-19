using AduSkin.Controls;

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
        EnableProxyCheckBox.IsChecked = isEnabled;
        ProxyTypeComboBox.SelectedIndex = proxyType.ToUpper() switch
        {
            "HTTPS" => 1,
            "SOCKS5" => 2,
            _ => 0
        };
        ProxyHostTextBox.Text = host;
        ProxyPortTextBox.Text = port > 0 ? port.ToString() : "";
        ProxyUsernameTextBox.Text = username;
        ProxyPasswordTextBox.Text = password;
    }

    private void Save_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        ProxyEnabled = EnableProxyCheckBox.IsChecked ?? false;
        ProxyType = (ProxyTypeComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "HTTP";
        ProxyHost = ProxyHostTextBox.Text;
        ProxyPort = int.TryParse(ProxyPortTextBox.Text, out var port) ? port : 0;
        Username = ProxyUsernameTextBox.Text;
        Password = ProxyPasswordTextBox.Text;

        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
