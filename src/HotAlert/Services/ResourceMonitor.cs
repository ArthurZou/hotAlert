using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using HotAlert.Models;
using Timer = System.Timers.Timer;

namespace HotAlert.Services;

/// <summary>
/// 资源监控服务，定时采样 CPU 和内存使用率
/// </summary>
public class ResourceMonitor : IDisposable
{
    private const int SamplingIntervalMs = 3000;

    private readonly PerformanceCounter _cpuCounter;
    private readonly Timer _timer;
    private bool _disposed;

    /// <summary>
    /// 当前 CPU 使用率 (0-100)
    /// </summary>
    public float CpuUsage { get; private set; }

    /// <summary>
    /// 当前内存使用率 (0-100)
    /// </summary>
    public float MemoryUsage { get; private set; }

    /// <summary>
    /// 监控是否运行中
    /// </summary>
    public bool IsRunning => _timer.Enabled;

    /// <summary>
    /// 资源使用率变化事件
    /// </summary>
    public event EventHandler<ResourceUsageEventArgs>? ResourceUsageChanged;

    public ResourceMonitor()
    {
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _timer = new Timer(SamplingIntervalMs);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
    }

    /// <summary>
    /// 启动监控
    /// </summary>
    public void Start()
    {
        if (_disposed) return;

        // 预热 CPU 计数器，首次调用返回 0
        _cpuCounter.NextValue();

        _timer.Start();
    }

    /// <summary>
    /// 停止监控
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _timer.Stop();
        _timer.Elapsed -= OnTimerElapsed;
        _timer.Dispose();
        _cpuCounter.Dispose();
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        CpuUsage = _cpuCounter.NextValue();
        MemoryUsage = GetMemoryUsage();

        ResourceUsageChanged?.Invoke(this, new ResourceUsageEventArgs(CpuUsage, MemoryUsage));
    }

    /// <summary>
    /// 通过 Win32 API 获取内存使用率
    /// </summary>
    private static float GetMemoryUsage()
    {
        var memoryStatus = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        if (GlobalMemoryStatusEx(ref memoryStatus))
        {
            return memoryStatus.dwMemoryLoad;
        }
        return 0;
    }

    #region Win32 API

    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    #endregion
}
