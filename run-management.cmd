@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
for %%I in ("%SCRIPT_DIR%.") do set "REPO_ROOT=%%~fI"

set "API_PROJECT=%REPO_ROOT%\management\src\ChadGreen.Management.Api"
set "CLIENT_PROJECT=%REPO_ROOT%\management\src\ChadGreen.Management.Client"
set "SITE_PACKAGE_JSON=%REPO_ROOT%\package.json"

where dotnet >nul 2>&1
if errorlevel 1 (
  echo [ERROR] dotnet was not found on PATH.
  exit /b 1
)

where npm >nul 2>&1
if errorlevel 1 (
  echo [ERROR] npm was not found on PATH.
  exit /b 1
)

if not exist "%API_PROJECT%\*.csproj" (
  echo [ERROR] API project not found at "%API_PROJECT%".
  exit /b 1
)

if not exist "%CLIENT_PROJECT%\*.csproj" (
  echo [ERROR] Client project not found at "%CLIENT_PROJECT%".
  exit /b 1
)

if not exist "%SITE_PACKAGE_JSON%" (
  echo [ERROR] Site package.json not found at "%SITE_PACKAGE_JSON%".
  exit /b 1
)

if /I "%~1"=="--check" (
  echo [OK] dotnet found.
  echo [OK] npm found.
  echo [OK] API project found: %API_PROJECT%
  echo [OK] Client project found: %CLIENT_PROJECT%
  echo [OK] Site package found: %SITE_PACKAGE_JSON%
  echo [OK] Launch target: Management API
  echo [OK] Launch target: Management Client
  echo [OK] Launch target: Astro Site
  echo [OK] Launcher check passed.
  exit /b 0
)

echo [INFO] Building management solution...
dotnet build "%REPO_ROOT%\management\ChadGreen.Management.slnx"
if errorlevel 1 (
  echo [ERROR] Management solution build failed.
  exit /b 1
)

start "Management API" cmd /k "cd /d ""%REPO_ROOT%"" && dotnet run --project .\management\src\ChadGreen.Management.Api --launch-profile http --no-build"
start "Management Client" cmd /k "cd /d ""%REPO_ROOT%"" && dotnet run --project .\management\src\ChadGreen.Management.Client --launch-profile http --no-build"
start "Astro Site" cmd /k "cd /d ""%REPO_ROOT%"" && npm run dev"

echo Started Management API, Management Client, and Astro Site in separate terminal windows.
exit /b 0
