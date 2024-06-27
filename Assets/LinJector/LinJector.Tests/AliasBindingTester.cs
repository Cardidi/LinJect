using LinJector.Core;
using LinJector.Core.Binder;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

public class AliasBindingTester
{
    private interface IAliasSource {}
        
    private class AliasTarget : IAliasSource {}

    [Test]
    public void InstanceAliasBinding()
    {
        var c = Container.Create((parent, builder) =>
        {
            builder.Bind<AliasTarget>().ToSelf().AsSingleton();
            builder.Bind<IAliasSource>().AliasOf<AliasTarget>();
        });
            
        Assert.AreEqual(c.Resolve<AliasTarget>(), c.Resolve<IAliasSource>(), "Alias test failed");
    }
    
    [Test]
    public void TransientAliasBinding()
    {
        var c = Container.Create((parent, builder) =>
        {
            builder.Bind<AliasTarget>().ToSelf();
            builder.Bind<IAliasSource>().AliasOf<AliasTarget>();
        });
            
        Assert.AreNotEqual(c.Resolve<AliasTarget>(), c.Resolve<IAliasSource>(), "Alias test failed");
    }
    
    
    [Test]
    public void ScopeAliasBinding()
    {
        var c = Container.Create((parent, builder) =>
        {
            builder.Bind<AliasTarget>().ToSelf().AsScoped();
            builder.Bind<IAliasSource>().AliasOf<AliasTarget>();
        });
            
        Assert.AreEqual(c.Resolve<AliasTarget>(), c.Resolve<IAliasSource>(), "Alias test failed");
        Assert.AreNotEqual(c.Resolve<AliasTarget>(), c.CreateChild((_,_) => {}).Resolve<IAliasSource>(), "Alias test failed");
    }
}