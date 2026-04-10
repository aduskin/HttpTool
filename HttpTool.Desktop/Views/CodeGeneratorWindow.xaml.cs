using System.Windows;
using AduSkin.Controls;
using HttpTool.Core.Interfaces;
using HttpTool.Core.Models;
using HttpTool.Desktop.ViewModels;

namespace HttpTool.Desktop.Views;

public partial class CodeGeneratorWindow : AduWindow
{
    public CodeGeneratorWindow(ICodeGeneratorService codeGeneratorService, ApiRequest apiRequest, Dictionary<string, string>? variables = null)
    {
        InitializeComponent();
        DataContext = new CodeGeneratorWindowViewModel(codeGeneratorService, apiRequest, variables);
    }
}
