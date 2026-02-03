using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Windows;
using System.Windows.Markup;
using HotAlert.Models;

namespace HotAlert.Services;

/// <summary>
/// 本地化服务，管理多语言支持
/// </summary>
public class LocalizationService
{
    private readonly ConfigService _configService;
    private readonly ResourceManager _resourceManager;
    private string _currentLanguage;
    private CultureInfo _currentCulture;

    /// <summary>
    /// 语言变更事件
    /// </summary>
    public event EventHandler? LanguageChanged;

    /// <summary>
    /// 当前语言
    /// </summary>
    public string CurrentLanguage => _currentLanguage;

    /// <summary>
    /// 支持的语言列表
    /// </summary>
    public List<LanguageOption> SupportedLanguages { get; } = new()
    {
        new LanguageOption("zh-CN", "中文"),
        new LanguageOption("en-US", "English")
    };

    /// <summary>
    /// 初始化本地化服务
    /// </summary>
    public LocalizationService(ConfigService configService)
    {
        _configService = configService ?? throw new ArgumentNullException(nameof(configService));

        // 初始化资源管理器（使用单一的 ResourceManager）
        var assembly = typeof(LocalizationService).Assembly;
        _resourceManager = new ResourceManager("HotAlert.Resources.Strings", assembly);

        // 从配置加载当前语言
        _currentLanguage = _configService.Config.Language;
        _currentCulture = GetCultureInfo(_currentLanguage);
    }

    /// <summary>
    /// 获取语言对应的 CultureInfo
    /// </summary>
    private static CultureInfo GetCultureInfo(string language)
    {
        return language switch
        {
            "en-US" => new CultureInfo("en-US"),
            _ => new CultureInfo("zh-CN")
        };
    }

    /// <summary>
    /// 设置当前语言
    /// </summary>
    public void SetLanguage(string language)
    {
        if (string.IsNullOrEmpty(language))
        {
            language = "zh-CN";
        }

        // 验证语言是否支持
        if (language != "zh-CN" && language != "en-US")
        {
            language = "zh-CN"; // 回退到中文
        }

        if (_currentLanguage != language)
        {
            _currentLanguage = language;
            _currentCulture = GetCultureInfo(language);

            // 保存到配置
            _configService.Update(config => config.Language = language);

            // 触发语言变更事件
            LanguageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 获取本地化字符串
    /// </summary>
    public string GetString(string key)
    {
        try
        {
            var value = _resourceManager.GetString(key, _currentCulture);
            return value ?? $"#{key}";
        }
        catch
        {
            return $"#{key}";
        }
    }

    /// <summary>
    /// 更新界面语言（用于 WPF 界面刷新）
    /// </summary>
    public void UpdateCulture()
    {
        var culture = _currentLanguage switch
        {
            "en-US" => new CultureInfo("en-US"),
            _ => new CultureInfo("zh-CN")
        };

        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;

        // 设置 FrameworkElement 的语言属性
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.Name)));
    }
}

/// <summary>
/// 语言选项
/// </summary>
public class LanguageOption
{
    public string Value { get; }
    public string DisplayName { get; }

    public LanguageOption(string value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }
}
