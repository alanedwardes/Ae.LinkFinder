using Ae.Nuntium.Extractors;
using Ae.Nuntium.Services;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace Ae.Nuntium.Enrichers
{
    public sealed class AmazonS3MediaCacheEnricher : IExtractedPostEnricher
    {
        private readonly ILogger<AmazonS3MediaCacheEnricher> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IAmazonS3 _storage;
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public string BucketName { get; set; }
        }

        public AmazonS3MediaCacheEnricher(ILogger<AmazonS3MediaCacheEnricher> logger, IHttpClientFactory clientFactory, IAmazonS3 storage, Configuration configuration)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _storage = storage;
            _configuration = configuration;
        }

        private IEnumerable<Uri> ExtractMediaFromPost(ExtractedPost post)
        {
            if (post.Avatar != null)
            {
                yield return post.Avatar;
            }

            if (post.Thumbnail != null)
            {
                yield return post.Thumbnail;
            }

            foreach (var mediaUri in post.Media)
            {
                yield return mediaUri;
            }
        }

        public async Task EnrichExtractedPosts(IList<ExtractedPost> posts, CancellationToken cancellation)
        {
            var uriMap = new Dictionary<Uri, Uri>();

            foreach (var mediaUri in posts.SelectMany(ExtractMediaFromPost).Distinct())
            {
                // Do this sync so as not to overload the downstream service
                uriMap.Add(mediaUri, await CacheMedia(mediaUri, cancellation));
            }

            foreach (var post in posts)
            {
                post.Media = post.Media.Select(x => uriMap[x]).ToHashSet();
                
                if (post.Avatar != null)
                {
                    post.Avatar = uriMap[post.Avatar];
                }

                if (post.Thumbnail != null)
                {
                    post.Thumbnail = uriMap[post.Thumbnail];
                }
            }
        }

        private async Task<Uri> CacheMedia(Uri mediaUri, CancellationToken cancellation)
        {
            var objectKey = Guid.NewGuid().ToString();

            using var httpClient = _clientFactory.CreateClient();

            using var response = await httpClient.GetAsync(mediaUri, cancellation);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Got {StatusCode} when attempting to download {MediaUri}", response.StatusCode, mediaUri);
                return mediaUri;
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellation);

            var putObjectResponse = await _storage.PutObjectAsync(new PutObjectRequest
            {
                ContentType = response.Content.Headers.ContentType?.ToString(),
                InputStream = stream,
                BucketName = _configuration.BucketName,
                CannedACL = S3CannedACL.PublicRead,
                Key = objectKey
            }, cancellation);

            // Generate a signed URL to easily resolve a full URL for the object,
            // but strip the crypto from the query parameters
            var preSignedUrl = _storage.GeneratePreSignedURL(_configuration.BucketName, objectKey, DateTime.UtcNow.AddDays(1), null);

            return new Uri(preSignedUrl).RemoveFragmentAndQueryString();
        }
    }
}
