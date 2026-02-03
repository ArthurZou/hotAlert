using System.Windows;

namespace HotAlert;

/// <summary>
/// 主窗口（将作为设置窗口使用）
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StateChanged += MainWindow_StateChanged;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        // 最小化时隐藏窗口（到托盘）
        if (WindowState == WindowState.Minimized)
        {
            Hide();
        }
    }

    /// <summary>
    /// 从托盘恢复显示
    /// </summary>
    public void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // 点击关闭按钮时隐藏到托盘而不是退出
        e.Cancel = true;
        Hide();
    }
}
