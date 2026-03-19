using System.Net;
using AduSkin.Controls;

namespace HttpTool.Desktop.Views;

/// <summary>
/// CookieManagerWindow.xaml 的交互逻辑
/// </summary>
public partial class CookieManagerWindow : AduWindow
{
    public List<CookieData> Cookies { get; } = new();

    public CookieManagerWindow()
    {
        InitializeComponent();
        DataContext = this;
        LoadCookies();
    }

    public CookieManagerWindow(IEnumerable<Cookie> cookies) : this()
    {
        Cookies.Clear();
        foreach (var cookie in cookies)
        {
            Cookies.Add(new CookieData
            {
                Domain = cookie.Domain,
                Name = cookie.Name,
                Value = cookie.Value,
                Path = cookie.Path,
                Expires = cookie.Expires
            });
        }
        CookieGrid.ItemsSource = Cookies;
    }

    private void LoadCookies()
    {
        // 默认加载为空，实际 Cookie 从 HttpClientHandler 获取
        CookieGrid.ItemsSource = Cookies;
    }

    private void ClearAll_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Cookies.Clear();
        CookieGrid.Items.Refresh();
    }

    private void Close_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Close();
    }
}

public class CookieData
{
    public string Domain { get; set; } = "";
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Path { get; set; } = "/";
    public DateTime Expires { get; set; }
}
