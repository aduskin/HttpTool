using HttpTool.Core.Models;

namespace HttpTool.Core.Interfaces;

/// <summary>
/// 历史记录服务接口
/// </summary>
public interface IHistoryService
{
    /// <summary>
    /// 添加历史记录
    /// </summary>
    Task AddHistoryAsync(RequestHistory history);

    /// <summary>
    /// 获取历史记录
    /// </summary>
    Task<List<RequestHistory>> GetHistoryAsync(string? projectId = null, int limit = 100);

    /// <summary>
    /// 清除历史记录
    /// </summary>
    Task ClearHistoryAsync(string? projectId = null);

    /// <summary>
    /// 历史记录已添加事件
    /// </summary>
    event EventHandler<RequestHistory>? HistoryAdded;
}
