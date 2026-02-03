**用于测试claude code能力的小项目，没有c#/windows开发经验，所有代码100%都是ai生成。**

# HotAlert - CPU/内存资源监控边框警告工具

<p align="center">
  <strong>一款当 CPU 或内存使用率超过阈值时显示彩色边框警告的 Windows 桌面应用</strong>
</p>

<p align="center">
  <a href="README.en.md">English</a> | <strong>简体中文</strong>
</p>

## 📋 概述

HotAlert 是一款 Windows 桌面 CPU/内存监控工具，当资源使用率超过用户设定的阈值时，会在屏幕四周显示醒目的彩色边框警告。灵感来源于导航应用的超速警告，它能在不打断你工作流程的情况下提供清晰的视觉提醒。

**核心概念**：当 CPU 或内存使用率超过设定阈值时，所有显示器的四边都会出现彩色边框，边框宽度与超限程度成正比。

## ✨ 主要功能

### 🖥️ **多显示器支持**
- 在**所有已连接的显示器**上同时显示警告边框
- 自动检测显示器变化（添加/移除）
- 支持高 DPI 缩放，边框宽度自适应计算

### 🎨 **视觉警告系统**
- **动态边框宽度**：使用率越高，边框越宽
  - 计算公式：`宽度 = 最小宽度 + (使用率 - 阈值) / (100 - 阈值) × (最大宽度 - 最小宽度)`
- **呼吸灯动画**：柔和的脉冲效果（透明度 0.7-1.0 循环）
- **渐变边框**：从边缘到内部平滑过渡的颜色渐变
- **颜色区分**：
  - CPU 警告：红色（默认 #FF4444）
  - 内存警告：橙色（默认 #FF8C00）
  - 两者同时超标：同时显示两种颜色

### 🖱️ **交互功能**
- **悬停信息**：鼠标悬停在边框上可查看精确的使用率百分比
- **手动关闭**：点击系统托盘图标关闭当前警告
- **持续警告**：边框会一直显示直到手动关闭（即使使用率降至阈值以下）
- **重新触发**：关闭警告后，如果再次超过阈值会重新显示警告

### ⚙️ **自定义设置**
- **阈值配置**：CPU 和内存分别设置（0-100%）
- **边框自定义**：调整最小和最大边框宽度（像素）
- **颜色选择器**：自定义 CPU 和内存的警告颜色
- **呼吸灯速度**：调整动画速度（慢速/中速/快速）
- **开机自启**：可选择开机自动启动
- **多语言支持**：支持中英文切换

### 📊 **系统集成**
- **系统托盘图标**：常驻托盘图标，显示状态（正常/警告）
- **右键菜单**：
  - 关闭警告（警告激活时）
  - 打开设置
  - 切换语言
  - 开机自启开关
  - 退出应用
- **配置持久化**：自动保存 JSON 配置文件

## 🚀 安装与使用

### **便携版（推荐）**
1. 从 [Releases](https://github.com/ArthurZou/hotAlert/releases) 下载最新的 `HotAlert.exe`（或从源码构建）
2. 将可执行文件放在任意目录
3. 双击运行
4. 配置文件自动创建在 `%AppData%\HotAlert\config.json`

### **安装包版本**
1. 运行 Inno Setup 安装程序（[HotAlert_Setup.exe](https://github.com/ArthurZou/hotAlert/releases)）
2. 按照安装向导操作
3. 可选项包括：
   - 创建桌面快捷方式
   - 添加开始菜单项
   - 注册开机自启
   - 安装后启动

### **运行应用**
- 首次运行时，应用会最小化到系统托盘
- 右键点击托盘图标访问设置和控制选项
- 默认阈值：CPU 80%，内存 80%
- 超过阈值时边框自动出现

## 🔧 从源码构建

### **前置要求**
- .NET 8.0 SDK 或更高版本
- Windows 10/11
- Visual Studio 2022 或 VS Code（可选）

### **构建命令**
```bash
# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行应用
dotnet run --project src/HotAlert/HotAlert.csproj

# 构建发布版本（单文件）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 运行测试
dotnet test
```

### **发布**
```bash
# 使用发布脚本（推荐）
publish.cmd

# 手动发布命令
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:PublishReadyToRun=true
```

**输出位置：**
- 便携版：`publish/portable/HotAlert.exe`（单文件，约 75.6 MB）
- 完整版：`publish/win-x64/`（包含所有依赖文件）
- 安装包：通过 `installer/HotAlert.iss`（Inno Setup）生成

## ⚙️ 配置说明

### **配置文件**
位置：`%AppData%\HotAlert\config.json`

默认配置：
```json
{
  "cpuThreshold": 80,
  "memoryThreshold": 80,
  "borderMinWidth": 10,
  "borderMaxWidth": 50,
  "cpuColor": "#FF4444",
  "memoryColor": "#FF8C00",
  "breathSpeed": "medium",
  "autoStart": true,
  "language": "zh-CN"
}
```

### **设置界面**
通过托盘图标右键菜单 → "设置" 打开
- 大多数设置无需重启即可生效
- 边框外观更改支持实时预览
- 语言切换需要重启应用

## 🏗️ 架构设计

### **技术栈**
- **框架**：C# WPF (.NET 8.0-windows)
- **平台**：Windows 10/11
- **UI 风格**：Windows 原生风格，跟随系统主题
- **架构模式**：MVVM (Model-View-ViewModel)
- **配置存储**：JSON (System.Text.Json)
- **系统监控**：PerformanceCounter API + Win32 API

### **核心组件**
| 组件 | 功能 |
|------|------|
| `ResourceMonitor` | 监控 CPU/内存使用率，3 秒采样间隔 |
| `AlertService` | 管理警告状态和边框显示逻辑 |
| `ConfigService` | 处理 JSON 配置持久化 |
| `BorderOverlayWindow` | 用于显示边框的透明置顶窗口 |
| `TrayService` | 系统托盘图标和菜单管理 |
| `AutoStartService` | 基于注册表的开机自启配置 |
| `LocalizationService` | 中英文语言切换 |
| `ScreenHelper` | 多显示器检测和 DPI 缩放 |

## 🎯 性能指标

| 指标 | 目标值 |
|------|--------|
| 内存占用 | < 100 MB |
| CPU 占用（空闲时） | < 1% |
| 启动时间 | < 2 秒 |
| 监控间隔 | 3 秒 |

## 📁 项目结构

```
HotAlert/
├── src/HotAlert/                      # 主应用程序
│   ├── Models/                        # 数据模型（AppConfig、AlertState 等）
│   ├── Services/                      # 业务服务（监控、配置、警告）
│   ├── ViewModels/                    # MVVM ViewModels
│   ├── Views/                         # WPF 窗口（设置、边框覆盖层）
│   ├── Helpers/                       # 工具类（ScreenHelper、TranslationSource）
│   ├── Converters/                    # WPF 值转换器
│   └── Resources/                     # 本地化资源
├── installer/                         # Inno Setup 安装包配置
├── publish/                           # 构建输出目录
├── scripts/                           # 工具脚本
├── tests/                             # 测试项目（待实现）
├── SPEC.md                            # 产品规格文档
├── CLAUDE.md                          # Claude Code 开发指南
├── milestone.md                       # 开发里程碑跟踪
└── publish.cmd                        # 发布脚本
```

## 📊 项目状态

### **已完成的里程碑**
1. ✓ 项目基础架构 (M1)
2. ✓ 核心资源监控 (M2)
3. ✓ 边框警告显示 (M3)
4. ✓ 系统托盘功能 (M4)
5. ✓ 设置界面 (M5)
6. ✓ 多语言支持 (M6 部分)

### **开发进度**
核心功能已全部实现：
- 多显示器警告
- 带右键菜单的系统托盘
- 支持实时预览的设置界面
- 中英文切换
- 配置持久化

### **计划中的改进**
- [ ] 完善测试套件
- [ ] 性能优化验证
- [ ] 安装包完善
- [ ] 更多语言支持
- [ ] 主题自定义选项

## ❓ 常见问题

### **Q：HotAlert 需要管理员权限吗？**
**A：** 不需要，它使用标准 Windows API 运行，无需提升权限。

### **Q：HotAlert 能用于全屏应用/游戏吗？**
**A：** 可以，边框会显示在全屏应用之上（使用 Topmost 窗口属性）。

### **Q：断开/连接显示器时会怎样？**
**A：** HotAlert 会自动检测显示器变化并相应调整边框位置。

### **Q：可以监控其他资源如磁盘或网络吗？**
**A：** 目前不行。HotAlert 仅专注于 CPU 和内存监控。

### **Q：应用会发出声音吗？**
**A：** 不会，HotAlert 是纯视觉提醒，不会产生任何音频警告。

## 🔒 隐私与安全

- **无数据收集**：HotAlert 不收集或传输任何使用数据
- **本地配置**：所有设置本地存储在 `%AppData%\HotAlert\`
- **无需联网**：应用不需要网络连接
- **开源透明**：完整源代码可供查阅

## 🤝 参与贡献

虽然这个项目是作为 Claude Code 能力演示由 AI 生成的，但欢迎提出建议和改进：

1. Fork 本仓库
2. 创建功能分支
3. 进行修改
4. 提交 Pull Request

请确保任何贡献都符合项目的范围和架构。

## 📄 许可证

本项目用于教育和演示目的。所有代码均由 AI（Claude）生成，作为 Claude Code 能力的测试。

---

**HotAlert** - 用清晰的视觉警告让你的系统资源尽在掌控！
