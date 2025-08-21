using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Godot;
using ProceduralDungeon.Level;

namespace ProceduralDungeon.Level
{
    class DungeonLevel
    {
        private int width;
        private int height;
        private int roomCount;
        private List<DungeonRoom> rooms;
        private DungeonRoom[,] roomsMap;
        private int seed;
        private Random rnd;
        private Coordinate startRoomPosition;
        private List<int> roomWeightCount;
        private int minRoomWidthSize;
        private int minRoomHeightSize;
        private int maxRoomWidthSize;
        private int maxRoomHeightSize;

        public DungeonRoom StartingRoom { get { return rooms[0]; } }

        public DungeonLevel(int width, int height, int roomCount, int seed, int weightCount, int minRoomWidthSize, int minRoomHeightSize, int maxRoomWidthSize, int maxRoomHeightSize)
        {
            this.width = width;
            this.height = height;
            this.roomCount = roomCount;
            this.seed = seed;
            this.minRoomWidthSize = minRoomWidthSize;
            this.minRoomHeightSize = minRoomHeightSize;
            this.maxRoomWidthSize = maxRoomWidthSize;
            this.maxRoomHeightSize = maxRoomHeightSize;

            rnd = new Random(seed);

            if (weightCount > 0)
                InitializeRoomWeights(weightCount);
        }

        public void Build()
        {
            rooms = new List<DungeonRoom>();
            roomsMap = new DungeonRoom[width, height];

            // spwan initial room
            InitializeStartRoom();
            InitializeRooms();

            InitializeRoomsItems();

            DumpFile();
        }

        private void InitializeRoomsItems()
        {
            // place unlock keys...
            int roomOffset = 0;
            if (roomWeightCount.Count > 1)
            {
                for(int w = 0; w < roomWeightCount.Count; w++)
                {
                    int roomCount = rooms.Where(r => r.Weight == w).Count();
                    int roomKeyIndex = rnd.Next(roomCount);
                    DungeonRoom dungeonRoom = rooms[roomOffset + roomKeyIndex];
                    Coordinate itemPosition = new Coordinate(rnd.Next(1, dungeonRoom.WidthSize - 1), rnd.Next(1, dungeonRoom.HeightSize - 1));

                    dungeonRoom.Items.Add(new DungeonItemKey(w + 1, 1000, itemPosition));
                    roomOffset += roomCount;
                }
            }
        }

        private void InitializeRoomWeights(int weightCount)
        {
            List<float> wightPercent = new List<float>(new float[weightCount]);

            for (int i = 0; i < weightCount; i++)
                wightPercent[i] = rnd.Next(1, 100);

            float k = wightPercent.Sum() / 100.0f;

            for (int i = 0; i < weightCount; i++)
                wightPercent[i] /= k;

            wightPercent.Sort();

            roomWeightCount = [.. new int[weightCount]];
            for (int i = 0; i < weightCount; i++)
                roomWeightCount[i] = (int)(this.roomCount * wightPercent[i] / 100);

            int sum = roomWeightCount.Sum();
            if (sum < this.roomCount)
            {
                int diff = this.roomCount - sum;
                roomWeightCount[0] += diff;
            }
        }

        private void DumpFile()
        {
            List<string> map = new List<string>();
            map.Add("Seed: " + this.seed);

            for(int y = 0; y < height; y++)
            {
                string line1 = "";
                string line2 = "";
                for (int x = 0; x < width; x++)
                {
                    DungeonRoom room = roomsMap[x, y];
                    if (room == null)
                    {
                        line1 = line1 + " . ";
                        line2 = line2 + "   ";
                    }
                    else
                    {
                        if (room.RoomConnections[(int)DungeonRoom.Connection.Right] != null)
                            line1 = line1 + string.Format("{0, 2}-", room.Index);
                        else
                            line1 = line1 + string.Format("{0, 2} ", room.Index);

                        if (room.RoomConnections[(int)DungeonRoom.Connection.Down] != null)
                            line2 = line2 + " | ";
                        else
                            line2 = line2 + "   ";
                    }
                }
                map.Add(line1);
                map.Add(line2);
            }

            File.WriteAllLines(string.Format("c:\\temp\\Dungeon.txt", seed), map.ToArray());
        }

        private void InitializeStartRoom()
        {
            int roomWidthSize = rnd.Next(minRoomWidthSize, maxRoomWidthSize);
            int roomHeightSize = rnd.Next(minRoomHeightSize, maxRoomHeightSize);

            DungeonRoom startRoom = new(0, roomWidthSize, roomHeightSize)
            {
                Position = new Coordinate(width >> 1, height >> 1),
                Weight = GetRoomWeight()
            };

            rooms.Add(startRoom);
            roomsMap[startRoom.Position.X, startRoom.Position.Y] = startRoom;
        }

        private void InitializeRooms()
        {
            while (rooms.Count < roomCount)
            {
                DungeonRoom randomRoom = rooms[rnd.Next(rooms.Count)];
                if (!randomRoom.HasAvailableConnections())
                    continue;

                DungeonRoom.Connection roomConnection = GetRandomConnection(randomRoom);
                if (roomConnection == DungeonRoom.Connection.Max)
                    continue;

                Coordinate newRoomPosition = GetNewRoomPosition(randomRoom, roomConnection);
                if (roomsMap[newRoomPosition.X, newRoomPosition.Y] != null)
                    continue;

                // create the new room
                int roomWidthSize = rnd.Next(minRoomWidthSize, maxRoomWidthSize);
                int roomHeightSize = rnd.Next(minRoomHeightSize, maxRoomHeightSize);

                DungeonRoom newRoom = new DungeonRoom(0, roomWidthSize, roomHeightSize)
                {
                    Index = rooms.Count,
                    Weight = GetRoomWeight(),
                    Position = newRoomPosition
                };

                ConnectRooms(randomRoom, newRoom, roomConnection);

                roomsMap[newRoomPosition.X, newRoomPosition.Y] = newRoom;
                rooms.Add(newRoom);
            }
        }

        private void ConnectRooms(DungeonRoom parentRoom, DungeonRoom newRoom, DungeonRoom.Connection roomConnection)
        {
            int minPosition = 0;
            int maxPosition = 0;
            int minPositionNew = 0;
            int maxPositionNew = 0;

            DungeonRoom.Connection newRoomConnection = DungeonRoom.Connection.Max;
            if (roomConnection == DungeonRoom.Connection.Up)
            {
                minPosition = 2;
                maxPosition = parentRoom.WidthSize - 2;
                minPositionNew = 2;
                maxPositionNew = newRoom.WidthSize - 2;
                newRoomConnection = DungeonRoom.Connection.Down;
            }
            else if (roomConnection == DungeonRoom.Connection.Down)
            {
                minPosition = 2;
                maxPosition = parentRoom.WidthSize - 2;
                minPositionNew = 2;
                maxPositionNew = newRoom.WidthSize - 2;
                newRoomConnection = DungeonRoom.Connection.Up;
            }
            else if (roomConnection == DungeonRoom.Connection.Left)
            {
                minPosition = 2;
                maxPosition = parentRoom.HeightSize - 2;
                minPositionNew = 2;
                maxPositionNew = newRoom.HeightSize - 2;
                newRoomConnection = DungeonRoom.Connection.Right;
            }
            else if (roomConnection == DungeonRoom.Connection.Right)
            {
                minPosition = 2;
                maxPosition = parentRoom.HeightSize - 2;
                minPositionNew = 2;
                maxPositionNew = newRoom.HeightSize - 2;
                newRoomConnection = DungeonRoom.Connection.Left;
            }

            DungeonRoomConnection connection = new DungeonRoomConnection(parentRoom, newRoom);

            parentRoom.RoomConnections[(int)roomConnection] = connection;
            parentRoom.RoomConnectionPositions[(int)roomConnection] = rnd.Next(minPosition, maxPosition);

            newRoom.RoomConnections[(int)newRoomConnection] = connection.Swap();
            newRoom.RoomConnectionPositions[(int)newRoomConnection] = rnd.Next(minPositionNew, maxPositionNew);
        }

        private Coordinate GetNewRoomPosition(DungeonRoom parentRoom, DungeonRoom.Connection roomConnection)
        {
            Coordinate newRoomPosition = new Coordinate(parentRoom.Position);
            if (roomConnection == DungeonRoom.Connection.Up)
                newRoomPosition.Y--;
            else if (roomConnection == DungeonRoom.Connection.Down)
                newRoomPosition.Y++;
            else if (roomConnection == DungeonRoom.Connection.Left)
                newRoomPosition.X--;
            else if (roomConnection == DungeonRoom.Connection.Right)
                newRoomPosition.X++;

            return newRoomPosition;
        }

        private int GetRoomWeight()
        {
            if (this.roomWeightCount == null)
                return 0;

            int roomWeight;
            for (roomWeight = 0; roomWeight < this.roomWeightCount.Count; roomWeight++)
            {
                if (this.roomWeightCount[roomWeight]-- > 0)
                    break;
            }

            return roomWeight;
        }

        private DungeonRoom.Connection GetRandomConnection(DungeonRoom room)
        {
            int randomConnection = rnd.Next((int)DungeonRoom.Connection.Max);

            int index = randomConnection;
            while(true)
            {
                if (room.RoomConnections[index] == null)
                    break;

                int direction = (rnd.Next() % 2 == 0) ? -1 : 1;

                index = Math.Abs(index + direction) % (int)DungeonRoom.Connection.Max;
            }

            if (((DungeonRoom.Connection)index == DungeonRoom.Connection.Up) && (room.Position.Y == 0))
                return DungeonRoom.Connection.Max;

            if (((DungeonRoom.Connection)index == DungeonRoom.Connection.Down) && (room.Position.Y == (height - 1)))
                return DungeonRoom.Connection.Max;

            if (((DungeonRoom.Connection)index == DungeonRoom.Connection.Left) && (room.Position.X == 0))
                return DungeonRoom.Connection.Max;

            if (((DungeonRoom.Connection)index == DungeonRoom.Connection.Right) && (room.Position.X == (width - 1)))
                return DungeonRoom.Connection.Max;
        
            return (DungeonRoom.Connection)index;
        }

        private Coordinate GetRandomPosition()
        {
            return new Coordinate(rnd.Next(width), rnd.Next(height));
        }
    }
}
