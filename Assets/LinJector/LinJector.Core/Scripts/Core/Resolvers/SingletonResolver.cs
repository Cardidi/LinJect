using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolvers
{
    public abstract class SingletonResolver : ILifetimeResolver
    {
        public Lifetime Lifetime => Lifetime.Singleton;

        public bool Duplicate(out ILifetimeResolver resolver)
        {
            resolver = this;
            return false;
        }

        public abstract object Resolve(Container container);

        public abstract T Resolve<T>(Container container);
    }
}