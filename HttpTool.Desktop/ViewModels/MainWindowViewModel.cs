using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;
using Microsoft.Win32;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 打开的项目标签页
/// </summary>
public partial class ProjectTabItem : ObservableObject
{
    [ObservableProperty]
    private string _title = "New Project";

    [ObservableProperty]
    private Project? _project;

    [ObservableProperty]
    private ObservableCollection<ApiRequest> _apis = new();

    [ObservableProperty]
    private ApiRequest? _selectedApi;

    [ObservableProperty]
    private ApiResponse? _currentResponse;

    [ObservableProperty]
    private bool _isLoading;

    public string ProjectId => Project?.Id ?? string.Empty;
}

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IProjectService _projectService;
    private readonly IHttpRequestService _httpRequestService;
    private readonly IHistoryService _historyService;
    private readonly IFormatterService _formatterService;

    [ObservableProperty]
    private ObservableCollection<ProjectTabItem> _projectTabs = new();

    [ObservableProperty]
    private ProjectTabItem? _selectedTab;

    [ObservableProperty]
    private ObservableCollection<RequestHistory> _history = new();

    [ObservableProperty]
    private bool _isLoading;

    public MainWindowViewModel(
        IProjectService projectService,
        IHttpRequestService httpRequestService,
        IHistoryService historyService,
        IFormatterService formatterService)
    {
        _projectService = projectService;
        _httpRequestService = httpRequestService;
        _historyService = historyService;
        _formatterService = formatterService;

        _projectService.ProjectChanged += OnProjectChanged;
        _historyService.HistoryAdded += OnHistoryAdded;
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
    private void NewProject()
    {
        var project = _projectService.CreateProject();
        var tab = new ProjectTabItem
        {
            Title = project.Name,
            Project = project,
            Apis = new ObservableCollection<ApiRequest>(project.Apis)
        };
        ProjectTabs.Add(tab);
        SelectedTab = tab;
    }

    [RelayCommand]
    private async Task OpenProjectAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "HttpTool Project (*.httptool)|*.httptool|All Files (*.*)|*.*",
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
            var tab = new ProjectTabItem
            {
                Title = project.Name,
                Project = project,
                Apis = new ObservableCollection<ApiRequest>(project.Apis)
            };

            // 检查是否已打开
            var existingTab = ProjectTabs.FirstOrDefault(t => t.ProjectId == project.Id);
            if (existingTab != null)
            {
                SelectedTab = existingTab;
            }
            else
            {
                ProjectTabs.Add(tab);
                SelectedTab = tab;
            }
        }
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
        await _projectService.SaveCurrentProjectAsync();
    }

    [RelayCommand]
    private async Task SaveProjectAsAsync()
    {
        if (SelectedTab?.Project == null)
            return;

        var dialog = new SaveFileDialog
        {
            Filter = "HttpTool Project (*.httptool)|*.httptool",
            Title = "Save Project As",
            FileName = SelectedTab.Project.Name + ".httptool"
        };

        if (dialog.ShowDialog() == true)
        {
            SelectedTab.Project.Apis = SelectedTab.Apis.ToList();
            await _projectService.SaveProjectAsAsync(dialog.FileName);
            SelectedTab.Title = SelectedTab.Project.Name;
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
        if (SelectedTab == null)
            return;

        var api = ApiRequest.CreateDefault();
        SelectedTab.Apis.Add(api);
        SelectedTab.SelectedApi = api;
    }

    [RelayCommand]
    private void DeleteApi(ApiRequest? api)
    {
        if (SelectedTab == null || api == null)
            return;

        SelectedTab.Apis.Remove(api);
        if (SelectedTab.SelectedApi == api)
        {
            SelectedTab.SelectedApi = SelectedTab.Apis.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task SendRequestAsync(ApiRequest? api)
    {
        if (api == null || SelectedTab == null)
            return;

        SelectedTab.IsLoading = true;
        SelectedTab.CurrentResponse = null;

        try
        {
            var variables = SelectedTab.Project?.Environment?.Variables
                .Where(v => v.IsEnabled)
                .ToDictionary(v => v.Key, v => v.Value);

            var response = await _httpRequestService.SendRequestAsync(api, variables);

            // 格式化响应内容
            response.Body = _formatterService.Format(response.Body, response.ContentType);

            SelectedTab.CurrentResponse = response;

            // 添加到历史
            await _historyService.AddHistoryAsync(new RequestHistory
            {
                ProjectId = SelectedTab.Project?.Id ?? string.Empty,
                ApiName = api.Name,
                Method = api.Method,
                Url = api.Url,
                StatusCode = response.StatusCode,
                Elapsed = response.Elapsed,
                IsSuccess = response.IsSuccess
            });
        }
        finally
        {
            SelectedTab.IsLoading = false;
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
