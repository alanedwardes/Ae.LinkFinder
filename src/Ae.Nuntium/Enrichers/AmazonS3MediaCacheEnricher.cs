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
            /// <summary>
            /// The name of the S3 bucket.
            /// </summary>
            public string BucketName { get; set; }
            /// <summary>
            /// The canned ACL to use when uploading the object.
            /// </summary>
            public string CannedAcl { get; set; } = "NoACL";
            /// <summary>
            /// Format string to use when constructing the object key
            /// {0} is a new guid
            /// </summary>
            public string KeyFormat { get; set; } = "{0}";
            /// <summary>
            /// Format string to use when constructing the final URI
            /// {0} is the full URL to the object in S3
            /// {1} is the object key
            /// </summary>
            public string UriFormat { get; set; } = "{0}";
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
                try
                {
                    // Do this sync so as not to overload the downstream service
                    uriMap.Add(mediaUri, await CacheMedia(mediaUri, cancellation));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Got exception when attempting to cache {MediaUri}", mediaUri);
                    uriMap.Add(mediaUri, mediaUri);
                }
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
            var objectKey = string.Format(_configuration.KeyFormat, Guid.NewGuid().ToString());

            using var httpClient = _clientFactory.CreateClient("GZIP_CLIENT");
            using var response = await httpClient.GetAsync(mediaUri, cancellation);
            using var stream = await response.Content.ReadAsStreamAsync(cancellation);

            var putObjectResponse = await _storage.PutObjectAsync(new PutObjectRequest
            {
                ContentType = response.Content.Headers.ContentType?.ToString(),
                InputStream = stream,
                BucketName = _configuration.BucketName,
                CannedACL = S3CannedACL.FindValue(_configuration.CannedAcl),
                Key = objectKey
            }, cancellation);

            // Generate a signed URL to easily resolve a full URL for the object,
            // but strip the crypto from the query parameters
            var preSignedUrl = _storage.GeneratePreSignedURL(_configuration.BucketName, objectKey, DateTime.UtcNow.AddDays(1), null);
            var objectUri = new Uri(preSignedUrl).RemoveFragmentAndQueryString();

            return new Uri(string.Format(_configuration.UriFormat, objectUri, objectKey));
        }
    }
}
