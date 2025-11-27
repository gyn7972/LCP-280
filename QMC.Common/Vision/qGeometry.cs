/*
 * Purpose
 *      Geometry(기하학)에 관련된 함수들을 제공한다.
 * 
 * Revision
 *      1. Created: 2018.12.21 by LIM.WT
 *      
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QMC.Common
{
    public static class qGeometry
    {
        /// <summary>
        /// 주어진 점이 폴리곤의 내부에 있는지를 반환한다.
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        /// <remarks>
        /// winding number (감싸는 횟수)를 확인하는 방법을 사용하였다.
        /// </remarks>
        public static int IsPointInsidePolygon(IList<PointD> polygon, PointD point)
        {
            int count = 0;

            PointD current, next;

            // loop through all edges of the polygon
            for (int i = 0; i < polygon.Count; i++)
            {
                current = polygon[i];
                next = polygon[(i + 1) % polygon.Count];

                // 선분에 있는 경우
                if (qGeometry.IsPointOnLineSegment(current, next, point, 0.0) == true)
                    return 0;

                if (current.Y <= point.Y)
                {
                    if (point.Y <= next.Y)
                        if (0 < qGeometry.WhereIsPointOnLine(current, next, point)) // left
                            count++;
                }
                else
                {
                    if (next.Y <= point.Y)
                        if (qGeometry.WhereIsPointOnLine(current, next, point) < 0) // right
                            count--;
                }
            }

            return count != 0 ? 1 : -1;
        }

        /// <summary>
        /// 점이 선을 기준으로 어디에 있는지를 확인한다.
        /// </summary>
        /// <param name="start">선의 시작점</param>
        /// <param name="end">선의 끝점</param>
        /// <param name="point">위치를 확인하고하는 점</param>
        /// <returns>
        /// 선의 왼쪽에 있는 경우는 0보다 큰값, 
        /// 선위에 있는 경우는 0, 
        /// 선의 오른쪽에 있는 경우는 0보다 작은 값을 반환한다.
        /// </returns>
        private static double WhereIsPointOnLine(PointD start, PointD end, PointD point)
        {
            return ((end.X - start.X) * (point.Y - start.Y) - (point.X - start.X) * (end.Y - start.Y));
        }

        /// <summary>
        /// 선분 (line segment)위에 주어진 점이 있는지를 확인한다.
        /// </summary>
        /// <param name="start">선분의 시작점</param>
        /// <param name="end">선분의 끝점</param>
        /// <param name="point">선분위에 있는지를 확인하고자 하는 점</param>
        /// <param name="epsilon">선분에 있는 것으로 판단하기 위한 오차 값</param>
        /// <returns></returns>
        public static bool IsPointOnLineSegment(PointD start, PointD end, PointD point, double epsilon = 0.001)
        {
            if (point.X - Math.Max(start.X, end.X) > epsilon ||
                Math.Min(start.X, end.X) - point.X > epsilon ||
                point.Y - Math.Max(start.Y, end.Y) > epsilon ||
                Math.Min(start.Y, end.Y) - point.Y > epsilon)
                return false;

            if (Math.Abs(end.X - start.X) <= epsilon)
                return Math.Abs(start.X - point.X) <= epsilon || Math.Abs(end.X - point.X) <= epsilon;
            if (Math.Abs(end.Y - start.Y) < epsilon)
                return Math.Abs(start.Y - point.Y) <= epsilon || Math.Abs(end.Y - point.Y) <= epsilon;

            double x = start.X + (point.Y - start.Y) * (end.X - start.X) / (end.Y - start.Y);
            double y = start.Y + (point.X - start.X) * (end.Y - start.Y) / (end.X - start.X);

            return Math.Abs(point.X - x) <= epsilon || Math.Abs(point.Y - y) <= epsilon;
        }

        /// <summary>
        /// 회전에 의한 위치 이동값을 구한다.
        /// </summary>
        /// <param name="center">회전 중심 좌표</param>
        /// <param name="target">회전하고자 하는 좌표</param>
        /// <param name="degree">회전 값 (degree)</param>
        /// <returns>target이 degree 만큼 회전시 이동된 좌표값을 반환한다.</returns>
        public static PointD CalculateRotationTransformation(PointD center, PointD target, double degree)
        {
            double x = 0.0, y = 0.0;
            double radian = 0.0;

            radian = qMath.DegreeToRadian(degree);

            x = (target.X - center.X) * Math.Cos(radian) - (target.Y - center.Y) * Math.Sin(radian);
            y = (target.X - center.X) * Math.Sin(radian) + (target.Y - center.Y) * Math.Cos(radian);

            return new PointD(center.X + x, center.Y + y);
        }

        public static XyCoordinate CalculateRotationTransformation(XyCoordinate center, XyCoordinate target, double degree)
        {
            PointD rotation = qGeometry.CalculateRotationTransformation(new PointD(center.X, center.Y), new PointD(target.X, target.Y), degree);

            target.X = rotation.X;
            target.Y = rotation.Y;

            return target;
        }
        public static IXyCoordinate CalculateRotationTransformation(IXyCoordinate center, IXyCoordinate target, double degree)
        {
            PointD rotation = qGeometry.CalculateRotationTransformation(new PointD(center.X, center.Y), new PointD(target.X, target.Y), degree);

            target.X = rotation.X;
            target.Y = rotation.Y;

            return target;
        }

        /// <summary>
        /// 2점이 이루는 직선의 기울기를 얻는다.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static bool GetAngle(PointD p1, PointD p2, ref double degree)
        {
            try
            {
                degree = qMath.RadianToDegree(Math.Atan((p1.Y - p2.Y) / (p1.X - p2.X)));
                // To Do: 2020.02.21 by LIM.WT
                // 직교좌표계에서는 아래와 같이 구현하는 것이 올바른 방법이다.
                // 현재 적용중인 (주로 vision) 코드 리뷰후 적용하도록 한다.
                //degree = qMath.RadianToDegree(Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static bool GetAngle2(PointD p1, PointD p2, ref double degree)
        {
            try
            {
                // To Do: 2020.02.21 by LIM.WT
                // 직교좌표계에서는 아래와 같이 구현하는 것이 올바른 방법이다.
                // 현재 적용중인 (주로 vision) 코드 리뷰후 적용하도록 한다.
                degree = qMath.RadianToDegree(Math.Atan2((p2.Y - p1.Y), (p2.X - p1.X)));
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 세점 사이의 각도를 얻는다.
        /// </summary>
        /// <param name="c">center point</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="degree">세점이 이루는 각도</param>
        /// <returns></returns>
        public static void GetAngle(PointD c, PointD p1, PointD p2, ref double degree)
        {
            double radian = 0.0;

            radian = Math.Atan2((p2.Y - c.Y), (p2.X - c.X)) - Math.Atan2((p1.Y - c.Y), (p1.X - c.X));

            degree = qMath.RadianToDegree(radian);
        }

        /// <summary>
        /// 2점 사이의 거리를 얻는다.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double GetDistanceBetweenTwoPoints(PointD a, PointD b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        /// <summary>
        /// 두 점의 1차 방정식을 구한다.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="slope"></param>
        /// <param name="constants"></param>
        public static void GetLinearEquation(PointD a, PointD b, ref double slope, ref double constants)
        {
            slope = (a.Y - b.Y) / (a.X - b.X);
            constants = a.Y - slope * a.X;
        }

        /// <summary>
        /// 직선의 1차 방정식을 구한다.
        /// </summary>
        /// <param name="line">직선</param>
        /// <param name="slope">기울기</param>
        /// <param name="constants">상수</param>
        public static void GetLinearEquation(LineD line, ref double slope, ref double constants)
        {
            slope = (line.Start.Y - line.End.Y) / (line.Start.X - line.End.X);
            constants = line.Start.Y - slope * line.Start.X;
        }

        /// <summary>
        /// Line과 한 점간의 최단 거리를 구한다.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static double GetDistanceBetweenLineAndPoint(LineD line, PointD point)
        {
            double slope = 0.0, constants = 0.0;
            double distance = 0.0;

            qGeometry.GetLinearEquation(line, ref slope, ref constants);

            distance = Math.Abs(slope * point.X - 1 * point.Y + constants) / Math.Sqrt(1 + Math.Pow(slope, 2));

            return distance;
        }
    }
}
