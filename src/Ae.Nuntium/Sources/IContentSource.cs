namespace Ae.Nuntium.Sources
{
    public interface IContentSource
    {
        Task<SourceDocument> GetContent(CancellationToken cancellation);
    }
}