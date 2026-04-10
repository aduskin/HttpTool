using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Constants;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 打开的项目标签页（项目级别管理）
/// </summary>
public partial class ProjectTabItem : ObservableObject
{
    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private RequestEditorViewModel? _editorViewModel;

    [ObservableProperty]
    private ObservableCollection<ApiRequest> _apis = new();

    [ObservableProperty]
    private ApiRequest? _selectedApi;

    public string ProjectId => Project?.Id ?? string.Empty;
}

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IHistoryService _historyService;
    private readonly IStorageService _storageService;
    private readonly IRequestEditorViewModelFactory _editorViewModelFactory;

    [ObservableProperty]
    private ObservableCollection<ProjectTabItem> _projectTabs = new();

    [ObservableProperty]
    private ProjectTabItem? _selectedTab;

    [ObservableProperty]
    private ObservableCollection<RequestHistory> _history = new();

    [ObservableProperty]
    private ObservableCollection<RecentProjectItem> _recentProjects = new();

    [ObservableProperty]
    private bool _isLoading;

    public MainWindowViewModel(
        IProjectService projectService,
        IHistoryService historyService,
        IStorageService storageService,
        IRequestEditorViewModelFactory editorViewModelFactory)
    {
        _projectService = projectService;
        _historyService = historyService;
        _storageService = storageService;
        _editorViewModelFactory = editorViewModelFactory;

        _projectService.ProjectChanged += OnProjectChanged;
        _historyService.HistoryAdded += OnHistoryAdded;

        LoadRecentProjectsAsync();
    }

    public async void LoadRecentProjectsAsync()
    {
        var paths = await _storageService.GetRecentProjectsAsync();
        RecentProjects.Clear();
        foreach (var path in paths)
        {
            if (System.IO.File.Exists(path))
            {
                RecentProjects.Add(new RecentProjectItem { FilePath = path });
            }
        }
    }

    private void OnProjectChanged(object? sender, Project? project)
    {
        // 项目变更处理
    }

    private void OnHistoryAdded(object? sender, RequestHistory history)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            History.Insert(0, history);
            if (History.Count > 100)
            {
                History.RemoveAt(History.Count - 1);
            }
        });
    }

    [RelayCommand]
    private async Task NewProjectAsync()
    {
        var project = _projectService.CreateProject();
        var tab = CreateProjectTabItem(project);
        ProjectTabs.Add(tab);
        SelectedTab = tab;

        // 如果项目已保存，添加到最近项目
        if (!string.IsNullOrEmpty(project.FilePath))
        {
            await _storageService.AddToRecentProjectsAsync(project.FilePath);
            LoadRecentProjectsAsync();
        }
    }

    [RelayCommand]
    private async Task OpenProjectAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = $"HttpTool Project (*{ProjectConstant.ProjectExtension})|*{ProjectConstant.ProjectExtension}|All Files (*.*)|*.*",
            Title = "Open Project"
        };

        if (dialog.ShowDialog() == true)
        {
            await OpenProjectFileAsync(dialog.FileName);
        }
    }

    public async Task OpenProjectFileAsync(string filePath)
    {
        var project = await _projectService.OpenProjectAsync(filePath);
        if (project != null)
        {
            // 检查是否已打开
            var existingTab = ProjectTabs.FirstOrDefault(t => t.ProjectId == project.Id);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
            }
            else
            {
                var tab = CreateProjectTabItem(project);
                ProjectTabs.Add(tab);
                SelectedTab = tab;
            }

            // 添加到最近项目
            await _storageService.AddToRecentProjectsAsync(filePath);
            LoadRecentProjectsAsync();
        }
    }

    private ProjectTabItem CreateProjectTabItem(Project project)
    {
        var editorViewModel = _editorViewModelFactory.Create(project);
        return new ProjectTabItem
        {
            Project = project,
            Apis = editorViewModel.Apis,
            SelectedApi = editorViewModel.SelectedApi,
            EditorViewModel = editorViewModel
        };
    }

    [RelayCommand]
    private async Task OpenRecentProjectAsync(RecentProjectItem? recentItem)
    {
        if (recentItem == null || string.IsNullOrEmpty(recentItem.FilePath))
            return;

        if (!System.IO.File.Exists(recentItem.FilePath))
        {
            MessageBox.Show($"Project file not found: {recentItem.FilePath}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            // 移除不存在的项目
            RecentProjects.Remove(recentItem);
            return;
        }

        await OpenProjectFileAsync(recentItem.FilePath);
    }

    [RelayCommand]
    private async Task RemoveProjectAsync(RecentProjectItem? recent)
    {
        if (recent == null) return;
        await _storageService.RemoveFromRecentProjectsAsync(recent.FilePath);
        RecentProjects.Remove(recent);
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (SelectedTab?.Project == null)
            return;

        if (string.IsNullOrEmpty(SelectedTab.Project.FilePath))
        {
            await SaveProjectAsAsync();
            return;
        }

        SelectedTab.Project.Apis = SelectedTab.Apis.ToList();
        await _projectService.SaveProjectAsync(SelectedTab.Project);
    }

    [RelayCommand]
    private async Task SaveProjectAsAsync()
    {
        if (SelectedTab?.Project == null)
            return;

        var dialog = new SaveFileDialog
        {
            Filter = $"HttpTool Project (*{ProjectConstant.ProjectExtension})|*{ProjectConstant.ProjectExtension}",
            Title = "Save Project As",
            FileName = $"{SelectedTab.Project.Name}{ProjectConstant.ProjectExtension}"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedTab.Project.Apis = SelectedTab.Apis.ToList();
            await _projectService.SaveProjectAsAsync(dialog.FileName);

            // 添加到最近项目
            await _storageService.AddToRecentProjectsAsync(dialog.FileName);
            LoadRecentProjectsAsync();
        }
    }

    [RelayCommand]
    private void CloseTab(ProjectTabItem? tab)
    {
        if (tab == null)
            return;

        var index = ProjectTabs.IndexOf(tab);
        ProjectTabs.Remove(tab);

        if (ProjectTabs.Count > 0)
        {
            SelectedTab = ProjectTabs[Math.Max(0, index - 1)];
        }
    }

    [RelayCommand]
    private void NewApi()
    {
        SelectedTab?.EditorViewModel?.NewApiCommand.Execute(null);
    }

    [RelayCommand]
    private void DeleteApi(ApiRequest? api)
    {
        if (api != null)
        {
            SelectedTab?.EditorViewModel?.DeleteApiCommand.Execute(api);
        }
    }

    [RelayCommand]
    private async Task SendRequestAsync(ApiRequest? api)
    {
        if (SelectedTab?.EditorViewModel != null)
        {
            await SelectedTab.EditorViewModel.SendRequestCommand.ExecuteAsync(api);
        }
    }

    [RelayCommand]
    private async Task LoadHistoryAsync()
    {
        var historyList = await _historyService.GetHistoryAsync();
        History.Clear();
        foreach (var item in historyList)
        {
            History.Add(item);
        }
    }
}