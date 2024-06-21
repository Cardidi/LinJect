namespace LinJector.Enum
{
    /// <summary>
    /// The timing we create object in container (or what we talk the lifetime of binding).
    /// </summary>
    public enum Lifetime
    {
        
        /// <summary>
        /// Create only one object across this container and it's children recursively.
        /// </summary>
        Singleton,
        
        /// <summary>
        /// Create different objects for each container and it's children. 
        /// </summary>
        Scope,
        
        /// <summary>
        /// Create new object everytime when resolving.
        /// </summary>
        Transient,
    }
}