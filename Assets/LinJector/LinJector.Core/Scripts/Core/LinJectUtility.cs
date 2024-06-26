using LinJector.Core.Activator;
using LinJector.Interface;

namespace LinJector.Core
{
    public static class LinJectUtility
    {

        private static IActivatorFactory _activatorFactory;

        public static IActivatorFactory GetRuntimeActivatorFactory()
        {
            if (_activatorFactory == null)
            {
#if ENABLE_MONO || ENABLE_IL2CPP
                // _activatorFactory = new MonoActivatorFactory();
                _activatorFactory = new IL2CPPActivatorFactory();
#else
                _activatorFactory = new UnsupportedActivatorFactory();
#endif
            }

            return _activatorFactory;
        }
    }
}