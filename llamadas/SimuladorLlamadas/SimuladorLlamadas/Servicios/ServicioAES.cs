using System;
using System.Security.Cryptography;
using System.Text;

namespace SimuladorLlamadas.Servicios
{
    public static class ServicioAES
    {
        // Constantes del cifrado AES-GCM
        private const int tamano_nonce = 12;
        private const int tamano_tag = 16;

        public static string cifrar_texto(string texto_plano, string llave_aes_base64)
        {
            byte[] llave_aes;
            byte[] nonce;
            byte[] texto_plano_bytes;
            byte[] texto_cifrado_bytes;
            byte[] tag;
            byte[] resultado;
            string texto_cifrado_base64;

            llave_aes = Convert.FromBase64String(llave_aes_base64);
            validar_llave_aes(llave_aes);

            nonce = RandomNumberGenerator.GetBytes(tamano_nonce);
            texto_plano_bytes = Encoding.UTF8.GetBytes(texto_plano);
            texto_cifrado_bytes = new byte[texto_plano_bytes.Length];
            tag = new byte[tamano_tag];

            using (AesGcm aes_gcm = new AesGcm(llave_aes, tamano_tag))
            {
                aes_gcm.Encrypt(
                    nonce,
                    texto_plano_bytes,
                    texto_cifrado_bytes,
                    tag
                );
            }

            resultado = new byte[nonce.Length + texto_cifrado_bytes.Length + tag.Length];

            Buffer.BlockCopy(nonce, 0, resultado, 0, nonce.Length);

            Buffer.BlockCopy(
                texto_cifrado_bytes,
                0,
                resultado,
                nonce.Length,
                texto_cifrado_bytes.Length
            );

            Buffer.BlockCopy(
                tag,
                0,
                resultado,
                nonce.Length + texto_cifrado_bytes.Length,
                tag.Length
            );

            texto_cifrado_base64 = Convert.ToBase64String(resultado);

            return texto_cifrado_base64;
        }

        public static string generar_llave_aes_256()
        {
            byte[] llave_aes;

            llave_aes = RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(llave_aes);
        }

        private static void validar_llave_aes(byte[] llave_aes)
        {
            if (llave_aes.Length != 16 &&
                llave_aes.Length != 24 &&
                llave_aes.Length != 32)
            {
                throw new ArgumentException(
                    "La llave AES debe tener 16, 24 o 32 bytes."
                );
            }
        }
    }
}