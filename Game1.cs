using Gum.DataTypes;
using Gum.Wireframe;
using GumRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Content;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input;
using RenderingLibrary;

namespace Tetris;

public class Game1 : Game
{
    private Board Board;
    private Board SecondBoard;
    private Song Song;
    private TitleScreen TitleScreen;
    private GumProjectSave GumProject;
    private GraphicalUiElement Root;

    public Game1()
    {
        State.Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        GumProject = MonoGameGum.GumService.Default.Initialize(GraphicsDevice, "ui/tetris.gumx");
        GumProject.LocalizationFile = "Content/ui/strings.csv";
        GumProject.ShowLocalizationInGum = true;
        State.Graphics.IsFullScreen = false;
        State.Graphics.PreferredBackBufferWidth = Config.GameBoardWidthBlocks * Config.BlockWidthPx * 2 + (4 * Config.BoardPaddingPx) + 300;
        State.Graphics.PreferredBackBufferHeight = Config.GameBoardHeightBlocks * Config.BlockHeightPx + (2 * Config.BoardPaddingPx);
        State.Graphics.ApplyChanges();

        Localization.Initialize(Content);

        TitleScreen = new TitleScreen(
            gumProject: GumProject,
            onStartNewGame: () =>
            {
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
                            x: State.Graphics.PreferredBackBufferWidth - Config.BoardPaddingPx - (Config.GameBoardWidthBlocks * Config.BlockWidthPx),
                            y: Config.BoardPaddingPx
                        )
                    );

                    SecondBoard.SpawnTetronimo();
                }

                Board.SpawnTetronimo();
            },

            onExit: Exit
        );
        TitleScreen.Show();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        State.GraphicsDevice = GraphicsDevice;
        State.SpriteBatch = new SpriteBatch(State.GraphicsDevice);

        Song = Content.Load<Song>("tetris");
        State.SoundEffects.Load(Content);
        State.Sprites.Load(Content);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        MouseExtended.Update();

        if (Board == null)
        {
            MonoGameGum.GumService.Default.Update(this, gameTime, [State.GumRoot]);
        }
        else
        {
            Board.Update(gameTime);
            SecondBoard?.Update(gameTime);
            base.Update(gameTime);

            if ((!Board.HaveLost || (SecondBoard != null && !SecondBoard.HaveLost)) && MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Play(Song);
            }
            else if (Board.HaveLost && (SecondBoard == null || SecondBoard.HaveLost))
            {
                MediaPlayer.Stop();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        State.SpriteBatch.Begin();

        if (Board == null)
        {
            MonoGameGum.GumService.Default.Draw();
        }
        else
        {
            Board.Draw(gameTime);
            SecondBoard?.Draw(gameTime);
            State.SpriteBatch.DrawString(
                spriteFont: State.Sprites.Font,
                text: string.Format(Localization.Strings.GameScore, State.Score),
                position: new Vector2(
                    x: Config.GameBoardWidthBlocks * Config.BlockWidthPx + (2 * Config.BoardPaddingPx) + 20,
                    y: 100
                ),
                color: Color.White
            );
            State.SpriteBatch.DrawString(
                spriteFont: State.Sprites.Font,
                text: string.Format(Localization.Strings.GameSpeed, Config.TetronimoBaseVerticalSpeedBlocksPerSecond(State.Score)),
                position: new Vector2(
                    x: Config.GameBoardWidthBlocks * Config.BlockWidthPx + (2 * Config.BoardPaddingPx) + 20,
                    y: 140
                ),
                color: Color.White
            );
        }

        base.Draw(gameTime);

        State.SpriteBatch.End();
    }
}
