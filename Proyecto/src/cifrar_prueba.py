import base64
import os
from cryptography.hazmat.primitives.ciphers.aead import AESGCM

AES_KEY_BASE64 = "XP2jwjvXFogsw3DHWywVFIU2ZS1J6IYEDIzvoyve/dg="

def encrypt(text):
    key = base64.b64decode(AES_KEY_BASE64)
    aes = AESGCM(key)
    nonce = os.urandom(12)
    cipher = aes.encrypt(nonce, str(text).encode("utf-8"), None)
    return base64.b64encode(nonce + cipher).decode("utf-8")

print(encrypt("25743715"))