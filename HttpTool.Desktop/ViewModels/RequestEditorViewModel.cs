using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;
using System.Collections.ObjectModel;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 请求编辑器 ViewModel
/// </summary>
public partial class RequestEditorViewModel : ObservableObject
{
    private readonly IHttpRequestService _httpRequestService;
    private readonly IFormatterService _formatterService;
    private readonly IHistoryService _historyService;

    [ObservableProperty]
    private Project _project;

    [ObservableProperty]
    private ObservableCollection<ApiRequest> _apis = new();

    [ObservableProperty]
    private ApiRequest? _selectedApi;

    [ObservableProperty]
    private KeyValueItem? _selectedParam;

    [ObservableProperty]
    private KeyValueItem? _selectedHeader;

    [ObservableProperty]
    private ApiResponse? _currentResponse;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _progressState = "Normal";

    [ObservableProperty]
    private double _currentProgress;

    public RequestEditorViewModel(
        IHttpRequestService httpRequestService,
        IFormatterService formatterService,
        IHistoryService historyService)
    {
        _httpRequestService = httpRequestService;
        _formatterService = formatterService;
        _historyService = historyService;
    }

    public RequestEditorViewModel(
        IHttpRequestService httpRequestService,
        IFormatterService formatterService,
        IHistoryService historyService,
        Project project) : this(httpRequestService, formatterService, historyService)
    {
        Project = project;
        Apis = new ObservableCollection<ApiRequest>(project.Apis);
        if (Apis.Count > 0)
        {
            SelectedApi = Apis[0];
        }
    }

    [RelayCommand]
    private void NewApi()
    {
        var api = ApiRequest.CreateDefault();
        Apis.Add(api);
        SelectedApi = api;
    }

    [RelayCommand]
    private void DeleteApi(ApiRequest? api)
    {
        if (api == null)
            return;

        Apis.Remove(api);
        if (SelectedApi == api)
        {
            SelectedApi = Apis.FirstOrDefault();
        }
    }

    [RelayCommand]
    private async Task SendRequestAsync(ApiRequest? api)
    {
        if (api == null)
            return;

        IsLoading = true;
        CurrentResponse = null;
        ProgressState = "Wait";  // 开始请求时显示等待状态
        CurrentProgress = 0;

        try
        {
            var variables = Project?.Environment?.Variables
                .Where(v => v.IsEnabled)
                .ToDictionary(v => v.Key, v => v.Value);

            var response = await _httpRequestService.SendRequestAsync(api, variables);

            // 格式化响应内容
            response.Body = _formatterService.Format(response.Body, response.ContentType);

            CurrentResponse = response;

            // 根据响应结果设置进度条状态
            ProgressState = response.IsSuccess ? "Success" : "Error";
            CurrentProgress = 100;

            // 添加到历史
            await _historyService.AddHistoryAsync(new RequestHistory
            {
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
            IsLoading = false;
        }
    }

    public void AddParam()
    {
        SelectedApi?.QueryParams.Add(new KeyValueItem());
    }

    public void DeleteParam()
    {
        if (SelectedParam != null)
        {
            SelectedApi?.QueryParams.Remove(SelectedParam);
            SelectedParam = null;
        }
    }

    public void AddHeader()
    {
        SelectedApi?.Headers.Add(new KeyValueItem());
    }

    public void DeleteHeader()
    {
        if (SelectedHeader != null)
        {
            SelectedApi?.Headers.Remove(SelectedHeader);
            SelectedHeader = null;
        }
    }
}
