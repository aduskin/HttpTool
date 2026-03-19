using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// 环境配置
/// </summary>
public partial class Environment : ObservableObject
{
    [ObservableProperty]
    private string _name = "Default";

    [ObservableProperty]
    private List<EnvironmentVariable> _variables = new();
}
