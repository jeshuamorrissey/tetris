using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Tetris;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private Board Board;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        State.SpriteBatch = new SpriteBatch(GraphicsDevice);
        Board = new Board(width: Config.GameBoardWidthBlocks, height: Config.GameBoardHeightBlocks);
        Board.SpawnTetronimo();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Board.Update(gameTime);
        base.Update(gameTime);
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
