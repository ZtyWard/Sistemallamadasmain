-- Credenciales locales esperadas por Proyecto/src/main.py.
-- Ejecutar en MySQL Workbench con una cuenta administradora.

CREATE USER IF NOT EXISTS 'identificador_user'@'localhost'
IDENTIFIED BY 'Identificador123';

ALTER USER 'identificador_user'@'localhost'
IDENTIFIED BY 'Identificador123';

GRANT ALL PRIVILEGES ON identificador_db.*
TO 'identificador_user'@'localhost';

FLUSH PRIVILEGES;
