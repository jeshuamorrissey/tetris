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
                        color: Config.FallingBlockColor,
                        canCollide: true
                    );
                }
                else
                {
                    Blocks[rowIdx, colIdx] = new Block(
                        gridTile: new Point(x: startingGridLocation.X + colIdx, y: startingGridLocation.Y + rowIdx),
                        color: Config.EmptyBlockColor,
                        canCollide: false
                    );
                }
            }
        }
    }

    public void Update(GameTime gameTime, Func<Block, Point, bool> checkCollision)
    {
        if (HasCollided)
        {
            return;
        }

        if (_verticalMovementAction == null)
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
                    if (Keyboard.GetState().IsKeyDown(Keys.Down)) return 1 / (Config.TetronimoBaseVerticalSpeedBlocksPerSecond * Config.TetronimoVerticalTurboBoostMultiplier);
                    return 1 / Config.TetronimoBaseVerticalSpeedBlocksPerSecond;
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
                action: Rotate,
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

    public void Rotate()
    {
        // First, reverse each row.
        var reversedRows = new Block[Blocks.GetLength(0), Blocks.GetLength(1)];
        for (int rowIdx = 0; rowIdx < reversedRows.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < reversedRows.GetLength(1); colIdx++)
            {
                reversedRows[rowIdx, colIdx] = Blocks[rowIdx, Blocks.GetLength(1) - colIdx - 1];
            }
        }

        // Now, transpose.
        var newBlocks = new Block[Blocks.GetLength(1), Blocks.GetLength(0)];
        for (int rowIdx = 0; rowIdx < newBlocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < newBlocks.GetLength(1); colIdx++)
            {
                newBlocks[rowIdx, colIdx] = reversedRows[colIdx, rowIdx];
            }
        }

        Blocks = newBlocks;
    }

    public void Draw()
    {
        // Draw each block from top left to top right, skipping any null blocks.
        for (int rowIdx = 0; rowIdx < Blocks.GetLength(0); rowIdx++)
        {
            for (int colIdx = 0; colIdx < Blocks.GetLength(1); colIdx++)
            {
                Blocks[rowIdx, colIdx].Draw();
            }
        }
    }
}