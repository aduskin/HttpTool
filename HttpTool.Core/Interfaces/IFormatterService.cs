namespace HttpTool.Core.Interfaces;

/// <summary>
/// 格式化服务接口
/// </summary>
public interface IFormatterService
{
    /// <summary>
    /// 格式化内容
    /// </summary>
    string Format(string content, string contentType);
}
