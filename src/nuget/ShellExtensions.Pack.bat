@echo off

call ShellExtensions.Build.bat
if %ERRORLEVEL% NEQ 0 goto EXIT

dotnet restore .\ShellExtensions\ShellExtensions.Pack\ShellExtensions.Pack.csproj
if %ERRORLEVEL% NEQ 0 goto EXIT
dotnet pack .\ShellExtensions\ShellExtensions.Pack\ShellExtensions.Pack.csproj --no-build -o .\nupkgs\
if %ERRORLEVEL% NEQ 0 goto EXIT

:EXIT
if %ERRORLEVEL% NEQ 0 pause
exit /b %ERRORLEVEL%