using System;
using Microsoft.Xna.Framework;

namespace Tetris;

public class Config
{
    public const int BoardPaddingPx = 32;
    public const int BlockHeightPx = 32;
    public const int BlockWidthPx = 32;

    public static bool ShowSecondBoard { set; get; } = false;

    public const int PointsPerRow = 1000;
    public const double PointsMultiplierPerRow = 0.1;

    public static Color EmptyBlockColor = Color.LightBlue;
    public static Color FixedBlockColor = Color.DarkBlue;
    public static Color FallingBlockColor = Color.Red;

    public const int GameBoardWidthBlocks = 12;
    public const int GameBoardHeightBlocks = 24;

    public static double TetronimoDefaultBaseVerticalSpeedBlocksPerSecond = 1;
    public static double TetronimoBaseVerticalSpeedBlocksPerSecond(int score)
    {
        if (score < 1000) return TetronimoDefaultBaseVerticalSpeedBlocksPerSecond;
        if (score < 5000) return TetronimoDefaultBaseVerticalSpeedBlocksPerSecond;
        if (score < 10000) return TetronimoDefaultBaseVerticalSpeedBlocksPerSecond;
        return TetronimoDefaultBaseVerticalSpeedBlocksPerSecond + 2 + Math.Floor((double)score / 10000);
    }

    public const double TetronimoVerticalTurboBoostMultiplier = 10;
    public const double TetronimoBaseHorizontalSpeedBlocksPerSecond = 10;

    public static bool[][,] SpawnableTetronimos { get; } = [
        // T shape.
        new bool[,] {
            {false, true, false},
            {true, true, true},
        },

        // L shape.
        new bool[,] {
            {true, false},
            {true, false},
            {true, true},
        },

        // Backwards L shape.
        new bool[,] {
            {false, true},
            {false, true},
            {true, true},
        },

        // Line.
        new bool[,] {
            {true},
            {true},
            {true},
            {true},
        },

        // Square.
        new bool[,] {
            {true, true},
            {true, true},
        },
    ];
}


