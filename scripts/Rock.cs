using Godot;
using System;

public partial class Rock : Area2D
{
    public enum State
    {
        Stand,
        Fall
    }
    [Export]
    public double RockMoveDelay = 0.5;

    public State CurrentState = State.Stand;
    public Vector2I GridPosition;
    private Main mainController;
    private Vector2 currentPosition;
    private double lastMoveTick = 0;

    public void Initilize(Main mc, Vector2I gridPosition)
    {
        mainController = mc;
        currentPosition = GlobalPosition;
        GridPosition = gridPosition;
    }

    private void Move(double delta)
    {
        if (CurrentState == State.Stand)
            return;

        if (lastMoveTick == 0)
        {
            lastMoveTick = delta;
        }

        if (lastMoveTick <= RockMoveDelay)
        {
            lastMoveTick += delta;
            return;
        }

        Vector2I prevGridPosition = new(GridPosition.X, GridPosition.Y);

        GridPosition.Y++;
        currentPosition.Y += 64;

        GlobalPosition = currentPosition;

        mainController.SwapGridItems(prevGridPosition, GridPosition, false);

        lastMoveTick = 0;
        CurrentState = State.Stand;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        bool fall = mainController.CanRockFall(GridPosition);
        if (fall)
        {
            GD.Print("Rock fall " + GridPosition);

            CurrentState = State.Fall;
            Move(delta);
        }
    }
}
