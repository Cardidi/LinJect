using System.Collections.Generic;
using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolvers
{
    public abstract class ScopeResolver : ILifetimeResolver
    {
        public Lifetime Lifetime => Lifetime.Scope;

        protected Dictionary<uint, object> ContainerCache { get; } = new();  

        public abstract bool Duplicate(out ILifetimeResolver resolver);

        public abstract object Resolve(Container container);

        public abstract T Resolve<T>(Container container);
    }
}