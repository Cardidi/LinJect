using LinJector.Core;
using LinJector.Core.Binder;
using LinJector.Core.Reflection;
using NUnit.Framework;
using UnityEngine;

public class InjectionTester
{
    private class Tester1
    {
        [Inject(Id = "id")]
        public int Id;

        public string Name;

        public string Addresss;

        private Tester1(string name, string addresss)
        {
            Name = name;
            Addresss = addresss;
        }
    }

    private static int idAlloc;
    
    private Container GetContainer()
    {
        return Container.Create((c, b) =>
        {
            b.Bind<int>().WithId("id").ToMethod(c => ++idAlloc);
            b.Bind<string>().ToInstance("def");
        });
    }

    [Test]
    public void CreateObject()
    {
        var obj = GetContainer().NewObject<Tester1>("abc");
        Assert.AreEqual(obj.Name, "abc");
        Assert.AreEqual(obj.Addresss, "def");
        Debug.Log(obj.Id);
    }

    private class Test2
    {
        [Inject(Id = "id")]
        public int Id;

        [Inject]
        public string Data;

        public int IdAlter = 0;
    }
    
    [Test]
    public void InjectObject()
    {
        var obj = new Test2();
        GetContainer().Inject(obj);
        
        Assert.AreEqual(obj.Data, "def");
        Assert.AreNotEqual(obj.Id, obj.IdAlter);
    }
    
    private class Test3
    {
        public int Id;

        public string Data;

        [Inject]
        private void Injector(string data, [Inject(Id = "id")] int id)
        {
            Id = id;
            Data = data;
        }
    }
    
    [Test]
    public void InjectObjectWithMethod()
    {
        var obj = new Test3();
        GetContainer().Inject(obj);
        
        Assert.AreEqual(obj.Data, "def");
        Assert.AreNotEqual(0, obj.Id);
    }
    
    private class Test4
    {
        public int Id;

        public string Data;

        private Test4([Inject(Id = "id")] int id)
        {
            Id = id;
        }
        
        [Inject]
        private void Injector(string data)
        {
            Data = data;
        }
    }
    
    [Test]
    public void CreateObjectWithMethodAndCtor()
    {
        var obj = GetContainer().NewObject<Test4>();
        
        Assert.AreEqual(obj.Data, "def");
        Assert.AreNotEqual(0, obj.Id);
    }
}