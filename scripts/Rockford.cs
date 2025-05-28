using Godot;
using System;

public partial class Rockford : Area2D
{
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

        bool canPlayerMove = mainController.CanPlayerMove(GridPosition);
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
        GD.Print("Rockford next grid position " + GridPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Main.UserEvent inputEvent = mainController.GetInputEvent(delta);
        if (inputEvent == Main.UserEvent.ueNone)
        {
            idleInputDelayTime += delta;
            if (idleInputDelayTime > 0.15)
            {
                idleInputDelayTime = 0;
                Move(MoveDirection.none);
            }
        }
        else if (inputEvent == Main.UserEvent.ueLeft)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.left);
        }
        else if (inputEvent == Main.UserEvent.ueRight)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.right);
        }
        else if (inputEvent == Main.UserEvent.ueUp)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.up);
        }
        else if (inputEvent == Main.UserEvent.ueDown)
        {
            idleInputDelayTime = 0;
            Move(MoveDirection.down);
        }

    }

    private void _on_player_body_entered(Node2D body)
    {
        // if (body is Rock)
        // {
        //     Rock rock = body as Rock;
        //     GD.Print("Rockford collide " + rock.CurrentState);
        // }
            
        // mainController.OnPlayerCollide(body, GridPosition);
    }
}
