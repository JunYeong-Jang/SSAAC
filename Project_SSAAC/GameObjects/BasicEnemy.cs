using System;
using System.Drawing;

namespace Project_SSAAC.GameObjects // 네임스페이스 확인!
{
    public class BasicEnemy : Enemy
    {
        private const int ENEMY_HEALTH = 10;
        private const float ENEMY_SPEED = 60f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(30, 30);

        public BasicEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED)
        {
            Console.WriteLine("BasicEnemy created at " + startPosition);
        }

        public override void UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return;
            MoveTowards(playerPosition, deltaTime); // 플레이어 추적
        }
    }
}