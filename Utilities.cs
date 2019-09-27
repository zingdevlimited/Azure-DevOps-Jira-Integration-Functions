using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace JiraDevOpsIntegrationFunctions
{
    public static class Utilities
    {
        /// <summary>
        /// This method is used in Access Token generation
        /// </summary>
        /// <param name="length">The length of the Token</param>
        /// <returns>Access Token</returns>
        public static string GetToken(int length)
        {
            const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            string Token = "";
            RNGCryptoServiceProvider Provider = new RNGCryptoServiceProvider();
            while (Token.Length != length)
            {
                byte[] oneByte = new byte[1];
                Provider.GetBytes(oneByte);
                char character = (char)oneByte[0];
                if (Chars.Contains(character))
                {
                    Token += character;
                }
            }
            return Token;
        }

        public static string GetHashedToken(string token)
        {
            SHA256 sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToUpper();
        }
    }
}
