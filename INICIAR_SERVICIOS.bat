@echo off
setlocal EnableExtensions EnableDelayedExpansion
title INICIAR SERVICIOS - SISTEMA CONTROL LLAMADAS
cd /d "%~dp0"

set "ROOT=%CD%"
set "IIS=C:\Program Files\IIS Express\iisexpress.exe"

echo ============================================================
echo INICIANDO TODOS LOS SERVICIOS DEL PROYECTO
echo ============================================================

if not exist "%IIS%" (
    echo ERROR: No se encontro IIS Express.
    goto :error
)

echo Preparando servicios web C#...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\FEM\scripts\preparar_servicios_web.ps1" -Raiz "%ROOT%"
if errorlevel 1 goto :error

call :port_open 5000
if errorlevel 1 (
    echo Iniciando Identificador Python...
    start "Identificador Python" "%ROOT%\ejecutar_identificador.bat"
) else (
    echo OK: Identificador Python ya usa el puerto 5000.
)

call :wait_port 5000 45 "Identificador Python"
if errorlevel 1 goto :error

call :port_open 6000
if errorlevel 1 (
    echo Iniciando Proveedor Java...
    start "Proveedor Java" "%ROOT%\ejecutar_proveedor.bat"
) else (
    echo OK: Proveedor Java ya usa el puerto 6000.
)

call :wait_port 6000 60 "Proveedor Java"
if errorlevel 1 goto :error

call :start_iis 8111 "WS_PROVEEDOR" "%ROOT%\FEM\WS_PROVEEDOR1\WS_PROVEEDOR"
if errorlevel 1 goto :error

call :start_iis 8112 "WS_AUTENTICACION2" "%ROOT%\FEM\WS_AUTENTICACION2\WS_AUTENTICACION2"
if errorlevel 1 goto :error

call :start_iis 8113 "WS_IDENTIFICADOR1" "%ROOT%\WS_IDENTIFICADOR1\WS_IDENTIFICADOR1"
if errorlevel 1 goto :error

call :start_iis 8114 "WS_AUTENTICACION1" "%ROOT%\WS_IDENTIFICADOR1\WS_AUTENTICACION1"
if errorlevel 1 goto :error

call :wait_port 8111 45 "WS_PROVEEDOR"
if errorlevel 1 goto :error
call :wait_port 8112 45 "WS_AUTENTICACION2"
if errorlevel 1 goto :error
call :wait_port 8113 45 "WS_IDENTIFICADOR1"
if errorlevel 1 goto :error
call :wait_port 8114 45 "WS_AUTENTICACION1"
if errorlevel 1 goto :error

echo Preparando usuarios administrativos y nombres de clientes...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%\FEM\scripts\inicializar_datos_administracion.ps1"
if errorlevel 1 goto :error

call :port_open 5155
if errorlevel 1 (
    echo Iniciando aplicacion web en puerto 5155...
    start "Aplicacion Web" /min /D "%ROOT%\ADM6\ADM6" cmd /k dotnet run --project ADM6.csproj --urls http://localhost:5155
) else (
    echo OK: la aplicacion web ya usa el puerto 5155.
)

call :wait_port 5155 60 "Aplicacion web"
if errorlevel 1 goto :error

start "" "http://localhost:5155/"

echo.
echo ============================================================
echo TODO LISTO - EL SISTEMA ESTA FUNCIONANDO
echo ============================================================
echo Python: http://localhost:5000
echo Java:   http://localhost:6000
echo WS_PROVEEDOR:      http://localhost:8111/WS_PROVEEDOR.asmx
echo WS_AUTENTICACION2: http://localhost:8112/WS_AUTENTICACION2.asmx
echo WS_IDENTIFICADOR1: http://localhost:8113/WS_IDENTIFICADOR1.asmx
echo WS_AUTENTICACION1: http://localhost:8114/WS_AUTENTICACION1.asmx
echo Aplicacion web:     http://localhost:5155/
echo.
echo No cierre las ventanas de Python, Java ni IIS Express.
pause
exit /b 0

:start_iis
call :port_open %~1
if errorlevel 1 (
    echo Iniciando %~2 en puerto %~1...
    start "%~2" /min "%IIS%" /path:"%~3" /port:%~1 /systray:false
) else (
    echo OK: %~2 ya usa el puerto %~1.
)
exit /b 0

:wait_port
set /a "TRIES=0"
:wait_loop
call :port_open %~1
if not errorlevel 1 (
    echo OK: %~3 responde en el puerto %~1.
    exit /b 0
)
set /a "TRIES+=1"
if !TRIES! geq %~2 (
    echo ERROR: %~3 no abrio el puerto %~1.
    exit /b 1
)
timeout /t 1 /nobreak >nul
goto :wait_loop

:port_open
netstat -ano -p tcp | findstr /R /C:":%~1 .*LISTENING" >nul
if errorlevel 1 exit /b 1
exit /b 0

:error
echo.
echo ============================================================
echo NO SE PUDIERON INICIAR TODOS LOS SERVICIOS
echo Revise el mensaje mostrado arriba.
echo ============================================================
pause
exit /b 1
