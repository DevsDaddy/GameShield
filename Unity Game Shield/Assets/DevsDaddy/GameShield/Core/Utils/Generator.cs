using System;

namespace DevsDaddy.GameShield.Core.Utils
{
    /// <summary>
    /// GameShield Generator Util Class
    /// </summary>
    public static class Generator
    {
        // Used in generators available characters
        public const string GeneratorSymbols = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        
        /// <summary>
        /// Generate Random String Key
        /// </summary>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string GenerateRandomKey(int len = 32) {
            var stringChars = new char[len];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = GeneratorSymbols[random.Next(GeneratorSymbols.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
    }
}