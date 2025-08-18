using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralDungeon.Level
{
    public class DungeonRoomConnection
    {
        private bool isLocked;
        private int unlockLevelNeeded;
        private DungeonRoom sourceRoom;
        private DungeonRoom destinationRoom;

        public bool IsLocked { get { return isLocked; } }
        public int UnlockLevelNeeded { get { return unlockLevelNeeded; } }
        public DungeonRoom SourceRoom { get { return sourceRoom; } }
        public DungeonRoom DestinationRoom { get { return destinationRoom; } }

        public DungeonRoomConnection(DungeonRoom sourceRoom, DungeonRoom destinationRoom)
        {
            this.isLocked = destinationRoom.Weight > sourceRoom.Weight;
            this.unlockLevelNeeded = destinationRoom.Weight;
            this.sourceRoom = sourceRoom;
            this.destinationRoom = destinationRoom;
        }

        public void Unlock()
        {
            this.isLocked = false;
        }

        public DungeonRoomConnection Swap()
        {
            return new DungeonRoomConnection(this.destinationRoom, this.sourceRoom);
        }
    }
}
