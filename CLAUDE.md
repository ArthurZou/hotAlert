# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HotAlert is a Windows desktop CPU/memory monitoring tool that displays colored border warnings around the screen when resource usage exceeds thresholds (similar to navigation app speed warnings).

## Technology Stack

- **Framework**: C# WPF (.NET 6+)
- **Platform**: Windows 10/11
- **UI Style**: Windows native style, follows system theme

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
├── src/HotAlert/           # Main application
│   ├── Views/              # WPF windows (SettingsWindow, BorderOverlay)
│   ├── ViewModels/         # MVVM view models
│   ├── Services/           # Core services
│   │   ├── ResourceMonitor.cs   # CPU/memory monitoring (3s interval)
│   │   ├── AlertService.cs      # Warning border management
│   │   └── ConfigService.cs     # Configuration persistence
│   ├── Models/             # Data models (AppConfig)
│   └── Resources/          # Localization (zh-CN, en-US)
└── src/HotAlert.Installer/ # Installation package project
```

### Key Components

1. **ResourceMonitor**: Uses PerformanceCounter to sample CPU/memory every 3 seconds
2. **BorderOverlay**: Transparent topmost windows on all monitors showing gradient warning borders
3. **AlertService**: Manages warning state, border width calculation: `width = minWidth + (currentValue - threshold) / (100 - threshold) × (maxWidth - minWidth)`
4. **System Tray**: Application runs minimized to tray with context menu

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