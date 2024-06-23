using System;
using LinJector.Core;
using LinJector.Enum;

namespace LinJector.Interface
{

    public interface IResolver
    {
        /// <summary>
        /// Try to get the result of resolver.
        /// </summary>
        /// <param name="container">Who requested for this resolver?</param>
        public object Resolve(Container container);

        /// <summary>
        /// Try to get the result of resolver.
        /// </summary>
        /// <param name="container">Who requested for this resolver?</param>
        public T Resolve<T>(Container container);
    }
    
    public interface ILifetimeResolver : IResolver
    {
        /// <summary>
        /// The lifetime of this kind of resolver
        /// </summary>
        public Lifetime Lifetime { get; }
        
        /// <summary>
        /// Try to get a brand-new resolver for child container.
        /// </summary>
        /// <param name="resolver">The final resolver we added to container</param>
        /// <returns>If return true, we will get a new resolver. If false, we can just reuse this resolver</returns>
        public bool Duplicate(out ILifetimeResolver resolver);
        
    }
}