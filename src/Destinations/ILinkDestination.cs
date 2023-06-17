namespace Ae.LinkFinder.Destinations
{
    public interface ILinkDestination
    {
        Task PostLinks(ISet<Uri> links);
    }
}