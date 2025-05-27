using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
    // Resolution Refs:
    // window res: 1280x768
    // sprite size: 64x64
    // grid tile size: 20x12
    /*
    public const int GRID_WIDTH = 20;
    public const int GRID_HEIGHT = 12;
    */

    public const int SPRITE_WIDTH = 64;
    public const int SPRITE_HEIGHT = 64;

    [Export]
    public double disableInputDelay = 1.0;

    [Export]
    public Vector2I testLevelGridSize = new(20, 12);

    [Export]
    public Vector2I testRockfordPosition = new(1, 1);

    [Export]
    public int rockPerColumn = 10;

    private Rockford player;

    private PackedScene playerScene = GD.Load<PackedScene>("res://scenes//rockford.tscn");
    private PackedScene mudScene = GD.Load<PackedScene>("res://scenes//mud-1.tscn");
    private PackedScene rockScene = GD.Load<PackedScene>("res://scenes//rock.tscn");
    private PackedScene metalWallScene = GD.Load<PackedScene>("res://scenes//metal-wall.tscn");

    private List<GridItem> levelGrid = [];

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
        InitilizeLevelGrid();
        SpawnTestLevel();
        SpawnRockford();
    }

    private int GetGridIndex(int x, int y)
    {
        int index = x + y * testLevelGridSize.X;
        return index;
    }

    private void AddGridItem(ItemType type, int x, int y, Node2D nodeObject)
    {
        int index = GetGridIndex(x, y);
        levelGrid[index] = new GridItem(type, x, y, nodeObject);
    }

    private void InitilizeLevelGrid()
    {
        Camera2D camera2D = GetNode<Camera2D>("Camera2D");

        camera2D.LimitTop = camera2D.LimitLeft = 0;
        camera2D.LimitRight = testLevelGridSize.X * SPRITE_WIDTH;
        camera2D.LimitBottom = testLevelGridSize.Y * SPRITE_HEIGHT;

        for (int x = 0; x < testLevelGridSize.X; x++)
        {
            for (int y = 0; y < testLevelGridSize.Y; y++)
            {
                levelGrid.Add(null);
            }
        }
    }

    private GridItem GetGridItem(int x, int y)
    {
        int index = GetGridIndex(x, y);
        return levelGrid[index];
    }

    private T AddObject<T>(PackedScene packedScene, float x, float y) where T : Node2D
    {
        var obj = packedScene.Instantiate<T>();
        obj.GlobalPosition = new(x, y);

        AddChild(obj);

        return obj;
    }

    private void SpawnTestLevel()
    {
        Random rnd = new(System.Environment.TickCount);

        for (int x = 0; x < testLevelGridSize.X; x++)
        {
            int rockToSpawn = rockPerColumn;
            for (int y = 0; y < testLevelGridSize.Y; y++)
            {
                if ((x == testRockfordPosition.X) && (y == testRockfordPosition.Y))
                    continue;

                bool isCorner = ((x == 0) && (y == 0)) || ((x == 0) && (y == (testLevelGridSize.Y - 1))) || ((x == (testLevelGridSize.X - 1)) && (y == 0)) || ((x == (testLevelGridSize.X - 1)) && (y == (testLevelGridSize.Y - 1)));
                bool isSide = ((x > 0) && (x < testLevelGridSize.X) && ((y == 0) || (y == (testLevelGridSize.Y - 1)))) || ((y > 0) && (y < testLevelGridSize.Y) && ((x == 0) || (x == (testLevelGridSize.X - 1))));

                if (isCorner || isSide)
                {
                    MetalWall metalWall = AddObject<MetalWall>(metalWallScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                    AddGridItem(ItemType.MetalWall, x, y, metalWall);
                }
                else
                {
                    if ((x > 2) && (y > 2) && ((rnd.Next(1, 100) % 5) == 0) && (rockToSpawn > 0))
                    {
                        Rock rock = AddObject<Rock>(rockScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                        rock.Initilize(this, new(x, y));

                        rockToSpawn--;
                        AddGridItem(ItemType.Rock, x, y, rock);
                    }
                    else
                    {
                        Mud1 mud1 = AddObject<Mud1>(mudScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                        AddGridItem(ItemType.Mud, x, y, mud1);
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
        player.Initilize(this, new(testRockfordPosition.X * SPRITE_WIDTH, testRockfordPosition.Y * SPRITE_HEIGHT), testRockfordPosition);

        Camera2D camera2D = GetNode<Camera2D>("Camera2D");
        RemoteTransform2D remoteTransform2D = new()
        {
            RemotePath = camera2D.GetPath()
        };
        player.AddChild(remoteTransform2D);

        AddGridItem(ItemType.Rockford, testRockfordPosition.X, testRockfordPosition.Y, player);
        AddChild(player);
    }

    public bool CanPlayerMove(Vector2I nextPlayerPosition)
    {
        GridItem gridItem = GetGridItem(nextPlayerPosition.X, nextPlayerPosition.Y);

        if (gridItem != null)
        {
            switch (gridItem.Type)
            {
                case ItemType.Rock:
                    return false;

                case ItemType.MetalWall:
                    return false;
            }
        }
        return true;
    }

    public bool CanRockFall(Vector2I rockPosition)
    {
        GridItem gridItem = GetGridItem(rockPosition.X, rockPosition.Y + 1);
        return gridItem.Type == ItemType.None;
    }

    public void SwapGridItems(Vector2I prevPosition, Vector2I newPosition, bool replaceNext)
    {
        int prevIndex = GetGridIndex(prevPosition.X, prevPosition.Y);
        int nextIndex = GetGridIndex(newPosition.X, newPosition.Y);

        GridItem prevGridItem = levelGrid[prevIndex];
        GridItem nextGridItem = levelGrid[nextIndex];

        if (replaceNext)
        {
            if (nextGridItem.NodeObject != null)
                nextGridItem.NodeObject.QueueFree();
            levelGrid[prevIndex] = new(ItemType.None, prevGridItem.Position, null);
            levelGrid[nextIndex] = new(prevGridItem.Type, prevGridItem.Position, prevGridItem.NodeObject);
        }
        else
        {
            levelGrid[prevIndex] = new(nextGridItem.Type, nextGridItem.Position, nextGridItem.NodeObject);
            levelGrid[nextIndex] = new(prevGridItem.Type, prevGridItem.Position, prevGridItem.NodeObject);
        }
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

    public void OnPlayerMove()
    {

    }

    public void OnPlayerCollide(Node2D collisionNode, Vector2I gridPosition)
    {
        if (collisionNode is Mud1)
        {
            AddGridItem(ItemType.None, gridPosition.X, gridPosition.Y, null);
            collisionNode.QueueFree();
        }
    }
}
