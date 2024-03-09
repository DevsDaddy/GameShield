using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DevsDaddy.GameShield.Core.Modules;

namespace DevsDaddy.GameShield.Core.Utils
{
    /// <summary>
    /// Module Manager Interface
    /// </summary>
    public static class ModuleManager
    {
        /// <summary>
        /// Get All Modules
        /// </summary>
        /// <returns></returns>
        public static List<IShieldModule> GetAllModules() {
            var modulesList = new List<IShieldModule>();
            foreach(Type t in FindDerivedTypes(Assembly.GetExecutingAssembly(), typeof(IShieldModule)))
            {
                var instance = (IShieldModule) Activator.CreateInstance(t);
                modulesList.Add(instance);
            }

            return modulesList;
        }
        
        /// <summary>
        /// Find Type by Base Type
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType)
        {
            return assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t));
        }
    }
}