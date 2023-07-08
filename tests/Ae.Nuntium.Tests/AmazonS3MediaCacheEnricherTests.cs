using Ae.Nuntium.Enrichers;
using Ae.Nuntium.Extractors;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Net;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class AmazonS3MediaCacheEnricherTests : IDisposable
    {
        private readonly MockRepository _repository = new(MockBehavior.Strict);

        public void Dispose() => _repository.VerifyAll();

        [Fact]
        public async Task TestMediaReplacement()
        {
            var mockStorage = _repository.Create<IAmazonS3>();

            var mockStream = new MemoryStream(new byte[] { 1, 2, 3 });

            var media1 = new Uri("https://www.example.com/media.jpeg");
            var media2 = new Uri("https://www.example.net/media.png");

            var enricher = new AmazonS3MediaCacheEnricher(NullLogger<AmazonS3MediaCacheEnricher>.Instance, new MockHttpClientFactory(request =>
            {
                if (request.RequestUri == media1)
                {
                    return new HttpResponseMessage
                    {
                        Content = new StreamContent(mockStream)
                        {
                            Headers = { { "Content-Type", "image/jpeg" } }
                        }
                    };
                }

                if (request.RequestUri == media2)
                {
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }

                throw new NotImplementedException();
            }), mockStorage.Object, new AmazonS3MediaCacheEnricher.Configuration
            {
                BucketName = "bucketName"
            });

            mockStorage.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), CancellationToken.None))
                .Callback<PutObjectRequest, CancellationToken>((request, cancellation) =>
                {
                    var ms = new MemoryStream();
                    request.InputStream.CopyTo(ms);
                    ms.Position = 0;

                    Assert.Equal("image/jpeg", request.ContentType);
                    Assert.EndsWith(".jpeg", request.Key);
                    Assert.Equal(new byte[] { 1, 2, 3 }, ms.ToArray());
                })
                .ReturnsAsync(new PutObjectResponse());

            mockStorage.Setup(x => x.GeneratePreSignedURL("bucketName", It.IsAny<string>(), It.IsAny<DateTime>(), null))
                       .Returns("https://www.example.com/replaced?signedurlstuff=true&wibble=false");

            var posts = new List<ExtractedPost>
            {
                new ExtractedPost(new Uri("https://www.example.com/"))
                {
                    Media = new HashSet<Uri> { media1, media2 }
                }
            };

            await enricher.EnrichExtractedPosts(posts, CancellationToken.None);

            // This media got replaced
            Assert.Equal(new Uri("https://www.example.com/replaced"), posts[0].Media.First());

            // This errored and didn't
            Assert.Equal(media2, posts[0].Media.Last());
        }
    }
}
