using System;
using System.Reflection;

namespace LinJector.Interface
{
    public interface IActivatorFactory
    {
        public delegate object ObjectActivator(params object[] args);

        public ObjectActivator MakeActivator(Type type, ConstructorInfo constructor, Type[] parameters);
        
        public ObjectActivator MakeDefaultActivator(Type type);
        
    }
}