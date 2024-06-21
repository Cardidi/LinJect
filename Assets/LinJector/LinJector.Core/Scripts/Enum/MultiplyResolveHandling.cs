namespace LinJector.Enum
{
    /// <summary>
    /// How do container do when parent has a same binding with current container.
    /// </summary>
    public enum MultiplyResolveHandling
    {
        
        /// <summary>
        /// Do not handle this and break building.
        /// </summary>
        Throw = 0,
        
        /// <summary>
        /// Use parent's binding and continue building.
        /// </summary>
        UseParent,
        
        /// <summary>
        /// Use current's binding and continue building.
        /// </summary>
        UseChild
    }
}