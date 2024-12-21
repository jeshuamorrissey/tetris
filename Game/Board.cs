using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Tetris;

public class Board : SimpleDrawableGameComponent
{
    private Block[,] Blocks;
    private Tetronimo FallingTetronimo = null;

    public int Width() { return Blocks.GetLength(1); }
    public int Height() { return Blocks.GetLength(0); }

    public Board(int width, int height)
    {
        Blocks = new Block[height, width];
        for (int rowIdx = 0; rowIdx < Height(); rowIdx++)
        {
            for (int colIdx = 0; colIdx < Width(); colIdx++)
            {
                Blocks[rowIdx, colIdx] = new Block(
                    gridTile: new Point(x: colIdx, y: rowIdx),
                    color: Config.EmptyBlockColor,
                    canCollide: false
                );
            }
        }
    }

    public void SpawnTetronimo()
    {
        FallingTetronimo = new Tetronimo(
            blocks: Config.SpawnableTetronimos[State.Random.Next(0, Config.SpawnableTetronimos.Length)],
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
            }

            // Reset the falling tetronimo.
            SpawnTetronimo();
        }

        CheckCompleteRows();
    }

    private void CheckCompleteRows() {
        var rowsToRemove = RowsToRemove(Blocks);
        var newBlocks = RemoveRows(Blocks, rowsToRemove);
        Blocks = FixBlockLocations(newBlocks);
    }

    private static HashSet<int> RowsToRemove(Block[,] blocks) {
        var rowsToRemove = new HashSet<int>();
        for (int rowIdx = 0; rowIdx < blocks.GetLength(0); rowIdx++) {
            var isCompleteRow = true;
            for (int colIdx = 0; colIdx < blocks.GetLength(1); colIdx++) {
                if (!blocks[rowIdx, colIdx].CanCollide) {
                    isCompleteRow = false;
                    break;
                }
            }

            if (isCompleteRow) {
                rowsToRemove.Add(rowIdx);
            }
        }

        return rowsToRemove;
    }

    private static Block[,] RemoveRows(Block[,] existingBlocks, HashSet<int> rowsToRemove) {
        // We do this by making a new block grid which scans from bottom to top and only
        // keeps the rows that we want.
        var newBlocks = new Block[existingBlocks.GetLength(0), existingBlocks.GetLength(1)];
        int newRowIdx = existingBlocks.GetLength(0) - 1;  // Row index we are up to inserting.
        for (int rowIdx = existingBlocks.GetLength(0) - 1; rowIdx >= 0; rowIdx--) {
            // If we want to remove this row, skip it.
            if (rowsToRemove.Contains(rowIdx)) {
                continue;
            }

            // Otherwise, copy the elements.
            for (int colIdx = 0; colIdx < existingBlocks.GetLength(1); colIdx++) {
                newBlocks[newRowIdx, colIdx] = existingBlocks[rowIdx, colIdx];
            }

            newRowIdx--;
        }

        return newBlocks;
    }

    private static Block[,] FixBlockLocations(Block[,] blocks) {
        for (int rowIdx = 0; rowIdx < blocks.GetLength(0); rowIdx++) {
            for (int colIdx = 0; colIdx < blocks.GetLength(1); colIdx++) {
                if (blocks[rowIdx, colIdx] == null) {
                    blocks[rowIdx, colIdx] = new Block(
                        gridTile: new Point(x: colIdx, y: rowIdx),
                        color: Config.EmptyBlockColor,
                        canCollide: false
                    );
                } else {
                    blocks[rowIdx, colIdx].Move(colIdx, rowIdx);
                }
            }
        }

        return blocks;
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
