using System;
using System.Collections.Generic;
using System.Linq;

namespace SP_MapGenerator
{
    public class DieData
    {
        public double RawX { get; set; }
        public double RawY { get; set; }
        public int IndexX { get; set; }
        public int IndexY { get; set; }

        public override string ToString()
        {
            return $"{RawX:F3},{RawY:F3} => ({IndexX},{IndexY})";
        }
    }

    public class RegionGrowingGenerator
    {
        // [설정] 분석된 최적 Pitch (약간의 오차가 있어도 Region Growing 방식이라 안전함)
        public double PitchX { get; set; } = 0.5414;
        public double PitchY { get; set; } = 0.5540;

        // 검색 반경 (Pitch의 약 1.5배, 대각선 포함 넉넉하게 잡음)
        private double SearchRadius => Math.Max(PitchX, PitchY) * 1.5;

        public List<DieData> GenerateMap(List<(double x, double y)> rawPoints)
        {
            int n = rawPoints.Count;
            if (n == 0) return new List<DieData>();

            // 1. 공간 해싱 (Spatial Hash) 구축
            // KDTree 대신 C# Dictionary를 이용하여 주변 이웃을 O(1)로 찾습니다.
            // 격자 크기는 검색 반경으로 설정
            double cellSize = SearchRadius;
            var grid = new Dictionary<(int, int), List<int>>();

            for (int i = 0; i < n; i++)
            {
                var key = GetGridKey(rawPoints[i], cellSize);
                if (!grid.ContainsKey(key)) grid[key] = new List<int>();
                grid[key].Add(i);
            }

            // 2. 시작점 찾기 (데이터의 중심점과 가장 가까운 칩)
            double avgX = rawPoints.Average(p => p.x);
            double avgY = rawPoints.Average(p => p.y);
            int startNode = -1;
            double minCenterDist = double.MaxValue;

            for (int i = 0; i < n; i++)
            {
                double dist = DistSq(rawPoints[i], (avgX, avgY));
                if (dist < minCenterDist)
                {
                    minCenterDist = dist;
                    startNode = i;
                }
            }

            // 3. BFS (너비 우선 탐색) 초기화
            var indices = new Dictionary<int, (int x, int y)>();
            var visited = new HashSet<int>();
            var queue = new Queue<int>();

            // 시작점 인덱스를 (0,0)으로 설정
            indices[startNode] = (0, 0);
            visited.Add(startNode);
            queue.Enqueue(startNode);

            // 4. 영역 확장 (Region Growing)
            while (queue.Count > 0)
            {
                int currIdx = queue.Dequeue();
                var currPos = rawPoints[currIdx];
                var (currIx, currIy) = indices[currIdx];

                // 내 주변 격자(3x3)를 검색하여 이웃 후보 찾기
                var centerKey = GetGridKey(currPos, cellSize);
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        var neighborKey = (centerKey.cx + dx, centerKey.cy + dy);
                        if (!grid.ContainsKey(neighborKey)) continue;

                        foreach (int neighborIdx in grid[neighborKey])
                        {
                            if (visited.Contains(neighborIdx)) continue;

                            var neighborPos = rawPoints[neighborIdx];
                            
                            // 실제 거리 체크 (원형 검색)
                            double dSq = DistSq(currPos, neighborPos);
                            if (dSq > SearchRadius * SearchRadius) continue;

                            // 5. 상대적 인덱스 계산 (핵심 로직)
                            double diffX = neighborPos.x - currPos.x;
                            double diffY = neighborPos.y - currPos.y;

                            // Pitch로 나누어 몇 칸 떨어져 있는지 계산 (반올림)
                            int stepX = (int)Math.Round(diffX / PitchX);
                            int stepY = (int)Math.Round(diffY / PitchY);

                            // 너무 가까운 점(중복/노이즈)은 무시
                            if (stepX == 0 && stepY == 0) continue;

                            // 인덱스 확정 및 큐에 추가
                            indices[neighborIdx] = (currIx + stepX, currIy + stepY);
                            visited.Add(neighborIdx);
                            queue.Enqueue(neighborIdx);
                        }
                    }
                }
            }

            // 6. 결과 정리 및 인덱스 정규화 (최소값을 0으로 이동)
            var result = new List<DieData>();
            if (indices.Count == 0) return result;

            int minIx = indices.Values.Min(v => v.x);
            int minIy = indices.Values.Min(v => v.y);

            foreach (var kvp in indices)
            {
                int idx = kvp.Key;
                var raw = rawPoints[idx];
                var (ix, iy) = kvp.Value;

                result.Add(new DieData
                {
                    RawX = raw.x,
                    RawY = raw.y,
                    IndexX = ix - minIx, // 0부터 시작하도록 보정
                    IndexY = iy - minIy
                });
            }

            return result;
        }

        // 보조 함수: 좌표를 격자 키로 변환
        private (int cx, int cy) GetGridKey((double x, double y) pos, double cellSize)
        {
            return ((int)Math.Floor(pos.x / cellSize), (int)Math.Floor(pos.y / cellSize));
        }

        // 보조 함수: 거리 제곱 계산 (Sqrt 생략 최적화)
        private double DistSq((double x, double y) p1, (double x, double y) p2)
        {
            double dx = p1.x - p2.x;
            double dy = p1.y - p2.y;
            return dx * dx + dy * dy;
        }

        // Pitch 자동 계산 함수
        public static (double px, double py) EstimatePitch(List<(double x, double y)> points)
        {
            // 데이터가 너무 적으면 계산 불가
            if (points == null || points.Count < 2) return (0.54, 0.55); // 기본값 반환

            var dxSamples = new List<double>();
            var dySamples = new List<double>();

            // 1. 성능 최적화를 위해 X축 기준으로 정렬 (전체 탐색 N^2 방지)
            var sorted = points.OrderBy(p => p.x).ToList();
            int searchWindow = 100; // 주변 100개만 검색해도 충분

            for (int i = 0; i < sorted.Count; i++)
            {
                var p1 = sorted[i];
                double minD = double.MaxValue;
                (double x, double y) closest = (0, 0);
                bool found = false;

                // 2. 내 주변(윈도우 범위)에서 가장 가까운 '진짜 이웃' 찾기
                int start = Math.Max(0, i - searchWindow);
                int end = Math.Min(sorted.Count, i + searchWindow);

                for (int j = start; j < end; j++)
                {
                    if (i == j) continue;
                    var p2 = sorted[j];

                    double dx = Math.Abs(p2.x - p1.x);
                    double dy = Math.Abs(p2.y - p1.y);

                    // X축 정렬 특성상 X거리가 너무 멀어지면 더 볼 필요 없음
                    if (dx > 2.0) continue;

                    double distSq = dx * dx + dy * dy;

                    // 너무 먼 거리(예: 2mm 이상)는 이웃이 아님
                    if (distSq > 4.0) continue;

                    if (distSq < minD)
                    {
                        minD = distSq;
                        closest = p2;
                        found = true;
                    }
                }

                if (found)
                {
                    double dx = Math.Abs(closest.x - p1.x);
                    double dy = Math.Abs(closest.y - p1.y);

                    // 3. 방향 판별 (중요!)
                    // 대각선 이웃은 제외하고, 수평/수직 이웃인 경우만 샘플로 수집

                    // 수평 이웃 (Y차이가 거의 없고, X차이가 유의미할 때)
                    if (dy < 0.15 && dx > 0.2)
                    {
                        dxSamples.Add(dx);
                    }
                    // 수직 이웃 (X차이가 거의 없고, Y차이가 유의미할 때)
                    else if (dx < 0.15 && dy > 0.2)
                    {
                        dySamples.Add(dy);
                    }
                }
            }

            // 4. 중앙값(Median) 계산 함수
            // 평균(Average)은 노이즈 하나만 튀어도 값이 망가지지만, 중앙값은 정확함
            double GetMedian(List<double> list)
            {
                if (list.Count == 0) return 0.0;
                list.Sort();
                int mid = list.Count / 2;
                return (list.Count % 2 != 0) ? list[mid] : (list[mid - 1] + list[mid]) / 2.0;
            }

            // 5. 결과 도출 (샘플이 없으면 기본값 반환)
            double px = (dxSamples.Count > 0) ? GetMedian(dxSamples) : 0.5414;
            double py = (dySamples.Count > 0) ? GetMedian(dySamples) : 0.5540;

            // (선택 사항) 조금 더 정밀하게 다듬기: 중앙값 근처 ±10% 값들의 평균 사용
            // 노이즈를 완벽 제거한 상태에서 평균을 내므로 소수점 4째자리까지 정확해짐
            double Refine(List<double> list, double median)
            {
                if (list.Count == 0) return median;
                var valid = list.Where(v => Math.Abs(v - median) < median * 0.1).ToList();
                return (valid.Count > 0) ? valid.Average() : median;
            }

            return (Refine(dxSamples, px), Refine(dySamples, py));
        }
    }
}
