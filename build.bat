@echo off
rem Builds build\PRPG.exe with your installed Unity, without opening a Unity window.
rem Pull the latest code first (VS Code: Source Control, then Pull), then double-click this file.
setlocal
set "VERSION=6000.6.5f1"
set "ROOT=%~dp0"
set "UNITY=C:\Program Files\Unity\Hub\Editor\%VERSION%\Editor\Unity.exe"
if not exist "%UNITY%" (
  echo Could not find Unity %VERSION% at:
  echo   %UNITY%
  echo Install that version from Unity Hub, or tell Claude where Unity is.
  goto fail
)
if not exist "%ROOT%build-logs" mkdir "%ROOT%build-logs"

echo [1/2] Preparing the project. The first time takes several minutes while Unity imports everything...
"%UNITY%" -batchmode -nographics -quit -projectPath "%ROOT%game" -executeMethod Prpg.EditorTools.BuildTools.Setup -logFile "%ROOT%build-logs\setup.log"
if errorlevel 1 (
  set "LOG=%ROOT%build-logs\setup.log"
  goto fail
)

echo [2/2] Building PRPG.exe...
"%UNITY%" -batchmode -nographics -quit -projectPath "%ROOT%game" -executeMethod Prpg.EditorTools.BuildTools.Windows -logFile "%ROOT%build-logs\build.log"
if errorlevel 1 (
  set "LOG=%ROOT%build-logs\build.log"
  goto fail
)

echo.
echo Done. Opening the build folder: run PRPG.exe.
start "" "%ROOT%build"
exit /b 0

:fail
echo.
echo BUILD FAILED.
if defined LOG (
  echo Errors from %LOG%:
  findstr /i /c:"error" "%LOG%"
  echo.
  echo Send Claude the file %LOG% if the reason is not obvious.
)
pause
exit /b 1
