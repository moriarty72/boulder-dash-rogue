using Godot;
using System;

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
    Butterfly,
    Explosion
}

public class BaseGridObject 
{
    public Guid ID;

    public ItemType Type;
    public Vector2I GridPosition;

    protected Main mainController;
    public Vector2 WorldPosition;
    public Node2D NodeObject { get; private set; }

    public BaseGridObject() 
    {

    }

    public virtual void Initialize<T>(Main mc, PackedScene packedScene, ItemType itemType, Vector2 worldPosition, Vector2I gridPosition) where T : Node2D
    {
        ID = new Guid();
        mainController = mc;
        Type = itemType;
        WorldPosition = worldPosition;
        GridPosition = gridPosition;

        NodeObject = packedScene.Instantiate<T>();
        NodeObject.GlobalPosition = worldPosition;

        mainController.AddChild(NodeObject);
    }

    public virtual void Process(double delta)
    {
        if (Type == ItemType.None)
            return;
            
        (NodeObject as IBaseGridObject)?.Process(mainController, this, delta);
    }

    public void Dead()
    {
        if (Type != ItemType.None)
        {
            NodeObject.QueueFree();
            Type = ItemType.None;
        }
    }
}