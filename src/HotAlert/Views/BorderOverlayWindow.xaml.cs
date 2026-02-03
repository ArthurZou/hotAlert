using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HotAlert.Helpers;
using HotAlert.Models;
using HotAlert.Services;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Duration = System.Windows.Duration;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Thickness = System.Windows.Thickness;
using Visibility = System.Windows.Visibility;
using Window = System.Windows.Window;

namespace HotAlert.Views;

/// <summary>
/// 边框覆盖层窗口
/// </summary>
public partial class BorderOverlayWindow : Window
{
    private readonly ScreenInfo _screenInfo;
    private Storyboard? _breathStoryboard;

    private Color _cpuColor = Colors.Red;
    private Color _memoryColor = Colors.Orange;
    private double _cpuBorderWidth;
    private double _memoryBorderWidth;
    private float _cpuUsage;
    private float _memoryUsage;
    private bool _cpuVisible;
    private bool _memoryVisible;

    // Win32 API 常量
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    public BorderOverlayWindow(ScreenInfo screenInfo)
    {
        InitializeComponent();

        _screenInfo = screenInfo;

        PositionWindow();
        SetupAnimation();
        InitializeBorderBrushes();
    }

    /// <summary>
    /// 关联的屏幕信息
    /// </summary>
    public ScreenInfo ScreenInfo => _screenInfo;

    /// <summary>
    /// 定位窗口到指定屏幕
    /// </summary>
    private void PositionWindow()
    {
        Left = _screenInfo.Bounds.Left;
        Top = _screenInfo.Bounds.Top;
        Width = _screenInfo.Bounds.Width;
        Height = _screenInfo.Bounds.Height;
    }

    /// <summary>
    /// 设置呼吸灯动画
    /// </summary>
    private void SetupAnimation()
    {
        _breathStoryboard = (Storyboard)FindResource("BreathAnimation");
    }

    /// <summary>
    /// 初始化边框渐变画刷
    /// </summary>
    private void InitializeBorderBrushes()
    {
        UpdateCpuBorderBrushes();
        UpdateMemoryBorderBrushes();
    }

    /// <summary>
    /// 设置 CPU 警告颜色
    /// </summary>
    public void SetCpuColor(Color color)
    {
        _cpuColor = color;
        UpdateCpuBorderBrushes();
    }

    /// <summary>
    /// 设置内存警告颜色
    /// </summary>
    public void SetMemoryColor(Color color)
    {
        _memoryColor = color;
        UpdateMemoryBorderBrushes();
    }

    /// <summary>
    /// 更新警告状态
    /// </summary>
    public void UpdateAlertState(AlertState state)
    {
        _cpuUsage = state.CpuUsage;
        _memoryUsage = state.MemoryUsage;
        _cpuVisible = state.Type.HasFlag(AlertType.Cpu);
        _memoryVisible = state.Type.HasFlag(AlertType.Memory);

        // 更新 CPU 边框
        if (_cpuVisible && Math.Abs(_cpuBorderWidth - state.CpuBorderWidth) > 0.1)
        {
            _cpuBorderWidth = state.CpuBorderWidth;
            UpdateCpuBorderSizes();
        }
        CpuBorderGrid.Visibility = _cpuVisible ? Visibility.Visible : Visibility.Collapsed;

        // 更新内存边框
        if (_memoryVisible && Math.Abs(_memoryBorderWidth - state.MemoryBorderWidth) > 0.1)
        {
            _memoryBorderWidth = state.MemoryBorderWidth;
            UpdateMemoryBorderSizes();
        }
        MemoryBorderGrid.Visibility = _memoryVisible ? Visibility.Visible : Visibility.Collapsed;

        // 设置内存边框的边距（在 CPU 边框内侧）
        if (_cpuVisible && _memoryVisible)
        {
            MemoryBorderGrid.Margin = new Thickness(_cpuBorderWidth);
        }
        else
        {
            MemoryBorderGrid.Margin = new Thickness(0);
        }

        UpdateTooltipText();
    }

    /// <summary>
    /// 隐藏所有边框
    /// </summary>
    public void HideAll()
    {
        _cpuVisible = false;
        _memoryVisible = false;
        CpuBorderGrid.Visibility = Visibility.Collapsed;
        MemoryBorderGrid.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// 启动呼吸灯动画
    /// </summary>
    public void StartBreathAnimation()
    {
        _breathStoryboard?.Begin(this, true);
    }

    /// <summary>
    /// 停止呼吸灯动画
    /// </summary>
    public void StopBreathAnimation()
    {
        _breathStoryboard?.Stop(this);
    }

    /// <summary>
    /// 设置呼吸灯速度
    /// </summary>
    public void SetBreathSpeed(string speed)
    {
        var ms = speed switch
        {
            "slow" => 1500,
            "fast" => 500,
            _ => 1000
        };

        if (_breathStoryboard?.Children[0] is DoubleAnimation animation)
        {
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
        }
    }

    /// <summary>
    /// 重写OnSourceInitialized以设置窗口点击穿透
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // 获取窗口句柄并设置 WS_EX_TRANSPARENT 样式实现点击穿透
        var hwnd = new WindowInteropHelper(this).Handle;
        var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
    }

    /// <summary>
    /// 刷新窗口位置
    /// </summary>
    public void RefreshPosition(ScreenInfo newScreenInfo)
    {
        Left = newScreenInfo.Bounds.Left;
        Top = newScreenInfo.Bounds.Top;
        Width = newScreenInfo.Bounds.Width;
        Height = newScreenInfo.Bounds.Height;
    }

    private void UpdateCpuBorderBrushes()
    {
        SetGradientBrush(CpuTopBorder, _cpuColor, GradientDirection.TopToBottom);
        SetGradientBrush(CpuBottomBorder, _cpuColor, GradientDirection.BottomToTop);
        SetGradientBrush(CpuLeftBorder, _cpuColor, GradientDirection.LeftToRight);
        SetGradientBrush(CpuRightBorder, _cpuColor, GradientDirection.RightToLeft);
    }

    private void UpdateMemoryBorderBrushes()
    {
        SetGradientBrush(MemoryTopBorder, _memoryColor, GradientDirection.TopToBottom);
        SetGradientBrush(MemoryBottomBorder, _memoryColor, GradientDirection.BottomToTop);
        SetGradientBrush(MemoryLeftBorder, _memoryColor, GradientDirection.LeftToRight);
        SetGradientBrush(MemoryRightBorder, _memoryColor, GradientDirection.RightToLeft);
    }

    private void UpdateCpuBorderSizes()
    {
        CpuTopBorder.Height = _cpuBorderWidth;
        CpuBottomBorder.Height = _cpuBorderWidth;
        CpuLeftBorder.Width = _cpuBorderWidth;
        CpuRightBorder.Width = _cpuBorderWidth;
    }

    private void UpdateMemoryBorderSizes()
    {
        MemoryTopBorder.Height = _memoryBorderWidth;
        MemoryBottomBorder.Height = _memoryBorderWidth;
        MemoryLeftBorder.Width = _memoryBorderWidth;
        MemoryRightBorder.Width = _memoryBorderWidth;
    }

    private void UpdateTooltipText()
    {
        var parts = new List<string>();
        if (_cpuVisible)
        {
            var cpuLabel = App.Current.LocalizationService.GetString("TooltipCpu");
            parts.Add($"{cpuLabel}: {_cpuUsage:F0}%");
        }
        if (_memoryVisible)
        {
            var memoryLabel = App.Current.LocalizationService.GetString("TooltipMemory");
            parts.Add($"{memoryLabel}: {_memoryUsage:F0}%");
        }
        TooltipText.Text = string.Join(" | ", parts);
    }

    private enum GradientDirection
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft
    }

    private static void SetGradientBrush(Rectangle rect, Color color, GradientDirection direction)
    {
        var (startPoint, endPoint) = direction switch
        {
            GradientDirection.TopToBottom => (new Point(0, 0), new Point(0, 1)),
            GradientDirection.BottomToTop => (new Point(0, 1), new Point(0, 0)),
            GradientDirection.LeftToRight => (new Point(0, 0), new Point(1, 0)),
            GradientDirection.RightToLeft => (new Point(1, 0), new Point(0, 0)),
            _ => (new Point(0, 0), new Point(0, 1))
        };

        var brush = new LinearGradientBrush
        {
            StartPoint = startPoint,
            EndPoint = endPoint
        };
        brush.GradientStops.Add(new GradientStop(color, 0));
        brush.GradientStops.Add(new GradientStop(Colors.Transparent, 1));

        rect.Fill = brush;
    }
}
