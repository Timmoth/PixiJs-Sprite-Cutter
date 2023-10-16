## PixiJs-Sprite-Cutter

The sole function of this utility is to take a collection of sprite sheets, help you pick and choose the sprites to be used together as part of an animation then pack them together into a single sprite sheet & PixiJs texture json file.

This is a super quick and unpolished all be it very useful utility I put together for [Cloud Rec Room](https://cloudrecroom.com/). I didn't want to wrangle with another sprite cutter tool to assemble the sprite sheets exactly how I needed them for use in PixiJs, and god forbid I try to do the job manually. If there is any interest in the project I would consider cleaning it up / adding features.

### Usage

Once you've created your config file run
`dotnet run ./myconfig.json`

##### Example config
```
{
   "pretty-json":true,
   "sprite-size":{
      "w":24,
      "h":24
   },
   "output-image-file":"sprites.png",
   "output-json-file":"sprites.json",
   "output-image-cols":24,
   "files":[
      {
         "file":"./character1.png",
         "file-id":"d1",
         "sprite-size":{
            "w":24,
            "h":24
         },
         "sprite-sheet-size":{
            "w":576,
            "h":24
         }
      },
      {
         "file":"./character2.png",
         "file-id":"d2",
         "sprite-size":{
            "w":24,
            "h":24
         },
         "sprite-sheet-size":{
            "w":576,
            "h":24
         }
      }
   ],
   "animation":[
      {
         "name":"d1-idle",
         "sprites":[
            "d1-0-0-1-4"
         ]
      },
      {
         "name":"d1-walk",
         "sprites":[
            "d1-0-4-1-6",
            "d2-1-4-1-2"
         ]
      },
   ]
}
```

##### Example notes
```
{
   "pretty-json" // Make the json output more readable at the cost of size
   "sprite-size" // The size of the sprite
   "output-image-file" // The file to write the output image to
   "output-json-file" // The file to write the output json to
   "output-image-cols" // The number of columns in the output image (sprites per row)
   "files":[
      {
         "file": // The relative path to the source sprite sheet
         "file-id": // A unique prefix for sprites from this sheet (keep this short to reduce file size)
         "sprite-size": // The size of the sprites in this sheet
         "sprite-sheet-size": // The resolution of the sprite sheet
      }
   ],
   "animation":[
      {
         "name": // The animation name (to be referenced in pixijs)
         "sprites": // A list of sprite selectors that make up frames of the animation
      }
   ]
}
```

##### Sprite selection syntax
The syntax for selecting sprites from an input sprite sheet to be used as the frames in an animation follows this format:

`[fileId]-[row]-[col]-[rowspan]-[colspan]`
[fileId] - select the input sprite sheet
[row] - the row the animation sprites start at
[col] - the column the animation sprites start at
[rowspan] - how many rows of sprites should be used
[colspan] - how many columns of sprites should be used

This way it is easy to define entire blocks of sprites from an input sprite sheet to be used in an animation. Keep in mind you can specify multiple sprite selectors that pull sprites from different sheets to be used in the same animation E.G
```
"sprites":[
            "d1-0-4-1-6",
            "d2-1-4-1-2"
         ]
```

#### PixiJs
Here is a very simple PixJs snippet for loading the output file and displaying an animation.
```

 await Assets.load("sprites.json");

const animations = PIXI.Assets.cache.get(`sprites.json`).data.animations;
const sprite = PIXI.AnimatedSprite.fromFrames(
      this.animations[`d1-idle`]
    );
    sprite.animationSpeed = 1 / 6; // 6 fps
    sprite.anchor.set(0.5);
    sprite.scale.set(2, 2);
    sprite.x = width / 2 - sprite.width / 2;
    sprite.y = height / 2 - sprite.height / 2;
    sprite.texture.baseTexture.scaleMode = PIXI.SCALE_MODES.NEAREST;
    sprite.play();
    container.addChild(sprite);


```
