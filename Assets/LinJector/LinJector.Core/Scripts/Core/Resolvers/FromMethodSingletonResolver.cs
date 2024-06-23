using System;

namespace LinJector.Core.Resolvers
{
    public class FromMethodSingletonResolver : SingletonResolver
    {

        private Func<object> _activator;

        private bool _cached;
        
        private object _result;
        
        public FromMethodSingletonResolver(Func<object> activator)
        {
            if (activator == null) throw LinJectErrors.TypedResolverCanNotActivate();
            _activator = activator;
        }
        
        private void MakeResolvable(Container container)
        {
            if (!_cached)
            {
                _cached = true;
                _result = _activator();
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