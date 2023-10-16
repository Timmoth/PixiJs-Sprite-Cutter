// See https://aka.ms/new-console-template for more information

using ImageMagick;
using System.Text.Json;
using System.Text.Json.Serialization;

if (args.Length == 0)
{
    Console.Error.WriteLine($"Please provide a file path as a command-line argument.");
    Environment.ExitCode = -1;
    return;
}

var configFileName = args[0];

if (!File.Exists(configFileName))
{
    Console.Error.WriteLine($"could not find input file '{configFileName}'");
    Environment.ExitCode = -1;
    return;
}

Console.WriteLine($"reading '{configFileName}'");
var configJsonString = File.ReadAllText(configFileName);
var config = JsonSerializer.Deserialize<Config>(configJsonString);

if (config == null)
{
    Console.Error.WriteLine($"could not parse '{configFileName}'");
    Environment.ExitCode = -1;
    return;
}

var pixiJsSpriteSheet = PixiJsSpriteSheet.From(config);

Console.WriteLine("loading input sprite sheets");
// Load all input sprite sheets
var inputSpriteSheetCollection = new Dictionary<string, (InputFile InputFile, MagickImage Image)>();
foreach (var inputFile in config.Files)
{
    try
    {
        var image = new MagickImage(inputFile.File);
        inputSpriteSheetCollection.Add(inputFile.FileId, (inputFile, image));
    }
    catch 
    {
        Console.Error.WriteLine($"could not read sprite sheet '{inputFile.FileId}' - '{inputFile.File}'");
        Environment.ExitCode = -1;
        return;
    }
}

// Determine all referenced sprites
var referencedSprites = new Dictionary<string, ReferencedSprite>();
var inputFileSpriteCounts = new Dictionary<string, int>();
foreach (var inputDataAnimation in config.Animations)
{
    var animationSpriteIds = new List<string>();
    foreach (var spriteSelector in inputDataAnimation.SpriteSelector)
    {
        var spriteSelection = SpriteSelection.From(spriteSelector);

        if (!inputSpriteSheetCollection.TryGetValue(spriteSelection.FileId, out var input))
        {
            Console.Error.WriteLine($"error, could not find input file with FileId '{spriteSelection.FileId}' when processing animation '{inputDataAnimation.Name}'");
            Environment.ExitCode = -1;
            return;
        }

        var (inputFile, _) = input;

        // Split the sprite sheet and rearrange the sprites
        for (var row = spriteSelection.Row; row < spriteSelection.Row + spriteSelection.RowSpan; row++)
        {
            for (var col = spriteSelection.Col; col < spriteSelection.Col + spriteSelection.ColSpan; col++)
            {
                var x = col * config.SpriteSize.W;
                var y = row * config.SpriteSize.H;
                var spriteId = $"{inputFile.FileId}-{x}-{y}-{config.SpriteSize.W}-{config.SpriteSize.H}";

                if (!referencedSprites.TryGetValue(spriteId, out var referencedSprite))
                {
                    if (inputFileSpriteCounts.TryGetValue(inputFile.FileId, out var count))
                    {
                        count++;
                    }
                    else
                    {
                        count = 0;
                    }
                    inputFileSpriteCounts[inputFile.FileId] = count;

                    referencedSprite = new ReferencedSprite()
                    {
                        FileSpriteIndex = count,
                        SpriteId = spriteId,
                        FileId = inputFile.FileId,
                        Rect = new Rect()
                        {
                            X = x,
                            Y = y,
                            W = config.SpriteSize.W,
                            H = config.SpriteSize.H
                        }
                    };
                    referencedSprites.Add(referencedSprite.SpriteId, referencedSprite);
                }

                animationSpriteIds.Add(referencedSprite.FileSpriteId);
            }
        }
    }

    pixiJsSpriteSheet.AnimationData.Add(inputDataAnimation.Name, animationSpriteIds);
}


Console.WriteLine("extracting sprites");

// Extract all sprites from input files
using var sprites = new MagickImageCollection();

var currentOutputSpriteRow = 0;
var currentOutputSpriteCol = 0;

foreach (var spriteGroup in referencedSprites.GroupBy(s => s.Value.FileId))
{
    if (!inputSpriteSheetCollection.TryGetValue(spriteGroup.Key, out var input))
    {
        Console.Error.WriteLine($"error, referenced FileId '{spriteGroup.Key}' was not defined in the 'files' array.");
        Environment.ExitCode = -1;
        return;
    }

    var (inputFile, image) = input;

    foreach (var referencedSprite in spriteGroup.Select(kvp => kvp.Value).OrderBy(s => s.FileSpriteIndex))
    {
        try
        {
            // Add the frame to the output json
            pixiJsSpriteSheet.Frames.Add(referencedSprite.FileSpriteId, new FrameData()
            {
                Frame = new Rect()
                {
                    X = currentOutputSpriteCol * referencedSprite.Rect.W,
                    Y = currentOutputSpriteRow * referencedSprite.Rect.H,
                    W = referencedSprite.Rect.W,
                    H = referencedSprite.Rect.H,
                },
                SpriteSourceSize = new Rect()
                {
                    X = 0,
                    Y = 0,
                    W = referencedSprite.Rect.W,
                    H = referencedSprite.Rect.H,
                }
            });

            var sprite = image.Clone(new MagickGeometry(referencedSprite.Rect.X, referencedSprite.Rect.Y, referencedSprite.Rect.W, referencedSprite.Rect.H));
            sprites.Add(sprite);
        }
        catch
        {
            Console.Error.WriteLine($"error extracting sprite '{referencedSprite.SpriteId}' / [{referencedSprite.Rect.X}, {referencedSprite.Rect.Y}, {referencedSprite.Rect.W}, {referencedSprite.Rect.H}] from sheet '{inputFile.FileId}' - '{inputFile.File}'");
            Environment.ExitCode = -1;
            return;
        }

        currentOutputSpriteCol++;
        if (currentOutputSpriteCol >= config.OutputCols)
        {
            currentOutputSpriteCol = 0;
            currentOutputSpriteRow++;
        }
    }

    // Add blank sprites
    var blankSprites = config.OutputCols - (spriteGroup.Count() % config.OutputCols);
    for (var i = 0; i < blankSprites; i++)
    {
        var blankSprite = new MagickImage(MagickColor.FromRgba(0,0,0,0), inputFile.SpriteSize.W, inputFile.SpriteSize.H);
        sprites.Add(blankSprite);

        currentOutputSpriteCol++;
        if (currentOutputSpriteCol >= config.OutputCols)
        {
            currentOutputSpriteCol = 0;
            currentOutputSpriteRow++;
        }
    }
}


try
{
    var outputRows = (int)Math.Ceiling(sprites.Count / (float)config.OutputCols);

    var spriteSheet = sprites.Montage(new MontageSettings
    {
        Geometry = new MagickGeometry(config.SpriteSize.W, config.SpriteSize.H),
        TileGeometry = new MagickGeometry(config.OutputCols, outputRows),
        BackgroundColor = MagickColors.None,
    });

    spriteSheet.Write(config.OutputImageFile);
}
catch
{
    Console.Error.WriteLine($"error saving image '{config.OutputImageFile}'");
    Environment.ExitCode = -1;
    return;
}


try
{
    // Write the JSON string to a file
    var options = new JsonSerializerOptions
    {
        WriteIndented = config.PrettyJson // Indent the JSON for readability
        ,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    File.WriteAllText(config.OutputJsonFile, JsonSerializer.Serialize(pixiJsSpriteSheet, options));
}
catch
{
    Console.Error.WriteLine($"error saving json '{config.OutputJsonFile}'");
    Environment.ExitCode = -1;
    return;
}

Console.WriteLine("done!");