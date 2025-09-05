using System;
using System.Collections.Generic;
using System.Threading;
using QMC.Common.Sequence;

namespace QMC.Common.SequenceSample
{
    /* ====================================================================================================
     * Sequence Usage Sample
     * ----------------------------------------------------------------------------------------------------
     * 목적
     *  - 여러 SequenceBase 파생 인스턴스를 동시에 운용하는 예시 제공.
     *  - 장비 애플리케이션에서 시퀀스 실행 / 에러 복구 / 상태 모니터링 / 동시 제어 방법을 보여준다.
     *
     * 주요 포인트
     *  1) 각 시퀀스는 자체 Task + CancellationToken 을 보유하므로 병렬 실행 안전.
     *  2) ErrorOccurred 이벤트에서 Recover 를 비동기로 호출(예: 지연 후 재시도) 가능.
     *  3) Pause / Resume 은 Running ↔ Paused 상태에서만 동작.
     *  4) Stop 은 즉시 Cancel + Stopping → Stopped 전환.
     *  5) Completed 는 정상 종료(-1 반환) 시 발생 (Stop 과 구분되는 최종 성공 상태).
     *
     * 확장 아이디어
     *  - 장비 UI: StepChanged / StateChanged 를 구독하여 진행바 / 로그 표시
     *  - 시퀀스 간 의존: 한 시퀀스 Completed 이벤트 발생 시 다른 시퀀스 Start
     *  - 글로벌 Abort: 모든 시퀀스 Stop 호출
     *  - 장비 상태 스냅샷: State, CurrentStep, 사용자 정의 Public Property 수집 → JSON Export
     *
     * ==================================================================================================== */
    public static class SequenceSampleRunner
    {
        public static void RunDemo()
        {
            var seq1 = new SamplePickPlaceSequence("PickPlace-A");
            var seq2 = new SamplePickPlaceSequence("PickPlace-B");

            var all = new List<SequenceBase> { seq1, seq2 };

            foreach (var s in all)
            {
                // 상태 변경 로깅
                s.StateChanged += (seq, oldSt, newSt) => Console.WriteLine($"[EVENT] {seq.Name} {oldSt} -> {newSt}");

                // 에러 발생: 3초 후 자동 Recover 예시
                s.ErrorOccurred += (seq, ex) =>
                {
                    Console.WriteLine($"[ERROR] {seq.Name} : {ex.Message} -> 3초 후 자동 Recover 시도");
                    new Thread(() =>
                    {
                        Thread.Sleep(3000);
                        // goToStep 지정 필요 시 seq.Recover(step); 형태
                        seq.Recover();
                    }) { IsBackground = true }.Start();
                };

                // 정상 완료
                s.Completed += seq => Console.WriteLine($"[COMPLETED] {seq.Name}");

                s.Start();
            }

            Console.WriteLine("Sequence demo started. Press 'P' to pause all, 'R' to resume, 'S' to stop.");
            while (true)
            {
                if (!Console.KeyAvailable)
                {
                    // 두 시퀀스 모두 완료되면 자동 종료
                    if (seq1.IsCompleted && seq2.IsCompleted) break;
                    Thread.Sleep(200);
                    continue;
                }
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.P)
                {
                    foreach (var s in all) s.Pause();
                }
                else if (key == ConsoleKey.R)
                {
                    foreach (var s in all) s.Resume();
                }
                else if (key == ConsoleKey.S)
                {
                    foreach (var s in all) s.Stop();
                    break;
                }
            }

            foreach (var s in all) s.Dispose();
            Console.WriteLine("Demo finished.");
        }
    }
}
