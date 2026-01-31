using Unity.Mathematics;

public static class SubActionTimeExtensions
{
    /// <summary>
    /// Checks if a timer tick occurs at the specified interval.
    /// Compares current time elapsed and time elapsed - delta time to detect when interval boundary is crossed.
    /// </summary>
    public static bool IsTimerTick(this SubActionTimeComponent timer, float interval, bool shouldTickAtStart = false)
    {
        if (interval <= 0)
            return false;

        float previousTime = timer.TimeElapsed - timer.DeltaTime;
        int previousTicks = (int)math.floor(previousTime / interval);
        int currentTicks = (int)math.floor(timer.TimeElapsed / interval);
        
        return currentTicks > previousTicks || 
            (shouldTickAtStart && (previousTime <= 0));
    }

    /// <summary>
    /// Checks if the timer has exceeded the specified timeout duration.
    /// </summary>
    public static bool IsTimeout(this SubActionTimeComponent timer, float timeoutSec)
    {
        return timer.TimeElapsed >= timeoutSec;
    }
}

