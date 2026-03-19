using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using HttpTool.Core.Enums;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// 导入导出服务实现
/// </summary>
public class ImportExportService : IImportExportService
{
    private readonly IHttpRequestService _httpRequestService;
    private readonly JsonSerializerOptions _jsonOptions;

    public ImportExportService(IHttpRequestService httpRequestService)
    {
        _httpRequestService = httpRequestService;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<Project?> ImportAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".json" => await ImportPostmanCollectionAsync(filePath),
            ".httptool" => await ImportHttpToolProjectAsync(filePath),
            _ => await ImportPostmanCollectionAsync(filePath)
        };
    }

    private async Task<Project> ImportPostmanCollectionAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var project = new Project
        {
            Name = Path.GetFileNameWithoutExtension(filePath)
        };

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 检查是 Postman Collection v2.1 格式
            if (root.TryGetProperty("info", out var info) &&
                info.TryGetProperty("name", out var name))
            {
                project.Name = name.GetString() ?? project.Name;
            }

            // 解析 items（Postman v2.1）
            if (root.TryGetProperty("item", out var items))
            {
                foreach (var item in items.EnumerateArray())
                {
                    var api = ParsePostmanItem(item);
                    if (api != null)
                    {
                        project.Apis.Add(api);
                    }
                }
            }
        }
        catch (JsonException)
        {
            // 如果不是有效的 JSON，尝试作为普通 API 列表导入
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in doc.RootElement.EnumerateArray())
                    {
                        var api = ParseJsonApiItem(item);
                        if (api != null)
                        {
                            project.Apis.Add(api);
                        }
                    }
                }
            }
            catch
            {
                // 忽略解析错误
            }
        }

        return project;
    }

    private ApiRequest? ParsePostmanItem(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return null;

        var request = new ApiRequest();

        // 获取名称
        if (item.TryGetProperty("name", out var name))
        {
            request.Name = name.GetString() ?? "Unnamed";
        }

        // 获取请求
        if (item.TryGetProperty("request", out var reqElement))
        {
            // 方法
            if (reqElement.TryGetProperty("method", out var method))
            {
                var methodStr = method.GetString() ?? "GET";
                if (Enum.TryParse<HttpMethodType>(methodStr, true, out var httpMethod))
                {
                    request.Method = httpMethod;
                }
            }

            // URL
            if (reqElement.TryGetProperty("url", out var urlElement))
            {
                request.Url = urlElement.ValueKind == JsonValueKind.String
                    ? urlElement.GetString() ?? ""
                    : urlElement.TryGetProperty("raw", out var raw) ? raw.GetString() ?? "" : "";
            }

            // Headers
            if (reqElement.TryGetProperty("header", out var headers))
            {
                foreach (var header in headers.EnumerateArray())
                {
                    var key = header.TryGetProperty("key", out var hKey) ? hKey.GetString() ?? "" : "";
                    var value = header.TryGetProperty("value", out var hValue) ? hValue.GetString() ?? "" : "";
                    if (!string.IsNullOrEmpty(key))
                    {
                        request.Headers.Add(new KeyValueItem(key, value));
                    }
                }
            }

            // Body
            if (reqElement.TryGetProperty("body", out var body))
            {
                if (body.TryGetProperty("mode", out var mode))
                {
                    var modeStr = mode.GetString();
                    switch (modeStr)
                    {
                        case "raw" when body.TryGetProperty("raw", out var rawBody):
                            request.Body = rawBody.GetString() ?? "";
                            request.BodyType = body.TryGetProperty("options", out var options) &&
                                             options.TryGetProperty("raw", out var rawOptions) &&
                                             rawOptions.TryGetProperty("language", out var lang) &&
                                             lang.GetString() == "json"
                                ? BodyType.Json
                                : BodyType.Raw;
                            break;
                        case "formdata" when body.TryGetProperty("formdata", out var formData):
                            request.BodyType = BodyType.FormUrlEncoded;
                            var formLines = formData.EnumerateArray()
                                .Select(f => $"{f.GetProperty("key")}={f.GetProperty("value")}");
                            request.Body = string.Join("&", formLines);
                            break;
                    }
                }
            }
        }

        return request;
    }

    private ApiRequest? ParseJsonApiItem(JsonElement item)
    {
        if (item.ValueKind != JsonValueKind.Object)
            return null;

        var request = new ApiRequest();

        if (item.TryGetProperty("name", out var name))
            request.Name = name.GetString() ?? "Unnamed";

        if (item.TryGetProperty("method", out var method))
        {
            var methodStr = method.GetString() ?? "GET";
            if (Enum.TryParse<HttpMethodType>(methodStr, true, out var httpMethod))
            {
                request.Method = httpMethod;
            }
        }

        if (item.TryGetProperty("url", out var url))
            request.Url = url.GetString() ?? "";

        if (item.TryGetProperty("headers", out var headers))
        {
            foreach (var header in headers.EnumerateArray())
            {
                if (header.ValueKind == JsonValueKind.Object)
                {
                    var key = header.TryGetProperty("key", out var k) ? k.GetString() ?? "" : "";
                    var value = header.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "";
                    if (!string.IsNullOrEmpty(key))
                    {
                        request.Headers.Add(new KeyValueItem(key, value));
                    }
                }
                else if (header.ValueKind == JsonValueKind.String)
                {
                    request.Headers.Add(new KeyValueItem("", header.GetString() ?? ""));
                }
            }
        }

        if (item.TryGetProperty("body", out var body))
        {
            request.Body = body.GetString() ?? "";
            if (!string.IsNullOrEmpty(request.Body))
            {
                request.BodyType = BodyType.Json;
            }
        }

        return request;
    }

    private async Task<Project> ImportHttpToolProjectAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var project = JsonSerializer.Deserialize<Project>(json, _jsonOptions);
        if (project != null)
        {
            project.FilePath = filePath;
        }
        return project ?? new Project();
    }

    public string ExportToCurl(ApiRequest request, Dictionary<string, string>? variables = null)
    {
        var sb = new StringBuilder();
        sb.Append("curl -X ");
        sb.Append(request.Method.ToString());
        sb.AppendLine();

        // URL
        var url = _httpRequestService.ReplaceVariables(request.Url, variables ?? new Dictionary<string, string>());

        // Query params
        if (request.QueryParams.Count > 0)
        {
            var queryParts = request.QueryParams
                .Where(p => p.IsEnabled && !string.IsNullOrEmpty(p.Key))
                .Select(p =>
                {
                    var key = _httpRequestService.ReplaceVariables(p.Key, variables ?? new Dictionary<string, string>());
                    var value = _httpRequestService.ReplaceVariables(p.Value, variables ?? new Dictionary<string, string>());
                    return $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
                });
            var queryString = string.Join("&", queryParts);
            if (!string.IsNullOrEmpty(queryString))
            {
                url += url.Contains('?') ? $"&{queryString}" : $"?{queryString}";
            }
        }

        sb.Append($"  \"{url}\"");
        sb.AppendLine();

        // Headers
        foreach (var header in request.Headers.Where(h => h.IsEnabled && !string.IsNullOrEmpty(h.Key)))
        {
            var key = _httpRequestService.ReplaceVariables(header.Key, variables ?? new Dictionary<string, string>());
            var value = _httpRequestService.ReplaceVariables(header.Value, variables ?? new Dictionary<string, string>());
            sb.AppendLine($"  -H \"{key}: {value}\"");
        }

        // Auth
        switch (request.AuthType)
        {
            case AuthType.Basic:
                var basicValue = _httpRequestService.ReplaceVariables(request.AuthValue, variables ?? new Dictionary<string, string>());
                sb.AppendLine($"  -H \"Authorization: Basic {basicValue}\"");
                break;
            case AuthType.Bearer:
                var bearerValue = _httpRequestService.ReplaceVariables(request.AuthValue, variables ?? new Dictionary<string, string>());
                sb.AppendLine($"  -H \"Authorization: Bearer {bearerValue}\"");
                break;
            case AuthType.ApiKey:
                var apiKeyValue = _httpRequestService.ReplaceVariables(request.AuthValue, variables ?? new Dictionary<string, string>());
                sb.AppendLine($"  -H \"X-API-Key: {apiKeyValue}\"");
                break;
        }

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            var body = _httpRequestService.ReplaceVariables(request.Body, variables ?? new Dictionary<string, string>());

            // Escape quotes and handle newlines
            body = body.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r\n", "\\n").Replace("\n", "\\n");

            var contentType = request.BodyType switch
            {
                BodyType.Json => "application/json",
                BodyType.Xml => "application/xml",
                BodyType.FormUrlEncoded => "application/x-www-form-urlencoded",
                BodyType.Raw => "text/plain",
                _ => "application/octet-stream"
            };

            sb.AppendLine($"  -H \"Content-Type: {contentType}\"");
            sb.AppendLine($"  -d \"{body}\"");
        }

        return sb.ToString();
    }

    public async Task ExportAsync(Project project, string filePath)
    {
        var json = JsonSerializer.Serialize(project, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Project?> ImportFromPostmanAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        return await ImportPostmanCollectionAsync(filePath);
    }

    public async Task<Project?> ImportFromHarAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        var project = new Project
        {
            Name = Path.GetFileNameWithoutExtension(filePath)
        };

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // HAR 格式有 log 数组
            if (root.TryGetProperty("log", out var log))
            {
                foreach (var entry in log.EnumerateArray())
                {
                    if (entry.TryGetProperty("request", out var request))
                    {
                        var api = ParseHarRequest(request);
                        if (api != null)
                        {
                            project.Apis.Add(api);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return project;
    }

    private ApiRequest? ParseHarRequest(JsonElement request)
    {
        var api = new ApiRequest();

        // Method
        if (request.TryGetProperty("method", out var method))
        {
            var methodStr = method.GetString() ?? "GET";
            if (Enum.TryParse<HttpMethodType>(methodStr, true, out var httpMethod))
            {
                api.Method = httpMethod;
            }
        }

        // URL
        if (request.TryGetProperty("url", out var url))
        {
            api.Url = url.ValueKind == JsonValueKind.String ? url.GetString() ?? "" : "";
        }

        // Headers
        if (request.TryGetProperty("headers", out var headers))
        {
            foreach (var header in headers.EnumerateArray())
            {
                var name = header.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var value = header.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(name))
                {
                    api.Headers.Add(new KeyValueItem(name, value));
                }
            }
        }

        // Query params
        if (request.TryGetProperty("queryString", out var queryString))
        {
            foreach (var param in queryString.EnumerateArray())
            {
                var name = param.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var value = param.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(name))
                {
                    api.QueryParams.Add(new KeyValueItem(name, value));
                }
            }
        }

        // Body
        if (request.TryGetProperty("postData", out var postData))
        {
            if (postData.TryGetProperty("text", out var bodyText))
            {
                api.Body = bodyText.GetString() ?? "";
            }
            if (postData.TryGetProperty("mimeType", out var mimeType))
            {
                var mime = mimeType.GetString() ?? "";
                api.BodyType = mime.Contains("json") ? BodyType.Json :
                              mime.Contains("xml") ? BodyType.Xml :
                              mime.Contains("form") ? BodyType.FormUrlEncoded :
                              BodyType.Raw;
            }
        }

        return api;
    }

    public async Task<Project?> ImportFromInsomniaAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        var project = new Project
        {
            Name = Path.GetFileNameWithoutExtension(filePath)
        };

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Insomnia 导出格式
            if (root.TryGetProperty("resources", out var resources))
            {
                foreach (var resource in resources.EnumerateArray())
                {
                    if (resource.TryGetProperty("_type", out var type) &&
                        type.GetString() == "request")
                    {
                        var api = ParseInsomniaRequest(resource);
                        if (api != null)
                        {
                            project.Apis.Add(api);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return project;
    }

    private ApiRequest? ParseInsomniaRequest(JsonElement resource)
    {
        var api = new ApiRequest();

        if (resource.TryGetProperty("name", out var name))
            api.Name = name.GetString() ?? "Unnamed";

        if (resource.TryGetProperty("method", out var method))
        {
            var methodStr = method.GetString() ?? "GET";
            if (Enum.TryParse<HttpMethodType>(methodStr, true, out var httpMethod))
            {
                api.Method = httpMethod;
            }
        }

        if (resource.TryGetProperty("url", out var url))
            api.Url = url.GetString() ?? "";

        if (resource.TryGetProperty("headers", out var headers))
        {
            foreach (var header in headers.EnumerateArray())
            {
                var hName = header.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                var hValue = header.TryGetProperty("value", out var v) ? v.GetString() ?? "" : "";
                if (!string.IsNullOrEmpty(hName))
                {
                    api.Headers.Add(new KeyValueItem(hName, hValue));
                }
            }
        }

        if (resource.TryGetProperty("body", out var body))
        {
            if (body.TryGetProperty("text", out var bodyText))
            {
                api.Body = bodyText.GetString() ?? "";
            }
            if (body.TryGetProperty("mimeType", out var mimeType))
            {
                var mime = mimeType.GetString() ?? "";
                api.BodyType = mime.Contains("json") ? BodyType.Json :
                              mime.Contains("xml") ? BodyType.Xml :
                              mime.Contains("form") ? BodyType.FormUrlEncoded :
                              BodyType.Raw;
            }
        }

        return api;
    }

    public async Task<Project?> ImportFromSwaggerAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        var project = new Project
        {
            Name = Path.GetFileNameWithoutExtension(filePath)
        };

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Swagger/OpenAPI 格式
            var isOpenApi = root.TryGetProperty("openapi", out var openapi);
            var isSwagger = root.TryGetProperty("swagger", out var swagger);

            if (!isOpenApi && !isSwagger)
                return project;

            if (isOpenApi)
                project.Name = root.TryGetProperty("info", out var info) &&
                              info.TryGetProperty("title", out var title) ?
                              title.GetString() ?? project.Name : project.Name;

            // 解析 paths
            if (root.TryGetProperty("paths", out var paths))
            {
                foreach (var path in paths.EnumerateObject())
                {
                    foreach (var method in path.Value.EnumerateObject())
                    {
                        if (Enum.TryParse<HttpMethodType>(method.Name, true, out var httpMethod))
                        {
                            var api = new ApiRequest
                            {
                                Name = method.Value.TryGetProperty("summary", out var summary) ?
                                       summary.GetString() ?? path.Name : path.Name,
                                Method = httpMethod,
                                Url = "{{baseUrl}}" + path.Name
                            };

                            // Parameters
                            if (method.Value.TryGetProperty("parameters", out var parameters))
                            {
                                foreach (var param in parameters.EnumerateArray())
                                {
                                    var pName = param.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                                    var pIn = param.TryGetProperty("in", out var pInElem) ? pInElem.GetString() ?? "" : "";

                                    if (pIn == "query")
                                    {
                                        var pValue = param.TryGetProperty("default", out var def) ? def.GetString() ?? "" : "";
                                        api.QueryParams.Add(new KeyValueItem(pName, pValue));
                                    }
                                    else if (pIn == "header")
                                    {
                                        var pValue = param.TryGetProperty("default", out var def) ? def.GetString() ?? "" : "";
                                        api.Headers.Add(new KeyValueItem(pName, pValue));
                                    }
                                }
                            }

                            project.Apis.Add(api);
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略解析错误
        }

        return project;
    }

    public async Task ExportToHarAsync(Project project, string filePath)
    {
        var har = new
        {
            log = new
            {
                version = "1.2",
                creator = new { name = "HttpTool", version = "1.0" },
                entries = project.Apis.Select(api => new
                {
                    startedDateTime = DateTime.UtcNow.ToString("o"),
                    time = 0,
                    request = new
                    {
                        method = api.Method.ToString(),
                        url = api.Url,
                        httpVersion = "HTTP/1.1",
                        headers = api.Headers.Select(h => new { name = h.Key, value = h.Value }),
                        queryString = api.QueryParams.Select(p => new { name = p.Key, value = p.Value }),
                        postData = !string.IsNullOrEmpty(api.Body) ? new
                        {
                            mimeType = api.BodyType switch
                            {
                                BodyType.Json => "application/json",
                                BodyType.Xml => "application/xml",
                                BodyType.FormUrlEncoded => "application/x-www-form-urlencoded",
                                _ => "text/plain"
                            },
                            text = api.Body
                        } : null,
                        headersSize = -1,
                        bodySize = api.Body?.Length ?? 0
                    },
                    response = new
                    {
                        status = 0,
                        statusText = "",
                        httpVersion = "HTTP/1.1",
                        headers = Array.Empty<object>(),
                        content = new { size = 0, mimeType = "" },
                        redirectURL = "",
                        headersSize = -1,
                        bodySize = 0
                    },
                    cache = new { },
                    timings = new { send = 0, wait = 0, receive = 0 }
                })
            }
        };

        var json = JsonSerializer.Serialize(har, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ExportToInsomniaAsync(Project project, string filePath)
    {
        var resources = new List<object>
        {
            new
            {
                _type = "workspace",
                _id = Guid.NewGuid().ToString(),
                id = Guid.NewGuid().ToString(),
                name = project.Name,
                scope = "collection"
            }
        };

        foreach (var api in project.Apis)
        {
            resources.Add(new
            {
                _type = "request",
                _id = Guid.NewGuid().ToString(),
                id = api.Id,
                name = api.Name,
                method = api.Method.ToString(),
                url = api.Url,
                headers = api.Headers.Select(h => new { name = h.Key, value = h.Value, enabled = h.IsEnabled }),
                body = !string.IsNullOrEmpty(api.Body) ? new
                {
                    mimeType = api.BodyType switch
                    {
                        BodyType.Json => "application/json",
                        BodyType.Xml => "application/xml",
                        BodyType.FormUrlEncoded => "application/x-www-form-urlencoded",
                        _ => "text/plain"
                    },
                    text = api.Body
                } : null
            });
        }

        var export = new { resources };
        var json = JsonSerializer.Serialize(export, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task ExportToSwaggerAsync(Project project, string filePath)
    {
        var swagger = new
        {
            openapi = "3.0.0",
            info = new
            {
                title = project.Name,
                version = "1.0.0"
            },
            paths = project.Apis.ToDictionary(
                api => api.Url.Replace("{{baseUrl}}", "").Split('?')[0],
                api => new Dictionary<string, object>
                {
                    [api.Method.ToString().ToLower()] = new
                    {
                        summary = api.Name,
                        responses = new Dictionary<string, object>
                        {
                            ["200"] = new { description = "Successful response" }
                        }
                    }
                })
        };

        var json = JsonSerializer.Serialize(swagger, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
}
