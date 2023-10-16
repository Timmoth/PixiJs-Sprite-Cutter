using System.Text.Json.Serialization;

public class PixiJsSpriteSheet
{
    [JsonPropertyName("frames")]
    public required Dictionary<string, FrameData> Frames { get; set; }

    [JsonPropertyName("animations")]
    public required Dictionary<string, List<string>> AnimationData { get; set; }

    [JsonPropertyName("meta")]
    public required Meta Meta { get; set; }

    public static PixiJsSpriteSheet From(Config config)
    {
        return new PixiJsSpriteSheet()
        {
            AnimationData = new Dictionary<string, List<string>>()
            {

            },
            Frames = new Dictionary<string, FrameData>()
            {

            },
            Meta = new Meta()
            {
                Format = "RGBA8888",
                Image = config.OutputImageFile,
                Scale = "1",
                Size = new Size()
                {
                    W = config.SpriteSize.W,
                    H = config.SpriteSize.H
                }
            }
        };
    }
}