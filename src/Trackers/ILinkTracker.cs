namespace Ae.LinkFinder.Trackers
{
    public interface ILinkTracker
    {
        Task<ISet<Uri>> GetUnseenLinks(ISet<Uri> links, CancellationToken token);
        Task SetLinksSeen(ISet<Uri> links, CancellationToken token);
    }
}