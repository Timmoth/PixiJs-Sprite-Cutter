public abstract class SpriteSelection
{
    public required string Selector { get; set; }
    public required string FileId { get; set; }

    public static SpriteSelection From(string spriteSelector)
    {
        if (spriteSelector.Contains("all"))
        {
            return SpriteSelectionAll.From(spriteSelector);
        }

        if (spriteSelector.Split("-").Length == 3)
        {
            return SpriteSelectionSingle.From(spriteSelector);
        }

        return SpriteSelectionRegion.From(spriteSelector);
    }

    public abstract List<Rect> GetReferencedSprites(Size spriteSize, InputFile inputFile);
}

public class SpriteSelectionAll : SpriteSelection
{
    public static SpriteSelection From(string spriteSelector)
    {
        var sections = spriteSelector.Split("-");
        return new SpriteSelectionAll()
        {
            Selector = spriteSelector,
            FileId = sections[0],
        };
    }

    public override List<Rect> GetReferencedSprites(Size spriteSize, InputFile inputFile)
    {
        var referencedSprites = new List<Rect>();
        // Split the sprite sheet and rearrange the sprites
        for (var row = 0; row < inputFile.SpriteSheetSize.H / spriteSize.H; row++)
        {
            for (var col = 0; col < inputFile.SpriteSheetSize.W / spriteSize.W; col++)
            {
                var x = col * spriteSize.W;
                var y = row * spriteSize.H;

                referencedSprites.Add(new Rect()
                {
                    X = x,
                    Y = y,
                    W = spriteSize.W,
                    H = spriteSize.H
                });
            }
        }

        return referencedSprites;
    }
}


public class SpriteSelectionSingle : SpriteSelection
{
    public required int Row { get; set; }
    public required int Col { get; set; }

    public static SpriteSelection From(string spriteSelector)
    {
        var sections = spriteSelector.Split("-");
        return new SpriteSelectionSingle()
        {
            Selector = spriteSelector,
            FileId = sections[0],
            Row = int.Parse(sections[1]),
            Col = int.Parse(sections[2]),
        };
    }

    public override List<Rect> GetReferencedSprites(Size spriteSize, InputFile inputFile)
    {
        var referencedSprites = new List<Rect>();

        var x = Col * spriteSize.W;
        var y = Row * spriteSize.H;

        referencedSprites.Add(new Rect()
        {
            X = x,
            Y = y,
            W = spriteSize.W,
            H = spriteSize.H
        });

        return referencedSprites;
    }
}

public class SpriteSelectionRegion : SpriteSelection
{
    public required int Row { get; set; }
    public required int Col { get; set; }
    public required int RowSpan { get; set; }
    public required int ColSpan { get; set; }

    public static SpriteSelection From(string spriteSelector)
    {
        var sections = spriteSelector.Split("-");
        return new SpriteSelectionRegion()
        {
            Selector = spriteSelector,
            FileId = sections[0],
            Row = int.Parse(sections[1]),
            Col = int.Parse(sections[2]),
            RowSpan = int.Parse(sections[3]),
            ColSpan = int.Parse(sections[4]),
        };
    }

    public override List<Rect> GetReferencedSprites(Size spriteSize, InputFile inputFile)
    {
        var referencedSprites = new List<Rect>();

        // Split the sprite sheet and rearrange the sprites
        for (var row = Row; row < Row + RowSpan; row++)
        {
            for (var col = Col; col < Col + ColSpan; col++)
            {
                var x = col * spriteSize.W;
                var y = row * spriteSize.H;

                referencedSprites.Add(new Rect()
                {
                    X = x,
                    Y = y,
                    W = spriteSize.W,
                    H = spriteSize.H
                });
            }
        }

        return referencedSprites;
    }
}