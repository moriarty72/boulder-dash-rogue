using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace ProceduralDungeon.Level
{
    public class DungeonItem
    {
        private int points;
        private Coordinate position;
        private Node2D gameObject;

        public int Points { get { return points; } }
        public Coordinate Position { get { return position; } }
        public Node2D GameObject { get { return gameObject; } set { gameObject = value; } }

        public DungeonItem(int points, Coordinate position)
        {
            this.points = points;
            this.position = position;
        }
    }
}
