using System.Text.Json.Serialization;

public class Config
{
    [JsonPropertyName("pretty-json")] public bool PrettyJson { get; set; } = false;

    [JsonPropertyName("sprite-size")]
    public required Size SpriteSize { get; set; }

    [JsonPropertyName("output-json-file")]
    public required string OutputJsonFile { get; set; }

    [JsonPropertyName("output-image-file")]
    public required string OutputImageFile { get; set; }

    [JsonPropertyName("output-image-cols")]
    public required int OutputCols { get; set; }

    [JsonPropertyName("files")]
    public required InputFile[] Files { get; set; }

    [JsonPropertyName("animation")]
    public required Animation[] Animations { get; set; }

}