using System;
using LinJector.Core.Resolver.Base;
using LinJector.Interface;

namespace LinJector.Core.Resolver
{
    public class TypedScopeResolver : ScopeResolver, IConsiderPostInitializeResolver
    {
        private bool _inited;
        
        private TypedResolver _resolver;
        
        private Type _type;

        private bool _cached;
        
        private object _result;
        
        private bool _noLazy;

        
        public TypedScopeResolver(bool noLazy, Type type)
        {
            _type = type;
            _noLazy = noLazy;
        }
        
        private TypedScopeResolver(bool noLazy, Type type, TypedResolver resolver)
        {
            _inited = true;
            _type = type;
            _resolver = resolver;
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

        public override bool Duplicate(out ILifetimeResolver resolver)
        {
            resolver = new TypedScopeResolver(_noLazy, _type, _resolver);
            return true;
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
                _resolver = new TypedResolver(container, _type, Array.Empty<object>());
            }
            
            if (_noLazy) MakeResolvable(container);
        }
    }
}