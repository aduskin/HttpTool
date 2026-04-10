using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// Cookie 数据模型
/// </summary>
public partial class CookieData : ObservableObject
{
    [ObservableProperty]
    private string _domain = "";

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _value = "";

    [ObservableProperty]
    private string _path = "/";

    [ObservableProperty]
    private DateTime _expires;
}

/// <summary>
/// Cookie 管理器窗口 ViewModel
/// </summary>
public partial class CookieManagerWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CookieData> _cookies = new();

    public CookieManagerWindowViewModel()
    {
    }

    public CookieManagerWindowViewModel(IEnumerable<Cookie> cookies)
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
    }

    [RelayCommand]
    private void ClearAll()
    {
        Cookies.Clear();
    }

    [RelayCommand]
    private void Close(Window? window)
    {
        if (window != null)
        {
            window.Close();
        }
    }
}
