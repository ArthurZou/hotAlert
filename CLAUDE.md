# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HotAlert is a Windows desktop CPU/memory monitoring tool that displays colored border warnings around the screen when resource usage exceeds thresholds (similar to navigation app speed warnings).

## Technology Stack

- **Framework**: C# WPF (.NET 8.0-windows)
- **Platform**: Windows 10/11
- **UI Style**: Windows native style, follows system theme
- **Architecture**: MVVM (Model-View-ViewModel)
- **Configuration**: JSON (System.Text.Json)
- **System Monitoring**: PerformanceCounter API + Win32 API

## Build and Run Commands

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project src/HotAlert/HotAlert.csproj

# Build release version (single file)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Run tests
dotnet test
```

## Architecture

### Project Structure
```
HotAlert/
├── src/HotAlert/                      # 主应用程序
│   ├── App.xaml                       # WPF 应用程序定义
│   ├── App.xaml.cs                    # 应用程序启动入口，初始化服务
│   ├── MainWindow.xaml                # 主窗口布局
│   ├── MainWindow.xaml.cs             # 主窗口代码后端
│   ├── HotAlert.csproj                # 项目配置 (.NET 8.0-windows, UseWindowsForms)
│   ├── AssemblyInfo.cs                # 程序集信息和主题定义
│   │
│   ├── Models/                        # 数据模型层
│   │   ├── AppConfig.cs               # 应用配置模型 (阈值、颜色、边框宽度等)
│   │   ├── AlertState.cs              # 警告状态模型 (AlertType枚举、边框宽度)
│   │   └── ResourceUsageEventArgs.cs  # 资源使用率事件参数 (CPU/内存/时间戳)
│   │
│   ├── Services/                      # 业务服务层
│   │   ├── ResourceMonitor.cs         # CPU/内存监控服务 (3秒采样间隔)
│   │   ├── ConfigService.cs           # 配置读写服务 (JSON持久化)
│   │   └── AlertService.cs            # 警告管理服务 (协调监控与边框显示)
│   │
│   ├── Helpers/                       # 工具类
│   │   └── ScreenHelper.cs            # 多显示器检测、DPI缩放计算
│   │
│   ├── ViewModels/                    # MVVM ViewModel层
│   │   ├── ViewModelBase.cs           # ViewModel基类 (INotifyPropertyChanged)
│   │   └── RelayCommand.cs            # 通用命令实现 (ICommand)
│   │
│   ├── Views/                         # WPF视图/窗口
│   │   ├── BorderOverlayWindow.xaml   # 边框覆盖层窗口 (透明、置顶)
│   │   └── [SettingsWindow.xaml]      # 设置窗口 (待实现)
│   │
│   └── Resources/                     # 本地化资源 (待实现)
│       ├── [Strings.zh-CN.resx]       # 中文资源文件
│       └── [Strings.en-US.resx]       # 英文资源文件
│
├── src/HotAlert.Installer/            # 安装包项目 (待实现)
├── tests/                             # 测试目录 (待实现)
├── HotAlert.sln                       # 解决方案文件
├── SPEC.md                            # 产品规格文档
├── CLAUDE.md                          # Claude Code 开发指引
└── milestone.md                       # 开发里程碑跟踪
```

### Key Components

**已实现:**
1. **ResourceMonitor**: 使用 PerformanceCounter API 采集 CPU，Win32 API (GlobalMemoryStatusEx) 采集内存，3秒采样间隔，通过 `ResourceUsageChanged` 事件通知
2. **ConfigService**: JSON 配置持久化到 `%AppData%\HotAlert\config.json`，支持加载/保存/更新/重置，配置变更触发 `ConfigChanged` 事件
3. **ViewModelBase**: MVVM 基类，实现 `INotifyPropertyChanged`，提供 `SetProperty<T>()` 方法
4. **RelayCommand**: 通用命令实现，支持 `canExecute` 条件
5. **AlertService**: 警告状态管理，订阅 ResourceMonitor 事件，计算动态边框宽度: `width = minWidth + (usage - threshold) / (100 - threshold) × (maxWidth - minWidth)`，管理 BorderOverlayWindow 窗口集合，支持 `DismissAlert()` 手动关闭警告
6. **BorderOverlayWindow**: 透明置顶窗口，四边渐变边框 (LinearGradientBrush)，呼吸灯动画 (Opacity 0.7-1.0)，鼠标悬浮显示数值提示
7. **ScreenHelper**: 使用 `System.Windows.Forms.Screen.AllScreens` 获取所有显示器，通过 Win32 API (GetDpiForMonitor) 计算 DPI 缩放，监听 `SystemEvents.DisplaySettingsChanged` 事件

**待实现:**
1. **System Tray**: 托盘图标运行，右键菜单，点击关闭警告
2. **SettingsWindow**: 设置界面，阈值/颜色/呼吸速度配置

### Configuration

- Location: `%AppData%\HotAlert\config.json`
- Defaults: 80% thresholds, 10-50px border width, red (#FF4444) for CPU, orange (#FF8C00) for memory
- Auto-start via registry: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`

### Multi-Monitor & DPI

- Border warnings display on ALL monitors simultaneously
- Borders overlay fullscreen applications (Topmost)
- DPI-aware border width scaling
- Dynamic monitor detection (add/remove)

### Warning Behavior

- Borders show breathing animation effect
- Mouse hover shows exact percentage
- Borders do NOT auto-dismiss when usage drops below threshold
- Click tray icon to manually dismiss warning
- Re-triggers if threshold exceeded again after dismissal

## Performance Targets

- Memory: < 100 MB
- CPU (idle): < 1%
- Startup: < 2 seconds

## Coding Standards

### 命名规范

| 元素 | 规范 | 示例 |
|------|------|------|
| 命名空间 | 分层 PascalCase | `HotAlert.Services` |
| 类名 | PascalCase，服务类加 `Service` 后缀 | `ConfigService`, `ViewModelBase` |
| 方法名 | PascalCase，动词开头 | `Load()`, `Save()`, `OnPropertyChanged()` |
| 属性名 | PascalCase | `Config`, `CpuThreshold` |
| 私有字段 | _camelCase（下划线前缀） | `_config`, `_execute` |
| 参数/局部变量 | camelCase | `propertyName`, `updateAction` |
| 事件 | PascalCase + Changed/Occurred | `ConfigChanged`, `PropertyChanged` |
| 接口 | I + PascalCase | `ICommand`, `INotifyPropertyChanged` |

### 文件与代码结构

- **命名空间声明**: 使用文件级声明 `namespace HotAlert.Models;`
- **类成员顺序**: 字段 → 属性 → 事件 → 构造函数 → 公有方法 → 私有方法
- **属性初始化**: 在属性声明处初始化默认值 `public int CpuThreshold { get; set; } = 80;`

### 注释规范

- **语言**: 统一使用中文注释
- **XML 文档**: 为类和公有成员添加 `<summary>` 注释
```csharp
/// <summary>
/// 应用配置模型
/// </summary>
public class AppConfig { }
```

### MVVM 模式

- **ViewModelBase**: 继承 `INotifyPropertyChanged`，提供 `SetProperty<T>()` 和 `OnPropertyChanged()` 方法
- **RelayCommand**: 实现 `ICommand`，支持 `canExecute` 条件
- **Models**: 纯数据对象，使用自动属性，不继承基类

### 事件处理

```csharp
// 声明：使用可空标记和泛型 EventHandler
public event EventHandler<AppConfig>? ConfigChanged;

// 触发：使用空条件操作符
ConfigChanged?.Invoke(this, _config);
```

### 错误处理

- 使用 `try-catch` 捕获预期异常，提供默认值回退
- 参数验证使用 `throw` 表达式: `_execute = execute ?? throw new ArgumentNullException(nameof(execute));`
- 非关键操作（如配置保存）可静默忽略异常

### 资源管理

- 需要释放资源的服务实现 `IDisposable`
- 使用 `using` 声明自动释放: `using var monitor = new ResourceMonitor();`

### 项目配置要求

- `Nullable=enable` - 启用可空引用类型
- `ImplicitUsings=enable` - 自动导入公共命名空间
- 可空类型必须标记 `?`