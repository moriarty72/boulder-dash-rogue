using Godot;

public partial class RockfordController : BaseGridObjectController
{
    private const double IDLE_INPUT_TIME = 0.15;

    public enum State
    {
        Alive,
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

    private double idleInputDelayTime = 0;
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
                GD.Print("Rockford can move rock direction: " + rockfordDirection.ToString());
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
                            // move rock
                            (gridItem as FallingObjectController).CurrentState = rockfordDirection == MoveDirection.left ? FallingObjectController.State.PushedLeft : FallingObjectController.State.PushedRight;
                            (gridItem as FallingObjectController).ProcessAndUpdate(delta);

                            mainController.PlayAudio("StonePushAudio");
                        }
                        return false;
                    }

                case ItemType.Diamond:
                    {
                        mainController.RemoveGridItem(gridItem.GridPosition);
                        mainController.PlayAudio("DiamondCollectAudio");
                        return true;
                    }

                case ItemType.MetalWall:
                    return false;
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

    private void ProcessPosition(double delta, MoveDirection moveDirection)
    {
        if (moveDirection == MoveDirection.none)
        {
            (NodeObject as Rockford).PlayAnimation(moveDirection);
            return;
        }

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

    private void ProcessUserInput(double delta)
    {
        Main.UserEvent inputEvent = mainController.GetInputEvent(delta);

        firePressed = (inputEvent & Main.UserEvent.ueFire) == Main.UserEvent.ueFire;

        if (inputEvent == Main.UserEvent.ueNone)
        {
            idleInputDelayTime += delta;
            if (idleInputDelayTime > IDLE_INPUT_TIME)
            {
                idleInputDelayTime = 0;
                ProcessPosition(delta, MoveDirection.none);
            }
        }

        if ((inputEvent & Main.UserEvent.ueLeft) == Main.UserEvent.ueLeft)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.left);
        }
        else if ((inputEvent & Main.UserEvent.ueRight) == Main.UserEvent.ueRight)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.right);
        }
        else if ((inputEvent & Main.UserEvent.ueUp) == Main.UserEvent.ueUp)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.up);
        }
        else if ((inputEvent & Main.UserEvent.ueDown) == Main.UserEvent.ueDown)
        {
            idleInputDelayTime = 0;
            ProcessPosition(delta, MoveDirection.down);
        }
    }

    public override void ProcessAndUpdate(double delta)
    {
        if (CurrentState == State.Dead)
            return;

        ProcessUserInput(delta);
        if (UpdateNodeObjectPosition())
            (NodeObject as Rockford).PlayAudio();
    }

    public override void Dead()
    {
        base.Dead();
        CurrentState = State.Dead;
    }
}