using System;
using System.Collections.Generic;
using System.Linq;
using Ca2d.Toolkit;
using LinJector.Interface;
using UnityEngine.Pool;

namespace LinJector.Core
{
    internal sealed class LifetimeEventRegistry : IDisposable
    {
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
            // Takes up all relative objects
            using (ListPool<ILifetimeResolver>.Get(out var rrs))
            {
                void SetupList<T>(List<T> target)
                {
                    _container.TakeResolvers(typeof(T), null, rrs);
                    target.AddRange(
                        rrs.Where(p => p is not IIgnoreEventResolver)
                            .Select(p => p.Resolve<T>(_container)));
                }
                
                SetupList(_initialize);
                rrs.Clear();
                SetupList(_fixedTickable);
                rrs.Clear();
                SetupList(_tickable);
                rrs.Clear();
                SetupList(_disposable);
            }
            
            // Do initialize callback
            InitializeCallback();
            LifetimeEventRaiser.Instance.Tick += TickCallback;
            LifetimeEventRaiser.Instance.FixedTick += FixedTickCallback;
        }
        
        public void Dispose()
        {
            // Do content dispose
            LifetimeEventRaiser.Instance.Tick -= TickCallback;
            LifetimeEventRaiser.Instance.FixedTick -= FixedTickCallback;
            DisposeCallback();
            
            // Do final cleanup
            _disp?.Dispose();
            _initialize.Clear();
            _fixedTickable.Clear();
            _tickable.Clear();
            _disposable.Clear();
        }

        private void InitializeCallback()
        {
            foreach (var cb in _initialize)
                cb.IgnoreException(c => c.Initialize());
        }

        private void DisposeCallback()
        {
            foreach (var cb in _disposable)
                cb.IgnoreException(c => c.Dispose());
        }
        
        private void FixedTickCallback()
        {
            foreach (var cb in _fixedTickable)
                cb.IgnoreException(c => c.FixedTick());
        }

        private void TickCallback()
        {
            foreach (var cb in _tickable)
                cb.IgnoreException(c => c.Tick());
        }
    }
}