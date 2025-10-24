@echo off
echo ====================================
echo   STARTING TRAVEL API BACKEND
echo ====================================
echo.

cd /d "%~dp0project\project"

echo Checking .NET SDK...
dotnet --version
echo.

echo Starting backend server...
echo Server will be available at:
echo   - HTTPS: https://localhost:7078
echo   - HTTP: http://localhost:5078
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run

pause
