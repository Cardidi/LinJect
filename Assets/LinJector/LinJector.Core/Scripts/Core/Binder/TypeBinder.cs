using System;

namespace LinJector.Core.Binder
{
    public class TypeBinder
    {
        public Type From;

        public object Id;

        public ResolverBinder Resolver;

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