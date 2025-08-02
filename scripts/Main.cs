using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static BaseGridObjectController;
using static FallingObjectController;
using static RockfordController;

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
    public int rockCount = 10;

    [Export]
    public int diamondPerColumn = 10;

    [Export]
    public int enemyCount = 4;

    private BaseGridObjectController player;

    private PackedScene playerScene = GD.Load<PackedScene>("res://scenes//rockford.tscn");
    private PackedScene mudScene = GD.Load<PackedScene>("res://scenes//mud-1.tscn");
    private PackedScene rockScene = GD.Load<PackedScene>("res://scenes//rock.tscn");
    private PackedScene diamondScene = GD.Load<PackedScene>("res://scenes//diamond.tscn");
    private PackedScene metalWallScene = GD.Load<PackedScene>("res://scenes//metal-wall.tscn");
    private PackedScene explosionScene = GD.Load<PackedScene>("res://scenes//explosion.tscn");
    private PackedScene enemySquareScene = GD.Load<PackedScene>("res://scenes//enemy-square.tscn");

    private List<BaseGridObjectController> levelGrid = [];

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

    private UserEvent currentUserEvent = UserEvent.ueNone;

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

    private BaseGridObjectController AddGridItem<T, K>(PackedScene packedScene, ItemType itemType, Vector2 worldPosition, Vector2I gridPosition) where T : Node2D where K : BaseGridObjectController, new()
    {
        K bgo = new();
        bgo.Initialize<T>(this, packedScene, itemType, worldPosition, gridPosition);

        int index = GetGridIndex(gridPosition.X, gridPosition.Y);
        levelGrid[index] = bgo;

        return bgo;
    }

    public BaseGridObjectController GetGridItem(int x, int y)
    {
        int index = GetGridIndex(x, y);
        return levelGrid[index];
    }

    public void RemoveGridItem(Vector2I position)
    {
        int index = GetGridIndex(position.X, position.Y);
        levelGrid[index].Dead();
    }

    public void RemoveGridObject(BaseGridObjectController baseGridObject)
    {
        int index = levelGrid.IndexOf(baseGridObject);
        levelGrid[index].Dead();
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
                item.Dead();
        });
        levelGrid = [];
    }

    private void SpawnTestLevel()
    {
        Random rnd = new(System.Environment.TickCount);

        void spawnEmptyLevel()
        {
            for (int x = 0; x < testLevelGridSize.X; x++)
            {
                for (int y = 0; y < testLevelGridSize.Y; y++)
                {
                    if ((x == testRockfordPosition.X) && (y == testRockfordPosition.Y))
                        continue;

                    bool isCorner = ((x == 0) && (y == 0)) || ((x == 0) && (y == (testLevelGridSize.Y - 1))) || ((x == (testLevelGridSize.X - 1)) && (y == 0)) || ((x == (testLevelGridSize.X - 1)) && (y == (testLevelGridSize.Y - 1)));
                    bool isSide = ((x > 0) && (x < testLevelGridSize.X) && ((y == 0) || (y == (testLevelGridSize.Y - 1)))) || ((y > 0) && (y < testLevelGridSize.Y) && ((x == 0) || (x == (testLevelGridSize.X - 1))));

                    if (isCorner || isSide)
                    {
                        AddGridItem<MetalWall, BaseGridObjectController>(metalWallScene, ItemType.MetalWall, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                    }
                    else
                    {
                        AddGridItem<Mud1, BaseGridObjectController>(mudScene, ItemType.Mud, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                    }
                }
            }
        }

        void spawnEnemies()
        {
            Vector2I enemyBoxSize = new(5, 5); //rnd.Next(2, 6), rnd.Next(2, 6));
            Vector2I enemyBoxPosition = new(7, 7); // rnd.Next(6, testLevelGridSize.X - 6), rnd.Next(6, testLevelGridSize.Y - 6));

            for (int x = enemyBoxPosition.X; x < enemyBoxPosition.X + enemyBoxSize.X; x++)
            {
                for (int y = enemyBoxPosition.Y; y < enemyBoxPosition.Y + enemyBoxSize.Y; y++)
                {
                    RemoveGridItem(new(x, y));
                }
            }

            AddGridItem<EnemySquare, EnemySquareController>(enemySquareScene, ItemType.EnemySquare, new(enemyBoxPosition.X * SPRITE_WIDTH, enemyBoxPosition.Y * SPRITE_HEIGHT), new(enemyBoxPosition.X, enemyBoxPosition.Y));
        }

        void spawnRocks()
        {
            Vector2I[] rockPositions = [new(3, 3), new(3, 4)];

            for (int i = 0; i < rockPositions.Length; i++)
            {
                RemoveGridItem(rockPositions[i]);
                AddGridItem<Rock, FallingObjectController>(rockScene, ItemType.Rock, new(rockPositions[i].X * SPRITE_WIDTH, rockPositions[i].Y * SPRITE_HEIGHT), rockPositions[i]);
            }
        }

        void spawnDiamonds()
        {
            int x = 4;
            int y = 3;

            RemoveGridItem(new(x, y));
            AddGridItem<Diamond, FallingObjectController>(diamondScene, ItemType.Diamond, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
        }

        spawnEmptyLevel();
        spawnEnemies();
        spawnRocks();
        spawnDiamonds();

        /*
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
                    AddGridItem<MetalWall, BaseGridObject>(metalWallScene, ItemType.MetalWall, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                }
                else
                {
                    // if ((x == 4) && (y == 4))
                    if ((x > 2) && (y > 2) && ((rnd.Next(1, 100) % 5) == 0) && (rockToSpawn > 0))
                    {
                        AddGridItem<Rock, FallingObject>(rockScene, ItemType.Rock, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                        rockToSpawn--;
                    }
                    else if ((x > 2) && (y > 2) && ((rnd.Next(1, 100) % 5) == 0) && (diamondToSpawn > 0))
                    {
                        AddGridItem<Diamond, FallingObject>(diamondScene, ItemType.Diamond, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                        diamondToSpawn--;
                    }
                    else
                    {
                        AddGridItem<Mud1, BaseGridObject>(mudScene, ItemType.Mud, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                    }
                }
            }
        }
        */
    }

    public UserEvent GetInputEvent(double delta)
    {
        return currentUserEvent;

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
        player = AddGridItem<Rockford, RockfordController>(playerScene, ItemType.Rockford, new(testRockfordPosition.X * SPRITE_WIDTH, testRockfordPosition.Y * SPRITE_HEIGHT), testRockfordPosition);

        Camera2D camera2D = GetNode<Camera2D>("Camera2D");
        RemoteTransform2D remoteTransform2D = new()
        {
            RemotePath = camera2D.GetPath()
        };
        player.NodeObject.AddChild(remoteTransform2D);
    }

    public void PlayAudio(string audioName)
    {
        GetNode<AudioStreamPlayer2D>(audioName)?.Play();
    }

    public bool CheckObjectCollision(Vector2I objectPosition, int offsetX, int offsetY, BaseGridObjectController gridObject)
    {
        BaseGridObjectController gridItem = GetGridItem(objectPosition.X + offsetX, objectPosition.Y + offsetY);
        if (gridItem.Type == ItemType.Rockford)
        {
            RemoveGridItem((player as BaseGridObjectController).GridPosition);
            RemoveGridObject(gridObject);

            SpawnExplosion(player.GridPosition);

            gameState = GameState.gsRockfordDead;
            return true;
        }
        else if (gridItem.Type == ItemType.EnemySquare)
        {
            RemoveGridItem(gridItem.GridPosition);
            RemoveGridObject(gridObject);

            SpawnExplosion(gridItem.GridPosition);
            return true;
        }

        return false;
    }

    public void SwapGridItems(Vector2I prevPosition, Vector2I newPosition, bool replaceNext)
    {
        int prevIndex = GetGridIndex(prevPosition.X, prevPosition.Y);
        int nextIndex = GetGridIndex(newPosition.X, newPosition.Y);

        BaseGridObjectController prevGridItem = levelGrid[prevIndex];
        BaseGridObjectController nextGridItem = levelGrid[nextIndex];

        if (replaceNext)
        {
            nextGridItem.Dead();
            levelGrid[prevIndex] = nextGridItem;
            levelGrid[nextIndex] = prevGridItem;
        }
        else
        {
            levelGrid[prevIndex] = nextGridItem;
            levelGrid[nextIndex] = prevGridItem;
        }
    }

    private void SpawnExplosion(Vector2I position)
    {
        for (int x = position.X - 1; x < position.X + 2; x++)
        {
            for (int y = position.Y - 1; y < position.Y + 2; y++)
            {
                BaseGridObjectController gridItem = GetGridItem(x, y);
                if (gridItem.Type != ItemType.MetalWall)
                {
                    RemoveGridItem(new(x, y));
                    BaseGridObjectController bgo = AddGridItem<Explosion, BaseGridObjectController>(explosionScene, ItemType.Explosion, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                    (bgo.NodeObject as Explosion).AnimationEnd += () =>
                    {
                        RemoveGridObject(bgo);
                    };
                }
            }
        }

        GetNode<AudioStreamPlayer2D>("ExplosionAudio").Play();
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
        currentUserEvent = UserEvent.ueNone;

        if (Input.IsActionPressed("ui_accept"))
            currentUserEvent |= UserEvent.ueFire;

        if (Input.IsActionPressed("ui_left"))
        {
            currentUserEvent |= UserEvent.ueLeft;
        }
        else if (Input.IsActionPressed("ui_right"))
        {
            currentUserEvent |= UserEvent.ueRight;
        }
        else if (Input.IsActionPressed("ui_up"))
        {
            currentUserEvent |= UserEvent.ueUp;
        }
        else if (Input.IsActionPressed("ui_down"))
        {
            currentUserEvent |= UserEvent.ueDown;
        }
    }
    private bool CheckCollisions(BaseGridObjectController bgo)
    {
        switch (bgo.Type)
        {
            // check collision with falling objects (rocks, diamonds) and rockford
            case ItemType.Rock:
            case ItemType.Diamond:
                {
                    FallingObjectController fallingObject = bgo as FallingObjectController;
                    if ((fallingObject != null) && (fallingObject.CurrentState == FallingObjectController.State.Fall))
                    {
                        BaseGridObjectController gridItem = GetGridItem(bgo.GridPosition.X, bgo.GridPosition.Y + 1);
                        if (gridItem.Type == ItemType.Rockford)
                        {
                            GD.Print("Rockford DEAD!!!");

                            RemoveGridObject(player);
                            RemoveGridObject(bgo);

                            SpawnExplosion(player.GridPosition);

                            gameState = GameState.gsRockfordDead;

                            return false;   // do not update on Rockford dead
                        }
                        else if (gridItem.Type == ItemType.EnemySquare)
                        {
                            RemoveGridObject(gridItem);
                            RemoveGridObject(bgo);

                            SpawnExplosion(gridItem.GridPosition);
                        }
                    }
                    break;
                }

            case ItemType.EnemySquare:
                {
                    // check collision rockford & enemy
                    break;
                }
        }

        // let update object on return
        return true;
    }

    private void ProcessGameObjects(double delta)
    {
        for (int i = 0; i < levelGrid.Count; i++)
        {
            levelGrid[i].ProcessAndUpdate(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMultipleInput(delta);
        GameLoopManagement(delta);
    }
}
