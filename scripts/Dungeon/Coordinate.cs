using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace ProceduralDungeon.Level
{
    public class Coordinate
    {
        public int X;
        public int Y;

        public Coordinate(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Coordinate(Coordinate coordinate)
        {
            this.X = coordinate.X;
            this.Y = coordinate.Y;
        }

        public int GetIndex()
        {
            return X * Y;
        }

        public Vector2I ToVector2I()
        {
            return new(X, Y);
        }
    }
}
