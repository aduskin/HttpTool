using System.Text;
using System.Windows;
using System.Windows.Input;
using AduSkin.Controls;
using HttpTool.Core.Interfaces;
using HttpTool.Infrastructure.Services;

namespace HttpTool.Desktop.Views;

public partial class WebSocketWindow : AduWindow
{
    private readonly IWebSocketService _webSocketService;

    public WebSocketWindow()
    {
        InitializeComponent();
        _webSocketService = new WebSocketService();
        _webSocketService.StateChanged += OnStateChanged;
        _webSocketService.MessageReceived += OnMessageReceived;
    }

    private void OnStateChanged(object? sender, WebSocketStateEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            switch (e.State)
            {
                case WebSocketConnectionState.Connected:
                    ConnectButton.IsEnabled = false;
                    DisconnectButton.IsEnabled = true;
                    UrlTextBox.IsEnabled = false;
                    AppendMessage($"[{DateTime.Now:HH:mm:ss}] Connected to {UrlTextBox.Text}");
                    break;

                case WebSocketConnectionState.Disconnected:
                    ConnectButton.IsEnabled = true;
                    DisconnectButton.IsEnabled = false;
                    UrlTextBox.IsEnabled = true;
                    AppendMessage($"[{DateTime.Now:HH:mm:ss}] Disconnected");
                    break;

                case WebSocketConnectionState.Connecting:
                    ConnectButton.IsEnabled = false;
                    AppendMessage($"[{DateTime.Now:HH:mm:ss}] Connecting...");
                    break;

                case WebSocketConnectionState.Error:
                    ConnectButton.IsEnabled = true;
                    DisconnectButton.IsEnabled = false;
                    UrlTextBox.IsEnabled = true;
                    AppendMessage($"[{DateTime.Now:HH:mm:ss}] Error: {e.Error}");
                    break;
            }
        });
    }

    private void OnMessageReceived(object? sender, WebSocketMessageEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            AppendMessage($"[{e.Timestamp:HH:mm:ss}] [{e.MessageType}] {e.Message}");
        });
    }

    private void AppendMessage(string message)
    {
        if (MessagesTextBox.Text.Length > 0)
        {
            MessagesTextBox.Text += Environment.NewLine;
        }
        MessagesTextBox.Text += message;
        MessagesTextBox.ScrollToEnd();
    }

    private async void Connect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _webSocketService.ConnectAsync(UrlTextBox.Text);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, $"Connection failed: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Disconnect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _webSocketService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            AppendMessage($"[{DateTime.Now:HH:mm:ss}] Error disconnecting: {ex.Message}");
        }
    }

    private async void Send_Click(object sender, RoutedEventArgs e)
    {
        await SendMessageAsync();
    }

    private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            return;

        try
        {
            await _webSocketService.SendAsync(MessageTextBox.Text);
            AppendMessage($"[{DateTime.Now:HH:mm:ss}] [Sent] {MessageTextBox.Text}");
            MessageTextBox.Clear();
        }
        catch (Exception ex)
        {
            AppendMessage($"[{DateTime.Now:HH:mm:ss}] Error sending: {ex.Message}");
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _webSocketService.StateChanged -= OnStateChanged;
        _webSocketService.MessageReceived -= OnMessageReceived;
        _webSocketService.Dispose();
        base.OnClosed(e);
    }
}
