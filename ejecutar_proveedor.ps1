$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

$proveedorDir = Join-Path $PSScriptRoot "ProveedorJava\ProveedorTelefonico"
$srcDir = Join-Path $proveedorDir "src"
$outDir = Join-Path $proveedorDir "out"
$jdbcJar = Join-Path $srcDir "lib\mssql-jdbc-13.4.0.jre11.jar"

if (-not (Get-Command javac -ErrorAction SilentlyContinue)) {
    throw "No se encontró javac. Instale un JDK 17 o superior y agréguelo al PATH."
}

$javacExe = (Get-Command javac).Source
$javaExe = Join-Path (Split-Path $javacExe -Parent) "java.exe"

if (-not (Test-Path -LiteralPath $javaExe)) {
    throw "No se encontro java.exe junto al javac del JDK: $javaExe"
}

if (-not (Test-Path -LiteralPath $jdbcJar)) {
    throw "No se encontró el controlador JDBC: $jdbcJar"
}

if (-not (Test-Path -LiteralPath $outDir)) {
    New-Item -ItemType Directory -Path $outDir | Out-Null
}

$javaFiles = Get-ChildItem -Path $srcDir -Recurse -Filter *.java |
    ForEach-Object { $_.FullName }

if ($javaFiles.Count -eq 0) {
    throw "No se encontraron archivos Java para compilar."
}

& javac -encoding UTF-8 -cp $jdbcJar -d $outDir $javaFiles

if ($LASTEXITCODE -ne 0) {
    throw "La compilación del proveedor falló."
}

$classpath = "$outDir;$jdbcJar"

# Usar la misma llave AES-GCM que ya está configurada en el Identificador.
# Esto evita que una variable vieja de PowerShell rompa IDENTIFICADOR6.
$identificadorLauncher = Get-Content -LiteralPath (Join-Path $PSScriptRoot "ejecutar_identificador.ps1") -Raw
if ($identificadorLauncher -notmatch 'AES_KEY_BASE64\s*=\s*"([^"]+)"') {
    throw "No se pudo leer AES_KEY_BASE64 de ejecutar_identificador.ps1"
}

$env:AES_KEY_BASE64 = $Matches[1]
$env:IDENTIFICADOR_HOST = "127.0.0.1"
$env:IDENTIFICADOR_PORT = "5000"
$env:PROVEEDOR_PORT = "6000"
$env:CODIGO_PROVEEDOR = "P1"

& $javaExe -cp $classpath Main
