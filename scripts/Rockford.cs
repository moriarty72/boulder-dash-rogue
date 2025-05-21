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

    private Main mainController;

    private AnimatedSprite2D animatedSprite2D;
    private Vector2 currentPosition;
    private MoveDirection lastHorizontalMove = MoveDirection.right;
    private MoveDirection currentMoveDirection = MoveDirection.none;
    private double lastMoveTick = 0;
    private double idleInputDelayTime = 0;

    public void Initilize(Main mc)
    {
        mainController = mc;
    }

    public override void _Ready()
    {
        currentPosition = new(0.0f, 0.0f);
        GlobalPosition = currentPosition;

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
            PlayAnimation(lastHorizontalMove);
        else if (moveDirection == MoveDirection.none)
            animatedSprite2D.Play("stand");
    }

    public void Move(MoveDirection moveDirection)
    {
        if (moveDirection == MoveDirection.up)
            currentPosition.Y -= 64;

        if (moveDirection == MoveDirection.left)
            currentPosition.X -= 64;

        if (moveDirection == MoveDirection.right)
            currentPosition.X += 64;

        if (moveDirection == MoveDirection.down)
            currentPosition.Y += 64;

        PlayAnimation(moveDirection);
        GlobalPosition = currentPosition;
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
        // GD.Print("Rockford collide " + body.Name);
        mainController.OnPlayerCollide(body);
    }
}
