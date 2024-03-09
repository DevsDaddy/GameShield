namespace DevsDaddy.GameShield.Core.Modules.Memory
{
    /// <summary>
    /// Memory Protector Module
    /// </summary>
    public class MemoryProtector : IShieldModule
    {

        /// <summary>
        /// Setup Module
        /// </summary>
        /// <param name="config"></param>
        public void SetupModule(IShieldModuleConfig config = null) {
            
        }

        /// <summary>
        /// Disconnect Module
        /// </summary>
        public void Disconnect() {
            
        }

        /// <summary>
        /// Get Module Information
        /// </summary>
        /// <returns></returns>
        public ModuleInfo GetModuleInfo() {
            return new ModuleInfo {
                Name = "Memory Protector",
                Description = ""
            };
        }
    }
}