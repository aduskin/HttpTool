using System.Windows;
using AduSkin.Controls;
using HttpTool.Core.Interfaces;
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
        LoadHistory();
    }

    private async void LoadHistory()
    {
        var historyService = App.Services.GetRequiredService<IHistoryService>();
        var history = await historyService.GetHistoryAsync();
        HistoryGrid.ItemsSource = history;
    }

    private async void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to clear all history?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            var historyService = App.Services.GetRequiredService<IHistoryService>();
            await historyService.ClearHistoryAsync();
            HistoryGrid.ItemsSource = null;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
