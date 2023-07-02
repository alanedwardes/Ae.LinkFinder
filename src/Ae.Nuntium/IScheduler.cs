using Ae.Nuntium.Configuration;

namespace Ae.Nuntium
{
    public interface IScheduler
    {
        Task Schedule(NuntiumConfiguration configuration, CancellationToken cancellation);
    }
}