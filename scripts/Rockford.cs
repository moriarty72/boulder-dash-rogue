using Godot;
using System;

public partial class Rockford : Area2D, IBaseGridObject
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

    private AnimatedSprite2D animatedSprite2D;
    private Vector2 currentPosition;
    private MoveDirection lastHorizontalMove = MoveDirection.right;
    private MoveDirection currentMoveDirection = MoveDirection.none;
    private double lastMoveTick = 0;
    private double idleInputDelayTime = 0;
    private bool firePressed = false;

    public State rockfordState = State.Alive;

    public Rockford()
    {

    }

    public override void _Ready()
    {
        base._Ready();

        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite2D.Play("stand");

        currentPosition = GlobalPosition;
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

    public void Move(Main mainController, BaseGridObject gridObject, MoveDirection moveDirection)
    {
        if (moveDirection == MoveDirection.none)
        {
            PlayAnimation(moveDirection);
            return;
        }

        Vector2I prevGridPosition = new(gridObject.GridPosition.X, gridObject.GridPosition.Y);

        if (!firePressed)
        {
            if (moveDirection == MoveDirection.up)
            {
                gridObject.GridPosition.Y--;
                currentPosition.Y -= 64;
            }

            if (moveDirection == MoveDirection.left)
            {
                gridObject.GridPosition.X--;
                currentPosition.X -= 64;
            }

            if (moveDirection == MoveDirection.right)
            {
                gridObject.GridPosition.X++;
                currentPosition.X += 64;
            }

            if (moveDirection == MoveDirection.down)
            {
                gridObject.GridPosition.Y++;
                currentPosition.Y += 64;
            }

            bool canPlayerMove = mainController.CanRockfordMove(gridObject.GridPosition, moveDirection);
            if (canPlayerMove)
            {
                PlayAnimation(moveDirection);
                gridObject.nodeObject.GlobalPosition = currentPosition;

                mainController.SwapGridItems(prevGridPosition, gridObject.GridPosition, true);
            }
            else
            {
                gridObject.GridPosition = prevGridPosition;
                currentPosition = gridObject.nodeObject.GlobalPosition;
            }
        }
        else
        {
            mainController.RockfordFireAction(prevGridPosition, moveDirection);
        }

        GD.Print("Rockford next grid position " + gridObject.GridPosition + " prev position " + prevGridPosition);
    }

    private void HandleMultipleUserInput(Main mainController, BaseGridObject gridObject, double delta)
    {
        Main.UserEvent inputEvent = mainController.GetInputEvent(delta);

        firePressed = (inputEvent & Main.UserEvent.ueFire) == Main.UserEvent.ueFire;

        if (inputEvent == Main.UserEvent.ueNone)
        {
            idleInputDelayTime += delta;
            if (idleInputDelayTime > 0.15)
            {
                idleInputDelayTime = 0;
                Move(mainController, gridObject, MoveDirection.none);
            }
        }

        if ((inputEvent & Main.UserEvent.ueLeft) == Main.UserEvent.ueLeft)
        {
            idleInputDelayTime = 0;
            Move(mainController, gridObject, MoveDirection.left);
        }
        else if ((inputEvent & Main.UserEvent.ueRight) == Main.UserEvent.ueRight)
        {
            idleInputDelayTime = 0;
            Move(mainController, gridObject, MoveDirection.right);
        }
        else if ((inputEvent & Main.UserEvent.ueUp) == Main.UserEvent.ueUp)
        {
            idleInputDelayTime = 0;
            Move(mainController, gridObject, MoveDirection.up);
        }
        else if ((inputEvent & Main.UserEvent.ueDown) == Main.UserEvent.ueDown)
        {
            idleInputDelayTime = 0;
            Move(mainController, gridObject, MoveDirection.down);
        }
    }

    public void Process(Main mainController, BaseGridObject gridObject, double delta)
    {
        if (rockfordState == State.Dead)
            return;

        HandleMultipleUserInput(mainController, gridObject, delta);
    }
}
