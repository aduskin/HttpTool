namespace HttpTool.Core.Interfaces;

/// <summary>
/// WebSocket 消息事件参数
/// </summary>
public class WebSocketMessageEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public WebSocketMessageType MessageType { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// WebSocket 连接状态事件参数
/// </summary>
public class WebSocketStateEventArgs : EventArgs
{
    public WebSocketConnectionState State { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// WebSocket 消息类型
/// </summary>
public enum WebSocketMessageType
{
    Text,
    Binary,
    Close,
    Ping,
    Pong
}

/// <summary>
/// WebSocket 连接状态
/// </summary>
public enum WebSocketConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Disconnecting,
    Error
}

/// <summary>
/// WebSocket 服务接口
/// </summary>
public interface IWebSocketService : IDisposable
{
    /// <summary>
    /// 连接状态
    /// </summary>
    WebSocketConnectionState State { get; }

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    event EventHandler<WebSocketStateEventArgs>? StateChanged;

    /// <summary>
    /// 收到消息事件
    /// </summary>
    event EventHandler<WebSocketMessageEventArgs>? MessageReceived;

    /// <summary>
    /// 连接
    /// </summary>
    Task ConnectAsync(string url, Dictionary<string, string>? headers = null);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 发送消息
    /// </summary>
    Task SendAsync(string message);

    /// <summary>
    /// 发送二进制消息
    /// </summary>
    Task SendBinaryAsync(byte[] data);
}
