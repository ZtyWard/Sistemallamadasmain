@echo off
setlocal
title BORRAR DATOS GENERADOS POR CARGAR_DATOS_DEMO
cd /d "%~dp0"
set "CLEAN_SCRIPT=%~f0"
set "CLEAN_ROOT=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$r=[IO.File]::ReadAllText($env:CLEAN_SCRIPT);$m=[char]35+' POWERSHELL';iex ($r.Substring($r.IndexOf($m)+$m.Length))"
echo.
pause
exit /b

# POWERSHELL
$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [Text.Encoding]::UTF8
$root = $env:CLEAN_ROOT.TrimEnd('\')

$phones = @("71160933", "71161036")
$phoneIds = @("9900000000160933", "9900000000161036")
$cardIds = @("9900000000000160933", "9900000000000161036")
$clientIds = @("916093380", "916103655")
$mongoIds = @("816103655")
$billingDate = "2026-07-24"

function Open-SqlConnection {
    $connectionString = "Server=localhost\SQLEXPRESS;Database=ProveedorTelefonicoDB;User ID=javauser;Password=Java123456;TrustServerCertificate=True;Encrypt=False"
    $connection = [Data.SqlClient.SqlConnection]::new($connectionString)
    $connection.Open()
    return $connection
}

function Invoke-SqlScalar([Data.SqlClient.SqlConnection]$connection, [string]$sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $sql
    return [int]$command.ExecuteScalar()
}

function Find-MongoPython {
    $candidates = @(
        (Join-Path $root ".venv\Scripts\python.exe"),
        "C:\DemoRAM\.venv\Scripts\python.exe"
    )
    foreach ($candidate in $candidates) {
        if (Test-Path -LiteralPath $candidate) {
            $test = Start-Process -FilePath $candidate `
                -ArgumentList "-c", '"import pymongo"' `
                -WindowStyle Hidden -Wait -PassThru
            if ($test.ExitCode -eq 0) { return $candidate }
        }
    }
    throw "No se encontro el cliente Python de MongoDB ya instalado en esta computadora."
}

Write-Host "============================================================" -ForegroundColor Yellow
Write-Host "BORRADO DE DATOS CREADOS POR CARGAR_DATOS_DEMO" -ForegroundColor Yellow
Write-Host "============================================================" -ForegroundColor Yellow
Write-Host "Telefonos: 71160933 y 71161036"
Write-Host "Clientes: 916093380 y 916103655"
Write-Host "Usuario MongoDB: 816103655"
Write-Host "Facturacion de prueba: 2026-07-24"
Write-Host ""

$sqlConnection = Open-SqlConnection
try {
    $sqlPhones = Invoke-SqlScalar $sqlConnection "SELECT COUNT(*) FROM dbo.telefonos WHERE telefono IN ('71160933','71161036')"
    $sqlCards = Invoke-SqlScalar $sqlConnection "SELECT COUNT(*) FROM dbo.tarjetas WHERE identificador_tarjeta IN ('9900000000000160933','9900000000000161036')"
    $sqlClients = Invoke-SqlScalar $sqlConnection "SELECT COUNT(*) FROM dbo.clientes WHERE identificacion IN ('916093380','916103655')"
    $sqlInvoices = Invoke-SqlScalar $sqlConnection "SELECT COUNT(*) FROM dbo.facturas WHERE fecha_calculo='2026-07-24'"
} finally {
    $sqlConnection.Close()
}

$mysql = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"
if (-not (Test-Path -LiteralPath $mysql)) { throw "No se encontro mysql.exe." }
$mysqlCount = & $mysql -h 127.0.0.1 -P 3306 -u identificador_user "-pIdentificador123" -D identificador_db -N -e "SELECT (SELECT COUNT(*) FROM telefonos WHERE numero_hash IN (SHA2('71160933',256),SHA2('71161036',256)))+(SELECT COUNT(*) FROM tarjetas WHERE identificador_chip_hash IN (SHA2('9900000000000160933',256),SHA2('9900000000000161036',256)));"
if ($LASTEXITCODE -ne 0) { throw "No se pudo consultar MySQL." }

$mongoPython = Find-MongoPython
$mongoCount = & $mongoPython -c "from pymongo import MongoClient;print(MongoClient('mongodb://localhost:27017').central_general_auth.usuarios.count_documents({'Identificacion':{'`$in':['816103655']}}))"
if ($LASTEXITCODE -ne 0) { throw "No se pudo consultar MongoDB." }

Write-Host "REGISTROS ENCONTRADOS" -ForegroundColor Cyan
Write-Host "SQL Server - telefonos: $sqlPhones"
Write-Host "SQL Server - tarjetas: $sqlCards"
Write-Host "SQL Server - clientes: $sqlClients"
Write-Host "SQL Server - facturas: $sqlInvoices"
Write-Host "MySQL - telefonos y tarjetas: $mysqlCount"
Write-Host "MongoDB - usuarios: $mongoCount"
Write-Host ""
Write-Host "No se borraran los datos de DATOS_PARA_EXPOSICION_INTEGRADA.txt." -ForegroundColor Green
Write-Host ""

$confirmation = Read-Host "Escriba BORRAR para confirmar; cualquier otro texto cancela"
if ($confirmation -cne "BORRAR") {
    Write-Host "Operacion cancelada. No se borro ningun dato." -ForegroundColor Yellow
    return
}

Write-Host "Eliminando datos exactos..." -ForegroundColor Yellow

$sqlConnection = Open-SqlConnection
$transaction = $sqlConnection.BeginTransaction()
try {
    $command = $sqlConnection.CreateCommand()
    $command.Transaction = $transaction
    $command.CommandText = @"
DELETE FROM dbo.facturas WHERE fecha_calculo='2026-07-24';
DELETE FROM dbo.tarjetas
WHERE identificador_tarjeta IN ('9900000000000160933','9900000000000161036');
DELETE FROM dbo.telefonos
WHERE telefono IN ('71160933','71161036');
DELETE FROM dbo.clientes
WHERE identificacion IN ('916093380','916103655')
  AND NOT EXISTS (SELECT 1 FROM dbo.telefonos t WHERE t.cliente_id=dbo.clientes.id);
"@
    [void]$command.ExecuteNonQuery()
    $transaction.Commit()
} catch {
    $transaction.Rollback()
    throw
} finally {
    $sqlConnection.Close()
}

& $mysql -h 127.0.0.1 -P 3306 -u identificador_user "-pIdentificador123" -D identificador_db -e "START TRANSACTION; DELETE FROM telefonos WHERE numero_hash IN (SHA2('71160933',256),SHA2('71161036',256)); DELETE FROM tarjetas WHERE identificador_chip_hash IN (SHA2('9900000000000160933',256),SHA2('9900000000000161036',256)) AND NOT EXISTS (SELECT 1 FROM telefonos WHERE telefonos.tarjeta_id=tarjetas.id); COMMIT;"
if ($LASTEXITCODE -ne 0) { throw "Fallo el borrado en MySQL." }

$mongoDeleted = & $mongoPython -c "from pymongo import MongoClient;r=MongoClient('mongodb://localhost:27017').central_general_auth.usuarios.delete_many({'Identificacion':{'`$in':['816103655']}});print(r.deleted_count)"
if ($LASTEXITCODE -ne 0) { throw "Fallo el borrado en MongoDB." }

Write-Host ""
Write-Host "BORRADO COMPLETADO" -ForegroundColor Green
Write-Host "Usuarios eliminados de MongoDB: $mongoDeleted"
Write-Host "Los datos principal y de reserva para la exposicion permanecen intactos."
