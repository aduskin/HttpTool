using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// 键值对（用于 Headers、Query 参数等）
/// </summary>
public partial class KeyValueItem : ObservableObject
{
    [ObservableProperty]
    private string _key = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;

    [ObservableProperty]
    private bool _isEnabled = true;

    [ObservableProperty]
    private bool _isChecked;

    public KeyValueItem() { }

    public KeyValueItem(string key, string value, bool isEnabled = true)
    {
        _key = key;
        _value = value;
        _isEnabled = isEnabled;
    }
}
