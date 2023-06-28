namespace Ae.Nuntium.Extractors
{
    public sealed class UriExtensions
    {
        public static bool TryCreateAbsoluteUri(string url, Uri baseAddress, out Uri newUri)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                newUri = uri;
                return true;
            }

            if (Uri.TryCreate(baseAddress, url, out var uri1))
            {
                newUri = uri1;
                return true;
            }

            newUri = null;
            return false;
        }
    }
}
