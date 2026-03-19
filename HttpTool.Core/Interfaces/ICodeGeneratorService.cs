using HttpTool.Core.Enums;
using HttpTool.Core.Models;

namespace HttpTool.Core.Interfaces;

/// <summary>
/// 代码生成服务接口
/// </summary>
public interface ICodeGeneratorService
{
    /// <summary>
    /// 支持的编程语言
    /// </summary>
    IReadOnlyList<CodeLanguage> SupportedLanguages { get; }

    /// <summary>
    /// 生成代码
    /// </summary>
    string GenerateCode(ApiRequest request, CodeLanguage language, Dictionary<string, string>? variables = null);
}

/// <summary>
/// 编程语言
/// </summary>
public record CodeLanguage(string Id, string Name, string DisplayName)
{
    public static readonly CodeLanguage CSharp = new("csharp", "C#", "C# (HttpClient)");
    public static readonly CodeLanguage Python = new("python", "Python", "Python (requests)");
    public static readonly CodeLanguage JavaScript = new("javascript", "JavaScript", "JavaScript (fetch)");
    public static readonly CodeLanguage Java = new("java", "Java", "Java (HttpClient)");
    public static readonly CodeLanguage Go = new("go", "Go", "Go (net/http)");
    public static readonly CodeLanguage Php = new("php", "PHP", "PHP (cURL)");
    public static readonly CodeLanguage Ruby = new("ruby", "Ruby", "Ruby (Net::HTTP)");
    public static readonly CodeLanguage Curl = new("curl", "cURL", "cURL");

    public static IReadOnlyList<CodeLanguage> All => new[]
    {
        CSharp, Python, JavaScript, Java, Go, Php, Ruby, Curl
    };
}
