@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinOrderService.exe"
net start Himall订单处理服务
pause
@pause



