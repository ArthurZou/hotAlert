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
    private readonly LocalizationService _localizationService;
    private readonly Icon _normalIcon;
    private readonly Icon _warningIcon;
    private readonly IntPtr _normalIconHandle;
    private readonly IntPtr _warningIconHandle;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _dismissMenuItem;
    private readonly ToolStripMenuItem _languageMenuItem;
    private readonly ToolStripMenuItem _languageChineseMenuItem;
    private readonly ToolStripMenuItem _languageEnglishMenuItem;

    private bool _disposed;

    /// <summary>
    /// 请求显示设置窗口事件
    /// </summary>
    public event EventHandler? ShowSettingsRequested;

    /// <summary>
    /// 请求退出程序事件
    /// </summary>
    public event EventHandler? ExitRequested;

    public TrayService(AlertService alertService, ConfigService configService, LocalizationService localizationService)
    {
        _alertService = alertService ?? throw new ArgumentNullException(nameof(alertService));
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

        // 创建图标
        (_normalIcon, _normalIconHandle) = CreateIcon(Color.FromArgb(76, 175, 80));   // 绿色
        (_warningIcon, _warningIconHandle) = CreateIcon(Color.FromArgb(244, 67, 54));  // 红色

        // 创建右键菜单
        _contextMenu = new ContextMenuStrip();

        _dismissMenuItem = new ToolStripMenuItem(_localizationService.GetString("MenuDismiss"));
        _dismissMenuItem.Click += OnDismissClick;
        _dismissMenuItem.Enabled = false;
        _contextMenu.Items.Add(_dismissMenuItem);

        var settingsMenuItem = new ToolStripMenuItem(_localizationService.GetString("MenuSettings"));
        settingsMenuItem.Click += OnSettingsClick;
        _contextMenu.Items.Add(settingsMenuItem);

        // 语言子菜单
        _languageMenuItem = new ToolStripMenuItem(_localizationService.GetString("MenuLanguage"));
        _languageChineseMenuItem = new ToolStripMenuItem(_localizationService.GetString("LanguageChinese"));
        _languageChineseMenuItem.Tag = "zh-CN";
        _languageChineseMenuItem.Click += OnLanguageClick;
        _languageEnglishMenuItem = new ToolStripMenuItem(_localizationService.GetString("LanguageEnglish"));
        _languageEnglishMenuItem.Tag = "en-US";
        _languageEnglishMenuItem.Click += OnLanguageClick;

        _languageMenuItem.DropDownItems.Add(_languageChineseMenuItem);
        _languageMenuItem.DropDownItems.Add(_languageEnglishMenuItem);
        _contextMenu.Items.Add(_languageMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem(_localizationService.GetString("MenuExit"));
        exitMenuItem.Click += OnExitClick;
        _contextMenu.Items.Add(exitMenuItem);

        // 创建托盘图标
        _notifyIcon = new NotifyIcon
        {
            Icon = _normalIcon,
            Text = _localizationService.GetString("TrayTooltip"),
            Visible = true,
            ContextMenuStrip = _contextMenu
        };

        // 单击托盘图标关闭警告
        _notifyIcon.Click += OnTrayIconClick;

        // 订阅警告状态变更事件
        _alertService.AlertStateChanged += OnAlertStateChanged;

        // 订阅语言变更事件
        _localizationService.LanguageChanged += OnLanguageChanged;
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
                parts.Add($"{_localizationService.GetString("TooltipCpu")}: {state.CpuUsage:F1}%");
            }
            if (state.Type.HasFlag(AlertType.Memory))
            {
                parts.Add($"{_localizationService.GetString("TooltipMemory")}: {state.MemoryUsage:F1}%");
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

    private void OnLanguageClick(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string languageCode)
        {
            _localizationService.SetLanguage(languageCode);
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // 在 UI 线程更新菜单文本
        if (_contextMenu.InvokeRequired)
        {
            _contextMenu.Invoke(UpdateMenuText);
        }
        else
        {
            UpdateMenuText();
        }
    }

    private void UpdateMenuText()
    {
        _dismissMenuItem.Text = _localizationService.GetString("MenuDismiss");

        // 更新设置菜单
        if (_contextMenu.Items.Count > 1 && _contextMenu.Items[1] is ToolStripMenuItem settingsItem)
        {
            settingsItem.Text = _localizationService.GetString("MenuSettings");
        }

        // 更新语言菜单
        _languageMenuItem.Text = _localizationService.GetString("MenuLanguage");
        _languageChineseMenuItem.Text = _localizationService.GetString("LanguageChinese");
        _languageEnglishMenuItem.Text = _localizationService.GetString("LanguageEnglish");

        // 更新退出菜单（最后一项）
        if (_contextMenu.Items.Count > 0 && _contextMenu.Items[_contextMenu.Items.Count - 1] is ToolStripMenuItem exitItem)
        {
            exitItem.Text = _localizationService.GetString("MenuExit");
        }

        // 更新托盘提示文本
        _notifyIcon.Text = _localizationService.GetString("TrayTooltip");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _alertService.AlertStateChanged -= OnAlertStateChanged;
        _localizationService.LanguageChanged -= OnLanguageChanged;

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
