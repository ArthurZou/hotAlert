using System.Diagnostics;
using System.Windows;
using HotAlert.Services;

namespace HotAlert;

/// <summary>
/// 应用程序入口
/// </summary>
public partial class App : Application
{
    private ResourceMonitor? _resourceMonitor;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _resourceMonitor = new ResourceMonitor();
        _resourceMonitor.ResourceUsageChanged += OnResourceUsageChanged;
        _resourceMonitor.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _resourceMonitor?.Dispose();
        base.OnExit(e);
    }

    private void OnResourceUsageChanged(object? sender, Models.ResourceUsageEventArgs e)
    {
        Debug.WriteLine($"[{e.Timestamp:HH:mm:ss}] CPU: {e.CpuUsage:F1}% | Memory: {e.MemoryUsage:F1}%");
    }
}

