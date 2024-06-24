using System;
using LinJector.Core.Resolvers.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolvers
{
    public class TypedSingletonResolver : SingletonResolver, IConsiderPreInitializeResolver
    {
        private TypedResolver _resolver;

        private bool _cached;
        
        private object _result;
        
        private bool _noLazy;
        
        public TypedSingletonResolver(bool noLazy, Type type)
        {
            _resolver = TypedResolver.Get(type);
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