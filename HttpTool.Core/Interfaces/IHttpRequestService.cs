using HttpTool.Core.Models;

namespace HttpTool.Core.Interfaces;

/// <summary>
/// HTTP 请求服务接口
/// </summary>
public interface IHttpRequestService
{
    /// <summary>
    /// 发送请求
    /// </summary>
    Task<ApiResponse> SendRequestAsync(ApiRequest request, Dictionary<string, string>? variables = null);

    /// <summary>
    /// 替换变量
    /// </summary>
    string ReplaceVariables(string input, Dictionary<string, string> variables);
}
