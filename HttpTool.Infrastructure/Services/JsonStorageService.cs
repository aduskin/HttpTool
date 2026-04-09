using System.Text.Json;
using System.Text.Json.Serialization;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// JSON 文件存储服务实现
/// </summary>
public class JsonStorageService : IStorageService
{
    private readonly string _appDataPath;
    private readonly string _recentProjectsFile;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService()
    {
        _appDataPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "HttpTool");

        _recentProjectsFile = Path.Combine(_appDataPath, "recent.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        EnsureDirectoryExists();
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_appDataPath))
        {
            Directory.CreateDirectory(_appDataPath);
        }
    }

    public string GetAppDataPath() => _appDataPath;

    public async Task SaveProjectAsync(Project project)
    {
        var directory = Path.GetDirectoryName(project.FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(project, _jsonOptions);
        await File.WriteAllTextAsync(project.FilePath, json);
    }

    public async Task<Project?> LoadProjectAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var project = JsonSerializer.Deserialize<Project>(json, _jsonOptions);

        if (project != null)
        {
            project.FilePath = filePath;
        }

        return project;
    }

    public async Task<List<string>> GetRecentProjectsAsync()
    {
        if (!File.Exists(_recentProjectsFile))
        {
            return new List<string>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_recentProjectsFile);
            return JsonSerializer.Deserialize<List<string>>(json, _jsonOptions) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task AddToRecentProjectsAsync(string filePath)
    {
        var recentProjects = await GetRecentProjectsAsync();

        // 移除已存在的路径
        recentProjects.RemoveAll(p => p.Equals(filePath, StringComparison.OrdinalIgnoreCase));

        // 添加到列表开头
        recentProjects.Insert(0, filePath);

        // 限制数量
        if (recentProjects.Count > 10)
        {
            recentProjects = recentProjects.Take(10).ToList();
        }

        var json = JsonSerializer.Serialize(recentProjects, _jsonOptions);
        await File.WriteAllTextAsync(_recentProjectsFile, json);
    }

    public async Task RemoveFromRecentProjectsAsync(string filePath)
    {
        var recentProjects = await GetRecentProjectsAsync();
        recentProjects.RemoveAll(p => p.Equals(filePath, StringComparison.OrdinalIgnoreCase));
        var json = JsonSerializer.Serialize(recentProjects, _jsonOptions);
        await File.WriteAllTextAsync(_recentProjectsFile, json);
    }
}
