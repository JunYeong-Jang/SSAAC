// 파일 위치: Project_SSAAC/GameObjects/Obstacle.cs
using System.Drawing;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 장애물의 종류를 정의하는 열거형입니다.
    /// </summary>
    public enum ObstacleType
    {
        Rock,   // 바위
        Spikes, // 가시
        Pit     // 구덩이
    }

    /// <summary>
    /// 바위, 가시 등 모든 정적 장애물을 나타내는 범용 클래스입니다.
    /// </summary>
    public class Obstacle : GameObject
    {
        public ObstacleType Type { get; private set; }
        public bool BlocksMovement { get; private set; }   // 이동을 막는지 여부
        public bool BlocksProjectiles { get; private set; } // 발사체를 막는지 여부
        public int CollisionDamage { get; private set; }    // 충돌 시 주는 대미지

        // 각 장애물 타입의 외형을 저장하는 정적 변수
        private static readonly Bitmap RockAppearance = Properties.Resources.rock;
        private static readonly Bitmap SpikesAppearance = Properties.Resources.spikes;
        private static readonly Bitmap PitAppearance = Properties.Resources.pit; // 구덩이 이미지 리소스 추가

        /// <summary>
        /// Obstacle 객체를 초기화합니다.
        /// </summary>
        public Obstacle(PointF position, SizeF size, ObstacleType type)
            : base(position, size)
        {
            this.Type = type;

            // 타입에 따라 속성을 다르게 설정합니다.
            switch (type)
            {
                case ObstacleType.Rock:
                    BlocksMovement = true;
                    BlocksProjectiles = true;
                    CollisionDamage = 0;
                    break;

                case ObstacleType.Spikes:
                    BlocksMovement = false;
                    BlocksProjectiles = false;
                    CollisionDamage = 1;
                    break;

                case ObstacleType.Pit: // 구덩이 속성 추가
                    BlocksMovement = true;       // 이동을 막음
                    BlocksProjectiles = false;   // 발사체는 통과
                    CollisionDamage = 0;
                    break;
            }
        }

        /// <summary>
        /// 정적인 장애물이므로 업데이트 로직은 비워둡니다.
        /// </summary>
        public override void Update(float deltaTime)
        {
            // 내용 없음
        }

        /// <summary>
        /// 장애물을 화면에 그립니다.
        /// </summary>
        public override void Draw(Graphics g)
        {
            Bitmap imageToDraw = null;
            switch (Type)
            {
                case ObstacleType.Rock:
                    imageToDraw = RockAppearance;
                    break;
                case ObstacleType.Spikes:
                    imageToDraw = SpikesAppearance;
                    break;
                case ObstacleType.Pit:
                    imageToDraw = PitAppearance;
                    break;
            }

            if (imageToDraw != null)
            {
                g.DrawImage(imageToDraw, this.Bounds);
            }
            else // 이미지가 없을 경우 회색 사각형으로 표시
            {
                g.FillRectangle(Brushes.Gray, this.Bounds);
            }
        }
    }
}