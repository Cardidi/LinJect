using LinJector.Core.Resolvers.Base;

namespace LinJector.Core.Resolvers
{
    public class FromInstanceSingletonResolver : SingletonResolver
    {
        private object _instance;

        public FromInstanceSingletonResolver(object instance)
        {
            _instance = instance;
        }
        
        public override object Resolve(Container container)
        {
            return _instance;
        }

        public override T Resolve<T>(Container container)
        {
            if (_instance is T r) return r;
            throw LinJectErrors.TypedResolverNotMatch();
        }
    }
}