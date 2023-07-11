using Ae.Nuntium.Extractors;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Ae.Nuntium.Tests;

public static class ExtractedPostTestExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public static string ToJson(this IEnumerable<ExtractedPost> posts)
    {
        return JsonSerializer.Serialize(posts, _options).Replace("\\r\\n", "\\n");
    }

    public static IEnumerable<ExtractedPost> FromJson(string json)
    {
        return JsonSerializer.Deserialize<IEnumerable<ExtractedPost>>(json, _options);
    }

    public static void Compare(this IEnumerable<ExtractedPost> posts, string fileName)
    {
        // Used to regenerate the baseline
        if (false)
        {
            var baselineFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../", "../", "../");
            File.WriteAllText(Path.Combine(baselineFilePath, fileName), posts.ToJson());

            throw new Exception("Regenerated baseline");
        }

        Assert.Equal(File.ReadAllText(fileName), posts.ToJson());
    }
}
