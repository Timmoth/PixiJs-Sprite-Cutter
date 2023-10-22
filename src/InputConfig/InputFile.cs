using ImageMagick;
using System.Text.Json.Serialization;
using static System.Net.Mime.MediaTypeNames;

public class InputFile
{
    [JsonPropertyName("file")]
    public required string File { get; set; }

    [JsonPropertyName("file-id")]
    public required string FileId { get; set; }


    [JsonIgnore] 
    public MagickImage Image { get; set; } = null!;

    [JsonIgnore]
    public Size SpriteSheetSize => new()
    {
        W = Image.Width,
        H = Image.Height,
    };
}