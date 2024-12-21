using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Tetris;

public class Block
{
    public Point GridTile { get; private set; }
    public Color Color { get; private set; }
    public bool CanCollide { get; private set; }
    private Texture2D Texture;

    public Block(Point gridTile, Color color, bool canCollide = false)
    {
        GridTile = gridTile;
        Color = color;
        CanCollide = canCollide;
        Texture = new Texture2D(State.SpriteBatch.GraphicsDevice, 1, 1);
        Texture.SetData([color]);
    }

    public void MoveHorizontally(int dx)
    {
        GridTile = new Point(x: GridTile.X + dx, y: GridTile.Y);
    }

    public void MoveVertically(int dy)
    {
        GridTile = new Point(x: GridTile.X, y: GridTile.Y + dy);
    }

    public void Move(int newX, int newY)
    {
        GridTile = new Point(x: newX, y: newY);
    }

    public string ToDebugString()
    {
        if (CanCollide)
        {
            return "□";
        }

        return " ";
    }

    public Block Clone()
    {
        return (Block)MemberwiseClone();
    }

    public void Draw()
    {
        var drawLocation = new Rectangle(
            x: Config.BoardPaddingPx + GridTile.X * Config.BlockWidthPx,
            y: Config.BoardPaddingPx + GridTile.Y * Config.BlockHeightPx,
            width: Config.BlockWidthPx,
            height: Config.BlockHeightPx
        );

        State.SpriteBatch.Draw(Texture, drawLocation, Color);
        State.SpriteBatch.DrawRectangle(drawLocation, Color.LightGray, thickness: 1);
    }
}