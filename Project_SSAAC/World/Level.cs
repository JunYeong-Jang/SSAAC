// 파일 위치: Project_SSAAC/World/Level.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects;
using System.Diagnostics;

namespace Project_SSAAC.World
{
    public class Level
    {
        public Dictionary<Point, Room> Rooms { get; private set; }
        public Room CurrentRoom { get; private set; }
        public Point StartRoomGridPosition { get; private set; }
        private SizeF _actualRoomPixelSize;

        public Level(SizeF actualRoomPixelSize)
        {
            Rooms = new Dictionary<Point, Room>();
            _actualRoomPixelSize = actualRoomPixelSize;
            Debug.WriteLine($"[Level] Created with actualRoomPixelSize: {_actualRoomPixelSize}");
        }

        public void AddRoom(Room room)
        {
            if (room == null) { Debug.WriteLine("[Level] AddRoom Error: Attempted to add a null room."); return; }
            if (!Rooms.ContainsKey(room.GridPosition))
            {
                Rooms.Add(room.GridPosition, room);
                if (room.Type == RoomType.Start && CurrentRoom == null)
                {
                    CurrentRoom = room;
                    CurrentRoom.IsDiscovered = true;
                    StartRoomGridPosition = room.GridPosition;
                    Debug.WriteLine($"[Level] Set StartRoom {CurrentRoom.GridPosition} as CurrentRoom. IsDiscovered: {CurrentRoom.IsDiscovered}");
                }
            }
            else
            {
                Debug.WriteLine($"[Level] AddRoom Warning: Room already exists at {room.GridPosition}. Not adding.");
            }
        }

        public void ForceSetCurrentRoom(Room room)
        {
            if (room == null) { Debug.WriteLine("[Level] ForceSetCurrentRoom Error: room was null."); return; }
            CurrentRoom = room;
            if (!CurrentRoom.IsDiscovered) CurrentRoom.IsDiscovered = true;
            if (room.Type == RoomType.Start && StartRoomGridPosition == Point.Empty) StartRoomGridPosition = room.GridPosition;
            Debug.WriteLine($"[Level] ForceSet CurrentRoom to {room.GridPosition} ({room.Type}). IsDiscovered: {CurrentRoom.IsDiscovered}");
        }

        public bool TryMoveToRoom(Point currentGridPos, int dx, int dy, Player player)
        {
            if (CurrentRoom == null) { Debug.WriteLine("[Level] TryMoveToRoom Error: CurrentRoom is null."); return false; }

            // <<-- 수정된 부분: 방이 잠겨있는지 IsSealed 속성으로 한 번에 확인 -->>
            if (CurrentRoom.IsSealed)
            {
                Debug.WriteLine($"[Level] Cannot leave room {CurrentRoom.GridPosition}({CurrentRoom.Type}): Room is sealed.");
                return false;
            }

            Point nextGridPos = new Point(currentGridPos.X + dx, currentGridPos.Y + dy);
            if (Rooms.TryGetValue(nextGridPos, out Room nextRoom))
            {
                bool canMove = false;
                if (dx == 1 && CurrentRoom.HasRightDoor && nextRoom.HasLeftDoor) canMove = true;
                else if (dx == -1 && CurrentRoom.HasLeftDoor && nextRoom.HasRightDoor) canMove = true;
                else if (dy == 1 && CurrentRoom.HasBottomDoor && nextRoom.HasTopDoor) canMove = true;
                else if (dy == -1 && CurrentRoom.HasTopDoor && nextRoom.HasBottomDoor) canMove = true;

                if (canMove)
                {
                    Room previousRoom = CurrentRoom;
                    CurrentRoom = nextRoom;
                    if (!CurrentRoom.IsDiscovered) CurrentRoom.IsDiscovered = true;

                    Debug.WriteLine($"[Level] Moved from {previousRoom.GridPosition}({previousRoom.Type}) to Room: {CurrentRoom.GridPosition}({CurrentRoom.Type}). Discovered: {CurrentRoom.IsDiscovered}");

                    float margin = player.Size.Width * 1.5f;
                    float halfPlayerWidth = player.Size.Width / 2f;
                    float halfPlayerHeight = player.Size.Height / 2f;
                    float halfRoomWidth = _actualRoomPixelSize.Width / 2f;
                    float halfRoomHeight = _actualRoomPixelSize.Height / 2f;

                    if (dx == 1) player.SetPosition(new PointF(margin, halfRoomHeight - halfPlayerHeight));
                    else if (dx == -1) player.SetPosition(new PointF(_actualRoomPixelSize.Width - player.Size.Width - margin, halfRoomHeight - halfPlayerHeight));
                    else if (dy == 1) player.SetPosition(new PointF(halfRoomWidth - halfPlayerWidth, margin));
                    else if (dy == -1) player.SetPosition(new PointF(halfRoomWidth - halfPlayerWidth, _actualRoomPixelSize.Height - player.Size.Height - margin));
                    return true;
                }
                Debug.WriteLine($"[Level] Cannot move from {CurrentRoom.GridPosition} to {nextGridPos}: Doors not aligned.");
            }
            else { Debug.WriteLine($"[Level] Cannot move from {CurrentRoom.GridPosition} to {nextGridPos}: Next room does not exist."); }
            return false;
        }

        public void DrawCurrentRoom(Graphics g, Rectangle clientBounds)
        {
            if (CurrentRoom == null)
            {
                g.Clear(Color.FromArgb(50, 0, 0));
                Font errorFont = new Font("Arial", 14, FontStyle.Bold);
                string errorMsg = "CRITICAL ERROR:\nCURRENT ROOM IS NULL.\nLevel data might be corrupted or generation failed.";
                SizeF msgSize = g.MeasureString(errorMsg, errorFont);
                g.DrawString(errorMsg, errorFont, Brushes.White,
                             clientBounds.Width / 2f - msgSize.Width / 2f,
                             clientBounds.Height / 2f - msgSize.Height / 2f);
                Debug.WriteLine("[Level] DrawCurrentRoom CRITICAL: CurrentRoom IS NULL! Cannot draw room.");
                return;
            }
            CurrentRoom.Draw(g, clientBounds);
        }
    }
}