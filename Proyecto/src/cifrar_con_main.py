from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent / "src"))

from main import Config, CryptoBox

cfg = Config()
crypto = CryptoBox(cfg.aes_key_b64)

telefono = "25743715"

token = crypto.encrypt(telefono)

print("LLAVE USADA:")
print(cfg.aes_key_b64)
print()
print("TELEFONO CIFRADO:")
print(token)
print()
print("PRUEBA DESCIFRADO:")
print(crypto.decrypt(token))