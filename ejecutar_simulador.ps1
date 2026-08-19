$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

$proyecto = ".\llamadas\SimuladorLlamadas\SimuladorLlamadas\SimuladorLlamadas.csproj"
$salida = ".\build\simulador"
$exe = Join-Path $salida "SimuladorLlamadas.exe"

if (-not (Test-Path $proyecto)) {
    throw "No se encontró el proyecto del simulador: $proyecto"
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "No se encontró dotnet. Se necesita .NET 8 para ejecutar el simulador."
}

Write-Host "Preparando el Simulador de Llamadas..."

# No se usa --no-restore: al copiar o limpiar el repositorio puede faltar
# obj\project.assets.json y dotnet debe restaurarlo antes de compilar.
dotnet build $proyecto -o $salida

if ($LASTEXITCODE -ne 0) {
    throw "No fue posible compilar el Simulador de Llamadas."
}

if (-not (Test-Path $exe)) {
    throw "La compilación terminó, pero no se encontró el ejecutable: $exe"
}

Write-Host "Abriendo el Simulador de Llamadas..."
& (Resolve-Path $exe).Path
