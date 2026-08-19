# Parte de Fabian: ADM1 a ADM5

Este archivo describe la parte administrativa en C# del proyecto final. Las historias asignadas a Fabian son `ADM1`, `ADM2`, `ADM3`, `ADM4` y `ADM5`.

## Estado funcional

- `ADM1`: ingreso administrativo contra `WS_AUTENTICACION1`, con usuario y contrasena cifrados mediante AES y tipo administrador oculto.
- `ADM2`: plantilla administrativa, menu comun, perfil, pie de pagina, cierre de sesion y proteccion de rutas.
- `ADM3`: listado, registro y eliminacion confirmada de lineas disponibles.
- `ADM4`: seleccion y activacion de una linea disponible para una identificacion de cliente.
- `ADM5`: listado de lineas activas con nombre del cliente y devolucion confirmada de la linea.

Las paginas web no acceden directamente a SQL Server, MongoDB ni sockets. La aplicacion usa `WS_PROVEEDOR`, `WS_AUTENTICACION1` y `WS_AUTENTICACION2`.

## Archivos principales

| Historia | Implementacion principal |
|---|---|
| ADM1 | `ADM6/ADM6/Pages/Administradores/Login.cshtml`, `Login.cshtml.cs` y `Services/AutenticacionWebServiceClient.cs` |
| ADM2 | `ADM6/ADM6/Pages/Shared/_Layout.cshtml`, `Program.cs` y `Pages/Administradores/Salir.cshtml.cs` |
| ADM3 | `ADM6/ADM6/Pages/NuevaLinea/` y operaciones administrativas de `FEM/WS_PROVEEDOR1` |
| ADM4 | `ADM6/ADM6/Pages/ActivarLinea.cshtml`, `ActivarLinea.cshtml.cs` y `WS_PROVEEDOR2` |
| ADM5 | `ADM6/ADM6/Pages/DesactivarLinea.cshtml`, `DesactivarLinea.cshtml.cs`, `WS_PROVEEDOR2` y `WS_AUTENTICACION2` |

## Requisitos de la computadora

1. Windows 10 u 11.
2. Visual Studio 2022 con desarrollo de ASP.NET y .NET Framework 4.8, incluido IIS Express.
3. SDK de .NET 8.
4. JDK 17 o superior, con `java` y `javac` en `PATH`.
5. Python 3.
6. SQL Server Express con la instancia `localhost\SQLEXPRESS`.
7. MySQL en el puerto `3306`.
8. MongoDB en el puerto `27017`. No es obligatorio instalar MongoDB Compass.

## Preparacion inicial de bases de datos

Realizar estos pasos una sola vez en una computadora nueva y con los servicios de base de datos encendidos.

1. En SQL Server Management Studio, ejecutar `ProveedorJava/proveedor_db.sql`. Advertencia: el script recrea las tablas y reinicia los datos de demostracion.
2. En MySQL Workbench, ejecutar en este orden:
   - `Proyecto/identificador/database/schema.sql`
   - `Proyecto/identificador/database/seed_pruebas.sql`
   - `FEM/scripts/preparar_usuario_mysql.sql`, usando una cuenta administradora.
3. Encender el servicio local de MongoDB. Los usuarios y nombres de clientes se crean automaticamente durante el arranque.

## Ejecucion completa

Desde PowerShell:

```powershell
Set-Location -LiteralPath 'E:\p\Sistema-Control-Llamadas-Alcance2-main (2)\Sistema-Control-Llamadas-Alcance2-main'
.\INICIAR_SERVICIOS.bat
```

Tambien se puede abrir `INICIAR_SERVICIOS.bat` con doble clic. El lanzador restaura y compila los servicios web, prepara Python, compila Java, inicializa los usuarios de MongoDB, levanta todos los puertos y abre la aplicacion.

URL de la aplicacion: `http://localhost:5155/`

No cerrar las ventanas de Python, Java o IIS Express mientras se utiliza el sistema.

## Credenciales de demostracion

Administrador:

- Usuario: `admin`
- Contrasena: `Admin12345678!`

Ejemplo de cliente asociado a la linea `25743715`:

- Usuario: `cliente101110111`
- Contrasena: `Cliente2026!Ab`

Los demas clientes precargados usan el usuario `cliente` seguido por su identificacion y la misma contrasena de demostracion.

## Prueba recomendada de ADM1 a ADM5

1. Intentar ingresar como administrador con una contrasena incorrecta y verificar `Usuario y/o contrasena incorrectos`.
2. Ingresar con las credenciales correctas y comprobar el menu y el pie de pagina.
3. En `Nuevas lineas`, registrar una linea con telefono de 8 digitos, identificador de telefono de 16 digitos e identificador de tarjeta de 19 digitos.
4. Confirmar que aparece en el listado de disponibles.
5. En `Activar linea`, seleccionarla y usar una identificacion de cliente de 9 digitos, por ejemplo `101110111`.
6. En `Devolucion de linea`, confirmar que aparecen identificacion y nombre del cliente, y desactivar la linea.
7. Regresar a `Nuevas lineas` y eliminarla aceptando la confirmacion.
8. Cerrar sesion y comprobar que una ruta administrativa vuelve a pedir ingreso.

## Entrega limpia

Antes de crear el ZIP se pueden excluir estas carpetas generadas: `.venv`, `bin`, `obj`, `build`, `build_verificacion`, `logs`, `out` y `__pycache__`. No se deben borrar archivos fuente, soluciones, scripts SQL, `FEM/scripts`, el controlador JDBC ni `INICIAR_SERVICIOS.bat`.

La primera ejecucion en otra computadora puede tardar porque restaura paquetes NuGet e instala las dependencias de Python.
