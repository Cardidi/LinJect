using System;
using System.Collections.Generic;
using System.Linq;
using LinJector.Core.Reflection;
using LinJector.Interface;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace LinJector.Core
{
    public static class Injection
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Initialize()
        {
            Container.OnContainerHasCreated += c =>
            {
                _injectionInfo.Add(c.Id, DictionaryPool<Type, ObjectInjectionMap>.Get());
            };
            
            Container.OnContainerWillDispose += c =>
            {
                if (_injectionInfo.TryGetValue(c.Id, out var dict))
                {
                    DictionaryPool<Type, ObjectInjectionMap>.Release(dict);
                    _injectionInfo.Remove(c.Id);
                }
            };
        }

        private static Dictionary<uint, Dictionary<Type, ObjectInjectionMap>> _injectionInfo = new();

        private static HashSet<object> _loopDetect = new();

        internal static void InjectValuesOnly(object target, ObjectInjectionMap map, Dictionary<Type, object> alterMap)
        {
            foreach (var iv in map.Structure.Values)
            {
                if (alterMap != null && alterMap.TryGetValue(iv.RequestedType, out var v))
                {
                    iv.SetData(target, v);
                    continue;
                }
                
                if (map.TryGetCachedResolver(iv.Name, out var resolver))
                    iv.SetData(target, ResolveSafe(resolver, map.ContextContainer));
            }
        }
        
        internal static void InjectObjectFromMap(object target, ObjectInjectionMap map)
        {
            var c = map.ContextContainer;
            using (ListPool<ObjectInjectionMap.ArgumentProvider>.Get(out var args))
            {
                // Run universe injection or method injection.
                if (map.TryGetNonCtorInjectionMethod(out var method, args))
                {
                    var a = args.Select(ap => ap.IsResolver
                        ? ResolveSafe((ILifetimeResolver) ap.Existed, c) 
                        : ap.Existed).ToArray();
                    
                    method.Invoke(target, a);
                }
            }
        }
                
        public static ObjectInjectionMap GetCachedInjectionMap(Container container, Type target)
        {
            if (_injectionInfo.TryGetValue(container.Id, out var typeDict))
            {
                if (!typeDict.TryGetValue(target, out var map))
                {
                    map = new ObjectInjectionMap(target, container);
                    typeDict.Add(target, map);
                }

                return map;
            }

            throw LinJectErrors.ContainerNotReady();
        }

        public static object ResolveArgumentSafe(ObjectInjectionMap.ArgumentProvider ap, Container c)
            => ap.IsResolver ? ResolveSafe((ILifetimeResolver)ap.Existed, c) : ap.Existed;

        public static object ResolveSafe(ILifetimeResolver resolver, Container container)
        {
            if (!_loopDetect.Add(resolver)) throw LinJectErrors.LoopedDependencyChainDetected();
            
            try
            {
                var result = resolver.Resolve(container);
                return result;
            }
            finally
            {
                _loopDetect.Remove(resolver);
            }
        }

        public static void Inject(this Container container, object injected)
        {
            Assert.IsNotNull(injected);
            Assert.IsNotNull(container);
            var type = injected.GetType();

            var map = GetCachedInjectionMap(container, type);
            InjectValuesOnly(injected, map, null);
            InjectObjectFromMap(injected, map);
        }

        public static void InjectExplicit(this Container container, object injected, params object[] extraBindings)
        {
            // Fallback
            if (extraBindings.Length == 0)
            {
                Inject(container, injected);
                return;
            }
            
            Assert.IsNotNull(injected);
            Assert.IsNotNull(container);
            var type = injected.GetType();

            var map = GetCachedInjectionMap(container, type);
            using (DictionaryPool<Type, object>.Get(out var alter))
            {
                foreach (var o in extraBindings)
                {
                    if (o == null) continue;
                    alter.Add(o.GetType(), o);
                }

                InjectValuesOnly(injected, map, null);
                InjectObjectFromMap(injected, map);
            }
        }

        public static void InjectGameObject(this Container container, GameObject gameObject)
        {
            Assert.IsNotNull(gameObject);
            Assert.IsNotNull(container);

            using (ListPool<MonoBehaviour>.Get(out var monos))
            {
                gameObject.GetComponentsInChildren(monos);
                foreach (var obj in monos)
                {
                    var map = GetCachedInjectionMap(container, obj.GetType());
                    InjectValuesOnly(obj, map, null);
                    InjectObjectFromMap(obj, map);
                }
            }
        }
        
        public static object NewObject(this Container container, Type type, params object[] args)
        {
            var map = GetCachedInjectionMap(container, type);
            using(ListPool<ObjectInjectionMap.ArgumentProvider>.Get(out var argps))
            {
                argps.AddRange(args.Select(a => new ObjectInjectionMap.ArgumentProvider(a)));   
                
                map.GetConstructor(out var ctor, argps);
                var objActivator = LinJectUtility.GetRuntimeActivatorFactory()
                    .MakeActivator(type, ctor.AsConstructorInfo);

                var obj = objActivator.Invoke(obj =>
                {
                    InjectValuesOnly(obj, map, null);
                }, argps.Select(p => ResolveArgumentSafe(p, container)).ToArray());
                
                using (ListPool<ObjectInjectionMap.ArgumentProvider>.Get(out var arg))
                {
                    if (map.TryGetNonCtorInjectionMethod(out var postInject, arg))
                        postInject.Invoke(obj, arg.Select(p => ResolveArgumentSafe(p, container)).ToArray());
                }

                return obj;
            }
        }


        public static T NewObject<T>(this Container container, params object[] args)
        {
            return (T) NewObject(container, typeof(T), args);
        }
    }
}