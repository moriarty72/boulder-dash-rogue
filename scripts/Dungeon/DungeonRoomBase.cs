using System;
using System.Collections.Generic;
using Godot;

public class DungeonRoomBase
{
    private int index;
    private int widthSize;
    private int heightSize;

    private Main mainController = null;
    private List<BaseGridObjectController> levelGrid = [];
    private BaseGridObjectController player;

    public int Index { get { return index; } set { index = value; } }
    public int WidthSize { get { return widthSize; } }
    public int HeightSize { get { return heightSize; } }


    public DungeonRoomBase(int index, int widthSize, int heightSize)
    {
        this.index = index;
        this.widthSize = widthSize;
        this.heightSize = heightSize;
    }

    public void Activate(Main mainController)
    {
        this.mainController = mainController;

        SpawnTestLevel(new(1, 1));
        SpawnRockford(new(1, 1));
    }

    private void InitilizeLevelGrid()
    {
        for (int x = 0; x < WidthSize; x++)
        {
            for (int y = 0; y < HeightSize; y++)
            {
                levelGrid.Add(null);
            }
        }
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
        levelGrid[index] = bgo;

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
        levelGrid[index]?.Dead();
    }

    public BaseGridObjectController GetGridItem(int x, int y)
    {
        int index = GetGridIndex(x, y);
        return levelGrid[index];
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

    public void RemoveGridObject(BaseGridObjectController baseGridObject)
    {
        int index = levelGrid.IndexOf(baseGridObject);
        levelGrid[index]?.Dead();
    }

    public void RemoveAllGridObjects()
    {
        for (int index = 0; index < levelGrid.Count; index++)
        {
            if (levelGrid[index] != null)
                levelGrid[index].Dead();
        }
        levelGrid = [];
    }

    public Vector2I GetRockfordPosition()
    {
        return player.GridPosition;
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
        player = AddGridItem<Rockford, RockfordController>(PackedSceneManager.RockfordScene, ItemType.Rockford, new(position.X * Global.SPRITE_WIDTH, position.Y * Global.SPRITE_HEIGHT), position);

        return player;
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
            RemoveGridItem(new(x, y));
            AddGridItem<Door, DoorController>(PackedSceneManager.DoorScene, ItemType.Door, new(x * Global.SPRITE_WIDTH, y * Global.SPRITE_HEIGHT), new(x, y));
        }

        InitilizeLevelGrid();

        spawnEmptyLevel();
        spawnEnemies();
        spawnRocks();
        spawnDiamonds();
        spawnDoor(10, 1);
        // spawnAmoeba(1, 2);
    }

    public void ProcessGameObjects(double delta)
    {
        for (int i = 0; i < levelGrid.Count; i++)
        {
            levelGrid[i]?.ProcessAndUpdate(delta);
        }
    }

}