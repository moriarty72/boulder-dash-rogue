using System.Collections.Generic;
using System.Linq;

public class DungeonRoomState
{
    private List<BaseGridObjectController> roomGrid = null;

    public bool HasState()
    {
        return roomGrid != null;
    }

    public void SaveState(List<BaseGridObjectController> currentRoomGrid)
    {
        roomGrid = [.. currentRoomGrid.Select(room => room.Clone())];
    }

    public List<BaseGridObjectController> RetrieveState()
    {
        return roomGrid;
    }
}