import mysql.connector

conexion = mysql.connector.connect(
    host="localhost",
    port=3306,
    user="identificador_user",
    password="Identificador123",
    database="identificador_db"
)

cursor = conexion.cursor(dictionary=True)

tablas = [
    "proveedores",
    "tarjetas",
    "telefonos",
    "codigos_pais",
    "llamadas"
]

for tabla in tablas:
    print("\n==============================")
    print(f"TABLA: {tabla}")
    print("==============================")

    cursor.execute(f"SELECT COUNT(*) AS total FROM {tabla}")
    total = cursor.fetchone()["total"]
    print(f"Total de registros: {total}")

    cursor.execute(f"SELECT * FROM {tabla}")
    filas = cursor.fetchall()

    for fila in filas:
        print(fila)

cursor.close()
conexion.close()