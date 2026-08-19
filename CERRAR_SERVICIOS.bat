@echo off
setlocal
title CERRAR SERVICIOS - SISTEMA CONTROL LLAMADAS
cd /d "%~dp0"

echo ============================================================
echo CERRANDO SERVICIOS DEL PROYECTO
echo ============================================================
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -Command ^
  "$puertos = 5000,6000,8111,8112,8113,8114,5155;" ^
  "$pids = @(Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue | Where-Object { $_.LocalPort -in $puertos } | Select-Object -ExpandProperty OwningProcess -Unique);" ^
  "foreach ($pidActual in $pids) {" ^
  "  $proceso = Get-Process -Id $pidActual -ErrorAction SilentlyContinue;" ^
  "  if ($proceso) { Write-Host ('Deteniendo ' + $proceso.ProcessName + ' (PID ' + $pidActual + ')...'); Stop-Process -Id $pidActual -Force -ErrorAction SilentlyContinue }" ^
  "}" ^
  "$procesosCmd = Get-CimInstance Win32_Process -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq 'cmd.exe' };" ^
  "foreach ($cmd in $procesosCmd) {" ^
  "  if ($cmd.CommandLine -and $cmd.CommandLine -match 'INICIAR_SERVICIOS|ejecutar_identificador|ejecutar_proveedor|ejecutar_simulador|dotnet run --project ADM6') {" ^
  "    Write-Host ('Cerrando ventana de lanzamiento (PID ' + $cmd.ProcessId + ')...'); Stop-Process -Id $cmd.ProcessId -Force -ErrorAction SilentlyContinue" ^
  "  }" ^
  "}" 

for %%T in ("Identificador Python" "Proveedor Java" "Aplicacion Web" "WS_PROVEEDOR" "WS_AUTENTICACION2" "WS_IDENTIFICADOR1" "WS_AUTENTICACION1") do taskkill /FI "WINDOWTITLE eq %%~T*" /T /F >nul 2>&1

echo.
echo Servicios del proyecto cerrados.
echo No se modificaron archivos ni bases de datos.
pause
endlocal
