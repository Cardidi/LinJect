using System;
using LinJector.Core.Resolvers.Base;

namespace LinJector.Core.Resolvers
{
    public class TypedTransientResolver : TransientResolver
    {
        private TypedResolver _resolver;

        public TypedTransientResolver(Type type)
        {
            _resolver = TypedResolver.Get(type);
        }
        
        public override object Resolve(Container container)
        {
            return _resolver.Resolve(container);
        }

        public override T Resolve<T>(Container container)
        {
            return _resolver.Resolve<T>(container);
        }
    }
}