using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 历史记录窗口 ViewModel
/// </summary>
public partial class HistoryWindowViewModel : ObservableObject
{
    private readonly IHistoryService _historyService;

    [ObservableProperty]
    private ObservableCollection<RequestHistory> _history = new();

    public HistoryWindowViewModel(IHistoryService historyService)
    {
        _historyService = historyService;
        _ = LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        var historyList = await _historyService.GetHistoryAsync();
        History = new ObservableCollection<RequestHistory>(historyList);
    }

    [RelayCommand]
    private async Task ClearHistoryAsync()
    {
        await _historyService.ClearHistoryAsync();
        History.Clear();
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
