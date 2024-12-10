using Microsoft.AspNetCore.Identity;
using Proj_Biblioteca.Models;
using System.Security.Cryptography;
using System.Text;

namespace Proj_Biblioteca.Utils
{
    public static class Encryption
    {
        public static string Encrypt(string clearText)
        {
            string EncryptionKey = "abc123";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                #pragma warning disable SYSLIB0041 // Il tipo o il membro è obsoleto
                Rfc2898DeriveBytes pdb = new(EncryptionKey, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
                #pragma warning restore SYSLIB0041 // Il tipo o il membro è obsoleto
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
            return clearText;
        }
        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "abc123";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                #pragma warning disable SYSLIB0041 // Il tipo o il membro è obsoleto
                Rfc2898DeriveBytes pdb = new(EncryptionKey, [0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76]);
                #pragma warning restore SYSLIB0041 // Il tipo o il membro è obsoleto
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using MemoryStream ms = new();
                using (CryptoStream cs = new(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                    cs.Close();
                }
                cipherText = Encoding.Unicode.GetString(ms.ToArray());
            }
            return cipherText;
        }

        public static string HashPassword(string password, Utente utente)
        {
            return new PasswordHasher<Utente>().HashPassword(utente, password);
        }

        public static string VerifyPassword(string input, Utente utente )
        {
            var result = new PasswordHasher<Utente>().VerifyHashedPassword(utente, utente.PasswordHash??"", input);

            return result switch
            {
                PasswordVerificationResult.Success => "Verificato",
                PasswordVerificationResult.Failed => "Fallito",
                PasswordVerificationResult.SuccessRehashNeeded => "Dovresti cambiare password",

                _ => "Errore",
            };
        }
    }
}