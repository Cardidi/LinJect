using System;

namespace LinJector.Core.Resolvers
{
    public class ParameterTypedSingletonResolver : SingletonResolver
    {
        private TypedResolver _resolver;

        private bool _cached;
        
        private object _result;
        
        public ParameterTypedSingletonResolver(Type type, object[] arguments)
        {
            _resolver = TypedResolver.Get(type, arguments);
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
    }
}