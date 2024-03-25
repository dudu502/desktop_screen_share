REM copy D:\dudu502\desktop_screen_share\DesktopHost\Common\bin\Debug\netstandard2.0\Common.dll D:\dudu502\desktop_screen_share\DesktopViewer\Assets\ThinkViewer\Plugins\Common.dll
@echo off
setlocal EnableDelayedExpansion

set "currentDirectory=%~dp0"
set "subs=DesktopViewer"
set "filename=Common.dll"

%~dp0\cpdll.exe  !currentDirectory! !subs! !filename!
endlocal

timeout /t 10