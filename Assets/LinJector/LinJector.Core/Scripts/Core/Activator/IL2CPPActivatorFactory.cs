using System;
using System.Reflection;
using LinJector.Interface;
using ObjectActivator = LinJector.Interface.IActivatorFactory.ObjectActivator;

namespace LinJector.Core.Activator
{
    public class IL2CPPActivatorFactory : IActivatorFactory
    {
        public ObjectActivator MakeActivator(Type type, ConstructorInfo constructor, Type[] parameters)
        {
            return args =>
            {
                var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
                constructor.Invoke(instance, args);
                return instance;
            };
        }

        public ObjectActivator MakeDefaultActivator(Type type)
        {
            return args => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        }
    }
}