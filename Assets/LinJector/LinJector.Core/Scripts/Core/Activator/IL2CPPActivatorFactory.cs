using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using LinJector.Interface;
using ObjectActivator = LinJector.Interface.IActivatorFactory.ObjectActivator;

namespace LinJector.Core.Activator
{
    public sealed class IL2CPPActivatorFactory : IActivatorFactory
    {
        public ObjectActivator MakeActivator(Type type, ConstructorInfo constructor, Type[] parameters)
        {
            return (call, args) =>
            {
                var instance = RuntimeHelpers.GetUninitializedObject(type);
                call?.Invoke(instance);
                constructor.Invoke(instance, args);
                
                return instance;
            };
        }

        public ObjectActivator MakeDefaultActivator(Type type)
        {
            var defaultConstructor = type.GetConstructor(Array.Empty<Type>());
            return (call, args) =>
            {
                var instance = RuntimeHelpers.GetUninitializedObject(type);
                call?.Invoke(instance);
                defaultConstructor?.Invoke(instance, null);

                return instance;
            };
        }
    }
}