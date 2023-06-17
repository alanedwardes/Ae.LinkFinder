namespace Ae.LinkFinder.Sources
{
    public interface ILinkSource
    {
        Task<ISet<Uri>> GetLinks(CancellationToken token);
    }
}