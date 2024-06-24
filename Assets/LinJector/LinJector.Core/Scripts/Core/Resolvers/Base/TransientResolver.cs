using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolvers.Base
{
    public abstract class TransientResolver : ILifetimeResolver, IIgnoreEventResolver
    {
        public CrossContainer CrossContainerStrategy => CrossContainer.Move;

        public bool Duplicate(out ILifetimeResolver resolver)
        {
            resolver = this;
            return false;
        }

        public abstract object Resolve(Container container);

        public abstract T Resolve<T>(Container container);
    }
}