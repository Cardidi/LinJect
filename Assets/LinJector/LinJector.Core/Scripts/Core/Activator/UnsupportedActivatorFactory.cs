using System;
using System.Reflection;
using LinJector.Interface;

namespace LinJector.Core.Activator
{
    public sealed class UnsupportedActivatorFactory : IActivatorFactory
    {
        private static readonly IActivatorFactory.ObjectActivator Error = (_, _) => throw new InvalidProgramException(
            "Activator is trying to create new object on this platform but unfortunately that this platform " +
            "is unsupported.");
        
        public IActivatorFactory.ObjectActivator MakeActivator(Type type, ConstructorInfo constructor)
        {
            return Error;
        }

        public IActivatorFactory.ObjectActivator MakeDefaultActivator(Type type)
        {
            return Error;
        }
    }
}