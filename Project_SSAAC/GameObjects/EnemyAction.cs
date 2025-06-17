// 파일 위치: Project_SSAAC/GameObjects/EnemyAction.cs
using System.Collections.Generic;

namespace Project_SSAAC.GameObjects
{
    /// <summary>
    /// 적이 한 프레임에 수행하는 행동의 결과를 담는 클래스입니다.
    /// </summary>
    public class EnemyAction
    {
        /// <summary>
        /// 이번 프레임에 새로 생성된 투사체 목록입니다.
        /// </summary>
        public List<Projectile> NewProjectiles { get; private set; }

        public EnemyAction()
        {
            NewProjectiles = new List<Projectile>();
        }
    }
}