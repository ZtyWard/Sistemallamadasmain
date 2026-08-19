param(
    [string]$Raiz = (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
)

$ErrorActionPreference = "Stop"

$raizReal = (Resolve-Path -LiteralPath $Raiz).Path
$raizTrabajo = $raizReal
$enlaceTemporal = $null

# NuGet/MSBuild de .NET Framework aun falla con rutas mayores de 260
# caracteres. Un enlace corto permite conservar el proyecto en su ubicacion
# actual sin reorganizarlo.
if ($raizReal.Length -gt 100) {
    $enlaceTemporal = Join-Path $env:TEMP "SCL_BUILD_$PID"
    if (Test-Path -LiteralPath $enlaceTemporal) {
        [IO.Directory]::Delete($enlaceTemporal, $false)
    }

    New-Item -ItemType Junction -Path $enlaceTemporal -Target $raizReal | Out-Null
    $raizTrabajo = $enlaceTemporal
}

$vswhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw "No se encontro Visual Studio Installer. Instale Visual Studio 2022 con desarrollo ASP.NET."
}

$instalacion = & $vswhere -latest -products * `
    -requires Microsoft.Component.MSBuild -property installationPath
$msbuild = Join-Path $instalacion "MSBuild\Current\Bin\MSBuild.exe"

if ([string]::IsNullOrWhiteSpace($instalacion) -or -not (Test-Path -LiteralPath $msbuild)) {
    throw "No se encontro MSBuild. Agregue la carga de trabajo de desarrollo ASP.NET a Visual Studio."
}

$soluciones = @(
    @{
        Ruta = "FEM\WS_AUTENTICACION2\WS_AUTENTICACION2.sln"
        Paquetes = "FEM\packages"
    },
    @{
        Ruta = "FEM\WS_PROVEEDOR1\WS_PROVEEDOR.sln"
        Paquetes = "FEM\WS_PROVEEDOR1\packages"
    },
    @{
        Ruta = "WS_IDENTIFICADOR1\WS_IDENTIFICADOR1.sln"
        Paquetes = "WS_IDENTIFICADOR1\packages"
    }
)
$configuracionNuGet = Join-Path $raizTrabajo "NuGet.Config"

if (-not (Test-Path -LiteralPath $configuracionNuGet)) {
    throw "No se encontro NuGet.Config en la raiz del proyecto."
}

try {
    foreach ($entrada in $soluciones) {
        $relativa = $entrada.Ruta
        $solucion = Join-Path $raizTrabajo $relativa
        $repositorioPaquetes = Join-Path $raizTrabajo $entrada.Paquetes
        if (-not (Test-Path -LiteralPath $solucion)) {
            throw "No se encontro la solucion: $solucion"
        }

        Write-Host "Restaurando $relativa..."
        & $msbuild $solucion /t:Restore /p:RestorePackagesConfig=true `
            "/p:RestoreConfigFile=$configuracionNuGet" `
            "/p:RestoreRepositoryPath=$repositorioPaquetes" /m
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo la restauracion de $relativa"
        }

        Write-Host "Compilando $relativa..."
        & $msbuild $solucion /t:Build /p:Configuration=Debug /m
        if ($LASTEXITCODE -ne 0) {
            throw "Fallo la compilacion de $relativa"
        }
    }
} finally {
    if ($enlaceTemporal -and (Test-Path -LiteralPath $enlaceTemporal)) {
        [IO.Directory]::Delete($enlaceTemporal, $false)
    }
}

Write-Host "Servicios web preparados correctamente."
