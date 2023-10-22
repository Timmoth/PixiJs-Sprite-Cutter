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

var deserializerOptions = new JsonSerializerOptions()
{
    AllowTrailingCommas = true
};

var config = JsonSerializer.Deserialize<Config>(configJsonString, deserializerOptions);

if (config == null)
{
    Console.Error.WriteLine($"could not parse '{configFileName}'");
    Environment.ExitCode = -1;
    return;
}

var pixiJsSpriteSheet = PixiJsSpriteSheet.From(config);

Console.WriteLine("loading input sprite sheets");
// Load all input sprite sheets
var inputSpriteSheetCollection = new Dictionary<string, InputFile>();
foreach (var inputFile in config.Files)
{
    try
    {
        if (inputFile.FileId.Contains("-"))
        {
            Console.Error.WriteLine($"can not use '-' in input file id '{inputFile.FileId}' - '{inputFile.File}'");
            Environment.ExitCode = -1;
            return;
        }

        var image = new MagickImage(inputFile.File);
        inputFile.Image = image;
        inputSpriteSheetCollection.Add(inputFile.FileId, inputFile);
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

        if (!inputSpriteSheetCollection.TryGetValue(spriteSelection.FileId, out var inputFile))
        {
            Console.Error.WriteLine($"error, could not find input file with FileId '{spriteSelection.FileId}' when processing animation '{inputDataAnimation.Name}'");
            Environment.ExitCode = -1;
            return;
        }

        var regions = spriteSelection.GetReferencedSprites(config.SpriteSize, inputFile);
        foreach (var region in regions)
        {
            var spriteId = $"{inputFile.FileId}-{region.X}-{region.Y}-{config.SpriteSize.W}-{config.SpriteSize.H}";
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
                    FileId = inputFile.FileId,
                    Rect = region
                };
                referencedSprites.Add(spriteId, referencedSprite);
            }

            animationSpriteIds.Add(referencedSprite.FileSpriteId);
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
    if (!inputSpriteSheetCollection.TryGetValue(spriteGroup.Key, out var inputFile))
    {
        Console.Error.WriteLine($"error, referenced FileId '{spriteGroup.Key}' was not defined in the 'files' array.");
        Environment.ExitCode = -1;
        return;
    }

    foreach (var referencedSprite in spriteGroup.Select(kvp => kvp.Value).OrderBy(s => s.FileSpriteIndex))
    {
        try
        {
            // Add the frame to the output json
            pixiJsSpriteSheet.Frames.Add(referencedSprite.FileSpriteId, new FrameData()
            {
                Frame = referencedSprite.Rect with { X = currentOutputSpriteCol * referencedSprite.Rect.W, Y = currentOutputSpriteRow * referencedSprite.Rect.H },
                SpriteSourceSize = referencedSprite.Rect with { X = 0, Y = 0 }
            });

            var sprite = inputFile.Image.Clone(new MagickGeometry(referencedSprite.Rect.X, referencedSprite.Rect.Y, referencedSprite.Rect.W, referencedSprite.Rect.H));
            sprites.Add(sprite);
        }
        catch
        {
            Console.Error.WriteLine($"error extracting sprite [{referencedSprite.Rect.X}, {referencedSprite.Rect.Y}, {referencedSprite.Rect.W}, {referencedSprite.Rect.H}] from sheet '{inputFile.FileId}' - '{inputFile.File}'");
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
        var blankSprite = new MagickImage(MagickColor.FromRgba(0,0,0,0), config.SpriteSize.W, config.SpriteSize.H);
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