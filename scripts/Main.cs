using Godot;
using System;
using System.Collections.Generic;
using static BaseStaticObject;

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

    [Export]
    public int diamondPerColumn = 10;

    private Rockford player;
    private List<GridItem> levelRocks = [];
    private List<GridItem> levelDiamonds = [];

    private PackedScene playerScene = GD.Load<PackedScene>("res://scenes//rockford.tscn");
    private PackedScene mudScene = GD.Load<PackedScene>("res://scenes//mud-1.tscn");
    private PackedScene rockScene = GD.Load<PackedScene>("res://scenes//rock.tscn");
    private PackedScene diamondScene = GD.Load<PackedScene>("res://scenes//diamond.tscn");
    private PackedScene metalWallScene = GD.Load<PackedScene>("res://scenes//metal-wall.tscn");
    private PackedScene explosionScene = GD.Load<PackedScene>("res://scenes//explosion.tscn");

    private List<GridItem> levelGrid = [];

    public enum UserEvent
    {
        ueNone = 0,
        ueLeft = 1,
        ueRight = 2,
        ueUp = 4,
        ueDown = 8,
        ueFire = 16
    }

    private enum GameState
    {
        gsInitialize,
        gsTitle,
        gsPlay,
        gsRockfordDead,
        gsGameOver
    }

    private Queue<UserEvent> eventQueue = new Queue<UserEvent>();
    private double disableInputDelayTime = 0;
    private double idleInputDelayTime = 0;

    private bool inputEnabled = true;
    private GameState gameState = GameState.gsInitialize;

    public override void _Ready()
    {
    }

    private void InitTestLevel()
    {
        CleanupLevelObjects();
        InitilizeLevelGrid();
        SpawnTestLevel();
        SpawnRockford();
    }

    private int GetGridIndex(int x, int y)
    {
        int index = x + y * testLevelGridSize.X;
        return index;
    }

    private GridItem AddGridItem(ItemType type, int x, int y, Node2D nodeObject)
    {
        int index = GetGridIndex(x, y);
        levelGrid[index] = new GridItem(type, x, y, nodeObject);

        return levelGrid[index];
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

    private void CleanupLevelObjects()
    {
        levelGrid.ForEach(item =>
        {
            if (item.Type != ItemType.None)
                item.NodeObject.QueueFree();
        });
        levelGrid = [];
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

    public void RemoveGridItem(Vector2I position)
    {
        int index = GetGridIndex(position.X, position.Y);
        levelGrid[index].Dead();
        levelGrid[index] = new(ItemType.None, position, null); ;
    }

    private void SpawnTestLevel()
    {
        Random rnd = new(System.Environment.TickCount);

        levelRocks = [];
        for (int x = 0; x < testLevelGridSize.X; x++)
        {
            int rockToSpawn = rockPerColumn;
            int diamondToSpawn = diamondPerColumn;

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
                    // if ((x == 4) && (y == 4))
                    if ((x > 2) && (y > 2) && ((rnd.Next(1, 100) % 5) == 0) && (rockToSpawn > 0))
                    {
                        Rock rock = AddObject<Rock>(rockScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                        rock.Initilize(this, new(x, y));

                        rockToSpawn--;
                        levelRocks.Add(AddGridItem(ItemType.Rock, x, y, rock));
                    }
                    else if ((x > 2) && (y > 2) && ((rnd.Next(1, 100) % 5) == 0) && (diamondToSpawn > 0))
                    {
                        Diamond diamond = AddObject<Diamond>(diamondScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                        diamond.Initilize(this, new(x, y));

                        diamondToSpawn--;
                        levelDiamonds.Add(AddGridItem(ItemType.Diamond, x, y, diamond));
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

    private bool CanRockfordPushRock(GridItem rockGridItem, Vector2I nextPlayerPosition, Rockford.MoveDirection rockfordDirection)
    {
        if ((rockfordDirection == Rockford.MoveDirection.left) || (rockfordDirection == Rockford.MoveDirection.right))
        {
            // check empty space after rock
            int x = rockGridItem.Position.X + (rockfordDirection == Rockford.MoveDirection.left ? -1 : 1);
            GridItem afterRockGridItem = GetGridItem(x, nextPlayerPosition.Y);
            if (afterRockGridItem.Type == ItemType.None)
            {
                GD.Print("Rockford can move rock direction: " + rockfordDirection.ToString());
                return true;
            }
        }
        return false;
    }

    public bool CanRockfordMove(Vector2I nextPlayerPosition, Rockford.MoveDirection rockfordDirection)
    {
        GridItem gridItem = GetGridItem(nextPlayerPosition.X, nextPlayerPosition.Y);

        if (gridItem != null)
        {
            switch (gridItem.Type)
            {
                case ItemType.Rock:
                    {
                        if (CanRockfordPushRock(gridItem, nextPlayerPosition, rockfordDirection))
                            (gridItem.NodeObject as Rock).UpdateCurrentState(rockfordDirection == Rockford.MoveDirection.left ? State.PushedLeft : State.PushedRight);
                        return false;
                    }

                case ItemType.Diamond:
                    {
                        gridItem.Dead();
                        levelDiamonds.Remove(gridItem);

                        return true;
                    }

                case ItemType.MetalWall:
                    return false;
            }
        }
        return true;
    }

    public State CheckObjectState(BaseStaticObject objectItem, Vector2I rockPosition)
    {
        switch (objectItem.CurrentState)
        {
            case State.Stand:
                {
                    GridItem gridItem = GetGridItem(rockPosition.X, rockPosition.Y + 1);
                    if (gridItem.Type == ItemType.None)
                        return State.Fall;

                    if ((gridItem.Type == ItemType.Rock) || (gridItem.Type == ItemType.Diamond))
                    {
                        if (objectItem.CurrentState == State.Stand)
                        {
                            gridItem = GetGridItem(rockPosition.X - 1, rockPosition.Y + 1);
                            GridItem gridItemLeft = GetGridItem(rockPosition.X - 1, rockPosition.Y);
                            if ((gridItem.Type == ItemType.None) && (gridItemLeft.Type == ItemType.None))
                                return State.FallLeft;

                            gridItem = GetGridItem(rockPosition.X + 1, rockPosition.Y + 1);
                            GridItem gridItemRight = GetGridItem(rockPosition.X + 1, rockPosition.Y);
                            if ((gridItem.Type == ItemType.None) && (gridItemRight.Type == ItemType.None))
                                return State.FallRight;
                        }
                    }
                    break;
                }

            case State.PushedRight:
            case State.PushedLeft:
                {
                    return objectItem.CurrentState;
                }
        }
        return State.Stand;
    }

    public bool CheckRockfordCollision(Vector2I rockPosition, int offsetX, int offsetY)
    {
        GridItem gridItem = GetGridItem(rockPosition.X + offsetX, rockPosition.Y + offsetY);
        if (gridItem.Type == ItemType.Rockford)
        {
            GD.Print("ROCKFORD DEAD !!!!!!");
            gameState = GameState.gsRockfordDead;
        }

        return false;
    }

    public void RockfordFireAction(Vector2I position, Rockford.MoveDirection direction)
    {
        GridItem gridItem = null;

        if (direction == Rockford.MoveDirection.up)
            gridItem = GetGridItem(position.X, position.Y - 1);
        else if (direction == Rockford.MoveDirection.left)
            gridItem = GetGridItem(position.X - 1, position.Y);
        else if (direction == Rockford.MoveDirection.down)
            gridItem = GetGridItem(position.X, position.Y + 1);
        else if (direction == Rockford.MoveDirection.right)
            gridItem = GetGridItem(position.X + 1, position.Y);

        if (gridItem.Type == ItemType.Mud)
            RemoveGridItem(gridItem.Position);
        else if (gridItem.Type == ItemType.Rock)
        {
            if (CanRockfordPushRock(gridItem, position, direction))
                (gridItem.NodeObject as Rock).UpdateCurrentState(direction == Rockford.MoveDirection.left ? Rock.State.PushedLeft : Rock.State.PushedRight);
        }
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
                nextGridItem.Dead();
            levelGrid[prevIndex] = new(ItemType.None, prevGridItem.Position, null);
            levelGrid[nextIndex] = new(prevGridItem.Type, nextGridItem.Position, prevGridItem.NodeObject);
        }
        else
        {
            levelGrid[prevIndex] = new(nextGridItem.Type, prevGridItem.Position, nextGridItem.NodeObject);
            levelGrid[nextIndex] = new(prevGridItem.Type, nextGridItem.Position, prevGridItem.NodeObject);
        }
    }

    private void SpawnRockfordDead(Vector2I position)
    {
        player.Dead();

        for (int x = position.X - 1; x < position.X + 2; x++)
        {
            for (int y = position.Y - 1; y < position.Y + 2; y++)
            {
                GridItem gridItem = GetGridItem(x, y);
                if (gridItem.Type != ItemType.MetalWall)
                {
                    Explosion explosion = AddObject<Explosion>(explosionScene, x * SPRITE_WIDTH, y * SPRITE_HEIGHT);
                    explosion.Initilize(this, new(x, y));

                    RemoveGridItem(new(x, y));
                    AddGridItem(ItemType.Explosion, x, y, explosion);
                }
            }
        }
    }

    private void GameLoopManagement(double delta)
    {
        switch (gameState)
        {
            case GameState.gsInitialize:
                {
                    gameState = GameState.gsPlay;
                    InitTestLevel();
                    break;
                }

            case GameState.gsPlay:
                {
                    ProcessGameObjects(delta);
                    break;
                }

            case GameState.gsRockfordDead:
                {
                    gameState = GameState.gsGameOver;

                    GD.Print("Spawn Rockford dead animation " + player.GridPosition);
                    SpawnRockfordDead(player.GridPosition);

                    break;
                }

            case GameState.gsGameOver:
                {
                    UserEvent userEvent = GetInputEvent(delta);
                    if (userEvent == UserEvent.ueFire)
                        gameState = GameState.gsInitialize;

                    ProcessGameObjects(delta);

                    break;
                }
        }

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

    private void HandleMultipleInput(double delta)
    {
        if (!inputEnabled)
        {
            disableInputDelayTime += delta;
            if (disableInputDelayTime < disableInputDelay)
                return;

            disableInputDelayTime = 0;
            inputEnabled = true;
        }

        UserEvent userEvents = UserEvent.ueNone;

        if (Input.IsActionPressed("ui_accept"))
            userEvents |= UserEvent.ueFire;

        if (Input.IsActionPressed("ui_left"))
        {
            userEvents |= UserEvent.ueLeft;
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            userEvents |= UserEvent.ueRight;
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_up"))
        {
            userEvents |= UserEvent.ueUp;
            inputEnabled = false;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            userEvents |= UserEvent.ueDown;
            inputEnabled = false;
        }

        eventQueue.Enqueue(userEvents);
    }

    private void ProcessGameObjects(double delta)
    {
        levelRocks.ForEach(rock => (rock.NodeObject as Rock).Process(delta));
        levelDiamonds.ForEach(diamond => (diamond.NodeObject as Diamond).Process(delta));
        player.Process(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMultipleInput(delta);
        GameLoopManagement(delta);
    }
}
