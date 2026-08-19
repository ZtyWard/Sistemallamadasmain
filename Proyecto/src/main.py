import base64, hashlib, heapq, json, math, os, queue, socket, threading, time
from dataclasses import dataclass, field
from datetime import datetime, timedelta
from decimal import Decimal, ROUND_HALF_UP

import mysql.connector
from cryptography.hazmat.primitives.ciphers.aead import AESGCM


# =========================
# CONFIGURACION
# =========================

@dataclass
class Config:
    host: str = os.getenv("IDENT_HOST", "0.0.0.0")
    port: int = int(os.getenv("IDENT_PORT", "5000"))

    mysql_host: str = os.getenv("MYSQL_HOST", "localhost")
    mysql_port: int = int(os.getenv("MYSQL_PORT", "3306"))
    mysql_user: str = os.getenv("MYSQL_USER", "identificador_user")
    mysql_password: str = os.getenv("MYSQL_PASSWORD", "Identificador123")
    mysql_db: str = os.getenv("MYSQL_DB", "identificador_db")

    proveedor_host: str = os.getenv("PROVEEDOR_HOST", "127.0.0.1")
    proveedor_port: int = int(os.getenv("PROVEEDOR_PORT", "6000"))

    log_path: str = os.getenv("LOG_PATH", "logs/identificador.log")
    aes_key_b64: str = os.getenv(
        "AES_KEY_BASE64",
        "XP2jwjvXFogsw3DHWywVFIU2ZS1J6IYEDIzvoyve/dg="
    )
    check_interval: int = int(os.getenv("CHECK_INTERVAL", "2"))


# =========================
# AES + HASH
# =========================

class CryptoBox:
    def __init__(self, key_b64):
        if not key_b64:
            raise RuntimeError("Debe configurar AES_KEY_BASE64.")
        key = base64.b64decode(key_b64)
        if len(key) not in (16, 24, 32):
            raise RuntimeError("La llave AES debe ser de 16, 24 o 32 bytes.")
        self.aes = AESGCM(key)

    def encrypt(self, text):
        nonce = os.urandom(12)
        cipher = self.aes.encrypt(nonce, str(text).encode("utf-8"), None)
        return base64.b64encode(nonce + cipher).decode("utf-8")

    def decrypt(self, token):
        raw = base64.b64decode(token)
        nonce, cipher = raw[:12], raw[12:]
        return self.aes.decrypt(nonce, cipher, None).decode("utf-8")

    def digest(self, text):
        return hashlib.sha256(str(text).strip().encode("utf-8")).hexdigest()


# =========================
# BASE DE DATOS MYSQL
# =========================

class Database:
    def __init__(self, cfg):
        self.cfg = cfg

    def connect(self):
        return mysql.connector.connect(
            host=self.cfg.mysql_host,
            port=self.cfg.mysql_port,
            user=self.cfg.mysql_user,
            password=self.cfg.mysql_password,
            database=self.cfg.mysql_db
        )
    # ============================================================
    # IDENTIFICADOR6
    # Busca un proveedor por código. Ejemplo: P1, P2.
    # Esto sirve para asociar la línea al proveedor correcto.
    # ============================================================
    def get_provider_by_code(self, codigo):
        return self.fetchone("""
            SELECT id, codigo, activo
            FROM proveedores
            WHERE codigo = %s
            LIMIT 1
        """, (codigo,))

    # ============================================================
    # IDENTIFICADOR6
    # Inserta una tarjeta si todavía no existe.
    # Si ya existe, devuelve el ID existente.
    # ============================================================
    def get_or_create_card(self, chip_enc, chip_hash):
        existing = self.fetchone("""
            SELECT id
            FROM tarjetas
            WHERE identificador_chip_hash = %s
            LIMIT 1
        """, (chip_hash,))

        if existing:
            return existing["id"]

        return self.execute("""
            INSERT INTO tarjetas
            (identificador_chip_enc, identificador_chip_hash, activo)
            VALUES (%s, %s, TRUE)
        """, (chip_enc, chip_hash))

    # ============================================================
    # IDENTIFICADOR6
    # Busca un teléfono por número cifrado/hash.
    # ============================================================
    def get_phone_simple_by_hash(self, numero_hash):
        return self.fetchone("""
            SELECT id, numero_hash, activo, identificacion_cliente
            FROM telefonos
            WHERE numero_hash = %s
            LIMIT 1
        """, (numero_hash,))

    def get_phone_by_identificador_tel_hash(self, identificador_tel_hash):
        return self.fetchone("""
            SELECT id, numero_hash, activo, identificacion_cliente
            FROM telefonos
            WHERE identificador_tel_hash = %s
            LIMIT 1
        """, (identificador_tel_hash,))

    # ============================================================
    # IDENTIFICADOR6
    # Registra una línea nueva en el identificador.
    # Se guarda el valor cifrado y también el hash para búsquedas.
    # ============================================================
    def insert_phone_identificador6(
        self,
        numero_enc,
        numero_hash,
        identificador_tel_enc,
        identificador_tel_hash,
        tarjeta_id,
        proveedor_id,
        tipo_servicio,
        identificacion_cliente,
        activo
    ):
        return self.execute("""
            INSERT INTO telefonos
            (
                numero_enc,
                numero_hash,
                identificador_tel_enc,
                identificador_tel_hash,
                tarjeta_id,
                proveedor_id,
                tipo_servicio,
                identificacion_cliente,
                activo,
                estado_linea
            )
            VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)
        """, (
            numero_enc,
            numero_hash,
            identificador_tel_enc,
            identificador_tel_hash,
            tarjeta_id,
            proveedor_id,
            tipo_servicio,
            identificacion_cliente,
            activo,
            "ACTIVO" if activo else "INACTIVO"
        ))

    # ============================================================
    # IDENTIFICADOR6
    # Actualiza una línea existente.
    # Esto se usa cuando el proveedor manda activar o desactivar.
    # ============================================================
    def update_phone_identificador6(
        self,
        numero_hash,
        identificador_tel_enc,
        identificador_tel_hash,
        tarjeta_id,
        proveedor_id,
        tipo_servicio,
        identificacion_cliente,
        activo
    ):
        self.execute("""
            UPDATE telefonos
            SET identificador_tel_enc = %s,
                identificador_tel_hash = %s,
                tarjeta_id = %s,
                proveedor_id = %s,
                tipo_servicio = %s,
                identificacion_cliente = %s,
                activo = %s,
                estado_linea = %s
            WHERE numero_hash = %s OR identificador_tel_hash = %s
        """, (
            identificador_tel_enc,
            identificador_tel_hash,
            tarjeta_id,
            proveedor_id,
            tipo_servicio,
            identificacion_cliente if activo else None,
            activo,
            "ACTIVO" if activo else "INACTIVO",
            numero_hash,
            identificador_tel_hash
        ))

    def delete_inactive_phone_by_hash(self, numero_hash):
        """Elimina una linea inactiva y su tarjeta si no tienen historial."""
        cnx = self.connect()
        try:
            cur = cnx.cursor(dictionary=True)
            cnx.start_transaction()

            cur.execute("""
                SELECT id, tarjeta_id, activo
                FROM telefonos
                WHERE numero_hash = %s
                FOR UPDATE
            """, (numero_hash,))
            telefono = cur.fetchone()

            # Una linea que nunca fue activada no existe en el Identificador.
            if not telefono:
                cnx.commit()
                return True

            if telefono["activo"]:
                cnx.rollback()
                return False

            cur.execute(
                "SELECT COUNT(*) AS cantidad FROM llamadas WHERE telefono_id = %s",
                (telefono["id"],)
            )
            if cur.fetchone()["cantidad"] != 0:
                cnx.rollback()
                return False

            cur.execute("DELETE FROM telefonos WHERE id = %s", (telefono["id"],))
            cur.execute("""
                DELETE FROM tarjetas
                WHERE id = %s
                  AND NOT EXISTS (
                      SELECT 1 FROM telefonos WHERE tarjeta_id = %s
                  )
            """, (telefono["tarjeta_id"], telefono["tarjeta_id"]))
            cnx.commit()
            return True
        except Exception:
            cnx.rollback()
            raise
        finally:
            cnx.close()

    def fetchone(self, sql, params=()):
        cnx = self.connect()
        try:
            cur = cnx.cursor(dictionary=True)
            cur.execute(sql, params)
            return cur.fetchone()
        finally:
            cnx.close()

    def execute(self, sql, params=()):
        cnx = self.connect()
        try:
            cur = cnx.cursor()
            cur.execute(sql, params)
            cnx.commit()
            return cur.lastrowid
        except Exception:
            cnx.rollback()
            raise
        finally:
            cnx.close()

    def get_phone_by_hash(self, numero_hash):
        return self.fetchone("""
            SELECT t.id AS telefono_id, t.activo AS telefono_activo,
                   t.identificador_tel_hash, t.tipo_servicio,
                   p.id AS proveedor_id, p.activo AS proveedor_activo,
                   ta.identificador_chip_hash, ta.activo AS tarjeta_activa
            FROM telefonos t
            JOIN proveedores p ON p.id = t.proveedor_id
            JOIN tarjetas ta ON ta.id = t.tarjeta_id
            WHERE t.numero_hash = %s
            LIMIT 1
        """, (numero_hash,))

    def country_code_exists(self, codigo):
        row = self.fetchone(
            "SELECT codigo FROM codigos_pais WHERE codigo=%s AND activo=TRUE",
            (codigo,)
        )
        return row is not None

    def insert_call(self, telefono_id, destino_enc, destino_hash, tipo_llamada,
                    inicio, tarifa_minuto):
        return self.execute("""
            INSERT INTO llamadas
            (telefono_id, telefono_destino_enc, telefono_destino_hash,
             tipo_llamada, fecha_inicio, tarifa_minuto, estado)
            VALUES (%s,%s,%s,%s,%s,%s,'ACTIVA')
        """, (telefono_id, destino_enc, destino_hash, tipo_llamada,
              inicio, tarifa_minuto))

    def finish_call(self, call_id, fin, duracion, costo, estado, proveedor_resp):
        self.execute("""
            UPDATE llamadas
            SET fecha_fin=%s, duracion_segundos=%s, costo=%s,
                estado=%s, respuesta_proveedor=%s
            WHERE id=%s
        """, (fin, duracion, costo, estado, proveedor_resp, call_id))


# =========================
# BITACORA EN SEGUNDO PLANO
# =========================

class AsyncLogger:
    def __init__(self, path):
        self.path = path
        self.q = queue.Queue()
        self.thread = threading.Thread(target=self._worker, daemon=True)

    def start(self):
        os.makedirs(os.path.dirname(self.path), exist_ok=True)
        self.thread.start()

    def log(self, tipo, data):
        self.q.put({
            "fecha": datetime.now().strftime("%d/%m/%Y %H:%M:%S"),
            "tipo": tipo,
            "data": data
        })

    def _worker(self):
        while True:
            item = self.q.get()
            with open(self.path, "a", encoding="utf-8") as f:
                f.write(json.dumps(item, ensure_ascii=False, default=str) + "\n")
            self.q.task_done()


# =========================
# CLIENTE HACIA PROVEEDOR
# =========================

class ProviderClient:
    def __init__(self, cfg):
        self.cfg = cfg

    def _send(self, trama):
        with socket.create_connection(
            (self.cfg.proveedor_host, self.cfg.proveedor_port),
            timeout=10
        ) as s:
            s.sendall((trama + "\n").encode("utf-8"))
            return s.recv(1024).decode("utf-8").strip()

    def autorizar_llamada(self, telefono, tipo_llamada):
        # Proveedor1: tipo transaccion 1 + telefono + tipo llamada.
        resp = self._send(f"1{telefono}{tipo_llamada}")

        if resp.startswith("OK"):
            if "|" in resp:
                _, tarifa, tiempo = resp.split("|")[:3]
            else:
                body = resp[2:].strip()
                tarifa, tiempo = body[:10], body[10:16]
            return {"status": "OK", "tarifa": tarifa.zfill(10), "tiempo": tiempo.zfill(6)}

        if resp.startswith("INSUF"):
            return {"status": "INSUF"}

        return {"status": "ERROR"}

    def consultar_saldo(self, telefono):
        # Proveedor1: tipo transaccion 2 + telefono + tipo llamada por defecto.
        resp = self._send(f"2{telefono}1")

        if resp.startswith("OK"):
            saldo = resp.split("|")[1] if "|" in resp else resp[2:].strip()
            return {"status": "OK", "saldo": saldo.zfill(19) if saldo != "-1" else "-1"}

        return {"status": "ERROR"}

    def registrar_movimiento(self, telefono, inicio, destino, costo, duracion_hms):
        # Proveedor2: 1 + telefono + fecha + hora + destino + costo8 + duracion6.
        fecha = inicio.strftime("%Y%m%d")
        hora = inicio.strftime("%H%M%S")
        costo8 = money_to_fixed(costo, 8)
        destino_num = only_digits(destino)
        trama = f"1{telefono}{fecha}{hora}{destino_num}{costo8}{duracion_hms}"
        return self._send(trama)


# =========================
# LLAMADAS ACTIVAS
# =========================

@dataclass(order=True)
class ActiveCall:
    expires_at: datetime
    key: str = field(compare=False)
    db_call_id: int = field(compare=False)
    telefono: str = field(compare=False)
    destino: str = field(compare=False)
    started_at: datetime = field(compare=False)
    tarifa_minuto: Decimal = field(compare=False)
    client_socket: socket.socket = field(compare=False, default=None)


class ActiveCalls:
    def __init__(self):
        self.lock = threading.Lock()
        self.heap = []
        self.calls = {}

    def add(self, call):
        with self.lock:
            self.calls[call.key] = call
            heapq.heappush(self.heap, call)

    def remove(self, key):
        with self.lock:
            return self.calls.pop(key, None)

    def expired(self):
        now = datetime.now()
        result = []
        with self.lock:
            while self.heap and self.heap[0].expires_at <= now:
                call = heapq.heappop(self.heap)
                active = self.calls.pop(call.key, None)
                if active:
                    result.append(active)
        return result


# =========================
# UTILIDADES
# =========================

ERROR = {
    "DESTINO_INVALIDO": 1,
    "TARJETA_NO_COINCIDE": 2,
    "FUERA_PAIS": 3,
    "ACCION_INVALIDA": 4,
    "CODIGO_PAIS_INVALIDO": 5,
    "ERROR_NO_CONTROLADO": 5
}

def only_digits(value):
    return "".join(ch for ch in str(value) if ch.isdigit())

def normalize_cr_phone(value):
    digits = only_digits(value)
    if len(digits) == 8:
        return digits
    if len(digits) == 9 and digits.endswith("1"):
        return digits[:8]
    if len(digits) == 11 and digits.startswith("506"):
        return digits[-8:]
    return None

def extract_country_code(value, db):
    digits = only_digits(value)
    if digits.startswith("00"):
        digits = digits[2:]
    if digits.startswith("506"):
        return None
    for size in range(1, 5):
        code = digits[:size]
        if db.country_code_exists(code):
            return code
    return None

def parse_coords(value):
    # Se recomienda enviar coordenadas decimales: {"lat":9.93,"lon":-84.08}
    if isinstance(value, dict):
        return float(value["lat"]), float(value["lon"])
    lat, lon = str(value).split(",")
    return float(lat), float(lon)

def inside_costa_rica(coords):
    lat, lon = parse_coords(coords)
    return 8.0 <= lat <= 11.3 and -86.2 <= lon <= -82.5

def hms_to_seconds(hms):
    hms = str(hms).zfill(6)
    return int(hms[:2]) * 3600 + int(hms[2:4]) * 60 + int(hms[4:6])

def seconds_to_hms(seconds):
    seconds = max(0, int(seconds))
    h = seconds // 3600
    m = (seconds % 3600) // 60
    s = seconds % 60
    return f"{h:02d}{m:02d}{s:02d}"

def money_to_fixed(amount, width):
    cents = int((Decimal(amount) * 100).quantize(Decimal("1"), rounding=ROUND_HALF_UP))
    return str(cents).zfill(width)

def rate_from_provider(value):
    value = str(value).zfill(10)
    if value == "9999999999":
        return Decimal("0.00")
    return Decimal(int(value)) / Decimal("100")

def call_key(telefono, destino):
    return hashlib.sha256(f"{telefono}|{destino}".encode("utf-8")).hexdigest()

def send_json(conn, data):
    conn.sendall((json.dumps(data, ensure_ascii=False) + "\n").encode("utf-8"))

def recv_text(conn):
    data = b""
    while not data.endswith(b"\n"):
        part = conn.recv(4096)
        if not part:
            break
        data += part
    return data.decode("utf-8").strip()


# =========================
# SOCKET IDENTIFICADOR
# =========================

class IdentificadorServer:
    def __init__(self, cfg):
        self.cfg = cfg
        self.crypto = CryptoBox(cfg.aes_key_b64)
        self.db = Database(cfg)
        self.provider = ProviderClient(cfg)
        self.logger = AsyncLogger(cfg.log_path)
        self.active = ActiveCalls()
        self.auth = {}
        self.auth_lock = threading.Lock()

    def start(self):
        self.logger.start()
        threading.Thread(target=self.monitor_expired_calls, daemon=True).start()

        server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)

        try:
            server.bind((self.cfg.host, self.cfg.port))
        except OSError as exc:
            if self.cfg.port != 0 and "address already in use" in str(exc).lower() or exc.errno in (10048, 10013):
                print(f"Puerto {self.cfg.port} ocupado; intentando usar un puerto disponible...")
                server.bind((self.cfg.host, 0))
            else:
                raise

        server.listen(50)
        actual_port = server.getsockname()[1]
        self.cfg.port = actual_port

        print(f"Identificador escuchando en {self.cfg.host}:{self.cfg.port}")

        while True:
            conn, addr = server.accept()
            threading.Thread(target=self.handle_client, args=(conn, addr), daemon=True).start()

   

    def decrypt_payload(self, req):
        data = dict(req)
        for field in ("telefono", "identificador_tel", "identificador_chip"):
            data[field] = self.crypto.decrypt(data[field])
        return data

    # ============================================================
    # IDENTIFICADOR6
    # Recibe texto plano desde el Proveedor para activar/desactivar
    # una línea dentro del Identificador.
    #
    # Formato esperado:
    # IDENTIFICADOR6|telefonoEnc|identificadorTelEnc|identificadorChipEnc|tipo|cliente|estado|proveedor
    #
    # Respuestas permitidas por el alcance:
    # OK
    # Activación fallida
    # ============================================================
    def tx_identificador6_texto_plano(self, raw):
        try:
            partes = raw.strip().split("|")

            # Deben venir exactamente 8 datos separados por pipe.
            if len(partes) != 8:
                return "Activación fallida"

            operacion = partes[0].strip()
            telefono_enc = partes[1].strip()
            identificador_tel_enc = partes[2].strip()
            identificador_chip_enc = partes[3].strip()
            tipo_servicio = partes[4].strip().upper()
            identificacion_cliente = partes[5].strip()
            estado = partes[6].strip().lower()
            codigo_proveedor = partes[7].strip().upper()

            # Validaciones básicas de la trama.
            if operacion != "IDENTIFICADOR6":
                return "Activación fallida"

            if not telefono_enc or not identificador_tel_enc or not identificador_chip_enc:
                return "Activación fallida"

            if not identificacion_cliente or not codigo_proveedor:
                return "Activación fallida"

            if tipo_servicio not in ("PREPAGO", "POSTPAGO"):
                return "Activación fallida"

            if estado not in ("activo", "desactivo", "desactivado", "inactivo"):
                return "Activación fallida"

            # Descifrado AES de datos sensibles.
            try:
                telefono = self.crypto.decrypt(telefono_enc)
                identificador_tel = self.crypto.decrypt(identificador_tel_enc)
                identificador_chip = self.crypto.decrypt(identificador_chip_enc)
            except Exception:
                return "Activación fallida"

            # Validaciones del alcance.
            if not telefono.isdigit() or len(telefono) != 8:
                return "Activación fallida"

            if not identificador_tel.isdigit() or len(identificador_tel) != 16:
                return "Activación fallida"

            if not identificador_chip.isdigit() or len(identificador_chip) != 19:
                return "Activación fallida"

            # Buscar proveedor en MySQL.
            proveedor = self.db.get_provider_by_code(codigo_proveedor)
            if not proveedor:
                return "Activación fallida"

            if not proveedor.get("activo"):
                return "Activación fallida"

            # Hashes para búsquedas sin guardar texto sensible plano.
            numero_hash = self.crypto.digest(telefono)
            identificador_tel_hash = self.crypto.digest(identificador_tel)
            identificador_chip_hash = self.crypto.digest(identificador_chip)

            # Crear tarjeta si no existe.
            tarjeta_id = self.db.get_or_create_card(
                identificador_chip_enc,
                identificador_chip_hash
            )

            # activo = True si viene activo; False si viene inactivo/desactivado.
            activar = estado == "activo"

            telefono_existente = self.db.get_phone_simple_by_hash(numero_hash)
            if not telefono_existente:
                telefono_existente = self.db.get_phone_by_identificador_tel_hash(identificador_tel_hash)

            if telefono_existente:
                self.db.update_phone_identificador6(
                    numero_hash=numero_hash,
                    identificador_tel_enc=identificador_tel_enc,
                    identificador_tel_hash=identificador_tel_hash,
                    tarjeta_id=tarjeta_id,
                    proveedor_id=proveedor["id"],
                    tipo_servicio=tipo_servicio,
                    identificacion_cliente=identificacion_cliente,
                    activo=activar
                )
            else:
                self.db.insert_phone_identificador6(
                    numero_enc=telefono_enc,
                    numero_hash=numero_hash,
                    identificador_tel_enc=identificador_tel_enc,
                    identificador_tel_hash=identificador_tel_hash,
                    tarjeta_id=tarjeta_id,
                    proveedor_id=proveedor["id"],
                    tipo_servicio=tipo_servicio,
                    identificacion_cliente=identificacion_cliente,
                    activo=activar
                )

            return "OK"

        except Exception as ex:
            self.logger.log("error_identificador6", str(ex))
            return "Activación fallida"

    def tx_identificador6_eliminar(self, raw):
        """Elimina del Identificador una linea inactiva sin historial."""
        try:
            partes = raw.strip().split("|")
            if len(partes) != 2 or partes[0].strip() != "IDENTIFICADOR6_ELIMINAR":
                return "ERROR"

            telefono_enc = partes[1].strip()
            if not telefono_enc:
                return "ERROR"

            try:
                telefono = self.crypto.decrypt(telefono_enc)
            except Exception:
                return "ERROR"

            if not telefono.isdigit() or len(telefono) != 8:
                return "ERROR"

            eliminado = self.db.delete_inactive_phone_by_hash(
                self.crypto.digest(telefono)
            )
            return "OK" if eliminado else "ERROR"
        except Exception as ex:
            self.logger.log("error_identificador6_eliminar", str(ex))
            return "ERROR"

    # ============================================================
    # WS_IDENTIFICADOR1
    # Consulta de saldo desde el Web Service C#.
    # WS_SALDO|telefonoEnc|WEB|saldo
    # ============================================================
    def tx_ws_saldo_texto_plano(self, raw):
        try:
            partes = raw.strip().split("|")

            if len(partes) != 4:
                return "ERROR"

            operacion = partes[0].strip()
            telefono_enc = partes[1].strip()
            origen = partes[2].strip().upper()
            transaccion = partes[3].strip().lower()

            if operacion != "WS_SALDO":
                return "ERROR"

            if not telefono_enc:
                return "ERROR"

            if origen != "WEB":
                return "ERROR"

            if transaccion != "saldo":
                return "ERROR"

            try:
                telefono = self.crypto.decrypt(telefono_enc)
            except Exception:
                return "ERROR"

            if len(only_digits(telefono)) != 8:
                return "ERROR"

            proveedor_resp = self.provider.consultar_saldo(telefono)

            if proveedor_resp["status"] == "OK":
                return "OK|" + proveedor_resp["saldo"]

            return "ERROR"

        except Exception as ex:
            self.logger.log("error_ws_saldo", str(ex))
            return "ERROR"

    def handle_client(self, conn, addr):
        keep_open = False

        try:
            raw = recv_text(conn)

            if raw.startswith("IDENTIFICADOR6_ELIMINAR|"):
                self.logger.log("entrada_identificador6_eliminar", raw)
                respuesta = self.tx_identificador6_eliminar(raw)
                self.logger.log("salida_identificador6_eliminar", respuesta)
                conn.sendall((respuesta + "\n").encode("utf-8"))
                return

            # ============================================================
            # IDENTIFICADOR6
            # ============================================================
            if raw.startswith("IDENTIFICADOR6|"):
                self.logger.log("entrada_identificador6", raw)

                respuesta = self.tx_identificador6_texto_plano(raw)

                self.logger.log("salida_identificador6", respuesta)
                conn.sendall((respuesta + "\n").encode("utf-8"))
                return

            # ============================================================
            # WS_IDENTIFICADOR1
            # ============================================================
            if raw.startswith("WS_SALDO|"):
                self.logger.log("entrada_ws_saldo", raw)

                respuesta = self.tx_ws_saldo_texto_plano(raw)

                self.logger.log("salida_ws_saldo", respuesta)

                conn.sendall((respuesta + "\n").encode("utf-8"))
                return

            # ============================================================
            # Flujo normal del simulador: JSON.
            # ============================================================
            req = json.loads(raw)
            self.logger.log("entrada", req)

            tx = req.get("transaccion") or req.get("tipo_transaccion")

            if tx == "solicitud":
                resp = self.tx_solicitud(req)
            elif tx == "saldo":
                resp = self.tx_saldo(req)
            elif tx == "llamada":
                resp, keep_open = self.tx_llamada(req, conn)
            elif tx == "finalizacion":
                resp = self.tx_finalizacion(req)
            else:
                resp = self.fail("ACCION_INVALIDA", "Tipo de transacción inválido.")

            self.logger.log("salida", resp)
            send_json(conn, resp)

        except Exception as ex:
            resp = self.fail("ERROR_NO_CONTROLADO", str(ex))
            self.logger.log("salida_error", resp)

            try:
                send_json(conn, resp)
            except Exception:
                pass

        finally:
            if not keep_open:
                conn.close()

    def validate_common(self, data, expected_tx, require_destino):
        required = ["telefono", "identificador_tel", "identificador_chip",
                    "coordenadas", "transaccion"]
        if require_destino:
            required.append("telefono_destino")

        for field in required:
            if not data.get(field):
                return self.fail("ERROR_NO_CONTROLADO", f"Falta {field}"), None

        if data["transaccion"] != expected_tx:
            return self.fail("ACCION_INVALIDA", "Acción inválida."), None

        if not inside_costa_rica(data["coordenadas"]):
            return self.fail("FUERA_PAIS", "Coordenadas fuera de Costa Rica."), None

        if len(only_digits(data["identificador_tel"])) != 16:
            return self.fail("TARJETA_NO_COINCIDE", "Identificador de teléfono inválido."), None

        if len(only_digits(data["identificador_chip"])) != 19:
            return self.fail("TARJETA_NO_COINCIDE", "Identificador de chip inválido."), None

        phone = self.db.get_phone_by_hash(self.crypto.digest(data["telefono"]))
        if not phone or not phone["telefono_activo"] or not phone["proveedor_activo"]:
            return self.fail("DESTINO_INVALIDO", "Teléfono origen no existe o está inactivo."), None

        if phone["identificador_tel_hash"] != self.crypto.digest(data["identificador_tel"]):
            return self.fail("TARJETA_NO_COINCIDE", "Identificador del teléfono no coincide."), None

        if phone["identificador_chip_hash"] != self.crypto.digest(data["identificador_chip"]):
            return self.fail("TARJETA_NO_COINCIDE", "Identificador del chip no coincide."), None

        ctx = {"phone": phone}

        if require_destino:
            dest_info, error = self.classify_destination(data["telefono_destino"], phone)
            if error:
                return error, None
            ctx.update(dest_info)

        return None, ctx

    def classify_destination(self, destino, source_phone):
        nacional = normalize_cr_phone(destino)

        if nacional:
            dest_phone = self.db.get_phone_by_hash(self.crypto.digest(nacional))
            if not dest_phone or not dest_phone["telefono_activo"]:
                return None, self.fail("DESTINO_INVALIDO", "Destino nacional inválido.")

            same_provider = dest_phone["proveedor_id"] == source_phone["proveedor_id"]
            return {
                "destino_normalizado": nacional,
                "tipo_llamada": "1" if same_provider else "2",
                "tipo_llamada_db": "MISMO_PROVEEDOR" if same_provider else "OTRO_PROVEEDOR"
            }, None

        code = extract_country_code(destino, self.db)
        if not code:
            return None, self.fail("CODIGO_PAIS_INVALIDO", "Código de país inválido.")

        return {
            "destino_normalizado": destino,
            "tipo_llamada": "3",
            "tipo_llamada_db": "INTERNACIONAL"
        }, None

    def tx_solicitud(self, req):
        data = self.decrypt_payload(req)
        error, ctx = self.validate_common(data, "solicitud", True)
        if error:
            return error

        prov = self.provider.autorizar_llamada(data["telefono"], ctx["tipo_llamada"])
        if prov["status"] == "OK":
            key = call_key(data["telefono"], ctx["destino_normalizado"])
            with self.auth_lock:
                self.auth[key] = {
                    "tarifa": prov["tarifa"],
                    "tiempo": prov["tiempo"],
                    "tipo_llamada_db": ctx["tipo_llamada_db"]
                }
            return {
                "status": "OK",
                "tiempo": prov["tiempo"],
                "monto_autorizado": prov["tarifa"],
                "tarifa": prov["tarifa"],
                "tipo_llamada": ctx["tipo_llamada_db"]
            }

        if prov["status"] == "INSUF":
            return {"status": "FALLIDO", "motivo": "INSUF"}

        return self.fail("ERROR_NO_CONTROLADO", "Proveedor respondió error.")

    def tx_saldo(self, req):
        data = self.decrypt_payload(req)
        error, _ = self.validate_common(data, "saldo", False)
        if error:
            return error

        prov = self.provider.consultar_saldo(data["telefono"])
        if prov["status"] == "OK":
            return {"status": "OK", "saldo": prov["saldo"]}

        return self.fail("ERROR_NO_CONTROLADO", "No se pudo consultar saldo.")

    def tx_llamada(self, req, conn):
        data = self.decrypt_payload(req)

        for field in ("telefono_destino", "tiempo_maximo"):
            if not data.get(field):
                return self.fail("ERROR_NO_CONTROLADO", f"Falta {field}"), False

        if data.get("transaccion") != "llamada":
            return self.fail("ACCION_INVALIDA", "Acción inválida."), False

        phone = self.db.get_phone_by_hash(self.crypto.digest(data["telefono"]))
        if not phone:
            return self.fail("DESTINO_INVALIDO", "Teléfono origen inválido."), False

        destino = normalize_cr_phone(data["telefono_destino"]) or data["telefono_destino"]
        key = call_key(data["telefono"], destino)

        with self.auth_lock:
            auth = self.auth.pop(key, None)

        if not auth:
            dest_info, error = self.classify_destination(data["telefono_destino"], phone)
            if not error and dest_info:
                prov = self.provider.autorizar_llamada(data["telefono"], dest_info["tipo_llamada"])
                if prov["status"] == "OK":
                    auth = {
                        "tarifa": prov["tarifa"],
                        "tiempo": prov["tiempo"],
                        "tipo_llamada_db": dest_info["tipo_llamada_db"]
                    }

        tarifa = rate_from_provider(auth["tarifa"]) if auth else Decimal("0.00")
        tipo_db = auth["tipo_llamada_db"] if auth else "MISMO_PROVEEDOR"

        inicio = datetime.now()
        max_seconds = hms_to_seconds(data["tiempo_maximo"])
        expires_at = inicio + timedelta(seconds=max_seconds)

        db_call_id = self.db.insert_call(
            phone["telefono_id"],
            self.crypto.encrypt(destino),
            self.crypto.digest(destino),
            tipo_db,
            inicio,
            tarifa
        )

        self.active.add(ActiveCall(
            expires_at=expires_at,
            key=key,
            db_call_id=db_call_id,
            telefono=data["telefono"],
            destino=destino,
            started_at=inicio,
            tarifa_minuto=tarifa,
            client_socket=conn
        ))

        return {"status": "OK", "llamada_id": db_call_id}, True

    def tx_finalizacion(self, req):
        data = self.decrypt_payload(req)

        if data.get("transaccion") != "finalizacion":
            return self.fail("ACCION_INVALIDA", "Acción inválida.")

        destino = normalize_cr_phone(data.get("telefono_destino")) or data.get("telefono_destino")
        key = call_key(data["telefono"], destino)
        call = self.active.remove(key)

        if not call:
            return {
                "status": "FALLIDO",
                "estado": "fallido",
                "detalle": "Llamada activa no encontrada."
            }

        cierre = self.close_call(call, "cliente")
        return {
            "status": "OK",
            "estado": "ok",
            "duracion_real": cierre["duracion_hms"],
            "duracion_segundos": cierre["duracion_segundos"],
            "costo": money_to_fixed(cierre["costo"], 8),
            "respuesta_proveedor": cierre["proveedor_resp"]
        }

    def monitor_expired_calls(self):
        while True:
            for call in self.active.expired():
                self.close_call(call, "saldo agotado")
                try:
                    send_json(call.client_socket, {
                        "estado": "finalizada",
                        "razon": "saldo agotado"
                    })
                except Exception:
                    pass
                finally:
                    try:
                        call.client_socket.close()
                    except Exception:
                        pass

            time.sleep(self.cfg.check_interval)

    def close_call(self, call, razon):
        fin = datetime.now()
        duracion = max(1, int((fin - call.started_at).total_seconds()))
        duracion_hms = seconds_to_hms(duracion)
        minutos = Decimal(math.ceil(duracion / 60))
        costo = minutos * call.tarifa_minuto

        try:
            proveedor_resp = self.provider.registrar_movimiento(
                call.telefono,
                call.started_at,
                call.destino,
                costo,
                duracion_hms
            )
            estado = "FINALIZADA" if proveedor_resp.startswith("OK") else "FALLIDA"
        except Exception:
            proveedor_resp = "ERROR"
            estado = "FALLIDA"

        self.db.finish_call(call.db_call_id, fin, duracion, costo, estado, proveedor_resp)

        if razon != "saldo agotado":
            try:
                call.client_socket.close()
            except Exception:
                pass

        return {
            "duracion_hms": duracion_hms,
            "duracion_segundos": duracion,
            "costo": costo,
            "estado": estado,
            "proveedor_resp": proveedor_resp
        }

    def fail(self, key, detail):
        return {
            "status": "FALLIDO",
            "motivo": ERROR[key],
            "detalle": detail
        }


if __name__ == "__main__":
    ConfiguredServer = IdentificadorServer(Config())
    ConfiguredServer.start()
