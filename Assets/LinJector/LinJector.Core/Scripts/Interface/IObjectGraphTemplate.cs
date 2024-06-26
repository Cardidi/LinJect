using LinJector.Core;

namespace LinJector.Interface
{
    public interface IObjectGraphTemplate
    {
        public void WriteGraph(ContainerBuilder builder);
    }
}