using System;
using System.Collections.Generic;
using System.Linq;
using LinJector.Interface;
using UnityEngine.Pool;

namespace LinJector.Core
{
    public class LifetimeEventRegistry : IDisposable
    {
        private static Type
            typeInit = typeof(IInitialize),
            typeTick = typeof(ITickable),
            typeFixedTick = typeof(IFixedTickable),
            typeDispose = typeof(IDisposable);
        
        private List<IInitialize> _initialize = new();

        private List<IFixedTickable> _fixedTickable = new();
        
        private List<ITickable> _tickable = new();

        private List<IDisposable> _disposable = new();

        private Container _container;

        private IDisposable _disp;
        
        public void BindContainer(Container container)
        {
            _container = container;
        }

        public void Initialize()
        {
            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                _container.TakeAllResolvers(rrs);
            }
        }
        
        public void Dispose()
        {
            // Do final cleanup
            _disp?.Dispose();
            _initialize.Clear();
            _fixedTickable.Clear();
            _tickable.Clear();
            _disposable.Clear();
        }
    }
}