using System.Text.Json.Serialization;

public class Size
{
    [JsonPropertyName("w")]
    public required int W { get; set; }

    [JsonPropertyName("h")]
    public required int H { get; set; }
}