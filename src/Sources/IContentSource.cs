namespace Ae.LinkFinder.Sources
{
    public interface IContentSource
    {
        Task<SourceDocument> GetContent(CancellationToken token);
    }
}