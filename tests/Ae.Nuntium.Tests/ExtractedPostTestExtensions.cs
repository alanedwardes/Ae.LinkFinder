using Ae.Nuntium.Extractors;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Ae.Nuntium.Tests;

public static class ExtractedPostTestExtensions
{
    public static string ToJson(this IEnumerable<ExtractedPost> posts)
    {
        return JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });
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
