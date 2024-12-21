using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Graphics;

namespace Tetris;

public class Game1 : Game
{
    private GraphicsDeviceManager Graphics;
    private Board Board;
    private Song Song;

    public Game1()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Graphics.IsFullScreen = false;
        Graphics.PreferredBackBufferWidth = Config.GameBoardWidthBlocks * Config.BlockWidthPx + (2 * Config.BoardPaddingPx);
        Graphics.PreferredBackBufferHeight = Config.GameBoardHeightBlocks * Config.BlockHeightPx + (2 * Config.BoardPaddingPx);
        Graphics.ApplyChanges();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        State.GraphicsDevice = GraphicsDevice;
        State.SpriteBatch = new SpriteBatch(State.GraphicsDevice);

        Song = Content.Load<Song>("tetris");
        State.SoundEffects.Load(Content);
        State.Sprites.Load(Content);
        Board = new Board(width: Config.GameBoardWidthBlocks, height: Config.GameBoardHeightBlocks);
        Board.SpawnTetronimo();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Board.Update(gameTime);
        base.Update(gameTime);

        if (MediaPlayer.State != MediaState.Playing)
        {
            MediaPlayer.Play(Song);
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.BlanchedAlmond);

        State.SpriteBatch.Begin();
        
        Board.Draw(gameTime);
        base.Draw(gameTime);

        State.SpriteBatch.End();
    }
}
