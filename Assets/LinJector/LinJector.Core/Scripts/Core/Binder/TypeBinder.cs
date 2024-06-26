using System;

namespace LinJector.Core.Binder
{
    internal class TypeBinder
    {
        internal Type From;

        internal object Id;

        internal ResolverBinder Resolver;

        internal bool Validate()
        {
            return From != null && Resolver != null;
        }

        internal void MakeReady()
        {
            From = null;
            Id = null;
            Resolver = null;
        }
    }
}