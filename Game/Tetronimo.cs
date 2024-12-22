using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Tetris;


enum HorizontalMovementActionKey
{
    LEFT,
    RIGHT,
}

public class Tetronimo
{
    public Block[,] Blocks { get; private set; }

    // State
    public bool HasCollided { get; private set; } = false;
    public bool IsFalling { get; private set; } = false;

    // Delayed actions.
    private DelayedAction _verticalMovementAction = null;
    private DelayedAction _rotateAction = null;
    private DelayedActionWithKey<HorizontalMovementActionKey> _horizontalMovementAction = null;

    public Tetronimo(bool[,] blocks, Point startingGridLocation)
    {
        Blocks = new Block[blocks.GetLength(0), blocks.GetLength(1)];
        for (int rowIdx = 0; rowIdx < blocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < blocks.GetLength(1); colIdx++)
            {
                if (blocks[rowIdx, colIdx])
                {
                    Blocks[rowIdx, colIdx] = new Block(
                        gridTile: new Point(x: startingGridLocation.X + colIdx, y: startingGridLocation.Y + rowIdx),
                        sprite: State.Sprites.RedBlock,
                        canCollide: true
                    );
                }
                else
                {
                    Blocks[rowIdx, colIdx] = new Block(
                        gridTile: new Point(x: startingGridLocation.X + colIdx, y: startingGridLocation.Y + rowIdx),
                        sprite: State.Sprites.LightBlueBlock,
                        canCollide: false
                    );
                }
            }
        }
    }

    public void StartFalling()
    {
        IsFalling = true;
    }

    public void Update(GameTime gameTime, Func<Block, Point, bool> checkCollision)
    {
        if (HasCollided)
        {
            return;
        }

        if (_verticalMovementAction == null && IsFalling)
        {
            _verticalMovementAction = new DelayedAction(
                action: () =>
                {
                    // First, check collisions with all blocks. If we collide, this becomes fixed blocks.
                    foreach (var block in Blocks)
                    {
                        if (checkCollision(block, new Point(x: 0, y: 1)))
                        {
                            HasCollided = true;
                            return;
                        }
                    }

                    // No collisions? Move away!
                    foreach (var block in Blocks)
                    {
                        block.MoveVertically(1);
                    }
                },
                getDelaySeconds: (int numExecutions) =>
                {
                    if (numExecutions == 0) return 0;
                    if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    {
                        var turboSpeedBps = Math.Min(20, Config.TetronimoBaseVerticalSpeedBlocksPerSecond(State.Score) * Config.TetronimoVerticalTurboBoostMultiplier);
                        return 1 / turboSpeedBps;
                    }
                    return 1 / Config.TetronimoBaseVerticalSpeedBlocksPerSecond(State.Score);
                },
                repeat: true
            );
        }

        HandleHorizontalMovement(checkCollision);
        HandleRotation(checkCollision);

        _verticalMovementAction?.Update(gameTime);
        _horizontalMovementAction?.Update(gameTime);
        _rotateAction?.Update(gameTime);
    }

    private void HandleHorizontalMovement(Func<Block, Point, bool> checkCollision)
    {
        HorizontalMovementActionKey directionKey = HorizontalMovementActionKey.RIGHT;
        int xChange = 0;
        if (Keyboard.GetState().IsKeyDown(Keys.Left))
        {
            xChange--;
            directionKey = HorizontalMovementActionKey.LEFT;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right))
        {
            xChange++;
            directionKey = HorizontalMovementActionKey.RIGHT;
        }

        // Case: We aren't moving at all, in which case we can cancel any in-flight actions and exit.
        if (xChange == 0)
        {
            _horizontalMovementAction = null;
            return;
        }

        // Case: We are already moving in the direction we want, in which case we are already done.
        if (_horizontalMovementAction != null && _horizontalMovementAction.Key == directionKey)
        {
            return;
        }

        // Case: We are either not moving or moving in the wrong direction; either way fix it.
        _horizontalMovementAction = new DelayedActionWithKey<HorizontalMovementActionKey>(
            key: directionKey,
            action: () =>
            {
                // First, check collisions with all blocks. If we collide, then negate the movement.
                foreach (var block in Blocks)
                {
                    if (checkCollision(block, new Point(x: xChange, y: 0)))
                    {
                        return;
                    }
                }

                // No collisions, so move the block!
                foreach (var block in Blocks)
                {
                    block.MoveHorizontally(xChange);
                }
            },
            getDelaySeconds: (int numExecutions) =>
            {
                if (numExecutions == 0) return 0;
                if (numExecutions == 1) return 1.5 / Config.TetronimoBaseHorizontalSpeedBlocksPerSecond;
                return 1 / Config.TetronimoBaseHorizontalSpeedBlocksPerSecond;
            },
            repeat: true
        );
    }

    private void HandleRotation(Func<Block, Point, bool> checkCollision)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Space) && _rotateAction == null)
        {
            _rotateAction = new DelayedAction(
                action: () =>
                {
                    var newBlocks = Rotate();
                    for (int rowIdx = 0; rowIdx < newBlocks.GetLength(0); rowIdx++)
                    {
                        for (int colIdx = 0; colIdx < newBlocks.GetLength(1); colIdx++)
                        {
                            if (checkCollision(newBlocks[rowIdx, colIdx], new Point(0, 0)))
                            {
                                return;
                            }
                        }
                    }

                    Blocks = newBlocks;
                },
                getDelaySeconds: (int numExecutions) =>
                {
                    if (numExecutions == 0) return 0;
                    if (numExecutions == 1) return 0.5;
                    return 0.1;
                },
                repeat: true
            );
        }
        else if (Keyboard.GetState().IsKeyUp(Keys.Space) && _rotateAction != null)
        {
            _rotateAction = null;
        }
    }

    public Block[,] Rotate()
    {
        // Transpose.
        var transposedBlocks = new Block[Blocks.GetLength(1), Blocks.GetLength(0)];
        for (int rowIdx = 0; rowIdx < transposedBlocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < transposedBlocks.GetLength(1); colIdx++)
            {
                transposedBlocks[rowIdx, colIdx] = Blocks[colIdx, rowIdx].Clone();
            }
        }

        // Reverse each row.
        var newBlocks = new Block[transposedBlocks.GetLength(0), transposedBlocks.GetLength(1)];
        for (int rowIdx = 0; rowIdx < newBlocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < newBlocks.GetLength(1); colIdx++)
            {
                newBlocks[rowIdx, colIdx] = transposedBlocks[rowIdx, transposedBlocks.GetLength(1) - colIdx - 1];
            }
        }

        // If the new block is wider than the old block it may cause the block to go out of bounds on
        // the left. In that case we shift the whole thing to the right by as many tiles as necessary to
        // ensure it stays within the boundary. This can only happen on the very left wall.
        var topRightBlockLocation = Blocks[0, Blocks.GetLength(1) - 1].GridTile;
        var xPush = Math.Max(0, newBlocks.GetLength(1) - 1 - topRightBlockLocation.X);

        // Move the blocks into the right position.
        // By default, we anchor based on the top right. If we would rotate out of bounds, anchor on the top
        // left instead.
        for (int rowIdx = 0; rowIdx < newBlocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < newBlocks.GetLength(1); colIdx++)
            {
                newBlocks[rowIdx, colIdx] = transposedBlocks[rowIdx, transposedBlocks.GetLength(1) - colIdx - 1];
                newBlocks[rowIdx, colIdx].Move(topRightBlockLocation.X - (newBlocks.GetLength(1) - 1 - colIdx) + xPush, topRightBlockLocation.Y + rowIdx);
            }
        }

        return newBlocks;
    }

    public void Draw()
    {
        // Draw each block from top left to top right, skipping any null blocks.
        for (int rowIdx = 0; rowIdx < Blocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < Blocks.GetLength(1); colIdx++)
            {
                if (Blocks[rowIdx, colIdx].CanCollide)
                {
                    Blocks[rowIdx, colIdx].Draw();
                }
            }
        }
    }
}