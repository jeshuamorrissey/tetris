using System;
using AsepriteDotNet.Aseprite;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;

namespace Tetris;

public class State
{
    public static SpriteBatch SpriteBatch { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static Random Random { get; } = new Random();
    public static int Score { get; set; } = 0;

    public class SoundEffects
    {
        public static SoundEffect Click { get; private set; }
        public static SoundEffect ClearRow { get; private set; }

        public static void Load(ContentManager content)
        {
            Click = content.Load<SoundEffect>("click");
            ClearRow = content.Load<SoundEffect>("clear");
        }
    }

    public class Sprites
    {
        public static AnimatedTilemap[] ActiveAnimations { get; private set; }
        public static Sprite RedBlock { get; private set; }
        public static Sprite LightBlueBlock { get; private set; }
        public static Sprite DarkBlueBlock { get; private set; }
        public static SpriteSheet ClickAnimation { get; private set; }
        public static SpriteFont Font { get; private set; }

        public static void Load(ContentManager content)
        {
            AsepriteFile blockSprites = content.Load<AsepriteFile>("sprites/blocks");
            SpriteSheet blockSheet = blockSprites.CreateSpriteSheet(GraphicsDevice, onlyVisibleLayers: true);

            RedBlock = blockSheet.CreateSprite(0);
            LightBlueBlock = blockSheet.CreateSprite(1);
            DarkBlueBlock = blockSheet.CreateSprite(2);

            AsepriteFile clickAnimationFiles = content.Load<AsepriteFile>("sprites/click_animation");
            ClickAnimation = clickAnimationFiles.CreateSpriteSheet(GraphicsDevice, onlyVisibleLayers: true);

            Font = content.Load<SpriteFont>("Arial");
        }
    }
}
