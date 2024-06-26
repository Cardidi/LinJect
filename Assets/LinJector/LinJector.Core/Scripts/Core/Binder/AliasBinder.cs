using System;

namespace LinJector.Core.Binder
{
    public class AliasBinder
    {
        internal Type From;

        internal object Id;

        internal Type To;

        internal object ToId;

        internal bool Validate()
        {
            return From != null && To != null;
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