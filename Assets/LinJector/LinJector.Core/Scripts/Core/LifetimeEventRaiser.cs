using System;
using UnityEngine;

namespace LinJector.Core
{
    [DisallowMultipleComponent]
    public class LifetimeEventRaiser : MonoBehaviour
    {
        private static LifetimeEventRaiser _instance;
        
        public static LifetimeEventRaiser Instance {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[LinJect::LifetimeEventRaiser]", typeof(LifetimeEventRaiser));
                    DontDestroyOnLoad(go);
                    _instance = go.GetComponent<LifetimeEventRaiser>();
                }

                return _instance;
            }
        }

        public event Action Tick;

        public event Action FixedTick;

        private void Update()
        {
            Tick?.Invoke();
        }

        private void FixedUpdate()
        {
            FixedTick?.Invoke();
        }
    }
}