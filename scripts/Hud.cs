using Godot;
using System;

public partial class Hud : CanvasLayer
{
    public void Update()
    {
        GetNode<Label>("ScoreLabel").Text = "Score: " + RockfordStatusBag.Score;   
    }
}
