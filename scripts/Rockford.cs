using Godot;
using System;

public partial class Rockford : Area2D, IGridItem
{
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

    public Vector2I GridPosition;

    private Main mainController;

    private AnimatedSprite2D animatedSprite2D;
    private Vector2 currentPosition;
    private MoveDirection lastHorizontalMove = MoveDirection.right;
    private MoveDirection currentMoveDirection = MoveDirection.none;
    private double lastMoveTick = 0;
    private double idleInputDelayTime = 0;
    private bool firePressed = false;

    public State rockfordState = State.Alive;

    public void Initilize(Main mc, Vector2 position, Vector2I gridPosition)
    {
        mainController = mc;
        currentPosition = position;
        GlobalPosition = currentPosition;
        GridPosition = gridPosition;
    }

    public override void _Ready()
    {
        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite2D.Play("stand");
    }

    private void PlayAnimation(MoveDirection moveDirection)
    {
        if (currentMoveDirection == moveDirection)
            return;

        currentMoveDirection = moveDirection;
        if (moveDirection == MoveDirection.left)
        {
            animatedSprite2D.Play("left");
            lastHorizontalMove = MoveDirection.left;
        }
        else if (moveDirection == MoveDirection.right)
        {
            animatedSprite2D.Play("right");
            lastHorizontalMove = MoveDirection.right;
        }
        else if ((moveDirection == MoveDirection.up) || (moveDirection == MoveDirection.down))
        {
            PlayAnimation(lastHorizontalMove);
        }
        else if (moveDirection == MoveDirection.none)
            animatedSprite2D.Play("stand");
    }

    public void Move(MoveDirection moveDirection)
    {
        if (moveDirection == MoveDirection.none)
        {
            PlayAnimation(moveDirection);
            return;
        }

        Vector2I prevGridPosition = new(GridPosition.X, GridPosition.Y);

        if (!firePressed)
        {
            if (moveDirection == MoveDirection.up)
            {
                GridPosition.Y--;
                currentPosition.Y -= 64;
            }

            if (moveDirection == MoveDirection.left)
            {
                GridPosition.X--;
                currentPosition.X -= 64;
            }

            if (moveDirection == MoveDirection.right)
            {
                GridPosition.X++;
                currentPosition.X += 64;
            }

            if (moveDirection == MoveDirection.down)
            {
                GridPosition.Y++;
                currentPosition.Y += 64;
            }

            bool canPlayerMove = mainController.CanRockfordMove(GridPosition, moveDirection);
            if (canPlayerMove)
            {
                PlayAnimation(moveDirection);
                GlobalPosition = currentPosition;

                mainController.SwapGridItems(prevGridPosition, GridPosition, true);
            }
            else
            {
                GridPosition = prevGridPosition;
                currentPosition = GlobalPosition;
            }
        }
        else
        {
            mainController.RockfordFireAction(prevGridPosition, moveDirection);
        }

        GD.Print("Rockford next grid position " + GridPosition);
    }

    private void HandleMultipleUserInput(double delta)
    {
        Main.UserEvent inputEvent = mainController.GetInputEvent(delta);

        firePressed = (inputEvent & Main.UserEvent.ueFire) == Main.UserEvent.ueFire;

        if (inputEvent == Main.UserEvent.ueNone)
        {
            idleInputDelayTime += delta;
            if (idleInputDelayTime > 0.15)
            {
                idleInputDelayTime = 0;
                Move(MoveDirection.none);
            }
        }

        if ((inputEvent & Main.UserEvent.ueLeft) == Main.UserEvent.ueLeft)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.left);
        }
        else if ((inputEvent & Main.UserEvent.ueRight) == Main.UserEvent.ueRight)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.right);
        }
        else if ((inputEvent & Main.UserEvent.ueUp) == Main.UserEvent.ueUp)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.up);
        }
        else if ((inputEvent & Main.UserEvent.ueDown) == Main.UserEvent.ueDown)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.down);
        }
    }

    public void Process(double delta)
    {
        if (rockfordState == State.Dead)
            return;

        base._PhysicsProcess(delta);
        HandleMultipleUserInput(delta);
    }

    public void Dead()
    {
        rockfordState = State.Dead;
    }
}
