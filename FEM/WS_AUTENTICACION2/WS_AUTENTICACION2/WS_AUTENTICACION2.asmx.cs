using System;
using System.Configuration;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WS_AUTENTICACION2
{
    [WebService(Namespace = "http://centralgeneral/autenticacion2")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class WSAutenticacion2 : WebService
    {
        private const string MONGO_URI_PREDETERMINADA = "mongodb://localhost:27017";
        private const string DATABASE_NAME_PREDETERMINADO = "central_general_auth";
        private const string COLLECTION_NAME_PREDETERMINADA = "usuarios";
        private const string AES_KEY_PREDETERMINADA = "12345678901234567890123456789012";

        private static readonly object BLOQUEO_INDICES = new object();
        private static bool indicesCreados;

        private const string MSG_OK = "Exitoso";
        private const string MSG_CREAR_ERROR = "Usuario ya existe o datos incorrectos o incompletos.";
        private const string MSG_MODIFICAR_ERROR = "Usuario no existe o datos incorrectos o incompletos.";
        private const string MSG_ESTADO_ERROR = "Usuario no existe o datos incorrectos.";

        [WebMethod]
        public RespuestaAutenticacion CrearUsuario(
            string identificacion,
            string nombre,
            string primerApellido,
            string segundoApellido,
            string correoElectronico,
            string usuarioEncriptado,
            string contrasenaEncriptada,
            string estado,
            int tipo)
        {
            try
            {
                string usuario = DescifrarAES(usuarioEncriptado);
                string contrasena = DescifrarAES(contrasenaEncriptada);

                usuario = usuario.Trim();

                if (!DatosUsuarioValidos(identificacion, nombre, primerApellido, segundoApellido,
                    correoElectronico, usuario, contrasena, tipo) || !EstadoNuevoValido(estado))
                {
                    return Error(MSG_CREAR_ERROR);
                }

                IMongoCollection<UsuarioMongo> coleccion = ObtenerColeccion();
                string usuarioHash = HashSHA256(usuario);
                string identificacionNormalizada = Normalizar(identificacion);

                bool existe = coleccion.Find(
                    Builders<UsuarioMongo>.Filter.Eq(x => x.Identificacion, identificacionNormalizada) |
                    Builders<UsuarioMongo>.Filter.Eq(x => x.UsuarioHash, usuarioHash)
                ).Any();

                if (existe)
                {
                    return Error(MSG_CREAR_ERROR);
                }

                UsuarioMongo nuevo = new UsuarioMongo
                {
                    Identificacion = identificacionNormalizada,
                    Nombre = nombre.Trim(),
                    PrimerApellido = primerApellido.Trim(),
                    SegundoApellido = segundoApellido.Trim(),
                    CorreoElectronico = correoElectronico.Trim(),
                    UsuarioEncriptado = CifrarAES(usuario),
                    ContrasenaEncriptada = CifrarAES(contrasena),
                    UsuarioHash = usuarioHash,
                    ContrasenaHash = HashSHA256(contrasena),
                    Estado = "activo",
                    Tipo = tipo
                };

                coleccion.InsertOne(nuevo);
                return Ok();
            }
            catch
            {
                return Error(MSG_CREAR_ERROR);
            }
        }

        [WebMethod]
        public RespuestaAutenticacion ModificarUsuario(
            string identificacion,
            string nombre,
            string primerApellido,
            string segundoApellido,
            string correoElectronico,
            string usuarioEncriptado,
            string contrasenaEncriptada)
        {
            try
            {
                if (!IdentificacionValida(identificacion))
                {
                    return Error(MSG_MODIFICAR_ERROR);
                }

                string usuario = DescifrarAES(usuarioEncriptado);
                string contrasena = DescifrarAES(contrasenaEncriptada);

                usuario = usuario.Trim();

                if (!DatosBaseValidosSinContrasena(identificacion, nombre, primerApellido,
                    segundoApellido, correoElectronico, usuario)
                    || (!string.IsNullOrWhiteSpace(contrasena) && !ContrasenaValida(contrasena)))
                {
                    return Error(MSG_MODIFICAR_ERROR);
                }

                IMongoCollection<UsuarioMongo> coleccion = ObtenerColeccion();
                string identificacionNormalizada = Normalizar(identificacion);
                string usuarioHash = HashSHA256(usuario);

                UsuarioMongo existente = coleccion.Find(
                    Builders<UsuarioMongo>.Filter.Eq(x => x.Identificacion, identificacionNormalizada)
                ).FirstOrDefault();

                if (existente == null)
                {
                    return Error(MSG_MODIFICAR_ERROR);
                }

                bool usuarioUsadoPorOtro = coleccion.Find(
                    Builders<UsuarioMongo>.Filter.Eq(x => x.UsuarioHash, usuarioHash) &
                    Builders<UsuarioMongo>.Filter.Ne(x => x.Identificacion, identificacionNormalizada)
                ).Any();

                if (usuarioUsadoPorOtro)
                {
                    return Error(MSG_MODIFICAR_ERROR);
                }

                var actualizacion = Builders<UsuarioMongo>.Update
                    .Set(x => x.Nombre, nombre.Trim())
                    .Set(x => x.PrimerApellido, primerApellido.Trim())
                    .Set(x => x.SegundoApellido, segundoApellido.Trim())
                    .Set(x => x.CorreoElectronico, correoElectronico.Trim())
                    .Set(x => x.UsuarioEncriptado, CifrarAES(usuario))
                    .Set(x => x.UsuarioHash, usuarioHash);

                if (!string.IsNullOrWhiteSpace(contrasena))
                {
                    actualizacion = actualizacion
                        .Set(x => x.ContrasenaEncriptada, CifrarAES(contrasena))
                        .Set(x => x.ContrasenaHash, HashSHA256(contrasena));
                }

                coleccion.UpdateOne(
                    Builders<UsuarioMongo>.Filter.Eq(x => x.Identificacion, identificacionNormalizada),
                    actualizacion);

                return Ok();
            }
            catch
            {
                return Error(MSG_MODIFICAR_ERROR);
            }
        }

        [WebMethod]
        public RespuestaAutenticacion CambiarEstadoUsuario(string identificacion, string estado)
        {
            try
            {
                if (!IdentificacionValida(identificacion) || !EstadoValido(estado))
                {
                    return Error(MSG_ESTADO_ERROR);
                }

                IMongoCollection<UsuarioMongo> coleccion = ObtenerColeccion();
                string identificacionNormalizada = Normalizar(identificacion);
                string estadoNormalizado = estado.Trim().ToLower();

                var resultado = coleccion.UpdateOne(
                    Builders<UsuarioMongo>.Filter.Eq(x => x.Identificacion, identificacionNormalizada),
                    Builders<UsuarioMongo>.Update.Set(x => x.Estado, estadoNormalizado));

                if (resultado.MatchedCount == 0)
                {
                    return Error(MSG_ESTADO_ERROR);
                }

                return Ok();
            }
            catch
            {
                return Error(MSG_ESTADO_ERROR);
            }
        }

        [WebMethod]
        public string EncriptarTexto(string texto)
        {
            return CifrarAES(texto);
        }

        [WebMethod]
        public RespuestaAutenticacion ProbarConexionMongo()
        {
            try
            {
                ObtenerColeccion().Find(Builders<UsuarioMongo>.Filter.Empty).Limit(1).ToList();
                return Ok();
            }
            catch (Exception ex)
            {
                return new RespuestaAutenticacion
                {
                    Resultado = false,
                    Mensaje = "Error MongoDB: " + ex.Message
                };
            }
        }

        [WebMethod]
        public List<UsuarioConsulta> ListarUsuarios(int tipo)
        {
            var resultado = new List<UsuarioConsulta>();

            if (!TipoValido(tipo))
            {
                return resultado;
            }

            try
            {
                var usuarios = ObtenerColeccion()
                    .Find(Builders<UsuarioMongo>.Filter.Eq(x => x.Tipo, tipo))
                    .SortBy(x => x.Nombre)
                    .ThenBy(x => x.PrimerApellido)
                    .ToList();

                foreach (UsuarioMongo usuario in usuarios)
                {
                    resultado.Add(new UsuarioConsulta
                    {
                        Identificacion = usuario.Identificacion,
                        Nombre = usuario.Nombre,
                        PrimerApellido = usuario.PrimerApellido,
                        SegundoApellido = usuario.SegundoApellido,
                        CorreoElectronico = usuario.CorreoElectronico,
                        Usuario = DescifrarAES(usuario.UsuarioEncriptado),
                        Contrasena = "**************",
                        Estado = usuario.Estado,
                        Tipo = usuario.Tipo
                    });
                }
            }
            catch
            {
                resultado.Clear();
            }

            return resultado;
        }

        [WebMethod]
        public RespuestaAutenticacion EliminarUsuario(string identificacion)
        {
            try
            {
                if (!IdentificacionValida(identificacion))
                {
                    return Error("Usuario no existe o identificación incorrecta.");
                }

                var resultado = ObtenerColeccion().DeleteOne(
                    Builders<UsuarioMongo>.Filter.Eq(
                        x => x.Identificacion,
                        Normalizar(identificacion)));

                return resultado.DeletedCount == 1
                    ? Ok()
                    : Error("Usuario no existe o identificación incorrecta.");
            }
            catch
            {
                return Error("No fue posible eliminar el usuario.");
            }
        }

        private IMongoCollection<UsuarioMongo> ObtenerColeccion()
        {
            MongoClient cliente = new MongoClient(ObtenerConfiguracion("MongoUri", MONGO_URI_PREDETERMINADA));
            IMongoDatabase db = cliente.GetDatabase(
                ObtenerConfiguracion("MongoDatabase", DATABASE_NAME_PREDETERMINADO));
            IMongoCollection<UsuarioMongo> coleccion = db.GetCollection<UsuarioMongo>(
                ObtenerConfiguracion("MongoCollection", COLLECTION_NAME_PREDETERMINADA));

            AsegurarIndices(coleccion);
            return coleccion;
        }

        private void AsegurarIndices(IMongoCollection<UsuarioMongo> coleccion)
        {
            if (indicesCreados)
            {
                return;
            }

            lock (BLOQUEO_INDICES)
            {
                if (indicesCreados)
                {
                    return;
                }

                CreateIndexOptions opciones = new CreateIndexOptions { Unique = true };
                coleccion.Indexes.CreateMany(new[]
                {
                    new CreateIndexModel<UsuarioMongo>(
                        Builders<UsuarioMongo>.IndexKeys.Ascending(x => x.Identificacion),
                        opciones),
                    new CreateIndexModel<UsuarioMongo>(
                        Builders<UsuarioMongo>.IndexKeys.Ascending(x => x.UsuarioHash),
                        new CreateIndexOptions { Unique = true })
                });

                indicesCreados = true;
            }
        }

        private string ObtenerConfiguracion(string clave, string valorPredeterminado)
        {
            string valor = ConfigurationManager.AppSettings[clave];
            return string.IsNullOrWhiteSpace(valor) ? valorPredeterminado : valor.Trim();
        }

        private bool DatosUsuarioValidos(string identificacion, string nombre, string primerApellido,
            string segundoApellido, string correo, string usuario, string contrasena, int tipo)
        {
            return DatosBaseValidos(identificacion, nombre, primerApellido, segundoApellido,
                    correo, usuario, contrasena)
                && TipoValido(tipo);
        }

        private bool DatosBaseValidos(string identificacion, string nombre, string primerApellido,
            string segundoApellido, string correo, string usuario, string contrasena)
        {
            return IdentificacionValida(identificacion)
                && NombreValido(nombre)
                && NombreValido(primerApellido)
                && NombreValido(segundoApellido)
                && CorreoValido(correo)
                && TextoPresente(usuario)
                && ContrasenaValida(contrasena);
        }

        private bool DatosBaseValidosSinContrasena(string identificacion, string nombre,
            string primerApellido, string segundoApellido, string correo, string usuario)
        {
            return IdentificacionValida(identificacion)
                && NombreValido(nombre)
                && NombreValido(primerApellido)
                && NombreValido(segundoApellido)
                && CorreoValido(correo)
                && TextoPresente(usuario);
        }

        private bool IdentificacionValida(string valor)
        {
            return TextoPresente(valor) && Regex.IsMatch(Normalizar(valor), @"^\d+$");
        }

        private bool NombreValido(string valor)
        {
            return TextoPresente(valor)
                && Regex.IsMatch(valor.Trim(), @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$");
        }

        private bool CorreoValido(string valor)
        {
            return TextoPresente(valor) && Regex.IsMatch(valor.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private bool ContrasenaValida(string valor)
        {
            if (string.IsNullOrEmpty(valor) || valor.Length != 14 || Regex.IsMatch(valor, @"\s"))
            {
                return false;
            }

            return Regex.IsMatch(valor, @"\p{Lu}")
                && Regex.IsMatch(valor, @"\p{Ll}")
                && Regex.IsMatch(valor, @"\d")
                && Regex.IsMatch(valor, @"[^\p{L}\p{Nd}]");
        }

        private bool TipoValido(int tipo)
        {
            return tipo == 1 || tipo == 2;
        }

        private bool EstadoNuevoValido(string estado)
        {
            return TextoPresente(estado) && estado.Trim().ToLower() == "activo";
        }

        private bool EstadoValido(string estado)
        {
            if (!TextoPresente(estado)) return false;
            string valor = estado.Trim().ToLower();
            return valor == "activo" || valor == "inactivo";
        }

        private bool TextoPresente(string valor)
        {
            return !string.IsNullOrWhiteSpace(valor);
        }

        private string Normalizar(string valor)
        {
            return valor == null ? "" : valor.Trim();
        }

        private RespuestaAutenticacion Ok()
        {
            return new RespuestaAutenticacion { Resultado = true, Mensaje = MSG_OK };
        }

        private RespuestaAutenticacion Error(string mensaje)
        {
            return new RespuestaAutenticacion { Resultado = false, Mensaje = mensaje };
        }

        private string CifrarAES(string textoPlano)
        {
            byte[] key = ObtenerLlaveAES();

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                byte[] textoBytes = Encoding.UTF8.GetBytes(textoPlano ?? "");

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                {
                    byte[] cifrado = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);
                    byte[] resultado = new byte[aes.IV.Length + cifrado.Length];

                    Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
                    Buffer.BlockCopy(cifrado, 0, resultado, aes.IV.Length, cifrado.Length);

                    return Convert.ToBase64String(resultado);
                }
            }
        }

        private string DescifrarAES(string textoEncriptado)
        {
            byte[] data = Convert.FromBase64String(textoEncriptado);

            if (data.Length <= 16)
            {
                throw new CryptographicException("Dato cifrado inválido.");
            }

            byte[] key = ObtenerLlaveAES();

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
                    byte[] plano = decryptor.TransformFinalBlock(cifrado, 0, cifrado.Length);
                    return Encoding.UTF8.GetString(plano);
                }
            }
        }

        private byte[] ObtenerLlaveAES()
        {
            string llave = ObtenerConfiguracion("AesKey", AES_KEY_PREDETERMINADA);
            byte[] bytes = Encoding.UTF8.GetBytes(llave);

            if (bytes.Length != 16 && bytes.Length != 24 && bytes.Length != 32)
            {
                throw new CryptographicException("La llave AES debe tener 16, 24 o 32 bytes.");
            }

            return bytes;
        }

        private string HashSHA256(string texto)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes((texto ?? "").Trim());
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

    public class RespuestaAutenticacion
    {
        public bool Resultado { get; set; }
        public string Mensaje { get; set; }
    }

    public class UsuarioConsulta
    {
        public string Identificacion { get; set; }
        public string Nombre { get; set; }
        public string PrimerApellido { get; set; }
        public string SegundoApellido { get; set; }
        public string CorreoElectronico { get; set; }
        public string Usuario { get; set; }
        public string Contrasena { get; set; }
        public string Estado { get; set; }
        public int Tipo { get; set; }
    }

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
