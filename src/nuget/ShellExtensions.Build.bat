@echo off
set DOTNET_CLI_UI_LANGUAGE=en
set VSLANG=en

set WORKDIR=%~dp0
set NUGET_ROOT=%WORKDIR%ShellExtensions\
set DLL_PROJECT_DIR=%WORKDIR%..\ShellExtensions\
set DLL_PROJECT_FILE=%DLL_PROJECT_DIR%ShellExtensions.csproj

set BUILD_FILES=%WORKDIR%BuildFiles\

rmdir /s /q %NUGET_ROOT%lib >nul 2>nul

mkdir %NUGET_ROOT%lib\net8.0-windows7.0\

rmdir /s /q %BUILD_FILES% >nul 2>nul
dotnet publish %DLL_PROJECT_FILE% -c Release -p:Platform=AnyCPU -o %BUILD_FILES%
if %ERRORLEVEL% NEQ 0 goto EXIT
echo F | xcopy %BUILD_FILES%ShellExtensions.dll %NUGET_ROOT%lib\net8.0-windows7.0\
if %ERRORLEVEL% NEQ 0 goto EXIT

:EXIT
rmdir /s /q %BUILD_FILES% >nul 2>nul
if %ERRORLEVEL% NEQ 0 pause
exit /b %ERRORLEVEL%
