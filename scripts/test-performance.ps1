# HotAlert 性能测试脚本
# 验证内存占用 < 100 MB，CPU 占用 < 1%（空闲时），启动时间 < 2 秒

param(
    [string]$HotAlertPath = "src\HotAlert\bin\Release\net8.0-windows\win-x64\HotAlert.exe"
)

Write-Host "HotAlert 性能测试" -ForegroundColor Green
Write-Host "================================"

# 检查文件是否存在
if (-not (Test-Path $HotAlertPath)) {
    Write-Host "错误: 找不到 HotAlert.exe" -ForegroundColor Red
    Write-Host "请先运行: dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true" -ForegroundColor Yellow
    exit 1
}

Write-Host "测试 1: 启动时间 (< 2 秒)"
$startTime = Get-Date
$process = Start-Process $HotAlertPath -PassThru -WindowStyle Hidden
Start-Sleep -Seconds 3  # 等待应用完全启动
$endTime = Get-Date
$startupTime = ($endTime - $startTime).TotalSeconds

if ($startupTime -lt 2) {
    Write-Host "✓ 启动时间: $startupTime 秒" -ForegroundColor Green
} else {
    Write-Host "✗ 启动时间: $startupTime 秒 (超过 2 秒)" -ForegroundColor Red
}

Write-Host "`n测试 2: 内存占用 (< 100 MB)"
# 获取进程内存使用情况
Start-Sleep -Seconds 2
$process.Refresh()
$memoryMB = [math]::Round($process.WorkingSet64 / 1MB, 2)

if ($memoryMB -lt 100) {
    Write-Host "✓ 内存占用: $memoryMB MB" -ForegroundColor Green
} else {
    Write-Host "✗ 内存占用: $memoryMB MB (超过 100 MB)" -ForegroundColor Red
}

Write-Host "`n测试 3: CPU 占用 (< 1% 空闲时)"
# 等待几秒让应用稳定
Start-Sleep -Seconds 2

# 获取 CPU 使用率（近似值）
$cpuBefore = (Get-Counter "\Process($($process.ProcessName))\% Processor Time").CounterSamples.CookedValue
Start-Sleep -Seconds 1
$cpuAfter = (Get-Counter "\Process($($process.ProcessName))\% Processor Time").CounterSamples.CookedValue
$cpuUsage = [math]::Round($cpuAfter - $cpuBefore, 2)

if ($cpuUsage -lt 1) {
    Write-Host "✓ CPU 占用: $cpuUsage%" -ForegroundColor Green
} else {
    Write-Host "✗ CPU 占用: $cpuUsage% (超过 1%)" -ForegroundColor Red
}

Write-Host "`n测试 4: 多语言功能"
# 检查是否可以切换语言（通过配置文件）
$appDataPath = "$env:APPDATA\HotAlert"
$configPath = Join-Path $appDataPath "config.json"

if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    if ($config.Language -in @("zh-CN", "en-US")) {
        Write-Host "✓ 多语言配置正常: $($config.Language)" -ForegroundColor Green
    } else {
        Write-Host "⚠ 语言配置异常: $($config.Language)" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ 配置文件不存在" -ForegroundColor Yellow
}

# 清理：结束进程
Write-Host "`n清理测试进程..."
Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue

Write-Host "`n================================"
Write-Host "测试完成" -ForegroundColor Cyan
Write-Host "启动时间: $startupTime 秒"
Write-Host "内存占用: $memoryMB MB"
Write-Host "CPU 占用: $cpuUsage%"

# 总体评估
$allPassed = $startupTime -lt 2 -and $memoryMB -lt 100 -and $cpuUsage -lt 1
if ($allPassed) {
    Write-Host "`n✅ 所有性能测试通过！" -ForegroundColor Green
} else {
    Write-Host "`n❌ 部分测试未通过" -ForegroundColor Red
    exit 1
}