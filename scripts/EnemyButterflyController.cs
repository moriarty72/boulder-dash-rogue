using Godot;
using System;
using System.Data;
using System.Dynamic;

public partial class EnemyButterflyController : BaseGridObjectController
{
    private Timer processTimer = new(0.35);

    private void MoveByDirection(double delta)
    {
        Vector2I calculateMoveOffset()
        {
            Vector2I rockforPosition = mainController.GetRockfordPosition();
            Vector2I movePosition = new(rockforPosition.X - GridPosition.X, rockforPosition.Y - GridPosition.Y);

            int x = movePosition.X > 0 ? 1 : (movePosition.X < 0 ? -1 : 0);
            int y = movePosition.Y > 0 ? 1 : (movePosition.Y < 0 ? -1 : 0);

            Vector2I offset = new(x, y);
            // GD.Print("Butterfly move direction = ", offset, movePosition);

            return offset;
        }

        bool checkMoveAvailability(Vector2I position)
        {
            BaseGridObjectController gridObject = mainController.GetGridItem(position.X, position.Y);
            return gridObject.Type == ItemType.None;
        }

        Vector2I calculateNextAvailableMove(Vector2I moveOffset)
        {
            Vector2I[] availableMoveOffset = [new(1, 1), new(0, 0), new(1, 0), new(0, 1)];
            foreach (var offset in availableMoveOffset)
            {
                Vector2I nextPosition = new(GridPosition.X + (moveOffset.X * offset.X), GridPosition.Y + (moveOffset.Y * offset.Y));
                if (checkMoveAvailability(nextPosition))
                    return nextPosition;
            }
            return GridPosition;
        }

        PrevGridPosition = GridPosition;
        GridPosition = calculateNextAvailableMove(calculateMoveOffset());


    }

    public override void ProcessAndUpdate(double delta)
    {
        if (!processTimer.IsElapsed(delta))
            return;

        MoveByDirection(delta);
        UpdateNodeObjectPosition();
    }
}