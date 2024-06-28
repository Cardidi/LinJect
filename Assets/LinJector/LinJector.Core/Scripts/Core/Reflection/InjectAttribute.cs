using System;

namespace LinJector.Core.Reflection
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Constructor | AttributeTargets.Method |
                    AttributeTargets.Property | AttributeTargets.Parameter, Inherited = true, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
        public bool Optional { get; set; }
        
        public object Id { get; set; }
    }
}