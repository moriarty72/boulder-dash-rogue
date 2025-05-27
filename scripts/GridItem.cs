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

    public GridItem(ItemType type, Vector2I position)
    {
        Type = type;
        Position = position;
    }

    public GridItem(ItemType type, int x, int y)
    {
        Type = type;
        Position = new Vector2I(x, y);
    }

}