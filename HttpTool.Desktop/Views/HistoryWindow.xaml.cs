using System.Windows;
using AduSkin.Controls;
using HttpTool.Core.Interfaces;
using HttpTool.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace HttpTool.Desktop.Views;

/// <summary>
/// HistoryWindow.xaml 的交互逻辑
/// </summary>
public partial class HistoryWindow : AduWindow
{
    public HistoryWindow()
    {
        InitializeComponent();
        DataContext = new HistoryWindowViewModel(App.Services.GetRequiredService<IHistoryService>());
    }
}
