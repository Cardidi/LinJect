using System;
using LinJector.Core;
using LinJector.Enum;

namespace LinJector.Interface
{

    /// <summary>
    /// The basic format of a resolver.
    /// </summary>
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
    
    /// <summary>
    /// An extended format resolver designed for container to run and manage 
    /// </summary>
    public interface ILifetimeResolver : IResolver
    {

        /// <summary>
        /// How did this resolver behave when create child container.
        /// </summary>
        public CrossContainer CrossContainerStrategy { get; }
        
        /// <summary>
        /// Try to get a brand-new resolver for child container.
        /// </summary>
        /// <param name="resolver">The final resolver we added to container</param>
        /// <returns>If return true, we will get a new resolver. If false, we can just reuse this resolver</returns>
        public bool Duplicate(out ILifetimeResolver resolver);
        
    }

    /// <summary>
    /// Additional flag for resolver to pre-initialize themselves intend to non-lazy format. 
    /// </summary>
    public interface IConsiderPreInitializeResolver
    {
        public void PreInitialize(Container container);
    }
    
    /// <summary>
    /// Additional flag for resolver to post-initialize themselves intend to create cached type generator format. 
    /// </summary>
    public interface IConsiderPostInitializeResolver
    {
        public void PostInitialize(Container container);
    }

    /// <summary>
    /// Additional flag for resolver to prevent container event handler do event on this resolver. Used to prevent
    /// cross-container lifetime manager conflict.
    /// </summary>
    public interface IIgnoreEventResolver
    {}
}