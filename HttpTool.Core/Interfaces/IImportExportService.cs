using HttpTool.Core.Models;

namespace HttpTool.Core.Interfaces;

/// <summary>
/// 导入导出服务接口
/// </summary>
public interface IImportExportService
{
    /// <summary>
    /// 导入项目（自动检测格式：Postman/HAR/Insomnia/Swagger）
    /// </summary>
    Task<Project?> ImportAsync(string filePath);

    /// <summary>
    /// 导入 Postman Collection v2.1
    /// </summary>
    Task<Project?> ImportFromPostmanAsync(string filePath);

    /// <summary>
    /// 导入 HAR 文件
    /// </summary>
    Task<Project?> ImportFromHarAsync(string filePath);

    /// <summary>
    /// 导入 Insomnia 导出
    /// </summary>
    Task<Project?> ImportFromInsomniaAsync(string filePath);

    /// <summary>
    /// 导入 Swagger/OpenAPI 规范
    /// </summary>
    Task<Project?> ImportFromSwaggerAsync(string filePath);

    /// <summary>
    /// 导出为 cURL 命令
    /// </summary>
    string ExportToCurl(ApiRequest request, Dictionary<string, string>? variables = null);

    /// <summary>
    /// 导出为 JSON 格式
    /// </summary>
    Task ExportAsync(Project project, string filePath);

    /// <summary>
    /// 导出为 HAR 格式
    /// </summary>
    Task ExportToHarAsync(Project project, string filePath);

    /// <summary>
    /// 导出为 Insomnia 格式
    /// </summary>
    Task ExportToInsomniaAsync(Project project, string filePath);

    /// <summary>
    /// 导出为 Swagger/OpenAPI 格式
    /// </summary>
    Task ExportToSwaggerAsync(Project project, string filePath);
}
