namespace HotAlert.Models;

/// <summary>
/// 资源使用率事件参数
/// </summary>
public class ResourceUsageEventArgs : EventArgs
{
    /// <summary>
    /// CPU 使用率 (0-100)
    /// </summary>
    public float CpuUsage { get; }

    /// <summary>
    /// 内存使用率 (0-100)
    /// </summary>
    public float MemoryUsage { get; }

    /// <summary>
    /// 采样时间戳
    /// </summary>
    public DateTime Timestamp { get; }

    public ResourceUsageEventArgs(float cpuUsage, float memoryUsage)
    {
        CpuUsage = cpuUsage;
        MemoryUsage = memoryUsage;
        Timestamp = DateTime.Now;
    }
}
