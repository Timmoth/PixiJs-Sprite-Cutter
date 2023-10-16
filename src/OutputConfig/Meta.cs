using System.Text.Json.Serialization;

public class Meta
{
    [JsonPropertyName("image")]
    public required string Image { get; set; }

    [JsonPropertyName("format")]
    public required string Format { get; set; }

    [JsonPropertyName("size")]
    public required Size Size { get; set; }

    [JsonPropertyName("scale")]
    public required string Scale { get; set; }
}