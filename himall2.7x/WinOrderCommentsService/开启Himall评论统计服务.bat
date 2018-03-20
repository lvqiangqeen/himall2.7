@echo off
cd /d %~dp0
C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe -i "bin/Debug/WinOrderCommentsService.exe"
net start Himall评论统计服务
pause
@pause



