using System.Text;
using System.Text.Json;
using HttpTool.Core.Enums;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// 代码生成服务实现
/// </summary>
public class CodeGeneratorService : ICodeGeneratorService
{
    private readonly IHttpRequestService _httpRequestService;

    public IReadOnlyList<CodeLanguage> SupportedLanguages => CodeLanguage.All;

    public CodeGeneratorService(IHttpRequestService httpRequestService)
    {
        _httpRequestService = httpRequestService;
    }

    public string GenerateCode(ApiRequest request, CodeLanguage language, Dictionary<string, string>? variables = null)
    {
        return language.Id switch
        {
            "csharp" => GenerateCSharp(request, variables),
            "python" => GeneratePython(request, variables),
            "javascript" => GenerateJavaScript(request, variables),
            "java" => GenerateJava(request, variables),
            "go" => GenerateGo(request, variables),
            "php" => GeneratePhp(request, variables),
            "ruby" => GenerateRuby(request, variables),
            "curl" => GenerateCurl(request, variables),
            _ => throw new NotSupportedException($"Language '{language.Name}' is not supported.")
        };
    }

    private string ReplaceVariables(string input, Dictionary<string, string>? variables)
    {
        return _httpRequestService.ReplaceVariables(input, variables ?? new Dictionary<string, string>());
    }

    private string BuildUrl(ApiRequest request, Dictionary<string, string>? variables)
    {
        var url = ReplaceVariables(request.Url, variables);

        // Query params
        if (request.QueryParams.Count > 0)
        {
            var queryParts = request.QueryParams
                .Where(p => p.IsEnabled && !string.IsNullOrEmpty(p.Key))
                .Select(p =>
                {
                    var key = ReplaceVariables(p.Key, variables);
                    var value = ReplaceVariables(p.Value, variables);
                    return $"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}";
                });
            var queryString = string.Join("&", queryParts);
            if (!string.IsNullOrEmpty(queryString))
            {
                url += url.Contains('?') ? $"&{queryString}" : $"?{queryString}";
            }
        }

        return url;
    }

    private Dictionary<string, string> BuildHeaders(ApiRequest request, Dictionary<string, string>? variables)
    {
        var headers = new Dictionary<string, string>();

        foreach (var header in request.Headers.Where(h => h.IsEnabled && !string.IsNullOrEmpty(h.Key)))
        {
            var key = ReplaceVariables(header.Key, variables);
            var value = ReplaceVariables(header.Value, variables);
            headers[key] = value;
        }

        // Auth
        switch (request.AuthType)
        {
            case AuthType.Basic:
                var basicValue = ReplaceVariables(request.AuthValue, variables);
                headers["Authorization"] = $"Basic {basicValue}";
                break;
            case AuthType.Bearer:
                var bearerValue = ReplaceVariables(request.AuthValue, variables);
                headers["Authorization"] = $"Bearer {bearerValue}";
                break;
            case AuthType.ApiKey:
                var apiKeyValue = ReplaceVariables(request.AuthValue, variables);
                headers["X-API-Key"] = apiKeyValue;
                break;
        }

        return headers;
    }

    private string GenerateCSharp(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToUpper();

        sb.AppendLine("using System.Net.Http;");
        sb.AppendLine("using System.Text;");
        sb.AppendLine();
        sb.AppendLine("var client = new HttpClient();");
        sb.AppendLine();

        // Headers
        foreach (var header in headers)
        {
            sb.AppendLine($"client.DefaultRequestHeaders.Add(\"{header.Key}\", \"{header.Value}\");");
        }

        sb.AppendLine();
        sb.AppendLine($"var request = new HttpRequestMessage(HttpMethod.{method}, \"{url}\");");

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None && method != "GET")
        {
            var body = ReplaceVariables(request.Body, variables);
            var contentType = GetContentType(request.BodyType);

            if (request.BodyType == BodyType.Json)
            {
                sb.AppendLine();
                sb.AppendLine("var jsonBody = @\"");
                sb.AppendLine(body);
                sb.AppendLine("\";");
                sb.AppendLine();
                sb.AppendLine("request.Content = new StringContent(jsonBody, Encoding.UTF8, \"application/json\");");
            }
            else
            {
                sb.AppendLine($"request.Content = new StringContent(@\"{body}\", Encoding.UTF8, \"{contentType}\");");
            }
        }

        sb.AppendLine();
        sb.AppendLine("var response = await client.SendAsync(request);");
        sb.AppendLine("var content = await response.Content.ReadAsStringAsync();");
        sb.AppendLine();
        sb.AppendLine("Console.WriteLine($\"Status: {(int)response.StatusCode} {response.StatusCode}\");");
        sb.AppendLine("Console.WriteLine($\"Response: {content}\");");

        return sb.ToString();
    }

    private string GeneratePython(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToLower();

        sb.AppendLine("import requests");
        sb.AppendLine();

        // Headers
        if (headers.Count > 0)
        {
            sb.AppendLine("headers = {");
            foreach (var header in headers)
            {
                sb.AppendLine($"    \"{header.Key}\": \"{header.Value}\",");
            }
            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            var body = ReplaceVariables(request.Body, variables);

            if (request.BodyType == BodyType.Json)
            {
                sb.AppendLine("data = '''");
                sb.AppendLine(body);
                sb.AppendLine("'''");
                sb.AppendLine();

                if (headers.Count > 0)
                {
                    sb.AppendLine($"response = requests.{method}(\"{url}\", headers=headers, data=data)");
                }
                else
                {
                    sb.AppendLine($"response = requests.{method}(\"{url}\", data=data)");
                }
            }
            else
            {
                if (headers.Count > 0)
                {
                    sb.AppendLine($"response = requests.{method}(\"{url}\", headers=headers)");
                }
                else
                {
                    sb.AppendLine($"response = requests.{method}(\"{url}\")");
                }
            }
        }
        else
        {
            if (headers.Count > 0)
            {
                sb.AppendLine($"response = requests.{method}(\"{url}\", headers=headers)");
            }
            else
            {
                sb.AppendLine($"response = requests.{method}(\"{url}\")");
            }
        }

        sb.AppendLine();
        sb.AppendLine("print(f\"Status: {response.status_code}\")");
        sb.AppendLine("print(f\"Response: {response.text}\")");

        return sb.ToString();
    }

    private string GenerateJavaScript(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToUpper();

        sb.AppendLine("const response = await fetch(");
        sb.AppendLine($"    \"{url}\",");
        sb.AppendLine("    {");
        sb.AppendLine($"        method: \"{method}\",");

        // Headers
        if (headers.Count > 0)
        {
            sb.AppendLine("        headers: {");
            foreach (var header in headers)
            {
                sb.AppendLine($"            \"{header.Key}\": \"{header.Value}\",");
            }
            sb.AppendLine("        },");
        }

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None && method != "GET")
        {
            var body = ReplaceVariables(request.Body, variables);

            if (request.BodyType == BodyType.Json)
            {
                sb.AppendLine("        body: JSON.stringify({");
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    foreach (var prop in doc.RootElement.EnumerateObject())
                    {
                        var value = prop.Value.ValueKind == JsonValueKind.String
                            ? $"\"{prop.Value.GetString()}\""
                            : prop.Value.ToString();
                        sb.AppendLine($"            {prop.Name}: {value},");
                    }
                }
                catch
                {
                    sb.AppendLine($"            // {body}");
                }
                sb.AppendLine("        }),");
            }
            else
            {
                sb.AppendLine($"        body: `{body}`,");
            }
        }

        sb.AppendLine("    }");
        sb.AppendLine(");");
        sb.AppendLine();
        sb.AppendLine("console.log(`Status: ${response.status}`);");
        sb.AppendLine("const data = await response.json();");
        sb.AppendLine("console.log(`Response: ${JSON.stringify(data, null, 2)}`);");

        return sb.ToString();
    }

    private string GenerateJava(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToUpper();

        sb.AppendLine("import java.net.http.*;");
        sb.AppendLine("import java.net.URI;");
        sb.AppendLine();
        sb.AppendLine("public class ApiRequest {");
        sb.AppendLine("    public static void main(String[] args) throws Exception {");
        sb.AppendLine("        HttpClient client = HttpClient.newHttpClient();");
        sb.AppendLine();

        // Build request
        sb.AppendLine($"        HttpRequest request = HttpRequest.newBuilder()");
        sb.AppendLine($"            .uri(URI.create(\"{url}\"))");
        sb.AppendLine($"            .method(\"{method}\", HttpRequest.BodyPublishers.noBody())");

        foreach (var header in headers)
        {
            sb.AppendLine($"            .header(\"{header.Key}\", \"{header.Value}\")");
        }

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None && method != "GET")
        {
            var body = ReplaceVariables(request.Body, variables);
            sb.AppendLine($"            .POST(HttpRequest.BodyPublishers.ofString(\"{body}\"))");
        }

        sb.AppendLine("            .build();");
        sb.AppendLine();
        sb.AppendLine("        HttpResponse<String> response = client.send(request, HttpResponse.BodyHandlers.ofString());");
        sb.AppendLine();
        sb.AppendLine("        System.out.println(\"Status: \" + response.statusCode());");
        sb.AppendLine("        System.out.println(\"Response: \" + response.body());");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GenerateGo(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToUpper();

        sb.AppendLine("package main");
        sb.AppendLine();
        sb.AppendLine("import (");
        sb.AppendLine("    \"fmt\"");
        sb.AppendLine("    \"io\"");
        sb.AppendLine("    \"net/http\"");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.AppendLine("func main() {");

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None && method != "GET")
        {
            var body = ReplaceVariables(request.Body, variables);
            sb.AppendLine($"    body := `{body}`");
            sb.AppendLine($"    reader := strings.NewReader(body)");
        }
        else
        {
            sb.AppendLine("    var reader io.Reader = nil");
        }

        sb.AppendLine();
        sb.AppendLine($"    req, err := http.NewRequest(\"{method}\", \"{url}\", reader)");
        sb.AppendLine("    if err != nil {");
        sb.AppendLine("        panic(err)");
        sb.AppendLine("    }");

        foreach (var header in headers)
        {
            sb.AppendLine($"    req.Header.Set(\"{header.Key}\", \"{header.Value}\")");
        }

        sb.AppendLine();
        sb.AppendLine("    client := &http.Client{}");
        sb.AppendLine("    resp, err := client.Do(req)");
        sb.AppendLine("    if err != nil {");
        sb.AppendLine("        panic(err)");
        sb.AppendLine("    }");
        sb.AppendLine("    defer resp.Body.Close()");
        sb.AppendLine();
        sb.AppendLine("    fmt.Printf(\"Status: %d\\n\", resp.StatusCode)");
        sb.AppendLine("    body, _ := io.ReadAll(resp.Body)");
        sb.AppendLine("    fmt.Printf(\"Response: %s\\n\", body)");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private string GeneratePhp(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToUpper();

        sb.AppendLine("<?php");
        sb.AppendLine();
        sb.AppendLine("$curl = curl_init();");
        sb.AppendLine();

        // Headers
        var headerLines = headers.Select(h => $"    \"{h.Key}: {h.Value}\"");
        if (headerLines.Any())
        {
            sb.AppendLine("$headers = [");
            foreach (var line in headerLines)
            {
                sb.AppendLine(line + ",");
            }
            sb.AppendLine("];");
            sb.AppendLine();
        }

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            var body = ReplaceVariables(request.Body, variables);
            sb.AppendLine($"$data = '{body}';");
            sb.AppendLine();
        }

        sb.AppendLine("curl_setopt_array($curl, [");
        sb.AppendLine($"    CURLOPT_URL => \"{url}\",");
        sb.AppendLine($"    CURLOPT_RETURNTRANSFER => true,");
        sb.AppendLine($"    CURLOPT_CUSTOMREQUEST => \"{method}\",");
        if (headerLines.Any())
        {
            sb.AppendLine("    CURLOPT_HTTPHEADER => $headers,");
        }
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            sb.AppendLine("    CURLOPT_POSTFIELDS => $data,");
        }
        sb.AppendLine("]);");
        sb.AppendLine();
        sb.AppendLine("$response = curl_exec($curl);");
        sb.AppendLine("$httpCode = curl_getinfo($curl, CURLINFO_HTTP_CODE);");
        sb.AppendLine();
        sb.AppendLine("curl_close($curl);");
        sb.AppendLine();
        sb.AppendLine("echo \"Status: \" . $httpCode . \"\\n\";");
        sb.AppendLine("echo \"Response: \" . $response . \"\\n\";");
        sb.AppendLine("?>");

        return sb.ToString();
    }

    private string GenerateRuby(ApiRequest request, Dictionary<string, string>? variables)
    {
        var sb = new StringBuilder();
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToLower();

        sb.AppendLine("require 'net/http'");
        sb.AppendLine("require 'uri'");
        sb.AppendLine("require 'json'");
        sb.AppendLine();
        sb.AppendLine($"uri = URI.parse(\"{url}\")");
        sb.AppendLine();
        sb.AppendLine("http = Net::HTTP.new(uri.host, uri.port)");
        sb.AppendLine("http.use_ssl = uri.scheme == 'https'");
        sb.AppendLine();

        // Body
        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            var body = ReplaceVariables(request.Body, variables);
            sb.AppendLine($"data = <<~JSON");
            sb.AppendLine(body);
            sb.AppendLine("JSON");
            sb.AppendLine();
        }

        sb.AppendLine($"request = Net::HTTP::{Capitalize(method)}.new(uri)");
        sb.AppendLine();

        foreach (var header in headers)
        {
            sb.AppendLine($"request[\"{header.Key}\"] = \"{header.Value}\"");
        }

        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            sb.AppendLine("request.body = data");
        }

        sb.AppendLine();
        sb.AppendLine("response = http.request(request)");
        sb.AppendLine();
        sb.AppendLine("puts \"Status: #{response.code}\"");
        sb.AppendLine("puts \"Response: #{response.body}\"");

        return sb.ToString();
    }

    private string GenerateCurl(ApiRequest request, Dictionary<string, string>? variables)
    {
        var url = BuildUrl(request, variables);
        var headers = BuildHeaders(request, variables);
        var method = request.Method.ToString().ToUpper();

        var sb = new StringBuilder();
        sb.AppendLine($"curl -X {method} \\");
        sb.AppendLine($"  \"{url}\" \\");

        foreach (var header in headers)
        {
            sb.AppendLine($"  -H \"{header.Key}: {header.Value}\" \\");
        }

        if (!string.IsNullOrEmpty(request.Body) && request.BodyType != BodyType.None)
        {
            var body = ReplaceVariables(request.Body, variables);
            var contentType = GetContentType(request.BodyType);
            sb.AppendLine($"  -H \"Content-Type: {contentType}\" \\");
            sb.AppendLine($"  -d '{body}'");
        }

        return sb.ToString().TrimEnd(' ', '\\');
    }

    private string GetContentType(BodyType bodyType)
    {
        return bodyType switch
        {
            BodyType.Json => "application/json",
            BodyType.Xml => "application/xml",
            BodyType.FormUrlEncoded => "application/x-www-form-urlencoded",
            BodyType.FormData => "multipart/form-data",
            BodyType.Raw => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private string Capitalize(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return char.ToUpper(str[0]) + str[1..].ToLower();
    }
}
