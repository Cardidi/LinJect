using System;
using LinJector.Core.Resolver.Base;

namespace LinJector.Core.Resolver
{
    public class ParameterTypedTransientResolver : TransientResolver
    {
        private TypedResolver _resolver;
        
        public ParameterTypedTransientResolver(Type type, object[] arguments)
        {
            _resolver = TypedResolver.Get(type, arguments);
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