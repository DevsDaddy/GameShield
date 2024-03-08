using DevsDaddy.Shared.EventFramework.Core.Extensions;
using UnityEngine;

namespace DevsDaddy.Shared.EventFramework.Core.Objects
{
    public class MonoSingleton<TInterface, TImplementation> : MonoBehaviour where TImplementation : MonoBehaviour, TInterface
    {
        private static TInterface _main;
        
        public static TInterface Main
        {
            get
            {
                var res = InvalidateInstance();
                return res;
            }
        }
        
        private void Awake()
        {
            InvalidateInstance();
        }
        
        private static TInterface InvalidateInstance()
        {
            if(!Equals(_main, default(TImplementation)))
            {
                return _main;
            }

            var typeInterface = typeof(TInterface);
            var objects = FindObjectsOfType<TImplementation>();
            if(!objects.IsNullOrEmpty())
            {
                foreach (var obj in objects)
                {
                    if(typeInterface.IsAssignableFrom(obj.GetType()))
                    {
                        _main = obj;
                    }
                }
            }
            
            if (!Equals(_main, default(TImplementation)))
            {
                return _main;
            }
            
            var typeImplementation = typeof(TImplementation);
            var go = new GameObject($"[{typeImplementation.Name}]");
            go.transform.SetParent(null);

            _main = go.AddComponent<TImplementation>();
            return _main;
        }
    }
}