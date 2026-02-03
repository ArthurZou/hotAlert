@echo off
echo HotAlert 发布脚本
echo.

REM 设置变量
set PROJECT_PATH=src\HotAlert\HotAlert.csproj
set OUTPUT_DIR=publish
set CONFIGURATION=Release
set RUNTIME=win-x64

REM 清理旧版本
echo 清理旧版本...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

REM 发布应用
echo 发布应用 (%CONFIGURATION% - %RUNTIME%)...
dotnet publish "%PROJECT_PATH%" ^
    -c %CONFIGURATION% ^
    -r %RUNTIME% ^
    --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:PublishReadyToRun=true ^
    -p:DebugType=none ^
    -p:DebugSymbols=false ^
    --output "%OUTPUT_DIR%\%RUNTIME%"

if errorlevel 1 (
    echo 发布失败！
    pause
    exit /b 1
)

REM 复制到便携版目录
echo 创建便携版...
mkdir "%OUTPUT_DIR%\portable"
copy "%OUTPUT_DIR%\%RUNTIME%\HotAlert.exe" "%OUTPUT_DIR%\portable\HotAlert.exe"
copy "%OUTPUT_DIR%\%RUNTIME%\*.dll" "%OUTPUT_DIR%\portable\" 2>nul

echo.
echo 发布完成！
echo 可执行文件位置: %OUTPUT_DIR%\portable\HotAlert.exe
echo 完整发布位置: %OUTPUT_DIR%\%RUNTIME%\
echo.
pause