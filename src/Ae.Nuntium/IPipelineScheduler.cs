using Ae.Nuntium.Configuration;

namespace Ae.Nuntium
{
    public interface IPipelineScheduler
    {
        Task Schedule(MainConfiguration configuration, CancellationToken cancellation);
    }
}