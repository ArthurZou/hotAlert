using System.Windows.Media;
using HotAlert.Helpers;
using HotAlert.Models;
using HotAlert.Views;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Colors = System.Windows.Media.Colors;

namespace HotAlert.Services;

/// <summary>
/// 警告服务，协调资源监控与边框显示
/// </summary>
public class AlertService : IDisposable
{
    private readonly ConfigService _configService;
    private readonly ResourceMonitor _resourceMonitor;
    private readonly List<BorderOverlayWindow> _overlayWindows = new();
    private readonly object _windowLock = new();

    private bool _disposed;
    private bool _cpuAlertDismissed;
    private bool _memoryAlertDismissed;
    private bool _cpuWasBelowThreshold = true;
    private bool _memoryWasBelowThreshold = true;

    /// <summary>
    /// 当前警告状态
    /// </summary>
    public AlertState CurrentState { get; private set; } = new();

    /// <summary>
    /// 警告状态变更事件
    /// </summary>
    public event EventHandler<AlertState>? AlertStateChanged;

    public AlertService(ConfigService configService, ResourceMonitor resourceMonitor)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _resourceMonitor = resourceMonitor ?? throw new ArgumentNullException(nameof(resourceMonitor));
    }

    /// <summary>
    /// 初始化警告服务
    /// </summary>
    public void Initialize()
    {
        _resourceMonitor.ResourceUsageChanged += OnResourceUsageChanged;
        _configService.ConfigChanged += OnConfigChanged;
        ScreenHelper.DisplaySettingsChanged += OnDisplaySettingsChanged;

        CreateOverlayWindows();
    }

    /// <summary>
    /// 手动关闭警告
    /// </summary>
    public void DismissAlert()
    {
        _cpuAlertDismissed = CurrentState.Type.HasFlag(AlertType.Cpu);
        _memoryAlertDismissed = CurrentState.Type.HasFlag(AlertType.Memory);

        HideAllWindows();

        CurrentState = new AlertState
        {
            Type = AlertType.None,
            CpuUsage = CurrentState.CpuUsage,
            MemoryUsage = CurrentState.MemoryUsage
        };

        AlertStateChanged?.Invoke(this, CurrentState);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _resourceMonitor.ResourceUsageChanged -= OnResourceUsageChanged;
        _configService.ConfigChanged -= OnConfigChanged;
        ScreenHelper.DisplaySettingsChanged -= OnDisplaySettingsChanged;

        Application.Current?.Dispatcher.Invoke(CloseAllWindows);
    }

    private void OnResourceUsageChanged(object? sender, ResourceUsageEventArgs e)
    {
        var config = _configService.Config;

        // 检测是否降到阈值以下（用于重置 dismiss 标记）
        if (e.CpuUsage < config.CpuThreshold)
        {
            if (!_cpuWasBelowThreshold)
            {
                _cpuAlertDismissed = false;
            }
            _cpuWasBelowThreshold = true;
        }
        else
        {
            _cpuWasBelowThreshold = false;
        }

        if (e.MemoryUsage < config.MemoryThreshold)
        {
            if (!_memoryWasBelowThreshold)
            {
                _memoryAlertDismissed = false;
            }
            _memoryWasBelowThreshold = true;
        }
        else
        {
            _memoryWasBelowThreshold = false;
        }

        // 计算警告类型
        var alertType = AlertType.None;

        if (e.CpuUsage >= config.CpuThreshold && !_cpuAlertDismissed)
        {
            alertType |= AlertType.Cpu;
        }

        if (e.MemoryUsage >= config.MemoryThreshold && !_memoryAlertDismissed)
        {
            alertType |= AlertType.Memory;
        }

        // 计算边框宽度
        var cpuBorderWidth = alertType.HasFlag(AlertType.Cpu)
            ? CalculateBorderWidth(e.CpuUsage, config.CpuThreshold, config.BorderMinWidth, config.BorderMaxWidth)
            : 0;

        var memoryBorderWidth = alertType.HasFlag(AlertType.Memory)
            ? CalculateBorderWidth(e.MemoryUsage, config.MemoryThreshold, config.BorderMinWidth, config.BorderMaxWidth)
            : 0;

        CurrentState = new AlertState
        {
            Type = alertType,
            CpuUsage = e.CpuUsage,
            MemoryUsage = e.MemoryUsage,
            CpuBorderWidth = cpuBorderWidth,
            MemoryBorderWidth = memoryBorderWidth
        };

        // 在 UI 线程更新窗口
        Application.Current?.Dispatcher.Invoke(() => UpdateWindows(CurrentState));

        AlertStateChanged?.Invoke(this, CurrentState);
    }

    private void OnConfigChanged(object? sender, AppConfig config)
    {
        Application.Current?.Dispatcher.Invoke(() =>
        {
            var cpuColor = ParseColor(config.CpuColor, Colors.Red);
            var memoryColor = ParseColor(config.MemoryColor, Colors.Orange);

            foreach (var window in _overlayWindows)
            {
                window.SetCpuColor(cpuColor);
                window.SetMemoryColor(memoryColor);
                window.SetBreathSpeed(config.BreathSpeed);
            }
        });
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        Application.Current?.Dispatcher.Invoke(RecreateOverlayWindows);
    }

    /// <summary>
    /// 计算动态边框宽度
    /// </summary>
    private static double CalculateBorderWidth(float usage, int threshold, int minWidth, int maxWidth)
    {
        if (usage <= threshold) return 0;

        var ratio = (usage - threshold) / (100f - threshold);
        return minWidth + ratio * (maxWidth - minWidth);
    }

    /// <summary>
    /// 解析颜色字符串
    /// </summary>
    private static Color ParseColor(string colorHex, Color fallback)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(colorHex);
        }
        catch
        {
            return fallback;
        }
    }

    /// <summary>
    /// 创建所有显示器的覆盖窗口
    /// </summary>
    private void CreateOverlayWindows()
    {
        lock (_windowLock)
        {
            var screens = ScreenHelper.GetAllScreens();
            var config = _configService.Config;
            var cpuColor = ParseColor(config.CpuColor, Colors.Red);
            var memoryColor = ParseColor(config.MemoryColor, Colors.Orange);

            foreach (var screen in screens)
            {
                var window = new BorderOverlayWindow(screen);
                window.SetCpuColor(cpuColor);
                window.SetMemoryColor(memoryColor);
                window.SetBreathSpeed(config.BreathSpeed);
                _overlayWindows.Add(window);
            }
        }
    }

    /// <summary>
    /// 重新创建覆盖窗口（用于显示器配置变更）
    /// </summary>
    private void RecreateOverlayWindows()
    {
        lock (_windowLock)
        {
            CloseAllWindows();
            CreateOverlayWindows();

            // 如果之前有警告，立即更新显示
            if (CurrentState.Type != AlertType.None)
            {
                UpdateWindows(CurrentState);
            }
        }
    }

    /// <summary>
    /// 更新所有窗口状态
    /// </summary>
    private void UpdateWindows(AlertState state)
    {
        lock (_windowLock)
        {
            foreach (var window in _overlayWindows)
            {
                // 根据屏幕 DPI 缩放边框宽度
                var scaledState = new AlertState
                {
                    Type = state.Type,
                    CpuUsage = state.CpuUsage,
                    MemoryUsage = state.MemoryUsage,
                    CpuBorderWidth = ScreenHelper.ScaleBorderWidth(state.CpuBorderWidth, window.ScreenInfo.DpiScale),
                    MemoryBorderWidth = ScreenHelper.ScaleBorderWidth(state.MemoryBorderWidth, window.ScreenInfo.DpiScale)
                };

                window.UpdateAlertState(scaledState);

                if (state.Type != AlertType.None)
                {
                    if (!window.IsVisible)
                    {
                        window.Show();
                        window.StartBreathAnimation();
                    }
                }
                else
                {
                    if (window.IsVisible)
                    {
                        window.StopBreathAnimation();
                        window.Hide();
                    }
                }
            }
        }
    }

    /// <summary>
    /// 隐藏所有窗口
    /// </summary>
    private void HideAllWindows()
    {
        lock (_windowLock)
        {
            foreach (var window in _overlayWindows)
            {
                window.HideAll();
                window.StopBreathAnimation();
                window.Hide();
            }
        }
    }

    /// <summary>
    /// 关闭所有窗口
    /// </summary>
    private void CloseAllWindows()
    {
        lock (_windowLock)
        {
            foreach (var window in _overlayWindows)
            {
                window.StopBreathAnimation();
                window.Close();
            }
            _overlayWindows.Clear();
        }
    }
}
