using System.Threading.Tasks;

namespace LinJector.Interface
{
    public interface IInitialize
    {
        public void Initialize();
    }

    public interface IAsyncInitialize
    {
        public ValueTask InitializeAsync();
    }

    public interface ITickable
    {
        public void Tick();
    }

    public interface IFixedTickable
    {
        public void FixedTick();
    }
}