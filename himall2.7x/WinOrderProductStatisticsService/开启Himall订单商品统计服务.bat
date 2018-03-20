@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinOrderProductStatisticsService.exe"
net start Himall订单商品统计服务
pause
@pause



