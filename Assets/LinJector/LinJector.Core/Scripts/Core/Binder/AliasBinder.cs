using System;

namespace LinJector.Core.Binder
{
    public class AliasBinder
    {
        public Type From;

        public object Id;

        public Type To;

        public object ToId;

        internal bool Validate()
        {
            return From != null && To != null && From != To;
        }

        internal void MakeReady()
        {
            From = null;
            Id = null;
            To = null;
            ToId = null;
        }
    }
}