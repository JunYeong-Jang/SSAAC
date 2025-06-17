// 파일 위치: Project_SSAAC/GameObjects/Enemy.cs
using System;
using System.Drawing;
using System.Diagnostics;

namespace Project_SSAAC.GameObjects
{
    public abstract class Enemy : GameObject
    {
        public int MaxHealth { get; protected set; }
        public int CurrentHealth { get; protected set; }
        public float Speed { get; set; }
        public int CollisionDamage { get; protected set; } = 1;
        public bool IsAlive => CurrentHealth > 0;
        public Bitmap enemyAppearance { get; set; } = Properties.Resources.enemy_slime_idle;
        public bool facingLeft { get; set; } = true;
        private const int frameWidth = 44;
        private const int frameHeight = 30;
        public int frameIndex { get; set; } = 0;

        protected Enemy(PointF startPosition, SizeF size, int maxHealth, float speed)
            : base(startPosition, size)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Speed = speed;
        }

        public override void Update(float deltaTime) { }

        // <<-- 수정: 반환 타입을 EnemyAction으로 변경 -->>
        public abstract EnemyAction UpdateEnemy(float deltaTime, PointF playerPosition);

        public override void Draw(Graphics g)
        {
            if (!IsAlive) return;
            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            if (this.facingLeft)
            {
                RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 50, 50);
                g.DrawImage(enemyAppearance, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Bounds.X + Bounds.Width, Bounds.Y);
                g.ScaleTransform(-1, 1);
                RectangleF destRect = new RectangleF(0, 0, 50, 50);
                g.DrawImage(enemyAppearance, destRect, srcRect, GraphicsUnit.Pixel);
                g.ResetTransform();
            }
        }

        public virtual void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            CurrentHealth -= amount;
            Debug.WriteLine($"[Enemy:{this.GetType().Name}] Took {amount} damage. Health: {CurrentHealth}/{MaxHealth} at {Position}");
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
        }

        protected virtual void Die()
        {
            Debug.WriteLine($"[Enemy:{this.GetType().Name}] Died at {Position}");
        }

        protected virtual void MoveTowards(PointF target, float deltaTime)
        {
            if (!IsAlive) return;
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 1.0f)
            {
                float moveX = (dx / distance) * Speed * deltaTime;
                float moveY = (dy / distance) * Speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }

            if (dx < 0) facingLeft = true;
            else if (dx > 0) facingLeft = false;
        }

        public void SetPosition(PointF newPosition)
        {
            this.Position = newPosition;
        }

        /// <summary>
        /// 적의 이동 속도를 지정된 만큼 증가시킵니다.
        /// </summary>
        /// <param name="amount">증가시킬 속도 값입니다.</param>
        public virtual void IncreaseSpeed(float amount)
        {
            Speed += amount;
        }
    }
}