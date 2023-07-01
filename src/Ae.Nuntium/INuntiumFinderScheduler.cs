using Ae.Nuntium.Configuration;

namespace Ae.Nuntium
{
    public interface INuntiumFinderScheduler
    {
        Task Schedule(NuntiumConfiguration configuration, CancellationToken cancellation);
    }
}