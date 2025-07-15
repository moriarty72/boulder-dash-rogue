using Godot;
using System;
using static FallingObjectController;

public partial class Rock : Area2D
{
    public Rock()
    {
    }
}

/*
public partial class Rock : Area2D, IGridItem
{
    public enum State
    {
        Dead,
        Stand,
        Fall,
        FallLeft,
        FallRight,
        MoveRight,
        MoveLeft,
        PushedLeft,
        PushedRight
    }
    [Export]
    public double RockMoveDelay = 0.0;

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

    public void UpdateCurrentState(State newState)
    {
        CurrentState = newState;
        GD.Print("UpdateCurrentStete for rock pos " + GridPosition);
    }

    private void UpdateGlobalPosition(Vector2 currentPosition, Vector2I prevGridPosition)
    {
        GlobalPosition = currentPosition;
        mainController.SwapGridItems(prevGridPosition, GridPosition, false);

        lastMoveTick = 0;
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

            case State.MoveLeft:
            case State.FallLeft:
                {
                    GridPosition.X--;
                    currentPosition.X -= 64;
                    break;
                }

            case State.MoveRight:
            case State.FallRight:
                {
                    GridPosition.X++;
                    currentPosition.X += 64;
                    break;
                }

            case State.PushedLeft:
                {
                    GridPosition.X--;
                    currentPosition.X -= 64;

                    UpdateGlobalPosition(currentPosition, prevGridPosition);

                    CurrentState = State.Stand;
                    return;
                }

            case State.PushedRight:
                {
                    GridPosition.X++;
                    currentPosition.X += 64;

                    UpdateGlobalPosition(currentPosition, prevGridPosition);

                    CurrentState = State.Stand;
                    return;
                }

        }

        mainController.CheckRockfordCollision(GridPosition, 0, 1);
        UpdateGlobalPosition(currentPosition, prevGridPosition);
    }

    public void Process(double delta)
    {
        CurrentState = mainController.CheckObjectState(this, GridPosition);
        Move(delta);
    }

    public void Dead()
    {
        CurrentState = State.Dead;
    }
}
*/
