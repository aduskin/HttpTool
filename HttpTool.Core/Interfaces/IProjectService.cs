using HttpTool.Core.Models;

namespace HttpTool.Core.Interfaces;

/// <summary>
/// 项目服务接口
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// 当前项目
    /// </summary>
    Project? CurrentProject { get; }

    /// <summary>
    /// 创建新项目
    /// </summary>
    Project CreateProject();

    /// <summary>
    /// 打开项目
    /// </summary>
    Task<Project?> OpenProjectAsync(string filePath);

    /// <summary>
    /// 保存当前项目
    /// </summary>
    Task SaveCurrentProjectAsync();

    /// <summary>
    /// 保存项目到指定路径
    /// </summary>
    Task SaveProjectAsAsync(string filePath);

    /// <summary>
    /// 关闭当前项目
    /// </summary>
    void CloseProject();

    /// <summary>
    /// 获取最近项目列表
    /// </summary>
    Task<List<string>> GetRecentProjectsAsync();

    /// <summary>
    /// 项目已变更事件
    /// </summary>
    event EventHandler<Project?>? ProjectChanged;
}
