using Ae.Nuntium.Extractors;
using Ae.Nuntium.Trackers;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class FilePostTrackerTests
    {
        [Fact]
        public async Task TestFilePostTracker()
        {
            var file = new FileInfo(Path.GetTempFileName());

            try
            {
                var tracker = new FilePostTracker(new FilePostTracker.Configuration { File = file.FullName });

                var posts = new List<ExtractedPost>
                {
                    new ExtractedPost(new Uri("https://www.example.com/")),
                    new ExtractedPost(new Uri("https://www.example.org/")),
                    new ExtractedPost(new Uri("https://www.example.net/"))
                };

                Assert.Equal(posts, await tracker.GetUnseenPosts(posts, CancellationToken.None));

                await tracker.SetSeenPosts(posts.Skip(0).Take(1), CancellationToken.None);

                Assert.Equal(posts.Skip(1), await tracker.GetUnseenPosts(posts, CancellationToken.None));

                await tracker.SetSeenPosts(posts.Skip(1).Take(1), CancellationToken.None);

                Assert.Equal(posts.Skip(2), await tracker.GetUnseenPosts(posts, CancellationToken.None));

                await tracker.SetSeenPosts(posts.Skip(2).Take(1), CancellationToken.None);

                Assert.Empty(await tracker.GetUnseenPosts(posts, CancellationToken.None));
            }
            finally
            {
                file.Delete();
            }
        }
    }
}
