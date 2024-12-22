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
    private Board SecondBoard;
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
        if (Config.ShowSecondBoard)
        {
            Graphics.PreferredBackBufferWidth = Config.GameBoardWidthBlocks * Config.BlockWidthPx * 2 + (4 * Config.BoardPaddingPx) + 300;
            Graphics.PreferredBackBufferHeight = Config.GameBoardHeightBlocks * Config.BlockHeightPx + (2 * Config.BoardPaddingPx);
        }
        else
        {
            Graphics.PreferredBackBufferWidth = Config.GameBoardWidthBlocks * Config.BlockWidthPx + (2 * Config.BoardPaddingPx) + 300;
            Graphics.PreferredBackBufferHeight = Config.GameBoardHeightBlocks * Config.BlockHeightPx + (2 * Config.BoardPaddingPx);
        }

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
        Board = new Board(
            width: Config.GameBoardWidthBlocks,
            height: Config.GameBoardHeightBlocks,
            drawLocation: new(x: Config.BoardPaddingPx, y: Config.BoardPaddingPx)
        );

        if (Config.ShowSecondBoard)
        {
            SecondBoard = new Board(
                width: Config.GameBoardWidthBlocks,
                height: Config.GameBoardHeightBlocks,
                drawLocation: new(
                    x: Graphics.PreferredBackBufferWidth - Config.BoardPaddingPx - (Config.GameBoardWidthBlocks * Config.BlockWidthPx),
                    y: Config.BoardPaddingPx
                )
            );
        }

        Board.SpawnTetronimo();
        SecondBoard?.SpawnTetronimo();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Board.Update(gameTime);
        SecondBoard?.Update(gameTime);
        base.Update(gameTime);

        if (!Board.HaveLost && MediaPlayer.State != MediaState.Playing)
        {
            MediaPlayer.Play(Song);
        }
        else if (Board.HaveLost)
        {
            MediaPlayer.Stop();
        }
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        State.SpriteBatch.Begin();

        Board.Draw(gameTime);
        SecondBoard?.Draw(gameTime);
        State.SpriteBatch.DrawString(
            spriteFont: State.Sprites.Font,
            text: $"Score: {State.Score}",
            position: new Vector2(
                x: Config.GameBoardWidthBlocks * Config.BlockWidthPx + (2 * Config.BoardPaddingPx) + 20,
                y: 100
            ),
            color: Color.White
        );
        State.SpriteBatch.DrawString(
            spriteFont: State.Sprites.Font,
            text: $"Speed: {Config.TetronimoBaseVerticalSpeedBlocksPerSecond(State.Score)}",
            position: new Vector2(
                x: Config.GameBoardWidthBlocks * Config.BlockWidthPx + (2 * Config.BoardPaddingPx) + 20,
                y: 140
            ),
            color: Color.White
        );
        base.Draw(gameTime);

        State.SpriteBatch.End();
    }
}
