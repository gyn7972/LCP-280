using System.Collections.Generic;

namespace QMC.Common
{
    /// <summary>
    /// IMotionAxis 객체를 키/이름으로 제공하는 프로바이더 인터페이스.
    /// 장비 Motion 연결 직후 각 Unit 이 축을 획득하여 InitializeAxes 할 때 사용.
    /// </summary>
    public interface IMotionAxisProvider
    {
        IMotionAxis GetAxis(string keyOrName);
        IReadOnlyList<IMotionAxis> GetAllAxes();
    }
}
