using System.Windows;
using AduSkin.Controls;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.Views;

/// <summary>
/// EnvironmentWindow.xaml 的交互逻辑
/// </summary>
public partial class EnvironmentWindow : AduWindow
{
    public EnvironmentWindow()
    {
        InitializeComponent();
    }

    public EnvironmentWindow(IEnumerable<EnvironmentVariable> variables) : this()
    {
        if (DataContext is ViewModels.EnvironmentWindowViewModel vm)
        {
            foreach (var variable in variables)
            {
                vm.Variables.Add(new EnvironmentVariable(variable.Key, variable.Value, variable.IsEnabled));
            }
        }
    }
}
