using System;
using System.Collections.Generic;
using System.Linq;
using LinJector.Interface;
using UnityEngine.Pool;

namespace LinJector.Core.Reflection
{
    /// <summary>
    /// Object injection information for next-step dependency resolver to get the right generate or inject path.
    /// </summary>
    public sealed class ObjectInjectionMap : IDisposable
    {
        public struct ArgumentProvider
        {
            public readonly object Existed;

            public readonly bool IsResolver;

            public ArgumentProvider(object obj)
            {
                IsResolver = false;
                Existed = obj;
            }

            public ArgumentProvider(object obj, bool isResolver)
            {
                IsResolver = isResolver;
                Existed = obj;
            }
        }
        
        public ObjectInjectionMap(Type objectType, Container context)
        {
            ContextContainer = context;
            Structure = ObjectReflectionStructureMap.Analyse(objectType);
            _rrCaches = DictionaryPool<string, ILifetimeResolver>.Get();

            // Start injection resolving...
            foreach (var v in Structure.Values)
            {
                if (context.TakeResolver(v.RequestedType, v.Id, out var rr))
                {
                    // Found a proper resolver
                    _rrCaches.Add(v.Name, rr);
                }
                else if (!v.IsOptional)
                {
                    // Object graph did not satisfied with injection requirements.
                    throw LinJectErrors.DependencyUnsatisfied();
                }
            }
            
            // Cache for non-ctor injection method
            _nonConstructorInjector = Structure.SearchInjectionMethod(ContextContainer.RegisteredTypes);
            if (_nonConstructorInjector != null)
            {
                _nonConstructorInjectorArguments = ListPool<ArgumentProvider>.Get();
                GetUnsatisfiedMethodArguments(_nonConstructorInjector, _nonConstructorInjectorArguments);
            }
        }
        
        
        public void Dispose()
        {
            DictionaryPool<string, ILifetimeResolver>.Release(_rrCaches);
            if (_nonConstructorInjectorArguments != null)
                ListPool<ArgumentProvider>.Release(_nonConstructorInjectorArguments);
        }
        
        /// <summary>
        /// Which container this graph valid for?
        /// </summary>
        public Container ContextContainer { get; }
        
        /// <summary>
        /// The actual structure of this object.
        /// </summary>
        public ObjectReflectionStructureMap Structure { get; }

        private Dictionary<string, ILifetimeResolver> _rrCaches;

        private InjectiveMethodBase _nonConstructorInjector;

        private List<ArgumentProvider> _nonConstructorInjectorArguments;

        public void GetUnsatisfiedMethodArguments(
            InjectiveMethodBase methodData, IList<ArgumentProvider> providers)
        {
            var p = methodData.Parameters;
            for (int i = 0; i < p.Length; i++)
            {
                var param = p[i];
                ArgumentProvider arg = default;
                
                var isOutOfBounds = i >= providers.Count;
                bool insertNew = false;

                void TryResolve(ref ArgumentProvider ap)
                {
                    if (ContextContainer.TakeResolver(param.RequestedType, param.Id, out var rr))
                    {
                        ap = new ArgumentProvider(rr, true);
                        return;
                    }
                    
                    if (param.IsInjectionOptional || param.IsParameterOptional)
                    {
                        ap = new ArgumentProvider(param.IsParameterOptional ? Type.Missing : null);
                        return;
                    }
                    
                    throw LinJectErrors.DependencyUnsatisfied();
                }
        
                if (isOutOfBounds)
                {
                    TryResolve(ref arg);
                }
                else
                {
                    var refArg = providers[i];
                    if (refArg.Existed == null)
                    {
                        // Arg is not existed
                        if (param.Injective) TryResolve(ref arg);
                        else arg = refArg;
                    }
                    else
                    {
                        // Arg is existed
                        var argType = refArg.Existed.GetType();
                        if (param.RequestedType.IsAssignableFrom(argType))
                        {
                            // Parameter and argument are matched;
                            arg = refArg;
                        }
                        else
                        {
                            // Not Matched: May position wrong...
                            // So consider inject this position.
                            insertNew = true;
                            TryResolve(ref arg);
                        }
                    }
                }
                
                if (isOutOfBounds) providers.Add(arg);
                else if (insertNew) providers.Insert(i, arg);
                else providers[i] = arg;
            }
        }
        
        /// <summary>
        /// Try to get a resolver for this injection map.
        /// </summary>
        public bool TryGetCachedResolver(string fieldName, out ILifetimeResolver resolver)
        {
            return _rrCaches.TryGetValue(fieldName, out resolver);
        }

        /// <summary>
        /// Try read the injection target which is beyonds of constructor.
        /// </summary>
        public bool TryGetNonCtorInjectionMethod(out InjectiveMethodBase method, IList<ArgumentProvider> args)
        {
            if (_nonConstructorInjector == null)
            {
                method = null;
                return false;
            }
            
            method = _nonConstructorInjector;
            foreach (var a in _nonConstructorInjectorArguments)
                args.Add(a);
            
            return true;
        }

        /// <summary>
        /// Try read the constructor with best selection
        /// </summary>
        public void GetConstructor(out InjectiveMethodBase method, IList<ArgumentProvider> args)
        {
            var ctor = Structure.SearchConstructor(
                _nonConstructorInjector != null,
                args.Select(p => p.Existed).ToArray());

            method = ctor;
            GetUnsatisfiedMethodArguments(ctor, args);
        }
    }
}