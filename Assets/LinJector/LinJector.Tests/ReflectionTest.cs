using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using LinJector.Core.Reflection;
using NUnit.Framework;

public class ReflectionTest
{
    
    #region MembersTest

    private class ReflectionMembers
    {
        public int Field1; // Invalid

        [Inject]
        public bool Field2; // Valid
        
        [Inject]
        public readonly object Field3; // Invalid
        
        [Inject]
        public static decimal Field4; // Invalid

        [Inject]
        private Action Field5; // Valid
        
        public float Property1 { get; set; } // Invalid
        
        [Inject]
        public double Property2 { get; } // Invalid
        
        [Inject]
        public List Property3 { get; set; } // Valid
        
        [Inject]
        public static Array Property4 { get; set; } // Invalid
        
        [Inject]
        private Hash Property5 { get; set; } // Valid
    }
    
    private class ChildReflectionMembers : ReflectionMembers
    {
        [Inject] private byte Child; // Valid
    }
    
    private bool CanGetType<T>()
        => ObjectReflectionStructureMap.Analyse(typeof(ReflectionMembers)).Values
            .Any(p => p.RequestedType == typeof(T));

    private bool CanGetTypeInChild<T>()
        => ObjectReflectionStructureMap.Analyse(typeof(ChildReflectionMembers)).Values
            .Any(p => p.RequestedType == typeof(T));
    
    [Test]
    public void ChildInjectedMember()
    {
        Assert.IsTrue(CanGetType<bool>(), "Parent public fields did not catch properly");
        Assert.IsTrue(CanGetType<List>(), "Parent public property did not catch properly");
        Assert.IsTrue(CanGetType<Action>(), "Parent private fields did not catch properly");
        Assert.IsTrue(CanGetType<Hash>(), "Parent private property did not catch properly");
        Assert.IsTrue(CanGetTypeInChild<byte>(), "Child member did not catch properly");
    }
        
    [Test]
    public void PublicInjectedMember()
    {
        Assert.IsTrue(CanGetType<bool>(), "Fields did not catch properly");
        Assert.IsTrue(CanGetType<List>(), "Property did not catch properly");
    }
        
    [Test]
    public void PrivateInjectedMember()
    {
        Assert.IsTrue(CanGetType<Action>(), "Fields did not catch properly");
        Assert.IsTrue(CanGetType<Hash>(), "Property did not catch properly");
    }

    [Test]
    public void ReadonlyInjectedMember()
    {
        Assert.IsFalse(CanGetType<object>(), "Fields did not catch properly");
        Assert.IsFalse(CanGetType<double>(), "Property did not catch properly");
    }
        
    [Test]
    public void StaticInjectedMember()
    {
        Assert.IsFalse(CanGetType<decimal>(), "Fields did not catch properly");
        Assert.IsFalse(CanGetType<Array>(), "Property did not catch properly");
    }
        
    [Test]
    public void NonInjectedMember()
    {
        Assert.IsFalse(CanGetType<int>(), "Fields did not catch properly");
        Assert.IsFalse(CanGetType<float>(), "Property did not catch properly");
    }

    #endregion

    #region PrivateConstructor

    private class PrivateConstructor
    {
        public int Result;

        [Inject]
        private PrivateConstructor()
        {
            Result = 1;
        }

        [Inject]
        private PrivateConstructor(int a)
        {
            Result = 2;
        }

        [Inject]
        private PrivateConstructor(int a, int b)
        {
            Result = 3;
        }
        
        [Inject]
        private PrivateConstructor(int a, bool b)
        {
            Result = 4;
        }

        private PrivateConstructor(string b)
        {
            Result = 5;
        }
    }
    

    [Test]
    public void NonInjectedPrivateConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, new object[]{"abc"});

        c.Invoke(test, new object[]{"abc"});
        
        Assert.AreEqual(5, test.Result);
    }
    
    
    [Test]
    public void NonInjectedPrivateExcludeInjectionConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(true, new object[]{"abc"});

        c.Invoke(test, new object[]{"abc"});
        
        Assert.AreEqual(5, test.Result);
    }

    [Test]
    public void DefaultPrivateConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchDefaultConstructor();

        c.Invoke(test, Array.Empty<object>());
        
        Assert.AreEqual(1, test.Result);
    }
    
    [Test]
    public void NoParameterPrivateConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, Array.Empty<object>());

        Assert.IsNull(c);
    }
    
    [Test]
    public void DefaultPrivateInjectedExcludeInjectionConstructor()
    {
        var t = typeof(PrivateConstructor);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchDefaultConstructor();

        Assert.IsNull(c);
    }
    
    [Test]
    public void OneParameterPrivateConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, new object[]{(int)1});

        c.Invoke(test, new object[]{1});
        
        Assert.AreEqual(2, test.Result);
    }
    
    [Test]
    public void OneParameterPrivateInjectedExcludeInjectionConstructor()
    {
        var t = typeof(PrivateConstructor);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(true, new object[]{(int)1});

        Assert.IsNull(c);
    }
    
    [Test]
    public void TwoParameterPrivateConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, new object[]{(int)1, (int)1});

        c.Invoke(test, new object[]{1, 1});
        
        Assert.AreEqual(3, test.Result);
    }
    
    [Test]
    public void TwoParameterPrivateInjectedExcludeInjectionConstructor()
    {
        var t = typeof(PrivateConstructor);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(true, new object[]{(int)1, (int)1});

        Assert.IsNull(c);
    }
    
    [Test]
    public void TwoParameterAlterArgumentPrivateConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, new object[]{(int)1, (bool)false});

        c.Invoke(test, new object[]{1, false});
        
        Assert.AreEqual(4, test.Result);
    }
    
    [Test]
    public void TwoParameterAlterArgumentPrivateInjectedExcludeInjectionConstructor()
    {
        var t = typeof(PrivateConstructor);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(true, new object[]{(int)1, false});

        Assert.IsNull(c);
    }
    
    [Test]
    public void TwoParameterReverseAlterArgumentPrivateInjectionConstructor()
    {
        var t = typeof(PrivateConstructor);
        var test = (PrivateConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, new object[]{false, (int)1});

        c.Invoke(test, new object[]{1, false});
        
        Assert.AreEqual(4, test.Result);
    }

    #endregion

    #region PublicConstructor

    private class PublicConstructor
    {
        public int Result;
        
        public PublicConstructor()
        {
            Result = 1;
        }
        
    }

    [Test]
    public void DefaultPublicConstructor()
    {
        var t = typeof(PublicConstructor);
        var test = (PublicConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchDefaultConstructor();

        c.Invoke(test, Array.Empty<object>());
        
        Assert.AreEqual(1, test.Result);
    }
    
    [Test]
    public void DefaultPublicExcludeInjectionConstructor()
    {
        var t = typeof(PublicConstructor);
        var test = (PublicConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchDefaultConstructor();

        c.Invoke(test, Array.Empty<object>());
        
        Assert.AreEqual(1, test.Result);
    }
    
    [Test]
    public void NoParameterPublicConstructor()
    {
        var t = typeof(PublicConstructor);
        var test = (PublicConstructor) RuntimeHelpers.GetUninitializedObject(t);
        
        var m = ObjectReflectionStructureMap.Analyse(t);
        var c = m.SearchConstructor(false, Array.Empty<object>());

        Assert.IsNull(c);
    }
    
    
    #endregion

    #region Methods

    private class Methods
    {
        [Inject]
        public Methods() {}
        
        public Methods(int a) {}
        
        public void M1()
        {}
        
        [Inject]
        public void M2()
        {}
        
        [Inject]
        public void M5(int a, string b)
        {}
        
        [Inject]
        public void M3(int a)
        {}
        
        public void M4(int a)
        {}
        
        
        public void M6(int a, string b)
        {}
    }

    [Test]
    public void ConstructorIsOutOfConsideration()
    {
        var m = ObjectReflectionStructureMap.Analyse(typeof(Methods));
        var call = m.SearchInjectionMethod(new List<Type>());
        
        Assert.IsFalse(call.IsConstructor);
    }
    
    [Test]
    public void NoInjectAttributeIsOutOfConsideration()
    {
        var m = ObjectReflectionStructureMap.Analyse(typeof(Methods));
        var call = m.SearchInjectionMethod(new List<Type>());
        
        Assert.AreEqual(call.AsMethodInfo.Name, "M2");
    }

    [Test]
    public void OneParamOneMatchMethod()
    {
        var m = ObjectReflectionStructureMap.Analyse(typeof(Methods));
        var call = m.SearchInjectionMethod(new List<Type>{typeof(int)});
        
        Assert.AreEqual(call.AsMethodInfo.Name, "M3");
    }
    
    
    [Test]
    public void TwoParamTwoMatchMethod()
    {
        var m = ObjectReflectionStructureMap.Analyse(typeof(Methods));
        var call = m.SearchInjectionMethod(new List<Type>{typeof(int), typeof(string)});
        
        Assert.AreEqual(call.AsMethodInfo.Name, "M5");
    }
    
    
    [Test]
    public void TwoParamOneMatchMethod()
    {
        var m = ObjectReflectionStructureMap.Analyse(typeof(Methods));
        var call = m.SearchInjectionMethod(new List<Type>{typeof(int)});
        
        Assert.AreEqual(call.AsMethodInfo.Name, "M3");
    }
    
    [Test]
    public void OneParamTwoMatchMethod()
    {
        var m = ObjectReflectionStructureMap.Analyse(typeof(Methods));
        var call = m.SearchInjectionMethod(new List<Type>{typeof(int), typeof(string)});
        
        Assert.AreEqual(call.AsMethodInfo.Name, "M3");
    }

    #endregion

    #region ValueType

    private struct ValueTypeTest
    {
        
    }

    #endregion
    
}