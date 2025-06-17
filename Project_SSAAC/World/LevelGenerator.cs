// 파일 위치: Project_SSAAC/World/LevelGenerator.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects;
using System.Diagnostics;
using System.Linq;
// RoomLayouts를 사용하기 위해 네임스페이스 추가
using Project_SSAAC.World.Layouts;

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
                        else if (_random.NextDouble() < 0.15 && roomsSuccessfullyCreated > 1 && numberOfRooms > 4) newRoomType = RoomType.Puzzle;
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
                string[] layoutToApply = RoomLayouts.GetLayoutForRoomType(room.Type);

                if (layoutToApply != null)
                {
                    ApplyLayoutToRoom(room, layoutToApply);
                }

                if (room.Type == RoomType.Boss && room.EnemyTypesToSpawn.Count == 0)
                {
                    // 보스룸 예외 처리 로직 (필요시)
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

        private void ApplyLayoutToRoom(Room room, string[] layout)
        {
            if (layout == null || layout.Length == 0) return;

            room.Obstacles.Clear();
            room.EnemySpawnPositions.Clear();
            room.EnemyTypesToSpawn.Clear();

            int layoutHeight = layout.Length;

            int layoutMaxWidth = 0;
            foreach (string row in layout)
            {
                if (row.Length > layoutMaxWidth)
                {
                    layoutMaxWidth = row.Length;
                }
            }
            if (layoutMaxWidth == 0) return;

            float cellWidth = _roomPixelSizeToUse.Width / layoutMaxWidth;
            float cellHeight = _roomPixelSizeToUse.Height / layoutHeight;

            for (int y = 0; y < layoutHeight; y++)
            {
                string currentRow = layout[y];
                if (string.IsNullOrEmpty(currentRow))
                {
                    continue;
                }

                for (int x = 0; x < currentRow.Length; x++)
                {
                    // <<-- 새로 추가된 부분: 문 위치를 확인하고 비워두는 로직 -->>
                    bool isDoorway = false;
                    int midX1 = layoutMaxWidth / 2 - 1; // 위/아래 문의 왼쪽 칸
                    int midX2 = layoutMaxWidth / 2;     // 위/아래 문의 오른쪽 칸
                    int midY = layoutHeight / 2;        // 좌/우 문의 중앙 칸

                    // 위쪽 문 확인
                    if (room.HasTopDoor && y == 0 && (x == midX1 || x == midX2)) isDoorway = true;
                    // 아래쪽 문 확인
                    if (room.HasBottomDoor && y == layoutHeight - 1 && (x == midX1 || x == midX2)) isDoorway = true;
                    // 왼쪽 문 확인
                    if (room.HasLeftDoor && x == 0 && y == midY) isDoorway = true;
                    // 오른쪽 문 확인
                    if (room.HasRightDoor && x == layoutMaxWidth - 1 && y == midY) isDoorway = true;

                    // 현재 위치가 문 입구라면, 아무것도 배치하지 않고 건너뜀
                    if (isDoorway)
                    {
                        continue;
                    }
                    // -- 추가된 부분 끝 --

                    char symbol = currentRow[x];
                    if (symbol == '.') continue;

                    PointF position = new PointF(x * cellWidth, y * cellHeight);
                    SizeF objectSize = new SizeF(cellWidth, cellHeight);

                    switch (symbol)
                    {
                        case 'R':
                            room.Obstacles.Add(new Obstacle(position, objectSize, ObstacleType.Rock));
                            break;
                        case 'S':
                            room.Obstacles.Add(new Obstacle(position, objectSize, ObstacleType.Spikes));
                            break;
                        case 'P':
                            room.Obstacles.Add(new Obstacle(position, objectSize, ObstacleType.Pit));
                            break;
                        case 'E':
                            PointF enemySpawnPos = new PointF(position.X + (cellWidth / 2) - 15, position.Y + (cellHeight / 2) - 15);
                            room.AddEnemySpawn(typeof(BasicEnemy), enemySpawnPos);
                            break;
                    }
                }
            }
        }
    }
}