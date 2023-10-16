public class SpriteSelection
{
    public required string Selector { get; set; }
    public required string FileId { get; set; }
    public required int Row { get; set; }
    public required int Col { get; set; }
    public required int RowSpan { get; set; }
    public required int ColSpan { get; set; }

    public static SpriteSelection From(string spriteSelector)
    {
        var sections = spriteSelector.Split("-");
        return new SpriteSelection()
        {
            Selector = spriteSelector,
            FileId = sections[0],
            Row = int.Parse(sections[1]),
            Col = int.Parse(sections[2]),
            RowSpan = int.Parse(sections[3]),
            ColSpan = int.Parse(sections[4]),

        };
    }
}