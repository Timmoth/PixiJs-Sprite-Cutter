using System.Text.Json.Serialization;

public class FrameData
{
    [JsonPropertyName("frame")]
    public required Rect Frame { get; set; }

    [JsonPropertyName("spriteSourceSize")]
    public required Rect SpriteSourceSize { get; set; }

    [JsonPropertyName("sourceSize")]
    public Size? SourceSize { get; set; }

    [JsonPropertyName("anchor")]
    public Pos? Anchor { get; set; }
}