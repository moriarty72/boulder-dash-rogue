using System;
using Godot;

public enum ItemType
{
    None,
    Mud,
    Rockford,
    MetalWall,
    Rock,
    Door,
    DoorOpen,
    Diamond,
    Butterfly
}

public class GridItem
{
    public ItemType Type;
    public Vector2I Position;
    public Node2D NodeObject;

    public GridItem(ItemType type, Vector2I position, Node2D nodeObject)
    {
        Type = type;
        Position = position;
        NodeObject = nodeObject;
    }

    public GridItem(ItemType type, int x, int y, Node2D nodeObject)
    {
        Type = type;
        Position = new Vector2I(x, y);
        NodeObject = nodeObject;
    }

}