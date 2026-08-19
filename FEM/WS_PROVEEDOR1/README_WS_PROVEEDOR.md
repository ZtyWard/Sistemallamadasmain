# WS_PROVEEDOR - Historias WS_PROVEEDOR1, WS_PROVEEDOR2 y WS_PROVEEDOR3

Proyecto Visual Studio:

`FEM\WS_PROVEEDOR1\WS_PROVEEDOR.sln`

Servicio principal:

`WS_PROVEEDOR\WS_PROVEEDOR.asmx.cs`

## Operaciones implementadas

### WS_PROVEEDOR1 - IngresarServicioNuevoDisponible

Recibe todos los datos encriptados:

- número de teléfono
- identificador del teléfono de 16 dígitos
- identificador de tarjeta/SIM de 19 dígitos
- tipo: PREPAGO o POSTPAGO
- estado: DISPONIBLE

Envía al proveedor Java la trama:

`PROVEEDOR4|telefonoCifrado|identificadorTelefonoCifrado|identificadorTarjetaCifrado|tipo|estado`

Los tres campos sensibles permanecen cifrados durante el envío y dentro de la
bitácora. El proveedor Java los descifra antes de aplicar sus validaciones.

Respuesta exitosa:

`Resultado = true`
`Mensaje = Exitoso`

Respuesta de error:

`Resultado = false`
`Mensaje = Problemas al incluir la información.`

### WS_PROVEEDOR2 - ActivarDesactivarServicio

Recibe todos los datos encriptados:

- número de teléfono
- identificador del teléfono de 16 dígitos
- identificador de tarjeta/SIM de 19 dígitos
- tipo: PREPAGO o POSTPAGO
- identificación del cliente
- estado: ACTIVO o DISPONIBLE

Envía al proveedor Java la trama:

`PROVEEDOR5|telefonoCifrado|identificadorTelefonoCifrado|identificadorTarjetaCifrado|tipo|identificacionCliente|estado`

Respuesta exitosa:

`Resultado = true`
`Mensaje = Exitoso`

Respuesta de error:

`Resultado = false`
`Mensaje = Problemas al activar/desactivar la línea.`

### WS_PROVEEDOR3 - CalcularFacturacion

Recibe:

- fecha de cálculo en formato `yyyy-MM-dd`
- fecha máxima de pago en formato `yyyy-MM-dd`

Envía al proveedor Java la trama:

`PROVEEDOR6|fechaCalculo|fechaMaximaPago`

Respuesta exitosa:

`Resultado = true`
`Mensaje = Exitoso`

Respuesta de error:

`Resultado = false`
`Mensaje = Problemas al realizar el cálculo.`

## Métodos auxiliares

### EncriptarTexto

Sirve para cifrar datos antes de llamar WS_PROVEEDOR1 o WS_PROVEEDOR2.

### ProbarConexionProveedor

Envía `PING` al proveedor Java y valida que responda `OK`.

## Integración hecha en proveedor Java

Se actualizó:

`ProveedorJava\ProveedorTelefonico\src\server\SocketServer.java`

Ahora reconoce:

- `PROVEEDOR4`
- `PROVEEDOR5`
- `PROVEEDOR6`
- `PING`

También se corrigió la bitácora para que registre el tipo de acción, por ejemplo:

- `proveedor4_ingresar_linea`
- `proveedor5_activar_desactivar_linea`
- `proveedor6_calcular_facturacion`
- `consulta_saldo`
- `autorizacion_llamada`
- `registro_movimiento`

## Cómo probar

1. Ejecutar el proveedor Java.
2. Abrir `WS_PROVEEDOR.sln` en Visual Studio.
3. Ejecutar con IIS Express.
4. Abrir `WS_PROVEEDOR.asmx`.
5. Probar primero `ProbarConexionProveedor`.
6. Usar `EncriptarTexto` para cifrar los datos.
7. Probar las operaciones del servicio.

La dirección, el puerto, el timeout y la llave AES se pueden cambiar en
`WS_PROVEEDOR\Web.config` sin recompilar.

Para una prueba completa de activación/desactivación, el Identificador Python
debe estar ejecutándose antes que el proveedor Java.
