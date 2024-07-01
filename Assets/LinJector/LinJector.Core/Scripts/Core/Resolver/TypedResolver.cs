using System;
using System.Linq;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public struct TypedResolver : IResolver
    {

        internal static TypedResolver Get(Type type)
        {
            return new TypedResolver(type);
        }

        internal static TypedResolver Get(Type type, object[] arguments)
        {
            return new TypedResolver(type, arguments);
        }
        
        private readonly IActivatorFactory.ObjectActivator _runtimeActivator;

        private readonly object[] _arguments;
        
        private TypedResolver(Type type)
        {
            if (type == null) throw LinJectErrors.TypedResolverCanNotActivate();
            _arguments = Array.Empty<object>();
            _runtimeActivator = LinJectUtility.GetRuntimeActivatorFactory().MakeDefaultActivator(type);
        }

        private TypedResolver(Type type, object[] arguments)
        {
            if (type == null) throw LinJectErrors.TypedResolverCanNotActivate();
            
            // Get constructor
            var parameterTypes = arguments.Select(p => p?.GetType()).ToArray();
            var constructor = type.GetConstructor(parameterTypes);
            
            // Set activation data
            _arguments = arguments;
            _runtimeActivator = LinJectUtility.GetRuntimeActivatorFactory()
                .MakeActivator(type, constructor, parameterTypes);
        }

        public object Resolve(Container container)
        {
            if (_runtimeActivator == null) throw LinJectErrors.TypedResolverCanNotActivate();
            return _runtimeActivator.Invoke(obj =>
            {
                // todo: made pre-injection done.
                // todo: Make sure the container for activator should matched with the container who holds this.
            }, _arguments);
        }

        public T Resolve<T>(Container container)
        {
            if (_runtimeActivator == null) throw LinJectErrors.TypedResolverCanNotActivate();
            return (T) _runtimeActivator.Invoke(obj =>
            {
                // todo: made pre-injection done.
            }, _arguments);
        }
    }
}