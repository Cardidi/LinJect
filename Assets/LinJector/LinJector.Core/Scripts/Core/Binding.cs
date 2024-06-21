using LinJector.Interface;

namespace LinJector.Core
{
    internal struct Binding
    {
        public readonly ResolveKey Key;

        public readonly IResolver Resolver;
    }
}