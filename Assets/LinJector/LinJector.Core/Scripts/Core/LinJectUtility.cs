using LinJector.Core.Activator;
using LinJector.Interface;

namespace LinJector.Core
{
    internal static class LinJectUtility
    {

        private static IActivatorFactory _activatorFactory;

        public static IActivatorFactory GetRuntimeActivatorFactory()
        {
            if (_activatorFactory == null)
            {
#if ENABLE_MONO
                _activatorFactory = new MonoActivatorFactory();
#elif ENABLE_IL2CPP
                _activatorFactory = new IL2CPPActivatorFactory();
#else
                _activatorFactory = new UnsupportedActivatorFactory();
#endif
            }

            return _activatorFactory;
        }
    }
}