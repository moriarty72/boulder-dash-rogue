using Godot;
using System;

public partial class EnemySquare : Area2D, IGridItem
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

    public State CurrentState = State.Alive;

    public void Dead()
    {

    }
}
