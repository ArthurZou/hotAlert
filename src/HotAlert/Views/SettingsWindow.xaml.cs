using System.ComponentModel;
using System.Windows;

namespace HotAlert.Views;

/// <summary>
/// 设置窗口
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        // 隐藏窗口而非销毁，以便下次快速显示
        e.Cancel = true;
        Hide();
    }
}
