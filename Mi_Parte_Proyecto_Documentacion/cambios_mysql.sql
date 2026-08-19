USE identificador_db;

-- ============================================================
-- COLUMNA: identificacion_cliente
-- Se agrega para IDENTIFICADOR6.
-- Guarda la identificación/cédula del cliente dueño de la línea.
-- ============================================================

SET @existe_columna := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'telefonos'
      AND COLUMN_NAME = 'identificacion_cliente'
);

SET @sql := IF(
    @existe_columna = 0,
    'ALTER TABLE telefonos ADD COLUMN identificacion_cliente VARCHAR(30) NULL AFTER tipo_servicio',
    'SELECT "La columna identificacion_cliente ya existe"'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================
-- COLUMNA: estado_linea
-- Se agrega para controlar ACTIVO / INACTIVO.
-- ============================================================

SET @existe_columna := (
    SELECT COUNT(*)
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'telefonos'
      AND COLUMN_NAME = 'estado_linea'
);

SET @sql := IF(
    @existe_columna = 0,
    'ALTER TABLE telefonos ADD COLUMN estado_linea VARCHAR(20) NOT NULL DEFAULT "ACTIVO" AFTER activo',
    'SELECT "La columna estado_linea ya existe"'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- ============================================================
-- PROVEEDOR P1
-- Se asegura que exista el proveedor usado en las pruebas.
-- ============================================================

INSERT INTO proveedores(codigo, nombre, activo)
SELECT 'P1', 'Proveedor principal', TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM proveedores
    WHERE codigo = 'P1'
);

UPDATE proveedores
SET activo = TRUE
WHERE codigo = 'P1';

-- ============================================================
-- VERIFICACIÓN FINAL
-- ============================================================

SELECT id, codigo, nombre, activo
FROM proveedores;

DESCRIBE telefonos;
