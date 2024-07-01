using System;
using LinJector.Core.Resolver.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class TypedTransientResolver : TransientResolver, IConsiderPostInitializeResolver
    {
        private bool _inited;
        
        private TypedResolver _resolver;
        
        private Type _type;
        
        public TypedTransientResolver(Type type)
        {
            _type = type;
        }
        
        public override object Resolve(Container container)
        {
            return _resolver.Resolve(container);
        }

        public override T Resolve<T>(Container container)
        {
            return _resolver.Resolve<T>(container);
        }
        
        public void PostInitialize(Container container)
        {
            if (!_inited)
            {
                _inited = true;
                _resolver = new TypedResolver(container, _type, Array.Empty<object>());
            }
        }
    }
}