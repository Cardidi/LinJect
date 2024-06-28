using System;
using LinJector.Core;
using LinJector.Core.Binder;
using NUnit.Framework;

public class BindingMethodsTester
{
    private class BindingSelfAndInterfaces : IDisposable, IServiceProvider
    {
        public void Dispose()
        {
            // TODO release managed resources here
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }
    
    [Test]
    public void BindingInterfacesAndSelfTo()
    {
        var c = Container.Create((parent, builder) =>
        {
            builder.BindInterfacesAndSelf<BindingSelfAndInterfaces>().ToSelf().AsSingleton();
        });
        
        Assert.AreEqual(c.Resolve<IDisposable>(), c.Resolve<BindingSelfAndInterfaces>());
        Assert.IsNotNull(c.Resolve<BindingSelfAndInterfaces>());
    }
    
    [Test]
    public void BindingInterfacesTo()
    {
        var c = Container.Create((parent, builder) =>
        {
            builder.BindInterfaces<BindingSelfAndInterfaces>().ToSelf().AsSingleton();
        });
        
        Assert.AreEqual(c.Resolve<IDisposable>(), c.Resolve<IServiceProvider>());
        Assert.IsNull(c.Resolve<BindingSelfAndInterfaces>());
    }
}