using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.Pool;

namespace LinJector.Core.Reflection
{
    #region ClassStructureDescription

    public class InjectiveValue
    {
        public static bool Analyse(FieldInfo info, out InjectiveValue result)
        {
            result = new();

            if (info.IsInitOnly || info.IsStatic) return false;
            
            var att = Attribute.GetCustomAttribute(info, typeof(InjectAttribute), true) as InjectAttribute;
            if (att == null) return false;
            
            result._isField = true;
            result._fieldInfo = info;
            result._attribute = att;

            return true;
        }
        
        public static bool Analyse(PropertyInfo info, out InjectiveValue result)
        {
            result = new();

            if (!info.CanWrite) return false;
            
            var att = Attribute.GetCustomAttribute(info, typeof(InjectAttribute), true) as InjectAttribute;
            if (att == null) return false;
            
            result._isField = false;
            result._propertyInfo = info;
            result._attribute = att;

            return true;
        }
        
        private InjectAttribute _attribute;
        
        private FieldInfo _fieldInfo;

        private PropertyInfo _propertyInfo;

        private bool _isField;
        
        private bool _isCollection;

        public string Name
        {
            get
            {
                if (_isField) return _fieldInfo.Name;
                return _propertyInfo.Name;
            }
        }
        
        public bool IsOptional => _attribute.Optional;

        public object Id => _attribute.Id;

        public Type RequestedType
        {
            get
            {
                if (_isField) return _fieldInfo.FieldType;
                return _propertyInfo.PropertyType;
            }
        }

        public void SetData(object target, object data)
        {
            if (_isField) _fieldInfo.SetValue(target, data);
            else _propertyInfo.SetValue(target, data);
        }
    }

    public struct InjectiveParameter
    {
        public static InjectiveParameter Analyse(ParameterInfo target)
        {
            InjectiveParameter result = default;
            result._parameterInfo = target;
            result._attribute = Attribute.GetCustomAttribute(target, typeof(InjectAttribute)) as InjectAttribute;

            return result;
        }
        
        private InjectAttribute _attribute;

        private ParameterInfo _parameterInfo;

        public bool Injective => _attribute != null;

        public bool IsInjectionOptional => _attribute?.Optional ?? false;

        public bool IsParameterOptional => _parameterInfo.IsOptional;
        
        public object Id => _attribute?.Id;

        public Type RequestedType => _parameterInfo.ParameterType;
    }

    public class InjectiveMethodBase
    {
        public static bool Analyse(MethodBase target, out InjectiveMethodBase result)
        {
            result = new();
            
            if (target.ContainsGenericParameters) return false;
            if (target is ConstructorInfo) result._isConstructor = true;
            
            // Do not use this method if this is not marked as injective or not a constructor
            var att = Attribute.GetCustomAttribute(target, typeof(InjectAttribute), true) as InjectAttribute;
            if (att == null && !result._isConstructor) return false;

            result._markAsInjection = att != null;
            result._attribute = att;
            result._methodInfo = target;
            result._parameters = target.GetParameters().Select(InjectiveParameter.Analyse).ToArray();

            return true;
        }
        
        private InjectAttribute _attribute;

        private bool _markAsInjection;

        private MethodBase _methodInfo;

        private bool _isConstructor;

        private InjectiveParameter[] _parameters;

        public InjectiveParameter[] Parameters => _parameters ?? Array.Empty<InjectiveParameter>();

        public bool MarkAsInjection => _markAsInjection;
        
        public bool IsConstructor => _isConstructor;
        
        public ConstructorInfo AsConstructorInfo => _methodInfo as ConstructorInfo;
        
        public MethodInfo AsMethodInfo => _methodInfo as MethodInfo;
        
        public void Invoke(object target, object[] parameters)
        {
            _methodInfo.Invoke(target, parameters);
        }
    }

    #endregion
    
    /// <summary>
    /// The map which used to describe the layout of Object/ValueType in injection view.
    /// </summary>
    public sealed class ObjectReflectionStructureMap
    {
        private static Dictionary<Type, ObjectReflectionStructureMap> _caching = new();

        public static void Cache(Type source) => Analyse(source);
        
        public static ObjectReflectionStructureMap Analyse(Type source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            
            if (!_caching.TryGetValue(source, out var r))
            {
                r = new ObjectReflectionStructureMap(source);
                _caching.Add(source, r);
            }

            return r;
        }
        
        private ObjectReflectionStructureMap(Type source)
        {
            using (ListPool<InjectiveMethodBase>.Get(out var methods))
            using (ListPool<InjectiveValue>.Get(out var members))
            {
                var searches = source.GetMembers(
                    BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | 
                    BindingFlags.NonPublic | BindingFlags.SetField | 
                    BindingFlags.SetProperty | BindingFlags.InvokeMethod);

                foreach (var b in searches.OfType<MethodBase>())
                    if (InjectiveMethodBase.Analyse(b, out var r)) methods.Add(r);

                foreach (var f in searches.OfType<FieldInfo>())
                    if (InjectiveValue.Analyse(f, out var r)) members.Add(r);
                
                foreach (var f in searches.OfType<PropertyInfo>())
                    if (InjectiveValue.Analyse(f, out var r)) members.Add(r);
                
                // Take out of results
                Methods = methods.ToArray();
                Values = members.ToArray();
            }

            CapturedType = source;
            IsValueType = source.IsValueType;
        }
        
        /// <summary>
        /// Which type did this structure capture for?
        /// </summary>
        public Type CapturedType { get; }
        
        /// <summary>
        /// All methods which can used as injection target such as constructor or method marked with [<see cref="InjectAttribute"/>]
        /// </summary>
        public IReadOnlyCollection<InjectiveMethodBase> Methods { get; }
        
        /// <summary>
        /// All properties and fields which is marked with [<see cref="InjectAttribute"/>]
        /// </summary>
        public IReadOnlyCollection<InjectiveValue> Values { get; }
        
        /// <summary>
        /// Is this structure capture from ValueType?
        /// </summary>
        public bool IsValueType { get; }

        /// <summary>
        /// Find the constructor which can match more arguments.
        /// </summary>
        /// <param name="excludeInjectAtt">Will constructor with injection attribute will being excluded?</param>
        /// <param name="arguments">Given arguments</param>
        /// <returns>The best fit constructor, but can not guarantee that it can be call successfully.</returns>
        public InjectiveMethodBase SearchConstructor(bool excludeInjectAtt, object[] arguments)
        {
            using (ListPool<Type>.Get(out var argTypes))
            {
                argTypes.AddRange(arguments.Where(a => a != null).Select(a => a.GetType()));
                var bestfitArgumentsCount = -1;
                InjectiveMethodBase bestfitMethod = null;

                var selector = excludeInjectAtt
                    ? Methods.Where(m => m.IsConstructor && !m.MarkAsInjection)
                    : Methods.Where(m => m.IsConstructor);
                
                foreach (var b in selector.Where(m => m.Parameters.Length > 0))
                {
                    var idx = 0;
                    foreach (var p in b.Parameters)
                    {
                        if (idx >= argTypes.Count) break;
                        var matchType = argTypes[idx];
                        if (p.RequestedType.IsAssignableFrom(matchType)) idx++;
                    }

                    if (idx <= bestfitArgumentsCount) continue;
                    bestfitArgumentsCount = idx;
                    bestfitMethod = b;
                }

                return bestfitMethod;
            }
        }

        /// <summary>
        /// Find the default constructor which can give the most less parameters.
        /// </summary>
        /// <returns>The best fit constructor, but can not guarantee that it can be call successfully.</returns>
        public InjectiveMethodBase SearchDefaultConstructor()
        {
            var selector = Methods.Where(m => m.IsConstructor);
            return selector.FirstOrDefault(m => !m.Parameters.Any());
        }

        /// <summary>
        /// Find the best method marked with [<see cref="InjectAttribute"/>] for injections.
        /// </summary>
        /// <param name="bindings">Collection of types can be provided from container.</param>
        /// <returns>The best fit method, but can not guarantee that it can be call successfully.</returns>
        public InjectiveMethodBase SearchInjectionMethod(ICollection<Type> bindings)
        {
            return Methods
                .Where(m => m.Parameters.Length > 0)
                .Where(m => !m.IsConstructor)
                .OrderByDescending(m => m.Parameters
                    .Sum(p =>
                    {
                        if (p.IsInjectionOptional || p.IsParameterOptional) return 0;
                        return bindings.Contains(p.RequestedType) ? 1 : -1;
                    }))
                .FirstOrDefault();
        }
    }
}