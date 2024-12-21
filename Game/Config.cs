using Microsoft.Xna.Framework;

namespace Tetris;

public class Config
{
    public const int BoardPaddingPx = 32;
    public const int BlockHeightPx = 16;
    public const int BlockWidthPx = 16;

    public static Color EmptyBlockColor = Color.LightBlue;
    public static Color FixedBlockColor = Color.DarkBlue;
    public static Color FallingBlockColor = Color.Red;

    public const int GameBoardWidthBlocks = 12;
    public const int GameBoardHeightBlocks = 24;

    public const double TetronimoBaseVerticalSpeedBlocksPerSecond = 1;
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

