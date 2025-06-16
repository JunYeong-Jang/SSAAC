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
        /// <summary>
        /// 플레이어의 이동 속도 (초당 픽셀)입니다.
        /// </summary>
        public float Speed { get; set; } = 150f;

        /// <summary>
        /// 플레이어의 현재 속도 벡터 (방향과 크기)입니다. Form1에서 주로 설정됩니다.
        /// </summary>
        public PointF Velocity { get; set; } = PointF.Empty;

        /// <summary>
        /// 플레이어의 공격 대미지입니다.
        /// </summary>
        public int Damage { get; private set; } = 3;

        /// <summary>
        /// 플레이어가 발사하는 투사체의 속도입니다.
        /// </summary>
        public float TearSpeed { get; private set; } = 300f;

        /// <summary>
        /// 플레이어가 발사하는 투사체의 크기입니다.
        /// </summary>
        public SizeF TearSize { get; private set; } = new SizeF(10, 10);

        /// <summary>
        /// 플레이어의 최대 체력입니다.
        /// </summary>
        public int MaxHealth { get; private set; } = 6; // 예: 하트 3개

        /// <summary>
        /// 플레이어의 현재 체력입니다.
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// 플레이어의 외형 시트를 상태별로 저장하는 Bitmap 딕셔너리입니다.
        /// </summary>
        private Dictionary<string, Bitmap> playerAppearances { get; set; } = new Dictionary<string, Bitmap>()
        {
            { "idle", Properties.Resources.player_blue_idle },
            { "run", Properties.Resources.player_blue_run },
            { "invincible", Properties.Resources.player_blue_invincible }
        };

        /// <summary>
        /// 플레이어가 바라보는 방향 (오른쪽이 기본)
        public bool facingRight { get; set; } = true;

        /// <summary>
        /// 플레이어 시트를 자르는 크기
        /// </summary>
        private const int frameWidth = 32;
        private const int frameHeight = 32;

        /// <summary>
        /// 플레이어 애니메이션 시트의 인덱스
        /// </summary>
        public int frameIndex { get; set; } = 0;


        /// <summary>
        /// 플레이어의 기본 크기입니다.
        /// </summary>
        private static readonly SizeF PlayerDefaultSize = new SizeF(32, 32);


        public bool IsInvincible { get; private set; } = false; // 무적 상태 플래그
        private float invincibilityTimer = 0f;                // 무적 시간 타이머
        private const float INVINCIBILITY_DURATION = 0.75f;   // 무적 지속 시간 (예: 0.75초)

        public bool IsRun { get; set; } = false;    // 플레이어가 현재 idle 상태인지 여부

        /// <summary>
        /// Player 객체를 초기화합니다.
        /// </summary>
        /// <param name="startPos">플레이어의 시작 위치입니다.</param>

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

        /// <summary>
        /// 플레이어의 상태를 업데이트합니다. 주로 Velocity에 따라 위치를 이동시킵니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 경과 시간입니다.</param>
        public override void Update(float deltaTime)
        {
            Position = new PointF(
                Position.X + Velocity.X * deltaTime,
                Position.Y + Velocity.Y * deltaTime
            );

            // 무적 시간 처리 로직 추가
            if (IsInvincible)
            {
                invincibilityTimer -= deltaTime;
                if (invincibilityTimer <= 0)
                {
                    IsInvincible = false;
                    invincibilityTimer = 0f;
                    // Debug.WriteLine("[Player] Invincibility ended.");
                }
            }
        }

        /// <summary>
        /// 플레이어를 화면에 그립니다.
        /// </summary>
        /// <param name="g">Graphics 객체입니다.</param>
        public override void Draw(Graphics g)
        {
            if (IsInvincible)
            {
                //// 무적 상태일 때 깜빡이는 효과 또는 색상 변경
                //// 예시: 0.1초(또는 N프레임) 간격으로 색 변경 또는 보였다 안보였다 하기
                //// 간단하게는 invincibilityTimer 값을 기준으로 홀/짝 프레임에 따라 그리거나 색을 바꿈
                //// (int)(invincibilityTimer * 10) % 2 == 0  -> 100ms 마다 상태 변경 (깜빡임 속도 조절)
                //if ((int)((INVINCIBILITY_DURATION - invincibilityTimer) * 10) % 2 == 0) // 초당 5번 깜빡이는 효과 (INVINCIBILITY_DURATION에 따라 빈도 조절 필요)
                //{
                //    g.FillRectangle(Brushes.Cyan, Bounds); // 무적일 때 시안색으로 표시
                //}
                //// else { /* 안 그리면 깜빡이는 효과가 됨 */ }
                RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);

                if (this.facingRight)
                {
                    RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 50, 50);
                    g.DrawImage(playerAppearances["invincible"], destRect, srcRect, GraphicsUnit.Pixel);
                }
                else
                {
                    g.TranslateTransform(Position.X + 50, Position.Y);  // 기준점 오른쪽으로
                    g.ScaleTransform(-1, 1);  // 좌우 반전
                    RectangleF destRect = new RectangleF(0, 0, 50, 50);  // 좌표계 바뀌었음!
                    g.DrawImage(playerAppearances["invincible"], destRect, srcRect, GraphicsUnit.Pixel);
                    g.ResetTransform();  // 다른 요소에 영향 안 주게!
                }

            }
            else
            {
                if (IsRun)
                {
                    // 잘라올 위치 (가로 방향으로 index번째)
                    RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);

                    if (this.facingRight)
                    {

                        // 도화지에 그릴 위치 (Position 위치에, 50*50 크기로 그림)
                        RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 50, 50);


                        //g.DrawImage(playerAppearance, destRect, srcRect, GraphicsUnit.Pixel);
                        g.DrawImage(playerAppearances["run"], destRect, srcRect, GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.TranslateTransform(Position.X + 50, Position.Y);  // 기준점 오른쪽으로
                        g.ScaleTransform(-1, 1);  // 좌우 반전

                        RectangleF destRect = new RectangleF(0, 0, 50, 50);  // 좌표계 바뀌었음!
                        g.DrawImage(playerAppearances["run"], destRect, srcRect, GraphicsUnit.Pixel);

                        g.ResetTransform();  // 다른 요소에 영향 안 주게!
                    }
                }
                else
                {
                    // 잘라올 위치 (가로 방향으로 index번째)
                    RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);

                    if (this.facingRight)
                    {

                        // 도화지에 그릴 위치 (Position 위치에, 50*50 크기로 그림)
                        RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 50, 50);


                        //g.DrawImage(playerAppearance, destRect, srcRect, GraphicsUnit.Pixel);
                        g.DrawImage(playerAppearances["idle"], destRect, srcRect, GraphicsUnit.Pixel);
                    }
                    else
                    {
                        g.TranslateTransform(Position.X + 50, Position.Y);  // 기준점 오른쪽으로
                        g.ScaleTransform(-1, 1);  // 좌우 반전

                        RectangleF destRect = new RectangleF(0, 0, 50, 50);  // 좌표계 바뀌었음!
                        g.DrawImage(playerAppearances["idle"], destRect, srcRect, GraphicsUnit.Pixel);

                        g.ResetTransform();  // 다른 요소에 영향 안 주게!
                    }
                }


            }
        }

        /// <summary>
        /// 지정된 방향으로 투사체를 발사합니다.
        /// </summary>
        /// <param name="direction">발사 방향 벡터입니다.</param>
        /// <returns>생성된 Projectile 객체, 발사할 수 없으면 null입니다.</returns>
        public Projectile Shoot(PointF direction)
        {
            if (direction.IsEmpty) return null;

            PointF tearVelocity = Normalize(direction, this.TearSpeed);
            PointF tearStartPosition = new PointF(
                this.Bounds.X + this.Bounds.Width / 2f - TearSize.Width / 2f,
                this.Bounds.Y + this.Bounds.Height / 2f - TearSize.Height / 2f
            );
            return new Projectile(tearStartPosition, this.TearSize, tearVelocity, this.Damage, true /*isPlayerProjectile*/);
        }

        /// <summary>
        /// 플레이어가 대미지를 입습니다.
        /// </summary>
        /// <param name="amount">입은 대미지의 양입니다.</param>
        public void TakeDamage(int amount)
        {
            if (CurrentHealth <= 0 || IsInvincible) return; // 이미 죽었거나 무적 상태면 데미지 안 받음

            CurrentHealth -= amount;
            Debug.WriteLine($"[Player] Took {amount} damage. Health: {CurrentHealth}/{MaxHealth}");

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0; // 체력이 음수가 되지 않도록
                Die();
            }
            else // 피해를 입었지만 아직 살아있다면 무적 상태로 전환
            {
                IsInvincible = true;
                invincibilityTimer = INVINCIBILITY_DURATION; // 설정된 무적 시간으로 타이머 초기화
                // Debug.WriteLine($"[Player] Became invincible for {INVINCIBILITY_DURATION} seconds.");
                // from character
                // damege를 입었을때 애니메이션 시트 0으로 초기화
                frameIndex = 0;
            }
        }

        /// <summary>
        /// 플레이어가 사망했을 때 호출됩니다.
        /// </summary>
        private void Die()
        {
            Debug.WriteLine("[Player] Died!");
            // 게임 오버 로직 또는 상태 변경 로직 (Form1에서 처리)
        }

        /// <summary>
        /// 플레이어의 위치를 강제로 설정합니다. (예: 방 이동 시)
        /// </summary>
        /// <param name="newPosition">새로운 위치입니다.</param>
        public void SetPosition(PointF newPosition)
        {
            this.Position = newPosition;
        }

        /// <summary>
        /// 벡터를 정규화하고 지정된 크기로 만듭니다.
        /// </summary>
        /// <param name="vector">대상 벡터입니다.</param>
        /// <param name="magnitude">결과 벡터의 원하는 길이입니다.</param>
        /// <returns>정규화되고 크기가 조절된 벡터입니다.</returns>
        private PointF Normalize(PointF vector, float magnitude)
        {
            float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            if (length > 0)
                return new PointF((vector.X / length) * magnitude, (vector.Y / length) * magnitude);
            return PointF.Empty;
        }

        /// <summary>
        /// 플레이어를 즉시 사망 상태로 만듭니다.
        /// </summary>
        public void InstantKill()
        {
            if (CurrentHealth <= 0) return; // 이미 사망 상태면 아무것도 하지 않음

            CurrentHealth = 0; // 체력을 즉시 0으로 설정
            Die();             // 사망 처리 메소드 호출
            System.Diagnostics.Debug.WriteLine($"[Player] Instantaneously killed by timeout or special event." +
                $" Health: {CurrentHealth}/{MaxHealth}");
        }
    }
}