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
