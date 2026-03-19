using System.Collections.ObjectModel;
using AduSkin.Controls;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.Views;

/// <summary>
/// EnvironmentWindow.xaml 的交互逻辑
/// </summary>
public partial class EnvironmentWindow : AduWindow
{
    public ObservableCollection<EnvironmentVariable> Variables { get; } = new();

    public EnvironmentWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public EnvironmentWindow(IEnumerable<EnvironmentVariable> variables) : this()
    {
        foreach (var variable in variables)
        {
            Variables.Add(new EnvironmentVariable(variable.Key, variable.Value, variable.IsEnabled));
        }
    }

    private void Add_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        Variables.Add(new EnvironmentVariable("new_variable", "", true));
    }

    private void Save_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
