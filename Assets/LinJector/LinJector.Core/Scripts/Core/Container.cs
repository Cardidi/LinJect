using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinJector.Interface;
using UnityEngine.Pool;

namespace LinJector.Core
{
    /// <summary>
    /// Data Structure which provides immutable object graph and dependency searcher.
    /// </summary>
    public sealed class Container : IDisposable, IAsyncDisposable
    {
        #region StaticMembers

        private static uint ContainerIdAllocator = 0;
        
        public static Container Root { get; private set; }
        
        #endregion

        private uint _id;
        
        private Container _parent;

        private List<Container> _children;
        
        private Dictionary<Type, Dictionary<object, IList<ILifetimeResolver>>> _innerMap;

        #region PrivateAPI

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
                dict.TryGetValue(type, out var list))
            {
                return list;
            }

            return Enumerable.Empty<ILifetimeResolver>();
        }
        
        private void DisposedTest()
        {
            if (IsDisposed()) 
                throw new InvalidOperationException("Do not trying to access an Disposed container!");
        }

        #endregion

        #region InternalAPI

        internal bool IsDisposed() => _innerMap == null;

        internal bool TakeResolver(Type type, object id, out ILifetimeResolver target)
        {
            if (id == null) target = Resolvable(type).FirstOrDefault();
            else target = Resolvable(type, id).FirstOrDefault();
            
            return target != null;
        }

        internal uint TakeResolvers(Type type, object id, List<ILifetimeResolver> targets)
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

        #endregion

        #region PublicAPI

        public uint Id
        {
            get
            {
                DisposedTest();
                return _id;
            }
        }
        
        public object Resolve(Type type, object id = null)
        {         
            DisposedTest();

            if (TakeResolver(type, id, out var target)) return target.Resolve(this);
            return default(object);
        }
        
        public object[] ResolveAll(Type type, object id = null)
        {
            DisposedTest();

            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return Array.Empty<object>();
                return rrs.Select(rr => rr.Resolve(this)).ToArray();
            }
        }


        public uint ResolveAll(Type type, object id, List<object> results)
        {
            DisposedTest();

            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return 0;
                results.AddRange(rrs.Select(p => p.Resolve(this)));
                return count;
            }
        }

        public T Resolve<T>(object id = null)
        {
            DisposedTest();

            var type = typeof(T);
            if (TakeResolver(type, id, out var target)) return target.Resolve<T>(this);
            return default(T);
        }
        
        public T[] ResolveAll<T>(object id = null)
        {
            DisposedTest();

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
            DisposedTest();
            
            var type = typeof(T);
            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                var count = TakeResolvers(type, id, rrs);
                if (count == 0) return 0;
                results.AddRange(rrs.Select(p => p.Resolve<T>(this)));
                return count;
            }
        }

        #endregion
        
        internal Container(Container parent)
        {
            _id = ++ContainerIdAllocator;
            _parent = parent;
            _children = ListPool<Container>.Get();
        }

        private void DisposeShared()
        {
            ListPool<Container>.Release(_children);
        }
        
        public void Dispose()
        {
            DisposedTest();
            DisposeShared();
        }

        public async ValueTask DisposeAsync()
        {
            DisposedTest();
            DisposeShared();
        }
    }
}