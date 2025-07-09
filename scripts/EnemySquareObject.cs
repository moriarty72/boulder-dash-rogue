using Godot;
using System;
using System.Data;
using System.Dynamic;

public partial class EnemySquareObject : BaseGridObject
{
    private enum MoveState
    {
        None,
        MoveUp,
        MoveRight,
        MoveDown,
        MoveLeft,
        MoveLeftUp,
        MoveRightDown
    }

    private MoveState moveState = MoveState.MoveUp;
    private MoveState lastHorizontalMove = MoveState.MoveRight;
    private MoveState lastVerticalMove = MoveState.MoveUp;
    private double lastMoveTick = 0;

    private void MoveOLD(double delta)
    {
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

    private void UpdatePosition(Vector2I newPosition)
    {
        mainController.SwapGridItems(GridPosition, newPosition, false);
        NodeObject.GlobalPosition = new(newPosition.X * 64, newPosition.Y * 64);
        GridPosition = newPosition;
    }

    private void Move(double delta)
    {
        Vector2I nextMovePosition = GridPosition;
        MoveState nextMoveState = MoveState.None;

        switch (moveState)
        {
            case MoveState.MoveUp:
                {
                    nextMovePosition = new(GridPosition.X, GridPosition.Y - 1);
                    nextMoveState = (lastHorizontalMove == MoveState.MoveRight) ? MoveState.MoveLeft : MoveState.MoveRight;
                    lastVerticalMove = MoveState.MoveUp;
                    break;
                }

            case MoveState.MoveRight:
                {
                    nextMovePosition = new(GridPosition.X + 1, GridPosition.Y);
                    nextMoveState = (lastVerticalMove == MoveState.MoveUp) ? MoveState.MoveDown : MoveState.MoveUp;
                    lastHorizontalMove = MoveState.MoveRight;
                    break;
                }

            case MoveState.MoveDown:
                {
                    nextMovePosition = new(GridPosition.X, GridPosition.Y + 1);
                    nextMoveState = (lastHorizontalMove == MoveState.MoveRight) ? MoveState.MoveLeft : MoveState.MoveRight;
                    lastVerticalMove = MoveState.MoveDown;
                    break;
                }

            case MoveState.MoveLeft:
                {
                    nextMovePosition = new(GridPosition.X - 1, GridPosition.Y);
                    nextMoveState = (lastVerticalMove == MoveState.MoveUp) ? MoveState.MoveDown : MoveState.MoveUp;
                    lastHorizontalMove = MoveState.MoveLeft;
                    break;
                }

        }

        BaseGridObject gridObject = mainController.GetGridItem(nextMovePosition.X, nextMovePosition.Y);
        if (gridObject.Type == ItemType.None)
            UpdatePosition(nextMovePosition);
        else
            moveState = nextMoveState;
    }

    public override void Process(double delta)
    {
        if ((lastMoveTick == 0) || (lastMoveTick <= 0.10))
        {
            lastMoveTick += delta;
            return;
        }
        lastMoveTick = 0;

        Move(delta);
    }
}