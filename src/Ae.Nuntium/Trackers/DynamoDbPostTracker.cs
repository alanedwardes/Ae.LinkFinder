using Ae.Nuntium.Extractors;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Ae.Nuntium.Trackers
{
    public sealed class DynamoDbPostTracker : IPostTracker
    {
        private readonly IAmazonDynamoDB _dynamo;
        private readonly Configuration _configuration;

        public sealed class Configuration
        {
            public string TableName { get; set; }
        }

        public DynamoDbPostTracker(IAmazonDynamoDB dynamo, Configuration configuration)
        {
            _dynamo = dynamo;
            _configuration = configuration;
        }

        public async Task<IEnumerable<ExtractedPost>> GetUnseenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            var seen = (await Task.WhenAll(posts.Select(post => _dynamo.GetItemAsync(new GetItemRequest
            {
                TableName = _configuration.TableName,
                Key = new Dictionary<string, AttributeValue> { { "Permalink", new AttributeValue(post.Permalink.ToString()) } },
                ProjectionExpression = "Permalink"
            }, cancellation)))).Where(x => x.IsItemSet).Select(x => new Uri(x.Item["Permalink"].S));

            return posts.ExceptBy(seen, x => x.Permalink);
        }

        public async Task SetSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            await Task.WhenAll(posts.Select(post => _dynamo.PutItemAsync(_configuration.TableName, ExtractedPostToDictionary(post), cancellation)));
        }

        public async Task RemoveSeenPosts(IEnumerable<ExtractedPost> posts, CancellationToken cancellation)
        {
            await Task.WhenAll(posts.Select(post => _dynamo.DeleteItemAsync(_configuration.TableName, PermalinkToDictionary(post.Permalink), cancellation)));
        }

        private static Dictionary<string, AttributeValue> PermalinkToDictionary(Uri permalink)
        {
            return new Dictionary<string, AttributeValue>
            {
                { "Permalink", new AttributeValue(permalink.ToString()) }
            };
        }

        private static Dictionary<string, AttributeValue> ExtractedPostToDictionary(ExtractedPost extractedPost)
        {
            var json = PermalinkToDictionary(extractedPost.Permalink);

            if (extractedPost.Summary != null)
            {
                json.Add("TextSummary", new AttributeValue(extractedPost.Summary));
            }

            if (extractedPost.Body != null)
            {
                json.Add("RawContent", new AttributeValue(extractedPost.Body));
            }

            if (extractedPost.Author != null)
            {
                json.Add("Author", new AttributeValue(extractedPost.Author));
            }

            if (extractedPost.Published.HasValue)
            {
                json.Add("Published", new AttributeValue(extractedPost.Published.Value.ToString("O")));
            }

            return json;
        }
    }
}