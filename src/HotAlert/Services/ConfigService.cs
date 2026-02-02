using System.IO;
using System.Text.Json;
using HotAlert.Models;

namespace HotAlert.Services;

/// <summary>
/// 配置服务，负责配置的读取和保存
/// </summary>
public class ConfigService
{
    private static readonly string ConfigFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HotAlert");

    private static readonly string ConfigPath = Path.Combine(ConfigFolder, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private AppConfig _config = new();

    /// <summary>
    /// 当前配置
    /// </summary>
    public AppConfig Config => _config;

    /// <summary>
    /// 配置变更事件
    /// </summary>
    public event EventHandler<AppConfig>? ConfigChanged;

    /// <summary>
    /// 加载配置
    /// </summary>
    public AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                _config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
        }
        catch
        {
            _config = new AppConfig();
        }

        return _config;
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    public void Save()
    {
        try
        {
            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }

            var json = JsonSerializer.Serialize(_config, JsonOptions);
            File.WriteAllText(ConfigPath, json);
        }
        catch
        {
            // 忽略保存失败
        }
    }

    /// <summary>
    /// 更新配置
    /// </summary>
    public void Update(Action<AppConfig> updateAction)
    {
        updateAction(_config);
        Save();
        ConfigChanged?.Invoke(this, _config);
    }

    /// <summary>
    /// 重置为默认配置
    /// </summary>
    public void Reset()
    {
        _config = new AppConfig();
        Save();
        ConfigChanged?.Invoke(this, _config);
    }
}
