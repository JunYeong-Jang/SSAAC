using System;
using System.Drawing;
using System.Diagnostics;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 기본적인 적 클래스입니다. 플레이어를 따라다닙니다.
    /// </summary>
    public class BasicEnemy : Enemy
    {
        private const int ENEMY_HEALTH = 10;
        private const float ENEMY_SPEED = 70f; // 기본 적 속도
        private static readonly SizeF ENEMY_SIZE = new SizeF(30, 30);

        /// <summary>
        /// BasicEnemy 객체를 초기화합니다.
        /// </summary>
        /// <param name="startPosition">적의 시작 위치입니다.</param>
        public BasicEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED)
        {
            // this.CollisionDamage = 1; // 필요 시 여기서 특정 적의 충돌 대미지 재정의 가능
            // Debug.WriteLine($"[BasicEnemy] Created at {startPosition}");
        }

        /// <summary>
        /// BasicEnemy의 AI를 업데이트합니다. 플레이어를 추적합니다.
        /// </summary>
        /// <param name="deltaTime">프레임 간 경과 시간입니다.</param>
        /// <param name="playerPosition">플레이어의 현재 위치입니다.</param>
        public override void UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return;
            MoveTowards(playerPosition, deltaTime); // 부모 클래스의 MoveTowards 사용
        }
    }
}