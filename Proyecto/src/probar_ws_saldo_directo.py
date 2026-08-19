import base64
import os
import socket
from cryptography.hazmat.primitives.ciphers.aead import AESGCM


AES_KEY_BASE64 = os.getenv("AES_KEY_BASE64", "XP2jwjvXFogsw3DHWywVFIU2ZS1J6IYEDIzvoyve/dg=")


def encrypt(text):
    key = base64.b64decode(AES_KEY_BASE64)
    if len(key) not in (16, 24, 32):
        raise ValueError("La llave AES debe ser de 16, 24 o 32 bytes.")

    aes = AESGCM(key)
    nonce = os.urandom(12)
    cipher = aes.encrypt(nonce, str(text).encode("utf-8"), None)

    return base64.b64encode(nonce + cipher).decode("utf-8")


# Usamos el número que el Proveedor Java sí respondió OK.
telefono_encriptado = encrypt("25743715")

trama = f"WS_SALDO|{telefono_encriptado}|WEB|saldo"

print("Trama enviada:")
print(trama)
print()

with socket.create_connection(("127.0.0.1", 5000), timeout=10) as s:
    s.sendall((trama + "\n").encode("utf-8"))
    respuesta = s.recv(4096).decode("utf-8").strip()

print("Respuesta del Identificador Python:")
print(respuesta)