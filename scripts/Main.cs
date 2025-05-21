using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
    // Resolution Refs:
    // window res: 1280x768
    // sprite size: 64x64
    // grid tile size: 20x12

    public const int GRID_WIDTH = 20;
    public const int GRID_HEIGHT = 12;

    public const int SPRITE_WIDTH = 64;
    public const int SPRITE_HEIGHT = 64;

    [Export]
    public double disableInputDelay = 1.0;

    [Export]
    public Vector2I testLevelGridSize = new(20, 12);

    [Export]
    public Vector2I testRockfordPosition = new(1, 1);

    private Rockford player;

    private PackedScene playerScene = GD.Load<PackedScene>("res://scenes//rockford.tscn");
    private PackedScene mudScene = GD.Load<PackedScene>("res://scenes//mud-1.tscn");
    private PackedScene rockScene = GD.Load<PackedScene>("res://scenes//rock.tscn");
    private PackedScene metalWallScene = GD.Load<PackedScene>("res://scenes//metal-wall.tscn");


    public enum UserEvent
    {
        ueNone,
        ueLeft,
        ueRight,
        ueUp,
        ueDown,
        ueFire
    }
    private Queue<UserEvent> eventQueue = new Queue<UserEvent>();
    private double disableInputDelayTime = 0;
    private double idleInputDelayTime = 0;

    private bool inputEnabled = true;

    public override void _Ready()
    {
        SpawnRockford();
        SpawnTestLevel();
    }

    private void AddObject<T>(PackedScene packedScene, float x, float y) where T : Node2D
    {
        var obj = packedScene.Instantiate<T>();
        obj.GlobalPosition = new(x, y);

        AddChild(obj);
    }

    private void SpawnTestLevel()
    {
        for (int x = 0; x < testLevelGridSize.X; x++)
        {
            for (int y = 0; y < testLevelGridSize.Y; y++)
            {
                bool isCorner = ((x == 0) && (y == 0)) || ((x == 0) && (y == (testLevelGridSize.Y - 1))) || ((x == (testLevelGridSize.X - 1)) && (y == 0)) || ((x == (testLevelGridSize.X - 1)) && (y == (testLevelGridSize.Y - 1)));
                bool isSide = ((x > 0) && (x < GRID_WIDTH) && ((y == 0) || (y == (testLevelGridSize.Y - 1)))) || ((y > 0) && (y < GRID_HEIGHT) && ((x == 0) || (x == (testLevelGridSize.X - 1))));

                if (isCorner || isSide)
                    AddObject<MetalWall>(metalWallScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                else
                    AddObject<Mud1>(mudScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
            }
        }
    }

    public UserEvent GetInputEvent(double delta)
    {
        if (eventQueue.Count == 0)
            return UserEvent.ueNone;

        return eventQueue.Dequeue();
    }

    private void DisableInput(double delay)
    {
        inputEnabled = false;
        disableInputDelay = delay;
        eventQueue.Clear();
    }

    private void SpawnRockford()
    {
        player = playerScene.Instantiate<Rockford>();
        player.Initilize(this, new(testRockfordPosition.X * SPRITE_WIDTH, testRockfordPosition.Y * SPRITE_HEIGHT));
        
        AddChild(player);
    }

    private void GameLoopManagement(double delta)
    {

    }

    private void HandleInput(double delta)
    {
        if (!inputEnabled)
        {
            disableInputDelayTime += delta;
            if (disableInputDelayTime < disableInputDelay)
                return;

            disableInputDelayTime = 0;
            inputEnabled = true;
        }

        if (Input.IsActionPressed("ui_left"))
        {
            eventQueue.Enqueue(UserEvent.ueLeft);
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            eventQueue.Enqueue(UserEvent.ueRight);
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_up"))
        {
            eventQueue.Enqueue(UserEvent.ueUp);
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            eventQueue.Enqueue(UserEvent.ueDown);
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_accept"))
            eventQueue.Enqueue(UserEvent.ueFire);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleInput(delta);
        GameLoopManagement(delta);
    }

    public void OnPlayerCollide(Node2D collisionNode)
    {
        if (collisionNode is Mud1)
        {
            collisionNode.QueueFree();
        }
    }

}
