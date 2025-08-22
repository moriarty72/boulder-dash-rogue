using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using ProceduralDungeon.Level;

public class DungeonRoom(int index, int widthSize, int heightSize, bool spawnTestLevel = false)
{
    public enum Connection : int
    {
        Up,
        Left,
        Right,
        Down,
        Max
    }

    private int index = index;
    private readonly int widthSize = widthSize;
    private readonly int heightSize = heightSize;

    private readonly bool spawnTestLevel = spawnTestLevel;

    private Main mainController = null;
    private List<BaseGridObjectController> roomMapObjects = null;
    private BaseGridObjectController rockfordObject;

    private int weight = 0;
    private Coordinate position;
    private DungeonRoomConnection[] roomConnections = new DungeonRoomConnection[4];
    private int[] roomConnectionPositions = new int[4];
    private List<DungeonItem> items = new List<DungeonItem>();

    public int Weight { get { return weight; } set { weight = value; } }
    public Coordinate Position { get { return position; } set { position = value; } }
    public DungeonRoomConnection[] RoomConnections { get { return roomConnections; } }
    public int[] RoomConnectionPositions { get { return roomConnectionPositions; } }
    public List<DungeonItem> Items { get { return items; } }

    public int Index { get { return index; } set { index = value; } }
    public int WidthSize { get { return widthSize; } }
    public int HeightSize { get { return heightSize; } }

    public void Activate(Main mainController, Connection connectionSide)
    {
        this.mainController = mainController;

        if (spawnTestLevel)
            SpawnTestLevel(new(1, 1));
        else
        {
            SpawnLevelObjectsAndTiles(new(1, 1));
            if (rockfordObject != null)
                SpawnRockford(rockfordObject.GridPosition);
            else
            {
                Vector2I rockfordSpawPosition = new(1, 1);
                if (connectionSide == Connection.Up)
                {
                    rockfordSpawPosition = new(RoomConnectionPositions[(int)Connection.Down], HeightSize - 2);
                }
                else if (connectionSide == Connection.Down)
                {
                    rockfordSpawPosition = new(RoomConnectionPositions[(int)Connection.Up], 1);
                }
                else if (connectionSide == Connection.Left)
                {
                    rockfordSpawPosition = new(WidthSize - 2, RoomConnectionPositions[(int)Connection.Right]);
                }
                else if (connectionSide == Connection.Right)
                {
                    rockfordSpawPosition = new(1, RoomConnectionPositions[(int)Connection.Left]);
                }

                SpawnRockford(rockfordSpawPosition);
            }
        }
    }

    public void Deactivate()
    {
        roomMapObjects.ForEach(obj => obj?.RemoveNodeFromScene());
    }

    public bool HasAvailableConnections()
    {
        return this.roomConnections.Any(c => c == null);
    }

    public bool IsLeaf()
    {
        return (this.roomConnections.Count(c => c != null) == 1);
    }

    private void InitilizeLevelGrid()
    {
        roomMapObjects = new List<BaseGridObjectController>(WidthSize * HeightSize);
        for (int i = 0; i < WidthSize * HeightSize; i++)
            roomMapObjects.Add(null);
    }

    private int GetGridIndex(int x, int y)
    {
        int index = x + y * WidthSize;
        return index;
    }

    #region Grid Item management
    public BaseGridObjectController AddGridItem<T, K>(PackedScene packedScene, ItemType itemType, Vector2 worldPosition, Vector2I gridPosition) where T : Node2D where K : BaseGridObjectController, new()
    {
        K bgo = new();
        bgo.Initialize<T>(mainController, packedScene, itemType, worldPosition, gridPosition);

        int index = GetGridIndex(gridPosition.X, gridPosition.Y);
        roomMapObjects[index] = bgo;

        return bgo;
    }

    public void ReplaceGridItem(int x, int y, ItemType newItemType)
    {
        if (newItemType == ItemType.Amoeba)
        {
            RemoveGridItem(new(x, y));
            AddGridItem<Amoeba, AmoebaController>(PackedSceneManager.AmoebaScene, ItemType.Amoeba, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
        }
        else if (newItemType == ItemType.Rock)
        {
            RemoveGridItem(new(x, y));
            AddGridItem<Rock, FallingObjectController>(PackedSceneManager.RockScene, ItemType.Rock, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
        }
        else if (newItemType == ItemType.Diamond)
        {
            RemoveGridItem(new(x, y));
            AddGridItem<Diamond, FallingObjectController>(PackedSceneManager.DiamondScene, ItemType.Diamond, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
        }
    }

    public void RemoveGridItem(Vector2I position)
    {
        int index = GetGridIndex(position.X, position.Y);
        roomMapObjects[index]?.Dead();
    }

    public BaseGridObjectController GetGridItem(int x, int y)
    {
        int index = GetGridIndex(x, y);
        return roomMapObjects[index];
    }

    public void SwapGridItems(Vector2I prevPosition, Vector2I newPosition, bool replaceNext)
    {
        int prevIndex = GetGridIndex(prevPosition.X, prevPosition.Y);
        int nextIndex = GetGridIndex(newPosition.X, newPosition.Y);

        BaseGridObjectController prevGridItem = roomMapObjects[prevIndex];
        BaseGridObjectController nextGridItem = roomMapObjects[nextIndex];

        if (replaceNext)
        {
            nextGridItem.Dead();
            roomMapObjects[prevIndex] = nextGridItem;
            roomMapObjects[nextIndex] = prevGridItem;
        }
        else
        {
            roomMapObjects[prevIndex] = nextGridItem;
            roomMapObjects[nextIndex] = prevGridItem;
        }
    }

    public void RemoveGridObject(BaseGridObjectController baseGridObject)
    {
        int index = roomMapObjects.IndexOf(baseGridObject);
        roomMapObjects[index]?.Dead();
    }

    public void RemoveAllGridObjects()
    {
        for (int index = 0; index < roomMapObjects.Count; index++)
        {
            if (roomMapObjects[index] != null)
                roomMapObjects[index].Dead();
        }
        roomMapObjects = [];
    }

    public Vector2I GetRockfordPosition()
    {
        return rockfordObject.GridPosition;
    }

    public void SpawnExplosion(Vector2I position)
    {
        for (int x = position.X - 1; x < position.X + 2; x++)
        {
            for (int y = position.Y - 1; y < position.Y + 2; y++)
            {
                BaseGridObjectController gridItem = GetGridItem(x, y);
                if (gridItem.Type != ItemType.MetalWall)
                {
                    RemoveGridItem(new(x, y));
                    BaseGridObjectController bgo = AddGridItem<Explosion, BaseGridObjectController>(PackedSceneManager.ExplosionScene, ItemType.Explosion, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
                    (bgo.NodeObject as Explosion).AnimationEnd += () =>
                    {
                        RemoveGridObject(bgo);
                    };
                }
            }
        }
    }

    public BaseGridObjectController SpawnRockford(Vector2I position)
    {
        RemoveGridItem(position);
        rockfordObject = AddGridItem<Rockford, RockfordController>(PackedSceneManager.RockfordScene, ItemType.Rockford, new(position.X * Global.SPRITE_WIDTH, position.Y * Global.SPRITE_HEIGHT), position);

        mainController.AttachCameraToEntity(rockfordObject.NodeObject);

        return rockfordObject;
    }
    #endregion

    private void SpawnTestLevel(Vector2I rockfordPosition)
    {
        Random rnd = new(System.Environment.TickCount);

        void spawnEmptyLevel()
        {
            for (int x = 0; x < widthSize; x++)
            {
                for (int y = 0; y < heightSize; y++)
                {
                    if ((x == rockfordPosition.X) && (y == rockfordPosition.Y))
                        continue;

                    bool isCorner = ((x == 0) && (y == 0)) || ((x == 0) && (y == (heightSize - 1))) || ((x == (widthSize - 1)) && (y == 0)) || ((x == (widthSize - 1)) && (y == (heightSize - 1)));
                    bool isSide = ((x > 0) && (x < widthSize) && ((y == 0) || (y == (heightSize - 1)))) || ((y > 0) && (y < heightSize) && ((x == 0) || (x == (widthSize - 1))));

                    if (isCorner || isSide)
                    {
                        AddGridItem<MetalWall, BaseGridObjectController>(PackedSceneManager.MetalWallScene, ItemType.MetalWall, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
                    }
                    else
                    {
                        AddGridItem<Mud1, BaseGridObjectController>(PackedSceneManager.MudScene, ItemType.Mud, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
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

            // AddGridItem<EnemySquare, EnemySquareController>(enemySquareScene, ItemType.EnemySquare, new(enemyBoxPosition.X * SPRITE_WIDTH, enemyBoxPosition.Y * SPRITE_HEIGHT), new(enemyBoxPosition.X, enemyBoxPosition.Y));
            AddGridItem<EnemyButterfly, EnemyButterflyController>(PackedSceneManager.EnemyButterflyScene, ItemType.EnemyButterfly, new(enemyBoxPosition.X * Global.SPRITE_WIDTH, enemyBoxPosition.Y * Global.SPRITE_HEIGHT), new(enemyBoxPosition.X, enemyBoxPosition.Y));
        }

        void spawnRocks()
        {
            Vector2I[] rockPositions = [new(3, 3), new(3, 4)];

            for (int i = 0; i < rockPositions.Length; i++)
            {
                RemoveGridItem(rockPositions[i]);
                AddGridItem<Rock, FallingObjectController>(PackedSceneManager.RockScene, ItemType.Rock, new(rockPositions[i].X * Global.SPRITE_WIDTH, rockPositions[i].Y * Global.SPRITE_HEIGHT), rockPositions[i]);
            }
        }

        void spawnDiamonds()
        {
            int x = 4;
            int y = 3;

            RemoveGridItem(new(x, y));
            AddGridItem<Diamond, FallingObjectController>(PackedSceneManager.DiamondScene, ItemType.Diamond, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
        }

        void spawnAmoeba(int x, int y)
        {
            RemoveGridItem(new(x, y));
            AddGridItem<Amoeba, AmoebaController>(PackedSceneManager.AmoebaScene, ItemType.Amoeba, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
        }

        void spawnDoor(int x, int y)
        {
            DoorController.Color[] doors = [DoorController.Color.Blue, DoorController.Color.Green, DoorController.Color.Red, DoorController.Color.Silver, DoorController.Color.Yellow];

            for (int i = 0; i < doors.Length; i++)
            {
                RemoveGridItem(new(x + i, y));

                var doorController = AddGridItem<Door, DoorController>(PackedSceneManager.DoorScene, ItemType.Door, new((x + i) * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x + i, y));
                ((DoorController)doorController).CurrentColor = doors[i];
            }
        }

        void spawnKeys(int x, int y)
        {
            DoorController.Color[] doors = [DoorController.Color.Blue, DoorController.Color.Green, DoorController.Color.Red, DoorController.Color.Silver, DoorController.Color.Yellow];

            for (int i = 0; i < doors.Length; i++)
            {
                RemoveGridItem(new(x + i, y));

                var keyController = AddGridItem<Key, KeyController>(PackedSceneManager.KeyScene, ItemType.Key, new((x + i) * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x + i, y));
                ((KeyController)keyController).SetKeyColor(doors[i]);
            }
        }

        InitilizeLevelGrid();

        spawnEmptyLevel();
        spawnEnemies();
        spawnRocks();
        spawnDiamonds();
        spawnDoor(10, 1);
        spawnKeys(10, 2);
        // spawnAmoeba(1, 2);

        SpawnRockford(rockfordPosition);
    }

    private void SpawnLevelObjectsAndTiles(Vector2I rockfordPosition)
    {
        if (roomMapObjects == null)
        {
            InitilizeLevelGrid();
            // instantiate doors...
            DungeonRoomConnection roomUpConnection = RoomConnections[(int)DungeonRoom.Connection.Up];
            DungeonRoomConnection roomRightConnection = RoomConnections[(int)DungeonRoom.Connection.Right];
            DungeonRoomConnection roomBottomConnection = RoomConnections[(int)DungeonRoom.Connection.Down];
            DungeonRoomConnection roomLeftConnection = RoomConnections[(int)DungeonRoom.Connection.Left];

            for (int y = 0; y < HeightSize; y++)
            {
                for (int x = 0; x < WidthSize; x++)
                {
                    Vector2I tilePosition = new(x, y);

                    if ((roomUpConnection != null) && (y == 0) && (x == RoomConnectionPositions[(int)DungeonRoom.Connection.Up]))
                    {
                        SpawnDoor(roomUpConnection, DungeonRoom.Connection.Up, tilePosition);
                        continue;
                    }

                    if ((roomRightConnection != null) && (x == WidthSize - 1) && (y == RoomConnectionPositions[(int)DungeonRoom.Connection.Right]))
                    {
                        SpawnDoor(roomRightConnection, DungeonRoom.Connection.Right, tilePosition);
                        continue;
                    }

                    if ((roomBottomConnection != null) && (y == HeightSize - 1) && (x == RoomConnectionPositions[(int)DungeonRoom.Connection.Down]))
                    {
                        SpawnDoor(roomBottomConnection, DungeonRoom.Connection.Down, tilePosition);
                        continue;
                    }

                    if ((roomLeftConnection != null) && (x == 0) && (y == RoomConnectionPositions[(int)DungeonRoom.Connection.Left]))
                    {
                        SpawnDoor(roomLeftConnection, DungeonRoom.Connection.Left, tilePosition);
                        continue;
                    }

                    bool isCorner = ((x == 0) && (y == 0)) || ((x == 0) && (y == (HeightSize - 1))) || ((x == (WidthSize - 1)) && (y == 0)) || ((x == (WidthSize - 1)) && (y == (HeightSize - 1)));
                    bool isSide = ((x > 0) && (x < WidthSize) && ((y == 0) || (y == (HeightSize - 1)))) || ((y > 0) && (y < HeightSize) && ((x == 0) || (x == (WidthSize - 1))));

                    if (isCorner || isSide)
                    {
                        AddGridItem<MetalWall, BaseGridObjectController>(PackedSceneManager.MetalWallScene, ItemType.MetalWall, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
                    }
                    else
                    {
                        AddGridItem<Mud1, BaseGridObjectController>(PackedSceneManager.MudScene, ItemType.Mud, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
                    }
                }
            }

            // spanw keys
            Items.ForEach(item =>
            {
                try
                {
                    if (item.GetType() == typeof(DungeonItemKey))
                    {
                        DungeonItemKey dungeonItemKey = (DungeonItemKey)item;

                        RemoveGridItem(dungeonItemKey.Position.ToVector2I());
                        KeyController keyObject = (KeyController)AddGridItem<Key, KeyController>(PackedSceneManager.KeyScene, ItemType.Key, new(dungeonItemKey.Position.X * Global.SPRITE_WIDTH, dungeonItemKey.Position.Y * Global.SPRITE_HEIGHT), dungeonItemKey.Position.ToVector2I());

                        keyObject.SetKeyColor((DoorController.Color)dungeonItemKey.KeyLevel);
                    }
                }
                catch (Exception e)
                {
                    GD.Print("DungeonMaster.RenderRoomItems: exception " + e.Message);
                }
            });
        }
        else
        {
            roomMapObjects.ForEach(obj => obj?.Respawn());
        }
    }

    private void SpawnDoor(DungeonRoomConnection roomConnection, DungeonRoom.Connection connection, Vector2I position)
    {
        Vector2I doorPosition = position;
        DoorController doorObject = (DoorController)AddGridItem<Door, DoorController>(PackedSceneManager.DoorScene, ItemType.Door, new(doorPosition.X * Global.SPRITE_WIDTH, doorPosition.Y * Global.SPRITE_HEIGHT), new(doorPosition.X, doorPosition.Y));

        doorObject.RoomConnection = roomConnection;
        doorObject.ConnectionSide = connection;

        if (roomConnection.IsLocked)
            doorObject.CurrentColor = (DoorController.Color)roomConnection.UnlockLevelNeeded;
        else
        {
            doorObject.CurrentColor = DoorController.Color.Brown;
            doorObject.CurrentState = DoorController.State.Opened;
        }
    }


    public void ProcessGameObjects(double delta)
    {
        for (int i = 0; i < WidthSize * HeightSize; i++)
            roomMapObjects[i]?.ProcessAndUpdate(delta);
    }
}