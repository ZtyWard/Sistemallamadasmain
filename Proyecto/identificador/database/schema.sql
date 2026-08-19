CREATE DATABASE IF NOT EXISTS identificador_db
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE identificador_db;

CREATE TABLE IF NOT EXISTS proveedores (
  id INT AUTO_INCREMENT PRIMARY KEY,
  codigo VARCHAR(20) NOT NULL UNIQUE,
  nombre VARCHAR(80) NOT NULL,
  host VARCHAR(120) NOT NULL,
  puerto INT NOT NULL,
  activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS tarjetas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  identificador_chip_enc TEXT NOT NULL,
  identificador_chip_hash CHAR(64) NOT NULL UNIQUE,
  activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS telefonos (
  id INT AUTO_INCREMENT PRIMARY KEY,
  numero_enc TEXT NOT NULL,
  numero_hash CHAR(64) NOT NULL UNIQUE,
  identificador_tel_enc TEXT NOT NULL,
  identificador_tel_hash CHAR(64) NOT NULL UNIQUE,
  tarjeta_id INT NOT NULL,
  proveedor_id INT NOT NULL,
  tipo_servicio ENUM('PREPAGO','POSTPAGO') NOT NULL DEFAULT 'PREPAGO',
  identificacion_cliente VARCHAR(20) NULL,
  activo BOOLEAN NOT NULL DEFAULT TRUE,
  estado_linea ENUM('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
  FOREIGN KEY (tarjeta_id) REFERENCES tarjetas(id),
  FOREIGN KEY (proveedor_id) REFERENCES proveedores(id)
);

SET @sql_identificacion_cliente = IF(
  (SELECT COUNT(*) FROM information_schema.columns
   WHERE table_schema = DATABASE()
     AND table_name = 'telefonos'
     AND column_name = 'identificacion_cliente') = 0,
  'ALTER TABLE telefonos ADD COLUMN identificacion_cliente VARCHAR(20) NULL AFTER tipo_servicio',
  'SELECT 1'
);
PREPARE stmt_identificacion_cliente FROM @sql_identificacion_cliente;
EXECUTE stmt_identificacion_cliente;
DEALLOCATE PREPARE stmt_identificacion_cliente;

SET @sql_estado_linea = IF(
  (SELECT COUNT(*) FROM information_schema.columns
   WHERE table_schema = DATABASE()
     AND table_name = 'telefonos'
     AND column_name = 'estado_linea') = 0,
  'ALTER TABLE telefonos ADD COLUMN estado_linea ENUM(''ACTIVO'',''INACTIVO'') NOT NULL DEFAULT ''ACTIVO'' AFTER activo',
  'SELECT 1'
);
PREPARE stmt_estado_linea FROM @sql_estado_linea;
EXECUTE stmt_estado_linea;
DEALLOCATE PREPARE stmt_estado_linea;

CREATE TABLE IF NOT EXISTS codigos_pais (
  codigo VARCHAR(4) PRIMARY KEY,
  pais VARCHAR(80) NOT NULL,
  activo BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS llamadas (
  id INT AUTO_INCREMENT PRIMARY KEY,
  telefono_id INT NOT NULL,
  telefono_destino_enc TEXT NOT NULL,
  telefono_destino_hash CHAR(64) NOT NULL,
  tipo_llamada ENUM('MISMO_PROVEEDOR','OTRO_PROVEEDOR','INTERNACIONAL') NOT NULL,
  fecha_inicio DATETIME NOT NULL,
  fecha_fin DATETIME NULL,
  duracion_segundos INT NULL,
  tarifa_minuto DECIMAL(10,2) NOT NULL DEFAULT 0,
  costo DECIMAL(10,2) NULL,
  estado ENUM('ACTIVA','FINALIZADA','FALLIDA') NOT NULL DEFAULT 'ACTIVA',
  respuesta_proveedor VARCHAR(20) NULL,
  FOREIGN KEY (telefono_id) REFERENCES telefonos(id)
);

INSERT IGNORE INTO codigos_pais(codigo,pais) VALUES
('507','Panamá'),('502','Guatemala'),('503','El Salvador'),
('504','Honduras'),('501','Belice'),('57','Colombia'),
('1','Estados Unidos'),('34','España');
