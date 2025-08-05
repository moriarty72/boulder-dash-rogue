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
    Explosion,
    Amoeba,
    EnemySquare
}

public class BaseGridObjectController
{
    public Guid ID;

    public ItemType Type;
    public Vector2I GridPosition;
    public Vector2I PrevGridPosition;

    protected Main mainController;
    public Vector2 WorldPosition;
    public Node2D NodeObject { get; private set; }

    public BaseGridObjectController()
    {

    }

    public virtual void Initialize<T>(Main mc, PackedScene packedScene, ItemType itemType, Vector2 worldPosition, Vector2I gridPosition) where T : Node2D
    {
        ID = new Guid();
        mainController = mc;
        Type = itemType;
        WorldPosition = worldPosition;
        GridPosition = gridPosition;
        PrevGridPosition = gridPosition;

        NodeObject = packedScene.Instantiate<T>();
        NodeObject.GlobalPosition = worldPosition;

        mainController.AddChild(NodeObject);
    }

    protected bool UpdateNodeObjectPosition()
    {
        if (PrevGridPosition != GridPosition)
        {
            NodeObject.GlobalPosition = new(GridPosition.X * 64, GridPosition.Y * 64);
            mainController.SwapGridItems(PrevGridPosition, GridPosition, true);

            PrevGridPosition = GridPosition;

            return true;
        }
        return false;
    }
    
    public virtual void ProcessAndUpdate(double delta)
    {
        if (Type == ItemType.None)
            return;

        (NodeObject as IBaseGridObject)?.Process(mainController, this, delta);
    }

    public virtual void Dead()
    {
        if (Type != ItemType.None)
        {
            NodeObject.QueueFree();
            NodeObject = null;

            Type = ItemType.None;
        }
    }
}