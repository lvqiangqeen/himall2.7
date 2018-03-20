@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinBrokerageService.exe"
net start Himall佣金处理服务
pause
@pause




