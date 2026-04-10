using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// WebSocket 消息
/// </summary>
public partial class WebSocketMessage : ObservableObject
{
    [ObservableProperty]
    private string _timestamp = "";

    [ObservableProperty]
    private string _messageType = "";

    [ObservableProperty]
    private string _message = "";
}

/// <summary>
/// WebSocket 连接状态
/// </summary>
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}

/// <summary>
/// WebSocket 窗口 ViewModel
/// </summary>
public partial class WebSocketWindowViewModel : ObservableObject
{
    private readonly IWebSocketService _webSocketService;

    [ObservableProperty]
    private string _url = "ws://localhost:8080";

    [ObservableProperty]
    private string _messageText = "";

    [ObservableProperty]
    private ObservableCollection<WebSocketMessage> _messages = new();

    [ObservableProperty]
    private ConnectionState _connectionState = ConnectionState.Disconnected;

    [ObservableProperty]
    private bool _isConnectEnabled = true;

    [ObservableProperty]
    private bool _isDisconnectEnabled;

    [ObservableProperty]
    private bool _isUrlEnabled = true;

    [ObservableProperty]
    private string _messagesText = "";

    public WebSocketWindowViewModel(IWebSocketService webSocketService)
    {
        _webSocketService = webSocketService;
        _webSocketService.StateChanged += OnStateChanged;
        _webSocketService.MessageReceived += OnMessageReceived;
    }

    private void OnStateChanged(object? sender, WebSocketStateEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            switch (e.State)
            {
                case WebSocketConnectionState.Connected:
                    ConnectionState = ConnectionState.Connected;
                    IsConnectEnabled = false;
                    IsDisconnectEnabled = true;
                    IsUrlEnabled = false;
                    AppendMessage($"Connected to {Url}");
                    break;

                case WebSocketConnectionState.Disconnected:
                    ConnectionState = ConnectionState.Disconnected;
                    IsConnectEnabled = true;
                    IsDisconnectEnabled = false;
                    IsUrlEnabled = true;
                    AppendMessage("Disconnected");
                    break;

                case WebSocketConnectionState.Connecting:
                    ConnectionState = ConnectionState.Connecting;
                    IsConnectEnabled = false;
                    AppendMessage("Connecting...");
                    break;

                case WebSocketConnectionState.Error:
                    ConnectionState = ConnectionState.Error;
                    IsConnectEnabled = true;
                    IsDisconnectEnabled = false;
                    IsUrlEnabled = true;
                    AppendMessage($"Error: {e.Error}");
                    break;
            }
        });
    }

    private void OnMessageReceived(object? sender, WebSocketMessageEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            AppendMessage($"[{e.MessageType}] {e.Message}", e.Timestamp);
        });
    }

    private void AppendMessage(string message, DateTime? timestamp = null)
    {
        var msg = new WebSocketMessage
        {
            Timestamp = timestamp?.ToString("HH:mm:ss") ?? DateTime.Now.ToString("HH:mm:ss"),
            MessageType = "",
            Message = message
        };
        Messages.Add(msg);

        // 更新 Text 属性
        var lines = Messages.Select(m => $"{m.Timestamp} {m.MessageType} {m.Message}");
        MessagesText = string.Join(System.Environment.NewLine, lines);
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            await _webSocketService.ConnectAsync(Url);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Connection failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        try
        {
            await _webSocketService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            AppendMessage($"Error disconnecting: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText))
            return;

        try
        {
            await _webSocketService.SendAsync(MessageText);
            AppendMessage($"[Sent] {MessageText}");
            MessageText = "";
        }
        catch (Exception ex)
        {
            AppendMessage($"Error sending: {ex.Message}");
        }
    }

    partial void OnMessagesTextChanged(string value)
    {
        // 触发自动滚动到末尾
    }

    public void Cleanup()
    {
        _webSocketService.StateChanged -= OnStateChanged;
        _webSocketService.MessageReceived -= OnMessageReceived;
        _webSocketService.Dispose();
    }
}
