// 파일 위치: Project_SSAAC/GameObjects/ChargerEnemy.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Project_SSAAC.GameObjects
{
    public class ChargerEnemy : Enemy
    {
        private enum ChargerState { Idling, Aiming, Charging, Cooldown }

        private const int ENEMY_HEALTH = 12;
        private const float ENEMY_SPEED = 60f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(50, 40);

        private ChargerState currentState = ChargerState.Idling;
        private float stateTimer = 0f;
        private PointF chargeTarget;
        private float chargeSpeed = 350f;

        public Dictionary<string, Bitmap> enemyAppearances { get; set; } = new Dictionary<string, Bitmap>()
        {
            { "idle", Properties.Resources.enemy_charger_idle },
            { "run", Properties.Resources.enemy_charger_run },
        };

        private const int frameWidth = 52;
        private const int frameHeight = 34;

        public ChargerEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED)
        {
            this.CollisionDamage = 2;
        }

        // <<-- 수정: 반환 타입을 EnemyAction으로 변경 -->>
        public override EnemyAction UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;
            stateTimer -= deltaTime;

            switch (currentState)
            {
                case ChargerState.Idling:
                    MoveTowards(playerPosition, deltaTime);
                    float distance = (float)Math.Sqrt(Math.Pow(playerPosition.X - Position.X, 2) + Math.Pow(playerPosition.Y - Position.Y, 2));
                    if (distance < 300f)
                    {
                        currentState = ChargerState.Aiming;
                        stateTimer = 0.7f;
                    }
                    break;
                case ChargerState.Aiming:
                    if (stateTimer <= 0f)
                    {
                        chargeTarget = playerPosition;
                        currentState = ChargerState.Charging;
                        stateTimer = 0.8f;
                    }
                    break;
                case ChargerState.Charging:
                    MoveTowards(chargeTarget, deltaTime, chargeSpeed);
                    if (stateTimer <= 0f)
                    {
                        currentState = ChargerState.Cooldown;
                        stateTimer = 1.5f;
                    }
                    break;
                case ChargerState.Cooldown:
                    if (stateTimer <= 0f)
                    {
                        currentState = ChargerState.Idling;
                    }
                    break;
            }
            return new EnemyAction();
        }

        protected virtual void MoveTowards(PointF target, float deltaTime, float speed)
        {
            if (!IsAlive) return;
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distance > 5.0f)
            {
                float moveX = (dx / distance) * speed * deltaTime;
                float moveY = (dy / distance) * speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }
            else
            {
                stateTimer = 0;
            }
        }
        public override void Draw(Graphics g)
        {
            Bitmap sheetToDraw = enemyAppearances["idle"];
            if (!IsAlive) return;
            else if (currentState == ChargerState.Charging) sheetToDraw = enemyAppearances["run"];

            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            if (this.facingLeft)
            {
                RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 50, 40);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Bounds.X + Bounds.Width, Bounds.Y);
                g.ScaleTransform(-1, 1);
                RectangleF destRect = new RectangleF(0, 0, 50, 40);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
                g.ResetTransform();
            }
        }
    }
}