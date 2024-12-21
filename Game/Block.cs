using Microsoft.Xna.Framework;
using MonoGame.Aseprite;

namespace Tetris;

public class Block
{
    public Point GridTile { get; private set; }
    public Sprite Sprite { get; private set; }
    public bool CanCollide { get; private set; }

    public Block(Point gridTile, Sprite sprite, bool canCollide = false)
    {
        GridTile = gridTile;
        Sprite = sprite;
        CanCollide = canCollide;
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

    public Block Clone()
    {
        return (Block)MemberwiseClone();
    }

    public void Draw()
    {
        Sprite.Draw(
            spriteBatch: State.SpriteBatch,
            position: new Vector2(
                x: Config.BoardPaddingPx + GridTile.X * Sprite.Width,
                y: Config.BoardPaddingPx + GridTile.Y * Sprite.Height)
        );
    }
}