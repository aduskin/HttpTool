using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// 项目服务实现
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IStorageService _storageService;
    private Project? _currentProject;

    public Project? CurrentProject => _currentProject;

    public event EventHandler<Project?>? ProjectChanged;

    public ProjectService(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public Project CreateProject()
    {
        var project = Project.CreateDefault();
        _currentProject = project;
        ProjectChanged?.Invoke(this, _currentProject);
        return project;
    }

    public async Task<Project?> OpenProjectAsync(string filePath)
    {
        var project = await _storageService.LoadProjectAsync(filePath);
        if (project != null)
        {
            _currentProject = project;
            await _storageService.AddToRecentProjectsAsync(filePath);
            ProjectChanged?.Invoke(this, _currentProject);
        }
        return project;
    }

    public async Task SaveProjectAsync(Project? project)
    {
        if (project == null)
            return;

        if (string.IsNullOrEmpty(project.FilePath))
        {
            throw new InvalidOperationException("Project file path is not set. Use SaveProjectAsAsync instead.");
        }

        project.UpdatedAt = DateTime.Now;
        await _storageService.SaveProjectAsync(project);
    }

    public async Task SaveProjectAsAsync(string filePath)
    {
        if (_currentProject == null)
        {
            return;
        }

        _currentProject.FilePath = filePath;
        _currentProject.UpdatedAt = DateTime.Now;
        await _storageService.SaveProjectAsync(_currentProject);
        await _storageService.AddToRecentProjectsAsync(filePath);
    }

    public void CloseProject()
    {
        _currentProject = null;
        ProjectChanged?.Invoke(this, _currentProject);
    }

    public Task<List<string>> GetRecentProjectsAsync()
    {
        return _storageService.GetRecentProjectsAsync();
    }
}
