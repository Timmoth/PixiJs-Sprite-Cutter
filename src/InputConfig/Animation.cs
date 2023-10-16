using System.Text.Json.Serialization;

public class Animation
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("sprites")]
    public required string[] SpriteSelector { get; set; }
}