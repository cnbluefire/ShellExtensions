@echo off

call ExplorerContextMenu.Build.bat
if %ERRORLEVEL% NEQ 0 goto EXIT

dotnet restore .\ExplorerContextMenu\ExplorerContextMenu.Pack\ExplorerContextMenu.Pack.csproj
if %ERRORLEVEL% NEQ 0 goto EXIT
dotnet pack .\ExplorerContextMenu\ExplorerContextMenu.Pack\ExplorerContextMenu.Pack.csproj --no-build -o .\nupkgs\
if %ERRORLEVEL% NEQ 0 goto EXIT

:EXIT
if %ERRORLEVEL% NEQ 0 pause
exit /b %ERRORLEVEL%