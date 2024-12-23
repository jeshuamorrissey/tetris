using System;
using Microsoft.Xna.Framework;
using MonoGame.Aseprite;
using MonoGame.Extended.Input;
using Tetris;

class Button(Action onClick, Rectangle rect, string text)
{
    private Action OnClick = onClick;
    private Rectangle Rect = rect;
    private string Text = text;

    private Sprite UnclickedSprite = null;
    private Sprite ClickedSprite = null;

    public void Update()
    {
        var mouse = MouseExtended.GetState();
        if (mouse.WasButtonPressed(MouseButton.Left) && Rect.Contains(mouse.Position))
        {
            OnClick();
        }
    }

    public void Draw()
    {
        if (UnclickedSprite == null) {
            UnclickedSprite = State.Sprites.Buttons.CreateSprite(0);
            UnclickedSprite.Scale = new(
                x: Rect.Width / UnclickedSprite.Width,
                y: Rect.Height / UnclickedSprite.Height
            );
        }

        if (ClickedSprite == null) {
            ClickedSprite = State.Sprites.Buttons.CreateSprite(1);
            ClickedSprite.Scale = new(
                x: Rect.Width / ClickedSprite.Width,
                y: Rect.Height / ClickedSprite.Height
            );
        }

        UnclickedSprite.Draw(State.SpriteBatch, new(x: Rect.X, y: Rect.Y));

        var textSize = State.Sprites.Font.MeasureString(Text);
        State.SpriteBatch.DrawString(
            spriteFont: State.Sprites.Font,
            text: Text,
            position: new(
                x: Rect.X + (Rect.Width / 2) - (textSize.X / 2),
                y: Rect.Y + (Rect.Height / 2) - (textSize.Y / 2)
            ),
            color: Color.White
        );
    }
}

public class TitleScreen(Action onStartNewGame, Action onExit)
{
    private Button newGame = new(
        onClick: onStartNewGame,
        rect: new(
            x: 100,
            y: 100,
            width: State.Graphics.PreferredBackBufferWidth - 200,
            height: 100
        ),
        text: "New Game"
    );

    private Button quitGame = new(
        onClick: onExit,
        rect: new(
            x: 100,
            y: 250,
            width: State.Graphics.PreferredBackBufferWidth - 200,
            height: 100
        ),
        text: "Quit Game"
    );

    public void Update(GameTime gameTime)
    {
        newGame.Update();
        quitGame.Update();
    }

    public void Draw(GameTime gameTime)
    {
        newGame.Draw();
        quitGame.Draw();
    }
}