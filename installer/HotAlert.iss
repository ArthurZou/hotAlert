; Inno Setup 安装脚本
; HotAlert - 系统资源监控工具

#define MyAppName "HotAlert"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "HotAlert"
#define MyAppURL "https://github.com/yourusername/hotalert"
#define MyAppExeName "HotAlert.exe"
#define MySourceDir "..\publish\win-x64"

[Setup]
; 基本信息
AppId={{E3A7B8D2-4F1C-4A9B-9C6D-123456789ABC}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=..\LICENSE
InfoBeforeFile=..\README.md
OutputDir=..\installer\output
OutputBaseFilename=HotAlert-Setup
SetupIconFile=..\src\HotAlert\icon.ico
Compression=lzma2/ultra
SolidCompression=yes
WizardStyle=modern

; 多语言支持
Languages=chinesesimp,english

; 架构
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; 任务选项
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "startupicon"; Description: "{cm:AutoStartProgram,{#MyAppName}}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1

[Files]
; 主程序文件
Source: "{#MySourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MySourceDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#MySourceDir}\*.json"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; 其他资源文件（如有）
; Source: "{#MySourceDir}\*.config"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; 开始菜单图标
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"

; 桌面图标
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; 快速启动图标
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

; 开机启动图标
Name: "{commonstartup}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: startupicon

[Run]
; 安装完成后运行程序
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 卸载时删除配置文件
Type: filesandordirs; Name: "{userappdata}\HotAlert"

[Code]
// 自定义安装过程
function InitializeSetup(): Boolean;
begin
  // 检查是否已安装 .NET 8.0 运行时
  // Inno Setup 默认不会检查 .NET 运行时，因为我们的应用是自包含的
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // 安装完成后的操作
  end;
end;