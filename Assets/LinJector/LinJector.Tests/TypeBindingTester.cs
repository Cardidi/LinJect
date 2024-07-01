using LinJector.Core;
using LinJector.Core.Binder;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

public class TypeBindingTester
{
    private class UnitTest_PublicConstructor
    {
        public object Storage { get; set; }
            
        public int Counter { get; set; }

        public UnitTest_PublicConstructor()
        {}

        public UnitTest_PublicConstructor(int counter)
        {
            Counter = counter;
        }
    }
        
        
    [Test]
    public void TransientBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(20);
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsFalse(t1 == t2, "Transient test failed!");
    }
    
            
    [Test]
    public void SubContainerTransientBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<int>().ToInstance(1);
            container.Bind<UnitTest_PublicConstructor>().ToSelf();
        }).CreateChild((_, _) => {});

        Assert.IsNotNull(c.Resolve<UnitTest_PublicConstructor>(), "Sub Container test failed!");
    }

        
    [Test]
    public void ParameterTransientBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(-1);
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsTrue(t1.Counter == -1 && t2.Counter == -1, "Parameter test failed!");
        Assert.IsFalse(t2 == t1, "Transient test failed!");
    }
    
    [Test]
    public void SubContainerParameterTransientBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(-1);
        }).CreateChild((_, _) => {});

        Assert.IsNotNull(c.Resolve<UnitTest_PublicConstructor>(), "Sub Container test failed!");
    }

    
    [Test]
    public void InstanceBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<int>().ToInstance(10);
            container.Bind<UnitTest_PublicConstructor>().ToSelf().AsSingleton();
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsTrue(t1 == t2, "Singleton test failed!");
    }
    
    [Test]
    public void SubContainerInstanceBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(20).AsSingleton();
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.CreateChild((_,_) => {}).Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsTrue(t1 == t2, "Singleton test failed!");
    }
        
    [Test]
    public void ParameterInstanceBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(-1).AsSingleton();
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsTrue(t2 == t1, "Singleton test failed!");
        Assert.IsTrue(t1.Counter == -1, "Parameter test failed!");
    }
    
    [Test]
    public void SubContainerParameterInstanceBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(-1).AsSingleton();
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.CreateChild((_,_) => {}).Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsTrue(t2 == t1, "Singleton test failed!");
        Assert.IsTrue(t1.Counter == -1, "Parameter test failed!");
    }
    
    [Test]
    public void ScopeBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<int>().ToInstance(10);
            container.Bind<UnitTest_PublicConstructor>().ToSelf(5).AsScoped();
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.CreateChild((_,_) => {}).Resolve<UnitTest_PublicConstructor>();
            
        Assert.AreEqual(t1.Counter, 5);
        Assert.IsFalse(t1 == t2, "Scoped test failed!");
    }
        
    [Test]
    public void ParameterScopeBinding()
    {
        var c = Container.Create((parent, container) =>
        {
            container.Bind<UnitTest_PublicConstructor>().ToSelf(-1).AsScoped();
        });

        var t1 = c.Resolve<UnitTest_PublicConstructor>();
        var t2 = c.CreateChild((_,_) => {}).Resolve<UnitTest_PublicConstructor>();
            
        Assert.IsFalse(t2 == t1, "Scope test failed!");
        Assert.IsTrue(t1.Counter == -1, "Parameter test failed!");
        Assert.IsTrue(t2.Counter == -1, "Parameter test failed!");
    }
}