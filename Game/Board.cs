using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using MonoGame.Extended;

namespace Tetris;

public class Board : SimpleDrawableGameComponent
{
    private Block[,] Blocks;
    private Tetronimo FallingTetronimo = null;
    private Tetronimo NextTetronimo = null;

    // Animations.
    private AnimatedSprite clickAnimation = null;
    private Vector2 clickAnimationLocation;

    public Vector2 DrawLocation { get; private set; }
    public bool HaveLost { get; private set; } = false;
    public int Width() { return Blocks.GetLength(1); }
    public int Height() { return Blocks.GetLength(0); }

    public Board(int width, int height, Vector2 drawLocation)
    {
        DrawLocation = drawLocation;
        Blocks = new Block[height, width];
        for (int rowIdx = 0; rowIdx < Height(); rowIdx++)
        {
            for (int colIdx = 0; colIdx < Width(); colIdx++)
            {
                Blocks[rowIdx, colIdx] = new Block(
                    board: this,
                    gridTile: new Point(x: colIdx, y: rowIdx),
                    sprite: State.Sprites.LightBlueBlock,
                    canCollide: false
                );
            }
        }

        NextTetronimo = new Tetronimo(
            board: this,
            blocks: Config.SpawnableTetronimos[State.Random.Next(0, Config.SpawnableTetronimos.Length)],
            startingGridLocation: new Point(x: 0, y: 0)
        );
    }

    public void SpawnTetronimo()
    {
        FallingTetronimo = NextTetronimo;
        FallingTetronimo.StartFalling();
        NextTetronimo = new Tetronimo(
            board: this,
            blocks: Config.SpawnableTetronimos[State.Random.Next(0, Config.SpawnableTetronimos.Length)],
            startingGridLocation: new Point(x: 0, y: 0)
        );
    }

    public override void Update(GameTime gameTime)
    {
        if (HaveLost)
        {
            return;
        }

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
            // Trigger animation.
            clickAnimation = State.Sprites.ClickAnimation.CreateAnimatedSprite("ClickAnimation");
            clickAnimation.Play(loopCount: 1);

            List<Block> blocksToPick = [];
            for (int colIdx = 0; colIdx < FallingTetronimo.Blocks.GetLength(1); colIdx++)
            {
                var block = FallingTetronimo.Blocks[FallingTetronimo.Blocks.GetLength(0) - 1, colIdx];
                if (block.CanCollide)
                {
                    blocksToPick.Add(block);
                }
            }

            var blockToAnimate = blocksToPick[State.Random.Next(0, blocksToPick.Count)];
            clickAnimationLocation = new Vector2(
                x: blockToAnimate.RenderPosition().X + (State.Random.Next(-1, 1) * (Config.BlockWidthPx / 2)),
                y: blockToAnimate.RenderPosition().Y + (Config.BlockHeightPx / 2)
            );

            // Copy the blocks.
            State.SoundEffects.Click.Play();

            foreach (var block in FallingTetronimo.Blocks)
            {
                if (block.CanCollide)
                {
                    Blocks[block.GridTile.Y, block.GridTile.X] = new Block(
                        board: this,
                        gridTile: block.GridTile,
                        sprite: State.Sprites.DarkBlueBlock,
                        canCollide: true
                    );
                }
            }

            // Check to see if any blocks are on the top row of the board.
            for (int colIdx = 0; colIdx < Width(); colIdx++)
            {
                if (Blocks[0, colIdx].CanCollide)
                {
                    FallingTetronimo = null;
                    clickAnimation = null;
                    HaveLost = true;
                    return;
                }
            }



            // Reset the falling tetronimo.
            SpawnTetronimo();
        }

        CheckCompleteRows();

        if (clickAnimation != null)
        {
            clickAnimation.Update(gameTime);
        }
    }

    private void CheckCompleteRows()
    {
        var rowsToRemove = RowsToRemove(Blocks);
        if (rowsToRemove.Count == 0)
        {
            return;
        }

        int scoreChange = Config.PointsPerRow * rowsToRemove.Count;
        if (rowsToRemove.Count > 1)
        {
            scoreChange = (int)Math.Round(scoreChange * (1 + Config.PointsMultiplierPerRow * (rowsToRemove.Count - 1)));
        }

        State.Score += scoreChange;

        State.SoundEffects.ClearRow.Play();
        var newBlocks = RemoveRows(Blocks, rowsToRemove);
        Blocks = FixBlockLocations(newBlocks);
    }

    private static HashSet<int> RowsToRemove(Block[,] blocks)
    {
        var rowsToRemove = new HashSet<int>();
        for (int rowIdx = 0; rowIdx < blocks.GetLength(0); rowIdx++)
        {
            var isCompleteRow = true;
            for (int colIdx = 0; colIdx < blocks.GetLength(1); colIdx++)
            {
                if (!blocks[rowIdx, colIdx].CanCollide)
                {
                    isCompleteRow = false;
                    break;
                }
            }

            if (isCompleteRow)
            {
                rowsToRemove.Add(rowIdx);
            }
        }

        return rowsToRemove;
    }

    private static Block[,] RemoveRows(Block[,] existingBlocks, HashSet<int> rowsToRemove)
    {
        // We do this by making a new block grid which scans from bottom to top and only
        // keeps the rows that we want.
        var newBlocks = new Block[existingBlocks.GetLength(0), existingBlocks.GetLength(1)];
        int newRowIdx = existingBlocks.GetLength(0) - 1;  // Row index we are up to inserting.
        for (int rowIdx = existingBlocks.GetLength(0) - 1; rowIdx >= 0; rowIdx--)
        {
            // If we want to remove this row, skip it.
            if (rowsToRemove.Contains(rowIdx))
            {
                continue;
            }

            // Otherwise, copy the elements.
            for (int colIdx = 0; colIdx < existingBlocks.GetLength(1); colIdx++)
            {
                newBlocks[newRowIdx, colIdx] = existingBlocks[rowIdx, colIdx];
            }

            newRowIdx--;
        }

        return newBlocks;
    }

    private Block[,] FixBlockLocations(Block[,] blocks)
    {
        for (int rowIdx = 0; rowIdx < blocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < blocks.GetLength(1); colIdx++)
            {
                if (blocks[rowIdx, colIdx] == null)
                {
                    blocks[rowIdx, colIdx] = new Block(
                        board: this,
                        gridTile: new Point(x: colIdx, y: rowIdx),
                        sprite: State.Sprites.LightBlueBlock,
                        canCollide: false
                    );
                }
                else
                {
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
        if (clickAnimation?.IsAnimating == true)
        {
            clickAnimation.Draw(State.SpriteBatch, clickAnimationLocation);
        }

        if (HaveLost)
        {
            // Draw a semi-transparent rectangle over the board.
            Texture2D texture = new Texture2D(State.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            Color[] c = [Color.FromNonPremultiplied(255, 0, 0, 150)]; // 0 transparent; 255 opaque
            texture.SetData(c);

            int drawWidth = Width() * Config.BlockWidthPx;
            int drawHeight = Height() * Config.BlockHeightPx;
            State.SpriteBatch.Draw(
                texture: texture,
                destinationRectangle: new Rectangle(
                    x: (int)DrawLocation.X,
                    y: (int)DrawLocation.Y,
                    width: drawWidth,
                    height: drawHeight
                ),
                color: Color.White
            );
            
            var loseString = Localization.Strings.GameYouLose;
            var stringSize = State.Sprites.Font.MeasureString(loseString);

            State.SpriteBatch.DrawString(
                spriteFont: State.Sprites.Font,
                text: loseString,
                position: new Vector2(
                    x: (int)DrawLocation.X + (int)Math.Floor((double)drawWidth / 2) - stringSize.X / 2,
                    y: (int)DrawLocation.Y + (int)Math.Floor((double)drawHeight / 2) - stringSize.Y / 2
                ),
                color: Color.White
            );
        }
    }
}
