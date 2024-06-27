using System;
using LinJector.Core;
using LinJector.Core.Binder;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

public class FatalBindingTester
{
    [Test]
    public void BinderToTargetNotMatch()
    {
        var c = Container.Create((parent, builder) => 
        { 
            builder.Bind(typeof(IDisposable)).ToInstance(10).NonLazy();
        });
     
        Assert.IsNull(c.Resolve<IDisposable>());
    }
}