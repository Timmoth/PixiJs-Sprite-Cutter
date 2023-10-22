public class ReferencedSprite
{
    public required string FileId { get; set; }
    public required int FileSpriteIndex { get; set; }
    public required Rect Rect { get; set; }

    public string FileSpriteId => $"{FileId}-{FileSpriteIndex}";

}