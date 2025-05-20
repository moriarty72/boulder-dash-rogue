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

    private AnimatedSprite2D animatedSprite2D;
    private Vector2 currentPosition;
    private MoveDirection lastHorizontalMove = MoveDirection.right;
    private MoveDirection currentMoveDirection = MoveDirection.none;
    private double lastMoveTick = 0;

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
    }

    private void _on_player_body_entered(Node2D body)
    {
        GD.Print("Rockford collide");
    }
}
