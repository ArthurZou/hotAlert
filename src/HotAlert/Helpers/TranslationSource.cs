using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using HotAlert.Services;

namespace HotAlert.Helpers;

/// <summary>
/// 本地化字符串绑定源，支持动态刷新
/// </summary>
public class TranslationSource : INotifyPropertyChanged
{
    private static readonly Lazy<TranslationSource> _instance = new(() => new TranslationSource());
    private LocalizationService? _localizationService;

    public static TranslationSource Instance => _instance.Value;

    /// <summary>
    /// 初始化本地化服务
    /// </summary>
    public void Initialize(LocalizationService service)
    {
        _localizationService = service;

        // 订阅语言变更事件
        _localizationService.LanguageChanged += (sender, e) =>
        {
            OnPropertyChanged(string.Empty); // 空字符串表示所有属性都变更
        };
    }

    /// <summary>
    /// 索引器，用于 XAML 绑定
    /// </summary>
    public string this[string key]
    {
        get
        {
            if (_localizationService == null)
            {
                // 如果服务未初始化，返回键值
                return $"#{key}";
            }

            return _localizationService.GetString(key);
        }
    }

    /// <summary>
    /// 属性变更事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// XAML 标记扩展，简化本地化字符串绑定语法
/// </summary>
[MarkupExtensionReturnType(typeof(string))]
public class LocExtension : MarkupExtension
{
    /// <summary>
    /// 资源键值
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 初始化本地化扩展
    /// </summary>
    public LocExtension()
    {
    }

    /// <summary>
    /// 初始化本地化扩展
    /// </summary>
    public LocExtension(string key)
    {
        Key = key;
    }

    /// <summary>
    /// 提供绑定值
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrEmpty(Key))
        {
            return string.Empty;
        }

        // 创建绑定
        var binding = new System.Windows.Data.Binding($"[{Key}]")
        {
            Source = TranslationSource.Instance,
            Mode = BindingMode.OneWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };

        // 获取绑定目标
        var target = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget))!;
        var targetObject = target.TargetObject as DependencyObject;
        var targetProperty = target.TargetProperty as DependencyProperty;

        if (targetObject == null || targetProperty == null)
        {
            // 如果无法创建绑定，返回当前值
            return TranslationSource.Instance[Key];
        }

        // 返回绑定
        return binding.ProvideValue(serviceProvider);
    }
}
