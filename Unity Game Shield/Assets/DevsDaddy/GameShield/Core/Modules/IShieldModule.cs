namespace DevsDaddy.GameShield.Core.Modules
{
    public interface IShieldModule
    {
        /// <summary>
        /// Setup Module Configuration
        /// </summary>
        /// <param name="config"></param>
        public void SetupModule(IShieldModuleConfig config = null);

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect();

        /// <summary>
        /// Get Module Info
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo();
    }
}