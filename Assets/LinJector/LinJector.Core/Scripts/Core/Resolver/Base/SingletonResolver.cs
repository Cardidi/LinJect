using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolver.Base
{
    public abstract class SingletonResolver : ILifetimeResolver
    {
        public CrossContainer CrossContainerStrategy => CrossContainer.Move;

        public bool Duplicate(out ILifetimeResolver resolver)
        {
            resolver = new ProxyResolver(this);
            return true;
        }

        public abstract object Resolve(Container container);

        public abstract T Resolve<T>(Container container);
    }
}