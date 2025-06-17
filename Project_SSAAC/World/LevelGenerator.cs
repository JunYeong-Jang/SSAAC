using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects;
using System.Diagnostics;
using System.Linq;

namespace Project_SSAAC.World
{
    /// <summary>
    /// 레벨(방들의 배치 및 내용)을 절차적으로 생성하는 클래스입니다.
    /// </summary>
    public class LevelGenerator
    {
        private Random _random = new Random();
        private SizeF _roomPixelSizeToUse; // 생성될 방들의 실제 픽셀 크기 (Form.ClientSize)

        /// <summary>
        /// LevelGenerator를 초기화합니다.
        /// </summary>
        /// <param name="roomActualPixelSize">생성될 각 방의 실제 픽셀 크기 (Form의 ClientSize)입니다.</param>
        public LevelGenerator(SizeF roomActualPixelSize)
        {
            _roomPixelSizeToUse = roomActualPixelSize;
            if (_roomPixelSizeToUse.IsEmpty || _roomPixelSizeToUse.Width <= 0 || _roomPixelSizeToUse.Height <= 0)
            {
                Debug.WriteLine($"[LevelGenerator] Constructor WARNING: roomActualPixelSize ({roomActualPixelSize}) is invalid. Defaulting to 1024x576. Ensure Form1.ClientSize is passed correctly.");
                _roomPixelSizeToUse = new SizeF(1024, 576); // Form1의 ClientSize와 일치해야 함
            }
        }

        /// <summary>
        /// 지정된 수의 방을 가진 새로운 레벨을 생성합니다.
        /// </summary>
        /// <param name="numberOfRooms">생성할 총 방의 수 (시작방 포함)입니다.</param>
        /// <returns>생성된 Level 객체입니다.</returns>
        public Level GenerateLevel(int numberOfRooms)
        {
            Debug.WriteLine($"[LevelGenerator] Generating level with requested {numberOfRooms} rooms. RoomSize for new Rooms: {_roomPixelSizeToUse}");
            if (numberOfRooms < 1)
            {
                Debug.WriteLine("[LevelGenerator] numberOfRooms < 1, forcing to 1.");
                numberOfRooms = 1; // 최소 1개의 방(시작방)은 보장
            }

            Level level = new Level(_roomPixelSizeToUse); // Level 객체에 실제 방 크기 전달
            Room startRoom = new Room(new Point(0, 0), RoomType.Start, _roomPixelSizeToUse); // Room 객체에도 실제 방 크기 전달
            level.AddRoom(startRoom);

            // AddRoom 후 CurrentRoom이 설정되었는지 확인 (매우 중요)
            if (level.CurrentRoom == null)
            {
                Debug.WriteLine("[LevelGenerator] CRITICAL FALLBACK: CurrentRoom was not set by AddRoom after adding StartRoom. Forcing it now.");
                level.ForceSetCurrentRoom(startRoom); // Level 클래스의 방어 로직
            }
            if (level.CurrentRoom == null) // 그래도 CurrentRoom이 null이면, Level 객체 반환 후 Form1에서 처리
            {
                Debug.WriteLine("[LevelGenerator] CRITICAL FAILURE: CurrentRoom is STILL NULL after force set. Returning level as is (likely only start room or empty).");
                return level;
            }

            List<Room> frontier = new List<Room> { startRoom }; // 방 확장을 시작할 수 있는 "개척지" 방 목록
            int roomsSuccessfullyCreated = 1; // 시작방 포함

            // 목표 방 개수만큼 또는 더 이상 확장할 곳이 없을 때까지 반복
            for (int i = 1; i < numberOfRooms && frontier.Count > 0; i++)
            {
                Room expandFromRoom = frontier[_random.Next(frontier.Count)]; // 확장 기준 방을 무작위로 선택

                List<Point> directions = new List<Point> { new Point(0, -1), new Point(0, 1), new Point(-1, 0), new Point(1, 0) };
                directions = directions.OrderBy(x => _random.Next()).ToList(); // 시도할 방향 순서 섞기

                bool placedThisIteration = false;
                foreach (Point dirOffset in directions) // 각 방향으로 방 배치 시도
                {
                    Point nextGridPos = new Point(expandFromRoom.GridPosition.X + dirOffset.X, expandFromRoom.GridPosition.Y + dirOffset.Y);
                    if (!level.Rooms.ContainsKey(nextGridPos)) // 해당 위치에 아직 방이 없다면
                    {
                        // 새 방 타입 결정
                        RoomType newRoomType = RoomType.Normal;
                        if (roomsSuccessfullyCreated == numberOfRooms - 1) newRoomType = RoomType.Boss;
                        else if (_random.NextDouble() < 0.08 && roomsSuccessfullyCreated > 2 && numberOfRooms > 5) newRoomType = RoomType.Survival;
                        else if (_random.NextDouble() < 0.10 && roomsSuccessfullyCreated > 1 && numberOfRooms > 4) newRoomType = RoomType.Puzzle;
                        else if (_random.NextDouble() < 0.15 && roomsSuccessfullyCreated > 2) newRoomType = RoomType.Treasure;
                        else if (_random.NextDouble() < 0.10 && roomsSuccessfullyCreated > 3) newRoomType = RoomType.Shop;
                        else if (_random.NextDouble() < 0.10 && roomsSuccessfullyCreated > 2 && numberOfRooms > 6) newRoomType = RoomType.MiniBoss;
                        // TODO: 비밀방은 더 복잡한 조건 (예: 2~3개의 방으로 둘러싸인 빈 공간)으로 배치

                        Room newRoom = new Room(nextGridPos, newRoomType, _roomPixelSizeToUse); // 새 방 생성

                        // 퍼즐방 또는 생존방 특정 설정
                        if (newRoom.Type == RoomType.Puzzle)
                        {
                            // 실제 퍼즐 내용(질문, 정답) 및 제한 시간 설정
                            int num1 = _random.Next(1, 10); int num2 = _random.Next(1, 10);
                            newRoom.PuzzleQuestion = $"Question: {num1} + {num2} = ?";  // 질문 설정
                            newRoom.PuzzleAnswer = (num1 + num2).ToString();            // 정답 설정
                            newRoom.PuzzleTimeLimit = (float)_random.Next(20, 41);      // 제한 시간 설정 (20~40초)
                            // Debug.WriteLine($"[LevelGenerator] PuzzleRoom at {newRoom.GridPosition}. Q: {newRoom.PuzzleQuestion},
                            // A: {newRoom.PuzzleAnswer}, Limit: {newRoom.PuzzleTimeLimit}s");
                        }
                        else if (newRoom.Type == RoomType.Survival)
                        {
                            newRoom.SurvivalTimeDuration = 30.0f; // 30초 고정 (또는 랜덤)
                            // Debug.WriteLine($"[LevelGenerator] SurvivalRoom at {newRoom.GridPosition}. Duration: {newRoom.SurvivalTimeDuration}s");
                        }

                        level.AddRoom(newRoom);
                        ConnectRooms(expandFromRoom, newRoom, dirOffset);
                        frontier.Add(newRoom); // 새로 생성된 방도 다음 확장의 후보가 됨
                        roomsSuccessfullyCreated++;
                        placedThisIteration = true;
                        // Debug.WriteLine($"[LevelGenerator] Placed {newRoom.Type} at {newRoom.GridPosition} from {expandFromRoom.GridPosition}. Total rooms: {roomsSuccessfullyCreated}");
                        break; // 이번 방은 배치했으므로 다음 방 생성으로 (for loop의 다음 i)
                    }
                }

                if (!placedThisIteration) // 이번 반복에서 어떤 방향으로도 방을 배치하지 못했다면
                {
                    frontier.Remove(expandFromRoom); // 현재 확장 기준 방은 더 이상 확장 불가로 간주하고 목록에서 제거
                    i--; // 방을 만들지 못했으므로, 목표 개수를 채우기 위해 이번 반복을 다시 시도 (루프 카운터 복원)
                    Debug.WriteLine($"[LevelGenerator] Failed to expand from {expandFromRoom.GridPosition}. Removed from frontier. Retrying for room count.");
                }
            }

            // 각 방에 적 배치
            foreach (var room in level.Rooms.Values)
            {
                // 전투가 필요한 방 타입이면서, 아직 클리어되지 않은(또는 특수 조건 만족) 방에만 적 배치
                bool needsEnemies = room.Type == RoomType.Normal ||
                                    room.Type == RoomType.MiniBoss ||
                                    room.Type == RoomType.Boss ||
                                   (room.Type == RoomType.Puzzle && !room.IsPuzzleSolved) || // 퍼즐방은 해결 전이면 적 스폰 (선택적)
                                   (room.Type == RoomType.Survival && !room.IsSurvivalCompleted); // 생존방은 완료 전이면 적 스폰

                if (needsEnemies)
                {
                    int enemyCount = 1; // 기본 1마리
                    if (room.Type == RoomType.Normal) enemyCount = _random.Next(1, 4); // 1~3
                    else if (room.Type == RoomType.MiniBoss) enemyCount = _random.Next(2, 4); // 2~3
                    else if (room.Type == RoomType.Puzzle) enemyCount = _random.Next(1, 3); // 1~2 (퍼즐 풀기 전 방해용)
                    else if (room.Type == RoomType.Survival) enemyCount = _random.Next(3, 6); // 3~5 (생존방은 더 많게)
                    else if (room.Type == RoomType.Boss) enemyCount = 1; // 보스는 보통 1마리 (강력한)

                    for (int k = 0; k < enemyCount; k++)
                    {
                        // 적 스폰 위치를 방의 중앙 60% 영역 내로 제한하여 벽에 끼는 것 방지
                        float spawnMarginX = _roomPixelSizeToUse.Width * 0.2f;
                        float spawnMarginY = _roomPixelSizeToUse.Height * 0.2f;
                        float spawnX = (float)(_random.NextDouble() * (_roomPixelSizeToUse.Width - 2 * spawnMarginX) + spawnMarginX);
                        float spawnY = (float)(_random.NextDouble() * (_roomPixelSizeToUse.Height - 2 * spawnMarginY) + spawnMarginY);

                        // 현재는 BasicEnemy만 스폰하지만, 추후 다양한 적 타입을 스폰하도록 확장 가능
                        room.AddEnemySpawn(typeof(BasicEnemy), new PointF(spawnX, spawnY));
                    }
                    // Debug.WriteLine($"[LevelGenerator] Added {enemyCount} BasicEnemy to {room.Type} Room {room.GridPosition}");
                }
            }
            Debug.WriteLine($"[LevelGenerator] Generation finished. Total rooms in level: {level.Rooms.Count} (Target: {numberOfRooms}). CurrentRoom set to: {level.CurrentRoom?.GridPosition}");
            return level;
        }

        /// <summary>
        /// 두 방 사이에 문을 연결합니다.
        /// </summary>
        private void ConnectRooms(Room room1, Room room2, Point directionOffset)
        {
            if (directionOffset.X == 1) { room1.HasRightDoor = true; room2.HasLeftDoor = true; } // room2가 room1의 오른쪽에
            else if (directionOffset.X == -1) { room1.HasLeftDoor = true; room2.HasRightDoor = true; } // room2가 room1의 왼쪽에
            else if (directionOffset.Y == 1) { room1.HasBottomDoor = true; room2.HasTopDoor = true; } // room2가 room1의 아래에
            else if (directionOffset.Y == -1) { room1.HasTopDoor = true; room2.HasBottomDoor = true; } // room2가 room1의 위에
        }
    }
}