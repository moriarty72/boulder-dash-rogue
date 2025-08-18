using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProceduralDungeon.Level
{
    public class DungeonItemKey : DungeonItem
    {
        private int keyLevel;

        public int KeyLevel { get { return keyLevel; } }

        public DungeonItemKey(int keyLevel, int points, Coordinate position)
            : base(points, position)
        {
            this.keyLevel = keyLevel;
        }
    }
}
