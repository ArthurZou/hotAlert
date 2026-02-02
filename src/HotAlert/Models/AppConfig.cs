namespace HotAlert.Models;

/// <summary>
/// 应用配置模型
/// </summary>
public class AppConfig
{
    /// <summary>
    /// CPU 使用率阈值 (0-100)
    /// </summary>
    public int CpuThreshold { get; set; } = 80;

    /// <summary>
    /// 内存使用率阈值 (0-100)
    /// </summary>
    public int MemoryThreshold { get; set; } = 80;

    /// <summary>
    /// 边框最小宽度 (像素)
    /// </summary>
    public int BorderMinWidth { get; set; } = 10;

    /// <summary>
    /// 边框最大宽度 (像素)
    /// </summary>
    public int BorderMaxWidth { get; set; } = 50;

    /// <summary>
    /// CPU 警告颜色
    /// </summary>
    public string CpuColor { get; set; } = "#FF4444";

    /// <summary>
    /// 内存警告颜色
    /// </summary>
    public string MemoryColor { get; set; } = "#FF8C00";

    /// <summary>
    /// 呼吸灯速度: slow, medium, fast
    /// </summary>
    public string BreathSpeed { get; set; } = "medium";

    /// <summary>
    /// 开机自启动
    /// </summary>
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// 语言: zh-CN, en-US
    /// </summary>
    public string Language { get; set; } = "zh-CN";
}
