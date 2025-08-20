using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralDungeon.Level
{
    public class DungeonRoom
    {
        public enum Connection : int
        {
            Up,
            Left,
            Right,
            Down,
            Max
        }

        private int index;
        private int weight;
        private Coordinate position;
        private DungeonRoomConnection[] roomConnections;
        private int[] roomConnectionPositions;
        private List<DungeonItem> items;
        private int widthSize;
        private int heightSize;

        public int Index { get { return index; } set { index = value; } }
        public int Weight { get { return weight; } set { weight = value; } }
        public Coordinate Position { get { return position; } set { position = value; } }
        public DungeonRoomConnection[] RoomConnections { get { return roomConnections; } }
        public int[] RoomConnectionPositions { get { return roomConnectionPositions; } }
        public List<DungeonItem> Items { get { return items; } }
        public int WidthSize { get { return widthSize; } }
        public int HeightSize { get { return heightSize; } }

        public DungeonRoomState RoomState;

        public DungeonRoom(int index, int widthSize, int heightSize)
        {
            this.index = index;
            this.weight = 0;
            this.roomConnections = new DungeonRoomConnection[4];
            this.roomConnectionPositions = new int[4];
            this.items = new List<DungeonItem>();
            this.widthSize = widthSize;
            this.heightSize = heightSize;
            this.RoomState = new DungeonRoomState();
        }

        public bool HasAvailableConnections()
        {
            return this.roomConnections.Any(c => c == null);
        }

        public bool IsLeaf()
        {
            return (this.roomConnections.Count(c => c != null) == 1);
        }
    }
}
