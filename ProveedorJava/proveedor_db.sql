IF DB_ID('ProveedorTelefonicoDB') IS NULL
BEGIN
    CREATE DATABASE ProveedorTelefonicoDB;
END
GO

USE ProveedorTelefonicoDB;
GO

/* ==========================================================
   LIMPIEZA CONTROLADA
   ========================================================== */

IF OBJECT_ID('dbo.sp_calcular_facturacion_postpago', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_calcular_facturacion_postpago;
GO

IF OBJECT_ID('dbo.facturas', 'U') IS NOT NULL DROP TABLE dbo.facturas;
IF OBJECT_ID('dbo.movimientos', 'U') IS NOT NULL DROP TABLE dbo.movimientos;
IF OBJECT_ID('dbo.tarjetas', 'U') IS NOT NULL DROP TABLE dbo.tarjetas;
IF OBJECT_ID('dbo.telefonos', 'U') IS NOT NULL DROP TABLE dbo.telefonos;
IF OBJECT_ID('dbo.tarifas', 'U') IS NOT NULL DROP TABLE dbo.tarifas;
IF OBJECT_ID('dbo.clientes', 'U') IS NOT NULL DROP TABLE dbo.clientes;
GO

/* ==========================================================
   TABLA CLIENTES

   Responsabilidad:
   Representa al propietario de una o varias lineas telefonicas.
   No almacena telefono, saldo ni tipo de servicio para evitar
   duplicacion con la tabla telefonos.
   ========================================================== */

CREATE TABLE dbo.clientes (
    id INT IDENTITY(1,1) NOT NULL,
    identificacion VARCHAR(20) NOT NULL,
    activo BIT NOT NULL CONSTRAINT DF_clientes_activo DEFAULT 1,

    CONSTRAINT PK_clientes PRIMARY KEY (id),
    CONSTRAINT UQ_clientes_identificacion UNIQUE (identificacion),
    CONSTRAINT CK_clientes_identificacion_no_vacia
        CHECK (LEN(LTRIM(RTRIM(identificacion))) > 0)
);
GO

/* ==========================================================
   TABLA TELEFONOS

   Responsabilidad:
   Representa la linea telefonica. Aqui vive el numero,
   el tipo de servicio, el estado y el saldo operativo.
   ========================================================== */

CREATE TABLE dbo.telefonos (
    id INT IDENTITY(1,1) NOT NULL,
    telefono VARCHAR(20) NOT NULL,
    identificador_telefono VARCHAR(16) NOT NULL,
    tipo_servicio VARCHAR(10) NOT NULL,
    estado VARCHAR(15) NOT NULL,
    cliente_id INT NULL,
    saldo DECIMAL(10,2) NOT NULL CONSTRAINT DF_telefonos_saldo DEFAULT 0,
    bono_mismo_proveedor DECIMAL(10,2) NOT NULL
        CONSTRAINT DF_telefonos_bono_mismo_proveedor DEFAULT 0,
    fecha_activacion DATETIME NULL,

    CONSTRAINT PK_telefonos PRIMARY KEY (id),
    CONSTRAINT UQ_telefonos_telefono UNIQUE (telefono),
    CONSTRAINT UQ_telefonos_identificador UNIQUE (identificador_telefono),
    CONSTRAINT FK_telefonos_clientes
        FOREIGN KEY (cliente_id) REFERENCES dbo.clientes(id),
    CONSTRAINT CK_telefonos_tipo_servicio
        CHECK (tipo_servicio IN ('PREPAGO', 'POSTPAGO')),
    CONSTRAINT CK_telefonos_estado
        CHECK (estado IN ('DISPONIBLE', 'ACTIVO', 'INACTIVO')),
    CONSTRAINT CK_telefonos_saldo
        CHECK (saldo >= 0),
    CONSTRAINT CK_telefonos_bono_mismo_proveedor
        CHECK (bono_mismo_proveedor >= 0),
    CONSTRAINT CK_telefonos_identificador_telefono
        CHECK (
            LEN(identificador_telefono) = 16
            AND identificador_telefono NOT LIKE '%[^0-9]%'
        )
);
GO

/* ==========================================================
   TABLA TARJETAS

   Responsabilidad:
   Representa la tarjeta fisica asociada a una linea.
   ========================================================== */

CREATE TABLE dbo.tarjetas (
    id INT IDENTITY(1,1) NOT NULL,
    identificador_tarjeta VARCHAR(19) NOT NULL,
    telefono_id INT NOT NULL,
    estado VARCHAR(15) NOT NULL,

    CONSTRAINT PK_tarjetas PRIMARY KEY (id),
    CONSTRAINT UQ_tarjetas_identificador UNIQUE (identificador_tarjeta),
    CONSTRAINT UQ_tarjetas_telefono UNIQUE (telefono_id),
    CONSTRAINT FK_tarjetas_telefonos
        FOREIGN KEY (telefono_id) REFERENCES dbo.telefonos(id),
    CONSTRAINT CK_tarjetas_estado
        CHECK (estado IN ('DISPONIBLE', 'ACTIVA', 'INACTIVA')),
    CONSTRAINT CK_tarjetas_identificador_tarjeta
        CHECK (
            LEN(identificador_tarjeta) = 19
            AND identificador_tarjeta NOT LIKE '%[^0-9]%'
        )
);
GO

/* ==========================================================
   TABLA TARIFAS

   Responsabilidad:
   Catalogo de tarifas por tipo de llamada.
   ========================================================== */

CREATE TABLE dbo.tarifas (
    id INT IDENTITY(1,1) NOT NULL,
    tipo_llamada VARCHAR(30) NOT NULL,
    costo_minuto DECIMAL(10,2) NOT NULL,

    CONSTRAINT PK_tarifas PRIMARY KEY (id),
    CONSTRAINT UQ_tarifas_tipo_llamada UNIQUE (tipo_llamada),
    CONSTRAINT CK_tarifas_tipo_llamada
        CHECK (tipo_llamada IN (
            'MISMO_PROVEEDOR',
            'OTRO_PROVEEDOR',
            'INTERNACIONAL'
        )),
    CONSTRAINT CK_tarifas_costo_minuto
        CHECK (costo_minuto > 0)
);
GO

/* ==========================================================
   TABLA MOVIMIENTOS

   Responsabilidad:
   Historial de llamadas realizadas por una linea.
   ========================================================== */

CREATE TABLE dbo.movimientos (
    id INT IDENTITY(1,1) NOT NULL,
    telefono_id INT NOT NULL,
    tarifa_id INT NOT NULL,
    fecha_llamada DATETIME NOT NULL,
    telefono_destino VARCHAR(20) NOT NULL,
    costo DECIMAL(10,2) NOT NULL,
    duracion VARCHAR(6) NOT NULL,

    CONSTRAINT PK_movimientos PRIMARY KEY (id),
    CONSTRAINT FK_movimientos_telefonos
        FOREIGN KEY (telefono_id) REFERENCES dbo.telefonos(id),
    CONSTRAINT FK_movimientos_tarifas
        FOREIGN KEY (tarifa_id) REFERENCES dbo.tarifas(id),
    CONSTRAINT CK_movimientos_costo
        CHECK (costo >= 0),
    CONSTRAINT CK_movimientos_duracion
        CHECK (
            LEN(duracion) = 6
            AND duracion NOT LIKE '%[^0-9]%'
        ),
    CONSTRAINT CK_movimientos_destino_no_vacio
        CHECK (LEN(LTRIM(RTRIM(telefono_destino))) > 0)
);
GO

/* ==========================================================
   TABLA FACTURAS

   Responsabilidad:
   Resultado del calculo de facturacion para lineas postpago.
   ========================================================== */

CREATE TABLE dbo.facturas (
    id INT IDENTITY(1,1) NOT NULL,
    telefono_id INT NOT NULL,
    fecha_calculo DATE NOT NULL,
    fecha_maxima_pago DATE NOT NULL,
    monto DECIMAL(10,2) NOT NULL,
    pagada BIT NOT NULL CONSTRAINT DF_facturas_pagada DEFAULT 0,

    CONSTRAINT PK_facturas PRIMARY KEY (id),
    CONSTRAINT FK_facturas_telefonos
        FOREIGN KEY (telefono_id) REFERENCES dbo.telefonos(id),
    CONSTRAINT UQ_facturas_telefono_fecha
        UNIQUE (telefono_id, fecha_calculo),
    CONSTRAINT CK_facturas_monto
        CHECK (monto >= 0),
    CONSTRAINT CK_facturas_fechas
        CHECK (fecha_maxima_pago >= fecha_calculo)
);
GO

/* ==========================================================
   INDICES
   ========================================================== */

CREATE INDEX IX_telefonos_cliente_id
ON dbo.telefonos(cliente_id);
GO

CREATE INDEX IX_movimientos_telefono_fecha
ON dbo.movimientos(telefono_id, fecha_llamada);
GO

CREATE INDEX IX_facturas_telefono_pagada
ON dbo.facturas(telefono_id, pagada);
GO

/* ==========================================================
   DATOS DE PRUEBA
   ========================================================== */

INSERT INTO dbo.clientes (identificacion, activo)
VALUES
('101110111', 1),
('202220222', 1),
('303330333', 1),
('404440444', 1),
('505550555', 1),
('606660666', 1),
('707770777', 1),
('808880888', 1),
('909990999', 1);
GO

INSERT INTO dbo.telefonos (
    telefono,
    identificador_telefono,
    tipo_servicio,
    estado,
    cliente_id,
    saldo,
    bono_mismo_proveedor,
    fecha_activacion
)
VALUES
('25743715', '1234567890123456', 'PREPAGO', 'ACTIVO', 1, 5000.00, 0.00, GETDATE()),
('25262020', '2222222222222222', 'PREPAGO', 'ACTIVO', 2, 2500.00, 0.00, GETDATE()),
('22334455', '3333333333333333', 'PREPAGO', 'ACTIVO', 3, 1500.00, 0.00, GETDATE()),
('88887777', '1000000000000004', 'PREPAGO', 'ACTIVO', 4, 1000.00, 0.00, GETDATE()),
('88888888', '1000000000000005', 'PREPAGO', 'ACTIVO', 5, 1800.00, 0.00, GETDATE()),
('87654321', '1000000000000006', 'PREPAGO', 'ACTIVO', 6, 2200.00, 0.00, GETDATE()),
('22223333', '1000000000000007', 'PREPAGO', 'ACTIVO', 7, 1200.00, 0.00, GETDATE()),
('88886666', '1000000000000008', 'POSTPAGO', 'ACTIVO', 8, 0.00, 0.00, GETDATE()),
('89154242', '4444444444444444', 'POSTPAGO', 'ACTIVO', 9, 0.00, 0.00, GETDATE()),
('70000000', '1000000000000010', 'PREPAGO', 'DISPONIBLE', NULL, 0.00, 0.00, NULL);
GO

INSERT INTO dbo.tarjetas (
    identificador_tarjeta,
    telefono_id,
    estado
)
VALUES
('1234567890123456789', 1, 'ACTIVA'),
('2222222222222222222', 2, 'ACTIVA'),
('3333333333333333333', 3, 'ACTIVA'),
('2000000000000000004', 4, 'ACTIVA'),
('2000000000000000005', 5, 'ACTIVA'),
('2000000000000000006', 6, 'ACTIVA'),
('2000000000000000007', 7, 'ACTIVA'),
('2000000000000000008', 8, 'ACTIVA'),
('4444444444444444444', 9, 'ACTIVA'),
('2000000000000000010', 10, 'DISPONIBLE');
GO

INSERT INTO dbo.tarifas (
    tipo_llamada,
    costo_minuto
)
VALUES
('MISMO_PROVEEDOR', 25.13),
('OTRO_PROVEEDOR', 8.72),
('INTERNACIONAL', 120.00);
GO

INSERT INTO dbo.movimientos (
    telefono_id,
    tarifa_id,
    fecha_llamada,
    telefono_destino,
    costo,
    duracion
)
VALUES
(8, 1, DATEADD(DAY, -3, GETDATE()), '25743715', 75.39, '000180'),
(8, 2, DATEADD(DAY, -2, GETDATE()), '40001234', 70.00, '000120'),
(9, 3, DATEADD(DAY, -1, GETDATE()), '0013055551212', 240.00, '000120');
GO

/* ==========================================================
   PROCEDIMIENTO: FACTURACION POSTPAGO

   Calcula el monto acumulado de movimientos para cada linea
   postpago activa hasta la fecha de calculo indicada.
   ========================================================== */

CREATE PROCEDURE dbo.sp_calcular_facturacion_postpago
    @fecha_calculo DATE,
    @fecha_maxima_pago DATE
AS
BEGIN
    SET NOCOUNT ON;

    IF @fecha_calculo IS NULL OR @fecha_maxima_pago IS NULL
    BEGIN
        RAISERROR('Las fechas son obligatorias.', 16, 1);
        RETURN;
    END

    IF @fecha_maxima_pago < @fecha_calculo
    BEGIN
        RAISERROR('La fecha maxima de pago no puede ser menor que la fecha de calculo.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.facturas (
        telefono_id,
        fecha_calculo,
        fecha_maxima_pago,
        monto,
        pagada
    )
    SELECT
        t.id,
        @fecha_calculo,
        @fecha_maxima_pago,
        ISNULL(SUM(m.costo), 0),
        0
    FROM dbo.telefonos t
    LEFT JOIN dbo.movimientos m
        ON m.telefono_id = t.id
        AND CAST(m.fecha_llamada AS DATE) <= @fecha_calculo
        AND CAST(m.fecha_llamada AS DATE) > ISNULL((
            SELECT MAX(f_prev.fecha_calculo)
            FROM dbo.facturas f_prev
            WHERE f_prev.telefono_id = t.id
              AND f_prev.fecha_calculo < @fecha_calculo
        ), '1900-01-01')
    WHERE t.tipo_servicio = 'POSTPAGO'
      AND t.estado = 'ACTIVO'
      AND NOT EXISTS (
          SELECT 1
          FROM dbo.facturas f
          WHERE f.telefono_id = t.id
            AND f.fecha_calculo = @fecha_calculo
      )
    GROUP BY t.id;
END
GO

/* ==========================================================
   LOGIN PARA JAVA
   ========================================================== */

IF NOT EXISTS (
    SELECT 1
    FROM sys.server_principals
    WHERE name = 'javauser'
)
BEGIN
    CREATE LOGIN javauser
    WITH PASSWORD = 'Java123456';
END
GO

USE ProveedorTelefonicoDB;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.database_principals
    WHERE name = 'javauser'
)
BEGIN
    CREATE USER javauser
    FOR LOGIN javauser;
END
GO

ALTER ROLE db_owner ADD MEMBER javauser;
GO
