using System;
using LinJector.Core.Resolver.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class ParameterTypedSingletonResolver : SingletonResolver, IConsiderPostInitializeResolver
    {
        private bool _inited;
        
        private TypedResolver _resolver;
        
        private Type _type;

        private object[] _arguments;

        private bool _cached;
        
        private object _result;
        
        private bool _noLazy;
        
        public ParameterTypedSingletonResolver(bool noLazy, Type type, object[] arguments)
        {
            _type = type;
            _arguments = arguments;
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
                _resolver = new TypedResolver(container, _type, _arguments);
            }
            
            if (_noLazy) MakeResolvable(container);
        }
    }
}