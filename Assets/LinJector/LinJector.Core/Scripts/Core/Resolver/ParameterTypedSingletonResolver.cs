using System;
using LinJector.Core.Resolver.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class ParameterTypedSingletonResolver : SingletonResolver, IConsiderPreInitializeResolver
    {
        private TypedResolver _resolver;

        private bool _cached;
        
        private object _result;
        
        private bool _noLazy;
        
        public ParameterTypedSingletonResolver(bool noLazy, Type type, object[] arguments)
        {
            _resolver = TypedResolver.Get(type, arguments);
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
        
        public void PreInitialize(Container container)
        {
            if (_noLazy) MakeResolvable(container);
        }
    }
}