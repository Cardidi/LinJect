using System;
using System.Reflection;

namespace LinJector.Interface
{
    public interface IActivatorFactory
    {
        public delegate object ObjectActivator(Action<object> beforeConstructor, object[] args);

        public ObjectActivator MakeActivator(Type type, ConstructorInfo constructor);
        
        public ObjectActivator MakeDefaultActivator(Type type);
        
    }
}