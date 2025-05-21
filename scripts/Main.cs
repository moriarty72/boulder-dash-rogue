using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
    // window res: 1280x768
    // sprite size: 64x64
    // grid size: 20x12
    public const int GRID_WIDTH = 20;
    public const int GRID_HEIGHT = 12;

    public const int SPRITE_WIDTH = 64;
    public const int SPRITE_HEIGHT = 64;

    [Export]
    public double disableInputDelay = 1.0;

    private Rockford player;

    private PackedScene playerScene = GD.Load<PackedScene>("res://scenes//rockford.tscn");
    private PackedScene mudScene = GD.Load<PackedScene>("res://scenes//mud-1.tscn");
    private PackedScene rockScene = GD.Load<PackedScene>("res://scenes//rock.tscn");


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
        SpawnMud();
    }

    private void AddObject<T>(PackedScene packedScene, float x, float y) where T : Node2D
    {
        var obj = packedScene.Instantiate<T>();
        obj.GlobalPosition = new(x, y);

        AddChild(obj);
    }

    private void SpawnMud()
    {
        int rockCount = 15;
        for (int x = 0; x <= GRID_WIDTH; x++)
        {
            for (int y = 0; y <= GRID_HEIGHT; y++)
            {
                if ((x != 0) || (y != 0))
                {
                    Random rnd = new Random(System.Environment.TickCount);
                    if ((rockCount >= 0) && ((rnd.Next(0, 10) % 2) == 0))
                    {
                        AddObject<Rock>(rockScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                        rockCount--;
                    }
                    else
                    {
                        AddObject<Mud1>(mudScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                    }
                }
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
        player.Initilize(this);

        AddChild(player);
    }

    private void GameLoopManagement(double delta)
    {
        if (player == null)
            return;

        /*
        UserEvent inputEvent = GetInputEvent(delta);
        if (inputEvent == UserEvent.ueNone)
        {
            idleInputDelayTime += delta;
            if (idleInputDelayTime > 0.15)
            {
                idleInputDelayTime = 0;
                player.Move(Rockford.MoveDirection.none);
            }
        }
        else if (inputEvent == UserEvent.ueLeft)
        {
            idleInputDelayTime = 0;
            player.Move(Rockford.MoveDirection.left);
        }
        else if (inputEvent == UserEvent.ueRight)
        {
            idleInputDelayTime = 0;
            player.Move(Rockford.MoveDirection.right);
        }
        else if (inputEvent == UserEvent.ueUp)
        {
            idleInputDelayTime = 0;
            player.Move(Rockford.MoveDirection.up);
        }
        else if (inputEvent == UserEvent.ueDown)
        {
            idleInputDelayTime = 0;
            player.Move(Rockford.MoveDirection.down);
        }
        */
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
