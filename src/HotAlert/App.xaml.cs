using System.Windows;
using HotAlert.Services;
using Application = System.Windows.Application;

namespace HotAlert;

/// <summary>
/// 应用程序入口
/// </summary>
public partial class App : Application
{
    private ConfigService? _configService;
    private ResourceMonitor? _resourceMonitor;
    private AlertService? _alertService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化配置服务
        _configService = new ConfigService();
        _configService.Load();

        // 初始化资源监控服务
        _resourceMonitor = new ResourceMonitor();

        // 初始化警告服务
        _alertService = new AlertService(_configService, _resourceMonitor);
        _alertService.Initialize();

        // 启动监控
        _resourceMonitor.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _alertService?.Dispose();
        _resourceMonitor?.Dispose();
        base.OnExit(e);
    }
}
