using Godot;
using System;

public partial class Explosion : Area2D
{
    public event Action AnimationEnd;

    public Explosion()
    {
    }

    public override void _Ready()
    {
        AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite2D.Play("explosion");
    }

    public void OnAnimationEnd()
    {
        // mainController.RemoveGridItem(GridPosition);
        AnimationEnd?.Invoke();
    }
}
