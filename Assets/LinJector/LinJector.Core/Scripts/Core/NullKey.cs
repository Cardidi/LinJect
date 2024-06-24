namespace LinJector.Core
{
    /// <summary>
    /// Used to present an only-one null but API did not support null-hanlding situation.
    /// </summary>
    public sealed class NullKey
    {
        public static NullKey Get { get; } = new NullKey();
    }
}