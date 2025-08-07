using Godot;
using System;
using System.Collections.Generic;

public partial class AmoebaController : BaseGridObjectController
{
    private class AmoebaTreeNode
    {
        public bool isRoot = false;
        public Vector2I gridPosition;
        public List<AmoebaTreeNode> children = [];
    }

    private const double GROW_TIME = 60;

    private const double GLOBAL_SPAWN_LIMIT = 100;
    private const int MAX_CHILD_NODE = 2;

    private double timeTick = 0;
    private static double growTick = 0;
    private double currentGrowTick = 0;

    private static int globalAmoebaSpawnCount = 1;
    private int currentAmoebaNodeSpawnCount = 0;


    public override void Initialize<T>(Main mc, PackedScene packedScene, ItemType itemType, Vector2 worldPosition, Vector2I gridPosition)
    {
        base.Initialize<T>(mc, packedScene, itemType, worldPosition, gridPosition);

        if (growTick == 0)
            growTick = 2.0;

        // mainController.PlayAudio("AmoebaAudio");
    }

    private Vector2I GetRandomOffset()
    {
        // Vector2I[] spawnOffsets = [new(1, 0), new(1, -1), new(0, -1), new(-1, -1), new(-1, 0), new(-1, 1), new(0, 1), new(1, 1)];

        Vector2I[] spawnOffsets = [new(1, 0), new(0, -1), new(-1, 0), new(0, 1)];

        Random rnd = new(System.Environment.TickCount);
        int rndIndex = rnd.Next(100);
        return spawnOffsets[rndIndex % 4];
    }

    private bool Spawn()
    {
        // spawn children
        Vector2I offset = GetRandomOffset();

        var gridItem = mainController.GetGridItem(GridPosition.X + offset.X, GridPosition.Y + offset.Y);
        if (gridItem.Type == ItemType.Mud)
        {
            mainController.ReplaceGridItem(GridPosition.X + offset.X, GridPosition.Y + offset.Y, ItemType.Amoeba);
            currentAmoebaNodeSpawnCount++;
            globalAmoebaSpawnCount++;

            return true;
        }
        return false;
    }

    private void Mutate(double delta)
    {

    }

    private void Grow(double delta)
    {
        timeTick += delta;
        if (timeTick >= GROW_TIME)
            return;

        if ((currentGrowTick == 0) || (currentGrowTick <= growTick))
        {
            currentGrowTick += delta;
            return;
        }
        currentGrowTick = 0;

        if (globalAmoebaSpawnCount < GLOBAL_SPAWN_LIMIT)
        {
            GD.Print("Amoeba time:", (int)timeTick, " global amoeba spawn: ", globalAmoebaSpawnCount);

            if (currentAmoebaNodeSpawnCount < MAX_CHILD_NODE)
            {
                if (!Spawn())
                    growTick -= (growTick * 0.10);
                else
                    growTick = 2.0;
            }
        }
    }

    public override void ProcessAndUpdate(double delta)
    {
        Grow(delta);
    }
}