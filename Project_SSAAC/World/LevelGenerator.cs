// 파일 위치: Project_SSAAC/World/LevelGenerator.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects;
using System.Diagnostics;
using System.Linq;

namespace Project_SSAAC.World
{
    public class LevelGenerator
    {
        private Random _random = new Random();
        private SizeF _roomPixelSizeToUse;

        public LevelGenerator(SizeF roomActualPixelSize)
        {
            _roomPixelSizeToUse = roomActualPixelSize;
            if (_roomPixelSizeToUse.IsEmpty || _roomPixelSizeToUse.Width <= 0 || _roomPixelSizeToUse.Height <= 0)
            {
                Debug.WriteLine($"[LevelGenerator] Constructor WARNING: roomActualPixelSize ({roomActualPixelSize}) is invalid. Defaulting to 1024x576. Ensure Form1.ClientSize is passed correctly.");
                _roomPixelSizeToUse = new SizeF(1024, 576);
            }
        }

        public Level GenerateLevel(int numberOfRooms)
        {
            Debug.WriteLine($"[LevelGenerator] Generating level with requested {numberOfRooms} rooms. RoomSize for new Rooms: {_roomPixelSizeToUse}");
            if (numberOfRooms < 1)
            {
                Debug.WriteLine("[LevelGenerator] numberOfRooms < 1, forcing to 1.");
                numberOfRooms = 1;
            }

            Level level = new Level(_roomPixelSizeToUse);
            Room startRoom = new Room(new Point(0, 0), RoomType.Start, _roomPixelSizeToUse);
            level.AddRoom(startRoom);

            if (level.CurrentRoom == null)
            {
                Debug.WriteLine("[LevelGenerator] CRITICAL FALLBACK: CurrentRoom was not set by AddRoom after adding StartRoom. Forcing it now.");
                level.ForceSetCurrentRoom(startRoom);
            }
            if (level.CurrentRoom == null)
            {
                Debug.WriteLine("[LevelGenerator] CRITICAL FAILURE: CurrentRoom is STILL NULL after force set. Returning level as is (likely only start room or empty).");
                return level;
            }

            List<Room> frontier = new List<Room> { startRoom };
            int roomsSuccessfullyCreated = 1;

            for (int i = 1; i < numberOfRooms && frontier.Count > 0; i++)
            {
                Room expandFromRoom = frontier[_random.Next(frontier.Count)];

                List<Point> directions = new List<Point> { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };
                directions = directions.OrderBy(x => _random.Next()).ToList();

                bool placedThisIteration = false;
                foreach (Point dirOffset in directions)
                {
                    Point nextGridPos = new Point(expandFromRoom.GridPosition.X + dirOffset.X, expandFromRoom.GridPosition.Y + dirOffset.Y);
                    if (!level.Rooms.ContainsKey(nextGridPos))
                    {
                        RoomType newRoomType = RoomType.Normal;
                        if (roomsSuccessfullyCreated == numberOfRooms - 1) newRoomType = RoomType.Boss;
                        else if (_random.NextDouble() < 0.08 && roomsSuccessfullyCreated > 2 && numberOfRooms > 5) newRoomType = RoomType.Survival;
                        else if (_random.NextDouble() < 0.10 && roomsSuccessfullyCreated > 1 && numberOfRooms > 4) newRoomType = RoomType.Puzzle;
                        else if (_random.NextDouble() < 0.15 && roomsSuccessfullyCreated > 2) newRoomType = RoomType.Treasure;
                        else if (_random.NextDouble() < 0.10 && roomsSuccessfullyCreated > 3) newRoomType = RoomType.Shop;
                        else if (_random.NextDouble() < 0.10 && roomsSuccessfullyCreated > 2 && numberOfRooms > 6) newRoomType = RoomType.MiniBoss;

                        Room newRoom = new Room(nextGridPos, newRoomType, _roomPixelSizeToUse);

                        if (newRoom.Type == RoomType.Puzzle)
                        {
                            int num1 = _random.Next(1, 10); int num2 = _random.Next(1, 10);
                            newRoom.PuzzleQuestion = $"Question: {num1} + {num2} = ?";
                            newRoom.PuzzleAnswer = (num1 + num2).ToString();
                            newRoom.PuzzleTimeLimit = (float)_random.Next(20, 41);
                        }
                        else if (newRoom.Type == RoomType.Survival)
                        {
                            newRoom.SurvivalTimeDuration = 30.0f;
                        }

                        level.AddRoom(newRoom);
                        ConnectRooms(expandFromRoom, newRoom, dirOffset);
                        frontier.Add(newRoom);
                        roomsSuccessfullyCreated++;
                        placedThisIteration = true;
                        break;
                    }
                }

                if (!placedThisIteration)
                {
                    frontier.Remove(expandFromRoom);
                    i--;
                    Debug.WriteLine($"[LevelGenerator] Failed to expand from {expandFromRoom.GridPosition}. Removed from frontier. Retrying for room count.");
                }
            }

            foreach (var room in level.Rooms.Values)
            {
                bool needsEnemies = room.Type == RoomType.Normal ||
                                    room.Type == RoomType.MiniBoss ||
                                    room.Type == RoomType.Boss ||
                                   (room.Type == RoomType.Puzzle && !room.IsPuzzleSolved) ||
                                   (room.Type == RoomType.Survival && !room.IsSurvivalCompleted);

                if (needsEnemies)
                {
                    int enemyCount = 1;
                    if (room.Type == RoomType.Normal) enemyCount = _random.Next(1, 4);
                    else if (room.Type == RoomType.MiniBoss) enemyCount = _random.Next(2, 4);
                    else if (room.Type == RoomType.Puzzle) enemyCount = _random.Next(1, 3);
                    else if (room.Type == RoomType.Survival) enemyCount = _random.Next(3, 6);
                    else if (room.Type == RoomType.Boss) enemyCount = 1;

                    for (int k = 0; k < enemyCount; k++)
                    {
                        float spawnMarginX = _roomPixelSizeToUse.Width * 0.2f;
                        float spawnMarginY = _roomPixelSizeToUse.Height * 0.2f;
                        float spawnX = (float)(_random.NextDouble() * (_roomPixelSizeToUse.Width - 2 * spawnMarginX) + spawnMarginX);
                        float spawnY = (float)(_random.NextDouble() * (_roomPixelSizeToUse.Height - 2 * spawnMarginY) + spawnMarginY);

                        room.AddEnemySpawn(typeof(BasicEnemy), new PointF(spawnX, spawnY));
                    }
                }

                if (room.Type == RoomType.Normal || room.Type == RoomType.Start || room.Type == RoomType.Treasure)
                {
                    PopulateRoomWithObstacles(room);
                }
            }
            Debug.WriteLine($"[LevelGenerator] Generation finished. Total rooms in level: {level.Rooms.Count} (Target: {numberOfRooms}). CurrentRoom set to: {level.CurrentRoom?.GridPosition}");
            return level;
        }

        private void ConnectRooms(Room room1, Room room2, Point directionOffset)
        {
            if (directionOffset.X == 1) { room1.HasRightDoor = true; room2.HasLeftDoor = true; }
            else if (directionOffset.X == -1) { room1.HasLeftDoor = true; room2.HasRightDoor = true; }
            else if (directionOffset.Y == 1) { room1.HasBottomDoor = true; room2.HasTopDoor = true; }
            else if (directionOffset.Y == -1) { room1.HasTopDoor = true; room2.HasBottomDoor = true; }
        }

        // **[수정됨] 바위, 가시, 구덩이를 모두 생성하도록 변경**
        private void PopulateRoomWithObstacles(Room room)
        {
            int obstacleCount = _random.Next(4, 10); // 장애물 개수 조정
            SizeF obstacleSize = new SizeF(40, 40);

            float marginX = _roomPixelSizeToUse.Width * 0.15f;
            float marginY = _roomPixelSizeToUse.Height * 0.15f;

            RectangleF spawnArea = new RectangleF(
                marginX, marginY,
                _roomPixelSizeToUse.Width - marginX * 2,
                _roomPixelSizeToUse.Height - marginY * 2
            );

            for (int i = 0; i < obstacleCount; i++)
            {
                PointF position;
                bool positionIsSafe;
                int attempts = 0;

                do
                {
                    float x = (float)(spawnArea.X + _random.NextDouble() * spawnArea.Width);
                    float y = (float)(spawnArea.Y + _random.NextDouble() * spawnArea.Height);
                    position = new PointF(x, y);

                    positionIsSafe = true;
                    RectangleF newObstacleBounds = new RectangleF(position, obstacleSize);

                    foreach (var existingObstacle in room.Obstacles)
                    {
                        if (newObstacleBounds.IntersectsWith(existingObstacle.Bounds))
                        {
                            positionIsSafe = false;
                            break;
                        }
                    }
                    attempts++;
                } while (!positionIsSafe && attempts < 10);

                if (positionIsSafe)
                {
                    // 50% 바위, 25% 가시, 25% 구덩이 확률로 생성
                    double chance = _random.NextDouble();
                    ObstacleType typeToSpawn;
                    if (chance < 0.5)
                    {
                        typeToSpawn = ObstacleType.Rock;
                    }
                    else if (chance < 0.75)
                    {
                        typeToSpawn = ObstacleType.Spikes;
                    }
                    else
                    {
                        typeToSpawn = ObstacleType.Pit;
                    }

                    room.Obstacles.Add(new Obstacle(position, obstacleSize, typeToSpawn));
                }
            }
        }
    }
}