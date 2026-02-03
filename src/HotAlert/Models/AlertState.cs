namespace HotAlert.Models;

/// <summary>
/// 警告类型枚举
/// </summary>
[Flags]
public enum AlertType
{
    /// <summary>
    /// 无警告
    /// </summary>
    None = 0,

    /// <summary>
    /// CPU 警告
    /// </summary>
    Cpu = 1,

    /// <summary>
    /// 内存警告
    /// </summary>
    Memory = 2,

    /// <summary>
    /// CPU 和内存同时警告
    /// </summary>
    Both = Cpu | Memory
}

/// <summary>
/// 警告状态数据
/// </summary>
public class AlertState
{
    /// <summary>
    /// 当前警告类型
    /// </summary>
    public AlertType Type { get; set; } = AlertType.None;

    /// <summary>
    /// CPU 使用率
    /// </summary>
    public float CpuUsage { get; set; }

    /// <summary>
    /// 内存使用率
    /// </summary>
    public float MemoryUsage { get; set; }

    /// <summary>
    /// CPU 边框宽度
    /// </summary>
    public double CpuBorderWidth { get; set; }

    /// <summary>
    /// 内存边框宽度
    /// </summary>
    public double MemoryBorderWidth { get; set; }
}
