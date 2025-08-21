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
    Key,
    Diamond,
    Explosion,
    Amoeba,
    EnemySquare,
    EnemyButterfly
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

    private PackedScene currentPackedScene;

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
        currentPackedScene = packedScene;

        NodeObject = packedScene.Instantiate<T>();
        NodeObject.GlobalPosition = worldPosition;

        mainController.AddChild(NodeObject);
    }

    public void InitializeEmpty(Main mc, Vector2 worldPosition, Vector2I gridPosition)
    {
        ID = new Guid();
        mainController = mc;
        Type = ItemType.None;
        WorldPosition = worldPosition;
        GridPosition = gridPosition;
        PrevGridPosition = gridPosition;

        NodeObject = null;
    }

    public virtual void Respawn()
    {
        if (Type != ItemType.None)
        {
            NodeObject = (Node2D)currentPackedScene.Instantiate();
            NodeObject.GlobalPosition = WorldPosition;

            mainController.AddChild(NodeObject);
        }
    }

    public void RemoveNodeFromScene()
    {
        NodeObject?.QueueFree();
        NodeObject = null;
    }

    public BaseGridObjectController Clone()
    {
        return new()
        {
            ID = ID,
            mainController = mainController,
            Type = Type,
            WorldPosition = WorldPosition,
            GridPosition = GridPosition,
            PrevGridPosition = GridPosition,
            currentPackedScene = currentPackedScene
        };
    }

    protected bool UpdateNodeObjectPosition()
    {
        if (Type == ItemType.None)
            return false;

        if (PrevGridPosition != GridPosition)
        {
            try
            {
                NodeObject.GlobalPosition = new(GridPosition.X * 64, GridPosition.Y * 64);
                mainController.SwapGridItems(PrevGridPosition, GridPosition, true);

                PrevGridPosition = GridPosition;
            }
            catch (Exception ex)
            {
                GD.Print("BaseGridObjectController exception: ", ex.Message, " object type: ", Type, " position ", GridPosition);
            }
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

    public static BaseGridObjectController SpawnNone(Main mc, Vector2I gridPosition)
    {
        BaseGridObjectController noneObject = new()
        {
            Type = ItemType.None,
            mainController = mc,
            GridPosition = gridPosition
        };

        return noneObject;
    }
}