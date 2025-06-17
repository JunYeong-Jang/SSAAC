// 파일 위치: Project_SSAAC/GameObjects/ChargerEnemy.cs
using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects
{
    public class ChargerEnemy : Enemy
    {
        // 적의 상태를 정의하는 열거형
        private enum ChargerState
        {
            Idling, // 대기
            Aiming, // 조준
            Charging, // 돌진
            Cooldown  // 돌진 후 현자타임
        }

        private const int ENEMY_HEALTH = 12;
        private const float ENEMY_SPEED = 60f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(35, 35);

        private ChargerState currentState = ChargerState.Idling;
        private float stateTimer = 0f;
        private PointF chargeTarget;
        private float chargeSpeed = 350f;

        public ChargerEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED)
        {
            this.CollisionDamage = 2; // 돌진하는 적은 더 아프게
        }

        public override Projectile UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;

            stateTimer -= deltaTime;

            // 상태에 따라 다른 행동 수행
            switch (currentState)
            {
                case ChargerState.Idling:
                    // 플레이어를 천천히 따라다님
                    MoveTowards(playerPosition, deltaTime);
                    // 플레이어가 일정 거리 안에 들어오면 조준 시작
                    float distance = (float)Math.Sqrt(Math.Pow(playerPosition.X - Position.X, 2) + Math.Pow(playerPosition.Y - Position.Y, 2));
                    if (distance < 300f)
                    {
                        currentState = ChargerState.Aiming;
                        stateTimer = 0.7f; // 0.7초간 조준
                    }
                    break;

                case ChargerState.Aiming:
                    // 조준 중에는 멈춤
                    if (stateTimer <= 0f)
                    {
                        chargeTarget = playerPosition; // 현재 플레이어 위치를 돌진 목표로 설정
                        currentState = ChargerState.Charging;
                        stateTimer = 0.8f; // 0.8초 동안 돌진
                    }
                    break;

                case ChargerState.Charging:
                    // 목표 지점을 향해 빠르게 돌진
                    MoveTowards(chargeTarget, deltaTime, chargeSpeed);
                    if (stateTimer <= 0f)
                    {
                        currentState = ChargerState.Cooldown;
                        stateTimer = 1.5f; // 1.5초 동안 재사용 대기
                    }
                    break;

                case ChargerState.Cooldown:
                    // 돌진 후 잠시 멈춤
                    if (stateTimer <= 0f)
                    {
                        currentState = ChargerState.Idling;
                    }
                    break;
            }

            return null; // 이 적은 투사체를 발사하지 않음
        }

        // 속도를 지정할 수 있는 MoveTowards 오버로드
        protected virtual void MoveTowards(PointF target, float deltaTime, float speed)
        {
            if (!IsAlive) return;
            float dx = target.X - Position.X;
            float dy = target.Y - Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 5.0f) // 돌진은 목표에 거의 도달하면 멈춤
            {
                float moveX = (dx / distance) * speed * deltaTime;
                float moveY = (dy / distance) * speed * deltaTime;
                Position = new PointF(Position.X + moveX, Position.Y + moveY);
            }
            else // 목표에 도달하면 바로 Cooldown 상태로 전환
            {
                stateTimer = 0;
            }
        }
    }
}