using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects // 네임스페이스 확인!
{
    public class Player : GameObject
    {
        public float Speed { get; set; } = 150f;
        public PointF Velocity { get; set; } = PointF.Empty;
        public int Damage { get; private set; } = 3;
        public float TearSpeed { get; private set; } = 300f;
        public SizeF TearSize { get; private set; } = new SizeF(10, 10);
        public int MaxHealth { get; private set; } = 6;
        public int CurrentHealth { get; private set; }

        public Player(PointF startPos)
            : base(startPos, new SizeF(32, 32))
        {
            CurrentHealth = MaxHealth;
            Console.WriteLine("Player created.");
        }

        public override void Update(float deltaTime)
        {
            Position = new PointF(Position.X + Velocity.X * deltaTime,
                                  Position.Y + Velocity.Y * deltaTime);
            // (벽 충돌 등 추가)
        }

        public override void Draw(Graphics g)
        {
            g.FillRectangle(Brushes.Blue, Bounds); // 파란 사각형
        }

        public Projectile Shoot(PointF direction)
        {
            if (direction.IsEmpty) return null;
            PointF tearVelocity = Normalize(direction, this.TearSpeed);
            PointF tearStartPosition = new PointF(
                this.Position.X + this.Size.Width / 2f - TearSize.Width / 2f,
                this.Position.Y + this.Size.Height / 2f - TearSize.Height / 2f
            );
            return new Projectile(tearStartPosition, this.TearSize, tearVelocity, this.Damage, true);
        }

        public void TakeDamage(int amount)
        {
            CurrentHealth -= amount;
            Console.WriteLine($"Player took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");
            if (CurrentHealth <= 0) Die();
        }

        private void Die() { Console.WriteLine("Player Died!"); }

        private PointF Normalize(PointF vec, float magnitude)
        {
            float length = (float)Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            if (length > 0) return new PointF((vec.X / length) * magnitude, (vec.Y / length) * magnitude);
            return PointF.Empty;
        }
    }
}