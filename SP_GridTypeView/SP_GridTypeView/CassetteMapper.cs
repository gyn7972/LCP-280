using System;
using System.Collections.Generic;
using System.Drawing;

namespace SP_GridTypeView
{
    public class CassetteMapper
    {
        public CassetteDataConfig Config { get; }
        public CassetteData Cassette { get; }

        public CassetteMapper(CassetteData cassette, CassetteDataConfig config)
        {
            Cassette = cassette;
            Config = config;
        }
        // 센서 기반 실시간 매핑 구동 예시
        public List<Point> MapBySensor(
            Func<bool> isWaferPresent,
            Func<Point> getCurrentPosition,
            Action<Point> moveTo,
            Point start,
            Point end,
            int pollIntervalMs = 1)
        {
            var mappedPoints = new List<Point>();
            // 시작 위치로 이동    
            moveTo(start);
            System.Threading.Thread.Sleep(100); // 안정화 대기

            // 끝 위치로 이동 시작 (여기선 동기 예시, 실제 장비는 비동기 처리 필요)
            moveTo(end);
            // 매핑 중 센서 polling
            while (true)
            {
                // 종료 조건(예: 현재 위치가 end에 도달, 또는 외부 신호 등)
                Point cur = getCurrentPosition();
                if (cur == end)
                    break;

                if (isWaferPresent())
                {
                    mappedPoints.Add(cur);
                }
                System.Threading.Thread.Sleep(pollIntervalMs);
            }
            return mappedPoints;
        }
    }
}
