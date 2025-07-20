public class ContentAnalysisRequestDTO
{
    public string Content { get; set; }
    public string ContentType { get; set; } // html, json, plain, image
    public Guid CustomerId { get; set; }
}   