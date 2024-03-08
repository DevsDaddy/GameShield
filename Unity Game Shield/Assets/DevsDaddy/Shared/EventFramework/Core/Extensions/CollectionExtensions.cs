using System;
using System.Collections.Generic;

namespace DevsDaddy.Shared.EventFramework.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static object[] ToObjectArray<T>(this T[] source)
        {
            if(source.IsNullOrEmpty())
            {
                return null;
            }
            var copy = new object[source.Length];
            Array.Copy(source, copy, source.Length);
            return copy;
        }
        
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            if(source == null)
            {
                return true;
            }
            if(source.Count < 1)
            {
                return true;
            }
            return false;
        }
        
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }
        
        public static bool IsNullOrEmpty(this string source)
        {
            var isEmpty = string.IsNullOrEmpty(source);
            return isEmpty;
        }
    }
}