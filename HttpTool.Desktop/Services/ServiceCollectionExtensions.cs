using System.Net.Http;
using HttpTool.Core.Interfaces;
using HttpTool.Desktop.ViewModels;
using HttpTool.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HttpTool.Desktop.Services;

/// <summary>
/// DI 服务注册
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpToolServices(this IServiceCollection services)
    {
        // 注册存储服务
        services.AddSingleton<IStorageService, JsonStorageService>();

        // 注册 HTTP 客户端工厂
        services.AddHttpClient("HttpTool")
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true
            });

        // 注册服务
        services.AddSingleton<IHttpRequestService, HttpRequestService>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IHistoryService, HistoryService>();
        services.AddSingleton<IFormatterService, FormatterService>();
        services.AddSingleton<IImportExportService, ImportExportService>();
        services.AddSingleton<ICodeGeneratorService, CodeGeneratorService>();

        // 注册 ViewModels
        services.AddTransient<MainWindowViewModel>();

        return services;
    }
}
