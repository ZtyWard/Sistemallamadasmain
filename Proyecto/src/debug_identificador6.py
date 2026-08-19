import importlib.util
import traceback

spec = importlib.util.spec_from_file_location('main_mod', 'c:/Users/jm076/Downloads/main_backup.py/Proyecto/src/main.py')
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)

server = mod.IdentificadorServer(mod.Config())
raw = 'IDENTIFICADOR6|inOywBgxi/ttFuxk0MiOPpMQ9wA94YreeCtO6yT/FG4a5ov9|9I2sbPdMQ1RbeDN1yc89VCd/Q3I+NGI0MIyUKNLsyJPhbos+41JGQNfTDAQ=|vIzFod3DG6nbgfzvEpr4l+ZBpYslAHf127pi92K5Qo9+1zLKIJ0s6/Hi3wzV138=|PREPAGO|118880999|activo|P1'

partes = raw.strip().split('|')
print('partes', len(partes), partes)
telefono_enc = partes[1].strip()
identificador_tel_enc = partes[2].strip()
identificador_chip_enc = partes[3].strip()
tipo_servicio = partes[4].strip().upper()
identificacion_cliente = partes[5].strip()
estado = partes[6].strip().lower()
codigo_proveedor = partes[7].strip().upper()

print('decrypting')
try:
    telefono = server.crypto.decrypt(telefono_enc)
    identificador_tel = server.crypto.decrypt(identificador_tel_enc)
    identificador_chip = server.crypto.decrypt(identificador_chip_enc)
except Exception:
    telefono = telefono_enc
    identificador_tel = identificador_tel_enc
    identificador_chip = identificador_chip_enc
print('decrypted', telefono, identificador_tel, identificador_chip)

print('provider lookup')
proveedor = server.db.get_provider_by_code(codigo_proveedor)
print('provider', proveedor)

numero_hash = server.crypto.digest(telefono)
identificador_tel_hash = server.crypto.digest(identificador_tel)
identificador_chip_hash = server.crypto.digest(identificador_chip)
print('hashes', numero_hash, identificador_tel_hash, identificador_chip_hash)

print('card')
tarjeta_id = server.db.get_or_create_card(identificador_chip_enc, identificador_chip_hash)
print('tarjeta_id', tarjeta_id)

activar = estado == 'activo'
print('activar', activar)

print('lookup phone')
telefono_existente = server.db.get_phone_simple_by_hash(numero_hash)
if not telefono_existente:
    telefono_existente = server.db.get_phone_by_identificador_tel_hash(identificador_tel_hash)
print('telefono_existente', telefono_existente)

try:
    if telefono_existente:
        print('updating phone')
        server.db.update_phone_identificador6(
            numero_hash=numero_hash,
            identificador_tel_enc=identificador_tel_enc,
            identificador_tel_hash=identificador_tel_hash,
            tarjeta_id=tarjeta_id,
            proveedor_id=proveedor['id'],
            tipo_servicio=tipo_servicio,
            identificacion_cliente=identificacion_cliente,
            activo=activar
        )
    else:
        print('inserting phone')
        server.db.insert_phone_identificador6(
            numero_enc=telefono_enc,
            numero_hash=numero_hash,
            identificador_tel_enc=identificador_tel_enc,
            identificador_tel_hash=identificador_tel_hash,
            tarjeta_id=tarjeta_id,
            proveedor_id=proveedor['id'],
            tipo_servicio=tipo_servicio,
            identificacion_cliente=identificacion_cliente,
            activo=activar
        )
    print('done')
except Exception:
    traceback.print_exc()
