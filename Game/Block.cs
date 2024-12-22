using Microsoft.Xna.Framework;
using MonoGame.Aseprite;

namespace Tetris;

public class Block
{
    public Board Board { get; private set; }
    public Point GridTile { get; private set; }
    public Sprite Sprite { get; private set; }
    public bool CanCollide { get; private set; }

    public Block(Board board, Point gridTile, Sprite sprite, bool canCollide = false)
    {
        Board = board;
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

    public Vector2 RenderPosition()
    {
        return new Vector2(
            x: Board.DrawLocation.X + GridTile.X * Sprite.Width,
            y: Board.DrawLocation.Y + GridTile.Y * Sprite.Height
        );
    }

    public Block Clone()
    {
        return (Block)MemberwiseClone();
    }

    public void Draw()
    {
        Sprite.Draw(
            spriteBatch: State.SpriteBatch,
            position: RenderPosition()
        );
    }
}