// 파일 위치: Project_SSAAC/GameObjects/BasicEnemy.cs
using System;
using System.Drawing;
using System.Diagnostics;

namespace Project_SSAAC.GameObjects
{
    public class BasicEnemy : Enemy
    {
        private const int ENEMY_HEALTH = 10;
        private const float ENEMY_SPEED = 70f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(30, 30);

        public BasicEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED)
        {
        }

        // <<-- 수정: 반환 타입을 Projectile로 변경 -->>
        public override Projectile UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;
            MoveTowards(playerPosition, deltaTime);
            return null; // 이 적은 투사체를 발사하지 않음
        }
    }
}