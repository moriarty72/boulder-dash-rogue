using Godot;
using System;
using System.Collections.Generic;

public partial class AmoebaController : BaseGridObjectController
{
    private class AmoebaItem
    {
        public Vector2I gridPosition;
    }

    private const double GROW_TIME = 60;
    private const double GROW_TICK = 10;

    private const double SPAWN_LIMIT = 10;

    private double timeTick = 0;
    private double growTick = GROW_TICK * 0.75;
    private double currentGrowTick = 0;

    private int amoebaSpawnCount = 1;

    private List<AmoebaItem> amoebas = [];

    public override void Initialize<T>(Main mc, PackedScene packedScene, ItemType itemType, Vector2 worldPosition, Vector2I gridPosition)
    {
        base.Initialize<T>(mc, packedScene, itemType, worldPosition, gridPosition);

        // mainController.PlayAudio("AmoebaAudio");
        amoebas.Add(new() { gridPosition = GridPosition });
    }

    private bool Spawn()
    {

        return true;
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
        growTick *= 0.5;

        if (amoebaSpawnCount < SPAWN_LIMIT)
            amoebaSpawnCount += 1;

        GD.Print("Amoeba time:", (int)timeTick, " amoeba to spawn: ", amoebaSpawnCount);
    }

    public override void ProcessAndUpdate(double delta)
    {
        Grow(delta);
    }
}