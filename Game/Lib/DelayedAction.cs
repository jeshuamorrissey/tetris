using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace Tetris;

public class DelayedAction(Action action, Func<int, double> getDelaySeconds, bool repeat = false)
{
    public Action Action { get; private set; } = action;
    public Func<int, double> GetDelaySeconds { get; private set; } = getDelaySeconds;
    public bool Repeat { get; private set; } = repeat;

    public double TimeElapsedSinceLastExecution { get; private set; } = 0;
    public int NumExecutions { get; private set; } = 0;

    public void Update(GameTime gameTime)
    {
        TimeElapsedSinceLastExecution += gameTime.GetElapsedSeconds();
        if (TimeElapsedSinceLastExecution >= GetDelaySeconds(NumExecutions) && (NumExecutions == 0 || Repeat))
        {
            Action();
            NumExecutions++;
            TimeElapsedSinceLastExecution = 0;
        }
    }
}


public class DelayedActionWithKey<T>(T key, Action action, Func<int, double> getDelaySeconds, bool repeat = false) : DelayedAction(action, getDelaySeconds, repeat)
{
    public T Key { get; private set; } = key;
}