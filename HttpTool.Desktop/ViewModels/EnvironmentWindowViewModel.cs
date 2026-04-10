using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 环境变量窗口 ViewModel
/// </summary>
public partial class EnvironmentWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private System.Collections.ObjectModel.ObservableCollection<EnvironmentVariable> _variables = new();

    [RelayCommand]
    private void Add()
    {
        Variables.Add(new EnvironmentVariable("new_variable", "", true));
    }

    [RelayCommand]
    private void Save(Window? window)
    {
        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
    }

    [RelayCommand]
    private void Close(Window? window)
    {
        if (window != null)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}
