using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class ProxyResolver : ILifetimeResolver, IIgnoreEventResolver
    {
        private ILifetimeResolver _innerResolver;

        public CrossContainer CrossContainerStrategy => CrossContainer.Move;

        public ProxyResolver(ILifetimeResolver resolver)
        {
            if (resolver == null) throw LinJectErrors.TypedResolverCanNotActivate();
            
            _innerResolver = resolver;
        }
        
        public object Resolve(Container container)
        {
            return _innerResolver.Resolve(container);
        }

        public T Resolve<T>(Container container)
        {
            return _innerResolver.Resolve<T>(container);
        }
        
        public bool Duplicate(out ILifetimeResolver resolver)
        {
            resolver = new ProxyResolver(_innerResolver);
            return false;
        }
    }
}