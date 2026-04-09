using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// RequestEditorViewModel 工厂实现
/// </summary>
public class RequestEditorViewModelFactory : IRequestEditorViewModelFactory
{
    private readonly IHttpRequestService _httpRequestService;
    private readonly IFormatterService _formatterService;
    private readonly IHistoryService _historyService;

    public RequestEditorViewModelFactory(
        IHttpRequestService httpRequestService,
        IFormatterService formatterService,
        IHistoryService historyService)
    {
        _httpRequestService = httpRequestService;
        _formatterService = formatterService;
        _historyService = historyService;
    }

    public RequestEditorViewModel Create(Project project)
    {
        return new RequestEditorViewModel(_httpRequestService, _formatterService, _historyService, project);
    }
}
