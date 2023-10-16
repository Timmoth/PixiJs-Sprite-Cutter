using System.Text.Json.Serialization;

public class Pos
{
    [JsonPropertyName("x")]
    public required int X { get; set; }

    [JsonPropertyName("y")]
    public required int Y { get; set; }
}