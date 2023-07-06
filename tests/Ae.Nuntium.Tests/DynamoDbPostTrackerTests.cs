using Ae.Nuntium.Trackers;
using Amazon.DynamoDBv2;
using Xunit;

namespace Ae.Nuntium.Tests
{
    public sealed class DynamoDbPostTrackerTests
    {
        [Fact(Skip = "Needs access to DynamoDB")]
        public async Task TestPostsSeenUnseen()
        {
            var tracker = new DynamoDbPostTracker(new AmazonDynamoDBClient(), new DynamoDbPostTracker.Configuration
            {
                TableName = "ExtractedPostsTest"
            });

            var posts = ExtractedPostTestExtensions.FromJson(File.ReadAllText("Files/feed7.json"))
                .Select(x =>
                {
                    x.Permalink = new Uri("https://" + Guid.NewGuid().ToString());
                    return x;
                })
                .ToArray();

            await tracker.RemoveSeenPosts(posts, CancellationToken.None);

            Assert.Equal(posts, await tracker.GetUnseenPosts(posts, CancellationToken.None));

            await tracker.SetSeenPosts(posts, CancellationToken.None);

            Assert.Empty(await tracker.GetUnseenPosts(posts, CancellationToken.None));
        }
    }
}
