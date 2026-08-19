import socket
from main import Config, CryptoBox

# ============================================================
# Probador de IDENTIFICADOR6
# Usa la misma llave AES del main.py para evitar errores
# de descifrado.
# ============================================================

cfg = Config()
crypto = CryptoBox(cfg.aes_key_b64)

telefono_enc = crypto.encrypt("25743715")
identificador_tel_enc = crypto.encrypt("1234567890123456")
identificador_chip_enc = crypto.encrypt("1234567890123456789")

trama = (
    "IDENTIFICADOR6|"
    f"{telefono_enc}|"
    f"{identificador_tel_enc}|"
    f"{identificador_chip_enc}|"
    "PREPAGO|"
    "101010101|"
    "activo|"
    "P1"
)

print("Trama enviada:")
print(trama)
print()

with socket.create_connection(("127.0.0.1", 5000), timeout=10) as s:
    s.sendall((trama + "\n").encode("utf-8"))
    respuesta = s.recv(4096).decode("utf-8").strip()

print("Respuesta del Identificador:")
print(respuesta)