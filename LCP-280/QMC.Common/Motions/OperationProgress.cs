using System;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 공용 장비 동작(홈 시퀀스 등) 진행 상황 모델
    /// </summary>
    public class OperationProgress
    {
        public string OperationId { get; set; }
        public string Title { get; set; }
        public int StepIndex { get; set; }         // 0-based
        public int TotalSteps { get; set; }
        public int StepAxisCount { get; set; }
        public int StepFailCount { get; set; }
        public string StepName { get; set; }
        public bool IsStepCompleted { get; set; }  // 해당 스텝 완료 알림 여부
        public bool IsCompleted { get; set; }      // 전체 완료
        public bool IsCanceled { get; set; }
        public bool IsAborted { get; set; }
        public string Message { get; set; }
    }
}
