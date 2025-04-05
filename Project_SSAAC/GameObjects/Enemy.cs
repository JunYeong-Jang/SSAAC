using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects // 네임스페이스 확인!
{
    public abstract class Enemy : GameObject
    {
        public int MaxHealth { get; protected set; }
        public int CurrentHealth { get; protected set; }
        public float Speed { get; protected set; }
        public int CollisionDamage { get; protected set; } = 1;
        public bool IsAlive => CurrentHealth > 0;

        protected Enemy(PointF startPosition, SizeF size, int maxHealth, float speed)
            : base(startPosition, size)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Speed = speed;
        }

        public override void Update(float deltaTime) { /* 공통 로직 */ }

        public abstract void UpdateEnemy(float deltaTime, PointF playerPosition); // AI 구현용

        public override void Draw(Graphics g)
        {
            if (!IsAlive) return;
            g.FillRectangle(Brushes.DarkRed, Bounds); // 빨간 사각형
            // DrawHealthBar(g); // 필요시 체력 바 그리기
        }

        public virtual void TakeDamage(int amount)
        {
            if (!IsAlive) return;
            CurrentHealth -= amount;
            Console.WriteLine($"{this.GetType().Name} took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");
            if (CurrentHealth <= 0) Die();
        }

        protected virtual void Die() { Console.WriteLine($"{this.GetType().Name} died."); }

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
        }

        // 필요시 체력 바 그리기 함수 추가
        // protected virtual void DrawHealthBar(Graphics g) { ... }
    }
}