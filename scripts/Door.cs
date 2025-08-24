using Godot;
using System;

public partial class Door : Area2D
{
    private AnimatedSprite2D animatedSprite2D;

    public override void _Ready()
    {
        base._Ready();

        animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    public void SetColorAndState(string color, int isOpen)
    {
        animatedSprite2D.Animation = color;
        animatedSprite2D.Frame = isOpen;
    }
}
