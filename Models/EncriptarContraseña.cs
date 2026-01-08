using System.Security.Cryptography;
using System.Text;

namespace El_Buen_Taco.Models
{
    public class EncriptarContraseña
    {
        public static string ComputeSHA256(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return ConvertToHexString(bytes);
            }
        }

        // Convertir byte array a hexadecimal
        private static string ConvertToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2")); // x2 = hexadecimal de 2 dígitos
            }
            return builder.ToString();
        }

    }
}
