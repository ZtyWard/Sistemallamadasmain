$ErrorActionPreference = "Stop"

$femDir = Split-Path -Parent $PSScriptRoot
$root = Split-Path -Parent $femDir

function Confirmar-Servicio([string]$nombre) {
    $servicio = Get-Service -Name $nombre -ErrorAction SilentlyContinue
    if ($null -eq $servicio) {
        Write-Warning "Servicio no encontrado: $nombre"
        return
    }

    if ($servicio.Status -ne 'Running') {
        Write-Warning "Servicio detenido: $nombre"
        return
    }

    Write-Host "OK servicio: $nombre"
}

function Confirmar-MotorLocal(
    [string]$descripcion,
    [string[]]$servicios,
    [int]$puerto) {

    foreach ($nombre in $servicios) {
        $servicio = Get-Service -Name $nombre -ErrorAction SilentlyContinue

        if ($null -ne $servicio -and $servicio.Status -eq 'Running') {
            Write-Host "OK servicio: $nombre ($descripcion)"
            return
        }
    }

    $escucha = Get-NetTCPConnection -State Listen -LocalPort $puerto `
        -ErrorAction SilentlyContinue

    if ($null -ne $escucha) {
        Write-Host "OK proceso local: $descripcion en puerto $puerto"
        return
    }

    Write-Warning "No se encontró $descripcion activo (servicios: $($servicios -join ', '); puerto: $puerto)."
}

function Ejecutar-O-Fallar([scriptblock]$accion, [string]$descripcion) {
    & $accion
    if ($LASTEXITCODE -ne 0) {
        throw "Falló: $descripcion"
    }
    Write-Host "OK: $descripcion"
}

Confirmar-Servicio 'MongoDB'
Confirmar-MotorLocal 'MySQL' @('MySQL80') 3306
Confirmar-MotorLocal 'SQL Server' @('MSSQL$SQLEXPRESS', 'MSSQLSERVER') 1433

$vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw 'No se encontró vswhere. Instale Visual Studio.'
}

$vsPath = & $vswhere -latest -products Microsoft.VisualStudio.Product.Community -property installationPath
if ([string]::IsNullOrWhiteSpace($vsPath)) {
    $vsPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
}
$msbuild = Join-Path $vsPath 'MSBuild\Current\Bin\MSBuild.exe'
if (-not (Test-Path -LiteralPath $msbuild)) {
    throw 'No se encontró MSBuild dentro de Visual Studio.'
}

$authSln = Join-Path $femDir 'WS_AUTENTICACION2\WS_AUTENTICACION2.sln'
$proveedorSln = Join-Path $femDir 'WS_PROVEEDOR1\WS_PROVEEDOR.sln'

Ejecutar-O-Fallar {
    & $msbuild $authSln /t:Restore /p:RestorePackagesConfig=true /m
} 'restauración de WS_AUTENTICACION2'

Ejecutar-O-Fallar {
    & $msbuild $authSln /t:Build /p:Configuration=Debug /m
} 'compilación de WS_AUTENTICACION2'

Ejecutar-O-Fallar {
    & $msbuild $proveedorSln /t:Build /p:Configuration=Debug /m
} 'compilación de WS_PROVEEDOR'

$src = Join-Path $root 'ProveedorJava\ProveedorTelefonico\src'
$out = Join-Path $root 'ProveedorJava\ProveedorTelefonico\out'
$jdbc = Join-Path $src 'lib\mssql-jdbc-13.4.0.jre11.jar'
$javaFiles = Get-ChildItem -LiteralPath $src -Recurse -Filter *.java |
    ForEach-Object { $_.FullName }

if (-not (Test-Path -LiteralPath $out)) {
    New-Item -ItemType Directory -Path $out | Out-Null
}

Ejecutar-O-Fallar {
    & javac -encoding UTF-8 -cp $jdbc -d $out $javaFiles
} 'compilación del proveedor Java'

Write-Host 'Verificación terminada correctamente.'
