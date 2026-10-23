@echo off
chcp 65001 >nul
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [ERROR] csc.exe not found: .NET Framework 4.x is required.
    exit /b 1
)

echo Building 时薪计算器.exe ...
"%CSC%" /nologo /target:winexe /codepage:65001 /optimize+ /out:"时薪计算器.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll src\Wage.cs src\Program.cs
if errorlevel 1 (
    echo [ERROR] build failed.
    exit /b 1
)

echo Done.
exit /b 0
