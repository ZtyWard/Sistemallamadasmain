@echo off
setlocal
title CARGAR DATOS Y PROBAR SIETE HISTORIAS
cd /d "%~dp0"
set "DEMO_SCRIPT=%~f0"
set "DEMO_ROOT=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$r=[IO.File]::ReadAllText($env:DEMO_SCRIPT);$m=[char]35+' POWERSHELL';iex ($r.Substring($r.IndexOf($m)+$m.Length))"
echo.
pause
exit /b

# POWERSHELL
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [Text.Encoding]::UTF8

$root = $env:DEMO_ROOT.TrimEnd('\')
$provider = "http://localhost:8111/WS_PROVEEDOR.asmx"
$auth2 = "http://localhost:8112/WS_AUTENTICACION2.asmx"
$identificador = "http://localhost:8113/WS_IDENTIFICADOR1.asmx"
$auth1 = "http://localhost:8114/WS_AUTENTICACION1.asmx"

function Post-SoapForm([string]$url, [hashtable]$body) {
    $response = Invoke-WebRequest -Uri $url -Method Post `
        -ContentType "application/x-www-form-urlencoded" -Body $body `
        -UseBasicParsing -TimeoutSec 30
    return [xml]$response.Content
}

function Encrypt-Text([string]$service, [string]$text) {
    $xml = Post-SoapForm "$service/EncriptarTexto" @{ texto = $text }
    return $xml.DocumentElement.InnerText
}

function Show-Result([string]$name, [xml]$xml, [bool]$expected = $true) {
    $result = [string]$xml.DocumentElement.Resultado
    $message = [string]$xml.DocumentElement.Mensaje
    $saldoNode = $xml.DocumentElement.SelectSingleNode("*[local-name()='Saldo']")
    $saldo = if ($null -ne $saldoNode) { " | Saldo=$($saldoNode.InnerText)" } else { "" }
    $ok = ($result.ToLowerInvariant() -eq $expected.ToString().ToLowerInvariant())
    $color = if ($ok) { "Green" } else { "Red" }
    Write-Host ("{0}: Resultado={1} | Mensaje={2}{3}" -f $name,$result,$message,$saldo) -ForegroundColor $color
    if (-not $ok) { throw "$name devolvio un resultado inesperado." }
}

function Wait-Http([string]$url, [int]$seconds = 60) {
    $end = (Get-Date).AddSeconds($seconds)
    while ((Get-Date) -lt $end) {
        try {
            $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -eq 200) { return $true }
        } catch { }
        Start-Sleep -Seconds 1
    }
    return $false
}

$script:cleanupLineBody = $null
trap {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($null -ne $script:cleanupLineBody) {
        try {
            Write-Host "Desactivando la linea creada antes de terminar..." -ForegroundColor Yellow
            $cleanupResult = Post-SoapForm "$provider/ActivarDesactivarServicio" $script:cleanupLineBody
            Write-Host "Limpieza: $($cleanupResult.DocumentElement.Mensaje)"
        } catch {
            Write-Host "No fue posible desactivar automaticamente la linea: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    break
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "CARGA AUTOMATICA Y PRUEBA DE LAS SIETE HISTORIAS" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

if (-not (Wait-Http $provider 2)) {
    Write-Host "Los servicios no estan encendidos. Ejecutando INICIAR_SERVICIOS.bat..."
    Start-Process -FilePath (Join-Path $root "INICIAR_SERVICIOS.bat") -WorkingDirectory $root
}

foreach ($url in @($provider,$auth2,$identificador,$auth1)) {
    if (-not (Wait-Http $url 90)) { throw "No responde el servicio: $url" }
}

$stamp6 = Get-Date -Format "HHmmss"
$stamp8 = Get-Date -Format "HHmmssff"
$telefono = "71$stamp6"
$idTelefono = "9900000000$stamp6"
$idTarjeta = "9900000000000$stamp6"
$cliente = "9$stamp8"
$identificacionUsuario = "8$stamp8"
$usuario = "demo$stamp6"
$contrasena = "Demo2026!Ab123"
$usuarioNuevo = "nuevo$stamp6"
$contrasenaNueva = "Nueva2026!Ab12"
$correo = "$usuario@example.com"

Write-Host ""
Write-Host "DATOS GENERADOS PARA ESTA EJECUCION" -ForegroundColor Yellow
Write-Host "Telefono:                 $telefono"
Write-Host "Identificador telefono:   $idTelefono"
Write-Host "Identificador tarjeta:    $idTarjeta"
Write-Host "Identificacion cliente:   $cliente"
Write-Host "Identificacion usuario:   $identificacionUsuario"
Write-Host "Usuario inicial:          $usuario"
Write-Host "Contrasena inicial:       $contrasena"
Write-Host "Usuario modificado:       $usuarioNuevo"
Write-Host "Contrasena modificada:    $contrasenaNueva"
Write-Host ""

Write-Host "1. WS_PROVEEDOR1 - Crear linea" -ForegroundColor Cyan
$providerConnection = Post-SoapForm "$provider/ProbarConexionProveedor" @{}
Show-Result "Conexion Proveedor" $providerConnection

$pTelefono = Encrypt-Text $provider $telefono
$pIdTelefono = Encrypt-Text $provider $idTelefono
$pIdTarjeta = Encrypt-Text $provider $idTarjeta
$pTipo = Encrypt-Text $provider "PREPAGO"
$pDisponible = Encrypt-Text $provider "DISPONIBLE"
$pCliente = Encrypt-Text $provider $cliente
$pActivo = Encrypt-Text $provider "ACTIVO"

$createLineBody = @{
    telefonoEncriptado = $pTelefono
    identificadorTelefonoEncriptado = $pIdTelefono
    identificadorTarjetaEncriptado = $pIdTarjeta
    tipoEncriptado = $pTipo
    estadoEncriptado = $pDisponible
}
$createdLine = Post-SoapForm "$provider/IngresarServicioNuevoDisponible" $createLineBody
Show-Result "Crear linea" $createdLine
$duplicateLine = Post-SoapForm "$provider/IngresarServicioNuevoDisponible" $createLineBody
Show-Result "Rechazo de linea duplicada" $duplicateLine $false

Write-Host ""
Write-Host "2. WS_PROVEEDOR2 / IDENTIFICADOR6 - Activar" -ForegroundColor Cyan
$activateBody = @{
    telefonoEncriptado = $pTelefono
    identificadorTelefonoEncriptado = $pIdTelefono
    identificadorTarjetaEncriptado = $pIdTarjeta
    tipoEncriptado = $pTipo
    identificacionClienteEncriptada = $pCliente
    estadoEncriptado = $pActivo
}
$activated = Post-SoapForm "$provider/ActivarDesactivarServicio" $activateBody
Show-Result "Activar linea y sincronizar MySQL" $activated
$script:cleanupLineBody = $activateBody.Clone()
$script:cleanupLineBody.estadoEncriptado = $pDisponible

Write-Host ""
Write-Host "3. WS_IDENTIFICADOR1 - Consultar saldo" -ForegroundColor Cyan
$launcherText = Get-Content -LiteralPath (Join-Path $root "ejecutar_identificador.ps1") -Raw
if ($launcherText -notmatch 'AES_KEY_BASE64\s*=\s*"([^"]+)"') {
    throw "No se pudo leer AES_KEY_BASE64."
}
$env:AES_KEY_BASE64 = $Matches[1]
$python = Join-Path $root ".venv\Scripts\python.exe"
$mainPath = Join-Path $root "Proyecto\src"
$pythonCode = "import sys;sys.path.insert(0,r'$mainPath');from main import Config,CryptoBox;print(CryptoBox(Config().aes_key_b64).encrypt('$telefono'))"
$telefonoGcm = (& $python -c $pythonCode | Select-Object -Last 1).Trim()
if ([string]::IsNullOrWhiteSpace($telefonoGcm)) { throw "No se pudo cifrar el telefono para WS_IDENTIFICADOR1." }
$saldo = Post-SoapForm "$identificador/ConsultarSaldo" @{
    telefonoEncriptado = $telefonoGcm
    origen = "WEB"
    tipoTransaccion = "saldo"
}
Show-Result "Consultar saldo" $saldo
$saldoInvalido = Post-SoapForm "$identificador/ConsultarSaldo" @{
    telefonoEncriptado = "NO_ES_AES"
    origen = "WEB"
    tipoTransaccion = "saldo"
}
Show-Result "Rechazo de cifrado invalido" $saldoInvalido $false

Write-Host ""
Write-Host "4. WS_AUTENTICACION2 - Crear usuario" -ForegroundColor Cyan
$mongo = Post-SoapForm "$auth2/ProbarConexionMongo" @{}
Show-Result "Conexion MongoDB" $mongo
$aUsuario = Encrypt-Text $auth2 $usuario
$aContrasena = Encrypt-Text $auth2 $contrasena
$createUserBody = @{
    identificacion = $identificacionUsuario
    nombre = "Fabian"
    primerApellido = "Mora"
    segundoApellido = "Soto"
    correoElectronico = $correo
    usuarioEncriptado = $aUsuario
    contrasenaEncriptada = $aContrasena
    estado = "activo"
    tipo = 1
}
$createdUser = Post-SoapForm "$auth2/CrearUsuario" $createUserBody
Show-Result "Crear usuario" $createdUser
$duplicateUser = Post-SoapForm "$auth2/CrearUsuario" $createUserBody
Show-Result "Rechazo de usuario duplicado" $duplicateUser $false

Write-Host ""
Write-Host "5. WS_AUTENTICACION1 - Autenticar" -ForegroundColor Cyan
$authenticated = Post-SoapForm "$auth1/Autenticar" @{
    usuarioEncriptado = $aUsuario
    contrasenaEncriptada = $aContrasena
    tipoUsuario = "1"
}
Show-Result "Autenticar usuario nuevo" $authenticated

Write-Host ""
Write-Host "6. WS_AUTENTICACION2 - Modificar y cambiar estado" -ForegroundColor Cyan
$aUsuarioNuevo = Encrypt-Text $auth2 $usuarioNuevo
$aContrasenaNueva = Encrypt-Text $auth2 $contrasenaNueva
$modified = Post-SoapForm "$auth2/ModificarUsuario" @{
    identificacion = $identificacionUsuario
    nombre = "Fabian"
    primerApellido = "Mora"
    segundoApellido = "Soto"
    correoElectronico = "nuevo.$correo"
    usuarioEncriptado = $aUsuarioNuevo
    contrasenaEncriptada = $aContrasenaNueva
}
Show-Result "Modificar usuario" $modified
$authenticatedNew = Post-SoapForm "$auth1/Autenticar" @{
    usuarioEncriptado = $aUsuarioNuevo
    contrasenaEncriptada = $aContrasenaNueva
    tipoUsuario = "1"
}
Show-Result "Autenticar datos modificados" $authenticatedNew
$inactive = Post-SoapForm "$auth2/CambiarEstadoUsuario" @{
    identificacion = $identificacionUsuario
    estado = "inactivo"
}
Show-Result "Inactivar usuario" $inactive
$rejectedInactive = Post-SoapForm "$auth1/Autenticar" @{
    usuarioEncriptado = $aUsuarioNuevo
    contrasenaEncriptada = $aContrasenaNueva
    tipoUsuario = "1"
}
Show-Result "Rechazo de usuario inactivo" $rejectedInactive $false
$activeAgain = Post-SoapForm "$auth2/CambiarEstadoUsuario" @{
    identificacion = $identificacionUsuario
    estado = "activo"
}
Show-Result "Reactivar usuario" $activeAgain

Write-Host ""
Write-Host "7. WS_PROVEEDOR3 - Facturacion y desactivacion" -ForegroundColor Cyan
$billing = Post-SoapForm "$provider/CalcularFacturacion" @{
    fechaCalculo = "2026-07-24"
    fechaMaximaPago = "2026-08-05"
}
Show-Result "Calcular facturacion" $billing
$badBilling = Post-SoapForm "$provider/CalcularFacturacion" @{
    fechaCalculo = "2026-08-31"
    fechaMaximaPago = "2026-08-01"
}
Show-Result "Rechazo de fechas invertidas" $badBilling $false

$deactivateBody = $activateBody.Clone()
$deactivateBody.estadoEncriptado = $pDisponible
$deactivated = Post-SoapForm "$provider/ActivarDesactivarServicio" $deactivateBody
Show-Result "Desactivar linea y sincronizar MySQL" $deactivated
$script:cleanupLineBody = $null

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "CARGA Y PRUEBA AUTOMATICA COMPLETADAS" -ForegroundColor Green
Write-Host "============================================================" -ForegroundColor Green
Write-Host "Conserve esta ventana para mostrar los datos y resultados."
