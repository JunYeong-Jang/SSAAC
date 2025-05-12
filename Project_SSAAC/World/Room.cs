using System;
using System.Collections.Generic;
using System.Drawing;
using Project_SSAAC.GameObjects; // EnemyType 저장 시 필요
using System.Diagnostics;

namespace Project_SSAAC.World
{
    /// <summary>
    /// 방의 유형을 정의하는 열거형입니다.
    /// </summary>
    public enum RoomType
    {
        Start,    // 시작방
        Normal,   // 일반 전투방
        Boss,     // 보스방
        Treasure, // 보물방
        Shop,     // 상점방
        Secret,   // 비밀방
        MiniBoss, // 미니보스방
        Puzzle,   // 퍼즐방
        Survival  // 생존방
    }

    /// <summary>
    /// 게임 내 개별 방을 나타내는 클래스입니다.
    /// </summary>
    public class Room
    {
        public Point GridPosition { get; private set; }
        public RoomType Type { get; set; }
        public bool IsCleared { get; set; }     // 방이 클리어되었는지 (적이 없거나, 퍼즐/생존 완료)
        public bool IsDiscovered { get; set; }  // 미니맵 표시를 위해 플레이어가 방문했는지

        // 문 존재 여부 플래그
        public bool HasTopDoor { get; set; }
        public bool HasBottomDoor { get; set; }
        public bool HasLeftDoor { get; set; }
        public bool HasRightDoor { get; set; }

        public List<PointF> EnemySpawnPositions { get; private set; }
        public List<Type> EnemyTypesToSpawn { get; private set; } // 스폰될 적의 Type 정보

        public RectangleF Bounds { get; private set; } // 방의 로컬 경계 (0,0 에서 roomPixelSize 만큼)
        private SizeF _roomLocalPixelSize; // 이 방 객체가 생성될 때의 기준 크기 (주로 Form.ClientSize)

        // 퍼즐방 관련 속성
        public string PuzzleQuestion { get; set; }
        public string PuzzleAnswer { get; set; }
        public bool IsPuzzleSolved { get; set; }   // 이 방의 퍼즐이 해결되었는지
        public bool IsPuzzleActive { get; set; }   // 현재 이 방의 퍼즐이 활성화되어 플레이어가 풀어야 하는 상태인지

        /// <summary>
        /// 퍼즐방의 제한 시간 (초) 입니다.
        /// </summary>
        public float PuzzleTimeLimit { get; set; } = 30.0f; // 기본값 30초

        // 생존방 관련 속성
        /// <summary>
        /// 생존방의 생존 시간 (초) 입니다.
        /// </summary>
        public float SurvivalTimeDuration { get; set; } = 30.0f; // 기본값 30초
        public bool IsSurvivalActive { get; set; }      // 현재 이 방에서 생존 모드가 진행 중인지
        public bool IsSurvivalCompleted { get; set; }   // 이 방의 생존 모드가 완료되었는지

        /// <summary>
        /// Room 객체를 초기화합니다.
        /// </summary>
        /// <param name="gridPosition">레벨 내 그리드 좌표입니다.</param>
        /// <param name="type">방의 유형입니다.</param>
        /// <param name="roomPixelSize">방의 실제 픽셀 크기 (보통 Form의 ClientSize) 입니다.</param>
        public Room(Point gridPosition, RoomType type, SizeF roomPixelSize)
        {
            if (roomPixelSize.IsEmpty || roomPixelSize.Width <= 0 || roomPixelSize.Height <= 0)
            {
                Debug.WriteLine($"[Room] Constructor CRITICAL: Invalid roomPixelSize {roomPixelSize} for room at {gridPosition}. Defaulting to 1024x576 (this should match Form's ClientSize).");
                _roomLocalPixelSize = new SizeF(1024, 576); // Form1의 ClientSize와 일치하는 값으로 fallback
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

            // 퍼즐방/생존방 관련 초기화
            IsPuzzleSolved = false;
            IsPuzzleActive = false;
            PuzzleQuestion = "N/A";
            PuzzleAnswer = "";
            IsSurvivalActive = false;
            IsSurvivalCompleted = false;

            // 전투가 없는 방은 기본적으로 클리어 상태로 시작
            if (Type == RoomType.Start || Type == RoomType.Shop || Type == RoomType.Treasure || Type == RoomType.Secret)
            {
                IsCleared = true;
            }
            Debug.WriteLine($"[Room] Created {Type} room at {GridPosition}, Size: {_roomLocalPixelSize}, Cleared: {IsCleared}");
        }

        /// <summary>
        /// 방에 적 스폰 정보를 추가합니다.
        /// </summary>
        /// <param name="enemyType">스폰될 적의 Type입니다.</param>
        /// <param name="spawnPosition">방 내부에서의 스폰 위치입니다.</param>
        public void AddEnemySpawn(Type enemyType, PointF spawnPosition)
        {
            EnemyTypesToSpawn.Add(enemyType);
            EnemySpawnPositions.Add(spawnPosition);
        }

        /// <summary>
        /// 방을 클리어 상태로 만듭니다. (예: 모든 적 제거, 퍼즐 해결, 생존 완료 시 호출)
        /// </summary>
        public void ClearRoom()
        {
            IsCleared = true;
            // 클리어 시 특별한 상태 변경이 필요하면 여기에 추가
            if (Type == RoomType.Puzzle) IsPuzzleActive = false; // 퍼즐이 풀리면 더 이상 활성 상태가 아님
            if (Type == RoomType.Survival) IsSurvivalActive = false; // 생존이 완료되면 활성 상태가 아님
            Debug.WriteLine($"[Room {GridPosition} ({Type})] Room has been CLEARED. IsCleared: {IsCleared}");
        }

        /// <summary>
        /// 방의 배경과 문 등을 그립니다.
        /// </summary>
        /// <param name="g">Graphics 객체입니다.</param>
        /// <param name="clientBounds">방이 그려질 전체 화면 영역 (Form의 ClientRectangle)입니다.</param>
        public void Draw(Graphics g, Rectangle clientBounds)
        {
            Brush backgroundBrush;
            switch (Type)
            {
                case RoomType.Boss: backgroundBrush = Brushes.DarkSlateGray; break;
                case RoomType.Treasure: backgroundBrush = Brushes.LightGoldenrodYellow; break;
                case RoomType.Shop: backgroundBrush = Brushes.LightSteelBlue; break;
                case RoomType.Secret: backgroundBrush = Brushes.Lavender; break;
                case RoomType.MiniBoss: backgroundBrush = Brushes.DarkOrange; break;
                case RoomType.Puzzle: backgroundBrush = Brushes.LightSeaGreen; break;
                case RoomType.Survival: backgroundBrush = Brushes.IndianRed; break; // 생존방 배경색 변경
                default: backgroundBrush = Brushes.DimGray; break;
            }
            g.FillRectangle(backgroundBrush, clientBounds); // 방 배경은 항상 clientBounds 전체에

            // 문 그리기 로직
            float doorWidth = 60, doorHeight = 60; // 문의 시각적 크기
            Brush doorBrushClosed = Brushes.SaddleBrown;
            Brush doorBrushOpen = Brushes.DarkOliveGreen;
            Brush currentDoorBrush;

            // 일반/보스/미니보스/퍼즐/생존 방은 IsCleared 상태에 따라 문의 개폐가 결정됨
            // 그 외 방(시작,상점,보물,비밀)은 IsCleared가 true로 시작하므로 항상 열린 상태로 보임
            bool doorsShouldBeOpen = IsCleared;
            if (Type == RoomType.Start || Type == RoomType.Shop || Type == RoomType.Treasure || Type == RoomType.Secret)
            {
                doorsShouldBeOpen = true; // 이 방들은 항상 문이 열린 것으로 간주 (IsCleared가 true이기도 함)
            }


            float doorVisualThickness = Math.Min(doorWidth, doorHeight) / 2.5f;

            if (HasTopDoor) { currentDoorBrush = doorsShouldBeOpen ? doorBrushOpen : doorBrushClosed; g.FillRectangle(currentDoorBrush, clientBounds.Width / 2f - doorWidth / 2f, 0, doorWidth, doorVisualThickness); g.DrawRectangle(Pens.Black, clientBounds.Width / 2f - doorWidth / 2f, 0, doorWidth, doorVisualThickness); }
            if (HasBottomDoor) { currentDoorBrush = doorsShouldBeOpen ? doorBrushOpen : doorBrushClosed; g.FillRectangle(currentDoorBrush, clientBounds.Width / 2f - doorWidth / 2f, clientBounds.Height - doorVisualThickness, doorWidth, doorVisualThickness); g.DrawRectangle(Pens.Black, clientBounds.Width / 2f - doorWidth / 2f, clientBounds.Height - doorVisualThickness, doorWidth, doorVisualThickness); }
            if (HasLeftDoor) { currentDoorBrush = doorsShouldBeOpen ? doorBrushOpen : doorBrushClosed; g.FillRectangle(currentDoorBrush, 0, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); g.DrawRectangle(Pens.Black, 0, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); }
            if (HasRightDoor) { currentDoorBrush = doorsShouldBeOpen ? doorBrushOpen : doorBrushClosed; g.FillRectangle(currentDoorBrush, clientBounds.Width - doorVisualThickness, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); g.DrawRectangle(Pens.Black, clientBounds.Width - doorVisualThickness, clientBounds.Height / 2f - doorHeight / 2f, doorVisualThickness, doorHeight); }
        }
    }
}