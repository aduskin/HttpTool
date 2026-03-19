using HttpTool.Core.Models;

namespace HttpTool.Core.Interfaces;

/// <summary>
/// 存储服务接口
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// 保存项目
    /// </summary>
    Task SaveProjectAsync(Project project);

    /// <summary>
    /// 加载项目
    /// </summary>
    Task<Project?> LoadProjectAsync(string filePath);

    /// <summary>
    /// 获取最近的项目列表
    /// </summary>
    Task<List<string>> GetRecentProjectsAsync();

    /// <summary>
    /// 添加到最近项目
    /// </summary>
    Task AddToRecentProjectsAsync(string filePath);

    /// <summary>
    /// 获取应用数据目录
    /// </summary>
    string GetAppDataPath();
}
