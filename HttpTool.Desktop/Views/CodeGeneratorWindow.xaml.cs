using System.Windows;
using System.Windows.Controls;
using AduSkin.Controls;
using HttpTool.Core.Interfaces;

namespace HttpTool.Desktop.Views;

public partial class CodeGeneratorWindow : AduWindow
{
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly Core.Models.ApiRequest _apiRequest;
    private readonly Dictionary<string, string>? _variables;

    public CodeGeneratorWindow(ICodeGeneratorService codeGeneratorService, Core.Models.ApiRequest apiRequest, Dictionary<string, string>? variables = null)
    {
        InitializeComponent();

        _codeGeneratorService = codeGeneratorService;
        _apiRequest = apiRequest;
        _variables = variables;

        LanguageComboBox.ItemsSource = _codeGeneratorService.SupportedLanguages;
        LanguageComboBox.SelectedIndex = 0;
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is CodeLanguage language)
        {
            var code = _codeGeneratorService.GenerateCode(_apiRequest, language, _variables);
            CodeTextBox.Text = code;
        }
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(CodeTextBox.Text))
        {
            Clipboard.SetText(CodeTextBox.Text);
            System.Windows.MessageBox.Show("Code copied to clipboard!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
