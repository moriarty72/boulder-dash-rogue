using Godot;
using System;

public partial class Rock : Area2D, IGridItem
{
    public enum State
    {
        Dead,
        Stand,
        Fall,
        FallLeft,
        FallRight
    }
    [Export]
    public double RockMoveDelay = 0.5;

    public State CurrentState = State.Stand;
    public Vector2I GridPosition;
    private Main mainController;
    private Vector2 currentPosition;
    private double lastMoveTick = 0;

    public void Initilize(Main mc, Vector2I gridPosition)
    {
        mainController = mc;
        currentPosition = GlobalPosition;
        GridPosition = gridPosition;
    }

    private void Move(double delta)
    {
        if ((CurrentState == State.Stand) || (CurrentState == State.Dead))
            return;

        if (lastMoveTick == 0)
        {
            lastMoveTick = delta;
        }

        if (lastMoveTick <= RockMoveDelay)
        {
            lastMoveTick += delta;
            return;
        }

        Vector2I prevGridPosition = new(GridPosition.X, GridPosition.Y);
        switch (CurrentState)
        {
            case State.Fall:
                {
                    GridPosition.Y++;
                    currentPosition.Y += 64;
                    break;
                }

            case State.FallLeft:
                {
                    GridPosition.X--;
                    currentPosition.X -= 64;
                    break;
                }

            case State.FallRight:
                {
                    GridPosition.X++;
                    currentPosition.X += 64;
                    break;
                }
        }

        mainController.CheckRockfordCollision(GridPosition, 0, 1);

        GlobalPosition = currentPosition;
        mainController.SwapGridItems(prevGridPosition, GridPosition, false);

        lastMoveTick = 0;
        // CurrentState = State.Stand;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        CurrentState = mainController.CanRockFall(GridPosition);
        Move(delta);
    }

    public void Dead()
    {
        CurrentState = State.Dead;
    }
}
