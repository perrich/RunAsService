@echo off
set INSTALL_UTIL_HOME=C:\Windows\Microsoft.NET\Framework\v2.0.50727

set PATH=%PATH%;%INSTALL_UTIL_HOME%

cd %SERVICE_HOME%

echo Uninstalling Service...
installutil -u Perrich.RunAsService.exe
echo Done.