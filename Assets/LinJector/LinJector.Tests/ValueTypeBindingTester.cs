using System;
using LinJector.Core;
using LinJector.Core.Binder;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

public class ValueTypeBindingTester
{
    
    /*
     * Create direct child container from SuperEmpty container and trying to resolve them.
     */
    
    private Container CreateValueTestInstanceContainer()
    {
        return Container.Create((parent, builder) =>
        {
            builder.Bind<int>().ToInstance(10);
            builder.Bind<int>().WithId("Test Int").ToInstance(20);
        });
    }
    
    // Resolve
    
    [Test]
    public void NonGenericValueInstanceResolve()
    {
        var c = CreateValueTestInstanceContainer();
        Assert.IsTrue(c.Resolve<int>() == 10, "NoID test failure!");
        Assert.IsTrue(c.Resolve<int>("Test Int") == 20, "WithID test failure!");
    }
    
    
    [Test]
    public void GenericValueInstanceResolve()
    {
        var c = CreateValueTestInstanceContainer();
        Assert.IsTrue((int) c.Resolve(typeof(int)) == 10, "NoID test failure!");
        Assert.IsTrue((int)c.Resolve(typeof(int), "Test Int") == 20, "WithID test failure!");
    }
    
    // Resovle All
    
    [Test]
    public void NonGenericValueInstanceResolveAll()
    {
        var c = CreateValueTestInstanceContainer();
        Assert.IsTrue(c.ResolveAll<int>().Length == 2, "NoID test failure!");
        Assert.IsTrue(c.ResolveAll<int>("Test Int").Length == 1, "WithID test failure!");
    }
    
    
    [Test]
    public void GenericValueInstanceResolveAll()
    {
        var c = CreateValueTestInstanceContainer();
        Assert.IsTrue(c.ResolveAll(typeof(int)).Length == 2, "NoID test failure!");
        Assert.IsTrue(c.ResolveAll(typeof(int), "Test Int").Length == 1, "WithID test failure!");
    }
    
    /*
     * Create Sub container from parent container and trying to resolve them.
     */
    
    private Container CreateSubValueTestInstanceContainer()
    {
        return CreateValueTestInstanceContainer().CreateChild((parent, builder) =>
        {
            builder.Bind<int>().ToInstance(30);
            builder.Bind<int>().WithId("Test Int").ToInstance(40);
        });
    }
    
    // Resolve
    
    [Test]
    public void NonGenericValueInstanceSubContainerResolve()
    {
        var c = CreateSubValueTestInstanceContainer();
        Assert.IsTrue(c.Resolve<int>() == 30, "NoID test failure!");
        Assert.IsTrue(c.Resolve<int>("Test Int") == 40, "WithID test failure!");
    }
    
    
    [Test]
    public void GenericValueInstanceSubContainerResolve()
    {
        var c = CreateSubValueTestInstanceContainer();
        Assert.IsTrue((int)c.Resolve(typeof(int)) == 30, "NoID test failure!");
        Assert.IsTrue((int)c.Resolve(typeof(int), "Test Int") == 40, "WithID test failure!");
    }
    
    // Resolve All
    
    [Test]
    public void GenericValueInstanceSubContainerResolveAll()
    {
        var c = CreateSubValueTestInstanceContainer();
        Assert.IsTrue(c.ResolveAll(typeof(int)).Length == 4, "NoID test failure!");
        Assert.IsTrue(c.ResolveAll(typeof(int), "Test Int").Length == 2, "WithID test failure!");
    }
    
    [Test]
    public void NonGenericValueInstanceSubContainerResolveAll()
    {
        var c = CreateSubValueTestInstanceContainer();
        Assert.IsTrue(c.ResolveAll<int>().Length == 4, "NoID test failure!");
        Assert.IsTrue(c.ResolveAll<int>("Test Int").Length == 2, "WithID test failure!");
    }
    
}