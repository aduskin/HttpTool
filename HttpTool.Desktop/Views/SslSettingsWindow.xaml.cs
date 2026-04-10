using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using AduSkin.Controls;
using HttpTool.Desktop.ViewModels;

namespace HttpTool.Desktop.Views;

/// <summary>
/// SslSettingsWindow.xaml 的交互逻辑
/// </summary>
public partial class SslSettingsWindow : AduWindow
{
    public bool VerifySsl { get; private set; } = true;
    public string CertificatePath { get; private set; } = "";
    public string CertificatePassword { get; private set; } = "";
    public X509Certificate2? Certificate { get; private set; }

    public SslSettingsWindow()
    {
        InitializeComponent();
    }

    public SslSettingsWindow(bool verifySsl, string certPath, string certPassword) : this()
    {
        if (DataContext is SslSettingsWindowViewModel vm)
        {
            vm.VerifySsl = verifySsl;
            vm.CertificatePath = certPath;
            vm.CertificatePassword = certPassword;

            if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
            {
                try
                {
                    Certificate = new X509Certificate2(certPath, certPassword);
                }
                catch
                {
                    // 无法加载证书
                }
            }
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SslSettingsWindowViewModel vm)
        {
            VerifySsl = vm.VerifySsl;
            CertificatePath = vm.CertificatePath;
            CertificatePassword = vm.CertificatePassword;
            Certificate = vm.Certificate;

            if (!vm.ValidateAndSave(this))
            {
                return;
            }
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
