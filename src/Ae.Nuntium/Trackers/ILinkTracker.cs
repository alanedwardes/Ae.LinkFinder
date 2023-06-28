namespace Ae.Nuntium.Trackers
{
    public interface ILinkTracker
    {
        Task<IEnumerable<Uri>> GetUnseenLinks(IEnumerable<Uri> links, CancellationToken token);
        Task SetLinksSeen(IEnumerable<Uri> links, CancellationToken token);
    }
}