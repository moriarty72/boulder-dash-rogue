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

    private Timer gameOverTimer;

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

    private void UpdateHUD()
    {
        Hud hud = GetNode<CanvasLayer>("HUD") as Hud;
        hud.Update();
    }

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
        Camera2D camera2D = GetNode<Camera2D>("Game/Camera2D");

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
        GetNode<AudioStreamPlayer2D>("Game/" + audioName)?.Play();
    }

    public void StopAudio(string audioName)
    {
        GetNode<AudioStreamPlayer2D>("Game/" + audioName)?.Stop();
    }

    public void SpawnExplosion(Vector2I position, bool isRockfordDead)
    {
        dungeonRoom.SpawnExplosion(position);

        if (isRockfordDead)
            gameState = GameState.gsRockfordDead;

        GetNode<AudioStreamPlayer2D>("Game/ExplosionAudio").Play();
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
                    UpdateHUD();
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
                    if (gameOverTimer == null)
                        gameOverTimer = new Timer(0);

                    if (gameOverTimer.IsElapsed(delta))
                    {
                        UserEvent userEvent = GetInputEvent(delta);
                        if (userEvent == UserEvent.ueFire)
                        {
                            dungeonRoom.Shutdown();
                            dungeonLevel = null;

                            gameState = GameState.gsInitialize;
                        }
                    }

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
