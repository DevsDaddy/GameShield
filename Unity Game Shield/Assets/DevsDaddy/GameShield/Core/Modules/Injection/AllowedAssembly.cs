namespace DevsDaddy.GameShield.Core.Modules.Injection
{
    /// <summary>
    /// Allowed Assembly Class
    /// </summary>
    public class AllowedAssembly
    {
        public readonly string name;
        public readonly int[] hashes;
        
        public AllowedAssembly(string name, int[] hashes)
        {
            this.name = name;
            this.hashes = hashes;
        }
    }
}