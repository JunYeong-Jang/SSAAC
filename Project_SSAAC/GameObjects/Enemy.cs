using System;
using System.Drawing;
using System.Diagnostics;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 모든 적 캐릭터의 기반이 되는 추상 클래스입니다.
    /// </summary>
    public abstract class Enemy : GameObject
    {
        /// <summary>
        /// 적의 최대 체력입니다.
        /// </summary>
        public int MaxHealth { get; protected set; }

        /// <summary>
        /// 적의 현재 체력입니다.
        /// </summary>
        public int CurrentHealth { get; protected set; }

        /// <summary>
        /// 적의 이동 속도입니다.
        /// </summary>
        public float Speed { get; protected set; }

        /// <summary>
        /// 적과 플레이어가 충돌했을 때 플레이어에게 주는 대미지입니다.
        /// </summary>
        public int CollisionDamage { get; protected set; } = 1;

        /// <summary>
        /// 적이 살아있는지 여부입니다. CurrentHealth가 0보다 크면 true입니다.
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// enemy의 외형 sheet를 저장하는 Bitmap 객체입니다.
        /// </summary>
        public Bitmap enemyAppearance { get; set; } = Properties.Resources.enemy_slime_idle;

        /// <summary>
        /// 적이 바라보는 방향을 저장하는 bool 값 (왼쪽이 기본)
        /// </summary>
        public bool facingLeft { get; set; } = true;

        /// <summary>
        /// enemy 시트를 자르는 크기
        /// </summary>
        private const int frameWidth = 44;
        private const int frameHeight = 30;

        /// <summary>
        /// enemy 애니메이션 시트의 인덱스
        /// </summary>
        public int frameIndex { get; set; } = 0;

        /// <summary>
        /// Enemy 객체를 초기화합니다.
        /// </summary>
        protected Enemy(PointF startPosition, SizeF size, int maxHealth, float speed)
            : base(startPosition, size)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Speed = speed;
        }

        /// <summary>
        /// GameObject의 Update를 오버라이드하지만, 적의 주요 로직은 UpdateEnemy에서 처리합니다.
        /// </summary>
        public override void Update(float deltaTime) { /* 개별 적 AI는 UpdateEnemy에서 처리 */ }

        /// <summary>
        /// 각 적 타입의 고유한 AI 및 행동 패턴을 업데이트합니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 경과 시간입니다.</param>
        /// <param name="playerPosition">플레이어의 현재 위치입니다 (추적 등에 사용).</param>
        public abstract void UpdateEnemy(float deltaTime, PointF playerPosition);

        /// <summary>
        /// 적을 화면에 그립니다. 살아있을 때만 그려집니다.
        /// </summary>
        /// <param name="g">Graphics 객체입니다.</param>
        public override void Draw(Graphics g)
        {
            if (!IsAlive) return;
            //g.FillRectangle(Brushes.DarkRed, Bounds); // 기본 적은 어두운 빨간 사각형

            // 잘라올 위치 (가로 방향으로 index번째)
            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);

            // 적이 오른쪽을 바라보는 경우, 이미지를 좌우 반전합니다.
            if (this.facingLeft)
            {
                // 도화지에 그릴 위치 (Position 위치에, 50*50 크기로 그림)
                RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 50, 50);


                //g.DrawImage(playerAppearance, destRect, srcRect, GraphicsUnit.Pixel);
                g.DrawImage(enemyAppearance, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Bounds.X + Bounds.Width, Bounds.Y); // X축 기준으로 이동
                g.ScaleTransform(-1, 1); // 좌우 반전

                RectangleF destRect = new RectangleF(0, 0, 50, 50); // 좌표계가 바뀌었으므로 (0,0)에서 시작
                g.DrawImage(enemyAppearance, destRect, srcRect, GraphicsUnit.Pixel);

                g.ResetTransform(); // 변환을 초기화하여 다른 요소에 영향 주지 않도록 합니다.
            }

        }

        /// <summary>
        /// 적이 대미지를 입습니다.
        /// </summary>
        /// <param name="amount">입은 대미지의 양입니다.</param>
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

        /// <summary>
        /// 적이 사망했을 때 호출됩니다.
        /// </summary>
        protected virtual void Die()
        {
            Debug.WriteLine($"[Enemy:{this.GetType().Name}] Died at {Position}");
            // 아이템 드랍, 점수 처리 등 로직 추가 가능
        }

        /// <summary>
        /// 지정된 목표 지점을 향해 이동합니다.
        /// </summary>
        /// <param name="target">목표 지점의 좌표입니다.</param>
        /// <param name="deltaTime">프레임 간 경과 시간입니다.</param>
        protected virtual void MoveTowards(PointF target, float deltaTime)
        {
            if (!IsAlive) return;
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // 목표에 매우 가까우면 이동을 멈춰 떨림 현상을 방지
            if (distance > 1.0f)
            {
                float moveX = (dx / distance) * Speed * deltaTime;
                float moveY = (dy / distance) * Speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }

            // 적이 플레이어를 바라보는 방향을 업데이트합니다.
            if (dx < 0)
            {
                facingLeft = true; // 왼쪽을 바라봄
            }
            else if (dx > 0)
            {
                facingLeft = false; // 오른쪽을 바라봄
            }
        }
    }
}