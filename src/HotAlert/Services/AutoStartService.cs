using Microsoft.Win32;

namespace HotAlert.Services;

/// <summary>
/// 开机自启动服务，通过注册表管理
/// </summary>
public static class AutoStartService
{
    private const string AppName = "HotAlert";
    private const string RunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// 设置开机自启动状态
    /// </summary>
    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
            if (key == null) return;

            if (enable)
            {
                key.SetValue(AppName, GetExecutablePath());
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // 注册表操作失败时静默忽略
        }
    }

    /// <summary>
    /// 获取当前自启动状态
    /// </summary>
    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取可执行文件路径
    /// </summary>
    private static string GetExecutablePath()
    {
        var processPath = Environment.ProcessPath;
        return processPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location;
    }
}
