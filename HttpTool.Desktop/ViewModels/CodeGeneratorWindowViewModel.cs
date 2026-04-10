using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;

namespace HttpTool.Desktop.ViewModels;

/// <summary>
/// 代码生成器窗口 ViewModel
/// </summary>
public partial class CodeGeneratorWindowViewModel : ObservableObject
{
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ApiRequest _apiRequest;
    private readonly Dictionary<string, string>? _variables;

    [ObservableProperty]
    private IEnumerable<CodeLanguage> _supportedLanguages = Enumerable.Empty<CodeLanguage>();

    [ObservableProperty]
    private CodeLanguage? _selectedLanguage;

    [ObservableProperty]
    private string _generatedCode = "";

    public CodeGeneratorWindowViewModel(
        ICodeGeneratorService codeGeneratorService,
        ApiRequest apiRequest,
        Dictionary<string, string>? variables = null)
    {
        _codeGeneratorService = codeGeneratorService;
        _apiRequest = apiRequest;
        _variables = variables;

        SupportedLanguages = _codeGeneratorService.SupportedLanguages;
        if (SupportedLanguages.Any())
        {
            SelectedLanguage = SupportedLanguages.First();
        }
    }

    partial void OnSelectedLanguageChanged(CodeLanguage? value)
    {
        if (value != null)
        {
            GeneratedCode = _codeGeneratorService.GenerateCode(_apiRequest, value, _variables);
        }
    }

    [RelayCommand]
    private void Copy()
    {
        if (!string.IsNullOrEmpty(GeneratedCode))
        {
            Clipboard.SetText(GeneratedCode);
            MessageBox.Show("Code copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    [RelayCommand]
    private void Close(Window? window)
    {
        if (window != null)
        {
            window.Close();
        }
    }
}
