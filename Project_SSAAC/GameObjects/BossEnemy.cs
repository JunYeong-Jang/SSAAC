// 파일 위치: Project_SSAAC/GameObjects/BossEnemy.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Project_SSAAC.GameObjects
{
    public class BossEnemy : Enemy
    {
        private enum BossState { Moving, PatternAttack, Dashing, Cooldown }

        private const int BOSS_HEALTH = 30;
        private const float BOSS_SPEED = 60f;
        private static readonly SizeF BOSS_SIZE = new SizeF(60, 60);

        private BossState currentState = BossState.Moving;
        private float stateTimer = 3f; // 다음 행동까지의 시간
        private bool isEnraged = false; // 광폭화 상태 (체력 50% 이하)

        private PointF chargeTarget;
        private float chargeSpeed = 400f;

        public Dictionary<string, Bitmap> enemyAppearances { get; set; } = new Dictionary<string, Bitmap>()
        {
            { "idle", Properties.Resources.enemy_boss_idle },
            { "enraged", Properties.Resources.enemy_boss_enraged },
            { "attack", Properties.Resources.enemy_boss_attack }
        };

        private const int frameWidth = 52;
        private const int frameHeight = 54;

        public BossEnemy(PointF startPosition)
            : base(startPosition, BOSS_SIZE, BOSS_HEALTH, BOSS_SPEED)
        {
            this.CollisionDamage = 2;
        }

        public override EnemyAction UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;
            var action = new EnemyAction();
            stateTimer -= deltaTime;

            switch (currentState)
            {
                case BossState.Moving:
                    MoveTowards(playerPosition, deltaTime);
                    if (stateTimer <= 0)
                    {
                        currentState = BossState.PatternAttack;
                    }
                    break;

                case BossState.PatternAttack:
                    // 탄막 패턴 실행
                    int bulletCount = isEnraged ? 16 : 8; // 광폭화 시 16발
                    float projectileSpeed = isEnraged ? 250f : 180f; // 광폭화 시 탄속 증가

                    for (int i = 0; i < bulletCount; i++)
                    {
                        double angle = 2 * Math.PI * i / bulletCount;
                        PointF velocity = new PointF((float)Math.Cos(angle) * projectileSpeed, (float)Math.Sin(angle) * projectileSpeed);
                        action.NewProjectiles.Add(new Projectile(this.Bounds.Location, new SizeF(10, 10), velocity, 1, false));
                    }

                    // 다음 행동 결정
                    if (isEnraged)
                    {
                        // 광폭화 상태에서는 돌진과 패턴 공격을 번갈아 사용
                        currentState = BossState.Dashing;
                        stateTimer = 0.5f; // 돌진 전 짧은 조준 시간
                        chargeTarget = playerPosition;
                    }
                    else
                    {
                        currentState = BossState.Cooldown;
                        stateTimer = 2f; // 일반 상태 재사용 대기시간
                    }
                    break;

                case BossState.Dashing: // 광폭화 상태 전용 패턴
                    MoveTowards(chargeTarget, deltaTime, chargeSpeed);
                    if (stateTimer <= 0)
                    {
                        currentState = BossState.Cooldown;
                        stateTimer = 1.5f; // 돌진 후 재사용 대기시간
                    }
                    break;

                case BossState.Cooldown:
                    if (stateTimer <= 0)
                    {
                        currentState = BossState.Moving;
                        stateTimer = 3f; // 다음 패턴까지 3초간 플레이어 추적
                    }
                    break;
            }
            return action;
        }

        // 피해를 입을 때마다 광폭화 조건을 확인
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            if (!isEnraged && IsAlive && (float)CurrentHealth / MaxHealth <= 0.5f)
            {
                isEnraged = true;
                this.Speed *= 1.5f; // 광폭화 시 이동 속도 증가
                // 광폭화 시 외형 변경 등도 가능
                // this.enemyAppearance = Properties.Resources.enemy_boss_enraged;
            }
        }

        // 돌진을 위한 MoveTowards
        protected void MoveTowards(PointF target, float deltaTime, float speed)
        {
            if (!IsAlive) return;
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distance > 10.0f)
            {
                float moveX = (dx / distance) * speed * deltaTime;
                float moveY = (dy / distance) * speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }
            else
            {
                stateTimer = 0; // 목표 도달 시 즉시 다음 상태로
            }
        }
        public override void Draw(Graphics g)
        {
            Bitmap sheetToDraw = enemyAppearances[isEnraged ? "enraged" : "idle"];
            if (!IsAlive) return;
            
            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);
            if (this.facingLeft)
            {
                RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 60, 60);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Bounds.X + Bounds.Width, Bounds.Y);
                g.ScaleTransform(-1, 1);
                RectangleF destRect = new RectangleF(0, 0, 60, 60);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
                g.ResetTransform();
            }
        }
    }
}