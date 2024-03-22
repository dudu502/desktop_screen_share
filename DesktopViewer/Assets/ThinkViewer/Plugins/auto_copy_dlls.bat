REM copy D:\dudu502\desktop_screen_share\DesktopHost\Common\bin\Debug\netstandard2.0\Common.dll D:\dudu502\desktop_screen_share\DesktopViewer\Assets\ThinkViewer\Plugins\Common.dll
REM @echo off
setlocal EnableDelayedExpansion

set "currentDirectory=%~dp0"
set "subs=DesktopViewer"

%~dp0\cpdll.exe  !currentDirectory!  !subs!
endlocal

cmd