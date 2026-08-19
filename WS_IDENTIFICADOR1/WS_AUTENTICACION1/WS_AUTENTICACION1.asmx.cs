using System;
using System.Security.Cryptography;
using System.Text;
using System.Web.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WS_AUTENTICACION1
{
    // ============================================================
    // WS_AUTENTICACION1
    // Servicio Web SOAP para autenticar usuarios.
    //
    // Recibe:
    // - usuario encriptado
    // - contraseña encriptada
    // - tipo usuario: administrador / cliente / 1 / 2
    //
    // Valida en MongoDB:
    // - usuario existe
    // - contraseña coincide
    // - estado activo
    // - tipo correcto
    //
    // Responde:
    // Resultado = true / false
    // Mensaje = Exitoso / Usuario y/o contraseña incorrectos.
    // ============================================================

    [WebService(Namespace = "http://centralgeneral/autenticacion")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class WSAutenticacion : WebService
    {
        // ========================================================
        // CONFIGURACIÓN DE MONGODB
        // ========================================================
        private const string MONGO_URI = "mongodb://localhost:27017";
        private const string DATABASE_NAME = "central_general_auth";
        private const string COLLECTION_NAME = "usuarios";

        // ========================================================
        // LLAVE AES PARA CIFRAR Y DESCIFRAR
        // Debe tener 32 caracteres para AES-256.
        // Esta misma llave se usa para usuario y contraseña.
        // ========================================================
        private const string AES_KEY = "12345678901234567890123456789012";

        // ========================================================
        // MÉTODO PRINCIPAL DE LA HISTORIA WS_AUTENTICACION1
        // ========================================================
        [WebMethod]
        public RespuestaAutenticacion Autenticar(
            string usuarioEncriptado,
            string contrasenaEncriptada,
            string tipoUsuario
        )
        {
            try
            {
                // Validación básica de datos obligatorios.
                if (string.IsNullOrWhiteSpace(usuarioEncriptado) ||
                    string.IsNullOrWhiteSpace(contrasenaEncriptada) ||
                    string.IsNullOrWhiteSpace(tipoUsuario))
                {
                    return new RespuestaAutenticacion
                    {
                        Resultado = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                // Convertimos administrador/cliente a 1/2.
                int tipo = ConvertirTipoUsuario(tipoUsuario);

                if (tipo == 0)
                {
                    return new RespuestaAutenticacion
                    {
                        Resultado = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                // Desciframos usuario y contraseña recibidos.
                string usuario = DescifrarAES(usuarioEncriptado);
                string contrasena = DescifrarAES(contrasenaEncriptada);

                if (string.IsNullOrWhiteSpace(usuario) ||
                    string.IsNullOrWhiteSpace(contrasena))
                {
                    return new RespuestaAutenticacion
                    {
                        Resultado = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                // Creamos hash para buscar en MongoDB sin depender del cifrado aleatorio.
                string usuarioHash = HashSHA256(usuario);
                string contrasenaHash = HashSHA256(contrasena);

                IMongoCollection<UsuarioMongo> coleccion = ObtenerColeccion();

                // Buscamos usuario activo, tipo correcto y contraseña correcta.
                var filtro =
                    Builders<UsuarioMongo>.Filter.Eq(x => x.UsuarioHash, usuarioHash) &
                    Builders<UsuarioMongo>.Filter.Eq(x => x.ContrasenaHash, contrasenaHash) &
                    Builders<UsuarioMongo>.Filter.Eq(x => x.Tipo, tipo) &
                    Builders<UsuarioMongo>.Filter.Eq(x => x.Estado, "activo");

                UsuarioMongo encontrado = coleccion.Find(filtro).FirstOrDefault();

                if (encontrado == null)
                {
                    return new RespuestaAutenticacion
                    {
                        Resultado = false,
                        Mensaje = "Usuario y/o contraseña incorrectos."
                    };
                }

                return new RespuestaAutenticacion
                {
                    Resultado = true,
                    Mensaje = "Exitoso",
                    Identificacion = encontrado.Identificacion,
                    Nombre = (encontrado.Nombre + " "
                        + encontrado.PrimerApellido).Trim(),
                    Tipo = encontrado.Tipo
                };
            }
            catch
            {
                return new RespuestaAutenticacion
                {
                    Resultado = false,
                    Mensaje = "Usuario y/o contraseña incorrectos."
                };
            }
        }

        // ========================================================
        // MÉTODO DE PRUEBA
        // Sirve para crear usuarios iniciales en MongoDB.
        // Primero ejecutás este método desde el navegador.
        // ========================================================
        [WebMethod]
        public RespuestaAutenticacion CrearUsuariosPrueba()
        {
            try
            {
                IMongoCollection<UsuarioMongo> coleccion = ObtenerColeccion();

                CrearUsuarioSiNoExiste(
                    coleccion,
                    identificacion: "101010101",
                    nombre: "Administrador",
                    primerApellido: "Sistema",
                    segundoApellido: "Central",
                    correo: "admin@centralgeneral.com",
                    usuario: "admin",
                    contrasena: "Admin12345678!",
                    estado: "activo",
                    tipo: 1
                );

                CrearUsuarioSiNoExiste(
                    coleccion,
                    identificacion: "202020202",
                    nombre: "Cliente",
                    primerApellido: "Prueba",
                    segundoApellido: "Central",
                    correo: "cliente@centralgeneral.com",
                    usuario: "cliente",
                    contrasena: "Cliente123456!",
                    estado: "activo",
                    tipo: 2
                );

                return new RespuestaAutenticacion
                {
                    Resultado = true,
                    Mensaje = "Usuarios de prueba creados correctamente."
                };
            }
            catch (Exception ex)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = false,
                    Mensaje = "Error creando usuarios: " + ex.Message
                };
            }
        }

        // ========================================================
        // MÉTODO DE PRUEBA
        // Sirve para cifrar usuario y contraseña antes de llamar
        // al método Autenticar.
        // ========================================================
        [WebMethod]
        public string EncriptarTexto(string texto)
        {
            return CifrarAES(texto);
        }

        // ========================================================
        // Conexión a MongoDB.
        // Si la base no existe, MongoDB la crea cuando se inserta.
        // ========================================================
        private IMongoCollection<UsuarioMongo> ObtenerColeccion()
        {
            MongoClient cliente = new MongoClient(MONGO_URI);
            IMongoDatabase db = cliente.GetDatabase(DATABASE_NAME);
            return db.GetCollection<UsuarioMongo>(COLLECTION_NAME);
        }

        // ========================================================
        // Crea un usuario solo si no existe previamente.
        // Guardamos usuario y contraseña encriptados.
        // También guardamos hash para buscar y comparar.
        // ========================================================
        private void CrearUsuarioSiNoExiste(
            IMongoCollection<UsuarioMongo> coleccion,
            string identificacion,
            string nombre,
            string primerApellido,
            string segundoApellido,
            string correo,
            string usuario,
            string contrasena,
            string estado,
            int tipo
        )
        {
            string usuarioHash = HashSHA256(usuario);

            var filtro = Builders<UsuarioMongo>.Filter.Eq(x => x.UsuarioHash, usuarioHash);
            UsuarioMongo existente = coleccion.Find(filtro).FirstOrDefault();

            if (existente != null)
            {
                return;
            }

            UsuarioMongo nuevo = new UsuarioMongo
            {
                Identificacion = identificacion,
                Nombre = nombre,
                PrimerApellido = primerApellido,
                SegundoApellido = segundoApellido,
                CorreoElectronico = correo,

                UsuarioEncriptado = CifrarAES(usuario),
                ContrasenaEncriptada = CifrarAES(contrasena),

                UsuarioHash = HashSHA256(usuario),
                ContrasenaHash = HashSHA256(contrasena),

                Estado = estado,
                Tipo = tipo
            };

            coleccion.InsertOne(nuevo);
        }

        // ========================================================
        // Convierte el tipo recibido.
        //
        // administrador = 1
        // cliente       = 2
        // 1             = 1
        // 2             = 2
        // ========================================================
        private int ConvertirTipoUsuario(string tipoUsuario)
        {
            string valor = tipoUsuario.Trim().ToLower();

            if (valor == "1" || valor == "administrador" || valor == "empleado")
            {
                return 1;
            }

            if (valor == "2" || valor == "cliente")
            {
                return 2;
            }

            return 0;
        }

        // ========================================================
        // AES CBC con IV aleatorio.
        // El resultado es Base64(IV + textoCifrado).
        // ========================================================
        private string CifrarAES(string textoPlano)
        {
            byte[] key = Encoding.UTF8.GetBytes(AES_KEY);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] textoBytes = Encoding.UTF8.GetBytes(textoPlano);

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cifrado = encryptor.TransformFinalBlock(
                        textoBytes,
                        0,
                        textoBytes.Length
                    );

                    byte[] resultado = new byte[aes.IV.Length + cifrado.Length];

                    Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
                    Buffer.BlockCopy(cifrado, 0, resultado, aes.IV.Length, cifrado.Length);

                    return Convert.ToBase64String(resultado);
                }
            }
        }

        // ========================================================
        // Descifra Base64(IV + textoCifrado).
        // ========================================================
        private string DescifrarAES(string textoEncriptado)
        {
            byte[] data = Convert.FromBase64String(textoEncriptado);
            byte[] key = Encoding.UTF8.GetBytes(AES_KEY);

            byte[] iv = new byte[16];
            byte[] cifrado = new byte[data.Length - 16];

            Buffer.BlockCopy(data, 0, iv, 0, 16);
            Buffer.BlockCopy(data, 16, cifrado, 0, cifrado.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                {
                    byte[] plano = decryptor.TransformFinalBlock(
                        cifrado,
                        0,
                        cifrado.Length
                    );

                    return Encoding.UTF8.GetString(plano);
                }
            }
        }

        // ========================================================
        // Hash SHA256 para búsqueda y comparación.
        // No reemplaza el cifrado; es apoyo para consultar.
        // ========================================================
        private string HashSHA256(string texto)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(texto.Trim());
                byte[] hash = sha.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }

    // ============================================================
    // Estructura que devuelve el SOAP.
    // ============================================================
    public class RespuestaAutenticacion
    {
        public bool Resultado { get; set; }
        public string Mensaje { get; set; }
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public int Tipo { get; set; }
    }

    // ============================================================
    // Modelo de usuario almacenado en MongoDB.
    // ============================================================
    public class UsuarioMongo
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Identificacion { get; set; }

        public string Nombre { get; set; }

        public string PrimerApellido { get; set; }

        public string SegundoApellido { get; set; }

        public string CorreoElectronico { get; set; }

        public string UsuarioEncriptado { get; set; }

        public string ContrasenaEncriptada { get; set; }

        public string UsuarioHash { get; set; }

        public string ContrasenaHash { get; set; }

        public string Estado { get; set; }

        public int Tipo { get; set; }
    }
}
