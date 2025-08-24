using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class RockfordController : BaseGridObjectController
{
    private const double MOVE_DELAY_TIME = 0.15;
    private const double PUSH_DELAY_TIME = 0.55;

    public enum State
    {
        Alive,
        Move,
        Push,
        Stand,
        Dead
    }

    public enum MoveDirection
    {
        left,
        right,
        up,
        down,
        none
    };

    public State CurrentState { get; set; } = State.Alive;

    private double moveDelayTime = 0;
    private double pushDelayTime = 0;
    private bool firePressed = false;

    public State rockfordState = State.Alive;

    private bool CanRockfordPushRock(BaseGridObjectController rockGridObject, MoveDirection rockfordDirection)
    {
        if ((rockfordDirection == MoveDirection.left) || (rockfordDirection == MoveDirection.right))
        {
            // check empty space after rock
            int x = rockGridObject.GridPosition.X + (rockfordDirection == MoveDirection.left ? -1 : 1);
            BaseGridObjectController afterRockGridItem = mainController.GetGridItem(x, rockGridObject.GridPosition.Y);
            BaseGridObjectController belowRockGridItem = mainController.GetGridItem(rockGridObject.GridPosition.X, rockGridObject.GridPosition.Y + 1);
            if ((afterRockGridItem.Type == ItemType.None) && (belowRockGridItem.Type != ItemType.None))
            {
                // GD.Print("Rockford can move rock direction: " + rockfordDirection.ToString());
                return true;
            }
        }
        return false;
    }

    private bool CanRockfordMove(double delta, Vector2I nextPlayerPosition, MoveDirection rockfordDirection)
    {
        BaseGridObjectController gridItem = mainController.GetGridItem(nextPlayerPosition.X, nextPlayerPosition.Y);

        if (gridItem != null)
        {
            switch (gridItem.Type)
            {
                case ItemType.Rock:
                    {
                        if (CanRockfordPushRock(gridItem, rockfordDirection))
                        {
                            CurrentState = State.Push;

                            // move rock
                            (gridItem as FallingObjectController).CurrentState = rockfordDirection == MoveDirection.left ? FallingObjectController.State.PushedLeft : FallingObjectController.State.PushedRight;
                            (gridItem as FallingObjectController).ProcessAndUpdate(delta);
                            mainController.PlayAudio("StonePushAudio");

                            return true;
                        }
                        return false;
                    }

                case ItemType.Diamond:
                    {
                        mainController.RemoveGridItem(gridItem.GridPosition);
                        mainController.PlayAudio("DiamondCollectAudio");
                        return true;
                    }

                case ItemType.Door:
                    {
                        DoorController doorController = (DoorController)gridItem;
                        if (doorController.CurrentState != DoorController.State.Locked)
                            mainController.ChangeRoom(doorController.RoomConnection, doorController.ConnectionSide);
                        else
                        {
                            bool hasKey = RockfordStatusBag.CollectedKeys.Contains((DoorController.Color)doorController.RoomConnection.UnlockLevelNeeded);
                            if (hasKey)
                            {
                                doorController.Unlock();
                                mainController.ChangeRoom(doorController.RoomConnection, doorController.ConnectionSide);
                            }
                        }

                        return false;
                    }

                case ItemType.Amoeba:
                case ItemType.MetalWall:
                    return false;

                case ItemType.Key:
                    {
                        KeyController keyController = (KeyController)gridItem;
                        RockfordStatusBag.CollectedKeys.Add(keyController.keyColor);

                        mainController.RemoveGridItem(gridItem.GridPosition);
                        mainController.PlayAudio("KeyAudio");
                        return true;
                    }
            }
        }
        return true;
    }

    public void RockfordFireAction(Vector2I position, MoveDirection direction)
    {
        BaseGridObjectController gridItem = null;

        if (direction == MoveDirection.up)
            gridItem = mainController.GetGridItem(position.X, position.Y - 1);
        else if (direction == MoveDirection.left)
            gridItem = mainController.GetGridItem(position.X - 1, position.Y);
        else if (direction == MoveDirection.down)
            gridItem = mainController.GetGridItem(position.X, position.Y + 1);
        else if (direction == MoveDirection.right)
            gridItem = mainController.GetGridItem(position.X + 1, position.Y);

        if (gridItem.Type == ItemType.Mud)
            mainController.RemoveGridItem(gridItem.GridPosition);
        else if (gridItem.Type == ItemType.Rock)
        {
            if (CanRockfordPushRock(gridItem, direction))
                (gridItem as FallingObjectController).CurrentState = direction == MoveDirection.left ? FallingObjectController.State.PushedLeft : FallingObjectController.State.PushedRight;
        }
        else if (gridItem.Type == ItemType.Diamond)
        {
            mainController.RemoveGridItem(gridItem.GridPosition);
        }
    }

    private void ComputeRockfordPosition(double delta, MoveDirection moveDirection)
    {
        PrevGridPosition = new(GridPosition.X, GridPosition.Y);
        if (!firePressed)
        {
            if (moveDirection == MoveDirection.up)
            {
                GridPosition.Y--;
            }

            if (moveDirection == MoveDirection.left)
            {
                GridPosition.X--;
            }

            if (moveDirection == MoveDirection.right)
            {
                GridPosition.X++;
            }

            if (moveDirection == MoveDirection.down)
            {
                GridPosition.Y++;
            }

            bool canPlayerMove = CanRockfordMove(delta, GridPosition, moveDirection);
            if (canPlayerMove)
                (NodeObject as Rockford).PlayAnimation(moveDirection);
            else
                GridPosition = PrevGridPosition;
        }
        else
        {
            RockfordFireAction(GridPosition, moveDirection);
        }
    }

    private void ProcessPosition(double delta, MoveDirection moveDirection)
    {
        bool ProcessMovement(ref double delayTime, double maxDelayTime)
        {
            if ((delayTime == 0) || (delayTime <= maxDelayTime))
            {
                delayTime += delta;
                return false;
            }
            delayTime = 0;

            ComputeRockfordPosition(delta, moveDirection);
            // (NodeObject as Rockford).PlayAnimation(moveDirection);

            return true;
        }

        switch (CurrentState)
        {
            case State.Dead:
                {
                    break;
                }

            case State.Move:
                {
                    ProcessMovement(ref moveDelayTime, MOVE_DELAY_TIME);
                    break;
                }

            case State.Push:
                {
                    ProcessMovement(ref pushDelayTime, PUSH_DELAY_TIME);
                    break;
                }

            case State.Alive:
            case State.Stand:
                {
                    (NodeObject as Rockford).PlayAnimation(moveDirection);
                    break;
                }
        }
    }

    private void ProcessUserInput(double delta)
    {
        Main.UserEvent inputEvent = mainController.GetInputEvent(delta);

        firePressed = (inputEvent & Main.UserEvent.ueFire) == Main.UserEvent.ueFire;
        if ((inputEvent & Main.UserEvent.ueLeft) == Main.UserEvent.ueLeft)
        {
            CurrentState = (CurrentState == State.Push) ? State.Push : State.Move;
            ProcessPosition(delta, MoveDirection.left);
        }
        else if ((inputEvent & Main.UserEvent.ueRight) == Main.UserEvent.ueRight)
        {
            CurrentState = (CurrentState == State.Push) ? State.Push : State.Move;
            ProcessPosition(delta, MoveDirection.right);
        }
        else if ((inputEvent & Main.UserEvent.ueUp) == Main.UserEvent.ueUp)
        {
            CurrentState = State.Move;
            ProcessPosition(delta, MoveDirection.up);
        }
        else if ((inputEvent & Main.UserEvent.ueDown) == Main.UserEvent.ueDown)
        {
            CurrentState = State.Move;
            ProcessPosition(delta, MoveDirection.down);
        }
        else //if (inputEvent == Main.UserEvent.ueNone)
        {
            CurrentState = State.Stand;
            ProcessPosition(delta, MoveDirection.none);
        }

        // GD.Print("Current Rockford state: [", CurrentState, "] InputEvent: [", inputEvent, "]");
    }

    public override void ProcessAndUpdate(double delta)
    {
        if (CurrentState == State.Dead)
            return;

        ProcessUserInput(delta);
        if (UpdateNodeObjectPosition())
            mainController.PlayAudio("RockfordWalkAudio");
    }

    public override void Respawn()
    {
        base.Respawn();
        CurrentState = State.Alive;
    }

    public override void Dead()
    {
        base.Dead();
        CurrentState = State.Dead;
    }
}