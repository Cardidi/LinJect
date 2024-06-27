using System;
using System.Collections.Generic;
using System.Linq;
using LinJector.Enum;

namespace LinJector.Core.Binder
{

    /*
     * Container.Bind<T>()
     * .WithId()
     * .To<T>()/ToMethod/Factory/Instance()/AliasOf();
     * .AsScoped/Singleton()
     * .NonLazy();
     * 
     */
    
    #region BinderReaction

    public interface IBinderNonLazy
    {
        public void NonLazy(bool nonLazy = true);
    }

    public interface IBinderSetLifetime : IBinderNonLazy
    {
        public IBinderNonLazy AsScoped();

        public IBinderNonLazy AsSingleton();

    }

    public interface IBinderSetTo<in TBinderSource> : IBinderSetTo
    {
        
        public IBinderSetLifetime ToSelf();
        
        public IBinderSetLifetime ToSelf(params object[] parameters);

        public IBinderSetLifetime To<T>() where T : TBinderSource;
        
        public IBinderSetLifetime To<T>(params object[] parameters) where T : TBinderSource;
        
        public void AliasOf<T>(object id = null) where T : TBinderSource;
        
        public IBinderNonLazy ToInstance(TBinderSource instance);

        public IBinderSetLifetime ToMethod(Func<Container, TBinderSource> activator);

        public void ToFactory(Func<Container, TBinderSource> activator);
    }

    public interface IBinderSetTo
    {
        public IBinderSetLifetime To(Type type);
        
        public IBinderSetLifetime To(Type type, params object[] parameters);

        public IBinderNonLazy ToInstance(object instance);

        public IBinderSetLifetime ToMethod(Func<Container, object> activator);

        public void ToFactory(Func<Container, object> activator);

        public void AliasOf(Type type, object id = null);
    }
    
    public interface IBinderSetId<in TBinderSource> : IBinderSetTo<TBinderSource>
    {
        public IBinderSetTo<TBinderSource> WithId(object id);
    }
    
    public interface IBinderSetId : IBinderSetTo
    {
        public IBinderSetTo WithId(object id);
    }

    #endregion

    internal struct BinderModificator : IBinderSetLifetime
    {

        private readonly ResolverBinder _target;
        
        public BinderModificator(ResolverBinder target)
        {
            _target = target;
        }
        
        void IBinderNonLazy.NonLazy(bool nonLazy)
        {
            _target.Lazy = nonLazy;
        }

        public IBinderNonLazy AsScoped() => SetLifetime(Lifetime.Scope);

        public IBinderNonLazy AsSingleton() => SetLifetime(Lifetime.Singleton);
        
        private IBinderNonLazy SetLifetime(Lifetime lifetime)
        {
            var b = this;
            b._target.Lifetime = lifetime;
            return b;
        }
    }

    internal struct BinderCreator<TBinderSource> : IBinderSetId<TBinderSource>
    {
        private readonly ContainerBuilder _builder;

        private readonly Type[] _froms;

        private object _id;

        public BinderCreator(ContainerBuilder builder, params Type[] types)
        {
            _builder = builder;
            _froms = types;
            _id = null;
        }

        private BinderModificator GenModificator(ResolverBinder rb)
        {
            var builder = _builder;
            var id = _id;
            foreach (var ft in _froms)
            {
                var tb = builder.CreateTypeBinder(ft, rb);
                tb.Id = id;
            }

            return new BinderModificator(rb);
        }

        public IBinderSetLifetime ToSelf()
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = typeof(TBinderSource);
            return GenModificator(rb);
        }

        public IBinderSetLifetime ToSelf(params object[] parameters)
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = typeof(TBinderSource);
            rb.Parameter = parameters;
            return GenModificator(rb);
        }

        public IBinderSetLifetime To<T>() where T : TBinderSource
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = typeof(T);
            return GenModificator(rb);
        }

        public IBinderSetLifetime To<T>(params object[] parameters) where T : TBinderSource
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = typeof(T);
            rb.Parameter = parameters;
            return GenModificator(rb);
        }

        public IBinderSetLifetime To(Type type)
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = type;
            return GenModificator(rb);
        }

        public IBinderSetLifetime To(Type type, params object[] parameters)
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = type;
            rb.Parameter = parameters;
            return GenModificator(rb);
        }

        public IBinderNonLazy ToInstance(object instance)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Singleton);
            rb.Instance = instance;
            return GenModificator(rb);
        }

        public IBinderSetLifetime ToMethod(Func<Container, object> activator)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Singleton);
            rb.Activator = activator;
            return GenModificator(rb);
        }

        public void ToFactory(Func<Container, object> activator)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Transient);
            rb.Activator = activator;
            GenModificator(rb);
        }

        public IBinderNonLazy ToInstance(TBinderSource instance)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Singleton);
            rb.Instance = instance;
            return GenModificator(rb);
        }

        public IBinderSetLifetime ToMethod(Func<Container, TBinderSource> activator)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Singleton);
            rb.Activator = c => activator(c);
            return GenModificator(rb);
        }

        public void ToFactory(Func<Container, TBinderSource> activator)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Transient);
            rb.Activator = c => activator(c);
            GenModificator(rb);
        }

        public void AliasOf(Type type, object id = null)
        {
            foreach (var ft in _froms)
            {
                var ab = _builder.CreateAliasBinder(ft, type);
                ab.Id = _id;
                ab.ToId = id;
            }
        }

        public void AliasOf<T>(object id = null) where T : TBinderSource
        {
            var type = typeof(T);
            foreach (var ft in _froms)
            {
                var ab = _builder.CreateAliasBinder(ft, type);
                ab.Id = _id;
                ab.ToId = id;
            }
        }

        public IBinderSetTo<TBinderSource> WithId(object id)
        {
            var b = this;
            b._id = id;
            return b;
        }
    }
    
     internal struct BinderCreator : IBinderSetId
    {
        private readonly ContainerBuilder _builder;

        private readonly Type[] _froms;

        private object _id;

        public BinderCreator(ContainerBuilder builder, params Type[] types)
        {
            _builder = builder;
            _froms = types;
            _id = null;
        }

        private BinderModificator GenModificator(ResolverBinder rb)
        {
            var builder = _builder;
            var id = _id;
            foreach (var ft in _froms)
            {
                var tb = builder.CreateTypeBinder(ft, rb);
                tb.Id = id;
            }

            return new BinderModificator(rb);
        }
        public IBinderSetLifetime To(Type type)
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = type;
            return GenModificator(rb);
        }

        public IBinderSetLifetime To(Type type, params object[] parameters)
        {
            var rb = _builder.CreateResolverBinder();
            rb.To = type;
            rb.Parameter = parameters;
            return GenModificator(rb);
        }

        public IBinderNonLazy ToInstance(object instance)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Singleton);
            rb.Instance = instance;
            return GenModificator(rb);
        }

        public IBinderSetLifetime ToMethod(Func<Container, object> activator)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Singleton);
            rb.Activator = activator;
            return GenModificator(rb);
        }

        public void ToFactory(Func<Container, object> activator)
        {
            var rb = _builder.CreateResolverBinder(Lifetime.Transient);
            rb.Activator = activator;
            GenModificator(rb);
        }

        public void AliasOf(Type type, object id = null)
        {
            foreach (var ft in _froms)
            {
                var ab = _builder.CreateAliasBinder(ft, type);
                ab.Id = _id;
                ab.ToId = id;
            }
        }
        
        public IBinderSetTo WithId(object id)
        {
            var b = this;
            b._id = id;
            return b;
        }
    }
    
    public static class BinderExtension
    {
        #region BindStart

        public static IBinderSetId Binds(this ContainerBuilder builder, params Type[] types)
        {
            return new BinderCreator(builder, types);
        }

        public static IBinderSetId Bind(this ContainerBuilder builder, Type type)
        {
            return new BinderCreator(builder, type);
        }
        
        public static IBinderSetId<T> Bind<T>(this ContainerBuilder builder)
        {
            return new BinderCreator<T>(builder, typeof(T));
        }
        
        public static IBinderSetId Bind<T1, T2>(this ContainerBuilder builder)
        {
            return new BinderCreator(builder, typeof(T1), typeof(T2));
        }
        
        public static IBinderSetId Bind<T1, T2, T3>(this ContainerBuilder builder)
        {
            return new BinderCreator(builder, 
                typeof(T1), typeof(T2), typeof(T3));
        }
        
        public static IBinderSetId Bind<T1, T2, T3, T4>(this ContainerBuilder builder)
        {
            return new BinderCreator(builder, 
                typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        }
        
        public static IBinderSetId Bind<T1, T2, T3, T4, T5>(this ContainerBuilder builder)
        {
            return new BinderCreator(builder, 
                typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        public static IBinderSetId<T> BindInterfaces<T>(this ContainerBuilder builder)
        {
            return new BinderCreator<T>(builder, 
                typeof(T).GetInterfaces());
        }
        
        public static IBinderSetId<T> BindInterfacesAndSelf<T>(this ContainerBuilder builder)
        {
            var types = typeof(T).GetInterfaces();
            Array.Resize(ref types, types.Length + 1);
            types[^1] = typeof(T);
            
            return new BinderCreator<T>(builder, types);
        }
        
        #endregion
    }
}