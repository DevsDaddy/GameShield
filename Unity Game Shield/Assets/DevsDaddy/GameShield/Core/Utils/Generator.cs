using System;
using System.Linq;
using DevsDaddy.GameShield.Core.Constants;
using UnityEngine;
using Random = System.Random;

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

        /// <summary>
        /// Generate an Unique Random Array of values with min and max cap
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static int[] GenerateUniqueRandomRange(int min, int max, int length) {
            if (length > max - min)
            {
                Debug.LogWarning($"{GeneralStrings.LOG_PREFIX} Range {min}-{max} is too small to have {length} unique random numbers.");
                return null;
            }
            
            int[] myNumbers = new int[length];
            if (length == max - min)
            {
                for (int i = 0; i < length; i++)
                    myNumbers[i] = i;
                return myNumbers;
            }
            
            System.Random randNum = new System.Random();
            
            for (int currIndex = 0; currIndex < length; currIndex++)
            {
                int randCandidate = randNum.Next(min, max);
                while (myNumbers.Contains(randCandidate))
                {
                    randCandidate = randNum.Next(min, max);
                }
 
                myNumbers[currIndex] = randCandidate;
 
                // some optimizations
                if (randCandidate == min)
                    min++;
 
                if (randCandidate == max)
                    max--;
            }
 
            return myNumbers;
        }
    }
}