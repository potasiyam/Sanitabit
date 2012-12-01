using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace SilverlightPhoneDatabase
{
    /// <summary>
    /// Class used to encrypt the database
    /// </summary>
    public static class Cryptography
    {

        /// <summary>
        /// Encrypt the input using password provided
        /// </summary>
        /// <param name="input">Input string to encrypt</param>
        /// <param name="password">Password to use</param>
        /// <returns>Encrypted string</returns>
        public static string Encrypt(string input, string password)
        {

            string data = input;
// ReSharper disable AccessToStaticMemberViaDerivedType
            byte[] utfdata = UTF8Encoding.UTF8.GetBytes(data);
// ReSharper restore AccessToStaticMemberViaDerivedType
// ReSharper disable AccessToStaticMemberViaDerivedType
            byte[] saltBytes = UTF8Encoding.UTF8.GetBytes(password);
// ReSharper restore AccessToStaticMemberViaDerivedType



            // Our symmetric encryption algorithm
            using (var aes = new AesManaged())
            {

                // We're using the PBKDF2 standard for password-based key generation
                var rfc = new Rfc2898DeriveBytes(password, saltBytes);

                // Setting our parameters
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                aes.Key = rfc.GetBytes(aes.KeySize/8);
                aes.IV = rfc.GetBytes(aes.BlockSize/8);

                // Encryption
                ICryptoTransform encryptTransf = aes.CreateEncryptor();

                // Output stream, can be also a FileStream
                using (var encryptStream = new MemoryStream())
                {
                    var encryptor = new CryptoStream(encryptStream, encryptTransf, CryptoStreamMode.Write);

                    encryptor.Write(utfdata, 0, utfdata.Length);
                    encryptor.Flush();
                    encryptor.Close();

                    byte[] encryptBytes = encryptStream.ToArray();
                    string encryptedString = Convert.ToBase64String(encryptBytes);

                    return encryptedString;
                }
            }
        }

        /// <summary>
        /// Decrypt string using password provided
        /// </summary>
        /// <param name="base64Input">Input to decrypt</param>
        /// <param name="password">Password to use</param>
        /// <returns>Decrypted string</returns>
        public static string Decrypt(string base64Input, string password)
        {

            byte[] encryptBytes = Convert.FromBase64String(base64Input);
            byte[] saltBytes = Encoding.UTF8.GetBytes(password);

            // Our symmetric encryption algorithm
            using (var aes = new AesManaged())
            {

                // We're using the PBKDF2 standard for password-based key generation
                var rfc = new Rfc2898DeriveBytes(password, saltBytes);

                // Setting our parameters
                aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
                aes.KeySize = aes.LegalKeySizes[0].MaxSize;
                aes.Key = rfc.GetBytes(aes.KeySize/8);
                aes.IV = rfc.GetBytes(aes.BlockSize/8);

                // Now, decryption
                ICryptoTransform decryptTrans = aes.CreateDecryptor();

                // Output stream, can be also a FileStream
                using (var decryptStream = new MemoryStream())
                {
                    var decryptor = new CryptoStream(decryptStream, decryptTrans, CryptoStreamMode.Write);

                    decryptor.Write(encryptBytes, 0, encryptBytes.Length);
                    decryptor.Flush();
                    decryptor.Close();

                    byte[] decryptBytes = decryptStream.ToArray();
                    // ReSharper disable AccessToStaticMemberViaDerivedType
                    var decryptedString = UTF8Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
                    // ReSharper restore AccessToStaticMemberViaDerivedType
                    return decryptedString;
                }
            }
        }
    }
}
