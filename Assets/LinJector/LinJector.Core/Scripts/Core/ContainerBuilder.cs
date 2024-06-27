using System;
using System.Collections.Generic;
using System.Linq;
using Ca2d.Toolkit;
using LinJector.Core.Binder;
using LinJector.Enum;
using LinJector.Interface;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using ObjectGraph = System.Collections.Generic.Dictionary<
    System.Type, 
    System.Collections.Generic.Dictionary<
        object, 
        System.Collections.Generic.List<LinJector.Interface.ILifetimeResolver>>>;

namespace LinJector.Core
{
    public sealed class ContainerBuilder
    {
        private struct BindingKey
        {
            public Type Source;

            public object Id;

            public ResolverBinder Resolver;
        }
        
        private struct BindingResult
        {
            public Type Source;

            public object Id;

            public bool IsParent;

            public ILifetimeResolver Resolver;
        }
        
        #region ResManager

        internal static ObjectGraph Get()
        {
            return DictionaryPool<Type, Dictionary<object, List<ILifetimeResolver>>>.Get();
        }
        
        internal static void Release(ObjectGraph innerMap)
        {
            foreach (var typeMapValue in innerMap.Values)
            {
                foreach (var idMapValue in typeMapValue.Values)
                    ListPool<ILifetimeResolver>.Release(idMapValue);
                DictionaryPool<object, List<ILifetimeResolver>>.Release(typeMapValue);
            }
            
            DictionaryPool<Type, Dictionary<object, List<ILifetimeResolver>>>.Release(innerMap);
        }

        #endregion

        private bool _ready = false;

        internal bool Ready
        {
            get => _ready;
            private set => _ready = value;
        }

        public Container Parent { get; private set; }

        private void Cleanup()
        {
            Parent = null;
            _typeBinders.ForEach(GenericPool<TypeBinder>.Release);
            _resolverBinders.ForEach(GenericPool<ResolverBinder>.Release);
            _aliasBinders.ForEach(GenericPool<AliasBinder>.Release);
            
            _typeBinders.Clear();
            _resolverBinders.Clear();
            _aliasBinders.Clear();
        }
        
        internal void Prepare(Container parent)
        {
            Assert.IsNotNull(parent);
            if (Ready) throw LinJectErrors.ContainerBuilderStateInvalid();
            Parent = parent;
            Ready = true;
        }

        internal void Reset()
        {
            Cleanup();
            Ready = false;
        }

        internal bool Validate()
        {
            if (Ready == false) throw LinJectErrors.ContainerBuilderStateInvalid();

            try
            {
                using (ListPool<BindingKey>.Get(out var ks))
                    CollectBindings(ks);
            }
            catch (Exception e)
            {
                DebugLogg.Error(e);
                return false;
            }

            return true;
        }
        
        internal void Generate(ObjectGraph targetMap, ObjectGraph parentMap)
        {
            if (Ready == false) throw LinJectErrors.ContainerBuilderStateInvalid();
            Assert.IsNotNull(targetMap);
            Assert.IsNotNull(parentMap);

            using (ListPool<BindingResult>.Get(out var results))
            using(DictionaryPool<ILifetimeResolver, ILifetimeResolver>.Get(out var rrCache))
            {
                // Get all bindings
                using (ListPool<BindingKey>.Get(out var foreheads))
                {
                    CollectBindings(foreheads);
                    ActivateBindings(foreheads, results);
                }
                
                // Copy previous bindings
                foreach (var type2Area in parentMap)
                {
                    var nb = new BindingResult
                    {
                        Source = type2Area.Key,
                        IsParent = true
                    };
                    
                    foreach (var id2Resolver in type2Area.Value)
                    {
                        nb.Id = id2Resolver.Key;
                        foreach (var rr in id2Resolver.Value)
                        {
                            if (!rrCache.TryGetValue(rr, out var nrr))
                            {
                                rr.Duplicate(out nrr);
                                rrCache.Add(rr, nrr);
                            }

                            nb.Resolver = nrr;
                            results.Add(nb);
                        }
                    }
                }
                
                // We must sort Id = null as first-class due to we are trying to make empty Id as first selected resolver.
                results.Sort((l, r) =>
                {
                    var val = 0;
                    
                    // Left guy
                    if (l.Id == NullKey.Get) val--;
                    if (l.IsParent) val++;
                    
                    // Right guy -> reverse of Left guy
                    if (r.Id == NullKey.Get) val++;
                    if (r.IsParent) val--;
                    
                    return val;
                });
                
                // Move them into new container.
                foreach (var b in results)
                {
                    // Generate actually resolver first
                    var target = b.Resolver;
                    
                    // Find insert area
                    if (!targetMap.TryGetValue(b.Source, out var typeArea))
                    {
                        typeArea = DictionaryPool<object, List<ILifetimeResolver>>.Get();
                        targetMap.Add(b.Source, typeArea);
                    }

                    object id = b.Id;
                    if (!typeArea.TryGetValue(id, out var rrs))
                    {
                        rrs = ListPool<ILifetimeResolver>.Get();
                        typeArea.Add(id, rrs);
                    }
                    
                    // Insert
                    rrs.Add(target);
                }
            }
        }

        private void CollectBindings(List<BindingKey> keys)
        {
            // If any binder is not valid?
            if (_resolverBinders.Any(p => !p.Validate()) ||
                _typeBinders.Any(p => !p.Validate()) ||
                _aliasBinders.Any(p => !p.Validate()))
                throw new InvalidProgramException("May contains an invalid buiding target!");
            
            using (DictionaryPool<Type, Dictionary<object, List<ResolverBinder>>>.Get(out var cache))
            {
                try
                {
                    // Create basic type binders.
                    foreach (var tb in _typeBinders)
                    {
                        if (!cache.TryGetValue(tb.From, out var dict))
                        {
                            dict = DictionaryPool<object, List<ResolverBinder>>.Get();
                            cache.Add(tb.From, dict);
                        }

                        var id = tb.Id ?? NullKey.Get;
                        if (!dict.TryGetValue(id, out var rrs))
                        {
                            rrs = ListPool<ResolverBinder>.Get();
                            dict.Add(id, rrs);
                        }
                        
                        rrs.Add(tb.Resolver);
                    }
                    
                    // Create alias binders
                    using (ListPool<AliasBinder>.Get(out var unsetBinders))
                    using (HashSetPool<AliasBinder>.Get(out var turns))
                    {
                        unsetBinders.AddRange(_aliasBinders);
                        
                        while (unsetBinders.Count > 0) // If not all binders has found target...
                        {
                            turns.Clear();

                            foreach (var ab in unsetBinders)
                            {
                                if (cache.TryGetValue(ab.To, out var tk) && 
                                    tk.TryGetValue(ab.ToId ?? NullKey.Get, out var orrs))
                                {
                                    if (!cache.TryGetValue(ab.From, out var dict))
                                    {
                                        dict = DictionaryPool<object, List<ResolverBinder>>.Get();
                                        cache.Add(ab.From, dict);
                                    }

                                    var id = ab.Id ?? NullKey.Get;
                                    if (!dict.TryGetValue(id, out var rrs))
                                    {
                                        rrs = ListPool<ResolverBinder>.Get();
                                        dict.Add(id, rrs);
                                    }
                        
                                    rrs.AddRange(orrs);
                                    turns.Add(ab);
                                }
                            }
                                
                            if (turns.Count == 0)
                                throw new InvalidProgramException("Can not found any alias binder can be fit!");

                            unsetBinders.RemoveAll(a => turns.Contains(a));
                        }

                    }
                    
                    // Copy Results
                    var gen = default(BindingKey);
                    foreach (var ta in cache)
                    {
                        gen.Source = ta.Key;
                        foreach (var aaMap in ta.Value)
                        {
                            gen.Id = aaMap.Key;
                            foreach (var r in aaMap.Value)
                            {
                                gen.Resolver = r;
                                keys.Add(gen);
                            }
                        }
                    }
                }
                finally
                {
                    foreach (var sub in cache.Values)
                    {
                        foreach (var rl in sub)
                            ListPool<ResolverBinder>.Release(rl.Value);
                        DictionaryPool<object, List<ResolverBinder>>.Release(sub);
                    }
                }
            }
        }

        private void ActivateBindings(List<BindingKey> keys, List<BindingResult> activates)
        {
            using (DictionaryPool<ResolverBinder, ILifetimeResolver>.Get(out var bind))
            {
                foreach (var k in keys)
                {
                    // Read cache of resolver to make sure only-one mapping from binds to resolvers.
                    if (!bind.TryGetValue(k.Resolver, out var r))
                    {
                        r = k.Resolver.Get()();
                        bind.Add(k.Resolver, r);
                    }
                    
                    activates.Add(new BindingResult
                    {
                        Source = k.Source,
                        Id = k.Id,
                        Resolver = r,
                        IsParent = false
                    });
                }
            }
            
        }

        #region GraphGenerator

        private List<ResolverBinder> _resolverBinders = new();

        private List<TypeBinder> _typeBinders = new();

        private List<AliasBinder> _aliasBinders = new();

        public ResolverBinder CreateResolverBinder(Lifetime lifetime = Lifetime.Transient)
        {
            if (Ready == false) throw LinJectErrors.ContainerBuilderStateInvalid();
            
            var binder = GenericPool<ResolverBinder>.Get();
            binder.MakeReady();
            binder.Lifetime = lifetime;
            
            _resolverBinders.Add(binder);
            return binder;
        }

        public TypeBinder CreateTypeBinder(Type from, ResolverBinder to)
        {
            if (Ready == false) throw LinJectErrors.ContainerBuilderStateInvalid();

            var binder = GenericPool<TypeBinder>.Get();
            binder.MakeReady();
            binder.From = from;
            binder.Resolver = to;
            
            _typeBinders.Add(binder);
            return binder;
        }
        
        public AliasBinder CreateAliasBinder(Type from, Type to)
        {
            if (Ready == false) throw LinJectErrors.ContainerBuilderStateInvalid();

            var binder = GenericPool<AliasBinder>.Get();
            binder.MakeReady();
            binder.From = from;
            binder.To = to;
            
            _aliasBinders.Add(binder);
            return binder;
        }
        
        #endregion
    }
}