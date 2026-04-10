using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using AduSkin.Controls;
using HttpTool.Core.Constants;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;
using HttpTool.Desktop.ViewModels;
using HttpTool.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace HttpTool.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AduWindow
{
    private readonly IImportExportService _importExportService;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ObservableCollection<Cookie> _cookies = new();

    // 全局设置
    public static bool EnableProxy { get; set; }
    public static string ProxyHost { get; set; } = "";
    public static int ProxyPort { get; set; }
    public static bool VerifySslCertificate { get; set; } = true;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        _importExportService = App.Services.GetRequiredService<IImportExportService>();
        _codeGeneratorService = App.Services.GetRequiredService<ICodeGeneratorService>();
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ViewHistory_Click(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow();
        historyWindow.Owner = this;
        historyWindow.ShowDialog();
    }

    private void ViewEnvironment_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.SelectedTab?.Project?.Environment == null)
        {
            System.Windows.MessageBox.Show("Please open a project first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var envWindow = new EnvironmentWindow(vm.SelectedTab.Project.Environment.Variables);
        envWindow.Owner = this;

        if (envWindow.ShowDialog() == true)
        {
            vm.SelectedTab.Project.Environment.Variables = envWindow.Variables.ToList();
        }
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = $"All Supported Files (*.json;*{ProjectConstant.ProjectExtension})|*.json;*{ProjectConstant.ProjectExtension}|Postman Collection (*.json)|*.json|HttpTool Project (*{ProjectConstant.ProjectExtension})|*{ProjectConstant.ProjectExtension}",
            Title = "Import"
        };

        if (dialog.ShowDialog() == true)
        {
            var project = await _importExportService.ImportAsync(dialog.FileName);
            if (project != null)
            {
                var vm = DataContext as MainWindowViewModel;
                if (vm == null) return;

                var tab = new ProjectTabItem
                {
                    Project = project,
                    Apis = new ObservableCollection<ApiRequest>(project.Apis)
                };
                vm.ProjectTabs.Add(tab);
                vm.SelectedTab = tab;
            }
        }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.SelectedTab?.Project == null)
        {
            System.Windows.MessageBox.Show("Please open a project first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = $"HttpTool Project (*{ProjectConstant.ProjectExtension})|*{ProjectConstant.ProjectExtension}|JSON (*.json)|*.json",
            Title = "Export",
            FileName = vm.SelectedTab.Project.Name
        };

        if (dialog.ShowDialog() == true)
        {
            vm.SelectedTab.Project.Apis = vm.SelectedTab.Apis.ToList();
            await _importExportService.ExportAsync(vm.SelectedTab.Project, dialog.FileName);
            System.Windows.MessageBox.Show("Export completed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CopyAsCurl_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.SelectedTab?.SelectedApi == null)
        {
            System.Windows.MessageBox.Show("Please select an API request first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var variables = vm.SelectedTab.Project?.Environment?.Variables
            .Where(v => v.IsEnabled)
            .ToDictionary(v => v.Key, v => v.Value);

        var curl = _importExportService.ExportToCurl(vm.SelectedTab.SelectedApi, variables);
        Clipboard.SetText(curl);
        System.Windows.MessageBox.Show("cURL command copied to clipboard.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void GenerateCode_Click(object sender, RoutedEventArgs e)
    {
        var vm = DataContext as MainWindowViewModel;
        if (vm?.SelectedTab?.SelectedApi == null)
        {
            System.Windows.MessageBox.Show("Please select an API request first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var variables = vm.SelectedTab.Project?.Environment?.Variables
            .Where(v => v.IsEnabled)
            .ToDictionary(v => v.Key, v => v.Value);

        var codeWindow = new CodeGeneratorWindow(_codeGeneratorService, vm.SelectedTab.SelectedApi, variables);
        codeWindow.Owner = this;
        codeWindow.ShowDialog();
    }

    private void CookieManager_Click(object sender, RoutedEventArgs e)
    {
        var cookieWindow = new CookieManagerWindow(_cookies);
        cookieWindow.Owner = this;
        cookieWindow.ShowDialog();
    }

    private void ProxySettings_Click(object sender, RoutedEventArgs e)
    {
        var proxyWindow = new ProxySettingsWindow(EnableProxy, "HTTP", ProxyHost, ProxyPort, "", "");
        proxyWindow.Owner = this;

        if (proxyWindow.ShowDialog() == true)
        {
            EnableProxy = proxyWindow.ProxyEnabled;
            ProxyHost = proxyWindow.ProxyHost;
            ProxyPort = proxyWindow.ProxyPort;
        }
    }

    private void SslSettings_Click(object sender, RoutedEventArgs e)
    {
        var sslWindow = new SslSettingsWindow(VerifySslCertificate, "", "");
        sslWindow.Owner = this;

        if (sslWindow.ShowDialog() == true)
        {
            VerifySslCertificate = sslWindow.VerifySsl;
        }
    }

    private void WebSocket_Click(object sender, RoutedEventArgs e)
    {
        var wsWindow = new WebSocketWindow();
        wsWindow.Owner = this;
        wsWindow.Show();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        aboutWindow.Owner = this;
        aboutWindow.ShowDialog();
    }

    private void ProjectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (DataContext is MainWindowViewModel vm && e.NewValue is ProjectTabItem tab)
        {
            vm.SelectedTab = tab;
        }
    }
}
