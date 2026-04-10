using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// 压测配置
/// </summary>
public partial class StressTestConfig : ObservableObject
{
    /// <summary>
    /// 总请求数
    /// </summary>
    [ObservableProperty]
    private int _totalRequests = 100;

    /// <summary>
    /// 请求间隔（毫秒）
    /// </summary>
    [ObservableProperty]
    private int _intervalMs = 100;

    /// <summary>
    /// 并发数
    /// </summary>
    [ObservableProperty]
    private int _concurrency = 1;

    /// <summary>
    /// 预热请求数（不计入统计）
    /// </summary>
    [ObservableProperty]
    private int _warmupRequests = 0;
}
