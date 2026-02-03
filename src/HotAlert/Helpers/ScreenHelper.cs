using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace HotAlert.Helpers;

/// <summary>
/// 屏幕信息数据
/// </summary>
public class ScreenInfo
{
    /// <summary>
    /// 屏幕设备名称
    /// </summary>
    public string DeviceName { get; init; } = string.Empty;

    /// <summary>
    /// 工作区域（逻辑像素）
    /// </summary>
    public Rect Bounds { get; init; }

    /// <summary>
    /// DPI 缩放比例
    /// </summary>
    public double DpiScale { get; init; } = 1.0;

    /// <summary>
    /// 是否为主显示器
    /// </summary>
    public bool IsPrimary { get; init; }
}

/// <summary>
/// 多显示器和 DPI 辅助工具
/// </summary>
public static class ScreenHelper
{
    /// <summary>
    /// 显示器配置变更事件
    /// </summary>
    public static event EventHandler? DisplaySettingsChanged;

    static ScreenHelper()
    {
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    /// <summary>
    /// 获取所有显示器信息
    /// </summary>
    public static IReadOnlyList<ScreenInfo> GetAllScreens()
    {
        var screens = new List<ScreenInfo>();

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var dpiScale = GetDpiScaleForScreen(screen);
            var bounds = new Rect(
                screen.Bounds.X / dpiScale,
                screen.Bounds.Y / dpiScale,
                screen.Bounds.Width / dpiScale,
                screen.Bounds.Height / dpiScale);

            screens.Add(new ScreenInfo
            {
                DeviceName = screen.DeviceName,
                Bounds = bounds,
                DpiScale = dpiScale,
                IsPrimary = screen.Primary
            });
        }

        return screens;
    }

    /// <summary>
    /// 获取指定屏幕的 DPI 缩放比例
    /// </summary>
    private static double GetDpiScaleForScreen(System.Windows.Forms.Screen screen)
    {
        try
        {
            var hMonitor = MonitorFromPoint(
                new POINT { x = screen.Bounds.X + 1, y = screen.Bounds.Y + 1 },
                MONITOR_DEFAULTTONEAREST);

            if (hMonitor != IntPtr.Zero)
            {
                GetDpiForMonitor(hMonitor, DpiType.Effective, out uint dpiX, out _);
                return dpiX / 96.0;
            }
        }
        catch
        {
            // 回退到系统 DPI
        }

        return GetSystemDpiScale();
    }

    /// <summary>
    /// 获取系统默认 DPI 缩放比例
    /// </summary>
    public static double GetSystemDpiScale()
    {
        using var source = new HwndSource(new HwndSourceParameters());
        var transformToDevice = source.CompositionTarget?.TransformToDevice;
        return transformToDevice?.M11 ?? 1.0;
    }

    /// <summary>
    /// 根据 DPI 缩放边框宽度
    /// </summary>
    public static double ScaleBorderWidth(double baseWidth, double dpiScale)
    {
        return baseWidth * dpiScale;
    }

    private static void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        DisplaySettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    #region Win32 API

    private const int MONITOR_DEFAULTTONEAREST = 2;

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(POINT pt, int dwFlags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr hMonitor, DpiType dpiType, out uint dpiX, out uint dpiY);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    private enum DpiType
    {
        Effective = 0,
        Angular = 1,
        Raw = 2
    }

    #endregion
}
