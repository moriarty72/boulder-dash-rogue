using Godot;
using System;
using System.Collections;
using System.Dynamic;

public partial class FallingObjectController : BaseGridObjectController
{
    private const double FALL_SPEED = 0.15;

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
        PushedRight,
        RockfordCollision,
        EnemyCollision
    }

    public State CurrentState { get; set; } = State.Stand;
    private double lastFallTick = 0;

    private State ProcessCurrentState()
    {
        switch (CurrentState)
        {
            case State.Stand:
                {
                    // check if rock or diamond must fall
                    BaseGridObjectController gridItem = mainController.GetGridItem(GridPosition.X, GridPosition.Y + 1);
                    if (gridItem.Type == ItemType.None)
                        return State.Fall;

                    if ((gridItem.Type == ItemType.Rock) || (gridItem.Type == ItemType.Diamond))
                    {
                        // check if rock or diamond must fall left/right
                        gridItem = mainController.GetGridItem(GridPosition.X - 1, GridPosition.Y + 1);
                        BaseGridObjectController gridItemLeft = mainController.GetGridItem(GridPosition.X - 1, GridPosition.Y);
                        if ((gridItem.Type == ItemType.None) && (gridItemLeft.Type == ItemType.None))
                            return State.FallLeft;

                        gridItem = mainController.GetGridItem(GridPosition.X + 1, GridPosition.Y + 1);
                        BaseGridObjectController gridItemRight = mainController.GetGridItem(GridPosition.X + 1, GridPosition.Y);
                        if ((gridItem.Type == ItemType.None) && (gridItemRight.Type == ItemType.None))
                            return State.FallRight;
                    }
                    break;
                }

            case State.Fall:
                {
                    BaseGridObjectController gridItem = mainController.GetGridItem(GridPosition.X, GridPosition.Y + 1);
                    if (gridItem.Type == ItemType.None)
                        return State.Fall;
                    else if (gridItem.Type == ItemType.Rockford)
                        return State.RockfordCollision;
                    else if (gridItem.Type == ItemType.EnemySquare)
                        return State.EnemyCollision;

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

    private bool ProcessPosition(double delta)
    {
        PrevGridPosition = new(GridPosition.X, GridPosition.Y);
        switch (CurrentState)
        {
            case State.Dead:
            case State.Stand:
                {
                    return false;
                }

            case State.Fall:
                {
                    if ((lastFallTick == 0) || (lastFallTick <= FALL_SPEED))
                    {
                        lastFallTick += delta;
                        return false;
                    }
                    lastFallTick = 0;

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
                    break;
                }

            case State.PushedRight:
                {
                    GridPosition.X++;
                    WorldPosition.X += 64;
                    break;
                }

            case State.EnemyCollision:
            case State.RockfordCollision:
                {
                    Dead();
                    mainController.SpawnExplosion(GridPosition, true);

                    return false;
                }
        }
        return true;
    }

    public override void ProcessAndUpdate(double delta)
    {
        CurrentState = ProcessCurrentState();
        if (ProcessPosition(delta))
            Update(delta);
    }

    private void Update(double delta)
    {
        switch (CurrentState)
        {
            case State.Dead:
            case State.Stand:
                {
                    break;
                }

            case State.Fall:
                {
                    UpdateNodeObjectPosition();
                    break;
                }

            case State.PushedLeft:
            case State.PushedRight:
                {
                    UpdateNodeObjectPosition();
                    CurrentState = State.Stand;
                    break;
                }

            default:
                {
                    UpdateNodeObjectPosition();
                    break;
                }
        }
    }

    public override void Dead()
    {
        base.Dead();
        CurrentState = State.Dead;
    }
}