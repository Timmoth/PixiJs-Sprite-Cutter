using System.Text.Json.Serialization;

public class InputFile
{
    [JsonPropertyName("file")]
    public required string File { get; set; }

    [JsonPropertyName("file-id")]
    public required string FileId { get; set; }

    [JsonPropertyName("sprite-size")]
    public required Size SpriteSize { get; set; }

    [JsonPropertyName("sprite-sheet-size")]
    public required Size SpriteSheetSize { get; set; }
}