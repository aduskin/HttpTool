using System.IO;
using System.Security.Cryptography.X509Certificates;
using AduSkin.Controls;
using Microsoft.Win32;

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
        VerifySslCheckBox.IsChecked = verifySsl;
        CertificatePathTextBox.Text = certPath;
        CertificatePasswordTextBox.Text = certPassword;

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

    private void BrowseCertificate_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Certificate Files (*.pfx;*.p12)|*.pfx;*.p12|All Files (*.*)|*.*",
            Title = "Select Certificate File"
        };

        if (dialog.ShowDialog() == true)
        {
            CertificatePathTextBox.Text = dialog.FileName;
        }
    }

    private void Save_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        VerifySsl = VerifySslCheckBox.IsChecked ?? true;
        CertificatePath = CertificatePathTextBox.Text;
        CertificatePassword = CertificatePasswordTextBox.Text;

        if (!string.IsNullOrEmpty(CertificatePath) && File.Exists(CertificatePath))
        {
            try
            {
                Certificate = new X509Certificate2(CertificatePath, CertificatePassword);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load certificate: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
        }

        DialogResult = true;
        Close();
    }

    private void Close_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
