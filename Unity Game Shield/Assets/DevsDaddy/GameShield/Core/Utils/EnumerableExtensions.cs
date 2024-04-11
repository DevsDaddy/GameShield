using System;
using System.Collections.Generic;
using System.Linq;

namespace DevsDaddy.GameShield.Core.Utils
{
    /// <summary>
    /// Enumerable Extensions
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Shuffle
        /// </summary>
        /// <param name="sequence"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IList<T> Shuffle<T>(this IEnumerable<T> sequence)
        {
            return sequence.Shuffle(new Random());
        }

        /// <summary>
        /// Shuffle
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="randomNumberGenerator"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IList<T> Shuffle<T>(this IEnumerable<T> sequence, Random randomNumberGenerator)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException("sequence");
            }

            if (randomNumberGenerator == null)
            {
                throw new ArgumentNullException("randomNumberGenerator");
            }

            T swapTemp;
            List<T> values = sequence.ToList();
            int currentlySelecting = values.Count;
            while (currentlySelecting > 1)
            {
                int selectedElement = randomNumberGenerator.Next(currentlySelecting);
                --currentlySelecting;
                if (currentlySelecting != selectedElement)
                {
                    swapTemp = values[currentlySelecting];
                    values[currentlySelecting] = values[selectedElement];
                    values[selectedElement] = swapTemp;
                }
            }

            return values;
        }
    }
}