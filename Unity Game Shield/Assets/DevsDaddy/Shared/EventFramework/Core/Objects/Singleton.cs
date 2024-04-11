namespace DevsDaddy.Shared.EventFramework.Core.Objects
{
    public abstract class Singleton<TInterface, TImplementation> 
        where TImplementation : TInterface, new()
    {
        public static TInterface Main { get; } = new TImplementation();

        protected Singleton()
        {
        }
    }
}