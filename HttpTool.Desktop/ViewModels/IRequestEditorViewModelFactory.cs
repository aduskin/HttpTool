using HttpTool.Core.Models;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// RequestEditorViewModel 工厂接口
/// </summary>
public interface IRequestEditorViewModelFactory
{
    RequestEditorViewModel Create(Project project);
}
