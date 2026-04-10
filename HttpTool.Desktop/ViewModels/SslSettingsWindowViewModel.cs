using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// SSL 设置窗口 ViewModel
/// </summary>
public partial class SslSettingsWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _verifySsl = true;

    [ObservableProperty]
    private string _certificatePath = "";

    [ObservableProperty]
    private string _certificatePassword = "";

    public X509Certificate2? Certificate { get; private set; }

    [RelayCommand]
    private void BrowseCertificate()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Certificate Files (*.pfx;*.p12)|*.pfx;*.p12|All Files (*.*)|*.*",
            Title = "Select Certificate File"
        };

        if (dialog.ShowDialog() == true)
        {
            CertificatePath = dialog.FileName;
        }
    }

    public bool ValidateAndSave(Window? window)
    {
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
                return false;
            }
        }

        if (window != null)
        {
            window.DialogResult = true;
            window.Close();
        }
        return true;
    }
}
