using Godot;
using System;
using System.Linq;

public partial class Key : Area2D
{
    private String[] spriteNames = ["Red", "Green", "Blue", "Yellow", "Silver"];
    private Sprite2D[] sprites = [null, null, null, null, null, null];
    public override void _Ready()
    {
        base._Ready();
        for (int i = 0; i < spriteNames.Count(); i++)
        {
            sprites[i] = GetNode<Sprite2D>(spriteNames[i]);
        }
    }

    public void SetVisibleByColor(int color)
    {
        sprites[color].Visible = true;
    }
}
