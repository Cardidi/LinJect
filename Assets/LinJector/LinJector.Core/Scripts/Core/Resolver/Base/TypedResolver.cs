using System;
using System.Collections.Generic;
using System.Linq;
using LinJector.Core.Reflection;
using LinJector.Interface;
using UnityEngine.Pool;

namespace LinJector.Core.Resolver.Base
{
    public struct TypedResolver : IResolver
    {
        private ObjectInjectionMap _map;

        private List<ObjectInjectionMap.ArgumentProvider> _args;

        private IActivatorFactory.ObjectActivator _activator;

        private object ActivateObject()
        {
            var m = _map;
            var c = _map.ContextContainer;
            var a = _args.Count == 0
                ? Array.Empty<object>()
                : _args.Select(p => Injection.ResolveArgumentSafe(p, c)).ToArray();
            
            // Create new object
            var obj = _activator.Invoke(obj =>
            {
                Injection.InjectValuesOnly(obj, m, null);
            }, a);

            // Try invoke injection method.
            using (ListPool<ObjectInjectionMap.ArgumentProvider>.Get(out var arg))
            {
                if (_map.TryGetNonCtorInjectionMethod(out var postInject, arg))
                    postInject.Invoke(obj, arg.Select(p => Injection.ResolveArgumentSafe(p, c)).ToArray());
            }

            return obj;
        }
        
        public TypedResolver(Container container, Type type, object[] arguments)
        {
            _map = Injection.GetCachedInjectionMap(container, type);
            _args = arguments.Select(p => new ObjectInjectionMap.ArgumentProvider(p)).ToList();
            
            _map.GetConstructor(out var ctor, _args);
            _activator = LinJectUtility.GetRuntimeActivatorFactory().MakeActivator(type, ctor.AsConstructorInfo);
        }
        
        public object Resolve(Container container)
        {
            return ActivateObject();
        }

        public T Resolve<T>(Container container)
        {
            return (T) ActivateObject();
        }
    }
}