$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

$env:AES_KEY_BASE64 = "XP2jwjvXFogsw3DHWywVFIU2ZS1J6IYEDIzvoyve/dg="

$python = ".\.venv\Scripts\python.exe"
$bundledPython = "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe"

function Test-PythonExecutable([string]$executable) {
    if (-not (Test-Path -LiteralPath $executable)) {
        return $false
    }

    try {
        & $executable --version *> $null
        return $LASTEXITCODE -eq 0
    } catch {
        return $false
    }
}

if (-not (Test-PythonExecutable $python)) {
    $creado = $false

    if (Get-Command py -ErrorAction SilentlyContinue) {
        & py -3 --version *> $null
        if ($LASTEXITCODE -eq 0) {
            py -3 -m venv --clear .venv
            $creado = $LASTEXITCODE -eq 0
        }
    }

    if (-not $creado -and (Get-Command python -ErrorAction SilentlyContinue)) {
        & python --version *> $null
        if ($LASTEXITCODE -eq 0) {
            python -m venv --clear .venv
            $creado = $LASTEXITCODE -eq 0
        }
    }

    if (-not $creado -and (Test-PythonExecutable $bundledPython)) {
        & $bundledPython -m venv --clear .venv
        $creado = $LASTEXITCODE -eq 0
    }

    if (-not $creado -or -not (Test-PythonExecutable $python)) {
        throw "No se encontró una instalación funcional de Python 3."
    }

    & $python -m pip install -r Proyecto\identificador\requirements.txt
    if ($LASTEXITCODE -ne 0) {
        throw "No fue posible instalar las dependencias del Identificador."
    }
}

# Un entorno virtual puede existir y aun asi estar incompleto (por ejemplo,
# si una instalacion anterior fue interrumpida). Verificar los imports evita
# que la ventana se cierre inmediatamente con ModuleNotFoundError.
& $python -c "import mysql.connector; import cryptography" *> $null
if ($LASTEXITCODE -ne 0) {
    & $python -m pip install -r Proyecto\identificador\requirements.txt
    if ($LASTEXITCODE -ne 0) {
        throw "No fue posible reparar las dependencias del Identificador."
    }
}

& $python Proyecto\src\main.py
