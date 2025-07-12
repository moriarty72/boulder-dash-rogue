using Godot;
using System;
using System.Data;
using System.Dynamic;

public partial class EnemySquareObject : BaseGridObject
{
    private double lastMoveTick = 0;

    private Vector2I moveDirection = new(1, 0);

    private void MoveByDirection(double delta)
    {
        Vector2I rotate(Vector2I vec, float deg)
        {
            var x = vec.X * (float)Math.Cos(float.DegreesToRadians(deg)) - vec.Y * (float)Math.Sin(float.DegreesToRadians(deg));
            var y = vec.X * (float)Math.Sin(float.DegreesToRadians(deg)) + vec.Y * (float)Math.Cos(float.DegreesToRadians(deg));
            return new((int)x, (int)y);
        }

        bool checkMoveAvailability(Vector2I nextDirection)
        {
            Vector2I nextPosition = new(GridPosition.X + nextDirection.X, GridPosition.Y + nextDirection.Y);
            Vector2I nextMovePosition = new(nextPosition.X, nextPosition.Y);

            BaseGridObject gridObject = mainController.GetGridItem(nextMovePosition.X, nextMovePosition.Y);
            return gridObject.Type == ItemType.None;
        }

        // GD.Print("Enemy direction " + moveDirection);

        Vector2I leftDirection = rotate(moveDirection, -90);
        if (checkMoveAvailability(leftDirection))
        {
            UpdatePosition(new(GridPosition.X + leftDirection.X, GridPosition.Y + leftDirection.Y));
            moveDirection = leftDirection;
            return;
        }

        Vector2I nextPosition = new(GridPosition.X + moveDirection.X, GridPosition.Y + moveDirection.Y);
        Vector2I nextMovePosition = new(nextPosition.X, nextPosition.Y);

        BaseGridObject gridObject = mainController.GetGridItem(nextMovePosition.X, nextMovePosition.Y);
        if (gridObject.Type == ItemType.None)
        {
            UpdatePosition(nextMovePosition);
            return;
        }

        Vector2I rightDirection = rotate(moveDirection, 90);
        if (checkMoveAvailability(rightDirection))
        {
            UpdatePosition(new(GridPosition.X + rightDirection.X, GridPosition.Y + rightDirection.Y));
            moveDirection = rightDirection;
            return;
        }

        Vector2I invertDirection = rotate(moveDirection, 180);
        if (checkMoveAvailability(invertDirection))
        {
            UpdatePosition(new(GridPosition.X + invertDirection.X, GridPosition.Y + invertDirection.Y));
            moveDirection = invertDirection;
            return;
        }
    }

    private void UpdatePosition(Vector2I newPosition)
    {
        mainController.SwapGridItems(GridPosition, newPosition, false);
        NodeObject.GlobalPosition = new(newPosition.X * 64, newPosition.Y * 64);
        GridPosition = newPosition;
    }

    private void CheckCollision()
    {
        Vector2I[] collisionOffset = [new(1, 0), new(0, -1), new(-1, 0), new(0, 1)];

        for (int i = 0; i < collisionOffset.Length; i++) {
            if (mainController.CheckObjectCollision(GridPosition, collisionOffset[i].X, collisionOffset[i].Y, this))
            {
                mainController.RemoveGridItem(GridPosition);
                break;
            }
        }
    }

    public override void Process(double delta)
    {
        if ((lastMoveTick == 0) || (lastMoveTick <= 0.10))
        {
            lastMoveTick += delta;
            return;
        }
        lastMoveTick = 0;

        CheckCollision();
        MoveByDirection(delta);
    }
}