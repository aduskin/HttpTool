using System.Windows;
using AduSkin.Controls;
using HttpTool.Desktop.ViewModels;
using HttpTool.Infrastructure.Services;

namespace HttpTool.Desktop.Views;

public partial class WebSocketWindow : AduWindow
{
    private readonly WebSocketWindowViewModel _viewModel;

    public WebSocketWindow()
    {
        InitializeComponent();
        var webSocketService = new WebSocketService();
        _viewModel = new WebSocketWindowViewModel(webSocketService);
        DataContext = _viewModel;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _viewModel.Cleanup();
    }
}
