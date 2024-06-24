namespace LinJector.Enum
{
    /// <summary>
    /// Indicate how did a resolver copy or move from parent container to children container.
    /// </summary>
    public enum CrossContainer
    {
        /// <summary>
        /// Just move this resolver from parent to child. We can also say that just copy the pointer of resolver.
        /// </summary>
        Move = 0,
        
        /// <summary>
        /// Create a new object in child and copy initial argument of parent resolver.
        /// </summary>
        Copy = 1,
    }
}