using CommunityToolkit.Mvvm.ComponentModel;

namespace HttpTool.Core.Models;

/// <summary>
/// 项目
/// </summary>
public partial class Project : ObservableObject
{
    [ObservableProperty]
    private string _id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _name = "New Project";

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private List<ApiRequest> _apis = new();

    [ObservableProperty]
    private Environment _environment = new();

    [ObservableProperty]
    private DateTime _createdAt = DateTime.Now;

    [ObservableProperty]
    private DateTime _updatedAt = DateTime.Now;

    [ObservableProperty]
    private string _filePath = string.Empty;

    /// <summary>
    /// 创建默认项目
    /// </summary>
    public static Project CreateDefault()
    {
        return new Project
        {
            Name = "New Project",
            Environment = new Environment
            {
                Variables = new List<EnvironmentVariable>
                {
                    new() { Key = "base_url", Value = "https://api.example.com", IsEnabled = true },
                    new() { Key = "api_key", Value = "", IsEnabled = false }
                }
            }
        };
    }
}
