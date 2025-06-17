// 파일 위치: Project_SSAAC/GameObjects/RangedEnemy.cs
using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects
{
    public class RangedEnemy : Enemy
    {
        private const int ENEMY_HEALTH = 7;
        private const float ENEMY_SPEED = 50f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(30, 30);

        // 행동 패턴을 결정하는 변수들
        private float optimalDistance = 250f; // 플레이어와 유지하려는 최적 거리
        private float tooCloseDistance = 150f;  // 이 거리보다 가까우면 뒤로 물러남
        private float shootCooldown = 0f;       // 발사 재사용 대기시간
        private const float SHOOT_INTERVAL = 1.5f; // 발사 간격 (1.5초)

        public RangedEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED)
        {
            // 외형을 다른 이미지로 변경하고 싶다면 여기서 설정
            // this.enemyAppearance = Properties.Resources.enemy_ranged_idle;
        }

        public override Projectile UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;

            // 1. 플레이어와의 거리 계산
            float dx = playerPosition.X - Position.X;
            float dy = playerPosition.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // 2. 거리에 따른 이동 패턴 결정
            if (distance < tooCloseDistance)
            {
                // 너무 가까우면 플레이어에게서 멀어짐
                MoveAwayFrom(playerPosition, deltaTime);
            }
            else if (distance > optimalDistance)
            {
                // 너무 멀면 플레이어에게 다가감
                MoveTowards(playerPosition, deltaTime);
            }
            // 최적 거리 안에서는 이동을 멈추고 공격 준비

            // 3. 공격 로직
            shootCooldown -= deltaTime;
            if (shootCooldown <= 0f)
            {
                shootCooldown = SHOOT_INTERVAL;
                return ShootAt(playerPosition); // 투사체 발사
            }

            return null; // 이번 프레임에 발사 안 함
        }

        // 플레이어를 향해 투사체를 발사하는 메서드
        private Projectile ShootAt(PointF targetPosition)
        {
            PointF direction = new PointF(targetPosition.X - Position.X, targetPosition.Y - Position.Y);
            float projectileSpeed = 200f;

            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (length > 0)
            {
                PointF velocity = new PointF((direction.X / length) * projectileSpeed, (direction.Y / length) * projectileSpeed);
                SizeF projectileSize = new SizeF(8, 8);
                // isPlayerProjectile: false 로 설정하여 적의 투사체임을 명시
                return new Projectile(this.Position, projectileSize, velocity, 1, false);
            }
            return null;
        }

        // 목표 지점에서 멀어지도록 이동하는 메서드
        protected virtual void MoveAwayFrom(PointF target, float deltaTime)
        {
            if (!IsAlive) return;
            float dx = Position.X - target.X; // 방향을 반대로
            float dy = Position.Y - target.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 1.0f)
            {
                float moveX = (dx / distance) * Speed * deltaTime;
                float moveY = (dy / distance) * Speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }
        }
    }
}