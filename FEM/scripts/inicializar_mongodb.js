// Inicializacion reproducible para WS_AUTENTICACION2.
// Ejecutar con: mongosh "mongodb://localhost:27017" --file .\FEM\scripts\inicializar_mongodb.js

const nombreBase = "central_general_auth";
const nombreColeccion = "usuarios";
const autenticacionDb = db.getSiblingDB(nombreBase);

autenticacionDb.runCommand({ ping: 1 });

if (!autenticacionDb.getCollectionNames().includes(nombreColeccion)) {
    autenticacionDb.createCollection(nombreColeccion);
    print(`Coleccion creada: ${nombreBase}.${nombreColeccion}`);
}

const usuarios = autenticacionDb.getCollection(nombreColeccion);
usuarios.createIndex({ Identificacion: 1 }, { unique: true });
usuarios.createIndex({ UsuarioHash: 1 }, { unique: true });

print(`MongoDB preparado: ${nombreBase}.${nombreColeccion}`);
printjson(usuarios.getIndexes());
