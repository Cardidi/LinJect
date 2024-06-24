using System;
using System.Reflection;

namespace LinJector.Interface
{
    public interface IActivatorFactory
    {
        public delegate object ObjectActivator(Action<object> beforeConstructor, params object[] args);

        public ObjectActivator MakeActivator(Type type, ConstructorInfo constructor, Type[] parameters);
        
        public ObjectActivator MakeDefaultActivator(Type type);
        
    }
}