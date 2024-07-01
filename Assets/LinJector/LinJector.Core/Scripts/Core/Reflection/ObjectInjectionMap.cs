using System;
using System.Collections.Generic;
using LinJector.Interface;
using UnityEngine.Pool;

namespace LinJector.Core.Reflection
{
    /// <summary>
    /// Object injection information for next-step dependency resolver to get the right generate or inject path.
    /// </summary>
    public sealed class ObjectInjectionMap
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
            _rrCaches = new();

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
            _nonConstructorInjector = Structure.SearchBestInjectionMethod(ContextContainer.RegisteredTypes);
            if (_nonConstructorInjector != null)
            {
            }
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

        private ArgumentProvider[] _nonConstructorInjectorArguments;

        // private void GetUnsatisfiedMethodArguments(
        //     InjectiveMethodBase methodData, IList<ArgumentProvider> providers)
        // {
        //     var p = methodData.Parameters;
        //     for (int i = 0; i < p.Length; i++)
        //     {
        //         var pProv = p[i];
        //         ArgumentProvider arg = default;
        //         
        //         var isOutOfBounds = i >= providers.Count;
        //         bool insertNew = false;
        //
        //         if (isOutOfBounds)
        //         {
        //             if (pProv.Injective)
        //             {
        //                 
        //             }
        //         }
        //         
        //
        //         if (isOutOfBounds) providers.Add(arg);
        //         else if (insertNew) providers.Insert(i, arg);
        //         else providers[i] = arg;
        //     }
        // }
        
        /// <summary>
        /// Try to get a resolver for this injection map.
        /// </summary>
        public bool TryGetCachedResolver(string fieldName, out ILifetimeResolver resolver)
        {
            return _rrCaches.TryGetValue(fieldName, out resolver);
        }

        public InjectiveMethodBase GetNonCtorInjectionMethod() => _nonConstructorInjector;
    }
}