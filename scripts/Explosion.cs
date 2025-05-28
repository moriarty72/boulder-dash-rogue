using Godot;
using System;

public partial class Explosion : Area2D
{
    public override void _Ready()
    {
        AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite2D.Play("explosion");
    }

}
