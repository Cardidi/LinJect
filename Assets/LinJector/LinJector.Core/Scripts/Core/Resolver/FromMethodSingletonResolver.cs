using System;
using LinJector.Core.Resolver.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class FromMethodSingletonResolver : SingletonResolver, IConsiderPreInitializeResolver
    {

        private Func<Container, object> _activator;

        private bool _cached;
        
        private object _result;

        private bool _noLazy;
        
        public FromMethodSingletonResolver(bool noLazy, Func<Container, object> activator)
        {
            if (activator == null) throw LinJectErrors.TypedResolverCanNotActivate();
            _activator = activator;
            _noLazy = noLazy;
        }
        
        private void MakeResolvable(Container container)
        {
            if (!_cached)
            {
                _cached = true;
                _result = _activator(container);
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