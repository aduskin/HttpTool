namespace HttpTool.Core.Models;

/// <summary>
/// 单个压测请求的结果
/// </summary>
public class StressTestResult
{
    /// <summary>
    /// 请求序号
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// 请求时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 响应状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 响应时间（毫秒）
    /// </summary>
    public double ResponseTimeMs { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误信息（如果有）
    /// </summary>
    public string? ErrorMessage { get; set; }
}
