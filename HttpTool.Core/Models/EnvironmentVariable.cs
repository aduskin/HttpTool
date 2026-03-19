using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// 环境变量
/// </summary>
public partial class EnvironmentVariable : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isChecked;

    public EnvironmentVariable() { }

    public EnvironmentVariable(string key, string value, bool isEnabled = true)
    {
        _key = key;
        _value = value;
        _isEnabled = isEnabled;
    }
}
