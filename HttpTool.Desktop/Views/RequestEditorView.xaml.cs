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

        if (dialog.ShowDialog() == true && DataContext is RequestEditorViewModel vm && vm.SelectedApi != null)
        {
            vm.SelectedApi.BinaryFilePath = dialog.FileName;
        }
    }

    private void AddParam_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is RequestEditorViewModel vm)
        {
            vm.AddParam();
        }
    }

    private void DeleteParam_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is RequestEditorViewModel vm)
        {
            vm.DeleteParam();
        }
    }

    private void AddHeader_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is RequestEditorViewModel vm)
        {
            vm.AddHeader();
        }
    }

    private void DeleteHeader_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is RequestEditorViewModel vm)
        {
            vm.DeleteHeader();
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
