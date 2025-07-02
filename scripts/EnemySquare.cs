using Godot;
using System;

public partial class EnemySquare : BaseGridObject
{
    public enum State
    {
        Alive,
        Dead,
        MoveRight,
        MoveLeft,
        MoveUp,
        MoveDown
    }
    [Export]
    public double MoveDelay = 0.0;

    public EnemySquare()
    {
    }
}
