using System;
using System.Drawing;
using System.Diagnostics;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 플레이어 캐릭터를 나타내는 클래스입니다. 이동, 공격, 체력 관리 등의 기능을 가집니다.
    /// </summary>
    public class Player : GameObject
    {
        /// <summary>
        /// 플레이어의 이동 속도 (초당 픽셀)입니다.
        /// </summary>
        public float Speed { get; set; } = 150f;

        /// <summary>
        /// 플레이어의 현재 속도 벡터 (방향과 크기)입니다. Form1에서 주로 설정됩니다.
        /// </summary>
        public PointF Velocity { get; set; } = PointF.Empty;

        /// <summary>
        /// 플레이어의 공격 대미지입니다.
        /// </summary>
        public int Damage { get; private set; } = 3;

        /// <summary>
        /// 플레이어가 발사하는 투사체의 속도입니다.
        /// </summary>
        public float TearSpeed { get; private set; } = 300f;

        /// <summary>
        /// 플레이어가 발사하는 투사체의 크기입니다.
        /// </summary>
        public SizeF TearSize { get; private set; } = new SizeF(10, 10);

        /// <summary>
        /// 플레이어의 최대 체력입니다.
        /// </summary>
        public int MaxHealth { get; private set; } = 6; // 예: 하트 3개

        /// <summary>
        /// 플레이어의 현재 체력입니다.
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// 플레이어의 기본 크기입니다.
        /// </summary>
        private static readonly SizeF PlayerDefaultSize = new SizeF(32, 32);

        /// <summary>
        /// Player 객체를 초기화합니다.
        /// </summary>
        /// <param name="startPos">플레이어의 시작 위치입니다.</param>
        public Player(PointF startPos)
            : base(startPos, PlayerDefaultSize)
        {
            CurrentHealth = MaxHealth;
            Debug.WriteLine($"[Player] Created at {startPos}. Health: {CurrentHealth}/{MaxHealth}");
        }

        /// <summary>
        /// 플레이어의 상태를 업데이트합니다. 주로 Velocity에 따라 위치를 이동시킵니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 경과 시간입니다.</param>
        public override void Update(float deltaTime)
        {
            Position = new PointF(
                Position.X + Velocity.X * deltaTime,
                Position.Y + Velocity.Y * deltaTime
            );
        }

        /// <summary>
        /// 플레이어를 화면에 그립니다.
        /// </summary>
        /// <param name="g">Graphics 객체입니다.</param>
        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, Bounds); // 임시로 파란 사각형으로 표시
        }

        /// <summary>
        /// 지정된 방향으로 투사체를 발사합니다.
        /// </summary>
        /// <param name="direction">발사 방향 벡터입니다.</param>
        /// <returns>생성된 Projectile 객체, 발사할 수 없으면 null입니다.</returns>
        public Projectile Shoot(PointF direction)
        {
            if (direction.IsEmpty) return null;

            PointF tearVelocity = Normalize(direction, this.TearSpeed);
            PointF tearStartPosition = new PointF(
                this.Bounds.X + this.Bounds.Width / 2f - TearSize.Width / 2f,
                this.Bounds.Y + this.Bounds.Height / 2f - TearSize.Height / 2f
            );
            return new Projectile(tearStartPosition, this.TearSize, tearVelocity, this.Damage, true /*isPlayerProjectile*/);
        }

        /// <summary>
        /// 플레이어가 대미지를 입습니다.
        /// </summary>
        /// <param name="amount">입은 대미지의 양입니다.</param>
        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0) return; // 이미 사망한 경우 추가 대미지 없음

            CurrentHealth -= amount;
            Debug.WriteLine($"[Player] Took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0; // 체력이 음수가 되지 않도록
                Die();
            }
        }

        /// <summary>
        /// 플레이어가 사망했을 때 호출됩니다.
        /// </summary>
        private void Die()
        {
            Debug.WriteLine("[Player] Died!");
            // 게임 오버 로직 또는 상태 변경 로직 (Form1에서 처리)
        }

        /// <summary>
        /// 플레이어의 위치를 강제로 설정합니다. (예: 방 이동 시)
        /// </summary>
        /// <param name="newPosition">새로운 위치입니다.</param>
        public void SetPosition(PointF newPosition)
        {
            this.Position = newPosition;
        }

        /// <summary>
        /// 벡터를 정규화하고 지정된 크기로 만듭니다.
        /// </summary>
        /// <param name="vector">대상 벡터입니다.</param>
        /// <param name="magnitude">결과 벡터의 원하는 길이입니다.</param>
        /// <returns>정규화되고 크기가 조절된 벡터입니다.</returns>
        private PointF Normalize(PointF vector, float magnitude)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length > 0)
                return new PointF((vector.X / length) * magnitude, (vector.Y / length) * magnitude);
            return PointF.Empty;
        }
    }
}