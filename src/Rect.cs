using System.Text.Json.Serialization;

public record Rect
{
    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }

    [JsonPropertyName("w")]
    public required int W { get; set; }

    [JsonPropertyName("h")]
    public required int H { get; set; }
}