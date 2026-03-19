using System.Windows;
using System.Windows.Controls;
using HttpTool.Core.Enums;
using HttpTool.Core.Models;
using HttpTool.Desktop.ViewModels;
using Microsoft.Win32;

namespace HttpTool.Desktop.Views;

/// <summary>
/// RequestEditorView.xaml 的交互逻辑
/// </summary>
public partial class RequestEditorView : UserControl
{
    public RequestEditorView()
    {
        InitializeComponent();
    }

    private void BrowseBinaryFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "All Files (*.*)|*.*",
            Title = "Select File to Upload"
        };

        if (dialog.ShowDialog() == true && DataContext is ProjectTabItem tab && tab.SelectedApi != null)
        {
            tab.SelectedApi.BinaryFilePath = dialog.FileName;
        }
    }
}

/// <summary>
/// HTTP 方法集合（用于 XAML 绑定）
/// </summary>
public static class HttpMethods
{
    public static HttpMethodType[] Values => Enum.GetValues<HttpMethodType>();
}
