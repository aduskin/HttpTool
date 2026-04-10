using HttpTool.Core.Enums;

namespace HttpTool.Desktop.Views;

/// <summary>
/// RequestEditorView.xaml 的交互逻辑
/// </summary>
public partial class RequestEditorView
{
    public RequestEditorView()
    {
        InitializeComponent();
    }
}

/// <summary>
/// HTTP 方法集合（用于 XAML 绑定）
/// </summary>
public static class HttpMethods
{
    public static HttpMethodType[] Values => Enum.GetValues<HttpMethodType>();
}
