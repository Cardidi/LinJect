using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolvers
{
    public abstract class TransientResolver : ILifetimeResolver
    {
        public Lifetime Lifetime => Lifetime.Transient;
        
        public abstract bool Duplicate(out ILifetimeResolver resolver);

        public abstract object Resolve(Container container);

        public abstract T Resolve<T>(Container container);
    }
}