using System.Drawing;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 게임 내 모든 시각적 객체의 기반이 되는 추상 클래스입니다.
    /// 위치, 크기, 업데이트 및 그리기 기능을 정의합니다.
    /// </summary>
    public abstract class GameObject
    {
        /// <summary>
        /// 객체의 현재 위치 (좌상단 기준)입니다.
        /// </summary>
        public PointF Position { get; protected set; }

        /// <summary>
        /// 객체의 크기 (너비와 높이)입니다.
        /// </summary>
        public SizeF Size { get; protected set; }

        /// <summary>
        /// 객체의 경계 사각형을 나타냅니다. Position과 Size를 기반으로 실시간 계산됩니다.
        /// 충돌 감지 및 그리기에 주로 사용됩니다.
        /// </summary>
        public RectangleF Bounds => new RectangleF(Position, Size);

        /// <summary>
        /// GameObject의 생성자입니다. 상속하는 클래스에서 호출됩니다.
        /// </summary>
        /// <param name="startPosition">객체의 초기 위치입니다.</param>
        /// <param name="size">객체의 크기입니다.</param>
        protected GameObject(PointF startPosition, SizeF size)
        {
            Position = startPosition;
            Size = size;
        }

        /// <summary>
        /// 매 프레임 호출되어 객체의 상태를 업데이트합니다.
        /// 상속받는 클래스에서 구체적인 로직을 구현해야 합니다.
        /// </summary>
        /// <param name="deltaTime">이전 프레임으로부터 경과된 시간(초)입니다.</param>
        public abstract void Update(float deltaTime);

        /// <summary>
        /// 객체를 화면에 그립니다.
        /// 상속받는 클래스에서 구체적인 그리기 로직을 구현해야 합니다.
        /// </summary>
        /// <param name="g">그리기에 사용될 Graphics 객체입니다.</param>
        public abstract void Draw(Graphics g);
    }
}