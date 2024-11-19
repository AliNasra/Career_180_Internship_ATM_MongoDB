using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;



namespace ConsoleApp1
{
    public class EncryptionServices
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16-byte key for AES-128
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("6543210987654321");  // 16-byte IV

        public static string EncryptPassword(string plainTextPassword)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainTextPassword);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
        public static string DecryptPassword(string encryptedPassword)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                // Convert the Base64 string back to a byte array
                byte[] encryptedBytes = Convert.FromBase64String(encryptedPassword);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        // Read the decrypted bytes from the decrypting stream and return as string
                        return sr.ReadToEnd();
                    }
                }
            }
        }


    }

}
