using System.Drawing;

namespace Project_SSAAC.GameObjects // 네임스페이스 확인!
{
    public abstract class GameObject
    {
        public PointF Position { get; protected set; }
        public SizeF Size { get; protected set; }
        // Bitmap Sprite 는 나중에 추가

        public RectangleF Bounds => new RectangleF(Position, Size);

        protected GameObject(PointF startPosition, SizeF size)
        {
            Position = startPosition;
            Size = size;
        }

        public abstract void Update(float deltaTime);
        public abstract void Draw(Graphics g);
    }
}