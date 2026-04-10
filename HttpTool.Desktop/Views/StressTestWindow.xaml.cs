using AduSkin.Controls;
using HttpTool.Desktop.ViewModels;

namespace HttpTool.Desktop.Views;

/// <summary>
/// StressTestWindow.xaml 的交互逻辑
/// </summary>
public partial class StressTestWindow : AduWindow
{
    public StressTestWindow(StressTestViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
