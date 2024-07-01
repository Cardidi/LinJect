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
            b.Bind<int>().WithId("id").ToMethod(c => idAlloc++);
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
}