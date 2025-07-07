using Godot;
using System;
using System.Dynamic;

public partial class FallingObject : BaseGridObject
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

    private void UpdateNodeObjectPosition(Vector2 currentPosition, Vector2I prevGridPosition)
    {
        NodeObject.GlobalPosition = currentPosition;
        mainController.SwapGridItems(prevGridPosition, GridPosition, false);
    }

    private void Move(double delta)
    {
        if ((CurrentState == State.Stand) || (CurrentState == State.Dead))
            return;

        if ((lastMoveTick == 0) || (lastMoveTick <= 0.08))
        {
            lastMoveTick += delta;
            return;
        }
        lastMoveTick = 0;

        Vector2I prevGridPosition = new(GridPosition.X, GridPosition.Y);
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

                    UpdateNodeObjectPosition(WorldPosition, prevGridPosition);

                    CurrentState = State.Stand;
                    return;
                }

            case State.PushedRight:
                {
                    GridPosition.X++;
                    WorldPosition.X += 64;

                    UpdateNodeObjectPosition(WorldPosition, prevGridPosition);

                    CurrentState = State.Stand;
                    return;
                }

        }

        mainController.CheckRockfordCollision(GridPosition, 0, 1);
        UpdateNodeObjectPosition(WorldPosition, prevGridPosition);
    }

    public override void Process(double delta)
    {
        if (Type == ItemType.None)
            return;

        CurrentState = CheckAndUpdateCurrentState();
        Move(delta);
    }
}