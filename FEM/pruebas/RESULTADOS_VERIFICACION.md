# Resultados de verificación final

Fecha: 15 de julio de 2026.

## Compilación

- `WS_PROVEEDOR.sln`: correcta, 0 errores y 0 advertencias.
- `WS_AUTENTICACION2.sln`: correcta, 0 errores y 0 advertencias.
- Proveedor Java: correcto, 33 clases compiladas.
- IIS Express: ambos `.asmx` y ambos WSDL respondieron HTTP 200.

## WS_PROVEEDOR1

- `ProbarConexionProveedor`: `true / Exitoso`.
- Alta de línea disponible con trama cifrada: `true / Exitoso`.
- Alta duplicada: `false / Problemas al incluir la información.`
- La bitácora registró `proveedor4_ingresar_linea` sin exponer teléfono,
  identificador de teléfono ni identificador de tarjeta en texto plano.

## WS_PROVEEDOR2

- Activación completa C# -> Java -> Python -> MySQL: `true / Exitoso`.
- Desactivación completa C# -> Java -> Python -> MySQL: `true / Exitoso`.
- La bitácora registró `proveedor5_activar_desactivar_linea`.

## WS_PROVEEDOR3

- Ejecución del procedimiento de facturación postpago: `true / Exitoso`.
- La bitácora registró `proveedor6_calcular_facturacion`.

## WS_AUTENTICACION2

- Conexión a MongoDB: `true / Exitoso`.
- Crear usuario válido: `true / Exitoso`.
- Rechazar usuario duplicado: correcto.
- Modificar usuario existente: `true / Exitoso`.
- Cambiar estado a inactivo: `true / Exitoso`.
- Usuario y contraseña verificados como cifrados en MongoDB.
- El usuario temporal de la prueba fue eliminado al finalizar.

## Limpieza

- SQL Server fue restaurado a los datos semilla del Alcance 2.
- Los registros temporales de integración fueron eliminados de MySQL y MongoDB.
- No quedaron servidores temporales escuchando en los puertos 5000, 6000,
  8091 o 8092.

