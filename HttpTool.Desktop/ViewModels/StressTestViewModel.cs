using AduSkin.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 压测 ViewModel
/// </summary>
public partial class StressTestViewModel : ObservableObject
{
    private readonly IHttpRequestService _httpRequestService;
    private readonly Dispatcher _dispatcher;
    private CancellationTokenSource? _cts;
    private readonly object _lock = new();

    /// <summary>
    /// 被压测的 API 请求
    /// </summary>
    [ObservableProperty]
    private ApiRequest _apiRequest;

    /// <summary>
    /// 项目
    /// </summary>
    public Project Project { get; }

    /// <summary>
    /// 环境变量
    /// </summary>
    public Dictionary<string, string>? Variables { get; }

    /// <summary>
    /// 可选的 API 列表
    /// </summary>
    public ObservableCollection<ApiRequest> Apis { get; }

    [ObservableProperty]
    private StressTestConfig _config = new();

    [NotifyCanExecuteChangedFor(nameof(StopCommand))]
    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private int _successCount;

    [ObservableProperty]
    private int _failCount;

    [ObservableProperty]
    private double _avgResponseTime;

    [ObservableProperty]
    private double _minResponseTime;

    [ObservableProperty]
    private double _maxResponseTime;

    [ObservableProperty]
    private double _currentRps;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private double _progressValue;

    /// <summary>
    /// 所有压测结果
    /// </summary>
    public ObservableCollection<StressTestResult> Results { get; } = new();

    /// <summary>
    /// 图表数据系列
    /// </summary>
    public ObservableCollection<AduTrendSeries> ChartSeries { get; } = new();

    /// <summary>
    /// 图表数据点
    /// </summary>
    private ObservableCollection<Point> _responseTimePoints = new();

    public StressTestViewModel(
        ApiRequest apiRequest,
        Dictionary<string, string>? variables,
        IHttpRequestService httpRequestService,
        Project project)
    {
        _apiRequest = apiRequest;
        Variables = variables;
        Project = project;
        _httpRequestService = httpRequestService;
        _dispatcher = Application.Current.Dispatcher;
        Apis = new ObservableCollection<ApiRequest>(project.Apis);

        // 初始化趋势图数据系列
        var trendSeries = new AduTrendSeries
        {
            Name = "Response Time (ms)",
            Stroke = new SolidColorBrush(Color.FromRgb(0x00, 0x7b, 0xff)),
            Fill = new SolidColorBrush(Color.FromRgb(0x00, 0x7b, 0xff)),
            StrokeThickness = 1.5,
            FillOpacity = 0.2,
            ShowPoints = false,
            DataPoints = _responseTimePoints
        };
        ChartSeries.Add(trendSeries);
    }

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task StartAsync()
    {
        if (IsRunning) return;

        IsRunning = true;
        _cts = new CancellationTokenSource();
        CompletedCount = 0;
        SuccessCount = 0;
        FailCount = 0;
        AvgResponseTime = 0;
        MinResponseTime = 0;
        MaxResponseTime = 0;
        CurrentRps = 0;
        ProgressValue = 0;
        StatusText = "Running...";

        Results.Clear();
        _responseTimePoints.Clear();

        try
        {
            var total = Config.TotalRequests;
            var interval = Config.IntervalMs;
            var concurrency = Math.Max(1, Config.Concurrency);
            var warmup = Config.WarmupRequests;

            // 预热阶段
            if (warmup > 0)
            {
                StatusText = "Warming up...";
                for (var i = 0; i < warmup && !_cts.Token.IsCancellationRequested; i++)
                {
                    await _httpRequestService.SendRequestAsync(ApiRequest, Variables, _cts.Token);
                }
                StatusText = "Running...";
            }

            // 使用 SemaphoreSlim 控制并发
            using var semaphore = new SemaphoreSlim(concurrency);
            var tasks = new List<Task>();
            var completed = 0;
            var success = 0;
            var fail = 0;
            long totalResponseTime = 0;
            var minTime = double.MaxValue;
            var maxTime = 0.0;
            var rpsStopwatch = System.Diagnostics.Stopwatch.StartNew();
            var rpsCount = 0;

            for (var i = 0; i < total; i++)
            {
                if (_cts.Token.IsCancellationRequested)
                    break;

                await semaphore.WaitAsync(_cts.Token);

                var index = i + 1;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var reqStart = DateTime.Now;
                        var response = await _httpRequestService.SendRequestAsync(ApiRequest, Variables, _cts!.Token);
                        var responseTime = response.ElapsedMilliseconds;

                        Interlocked.Increment(ref completed);
                        Interlocked.Add(ref rpsCount, 1);

                        var result = new StressTestResult
                        {
                            Index = index,
                            Timestamp = reqStart,
                            StatusCode = response.StatusCode,
                            ResponseTimeMs = responseTime,
                            IsSuccess = response.IsSuccess,
                            ErrorMessage = response.ErrorMessage
                        };

                        if (response.IsSuccess)
                        {
                            Interlocked.Increment(ref success);
                        }
                        else
                        {
                            Interlocked.Increment(ref fail);
                        }

                        Interlocked.Add(ref totalResponseTime, (long)responseTime);

                        // 更新最大/最小响应时间（需要锁）
                        lock (_lock)
                        {
                            if (responseTime < minTime) minTime = responseTime;
                            if (responseTime > maxTime) maxTime = responseTime;
                        }

                        // 更新 UI（线程安全）
                        await _dispatcher.InvokeAsync(() =>
                        {
                            Results.Add(result);

                            // 添加到图表
                            _responseTimePoints.Add(new Point(index, responseTime));

                            // 更新统计
                            CompletedCount = completed;
                            SuccessCount = success;
                            FailCount = fail;
                            AvgResponseTime = completed > 0 ? totalResponseTime / completed : 0;
                            MinResponseTime = minTime == double.MaxValue ? 0 : minTime;
                            MaxResponseTime = maxTime;

                            // 计算 RPS
                            var elapsed = rpsStopwatch.Elapsed.TotalSeconds;
                            CurrentRps = elapsed > 0 ? rpsCount / elapsed : 0;

                            // 更新进度
                            ProgressValue = (double)completed / total * 100;
                            StatusText = $"Running... ({completed}/{total})";
                        });
                    }
                    finally
                    {
                        semaphore.Release();

                        // 间隔等待（只在并发为1时严格控制间隔）
                        if (concurrency == 1 && interval > 0)
                        {
                            await Task.Delay(interval, _cts.Token);
                        }
                    }
                }, _cts.Token));

                // 并发模式下，不等待直接发送下一个
                if (concurrency > 1 && interval > 0)
                {
                    await Task.Delay(interval, _cts.Token);
                }
            }

            // 等待所有任务完成
            await Task.WhenAll(tasks);

            StatusText = CompletedCount == total ? "Completed" : "Stopped";
        }
        catch (OperationCanceledException)
        {
            StatusText = "Stopped";
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanStart() => !IsRunning;

    [RelayCommand(CanExecute = nameof(CanStop))]
    private void Stop()
    {
        _cts?.Cancel();
        StatusText = "Stopping...";
    }

    private bool CanStop() => IsRunning;

    [RelayCommand]
    private void Clear()
    {
        Results.Clear();
        _responseTimePoints.Clear();
        CompletedCount = 0;
        SuccessCount = 0;
        FailCount = 0;
        AvgResponseTime = 0;
        MinResponseTime = 0;
        MaxResponseTime = 0;
        CurrentRps = 0;
        ProgressValue = 0;
        StatusText = "Ready";
    }
}
