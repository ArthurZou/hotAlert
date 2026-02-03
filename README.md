# HotAlert - CPU/Memory Resource Monitor with Border Warnings

<p align="center">
  <strong>A Windows desktop application that displays colored border warnings when CPU or memory usage exceeds thresholds</strong>
</p>

## 📋 Overview

HotAlert is a Windows desktop CPU/memory monitoring tool that displays prominent colored border warnings around your screen(s) when resource usage exceeds user-defined thresholds. Inspired by navigation app speed warnings, it provides a clear visual alert without interrupting your workflow.

**Core Concept**: When CPU or memory usage crosses the configured threshold, colored borders appear on all edges of all monitors, with width proportional to the severity of over-usage.

## ✨ Key Features

### 🖥️ **Multi-Monitor Support**
- Displays warning borders on **all connected monitors** simultaneously
- Automatically detects monitor changes (add/remove)
- Supports high DPI scaling with DPI-aware border width calculation

### 🎨 **Visual Warning System**
- **Dynamic Border Width**: Borders grow wider as usage increases beyond threshold
  - Formula: `width = minWidth + (usage - threshold) / (100 - threshold) × (maxWidth - minWidth)`
- **Breathing Animation**: Gentle pulsing effect (opacity 0.7-1.0 cycle)
- **Gradient Borders**: Smooth color gradient from edge to transparent interior
- **Color-Coded Alerts**:
  - CPU warnings: Red (default #FF4444)
  - Memory warnings: Orange (default #FF8C00)
  - Both resources over threshold: Both colors displayed simultaneously

### 🖱️ **Interactive Features**
- **Hover Information**: Mouse over borders to see exact usage percentages
- **Manual Dismissal**: Click system tray icon to dismiss current warnings
- **Persistent Warnings**: Borders remain visible until manually dismissed (even if usage drops below threshold)
- **Re-triggering**: Warnings reappear if thresholds are exceeded again after dismissal

### ⚙️ **Customizable Settings**
- **Threshold Configuration**: Separate sliders for CPU (0-100%) and memory (0-100%)
- **Border Customization**: Adjust minimum and maximum border widths (pixels)
- **Color Picker**: Customize warning colors for CPU and memory
- **Breathing Speed**: Adjust animation speed (Slow/Medium/Fast)
- **Auto-Start**: Option to launch on Windows startup
- **Language Support**: Switch between Chinese and English

### 📊 **System Integration**
- **System Tray Icon**: Always-visible tray icon with status indication (normal/warning)
- **Right-Click Menu**:
  - Dismiss Warning (when active)
  - Open Settings
  - Switch Language
  - Toggle Auto-Start
  - Exit Application
- **Configuration Persistence**: Automatic JSON configuration file storage

## 🚀 Installation & Usage

### **Portable Version (Recommended)**
1. Download the latest `HotAlert.exe` from [Releases](#) (or build from source)
2. Place the executable in any directory
3. Double-click to run
4. Configuration is automatically created at `%AppData%\HotAlert\config.json`

### **Installer Version**
1. Run the Inno Setup installer (`HotAlert_Setup.exe`)
2. Follow the installation wizard
3. Options include:
   - Desktop shortcut creation
   - Start menu entry
   - Auto-start registration
   - Post-installation launch

### **Running the Application**
- On first run, the application minimizes to system tray
- Right-click the tray icon to access settings and controls
- Default thresholds: CPU 80%, Memory 80%
- Borders appear automatically when thresholds are exceeded

## 🔧 Building from Source

### **Prerequisites**
- .NET 8.0 SDK or later
- Windows 10/11
- Visual Studio 2022 or VS Code (optional)

### **Build Commands**
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

### **Publishing**
```bash
# Using the publish script (recommended)
publish.cmd

# Manual publish command
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:PublishReadyToRun=true
```

**Output Locations:**
- Portable version: `publish/portable/HotAlert.exe` (single file, ~75.6 MB)
- Full version: `publish/win-x64/` (includes all dependency files)
- Installer: Generated via `installer/HotAlert.iss` (Inno Setup)

## ⚙️ Configuration

### **Configuration File**
Location: `%AppData%\HotAlert\config.json`

Default Configuration:
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

### **Settings Window**
Access via tray icon right-click menu → "Settings"
- Most settings take effect immediately without restart
- Real-time preview of border appearance changes
- Language switching requires application restart

## 🏗️ Architecture

### **Technology Stack**
- **Framework**: C# WPF (.NET 8.0-windows)
- **Platform**: Windows 10/11
- **UI Style**: Windows native style, follows system theme
- **Architecture**: MVVM (Model-View-ViewModel)
- **Configuration**: JSON (System.Text.Json)
- **System Monitoring**: PerformanceCounter API + Win32 API

### **Key Components**
| Component | Purpose |
|-----------|---------|
| `ResourceMonitor` | Monitors CPU/memory usage with 3-second sampling |
| `AlertService` | Manages warning states and border display logic |
| `ConfigService` | Handles JSON configuration persistence |
| `BorderOverlayWindow` | Transparent top-most window for border display |
| `TrayService` | System tray icon and menu management |
| `AutoStartService` | Registry-based startup configuration |
| `LocalizationService` | Chinese/English language switching |
| `ScreenHelper` | Multi-monitor detection and DPI scaling |

## 🎯 Performance Targets

| Metric | Target Value |
|--------|--------------|
| Memory Usage | < 100 MB |
| CPU Usage (idle) | < 1% |
| Startup Time | < 2 seconds |
| Monitoring Interval | 3 seconds |

## 📁 Project Structure

```
HotAlert/
├── src/HotAlert/                      # Main application
│   ├── Models/                        # Data models (AppConfig, AlertState, etc.)
│   ├── Services/                      # Business services (monitoring, config, alerts)
│   ├── ViewModels/                    # MVVM ViewModels
│   ├── Views/                         # WPF windows (Settings, BorderOverlay)
│   ├── Helpers/                       # Utility classes (ScreenHelper, TranslationSource)
│   ├── Converters/                    # WPF value converters
│   └── Resources/                     # Localization resources
├── installer/                         # Inno Setup installer configuration
├── publish/                           # Build output directories
├── scripts/                           # Utility scripts
├── tests/                             # Test projects (to be implemented)
├── SPEC.md                            # Product specification document
├── CLAUDE.md                          # Claude Code development guide
├── milestone.md                       # Development milestone tracking
└── publish.cmd                        # Publish script
```

## 📊 Project Status

### **Completed Milestones**
1. ✓ Project foundation (M1)
2. ✓ Core resource monitoring (M2)
3. ✓ Border warning display (M3)
4. ✓ System tray functionality (M4)
5. ✓ Settings interface (M5)
6. ✓ Multi-language support (M6 partially)

### **Development Progress**
Core features are fully implemented:
- Multi-monitor warnings
- System tray with context menu
- Settings interface with real-time preview
- Chinese/English language switching
- Configuration persistence

### **Planned Enhancements**
- [ ] Comprehensive test suite
- [ ] Performance optimization verification
- [ ] Installer package refinement
- [ ] Additional language support
- [ ] Theme customization options

## ❓ Frequently Asked Questions

### **Q: Does HotAlert require administrator privileges?**
**A:** No, it runs without elevated permissions using standard Windows APIs.

### **Q: Can I use HotAlert with fullscreen applications/games?**
**A:** Yes, borders display on top of fullscreen applications (Topmost window property).

### **Q: What happens when I disconnect/connect a monitor?**
**A:** HotAlert automatically detects display changes and adjusts border positioning accordingly.

### **Q: Can I monitor other resources like disk or network?**
**A:** Not currently. HotAlert focuses on CPU and memory monitoring only.

### **Q: Does the application make any sounds?**
**A:** No, HotAlert is purely visual. No audio alerts are generated.

## 🔒 Privacy & Security

- **No Data Collection**: HotAlert does not collect or transmit any usage data
- **Local Configuration**: All settings stored locally in `%AppData%\HotAlert\`
- **No Internet Access**: The application does not require network connectivity
- **Open Source**: Full source code available for inspection

## 🤝 Contributing

While this project was AI-generated as a Claude Code capability demonstration, suggestions and improvements are welcome:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

Please ensure any contributions align with the project's scope and architecture.

## 📄 License

This project is provided for educational and demonstration purposes. All code was generated by AI (Claude) as a test of Claude Code capabilities.

---

**HotAlert** - Keep your system resources in check with clear visual warnings!
