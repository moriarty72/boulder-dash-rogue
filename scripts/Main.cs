using Godot;
using ProceduralDungeon.Level;
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

    [Export]
    public bool UseTestLevel = false;

    [Export]
    public double disableInputDelay = 1.0;

    [Export]
    public Vector2I testLevelGridSize = new(20, 12);

    [Export]
    public Vector2I rockfordSpawnPosition = new(1, 1);

    [Export]
    public int rockCount = 10;

    [Export]
    public int diamondPerColumn = 10;

    [Export]
    public int enemyCount = 4;

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
        gsChangeRoom,
        gsRockfordDead,
        gsGameOver
    }

    private UserEvent currentUserEvent = UserEvent.ueNone;

    private Queue<UserEvent> eventQueue = new Queue<UserEvent>();
    private double disableInputDelayTime = 0;
    private double idleInputDelayTime = 0;

    private bool inputEnabled = true;
    private GameState gameState = GameState.gsInitialize;

    // dungeon management
    [Export]
    public int roomGridWidth;
    [Export]
    public int roomGridHeight;
    [Export]
    public int roomCount;
    [Export]
    public int seed;
    [Export]
    public int deepLevel;
    [Export]
    public int minRoomWidthSize;
    [Export]
    public int minRoomHeightSize;
    [Export]
    public int maxRoomWidthSize;
    [Export]
    public int maxRoomHeightSize;

    private DungeonLevel dungeonLevel;
    private DungeonRoom dungeonRoom;
    private DungeonRoomConnection roomConnection;
    private DungeonRoom.Connection rommConnectionSide;
    private int playerKeyLevel = 0;

    // public DungeonRoom CurrentRoom { get { return dungeonRoom; } }

    public override void _Ready()
    {
    }

    private void InitTestLevel()
    {
        dungeonRoom = new DungeonRoom(0, testLevelGridSize.X, testLevelGridSize.Y, true);
        dungeonRoom.Activate(this, DungeonRoom.Connection.Max);
    }

    private void InitializeDungeon()
    {
        dungeonLevel = new DungeonLevel(roomGridWidth, roomGridHeight, roomCount, seed, deepLevel, minRoomWidthSize, minRoomHeightSize, maxRoomWidthSize, maxRoomHeightSize);
        dungeonLevel.Build();

        dungeonRoom = dungeonLevel.StartingRoom;
        dungeonRoom.Activate(this, DungeonRoom.Connection.Max);
    }

    private void InitializeCamera()
    {
        Camera2D camera2D = GetNode<Camera2D>("Camera2D");

        camera2D.LimitTop = camera2D.LimitLeft = 0;
        camera2D.LimitRight = dungeonRoom.WidthSize * Global.SPRITE_WIDTH;
        camera2D.LimitBottom = dungeonRoom.HeightSize * Global.SPRITE_HEIGHT;
    }

    #region Dungeon Management

    /*
        private void SpawnRoomTiles()
        {
            if (dungeonRoom.RoomState.HasState())
            {
                List<BaseGridObjectController> roomGridObjects = dungeonRoom.RoomState.RetrieveState();
                roomGridObjects.ForEach(bgo =>
                {
                    if (bgo.Type != ItemType.Rockford)
                        RespawnGridItem(bgo);
                    else
                        AddEmptyGridItem(bgo.WorldPosition, bgo.GridPosition);
                });
            }
            else
            {
                // instantiate doors...
                DungeonRoomConnection roomUpConnection = dungeonRoom.RoomConnections[(int)DungeonRoom.Connection.Up];
                DungeonRoomConnection roomRightConnection = dungeonRoom.RoomConnections[(int)DungeonRoom.Connection.Right];
                DungeonRoomConnection roomBottomConnection = dungeonRoom.RoomConnections[(int)DungeonRoom.Connection.Down];
                DungeonRoomConnection roomLeftConnection = dungeonRoom.RoomConnections[(int)DungeonRoom.Connection.Left];

                for (int y = 0; y < dungeonRoom.HeightSize; y++)
                {
                    for (int x = 0; x < dungeonRoom.WidthSize; x++)
                    {
                        Vector2I tilePosition = new(x, y);

                        if ((roomUpConnection != null) && (y == 0) && (x == dungeonRoom.RoomConnectionPositions[(int)DungeonRoom.Connection.Up]))
                        {
                            SpawnDoor(roomUpConnection, DungeonRoom.Connection.Up, tilePosition);
                            continue;
                        }

                        if ((roomRightConnection != null) && (x == dungeonRoom.WidthSize - 1) && (y == dungeonRoom.RoomConnectionPositions[(int)DungeonRoom.Connection.Right]))
                        {
                            SpawnDoor(roomRightConnection, DungeonRoom.Connection.Right, tilePosition);
                            continue;
                        }

                        if ((roomBottomConnection != null) && (y == dungeonRoom.HeightSize - 1) && (x == dungeonRoom.RoomConnectionPositions[(int)DungeonRoom.Connection.Down]))
                        {
                            SpawnDoor(roomBottomConnection, DungeonRoom.Connection.Down, tilePosition);
                            continue;
                        }

                        if ((roomLeftConnection != null) && (x == 0) && (y == dungeonRoom.RoomConnectionPositions[(int)DungeonRoom.Connection.Left]))
                        {
                            SpawnDoor(roomLeftConnection, DungeonRoom.Connection.Left, tilePosition);
                            continue;
                        }

                        bool isCorner = ((x == 0) && (y == 0)) || ((x == 0) && (y == (dungeonRoom.HeightSize - 1))) || ((x == (dungeonRoom.WidthSize - 1)) && (y == 0)) || ((x == (dungeonRoom.WidthSize - 1)) && (y == (dungeonRoom.HeightSize - 1)));
                        bool isSide = ((x > 0) && (x < dungeonRoom.WidthSize) && ((y == 0) || (y == (dungeonRoom.HeightSize - 1)))) || ((y > 0) && (y < dungeonRoom.HeightSize) && ((x == 0) || (x == (dungeonRoom.WidthSize - 1))));

                        if (isCorner || isSide)
                        {
                            AddGridItem<MetalWall, BaseGridObjectController>(PackedSceneManager.MetalWallScene, ItemType.MetalWall, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                        }
                        else
                        {
                            AddGridItem<Mud1, BaseGridObjectController>(PackedSceneManager.MudScene, ItemType.Mud, new(x * SPRITE_WIDTH, y * SPRITE_HEIGHT), new(x, y));
                        }
                    }
                }

                // spanw keys
                dungeonRoom.Items.ForEach(item =>
                {
                    try
                    {
                        if (item.GetType() == typeof(DungeonItemKey))
                        {
                            DungeonItemKey dungeonItemKey = (DungeonItemKey)item;

                            RemoveGridItem(dungeonItemKey.Position.ToVector2I());
                            KeyController keyObject = (KeyController)AddGridItem<Key, KeyController>(PackedSceneManager.KeyScene, ItemType.Key, new(dungeonItemKey.Position.X * SPRITE_WIDTH, dungeonItemKey.Position.Y * SPRITE_HEIGHT), dungeonItemKey.Position.ToVector2I());

                            keyObject.SetKeyColor((DoorController.Color)dungeonItemKey.KeyLevel);
                        }
                    }
                    catch (Exception e)
                    {
                        GD.Print("DungeonMaster.RenderRoomItems: exception " + e.Message);
                    }
                });
                dungeonRoom.RoomState.SaveState(levelGrid);
            }
        }

    private void SpawnDoor(DungeonRoomConnection roomConnection, DungeonRoom.Connection connection, Vector2I position)
    {
        Vector2I doorPosition = position;
        DoorController doorObject = (DoorController)AddGridItem<Door, DoorController>(PackedSceneManager.DoorScene, ItemType.Door, new(doorPosition.X * SPRITE_WIDTH, doorPosition.Y * SPRITE_HEIGHT), new(doorPosition.X, doorPosition.Y));

        doorObject.RoomConnection = roomConnection;
        if (roomConnection.IsLocked)
            doorObject.CurrentColor = (DoorController.Color)roomConnection.UnlockLevelNeeded;
        else
        {
            doorObject.CurrentColor = DoorController.Color.Brown;
            doorObject.CurrentState = DoorController.State.Opened;
        }
    }

    public void ChangeRoom(DungeonRoomConnection dungeonRoomConnection)
    {
        // save current room state
        dungeonRoom.RoomState.SaveState(levelGrid);

        gameState = GameState.gsChangeRoom;
        dungeonRoom = dungeonRoomConnection.DestinationRoom;

        rockfordSpawnPosition = new(1, 1);
    }
    */

    #endregion

    public UserEvent GetInputEvent(double delta)
    {
        return currentUserEvent;
    }

    private void DisableInput(double delay)
    {
        inputEnabled = false;
        disableInputDelay = delay;
        eventQueue.Clear();
    }

    public void AttachCameraToEntity(Node2D entity)
    {
        Camera2D camera2D = GetNode<Camera2D>("Camera2D");

        camera2D.LimitTop = camera2D.LimitLeft = 0;
        camera2D.LimitRight = dungeonRoom.WidthSize * Global.SPRITE_WIDTH;
        camera2D.LimitBottom = dungeonRoom.HeightSize * Global.SPRITE_HEIGHT;

        RemoteTransform2D remoteTransform2D = new()
        {
            RemotePath = camera2D.GetPath()
        };
        entity.AddChild(remoteTransform2D);
    }

    public void PlayAudio(string audioName)
    {
        GetNode<AudioStreamPlayer2D>(audioName)?.Play();
    }

    public void StopAudio(string audioName)
    {
        GetNode<AudioStreamPlayer2D>(audioName)?.Stop();
    }

    public void SpawnExplosion(Vector2I position, bool isRockfordDead)
    {
        dungeonRoom.SpawnExplosion(position);

        if (isRockfordDead)
            gameState = GameState.gsRockfordDead;

        GetNode<AudioStreamPlayer2D>("ExplosionAudio").Play();
    }

    private void GameLoopManagement(double delta)
    {
        switch (gameState)
        {
            case GameState.gsInitialize:
                {
                    gameState = GameState.gsPlay;
                    if (UseTestLevel)
                        InitTestLevel();
                    else
                        InitializeDungeon();

                    break;
                }

            case GameState.gsPlay:
                {
                    dungeonRoom.ProcessGameObjects(delta);
                    break;
                }

            case GameState.gsChangeRoom:
                {
                    dungeonRoom.Activate(this, rommConnectionSide);

                    gameState = GameState.gsPlay;
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

                    dungeonRoom.ProcessGameObjects(delta);

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

    public override void _PhysicsProcess(double delta)
    {
        HandleMultipleInput(delta);
        GameLoopManagement(delta);
    }

    #region Room functions
    public BaseGridObjectController GetGridItem(int x, int y)
    {
        return dungeonRoom.GetGridItem(x, y);
    }

    public void ReplaceGridItem(int x, int y, ItemType newItemType)
    {
        dungeonRoom.ReplaceGridItem(x, y, newItemType);
    }
    public void SwapGridItems(Vector2I prevPosition, Vector2I newPosition, bool replaceNext)
    {
        dungeonRoom.SwapGridItems(prevPosition, newPosition, replaceNext);
    }

    public void RemoveGridItem(Vector2I position)
    {
        dungeonRoom.RemoveGridItem(position);
    }

    public Vector2I GetRockfordPosition()
    {
        return dungeonRoom.GetRockfordPosition();
    }

    public void ChangeRoom(DungeonRoomConnection dungeonRoomConnection, DungeonRoom.Connection connectionSide)
    {
        dungeonRoom.Deactivate();

        gameState = GameState.gsChangeRoom;

        roomConnection = dungeonRoomConnection;
        rommConnectionSide = connectionSide;

        dungeonRoom = dungeonRoomConnection.DestinationRoom;
    }
    #endregion
}
