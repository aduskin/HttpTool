using System.Net.WebSockets;
using System.Text;
using HttpTool.Core.Interfaces;

namespace HttpTool.Infrastructure.Services;

/// <summary>
/// WebSocket 服务实现
/// </summary>
public class WebSocketService : IWebSocketService, IDisposable
{
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _lock = new();

    public WebSocketConnectionState State { get; private set; } = WebSocketConnectionState.Disconnected;

    public event EventHandler<WebSocketStateEventArgs>? StateChanged;
    public event EventHandler<WebSocketMessageEventArgs>? MessageReceived;

    public async Task ConnectAsync(string url, Dictionary<string, string>? headers = null)
    {
        if (State == WebSocketConnectionState.Connected || State == WebSocketConnectionState.Connecting)
        {
            return;
        }

        try
        {
            UpdateState(WebSocketConnectionState.Connecting);

            _webSocket = new ClientWebSocket();

            // 添加 headers
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    _webSocket.Options.SetRequestHeader(header.Key, header.Value);
                }
            }

            _cancellationTokenSource = new CancellationTokenSource();

            await _webSocket.ConnectAsync(new Uri(url), _cancellationTokenSource.Token);

            UpdateState(WebSocketConnectionState.Connected);

            // 开始接收消息
            _ = ReceiveLoopAsync();
        }
        catch (Exception ex)
        {
            UpdateState(WebSocketConnectionState.Error, ex.Message);
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        if (State != WebSocketConnectionState.Connected && State != WebSocketConnectionState.Connecting)
        {
            return;
        }

        try
        {
            UpdateState(WebSocketConnectionState.Disconnecting);

            _cancellationTokenSource?.Cancel();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            }

            UpdateState(WebSocketConnectionState.Disconnected);
        }
        catch (Exception ex)
        {
            UpdateState(WebSocketConnectionState.Error, ex.Message);
            throw;
        }
    }

    public async Task SendAsync(string message)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }

        var bytes = Encoding.UTF8.GetBytes(message);
        await _webSocket.SendAsync(new ArraySegment<byte>(bytes), System.Net.WebSockets.WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public async Task SendBinaryAsync(byte[] data)
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected.");
        }

        await _webSocket.SendAsync(new ArraySegment<byte>(data), System.Net.WebSockets.WebSocketMessageType.Binary, true, CancellationToken.None);
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = new byte[8192];

        try
        {
            while (_webSocket?.State == WebSocketState.Open && _cancellationTokenSource != null)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                {
                    UpdateState(WebSocketConnectionState.Disconnected);
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var messageType = result.MessageType switch
                {
                    System.Net.WebSockets.WebSocketMessageType.Text => Core.Interfaces.WebSocketMessageType.Text,
                    System.Net.WebSockets.WebSocketMessageType.Binary => Core.Interfaces.WebSocketMessageType.Binary,
                    System.Net.WebSockets.WebSocketMessageType.Close => Core.Interfaces.WebSocketMessageType.Close,
                    _ => Core.Interfaces.WebSocketMessageType.Text
                };

                MessageReceived?.Invoke(this, new WebSocketMessageEventArgs
                {
                    Message = message,
                    MessageType = messageType,
                    Timestamp = DateTime.Now
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when disconnecting
        }
        catch (Exception ex)
        {
            UpdateState(WebSocketConnectionState.Error, ex.Message);
        }
    }

    private void UpdateState(WebSocketConnectionState state, string? error = null)
    {
        State = state;
        StateChanged?.Invoke(this, new WebSocketStateEventArgs
        {
            State = state,
            Error = error
        });
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _webSocket?.Dispose();
    }
}
