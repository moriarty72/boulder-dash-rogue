public class Timer
{
    private double lastTick = 0;
    private double elapsedTick = 0;

    public Timer(double elapsed)
    {
        elapsedTick = elapsed;
    }

    public bool IsElapsed(double delta)
    {
        if ((lastTick == 0) || (lastTick <= elapsedTick))
        {
            lastTick += delta;
            return false;
        }

        lastTick = 0;
        return true;
    }
}