using System;
using LinJector.Core.Resolver.Base;

namespace LinJector.Core.Resolver
{
    public class FromFactoryTransientResolver : TransientResolver
    {
        private Func<Container, object> _activator;
        
        public FromFactoryTransientResolver(Func<Container, object> activator)
        {
            if (activator == null) throw LinJectErrors.TypedResolverCanNotActivate();
            _activator = activator;
        }
        
        public override object Resolve(Container container)
        {
            return _activator.Invoke(container);
        }

        public override T Resolve<T>(Container container)
        {
            return (T) _activator.Invoke(container);
        }
    }
}