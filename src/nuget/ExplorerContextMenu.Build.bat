@echo off
set DOTNET_CLI_UI_LANGUAGE=en
set VSLANG=en

set WORKDIR=%~dp0
set NUGET_ROOT=%WORKDIR%ExplorerContextMenu\
set PROJECTS_PARENT_DIR=%WORKDIR%..\ExplorerContextMenu\
set DLL_PROJECT_DIR=%PROJECTS_PARENT_DIR%ExplorerContextMenu\
set DLL_PROJECT_FILE=%DLL_PROJECT_DIR%ExplorerContextMenu.csproj
set RESHELPER_PROJECT_DIR=%PROJECTS_PARENT_DIR%ExplorerContextMenu.ResourceHelper\
set RESHELPER_PROJECT_FILE=%RESHELPER_PROJECT_DIR%ExplorerContextMenu.ResourceHelper.csproj

set BUILD_FILES=%WORKDIR%BuildFiles\

rmdir /s /q %NUGET_ROOT%tools\x86 >nul 2>nul
rmdir /s /q %NUGET_ROOT%tools\x64 >nul 2>nul
rmdir /s /q %NUGET_ROOT%tools\arm64 >nul 2>nul
del /f /q %NUGET_ROOT%tools\reshelper.exe

mkdir %NUGET_ROOT%tools\x86
mkdir %NUGET_ROOT%tools\x64
mkdir %NUGET_ROOT%tools\arm64

rmdir /s /q %BUILD_FILES% >nul 2>nul
dotnet publish %DLL_PROJECT_FILE% -c Release -r win-x86 -o %BUILD_FILES%
if %ERRORLEVEL% NEQ 0 goto EXIT
xcopy %BUILD_FILES%ExplorerContextMenu.dll %NUGET_ROOT%tools\x86\
if %ERRORLEVEL% NEQ 0 goto EXIT

rmdir /s /q %BUILD_FILES% >nul 2>nul
dotnet publish %DLL_PROJECT_FILE% -c Release -r win-x64 -o %BUILD_FILES%
if %ERRORLEVEL% NEQ 0 goto EXIT
xcopy %BUILD_FILES%ExplorerContextMenu.dll %NUGET_ROOT%tools\x64\
if %ERRORLEVEL% NEQ 0 goto EXIT

rmdir /s /q %BUILD_FILES% >nul 2>nul
dotnet publish %DLL_PROJECT_FILE% -c Release -r win-arm64 -o %BUILD_FILES%
if %ERRORLEVEL% NEQ 0 goto EXIT
xcopy %BUILD_FILES%ExplorerContextMenu.dll %NUGET_ROOT%tools\arm64\
if %ERRORLEVEL% NEQ 0 goto EXIT

rmdir /s /q %BUILD_FILES% >nul 2>nul
dotnet publish %RESHELPER_PROJECT_FILE% -c Release -r win-x86 -o %BUILD_FILES%
if %ERRORLEVEL% NEQ 0 goto EXIT
echo F | xcopy %BUILD_FILES%ExplorerContextMenu.ResourceHelper.exe %NUGET_ROOT%tools\reshelper.exe >nul 2>nul
if %ERRORLEVEL% NEQ 0 goto EXIT

:EXIT
rmdir /s /q %BUILD_FILES% >nul 2>nul
if %ERRORLEVEL% NEQ 0 pause
exit /b %ERRORLEVEL%
