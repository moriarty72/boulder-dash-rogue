using Godot;
using System;
using System.Data;
using System.Dynamic;

public partial class EnemySquareObject : BaseGridObject
{
    private enum MoveState
    {
        MoveLeftUp,
        MoveRightDown
    }

    private MoveState moveState = MoveState.MoveLeftUp;
    private double lastMoveTick = 0;

    private void Move(double delta)
    {
        if ((lastMoveTick == 0) || (lastMoveTick <= 0.10))
        {
            lastMoveTick += delta;
            return;
        }
        lastMoveTick = 0;

        if (moveState == MoveState.MoveLeftUp)
        {
            // try to move UP
            Vector2I upMovePosition = new(GridPosition.X, GridPosition.Y - 1);
            BaseGridObject upMoveObject = mainController.GetGridItem(upMovePosition.X, upMovePosition.Y);
            if (upMoveObject.Type == ItemType.None)
            {
                mainController.SwapGridItems(GridPosition, upMovePosition, false);
                NodeObject.GlobalPosition = new(upMovePosition.X * 64, upMovePosition.Y * 64);
                GridPosition = upMovePosition;
                return;
            }

            // try to move LEFT
            Vector2I leftMovePosition = new(GridPosition.X - 1, GridPosition.Y);
            BaseGridObject leftMoveObject = mainController.GetGridItem(leftMovePosition.X, leftMovePosition.Y);
            if (leftMoveObject.Type == ItemType.None)
            {
                mainController.SwapGridItems(GridPosition, leftMovePosition, false);
                NodeObject.GlobalPosition = new(leftMovePosition.X * 64, leftMovePosition.Y * 64);
                GridPosition = leftMovePosition;
                return;
            }

            moveState = MoveState.MoveRightDown;
        }
        else if (moveState == MoveState.MoveRightDown)
        {
            // try to move DOWN
            Vector2I downMovePosition = new(GridPosition.X, GridPosition.Y + 1);
            BaseGridObject downMoveObject = mainController.GetGridItem(downMovePosition.X, downMovePosition.Y);
            if (downMoveObject.Type == ItemType.None)
            {
                mainController.SwapGridItems(GridPosition, downMovePosition, false);
                NodeObject.GlobalPosition = new(downMovePosition.X * 64, downMovePosition.Y * 64);
                GridPosition = downMovePosition;
                return;
            }

            // try to move RIGHT
            Vector2I rightMovePosition = new(GridPosition.X + 1, GridPosition.Y);
            BaseGridObject rightMoveObject = mainController.GetGridItem(rightMovePosition.X, rightMovePosition.Y);
            if (rightMoveObject.Type == ItemType.None)
            {
                mainController.SwapGridItems(GridPosition, rightMovePosition, false);
                NodeObject.GlobalPosition = new(rightMovePosition.X * 64, rightMovePosition.Y * 64);
                GridPosition = rightMovePosition;
                return;
            }

            moveState = MoveState.MoveLeftUp;
        }
    }

    public override void Process(double delta)
    {
        Move(delta);
    }
}