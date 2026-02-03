using System.Windows.Input;
using HotAlert.Services;

namespace HotAlert.ViewModels;

/// <summary>
/// 设置窗口 ViewModel
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly ConfigService _configService;

    private int _cpuThreshold;
    private int _memoryThreshold;
    private int _borderMinWidth;
    private int _borderMaxWidth;
    private string _cpuColor = "#FF4444";
    private string _memoryColor = "#FF8C00";
    private string _breathSpeed = "medium";
    private bool _autoStart;

    /// <summary>
    /// CPU 阈值 (10-100)
    /// </summary>
    public int CpuThreshold
    {
        get => _cpuThreshold;
        set
        {
            if (SetProperty(ref _cpuThreshold, Math.Clamp(value, 10, 100)))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 内存阈值 (10-100)
    /// </summary>
    public int MemoryThreshold
    {
        get => _memoryThreshold;
        set
        {
            if (SetProperty(ref _memoryThreshold, Math.Clamp(value, 10, 100)))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 边框最小宽度
    /// </summary>
    public int BorderMinWidth
    {
        get => _borderMinWidth;
        set
        {
            if (SetProperty(ref _borderMinWidth, Math.Clamp(value, 5, 30)))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 边框最大宽度
    /// </summary>
    public int BorderMaxWidth
    {
        get => _borderMaxWidth;
        set
        {
            if (SetProperty(ref _borderMaxWidth, Math.Clamp(value, 20, 100)))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// CPU 警告颜色
    /// </summary>
    public string CpuColor
    {
        get => _cpuColor;
        set
        {
            if (SetProperty(ref _cpuColor, value))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 内存警告颜色
    /// </summary>
    public string MemoryColor
    {
        get => _memoryColor;
        set
        {
            if (SetProperty(ref _memoryColor, value))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 呼吸灯速度
    /// </summary>
    public string BreathSpeed
    {
        get => _breathSpeed;
        set
        {
            if (SetProperty(ref _breathSpeed, value))
            {
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 开机自启动
    /// </summary>
    public bool AutoStart
    {
        get => _autoStart;
        set
        {
            if (SetProperty(ref _autoStart, value))
            {
                AutoStartService.SetAutoStart(value);
                ApplySettings();
            }
        }
    }

    /// <summary>
    /// 预设颜色列表
    /// </summary>
    public string[] PresetColors { get; } = new[]
    {
        // 红色系
        "#FF4444", "#FF6B6B", "#E53935",
        // 橙色系
        "#FF8C00", "#FFA726", "#FF7043",
        // 黄色系
        "#FFD600", "#FFEB3B", "#FFC107",
        // 绿色系
        "#4CAF50", "#66BB6A", "#81C784",
        // 蓝色系
        "#2196F3", "#42A5F5", "#64B5F6",
        // 紫色系
        "#9C27B0", "#AB47BC", "#BA68C8"
    };

    /// <summary>
    /// 呼吸灯速度选项
    /// </summary>
    public BreathSpeedOption[] BreathSpeedOptions { get; } = new[]
    {
        new BreathSpeedOption("slow", "慢"),
        new BreathSpeedOption("medium", "中"),
        new BreathSpeedOption("fast", "快")
    };

    /// <summary>
    /// 选择 CPU 颜色命令
    /// </summary>
    public ICommand SelectCpuColorCommand { get; }

    /// <summary>
    /// 选择内存颜色命令
    /// </summary>
    public ICommand SelectMemoryColorCommand { get; }

    /// <summary>
    /// 恢复默认设置命令
    /// </summary>
    public ICommand ResetCommand { get; }

    public SettingsViewModel(ConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        SelectCpuColorCommand = new RelayCommand(color => CpuColor = (string)color!);
        SelectMemoryColorCommand = new RelayCommand(color => MemoryColor = (string)color!);
        ResetCommand = new RelayCommand(Reset);
    }

    /// <summary>
    /// 从配置服务加载配置
    /// </summary>
    public void LoadFromConfig()
    {
        var config = _configService.Config;

        // 直接设置字段避免触发 ApplySettings
        _cpuThreshold = config.CpuThreshold;
        _memoryThreshold = config.MemoryThreshold;
        _borderMinWidth = config.BorderMinWidth;
        _borderMaxWidth = config.BorderMaxWidth;
        _cpuColor = config.CpuColor;
        _memoryColor = config.MemoryColor;
        _breathSpeed = config.BreathSpeed;
        _autoStart = config.AutoStart;

        // 通知 UI 更新
        OnPropertyChanged(nameof(CpuThreshold));
        OnPropertyChanged(nameof(MemoryThreshold));
        OnPropertyChanged(nameof(BorderMinWidth));
        OnPropertyChanged(nameof(BorderMaxWidth));
        OnPropertyChanged(nameof(CpuColor));
        OnPropertyChanged(nameof(MemoryColor));
        OnPropertyChanged(nameof(BreathSpeed));
        OnPropertyChanged(nameof(AutoStart));
    }

    /// <summary>
    /// 应用设置（实时保存并触发预览）
    /// </summary>
    private void ApplySettings()
    {
        _configService.Update(config =>
        {
            config.CpuThreshold = _cpuThreshold;
            config.MemoryThreshold = _memoryThreshold;
            config.BorderMinWidth = _borderMinWidth;
            config.BorderMaxWidth = _borderMaxWidth;
            config.CpuColor = _cpuColor;
            config.MemoryColor = _memoryColor;
            config.BreathSpeed = _breathSpeed;
            config.AutoStart = _autoStart;
        });
    }

    /// <summary>
    /// 恢复默认设置
    /// </summary>
    private void Reset()
    {
        _configService.Reset();
        LoadFromConfig();
    }
}

/// <summary>
/// 呼吸灯速度选项
/// </summary>
public class BreathSpeedOption
{
    public string Value { get; }
    public string DisplayName { get; }

    public BreathSpeedOption(string value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }
}
