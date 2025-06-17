using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 게임 내에서 발사되는 투사체를 나타내는 클래스입니다.
    /// </summary>
    public class Projectile : GameObject
    {
        /// <summary>
        /// 투사체의 속도 벡터입니다.
        /// </summary>
        public PointF Velocity { get; private set; }

        /// <summary>
        /// 투사체의 대미지입니다.
        /// </summary>
        public int Damage { get; private set; }

        /// <summary>
        /// 이 투사체가 플레이어에 의해 발사되었는지 여부입니다.
        /// </summary>
        public bool IsPlayerProjectile { get; private set; }

        private float lifeTimer = 0f; // 투사체 생존 시간 타이머
        private const float MAX_LIFETIME = 2.0f; // 투사체 최대 생존 시간 (초)

        /// <summary>
        /// Projectile 객체를 초기화합니다.
        /// </summary>
        /// <param name="startPosition">투사체 시작 위치입니다.</param>
        /// <param name="size">투사체 크기입니다.</param>
        /// <param name="velocity">투사체 속도 벡터입니다.</param>
        /// <param name="damage">투사체 대미지입니다.</param>
        /// <param name="isPlayer">플레이어의 투사체인지 여부입니다.</param>
        public Projectile(PointF startPosition, SizeF size, PointF velocity, int damage, bool isPlayer)
            : base(startPosition, size)
        {
            Velocity = velocity;
            Damage = damage;
            IsPlayerProjectile = isPlayer;
        }

        /// <summary>
        /// 투사체의 상태를 업데이트합니다. 위치 이동 및 생존 시간을 관리합니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 경과 시간입니다.</param>
        public override void Update(float deltaTime)
        {
            Position = new PointF(Position.X + Velocity.X * deltaTime,
                                  Position.Y + Velocity.Y * deltaTime);
            lifeTimer += deltaTime;
        }

        /// <summary>
        /// 투사체를 화면에 그립니다.
        /// </summary>
        /// <param name="g">Graphics 객체입니다.</param>
        public override void Draw(Graphics g)
        {
            Brush color = IsPlayerProjectile ? Brushes.Cyan : Brushes.OrangeRed;
            g.FillEllipse(color, Bounds); // 원형으로 투사체 표현
        }

        /// <summary>
        /// 이 투사체가 제거되어야 하는지 확인합니다.
        /// (수명이 다했거나 화면 밖으로 나갔을 경우)
        /// </summary>
        /// <param name="clientBounds">게임 화면의 경계입니다.</param>
        /// <returns>제거되어야 하면 true, 아니면 false입니다.</returns>
        public bool ShouldBeRemoved(Rectangle clientBounds)
        {
            // 수명이 다했거나, 화면 경계를 완전히 벗어났는지 확인
            return lifeTimer > MAX_LIFETIME || !clientBounds.IntersectsWith(Rectangle.Round(this.Bounds));
        }
    }
}