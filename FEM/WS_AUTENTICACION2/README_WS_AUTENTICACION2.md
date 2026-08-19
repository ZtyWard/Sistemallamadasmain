# WS_AUTENTICACION2 - Mantenimiento de usuarios

Historia asignada: WS_AUTENTICACION2.

Objetivo: crear un Web Service SOAP/XML para mantenimiento de usuarios en MongoDB.

## Ubicación

Proyecto Visual Studio:

`FEM\WS_AUTENTICACION2\WS_AUTENTICACION2.sln`

Servicio principal:

`WS_AUTENTICACION2\WS_AUTENTICACION2.asmx.cs`

## Base de datos

MongoDB local:

`mongodb://localhost:27017`

Base:

`central_general_auth`

Colección:

`usuarios`

## Métodos SOAP implementados

### CrearUsuario

Recibe:

- identificación
- nombre
- primer apellido
- segundo apellido
- correo electrónico
- usuario encriptado
- contraseña encriptada
- estado: debe ser `activo`
- tipo de usuario: 1 empleado, 2 cliente

Reglas:

- La identificación debe ser numérica.
- Nombre y apellidos no pueden estar vacíos ni contener números.
- El correo debe tener formato válido.
- La contraseña debe tener exactamente 14 caracteres, con mayúscula, minúscula, número y carácter especial.
- El tipo debe ser 1 o 2.
- El estado inicial siempre queda como `activo`.
- No permite repetir identificación ni usuario.

Respuesta exitosa:

`Resultado = true`
`Mensaje = Exitoso`

Respuesta de error:

`Resultado = false`
`Mensaje = Usuario ya existe o datos incorrectos o incompletos.`

### ModificarUsuario

Recibe identificación, nombre, primer apellido, segundo apellido, correo electrónico, usuario encriptado y contraseña encriptada.

Reglas:

- Busca por identificación.
- No permite cambiar la identificación.
- Actualiza nombre, apellidos, correo, usuario y contraseña.
- No permite usar un nombre de usuario que ya pertenezca a otra identificación.

Respuesta exitosa:

`Resultado = true`
`Mensaje = Exitoso`

Respuesta de error:

`Resultado = false`
`Mensaje = Usuario no existe o datos incorrectos o incompletos.`

### CambiarEstadoUsuario

Recibe:

- identificación
- estado: `activo` o `inactivo`

Respuesta exitosa:

`Resultado = true`
`Mensaje = Exitoso`

Respuesta de error:

`Resultado = false`
`Mensaje = Usuario no existe o datos incorrectos.`

### EncriptarTexto

Método auxiliar para pruebas. Permite cifrar usuario y contraseña antes de llamar CrearUsuario o ModificarUsuario.

### ProbarConexionMongo

Método auxiliar para verificar que MongoDB esté instalado y corriendo.

## Cómo probar

1. Abrir `WS_AUTENTICACION2.sln` en Visual Studio.
2. Restaurar paquetes NuGet si Visual Studio lo solicita.
3. Ejecutar el proyecto con IIS Express.
4. Abrir `WS_AUTENTICACION2.asmx`.
5. Primero probar `ProbarConexionMongo`.
6. Usar `EncriptarTexto` para cifrar usuario y contraseña.
7. Usar esos textos cifrados en `CrearUsuario`.
8. Probar `ModificarUsuario` y `CambiarEstadoUsuario`.

La URI, base, colección de MongoDB y llave AES se pueden cambiar en
`WS_AUTENTICACION2\Web.config` sin recompilar.

La solución incluye un `NuGet.Config` que restaura las dependencias en
`FEM\packages`; esto evita errores por longitud de ruta al trabajar dentro de la
carpeta anidada del proyecto.
