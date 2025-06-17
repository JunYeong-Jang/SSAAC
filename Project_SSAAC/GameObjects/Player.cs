// 파일 위치: Project_SSAAC/GameObjects/Player.cs
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 플레이어 캐릭터를 나타내는 클래스입니다. 이동, 공격, 체력 관리 등의 기능을 가집니다.
    /// </summary>
    public class Player : GameObject
    {
        public float Speed { get; set; } = 150f;
        public PointF Velocity { get; set; } = PointF.Empty;
        public int Damage { get; private set; } = 3;
        public float TearSpeed { get; private set; } = 300f;
        public SizeF TearSize { get; private set; } = new SizeF(10, 10);
        public int MaxHealth { get; private set; } = 6;
        public int CurrentHealth { get; private set; }
        private Dictionary<string, Bitmap> playerAppearances { get; set; } = new Dictionary<string, Bitmap>()
        {
            { "idle", Properties.Resources.player_blue_idle },
            { "run", Properties.Resources.player_blue_run },
            { "invincible", Properties.Resources.player_blue_invincible }
        };
        public bool facingRight { get; set; } = true;
        private const int frameWidth = 32;
        private const int frameHeight = 32;
        public int frameIndex { get; set; } = 0;
        private static readonly SizeF PlayerDefaultSize = new SizeF(32, 32);
        public bool IsInvincible { get; private set; } = false;
        private float invincibilityTimer = 0f;
        private const float INVINCIBILITY_DURATION = 0.75f;
        public bool IsRun { get; set; } = false;

        public Player(PointF startPos)
            : base(startPos, PlayerDefaultSize)
        {
            CurrentHealth = MaxHealth;
            Debug.WriteLine($"[Player] Created at {startPos}. Health: {CurrentHealth}/{MaxHealth}");
        }

        public void SetAppearance(string appearance)
        {
            if (appearance == "blue")
            {
                this.playerAppearances["idle"] = Properties.Resources.player_blue_idle;
                this.playerAppearances["run"] = Properties.Resources.player_blue_run;
                this.playerAppearances["invincible"] = Properties.Resources.player_blue_invincible;
            }
            else if (appearance == "pink")
            {
                this.playerAppearances["idle"] = Properties.Resources.player_pink_idle;
                this.playerAppearances["run"] = Properties.Resources.player_pink_run;
                this.playerAppearances["invincible"] = Properties.Resources.player_pink_invincible;
            }
            else if (appearance == "frog")
            {
                this.playerAppearances["idle"] = Properties.Resources.player_frog_idle;
                this.playerAppearances["run"] = Properties.Resources.player_frog_run;
                this.playerAppearances["invincible"] = Properties.Resources.player_frog_invincible;
            }
            else if (appearance == "mask")
            {
                this.playerAppearances["idle"] = Properties.Resources.player_mask_idle;
                this.playerAppearances["run"] = Properties.Resources.player_mask_run;
                this.playerAppearances["invincible"] = Properties.Resources.player_mask_invincible;
            }
            else
            {
                Debug.WriteLine($"[Player] Unknown appearance: {appearance}");
                return;
            }
        }

        public override void Update(float deltaTime)
        {
            Position = new PointF(
                Position.X + Velocity.X * deltaTime,
                Position.Y + Velocity.Y * deltaTime
            );

            if (IsInvincible)
            {
                invincibilityTimer -= deltaTime;
                if (invincibilityTimer <= 0)
                {
                    IsInvincible = false;
                    invincibilityTimer = 0f;
                }
            }
        }

        public override void Draw(Graphics g)
        {
            Bitmap sheetToDraw = playerAppearances["idle"];
            if (IsInvincible) sheetToDraw = playerAppearances["invincible"];
            else if (IsRun) sheetToDraw = playerAppearances["run"];

            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            RectangleF destRect = new RectangleF(0, 0, 50, 50);

            if (this.facingRight)
            {
                destRect.X = Bounds.X;
                destRect.Y = Bounds.Y;
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Position.X + 50, Position.Y);
                g.ScaleTransform(-1, 1);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
                g.ResetTransform();
            }
        }

        public Projectile Shoot(PointF direction)
        {
            if (direction.IsEmpty) return null;
            PointF tearVelocity = Normalize(direction, this.TearSpeed);
            PointF tearStartPosition = new PointF(
                this.Bounds.X + this.Bounds.Width / 2f - TearSize.Width / 2f,
                this.Bounds.Y + this.Bounds.Height / 2f - TearSize.Height / 2f
            );
            return new Projectile(tearStartPosition, this.TearSize, tearVelocity, this.Damage, true);
        }

        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0 || IsInvincible) return;
            CurrentHealth -= amount;
            Debug.WriteLine($"[Player] Took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                Die();
            }
            else
            {
                IsInvincible = true;
                invincibilityTimer = INVINCIBILITY_DURATION;
                frameIndex = 0;
            }
        }

        private void Die()
        {
            Debug.WriteLine("[Player] Died!");
        }

        public void SetPosition(PointF newPosition)
        {
            this.Position = newPosition;
        }

        private PointF Normalize(PointF vector, float magnitude)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length > 0)
                return new PointF((vector.X / length) * magnitude, (vector.Y / length) * magnitude);
            return PointF.Empty;
        }

        public void InstantKill()
        {
            if (CurrentHealth <= 0) return;
            CurrentHealth = 0;
            Die();
            Debug.WriteLine($"[Player] Instantaneously killed. Health: {CurrentHealth}/{MaxHealth}");
        }
    }
}