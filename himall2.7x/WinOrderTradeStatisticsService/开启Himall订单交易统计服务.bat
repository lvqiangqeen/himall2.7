@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinOrderTradeStatisticsService.exe"
net start Himall订单交易统计服务
pause
@pause



