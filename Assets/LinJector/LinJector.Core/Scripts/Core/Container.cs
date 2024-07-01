using System;
using System.Collections.Generic;
using System.Linq;
using Ca2d.Toolkit;
using LinJector.Interface;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

namespace LinJector.Core
{
    /// <summary>
    /// Data Structure which provides immutable object graph and dependency searcher.
    /// </summary>
    public sealed class Container : IDisposable, IServiceProvider
    {
        #region StaticMembers

        private static uint ContainerIdAllocator = 1;

        public delegate void ContainerBuilderCallback(Container parent, ContainerBuilder builder);

        public static Container SuperEmpty { get; } = new();

        public static Container Create(ContainerBuilderCallback build, bool autoInit = true)
        {
            return SuperEmpty.CreateChild(build, autoInit);
        }

        public static event Action<Container> OnContainerHasCreated; 
        
        public static event Action<Container> OnContainerWillDispose;
        
        #endregion

        private uint _id;

        private bool _initialized;
        
        private Container _parent;

        private List<Container> _children;

        private LifetimeEventRegistry _eventRegistry;
        
        private Dictionary<Type, Dictionary<object, List<ILifetimeResolver>>> _innerMap;

        #region PrivateAPI

        private IEnumerable<ILifetimeResolver> Resolvable()
        {
            return _innerMap.Values.SelectMany(kp => kp.Values).SelectMany(p => p);
        }

        private IEnumerable<ILifetimeResolver> Resolvable(Type type)
        {
            if (_innerMap.TryGetValue(type, out var dict))
            {
                return dict.SelectMany(i => i.Value);
            }

            return Enumerable.Empty<ILifetimeResolver>();
        }

        private IEnumerable<ILifetimeResolver> Resolvable(Type type, object id)
        {
            if (_innerMap.TryGetValue(type, out var dict) && 
                dict.TryGetValue(id, out var list))
            {
                return list;
            }

            return Enumerable.Empty<ILifetimeResolver>();
        }


        private void SelfAvailableTest(bool requireInit = true)
        {
            if (requireInit && !_initialized)
                throw new InvalidOperationException("Do not trying to access an uninitalized container!");
            if (IsDisposed) throw new InvalidOperationException("Do not trying to access an Disposed container!");
        }

        #endregion

        #region InternalAPI
        
        internal bool TakeResolver(Type type, object id, out ILifetimeResolver target)
        {
            if (id == null) target = Resolvable(type).FirstOrDefault();
            else target = Resolvable(type, id).FirstOrDefault();
            
            return target != null;
        }

        internal uint TakeResolvers(Type type, object id, ICollection<ILifetimeResolver> targets)
        {
            IEnumerable<ILifetimeResolver> iter;
            if (id == null) iter = Resolvable(type);
            else iter = Resolvable(type, id);

            uint count = 0;
            foreach (var r in iter)
            {
                targets.Add(r);
                count++;
            }

            return count;
        }

        internal uint TakeAllResolvers(ICollection<ILifetimeResolver> targets)
        {
            uint count = 0;
            foreach (var r in Resolvable())
            {
                targets.Add(r);
                count++;
            }

            return count;
        }

        internal uint TakeAllRegisteredType(ICollection<Type> types)
        {
            uint count = 0;
            foreach (var r in _innerMap)
            {
                types.Add(r.Key);
                count++;
            }

            return count;
        }

        private HashSet<Type> _registeredTypes;
        
        internal HashSet<Type> RegisteredTypes
        {
            get
            {
                if (_registeredTypes == null)
                {
                    _registeredTypes = HashSetPool<Type>.Get();
                    TakeAllRegisteredType(_registeredTypes);
                }

                return _registeredTypes;
            }
        }

        #endregion

        #region PublicAPI
        
        public bool IsDisposed => _innerMap == null;
        
        public uint Id => _id;
        
        public object Resolve(Type type, object id = null)
        {         
            SelfAvailableTest();

            if (TakeResolver(type, id, out var target)) return target.Resolve(this);
            return default(object);
        }


        public bool TryResolve(out object result, Type type, object id = null)
        {
            SelfAvailableTest();

            if (TakeResolver(type, id, out var target))
            {
                result = target.Resolve(this);
                return true;
            }

            result = null;
            return false;
        }
        
        public object[] ResolveAll(Type type, object id = null)
        {
            SelfAvailableTest();

            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return Array.Empty<object>();
                return rrs.Select(rr => rr.Resolve(this)).ToArray();
            }
        }
        
        public uint ResolveAll(Type type, List<object> results, object id = null)
        {
            SelfAvailableTest();

            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return 0;
                results.AddRange(rrs.Select(p => p.Resolve(this)));
                return count;
            }
        }
        
        public T Resolve<T>(object id = null) where T : class
        {
            SelfAvailableTest();

            var type = typeof(T);
            if (TakeResolver(type, id, out var target)) return target.Resolve<T>(this);
            return default(T);
        }
        
        public T Resolve<T>(T fallback, object id = null) where T : struct
        {
            SelfAvailableTest();

            var type = typeof(T);
            if (TakeResolver(type, id, out var target)) return target.Resolve<T>(this);
            return fallback;
        }
        
        
        public bool TryResolve<T>(out T result, object id = null)
        {
            SelfAvailableTest();

            var type = typeof(T);
            if (TakeResolver(type, id, out var target))
            {
                result = target.Resolve<T>(this);
                return true;
            }

            result = default;
            return false;
        }
        
        public T[] ResolveAll<T>(object id = null)
        {
            SelfAvailableTest();

            var type = typeof(T);
            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return Array.Empty<T>();
                return rrs.Select(rr => rr.Resolve<T>(this)).ToArray();
            }
        }

        public uint ResolveAll<T>(object id, List<T> results)
        {
            SelfAvailableTest();
            
            var type = typeof(T);
            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return 0;
                results.AddRange(rrs.Select(p => p.Resolve<T>(this)));
                return count;
            }
        }

        public Container CreateChild(ContainerBuilderCallback build, bool autoInit = true)
        {
            SelfAvailableTest();
            Assert.IsNotNull(build);

            using (GenericPool<ContainerBuilder>.Get(out var builder))
            {
                Container c;
                try
                {
                    builder.Prepare(this);
                    build.Invoke(this, builder);
                    c = new Container(this, builder);
                }
                finally
                {
                    builder.Reset();
                }
                
                _children.Add(c);
                if (autoInit) c.Initialize();

                return c;
            }
        }
        
        #endregion

        #region LifecircleControlPrivate

        private void DoPreInitialization()
        {
            foreach (var rr in Resolvable().OfType<IConsiderPreInitializeResolver>())
            {
                rr.PreInitialize(this);
            }
        }
        
        private void DoPostInitialization()
        {
            foreach (var rr in Resolvable().OfType<IConsiderPostInitializeResolver>())
            {
                rr.PostInitialize(this);
            }
        }

        #endregion

        #region LifecircleControl

        /// <summary>
        /// Generate a container with elements on this.
        /// </summary>
        private Container(Container parent, ContainerBuilder builder)
        {
            _id = ContainerIdAllocator++;
            _parent = parent;
            _initialized = false;
            _children = ListPool<Container>.Get();
#if UNITY_EDITOR
            if (Application.isPlaying){
#endif
            _eventRegistry = GenericPool<LifetimeEventRegistry>.Get();
            _eventRegistry.BindContainer(this);
#if UNITY_EDITOR
            }
#endif
            _innerMap = ContainerBuilder.Get();
            builder.Generate(_innerMap, parent._innerMap);
        }

        /// <summary>
        /// Generate a super-empty container
        /// </summary>
        private Container()
        {
            _id = 0;
            _parent = null;
            _initialized = true;
            _children = new List<Container>();
            _eventRegistry = null;
            _innerMap = new(); // Super-empty will get NO resolvers.
        }

        public void Initialize()
        {
            if (_initialized || _id == 0) return;
            SelfAvailableTest(false);
            
            DoPreInitialization();
            
#if UNITY_EDITOR
            if (Application.isPlaying){
#endif
            _eventRegistry.Initialize();
#if UNITY_EDITOR
            }
#endif

            _initialized = true;
            if (OnContainerHasCreated != null) Guard.IgnoreException(OnContainerHasCreated, this);
            DoPostInitialization();
        }

        public void Dispose()
        {
            if (_id == 0) throw LinJectErrors.SuperEmptyContainerDidNotAllowThisOperation();
            SelfAvailableTest(false);
            
            if (OnContainerWillDispose != null) Guard.IgnoreException(OnContainerWillDispose, this);
            
            // Dispose all children first.
            using (ListPool<Container>.Get(out var chs))
            {
                chs.AddRange(_children);
                foreach (var c in chs)
                {
                    if (c.IsDisposed) continue;
                    c.Dispose();
                }
            }

            // Do runtime disposal
            _parent._children.Remove(this);
            
#if UNITY_EDITOR
            if (Application.isPlaying){
#endif
            _eventRegistry.Dispose();
#if UNITY_EDITOR
            }
#endif
            // Return all objects
            // DicPool and ListPool will clear all element when release an object.
            ContainerBuilder.Release(_innerMap);
            ListPool<Container>.Release(_children);
            if (_registeredTypes != null) HashSetPool<Type>.Release(_registeredTypes);
#if UNITY_EDITOR
            if (Application.isPlaying){
#endif
            GenericPool<LifetimeEventRegistry>.Release(_eventRegistry);
#if UNITY_EDITOR
            }
#endif
            
            // Unlink all objects for GC
            _innerMap = null;
            _children = null;
            _eventRegistry = null;
            _parent = null;
        }

        #endregion

        #region Compability

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        #endregion
    }
}