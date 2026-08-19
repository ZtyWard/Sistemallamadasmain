USE identificador_db;

INSERT IGNORE INTO proveedores(codigo, nombre, host, puerto, activo) VALUES
('P1', 'Proveedor principal', '127.0.0.1', 6000, TRUE),
('P2', 'Proveedor secundario', '127.0.0.1', 6000, TRUE);

INSERT IGNORE INTO tarjetas(identificador_chip_enc, identificador_chip_hash, activo) VALUES
('seed:1234567890123456789', SHA2('1234567890123456789', 256), TRUE),
('seed:2222222222222222222', SHA2('2222222222222222222', 256), TRUE),
('seed:3333333333333333333', SHA2('3333333333333333333', 256), TRUE),
('seed:4444444444444444444', SHA2('4444444444444444444', 256), TRUE),
('seed:5555555555555555555', SHA2('5555555555555555555', 256), TRUE),
('seed:6666666666666666666', SHA2('6666666666666666666', 256), TRUE),
('seed:7777777777777777777', SHA2('7777777777777777777', 256), TRUE),
('seed:8888888888888888888', SHA2('8888888888888888888', 256), TRUE),
('seed:9999999999999999999', SHA2('9999999999999999999', 256), TRUE);

INSERT IGNORE INTO telefonos(
  numero_enc,
  numero_hash,
  identificador_tel_enc,
  identificador_tel_hash,
  tarjeta_id,
  proveedor_id,
  tipo_servicio,
  activo
) VALUES
(
  'seed:25743715',
  SHA2('25743715', 256),
  'seed:1234567890123456',
  SHA2('1234567890123456', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('1234567890123456789', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P1'),
  'PREPAGO',
  TRUE
),
(
  'seed:25262020',
  SHA2('25262020', 256),
  'seed:2222222222222222',
  SHA2('2222222222222222', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('2222222222222222222', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P1'),
  'PREPAGO',
  TRUE
),
(
  'seed:22334455',
  SHA2('22334455', 256),
  'seed:3333333333333333',
  SHA2('3333333333333333', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('3333333333333333333', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P1'),
  'PREPAGO',
  TRUE
),
(
  'seed:89154242',
  SHA2('89154242', 256),
  'seed:4444444444444444',
  SHA2('4444444444444444', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('4444444444444444444', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P2'),
  'POSTPAGO',
  TRUE
),
(
  'seed:88889999',
  SHA2('88889999', 256),
  'seed:5555555555555555',
  SHA2('5555555555555555', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('5555555555555555555', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P2'),
  'PREPAGO',
  TRUE
),
(
  'seed:70001122',
  SHA2('70001122', 256),
  'seed:6666666666666666',
  SHA2('6666666666666666', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('6666666666666666666', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P2'),
  'PREPAGO',
  TRUE
),
(
  'seed:88888888',
  SHA2('88888888', 256),
  'seed:7777777777777777',
  SHA2('7777777777777777', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('7777777777777777777', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P2'),
  'PREPAGO',
  TRUE
),
(
  'seed:87654321',
  SHA2('87654321', 256),
  'seed:8888888888888888',
  SHA2('8888888888888888', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('8888888888888888888', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P2'),
  'PREPAGO',
  TRUE
),
(
  'seed:22223333',
  SHA2('22223333', 256),
  'seed:9999999999999999',
  SHA2('9999999999999999', 256),
  (SELECT id FROM tarjetas WHERE identificador_chip_hash = SHA2('9999999999999999999', 256)),
  (SELECT id FROM proveedores WHERE codigo = 'P1'),
  'PREPAGO',
  TRUE
);

INSERT IGNORE INTO codigos_pais(codigo,pais,activo) VALUES
('505','Nicaragua',TRUE),
('52','Mexico',TRUE),
('54','Argentina',TRUE),
('55','Brasil',TRUE),
('56','Chile',TRUE),
('58','Venezuela',TRUE),
('33','Francia',TRUE),
('39','Italia',TRUE),
('44','Reino Unido',TRUE),
('49','Alemania',TRUE),
('53','Cuba',TRUE);

-- Sincroniza el teléfono postpago si la base ya contenía una versión anterior.
UPDATE telefonos
SET identificador_tel_enc = 'seed:4444444444444444',
    identificador_tel_hash = SHA2('4444444444444444', 256),
    tarjeta_id = (
      SELECT id FROM tarjetas
      WHERE identificador_chip_hash = SHA2('4444444444444444444', 256)
    ),
    proveedor_id = (SELECT id FROM proveedores WHERE codigo = 'P2'),
    tipo_servicio = 'POSTPAGO',
    activo = TRUE
WHERE numero_hash = SHA2('89154242', 256);
