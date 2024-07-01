using System;
using LinJector.Core.Resolver.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class TypedSingletonResolver : SingletonResolver, IConsiderPostInitializeResolver
    {
        private bool _inited;
        
        private TypedResolver _resolver;
        
        private Type _type;

        private bool _cached;
        
        private object _result;
        
        private bool _noLazy;
        
        public TypedSingletonResolver(bool noLazy, Type type)
        {
            _type = type;
            _noLazy = noLazy;
        }

        private void MakeResolvable(Container container)
        {
            if (!_cached)
            {
                _cached = true;
                _result = _resolver.Resolve(container);
            }
        }

        public override object Resolve(Container container)
        {
            MakeResolvable(container);
            return _result;
        }

        public override T Resolve<T>(Container container)
        {
            MakeResolvable(container);
            return (T) _result;
        }
        

        public void PostInitialize(Container container)
        {
            if (!_inited)
            {
                _inited = true;
                _resolver = new TypedResolver(container, _type, Array.Empty<object>());
            }
            
            if (_noLazy) MakeResolvable(container);
        }
    }
}