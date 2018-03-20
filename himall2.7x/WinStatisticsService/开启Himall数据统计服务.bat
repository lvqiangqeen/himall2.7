@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinStatisticsService.exe"
net start Himall数据统计服务
pause
@pause




