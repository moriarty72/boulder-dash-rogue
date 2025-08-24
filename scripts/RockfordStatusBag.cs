using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

public static class RockfordStatusBag
{
    public static int Score;
    public static List<DoorController.Color> CollectedKeys = [];

    public static void Reset()
    {
        Score = 0;
        CollectedKeys = [];
    }
}