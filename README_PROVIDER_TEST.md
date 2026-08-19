# Proveedor Java - pasos rápidos

## Inicio recomendado

Desde la carpeta raíz del proyecto, ejecutar:

```powershell
.\ejecutar_proveedor.bat
```

El lanzador compila el código con UTF-8, agrega el controlador JDBC incluido en
el repositorio e inicia el servidor en el puerto `6000`.

## Verificación rápida

Con el proveedor encendido, abrir otra ventana de PowerShell y ejecutar:

```powershell
Test-NetConnection 127.0.0.1 -Port 6000
```

`TcpTestSucceeded` debe mostrar `True`.

Para verificar también SQL Server, MySQL, MongoDB y las compilaciones, ejecutar:

```powershell
powershell -ExecutionPolicy Bypass -File .\FEM\scripts\verificar_entorno.ps1
```

Los registros del proveedor se escriben en `logs\proveedor.log`.
