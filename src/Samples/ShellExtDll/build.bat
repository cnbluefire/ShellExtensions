@echo off
set DOTNET_CLI_UI_LANGUAGE=en
set VSLANG=en

set PLATFORM=%1
set CONFIGURATION=%2

setlocal enabledelayedexpansion

set "PLATFORM_LOWER=!PLATFORM!"

for %%i in ("A=a" "B=b" "C=c" "D=d" "E=e" "F=f" "G=g" "H=h" "I=i" "J=j" "K=k" "L=l" "M=m" "N=n" "O=o" "P=p" "Q=q" "R=r" "S=s" "T=t" "U=u" "V=v" "W=w" "X=x" "Y=y" "Z=z") do (
    set "PLATFORM_LOWER=!PLATFORM_LOWER:%%~i!"
)

if /i not "!PLATFORM_LOWER!"=="x86" if /i not "!PLATFORM_LOWER!"=="x64" if /i not "!PLATFORM_LOWER!"=="arm64" (
    set PLATFORM=x86
)

if "!CONFIGURATION!"=="" (set CONFIGURATION=Release)

dotnet publish -c !CONFIGURATION! -r win-!PLATFORM! -o .\bin\output\!PLATFORM!\!CONFIGURATION!

if %ERRORLEVEL% NEQ 0 ( goto EXIT )

EXIT:
endlocal
exit /b %ERRORLEVEL%
