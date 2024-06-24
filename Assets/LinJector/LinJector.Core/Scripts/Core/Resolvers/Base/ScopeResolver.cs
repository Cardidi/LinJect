using System.Collections.Generic;
using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolvers.Base
{
    public abstract class ScopeResolver : ILifetimeResolver
    {
        public CrossContainer CrossContainerStrategy => CrossContainer.Copy;
        
        public abstract bool Duplicate(out ILifetimeResolver resolver);

        public abstract object Resolve(Container container);

        public abstract T Resolve<T>(Container container);
    }
}