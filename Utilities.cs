using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace JiraDevOpsIntegrationFunctions
{
    public static class Utilities
    {
        public static string GetRandomString(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string s = "";
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (s.Length != length)
                {
                    byte[] oneByte = new byte[1];
                    provider.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character))
                    {
                        s += character;
                    }
                }
            }
            return s;
        }

        public static string HashValue(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                return BitConverter.ToString(hashedBytes).Replace("-", string.Empty).ToUpper();
            }
        }
    }
}
