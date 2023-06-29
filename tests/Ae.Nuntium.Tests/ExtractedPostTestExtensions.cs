using Ae.Nuntium.Extractors;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ae.Nuntium.Tests;

public static class ExtractedPostTestExtensions
{
    public static string ToJson(this IEnumerable<ExtractedPost> posts)
    {
        return JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });
    }
}
