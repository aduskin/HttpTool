using System.Net;
using AduSkin.Controls;
using HttpTool.Desktop.ViewModels;

namespace HttpTool.Desktop.Views;

/// <summary>
/// CookieManagerWindow.xaml 的交互逻辑
/// </summary>
public partial class CookieManagerWindow : AduWindow
{
    public CookieManagerWindow()
    {
        InitializeComponent();
    }

    public CookieManagerWindow(IEnumerable<Cookie> cookies) : this()
    {
        if (DataContext is CookieManagerWindowViewModel vm)
        {
            foreach (var cookie in cookies)
            {
                vm.Cookies.Add(new CookieData
                {
                    Domain = cookie.Domain,
                    Name = cookie.Name,
                    Value = cookie.Value,
                    Path = cookie.Path,
                    Expires = cookie.Expires
                });
            }
        }
    }
}
