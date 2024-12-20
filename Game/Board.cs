using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Tetris;

public class Board : SimpleDrawableGameComponent
{
    Block[,] Blocks;
    Tetronimo FallingTetronimo = null;

    public int Width() { return Blocks.GetLength(1); }
    public int Height() { return Blocks.GetLength(0); }

    public Board(int width, int height)
    {
        Blocks = new Block[height, width];
        for (int rowIdx = 0; rowIdx < Height(); rowIdx++)
        {
            for (int colIdx = 0; colIdx < Width(); colIdx++)
            {
                if (colIdx == Width() - 1)
                {
                    Blocks[rowIdx, colIdx] = new Block(
                        gridTile: new Point(x: colIdx, y: rowIdx),
                        color: Config.FixedBlockColor,
                        canCollide: true
                    );
                }
                else
                {
                    Blocks[rowIdx, colIdx] = new Block(
                        gridTile: new Point(x: colIdx, y: rowIdx),
                        color: Config.EmptyBlockColor,
                        canCollide: false
                    );
                }
            }
        }
    }

    public void SpawnTetronimo()
    {
        FallingTetronimo = new Tetronimo(
            blocks: new bool[,] {
                {false, true, false},
                {true, true, true},
            },
            startingGridLocation: new Point(x: 0, y: 0)
        );
    }

    public override void Update(GameTime gameTime)
    {
        FallingTetronimo?.Update(
            gameTime: gameTime,
            checkCollision: (block, dp) =>
            {
                if (!block.CanCollide)
                {
                    return false;
                }

                var newLocation = block.GridTile + dp;

                // Check if the new location collides with any block.
                foreach (var gridBlock in Blocks)
                {
                    if (!gridBlock.CanCollide)
                    {
                        continue;
                    }

                    if (gridBlock.GridTile == newLocation)
                    {
                        return true;
                    }
                }

                // Check it the new X location is out of bounds or collides with an existing block.
                if (newLocation.X < 0 || newLocation.X >= Width() || newLocation.Y < 0 || newLocation.Y >= Height())
                {
                    return true;
                }

                return false;
            }
        );

        // If the tetronimo has collided, we must absorb it.
        if (FallingTetronimo != null && FallingTetronimo.HasCollided)
        {
            // Copy the blocks.
            foreach (var block in FallingTetronimo.Blocks)
            {
                if (block.CanCollide)
                {
                    Blocks[block.GridTile.Y, block.GridTile.X] = new Block(
                        gridTile: block.GridTile,
                        color: Config.FixedBlockColor,
                        canCollide: true
                    );
                }
                else
                {
                    Blocks[block.GridTile.Y, block.GridTile.X] = new Block(
                        gridTile: block.GridTile,
                        color: Config.EmptyBlockColor,
                        canCollide: false
                    );
                }
            }

            // Reset the falling tetronimo.
            SpawnTetronimo();
        }
    }

    public override void Draw(GameTime gameTime)
    {
        // Draw each individual block.
        for (int rowIdx = 0; rowIdx < Blocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < Blocks.GetLength(1); colIdx++)
            {
                Blocks[rowIdx, colIdx].Draw();
            }
        }

        // Draw the tetronimo.
        FallingTetronimo?.Draw();
    }
}
