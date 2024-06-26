using System;
using System.Linq.Expressions;
using System.Reflection;
using LinJector.Interface;
using ObjectActivator = LinJector.Interface.IActivatorFactory.ObjectActivator;

namespace LinJector.Core.Activator
{
    public sealed class MonoActivatorFactory : IActivatorFactory
    {
        
        public ObjectActivator MakeActivator(Type type, ConstructorInfo constructor, Type[] parameters)
        {
            var param = Expression.Parameter(typeof(object[]));
            var argumentsExpressions = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var index = Expression.Constant(i);
                var parameterType = parameters[i];
                var parameterAccessor = Expression.ArrayIndex(param, index);
                var parameterCast = Expression.Convert(parameterAccessor, parameterType ?? typeof(object));
                argumentsExpressions[i] = parameterCast;
            }

            var newExpression = Expression.New(constructor, argumentsExpressions);
            var lambda = Expression.Lambda(
                typeof(ObjectActivator),
                Expression.Convert(newExpression, typeof(object)), param);
            
            return (ObjectActivator) lambda.Compile();
        }

        public ObjectActivator MakeDefaultActivator(Type type)
        {
            var body = Expression.Convert(Expression.Default(type), typeof(object));
            var lambda = Expression.Lambda(typeof(ObjectActivator), body, Expression.Parameter(typeof(object[])));
            return (ObjectActivator) lambda.Compile();
        }
    }
}