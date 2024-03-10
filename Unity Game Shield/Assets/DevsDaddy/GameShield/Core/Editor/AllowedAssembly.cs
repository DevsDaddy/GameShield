using System;

namespace DevsDaddy.GameShield.Core.Editor
{
    /// <summary>
    /// Allowed Assembly Class for Editor
    /// </summary>
    internal class AllowedAssembly
    {
        public string name;
        public int[] hashes;
        
        public AllowedAssembly(string name, int[] hashes)
        {
            this.name = name;
            this.hashes = hashes;
        }
        
        public bool AddHash(int hash)
        {
            if (Array.IndexOf(hashes, hash) != -1) return false;

            int oldLen = hashes.Length;
            int newLen = oldLen + 1;

            int[] newHashesArray = new int[newLen];
            Array.Copy(hashes, newHashesArray, oldLen);

            hashes = newHashesArray;
            hashes[oldLen] = hash;

            return true;
        }

        public override string ToString()
        {
            return name + " (hashes: " + hashes.Length + ")";
        }
    }
}