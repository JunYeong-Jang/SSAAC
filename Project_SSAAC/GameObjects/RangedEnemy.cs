// 파일 위치: Project_SSAAC/GameObjects/RangedEnemy.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Project_SSAAC.GameObjects
{
    public class RangedEnemy : Enemy
    {
        private const int ENEMY_HEALTH = 7;
        private const float ENEMY_SPEED = 50f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(60, 35);

        private float optimalDistance = 250f;
        private float tooCloseDistance = 150f;
        private float shootCooldown = 0f;
        private const float SHOOT_INTERVAL = 1.55f;

        public RangedEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED) { }

        public Dictionary<string, Bitmap> enemyAppearances { get; set; } = new Dictionary<string, Bitmap>()
        {
            { "idle", Properties.Resources.enemy_ranged_idle },
            { "run", Properties.Resources.enemy_ranged_run },
            { "attack", Properties.Resources.enemy_ranged_attack }
        };

        private const int frameWidth = 64;
        private const int frameHeight = 32;

        public bool IsAttack { get; set; } = false;

        // <<-- 수정: 반환 타입을 EnemyAction으로 변경 -->>
        public override EnemyAction UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;
            var action = new EnemyAction();

            float dx = playerPosition.X - Position.X;
            float dy = playerPosition.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance < tooCloseDistance) MoveAwayFrom(playerPosition, deltaTime);
            else if (distance > optimalDistance) MoveTowards(playerPosition, deltaTime);

            shootCooldown -= deltaTime;
            if (shootCooldown <= 0f)
            {
                IsAttack = true;
                shootCooldown = SHOOT_INTERVAL;
                Projectile p = ShootAt(playerPosition);
                if (p != null) action.NewProjectiles.Add(p);
            }

            return action;
        }

        private Projectile ShootAt(PointF targetPosition)
        {
            PointF direction = new PointF(targetPosition.X - Position.X, targetPosition.Y - Position.Y);
            float projectileSpeed = 200f;
            float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
            if (length > 0)
            {
                PointF velocity = new PointF((direction.X / length) * projectileSpeed, (direction.Y / length) * projectileSpeed);
                SizeF projectileSize = new SizeF(8, 8);
                return new Projectile(this.Position, projectileSize, velocity, 1, false);
            }
            return null;
        }

        protected virtual void MoveAwayFrom(PointF target, float deltaTime)
        {
            if (!IsAlive) return;
            float dx = Position.X - target.X;
            float dy = Position.Y - target.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distance > 1.0f)
            {
                float moveX = (dx / distance) * Speed * deltaTime;
                float moveY = (dy / distance) * Speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }
        }
        public override void Draw(Graphics g)
        {
            Bitmap sheetToDraw = enemyAppearances["run"];
            if (!IsAlive) return;
            else if (IsAttack) sheetToDraw =  enemyAppearances["attack"];

            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            
            if (this.facingLeft)
            {
                RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 60, 35);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Bounds.X + Bounds.Width, Bounds.Y);
                g.ScaleTransform(-1, 1);
                RectangleF destRect = new RectangleF(0, 0, 60, 35);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
                g.ResetTransform();
            }
        }
    }
}