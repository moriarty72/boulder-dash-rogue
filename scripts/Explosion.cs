using Godot;
using System;

public partial class Explosion : Area2D, IGridItem
{
    public Vector2I GridPosition;
    private Main mainController;

    public override void _Ready()
    {
        AnimatedSprite2D animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite2D.Play("explosion");
    }

    public void OnAnimationEnd()
    {
        mainController.RemoveGridItem(GridPosition);
    }

    public void Initilize(Main mc, Vector2I gridPosition)
    {
        mainController = mc;
        GridPosition = gridPosition;
    }

    public void Dead()
    {
        
    }
}
