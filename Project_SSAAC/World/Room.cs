// 파일 위치: Project_SSAAC/World/Room.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects;
using System.Diagnostics;

namespace Project_SSAAC.World
{
    public enum RoomType
    {
        Start,
        Normal,
        Boss,
        Treasure,
        Shop,
        Secret,
        MiniBoss,
        Puzzle,
        Survival
    }

    public class Room
    {
        public Point GridPosition { get; private set; }
        public RoomType Type { get; set; }
        public bool IsCleared { get; set; }
        public bool IsDiscovered { get; set; }

        public bool HasTopDoor { get; set; }
        public bool HasBottomDoor { get; set; }
        public bool HasLeftDoor { get; set; }
        public bool HasRightDoor { get; set; }

        public List<PointF> EnemySpawnPositions { get; private set; }
        public List<Type> EnemyTypesToSpawn { get; private set; }
        public List<Obstacle> Obstacles { get; private set; }

        public RectangleF Bounds { get; private set; }
        private SizeF _roomLocalPixelSize;

        public string PuzzleQuestion { get; set; }
        public string PuzzleAnswer { get; set; }
        public bool IsPuzzleSolved { get; set; }
        public bool IsPuzzleActive { get; set; }
        public float PuzzleTimeLimit { get; set; } = 30.0f;

        public float SurvivalTimeDuration { get; set; } = 30.0f;
        public bool IsSurvivalActive { get; set; }
        public bool IsSurvivalCompleted { get; set; }

        // <<-- 새로 추가된 속성: 방이 잠겼는지 여부를 결정 -->>
        /// <summary>
        /// 현재 방이 잠겨있는지 여부를 반환합니다.
        /// 전투, 퍼즐, 생존 방의 클리어 조건에 따라 결정됩니다.
        /// </summary>
        public bool IsSealed
        {
            get
            {
                switch (Type)
                {
                    // 전투가 필요한 방들은 클리어되지 않았다면 잠깁니다.
                    case RoomType.Normal:
                    case RoomType.Boss:
                    case RoomType.MiniBoss:
                        return !IsCleared;

                    // 퍼즐 방은 해결되지 않았다면 잠깁니다.
                    case RoomType.Puzzle:
                        return !IsPuzzleSolved;

                    // 생존 방은 완료되지 않았다면 잠깁니다.
                    case RoomType.Survival:
                        return !IsSurvivalCompleted;

                    // 그 외 시작방, 상점 등은 절대 잠기지 않습니다.
                    default:
                        return false;
                }
            }
        }

        public Room(Point gridPosition, RoomType type, SizeF roomPixelSize)
        {
            if (roomPixelSize.IsEmpty || roomPixelSize.Width <= 0 || roomPixelSize.Height <= 0)
            {
                Debug.WriteLine($"[Room] Constructor CRITICAL: Invalid roomPixelSize {roomPixelSize} for room at {gridPosition}. Defaulting to 1024x576 (this should match Form's ClientSize).");
                _roomLocalPixelSize = new SizeF(1024, 576);
            }
            else
            {
                _roomLocalPixelSize = roomPixelSize;
            }
            GridPosition = gridPosition;
            Type = type;
            Bounds = new RectangleF(0, 0, _roomLocalPixelSize.Width, _roomLocalPixelSize.Height);

            IsCleared = false;
            IsDiscovered = false;
            HasTopDoor = false; HasBottomDoor = false; HasLeftDoor = false; HasRightDoor = false;
            EnemySpawnPositions = new List<PointF>();
            EnemyTypesToSpawn = new List<Type>();
            Obstacles = new List<Obstacle>();

            IsPuzzleSolved = false;
            IsPuzzleActive = false;
            PuzzleQuestion = "N/A";
            PuzzleAnswer = "";
            IsSurvivalActive = false;
            IsSurvivalCompleted = false;

            if (Type == RoomType.Start || Type == RoomType.Shop || Type == RoomType.Treasure || Type == RoomType.Secret)
            {
                IsCleared = true;
            }
            Debug.WriteLine($"[Room] Created {Type} room at {GridPosition}, Size: {_roomLocalPixelSize}, Cleared: {IsCleared}");
        }

        public void AddEnemySpawn(Type enemyType, PointF spawnPosition)
        {
            EnemyTypesToSpawn.Add(enemyType);
            EnemySpawnPositions.Add(spawnPosition);
        }

        public void ClearRoom()
        {
            IsCleared = true;
            if (Type == RoomType.Puzzle) IsPuzzleActive = false;
            if (Type == RoomType.Survival) IsSurvivalActive = false;
            Debug.WriteLine($"[Room {GridPosition} ({Type})] Room has been CLEARED. IsCleared: {IsCleared}");
        }

        public void Draw(Graphics g, Rectangle clientBounds)
        {
            //Brush backgroundBrush;
            TextureBrush backgroundBrush2 = new TextureBrush(Properties.Resources.resized_dungeon_1024x576);
            /*switch (Type)
            {
                case RoomType.Boss: backgroundBrush = Brushes.DarkSlateGray; break;
                case RoomType.Treasure: backgroundBrush = Brushes.LightGoldenrodYellow; break;
                case RoomType.Shop: backgroundBrush = Brushes.LightSteelBlue; break;
                case RoomType.Secret: backgroundBrush = Brushes.Lavender; break;
                case RoomType.MiniBoss: backgroundBrush = Brushes.DarkOrange; break;
                case RoomType.Puzzle: backgroundBrush = Brushes.LightSeaGreen; break;
                case RoomType.Survival: backgroundBrush = Brushes.IndianRed; break;
                default: backgroundBrush = Brushes.DimGray; break;
            }*/

            g.FillRectangle(backgroundBrush2, clientBounds);

            // <<-- 수정된 부분: 문 개폐 여부를 IsSealed 속성으로 판단 -->>
            bool doorsShouldBeOpen = !this.IsSealed;

            float doorWidth = 60, doorHeight = 60;
            Brush doorBrushClosed = Brushes.SaddleBrown;
            Brush doorBrushOpen = Brushes.DarkOliveGreen;
            Brush currentDoorBrush = doorsShouldBeOpen ? doorBrushOpen : doorBrushClosed;

            float doorVisualThickness = Math.Min(doorWidth, doorHeight) / 2.5f;

            if (HasTopDoor) { g.FillRectangle(currentDoorBrush, clientBounds.Width / 2f - doorWidth / 2f, 0, doorWidth, doorVisualThickness); g.DrawRectangle(Pens.Black, clientBounds.Width / 2f - doorWidth / 2f, 0, doorWidth, doorVisualThickness); }
            if (HasBottomDoor) { g.FillRectangle(currentDoorBrush, clientBounds.Width / 2f - doorWidth / 2f, clientBounds.Height - doorVisualThickness, doorWidth, doorVisualThickness); g.DrawRectangle(Pens.Black, clientBounds.Width / 2f - doorWidth / 2f, clientBounds.Height - doorVisualThickness, doorWidth, doorVisualThickness); }
            if (HasLeftDoor) { g.FillRectangle(currentDoorBrush, 0, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); g.DrawRectangle(Pens.Black, 0, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); }
            if (HasRightDoor) { g.FillRectangle(currentDoorBrush, clientBounds.Width - doorVisualThickness, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); g.DrawRectangle(Pens.Black, clientBounds.Width - doorVisualThickness, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); }
        }
    }
}