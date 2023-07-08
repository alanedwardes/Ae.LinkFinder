namespace Ae.Nuntium.Services
{
    public static class UriExtensions
    {
        public static bool TryCreateAbsoluteUri(string url, Uri baseAddress, out Uri newUri)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                newUri = new Uri(url);
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

        public static Uri RemoveFragmentAndQueryString(this Uri uri)
        {
            return new UriBuilder(uri)
            {
                Fragment = null,
                Query = null,
                Port = -1
            }.Uri;
        }
    }
}
