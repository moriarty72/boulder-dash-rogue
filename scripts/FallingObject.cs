using Godot;
using System;
using System.Dynamic;

public partial class FallingObject : BaseGridObject
{
    private const double FALL_SPEED = 0.07;

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

    public State CurrentState { get; set; } = State.Stand;
    private double lastMoveTick = 0;

    private State CheckAndUpdateCurrentState()
    {
        switch (CurrentState)
        {
            case State.Stand:
                {
                    if (CurrentState == State.Stand)
                    {
                        // check if rock or diamond must fall
                        BaseGridObject gridItem = mainController.GetGridItem(GridPosition.X, GridPosition.Y + 1);
                        if (gridItem.Type == ItemType.None)
                            return State.Fall;

                        if ((gridItem.Type == ItemType.Rock) || (gridItem.Type == ItemType.Diamond))
                        {
                            // check if rock or diamond must fall left/right
                            gridItem = mainController.GetGridItem(GridPosition.X - 1, GridPosition.Y + 1);
                            BaseGridObject gridItemLeft = mainController.GetGridItem(GridPosition.X - 1, GridPosition.Y);
                            if ((gridItem.Type == ItemType.None) && (gridItemLeft.Type == ItemType.None))
                                return State.FallLeft;

                            gridItem = mainController.GetGridItem(GridPosition.X + 1, GridPosition.Y + 1);
                            BaseGridObject gridItemRight = mainController.GetGridItem(GridPosition.X + 1, GridPosition.Y);
                            if ((gridItem.Type == ItemType.None) && (gridItemRight.Type == ItemType.None))
                                return State.FallRight;
                        }
                    }
                    break;
                }

            case State.PushedRight:
            case State.PushedLeft:
                {
                    return CurrentState;
                }
        }
        return State.Stand;
    }

    private void UpdateNodeObjectPosition()
    {
        if (PrevGridPosition != GridPosition)
        {
            NodeObject.GlobalPosition = WorldPosition;
            mainController.SwapGridItems(PrevGridPosition, GridPosition, true);

            PrevGridPosition = GridPosition;
        }
    }

    private void ProcessPosition(double delta)
    {
        if ((CurrentState == State.Stand) || (CurrentState == State.Dead))
            return;

        if ((lastMoveTick == 0) || (lastMoveTick <= FALL_SPEED))
        {
            lastMoveTick += delta;
            return;
        }
        lastMoveTick = 0;

        PrevGridPosition = new(GridPosition.X, GridPosition.Y);
        switch (CurrentState)
        {
            case State.Fall:
                {
                    GridPosition.Y++;
                    WorldPosition.Y += 64;
                    break;
                }

            case State.MoveLeft:
            case State.FallLeft:
                {
                    GridPosition.X--;
                    WorldPosition.X -= 64;
                    break;
                }

            case State.MoveRight:
            case State.FallRight:
                {
                    GridPosition.X++;
                    WorldPosition.X += 64;
                    break;
                }

            case State.PushedLeft:
                {
                    GridPosition.X--;
                    WorldPosition.X -= 64;

                    CurrentState = State.Stand;
                    return;
                }

            case State.PushedRight:
                {
                    GridPosition.X++;
                    WorldPosition.X += 64;

                    CurrentState = State.Stand;
                    return;
                }
        }
    }

    private void Move(double delta)
    {
        UpdateNodeObjectPosition();
    }

    public override void Process(double delta)
    {
        if (CurrentState == State.Dead)
            return;

        CurrentState = CheckAndUpdateCurrentState();
        ProcessPosition(delta);
    }

    public override void Update(double delta)
    {
        if ((CurrentState == State.Dead) || (CurrentState == State.Stand))
            return;

        Move(delta);
    }

    public override void Dead()
    {
        base.Dead();
        CurrentState = State.Dead;
    }
}