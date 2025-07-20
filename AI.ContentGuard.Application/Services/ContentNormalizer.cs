public class ContentNormalizer
{
    public string Normalize(string content, string contentType)
    {
        return contentType switch
        {
            "html" => StripHtml(content),
            "json" => NormalizeJson(content),
            "plain" => content,
            _ => throw new ValidationException("Unknown content type")
        };
    }

    private string StripHtml(string html)
    {
        // Basic HTML tag removal
        return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
    }

    private string NormalizeJson(string json)
    {
        // Minify JSON
        var obj = System.Text.Json.JsonSerializer.Deserialize<object>(json);
        return System.Text.Json.JsonSerializer.Serialize(obj);
    }
}