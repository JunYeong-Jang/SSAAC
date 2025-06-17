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
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED) { }

        // <<-- 수정: 반환 타입을 EnemyAction으로 변경 -->>
        public override EnemyAction UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;
            MoveTowards(playerPosition, deltaTime);
            return new EnemyAction(); // 아무 행동도 하지 않으므로 빈 Action 객체 반환
        }
    }
}