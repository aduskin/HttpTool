using System.Diagnostics;
using System.Net.Http;
using System.Text;
using HttpTool.Core.Enums;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// HTTP 请求服务实现
/// </summary>
public class HttpRequestService : IHttpRequestService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpRequestService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ApiResponse> SendRequestAsync(ApiRequest request, Dictionary<string, string>? variables = null)
    {
        var response = new ApiResponse();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var client = _httpClientFactory.CreateClient("HttpTool");

            // 设置超时
            client.Timeout = TimeSpan.FromSeconds(request.TimeoutSeconds > 0 ? request.TimeoutSeconds : 30);

            // 替换变量
            var url = ReplaceVariables(request.Url, variables ?? new Dictionary<string, string>());

            // 构建 URL（添加 Query 参数）
            if (request.QueryParams.Count > 0)
            {
                var queryString = BuildQueryString(request.QueryParams, variables);
                if (!string.IsNullOrEmpty(queryString))
                {
                    url = url.Contains('?') ? $"{url}&{queryString}" : $"{url}?{queryString}";
                }
            }

            // 创建 HttpMethod
            var httpMethod = new HttpMethod(request.Method.ToString());

            var httpRequest = new HttpRequestMessage(httpMethod, url);

            // 添加 Headers
            foreach (var header in request.Headers.Where(h => h.IsEnabled && !string.IsNullOrEmpty(h.Key)))
            {
                var key = ReplaceVariables(header.Key, variables ?? new Dictionary<string, string>());
                var value = ReplaceVariables(header.Value, variables ?? new Dictionary<string, string>());
                httpRequest.Headers.TryAddWithoutValidation(key, value);
            }

            // 添加认证
            AddAuthentication(httpRequest, request, variables);

            // 添加 Body
            if (request.Method != HttpMethodType.GET && request.Method != HttpMethodType.HEAD && request.BodyType != BodyType.None)
            {
                var body = ReplaceVariables(request.Body, variables ?? new Dictionary<string, string>());
                var contentType = GetContentType(request.BodyType, request.Headers);

                if (request.BodyType == BodyType.Binary && !string.IsNullOrEmpty(request.BinaryFilePath))
                {
                    var filePath = ReplaceVariables(request.BinaryFilePath, variables ?? new Dictionary<string, string>());
                    if (File.Exists(filePath))
                    {
                        var fileName = Path.GetFileName(filePath);
                        var fileContent = await File.ReadAllBytesAsync(filePath);
                        var fileContentType = GetMimeType(fileName);
                        var streamContent = new ByteArrayContent(fileContent);
                        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(fileContentType);
                        streamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                        {
                            Name = "file",
                            FileName = fileName
                        };
                        httpRequest.Content = streamContent;
                    }
                }
                else
                {
                    httpRequest.Content = new StringContent(body, Encoding.UTF8, contentType);
                }
            }

            // 发送请求
            var httpResponse = await client.SendAsync(httpRequest);
            stopwatch.Stop();

            // 构建响应
            response.StatusCode = (int)httpResponse.StatusCode;
            response.StatusDescription = httpResponse.ReasonPhrase ?? string.Empty;
            response.IsSuccess = httpResponse.IsSuccessStatusCode;
            response.Elapsed = stopwatch.Elapsed;

            // 响应 Headers
            foreach (var header in httpResponse.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }
            foreach (var header in httpResponse.Content.Headers)
            {
                response.Headers[header.Key] = string.Join(", ", header.Value);
            }

            // 响应 Body
            response.Body = await httpResponse.Content.ReadAsStringAsync();
            response.ContentLength = response.Body.Length;
            response.ContentType = httpResponse.Content.Headers.ContentType?.MediaType ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            response.Elapsed = stopwatch.Elapsed;
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
            response.StatusCode = 0;
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            response.Elapsed = stopwatch.Elapsed;
            response.IsSuccess = false;
            response.ErrorMessage = "Request timed out";
            response.StatusCode = 0;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.Elapsed = stopwatch.Elapsed;
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
            response.StatusCode = 0;
        }

        return response;
    }

    public string ReplaceVariables(string input, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(input) || variables.Count == 0)
        {
            return input;
        }

        var result = input;
        foreach (var variable in variables)
        {
            result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
        }
        return result;
    }

    private string BuildQueryString(List<KeyValueItem> queryParams, Dictionary<string, string>? variables)
    {
        var enabledParams = queryParams.Where(p => p.IsEnabled && !string.IsNullOrEmpty(p.Key));

        var queryParts = new List<string>();
        foreach (var param in enabledParams)
        {
            var key = ReplaceVariables(param.Key, variables ?? new Dictionary<string, string>());
            var value = ReplaceVariables(param.Value, variables ?? new Dictionary<string, string>());
            queryParts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
        }

        return string.Join("&", queryParts);
    }

    private void AddAuthentication(HttpRequestMessage request, ApiRequest apiRequest, Dictionary<string, string>? variables)
    {
        switch (apiRequest.AuthType)
        {
            case AuthType.Basic:
                var basicValue = ReplaceVariables(apiRequest.AuthValue, variables ?? new Dictionary<string, string>());
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicValue);
                break;

            case AuthType.Bearer:
                var bearerValue = ReplaceVariables(apiRequest.AuthValue, variables ?? new Dictionary<string, string>());
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerValue);
                break;

            case AuthType.ApiKey:
                var apiKeyValue = ReplaceVariables(apiRequest.AuthValue, variables ?? new Dictionary<string, string>());
                request.Headers.TryAddWithoutValidation("X-API-Key", apiKeyValue);
                break;

            case AuthType.OAuth2:
                var token = ReplaceVariables(apiRequest.OAuthConfig.AccessToken, variables ?? new Dictionary<string, string>());
                if (!string.IsNullOrEmpty(token))
                {
                    var tokenType = string.IsNullOrEmpty(apiRequest.OAuthConfig.TokenType) ? "Bearer" : apiRequest.OAuthConfig.TokenType;
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(tokenType, token);
                }
                break;
        }
    }

    private string GetContentType(BodyType bodyType, List<KeyValueItem> headers)
    {
        // 检查是否已设置 Content-Type
        var existingContentType = headers.FirstOrDefault(h =>
            h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) && h.IsEnabled);

        if (existingContentType != null)
        {
            return existingContentType.Value;
        }

        return bodyType switch
        {
            BodyType.Json => "application/json",
            BodyType.Xml => "application/xml",
            BodyType.FormUrlEncoded => "application/x-www-form-urlencoded",
            BodyType.FormData => "multipart/form-data",
            BodyType.Raw => "text/plain",
            BodyType.Binary => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }

    private string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
