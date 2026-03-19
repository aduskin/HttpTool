using System.Text.Json;
using System.Text.Json.Serialization;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// 历史记录服务实现
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly IStorageService _storageService;
    private readonly string _historyFile;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly List<RequestHistory> _histories = new();

    public event EventHandler<RequestHistory>? HistoryAdded;

    public HistoryService(IStorageService storageService)
    {
        _storageService = storageService;
        _historyFile = Path.Combine(_storageService.GetAppDataPath(), "history.json");

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        LoadHistorySync();
    }

    private void LoadHistorySync()
    {
        if (!File.Exists(_historyFile))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(_historyFile);
            var history = JsonSerializer.Deserialize<List<RequestHistory>>(json, _jsonOptions);
            if (history != null)
            {
                _histories.Clear();
                _histories.AddRange(history);
            }
        }
        catch
        {
            // 忽略加载错误
        }
    }

    private async Task SaveHistoryAsync()
    {
        var json = JsonSerializer.Serialize(_histories, _jsonOptions);
        await File.WriteAllTextAsync(_historyFile, json);
    }

    public async Task AddHistoryAsync(RequestHistory history)
    {
        _histories.Insert(0, history);

        // 限制历史记录数量
        if (_histories.Count > 500)
        {
            _histories.RemoveRange(500, _histories.Count - 500);
        }

        await SaveHistoryAsync();
        HistoryAdded?.Invoke(this, history);
    }

    public Task<List<RequestHistory>> GetHistoryAsync(string? projectId = null, int limit = 100)
    {
        var query = _histories.AsEnumerable();

        if (!string.IsNullOrEmpty(projectId))
        {
            query = query.Where(h => h.ProjectId == projectId);
        }

        return Task.FromResult(query.Take(limit).ToList());
    }

    public async Task ClearHistoryAsync(string? projectId = null)
    {
        if (string.IsNullOrEmpty(projectId))
        {
            _histories.Clear();
        }
        else
        {
            _histories.RemoveAll(h => h.ProjectId == projectId);
        }

        await SaveHistoryAsync();
    }
}
