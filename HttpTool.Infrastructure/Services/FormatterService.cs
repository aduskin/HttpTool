using System.Text.Json;
using System.Xml;
using HttpTool.Core.Interfaces;

namespace HttpTool.Infrastructure.Services;

public class FormatterService : IFormatterService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public FormatterService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public string Format(string content, string contentType)
    {
        if (string.IsNullOrWhiteSpace(content))
            return content;

        try
        {
            if (IsJson(contentType) || LooksLikeJson(content))
            {
                return FormatJson(content);
            }

            if (IsXml(contentType) || LooksLikeXml(content))
            {
                return FormatXml(content);
            }
        }
        catch
        {
            // 格式化失败，返回原始内容
        }

        return content;
    }

    private bool IsJson(string contentType)
    {
        return contentType.Contains("json", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsXml(string contentType)
    {
        return contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
    }

    private bool LooksLikeJson(string content)
    {
        var trimmed = content.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[');
    }

    private bool LooksLikeXml(string content)
    {
        var trimmed = content.TrimStart();
        return trimmed.StartsWith('<');
    }

    private string FormatJson(string content)
    {
        using var doc = JsonDocument.Parse(content);
        return JsonSerializer.Serialize(doc.RootElement, _jsonOptions);
    }

    private string FormatXml(string content)
    {
        var doc = new XmlDocument();
        doc.LoadXml(content);

        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.Replace
        };

        using var stringWriter = new StringWriter();
        using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
        {
            doc.WriteTo(xmlWriter);
        }
        return stringWriter.ToString();
    }
}
