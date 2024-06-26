using System;
using Ca2d.Toolkit;
using LinJector.Core.Resolver;
using LinJector.Enum;
using LinJector.Interface;

namespace LinJector.Core.Binder
{
    public class ResolverBinder
    {

        public delegate ILifetimeResolver ResolverGenerator();

        /* Object source */
        
        public Lifetime Lifetime;
        
        public Type To;
        
        public Func<Container, object> Activator;

        public object Instance;
        
        /* Additional Info */
        
        public bool Lazy;
        
        public object[] Parameter;

        internal bool Validate()
        {
            try
            {
                Get();
            }
            catch (Exception e)
            {
#if DEBUG
                DebugLogg.Error(e);
#endif
                return false;
            }

            return true;
        }

        internal ResolverGenerator Get()
        {
            switch (Lifetime)
            {
                case Lifetime.Scope:     return GenerateStart(instanceOk: false);
                case Lifetime.Singleton: return GenerateStart();
                case Lifetime.Transient: return GenerateStart(instanceOk: false);
            }

            return GenerateOther();
        }

        #region GeneratorSelector

        private ResolverGenerator GenerateStart(bool toOk = true, bool activatorOk = true, bool instanceOk = true)
        {
            ResolverGenerator gen = null;

            if (To != null)
            {
                if (!toOk) 
                    throw new InvalidProgramException("Can not get to generator!");
                
                TestGeneratorNotMatched(gen);
                gen = GenerateTyped();
            }

            if (Activator != null)
            {
                if (!activatorOk) 
                    throw new InvalidProgramException("Can not get activator generator!");
                
                TestGeneratorNotMatched(gen);
                gen = GenerateMethod();
            }
            
            if (Instance != null)
            {
                if (!instanceOk) 
                    throw new InvalidProgramException("Can not get instance generator!");
                
                TestGeneratorNotMatched(gen);
                gen = GenerateInstance();
            }

            if (gen == null) 
                throw new InvalidProgramException("Binder is not valid! Can not collect any information " +
                                                  "related to resolver result");
                
            return gen;
        }

        private ResolverGenerator GenerateTyped()
        {
            var parameterOk = Parameter != null && Parameter.Length > 0;

            switch (Lifetime)
            {
                case Lifetime.Transient:
                {
                    if (parameterOk) return () => new ParameterTypedTransientResolver(To, Parameter);
                    else return () => new TypedTransientResolver(To);
                } break;

                case Lifetime.Scope:
                {
                    if (parameterOk) return () => new ParameterTypedScopeResolver(!Lazy, To, Parameter);
                    else return () => new TypedScopeResolver(!Lazy, To);
                } break;
                
                
                case Lifetime.Singleton:
                {
                    if (parameterOk) return () => new ParameterTypedSingletonResolver(!Lazy, To, Parameter);
                    else return () => new TypedSingletonResolver(!Lazy, To);
                } break;
            }

            return () => null;
        }

        private ResolverGenerator GenerateMethod()
        {
            switch (Lifetime)
            {
                case Lifetime.Transient: return () => new FromFactoryTransientResolver(Activator);
                case Lifetime.Scope: return () => new FromMethodScopeResolver(!Lazy, Activator);
                case Lifetime.Singleton: return () => new FromMethodSingletonResolver(!Lazy, Activator);
            }

            return () => null;
        }

        private ResolverGenerator GenerateInstance()
        {
            switch (Lifetime)
            {
                case Lifetime.Singleton: return () => new FromInstanceSingletonResolver(Instance);
            }

            return () => null;
        }
        
        private ResolverGenerator GenerateOther()
        {
            throw new InvalidProgramException("Can not generate properly Lifetime Resolver!");
        }

        #endregion

        internal void MakeReady()
        {
            Lifetime = Lifetime.Transient;
            To = null;
            Activator = null;
            Instance = null;
            Lazy = true;
            Parameter = null;
        }

        private void TestGeneratorNotMatched(ResolverGenerator gen)
        {
            if (gen != null) throw new InvalidProgramException(
                "Can not get valid generator: Argument is not correct.");
        }
    }
}