param(
    [string]$Autenticacion1 = "http://localhost:8114/WS_AUTENTICACION1.asmx",
    [string]$Autenticacion2 = "http://localhost:8112/WS_AUTENTICACION2.asmx"
)

$ErrorActionPreference = "Stop"

function Enviar-FormularioSoap([string]$url, [hashtable]$datos) {
    $respuesta = Invoke-WebRequest -UseBasicParsing -Uri $url -Method Post `
        -ContentType "application/x-www-form-urlencoded" -Body $datos -TimeoutSec 20
    return [xml]$respuesta.Content
}

function Obtener-Valor([xml]$xml, [string]$nombre) {
    $nodo = $xml.SelectSingleNode("//*[local-name()='$nombre']")
    if ($null -eq $nodo) { return "" }
    return $nodo.InnerText
}

function Cifrar-Texto([string]$texto) {
    $xml = Enviar-FormularioSoap "$Autenticacion2/EncriptarTexto" @{ texto = $texto }
    return $xml.DocumentElement.InnerText
}

$usuarios = @(
    @{ Id = "101110111"; Nombre = "Ana";      Apellido1 = "Solis";    Apellido2 = "Rojas" },
    @{ Id = "202220222"; Nombre = "Bruno";    Apellido1 = "Vargas";   Apellido2 = "Mora" },
    @{ Id = "303330333"; Nombre = "Carla";    Apellido1 = "Ramirez";  Apellido2 = "Soto" },
    @{ Id = "404440444"; Nombre = "Diego";    Apellido1 = "Jimenez";  Apellido2 = "Arias" },
    @{ Id = "505550555"; Nombre = "Elena";    Apellido1 = "Castro";   Apellido2 = "Leon" },
    @{ Id = "606660666"; Nombre = "Felipe";   Apellido1 = "Ruiz";     Apellido2 = "Vega" },
    @{ Id = "707770777"; Nombre = "Gabriela"; Apellido1 = "Mendez";   Apellido2 = "Rojas" },
    @{ Id = "808880888"; Nombre = "Hugo";     Apellido1 = "Salazar";  Apellido2 = "Mora" },
    @{ Id = "909990999"; Nombre = "Irene";    Apellido1 = "Quesada";  Apellido2 = "Soto" }
)

$admin = Enviar-FormularioSoap "$Autenticacion1/CrearUsuariosPrueba" @{}
if ((Obtener-Valor $admin "Resultado") -ne "true") {
    throw "No se pudo preparar el usuario administrador: $(Obtener-Valor $admin 'Mensaje')"
}

$listado = Enviar-FormularioSoap "$Autenticacion2/ListarUsuarios" @{ tipo = 2 }
$existentes = @{}
$listado.SelectNodes("//*[local-name()='Identificacion']") | ForEach-Object {
    $existentes[$_.InnerText] = $true
}

foreach ($usuario in $usuarios) {
    if ($existentes.ContainsKey($usuario.Id)) {
        Write-Host "OK cliente existente: $($usuario.Id)"
        continue
    }

    $nombreUsuario = "cliente$($usuario.Id)"
    $resultado = Enviar-FormularioSoap "$Autenticacion2/CrearUsuario" @{
        identificacion = $usuario.Id
        nombre = $usuario.Nombre
        primerApellido = $usuario.Apellido1
        segundoApellido = $usuario.Apellido2
        correoElectronico = "$nombreUsuario@centralgeneral.com"
        usuarioEncriptado = Cifrar-Texto $nombreUsuario
        contrasenaEncriptada = Cifrar-Texto "Cliente2026!Ab"
        estado = "activo"
        tipo = 2
    }

    if ((Obtener-Valor $resultado "Resultado") -ne "true") {
        throw "No se pudo preparar el cliente $($usuario.Id): $(Obtener-Valor $resultado 'Mensaje')"
    }

    Write-Host "OK cliente creado: $($usuario.Id)"
}

Write-Host "Datos administrativos preparados correctamente."
