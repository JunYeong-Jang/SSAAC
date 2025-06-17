// 파일 위치: Project_SSAAC/GameObjects/BasicEnemy.cs
using System;
using System.Diagnostics;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Collections.Generic;

namespace Project_SSAAC.GameObjects
{
    public class BasicEnemy : Enemy
    {
        private const int ENEMY_HEALTH = 10;
        private const float ENEMY_SPEED = 70f;
        private static readonly SizeF ENEMY_SIZE = new SizeF(35, 35);

        public Dictionary<string, Bitmap> enemyAppearances { get; set; } = new Dictionary<string, Bitmap>()
        {
            { "idle", Properties.Resources.enemy_slime_idle }
        };

        private const int frameWidth = 44;
        private const int frameHeight = 30;

        public BasicEnemy(PointF startPosition)
            : base(startPosition, ENEMY_SIZE, ENEMY_HEALTH, ENEMY_SPEED) { }

        // <<-- 수정: 반환 타입을 EnemyAction으로 변경 -->>
        public override EnemyAction UpdateEnemy(float deltaTime, PointF playerPosition)
        {
            if (!IsAlive) return null;
            MoveTowards(playerPosition, deltaTime);
            return new EnemyAction(); // 아무 행동도 하지 않으므로 빈 Action 객체 반환
        }
        public override void Draw(Graphics g)
        {
            Bitmap sheetToDraw = enemyAppearances["idle"];
            if (!IsAlive) return;

            RectangleF srcRect = new RectangleF(frameIndex * frameWidth, 0, frameWidth, frameHeight);

            if (this.facingLeft)
            {
                RectangleF destRect = new RectangleF(Bounds.X, Bounds.Y, 35, 35);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
            }
            else
            {
                g.TranslateTransform(Bounds.X + Bounds.Width, Bounds.Y);
                g.ScaleTransform(-1, 1);
                RectangleF destRect = new RectangleF(0, 0, 35, 35);
                g.DrawImage(sheetToDraw, destRect, srcRect, GraphicsUnit.Pixel);
                g.ResetTransform();
            }
        }
    }
}