using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HotAlert.Models;

namespace HotAlert.Services;

/// <summary>
/// 系统托盘服务，管理托盘图标和右键菜单
/// </summary>
public class TrayService : IDisposable
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool DestroyIcon(IntPtr handle);

    private readonly NotifyIcon _notifyIcon;
    private readonly AlertService _alertService;
    private readonly ConfigService _configService;
    private readonly Icon _normalIcon;
    private readonly Icon _warningIcon;
    private readonly IntPtr _normalIconHandle;
    private readonly IntPtr _warningIconHandle;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _dismissMenuItem;

    private bool _disposed;

    /// <summary>
    /// 请求显示设置窗口事件
    /// </summary>
    public event EventHandler? ShowSettingsRequested;

    /// <summary>
    /// 请求退出程序事件
    /// </summary>
    public event EventHandler? ExitRequested;

    public TrayService(AlertService alertService, ConfigService configService)
    {
        _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        // 创建图标
        (_normalIcon, _normalIconHandle) = CreateIcon(Color.FromArgb(76, 175, 80));   // 绿色
        (_warningIcon, _warningIconHandle) = CreateIcon(Color.FromArgb(244, 67, 54));  // 红色

        // 创建右键菜单
        _contextMenu = new ContextMenuStrip();

        _dismissMenuItem = new ToolStripMenuItem("关闭警告");
        _dismissMenuItem.Click += OnDismissClick;
        _dismissMenuItem.Enabled = false;
        _contextMenu.Items.Add(_dismissMenuItem);

        var settingsMenuItem = new ToolStripMenuItem("设置...");
        settingsMenuItem.Click += OnSettingsClick;
        _contextMenu.Items.Add(settingsMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem("退出");
        exitMenuItem.Click += OnExitClick;
        _contextMenu.Items.Add(exitMenuItem);

        // 创建托盘图标
        _notifyIcon = new NotifyIcon
        {
            Icon = _normalIcon,
            Text = "HotAlert - 系统资源监控",
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        // 单击托盘图标关闭警告
        _notifyIcon.Click += OnTrayIconClick;

        // 订阅警告状态变更事件
        _alertService.AlertStateChanged += OnAlertStateChanged;
    }

    /// <summary>
    /// 动态创建托盘图标，返回图标和句柄用于后续释放
    /// </summary>
    private static (Icon icon, IntPtr handle) CreateIcon(Color color)
    {
        const int size = 16;
        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);

        // 抗锯齿
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // 绘制填充的圆形
        using var brush = new SolidBrush(color);
        graphics.FillEllipse(brush, 1, 1, size - 2, size - 2);

        // 绘制边框
        using var pen = new Pen(Color.FromArgb(50, 50, 50), 1);
        graphics.DrawEllipse(pen, 1, 1, size - 3, size - 3);

        // 转换为 Icon，保留句柄用于释放
        var iconHandle = bitmap.GetHicon();
        return (Icon.FromHandle(iconHandle), iconHandle);
    }

    /// <summary>
    /// 更新图标状态
    /// </summary>
    private void UpdateIcon(AlertState state)
    {
        if (_disposed) return;

        var hasAlert = state.Type != AlertType.None;
        _notifyIcon.Icon = hasAlert ? _warningIcon : _normalIcon;
        _dismissMenuItem.Enabled = hasAlert;

        // 更新提示文本
        var text = "HotAlert";
        if (hasAlert)
        {
            var parts = new List<string>();
            if (state.Type.HasFlag(AlertType.Cpu))
            {
                parts.Add($"CPU: {state.CpuUsage:F1}%");
            }
            if (state.Type.HasFlag(AlertType.Memory))
            {
                parts.Add($"内存: {state.MemoryUsage:F1}%");
            }
            text = string.Join(" | ", parts);
        }

        // NotifyIcon.Text 最多 63 字符
        _notifyIcon.Text = text.Length > 63 ? text[..63] : text;
    }

    private void OnAlertStateChanged(object? sender, AlertState state)
    {
        // 需要在 UI 线程更新
        if (_notifyIcon.ContextMenuStrip?.InvokeRequired == true)
        {
            _notifyIcon.ContextMenuStrip.Invoke(() => UpdateIcon(state));
        }
        else
        {
            UpdateIcon(state);
        }
    }

    private void OnTrayIconClick(object? sender, EventArgs e)
    {
        // 只响应鼠标左键点击
        if (e is MouseEventArgs mouseArgs && mouseArgs.Button == MouseButtons.Left)
        {
            if (_alertService.CurrentState.Type != AlertType.None)
            {
                _alertService.DismissAlert();
            }
        }
    }

    private void OnDismissClick(object? sender, EventArgs e)
    {
        _alertService.DismissAlert();
    }

    private void OnSettingsClick(object? sender, EventArgs e)
    {
        ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _alertService.AlertStateChanged -= OnAlertStateChanged;

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _normalIcon.Dispose();
        _warningIcon.Dispose();

        // 释放图标句柄
        DestroyIcon(_normalIconHandle);
        DestroyIcon(_warningIconHandle);
    }
}
