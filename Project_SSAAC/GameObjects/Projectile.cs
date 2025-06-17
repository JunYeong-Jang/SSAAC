// 파일 위치: Project_SSAAC/GameObjects/Projectile.cs
using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 게임 내에서 발사되는 투사체를 나타내는 클래스입니다.
    /// </summary>
    public class Projectile : GameObject
    {
        public PointF Velocity { get; private set; }
        public int Damage { get; private set; }
        public bool IsPlayerProjectile { get; private set; }
        private float lifeTimer = 0f;
        private const float MAX_LIFETIME = 2.0f;

        public Projectile(PointF startPosition, SizeF size, PointF velocity, int damage, bool isPlayer)
            : base(startPosition, size)
        {
            Velocity = velocity;
            Damage = damage;
            IsPlayerProjectile = isPlayer;
        }

        public override void Update(float deltaTime)
        {
            Position = new PointF(Position.X + Velocity.X * deltaTime,
                                  Position.Y + Velocity.Y * deltaTime);
            lifeTimer += deltaTime;
        }

        public override void Draw(Graphics g)
        {
            Brush color = IsPlayerProjectile ? Brushes.Cyan : Brushes.OrangeRed;
            g.FillEllipse(color, Bounds);
        }

        public bool ShouldBeRemoved(Rectangle clientBounds)
        {
            return lifeTimer > MAX_LIFETIME || !clientBounds.IntersectsWith(Rectangle.Round(this.Bounds));
        }
    }
}