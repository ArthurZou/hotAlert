using System.Windows;
using HotAlert.Services;
using HotAlert.ViewModels;
using HotAlert.Views;
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
    private TrayService? _trayService;
    private MainWindow? _mainWindow;
    private SettingsWindow? _settingsWindow;

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

        // 初始化托盘服务
        _trayService = new TrayService(_alertService, _configService);
        _trayService.ShowSettingsRequested += OnShowSettingsRequested;
        _trayService.ExitRequested += OnExitRequested;

        // 创建主窗口但不显示（最小化到托盘）
        _mainWindow = new MainWindow();
        // 主窗口暂时不显示，等设置窗口实现后再启用
        // _mainWindow.Hide();

        // 启动监控
        _resourceMonitor.Start();

        // 应用自启动设置
        AutoStartService.SetAutoStart(_configService.Config.AutoStart);
    }

    private void OnShowSettingsRequested(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }

    private void ShowSettingsWindow()
    {
        if (_settingsWindow == null || !_settingsWindow.IsLoaded)
        {
            var viewModel = new SettingsViewModel(_configService!);
            viewModel.LoadFromConfig();
            _settingsWindow = new SettingsWindow { DataContext = viewModel };
            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        }
        _settingsWindow.Show();
        _settingsWindow.WindowState = WindowState.Normal;
        _settingsWindow.Activate();
    }

    private void OnExitRequested(object? sender, EventArgs e)
    {
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayService?.Dispose();
        _alertService?.Dispose();
        _resourceMonitor?.Dispose();
        base.OnExit(e);
    }
}
