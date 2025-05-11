using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects;
using System.Diagnostics;

namespace Project_SSAAC.World
{
    /// <summary>
    /// 한 층(스테이지)의 모든 방과 현재 방 정보를 관리하는 클래스입니다.
    /// </summary>
    public class Level
    {
        /// <summary>
        /// 현재 레벨에 포함된 모든 방의 컬렉션입니다. (Key: 그리드 좌표, Value: Room 객체)
        /// </summary>
        public Dictionary<Point, Room> Rooms { get; private set; }

        /// <summary>
        /// 플레이어가 현재 위치한 방입니다.
        /// </summary>
        public Room CurrentRoom { get; private set; }

        /// <summary>
        /// 시작 방의 그리드 좌표입니다.
        /// </summary>
        public Point StartRoomGridPosition { get; private set; }

        /// <summary>
        /// 이 레벨의 각 방들이 그려질 실제 픽셀 크기입니다 (Form의 ClientSize와 동일).
        /// </summary>
        private SizeF _actualRoomPixelSize;

        /// <summary>
        /// Level 객체를 초기화합니다.
        /// </summary>
        /// <param name="actualRoomPixelSize">각 방의 실제 픽셀 크기 (Form의 ClientSize) 입니다.</param>
        public Level(SizeF actualRoomPixelSize)
        {
            Rooms = new Dictionary<Point, Room>();
            _actualRoomPixelSize = actualRoomPixelSize; // Form1으로부터 전달받은 ClientSize
            Debug.WriteLine($"[Level] Created with actualRoomPixelSize: {_actualRoomPixelSize}");
        }

        /// <summary>
        /// 레벨에 새로운 방을 추가합니다. 시작방일 경우 CurrentRoom으로 설정합니다.
        /// </summary>
        /// <param name="room">추가할 Room 객체입니다.</param>
        public void AddRoom(Room room)
        {
            if (room == null) { Debug.WriteLine("[Level] AddRoom Error: Attempted to add a null room."); return; }
            if (!Rooms.ContainsKey(room.GridPosition))
            {
                Rooms.Add(room.GridPosition, room);
                // Debug.WriteLine($"[Level] Added {room.Type} room at {room.GridPosition}. Total rooms: {Rooms.Count}");
                if (room.Type == RoomType.Start && CurrentRoom == null) // 첫 번째 시작방만 CurrentRoom으로 설정
                {
                    CurrentRoom = room;
                    CurrentRoom.IsDiscovered = true; // 시작방은 처음부터 발견된 상태
                    StartRoomGridPosition = room.GridPosition;
                    Debug.WriteLine($"[Level] Set StartRoom {CurrentRoom.GridPosition} as CurrentRoom. IsDiscovered: {CurrentRoom.IsDiscovered}");
                }
            }
            else
            {
                Debug.WriteLine($"[Level] AddRoom Warning: Room already exists at {room.GridPosition}. Not adding.");
            }
        }

        /// <summary>
        /// (비상용) CurrentRoom을 강제로 설정합니다. LevelGenerator에서 시작방 설정 실패 시 사용될 수 있습니다.
        /// </summary>
        public void ForceSetCurrentRoom(Room room)
        {
            if (room == null) { Debug.WriteLine("[Level] ForceSetCurrentRoom Error: room was null."); return; }
            CurrentRoom = room;
            if (!CurrentRoom.IsDiscovered) CurrentRoom.IsDiscovered = true; // 강제 설정 시에도 발견 처리
            if (room.Type == RoomType.Start && StartRoomGridPosition == Point.Empty) StartRoomGridPosition = room.GridPosition;
            Debug.WriteLine($"[Level] ForceSet CurrentRoom to {room.GridPosition} ({room.Type}). IsDiscovered: {CurrentRoom.IsDiscovered}");
        }

        /// <summary>
        /// 플레이어가 현재 방에서 지정된 방향으로 이동을 시도합니다.
        /// </summary>
        /// <param name="currentGridPos">플레이어의 현재 방 그리드 좌표입니다.</param>
        /// <param name="dx">X축 이동 오프셋 (-1, 0, 또는 1)입니다.</param>
        /// <param name="dy">Y축 이동 오프셋 (-1, 0, 또는 1)입니다.</param>
        /// <param name="player">플레이어 객체 (위치 조정을 위해 필요)입니다.</param>
        /// <returns>이동에 성공하면 true, 아니면 false입니다.</returns>
        public bool TryMoveToRoom(Point currentGridPos, int dx, int dy, Player player)
        {
            if (CurrentRoom == null) { Debug.WriteLine("[Level] TryMoveToRoom Error: CurrentRoom is null."); return false; }

            // 특정 방 타입은 클리어해야만 나갈 수 있음
            bool requiresClearToExit = CurrentRoom.Type == RoomType.Normal ||
                                       CurrentRoom.Type == RoomType.Boss ||
                                       CurrentRoom.Type == RoomType.MiniBoss ||
                                       CurrentRoom.Type == RoomType.Puzzle ||
                                       CurrentRoom.Type == RoomType.Survival;
            if (requiresClearToExit && !CurrentRoom.IsCleared)
            {
                Debug.WriteLine($"[Level] Cannot leave room {CurrentRoom.GridPosition}({CurrentRoom.Type}): Not cleared (IsCleared: {CurrentRoom.IsCleared})");
                return false;
            }

            Point nextGridPos = new Point(currentGridPos.X + dx, currentGridPos.Y + dy);
            if (Rooms.TryGetValue(nextGridPos, out Room nextRoom))
            {
                bool canMove = false;
                // 문 연결 상태 확인
                if (dx == 1 && CurrentRoom.HasRightDoor && nextRoom.HasLeftDoor) canMove = true;
                else if (dx == -1 && CurrentRoom.HasLeftDoor && nextRoom.HasRightDoor) canMove = true;
                else if (dy == 1 && CurrentRoom.HasBottomDoor && nextRoom.HasTopDoor) canMove = true;
                else if (dy == -1 && CurrentRoom.HasTopDoor && nextRoom.HasBottomDoor) canMove = true;

                if (canMove)
                {
                    Room previousRoom = CurrentRoom; // Form1에서 이전 방 상태 관리를 위해 필요할 수 있음
                    CurrentRoom = nextRoom;
                    if (!CurrentRoom.IsDiscovered) CurrentRoom.IsDiscovered = true;

                    Debug.WriteLine($"[Level] Moved from {previousRoom.GridPosition}({previousRoom.Type}) to Room: {CurrentRoom.GridPosition}({CurrentRoom.Type}). Discovered: {CurrentRoom.IsDiscovered}");

                    // 플레이어 위치를 새 방의 문 반대편으로 설정
                    float margin = player.Size.Width * 0.8f; // 문 통과 후 여백
                    float halfPlayerWidth = player.Size.Width / 2f;
                    float halfPlayerHeight = player.Size.Height / 2f;
                    // _actualRoomPixelSize는 Level 생성 시 Form1.ClientSize로 설정된 값
                    float halfRoomWidth = _actualRoomPixelSize.Width / 2f;
                    float halfRoomHeight = _actualRoomPixelSize.Height / 2f;

                    if (dx == 1) player.SetPosition(new PointF(margin, halfRoomHeight - halfPlayerHeight)); // 오른쪽 문으로 진입 -> 왼쪽에서 등장
                    else if (dx == -1) player.SetPosition(new PointF(_actualRoomPixelSize.Width - player.Size.Width - margin, halfRoomHeight - halfPlayerHeight)); // 왼쪽 문으로 진입 -> 오른쪽에서 등장
                    else if (dy == 1) player.SetPosition(new PointF(halfRoomWidth - halfPlayerWidth, margin)); // 아래쪽 문으로 진입 -> 위쪽에서 등장
                    else if (dy == -1) player.SetPosition(new PointF(halfRoomWidth - halfPlayerWidth, _actualRoomPixelSize.Height - player.Size.Height - margin)); // 위쪽 문으로 진입 -> 아래쪽에서 등장
                    return true;
                }
                Debug.WriteLine($"[Level] Cannot move from {CurrentRoom.GridPosition} to {nextGridPos}: Doors not aligned.");
            }
            else { Debug.WriteLine($"[Level] Cannot move from {CurrentRoom.GridPosition} to {nextGridPos}: Next room does not exist."); }
            return false;
        }

        /// <summary>
        /// 현재 방의 내용을 그립니다. CurrentRoom이 null이면 오류 메시지를 표시합니다.
        /// </summary>
        /// <param name="g">Graphics 객체입니다.</param>
        /// <param name="clientBounds">방이 그려질 전체 화면 영역 (Form의 ClientRectangle)입니다.</param>
        public void DrawCurrentRoom(Graphics g, Rectangle clientBounds)
        {
            if (CurrentRoom == null)
            {
                g.Clear(Color.FromArgb(50, 0, 0)); // 매우 어두운 빨간색 배경
                Font errorFont = new Font("Arial", 14, FontStyle.Bold);
                string errorMsg = "CRITICAL ERROR:\nCURRENT ROOM IS NULL.\nLevel data might be corrupted or generation failed.";
                SizeF msgSize = g.MeasureString(errorMsg, errorFont);
                g.DrawString(errorMsg, errorFont, Brushes.White,
                             clientBounds.Width / 2f - msgSize.Width / 2f,
                             clientBounds.Height / 2f - msgSize.Height / 2f);
                Debug.WriteLine("[Level] DrawCurrentRoom CRITICAL: CurrentRoom IS NULL! Cannot draw room.");
                return;
            }
            // Room의 Draw 메서드는 clientBounds를 받아 전체 영역에 자신을 그림
            CurrentRoom.Draw(g, clientBounds);
        }
    }
}