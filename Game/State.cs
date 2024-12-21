using System;
using Microsoft.Xna.Framework.Graphics;

namespace Tetris;

public class State
{
    public static SpriteBatch SpriteBatch { get; set; }
    public static Random Random { get; } = new Random();
}
