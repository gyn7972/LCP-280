/*
 * Purpose
 *      QMC 회사만의 도형에 대해서 정의한다.
 *      
 *  Revision
 *      1. Created: 2019.04.17 by JUNG.CY
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace QMC.Common
{
    [Serializable]
    public enum ShapeLocation
    {
        Inner,
        Outer,
        Overlapping,
    }

    public interface IShape
    {
        /// <summary>
        /// 도형이 비어 있는지 여부를 나타내는 값을 가져옵니다.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 주어진 점이 도형의 어느 위치에 있는지를 판단한다.
        /// </summary>
        /// <param name="x">찾으려는 X의 좌표</param>
        /// <param name="y">찾으려는 Y의 좌표</param>
        /// <returns></returns>
        ShapeLocation Contains(double x, double y);

        /// <summary>
        /// 주어진 점이 도형의 어느 위치에 있는지를 판단한다.
        /// </summary>
        /// <param name="point">찾으려는 지점</param>
        /// <returns></returns>
        ShapeLocation Contains(Point point);

        /// <summary>
        /// 주어진 점이 도형의 어느 위치에 있는지를 판단한다.
        /// </summary>
        /// <param name="point">찾으려는 지점</param>
        /// <returns></returns>
        ShapeLocation Contains(PointD point);

        /// <summary>
        /// 주어진 점이 도형의 어느 위치에 있는지를 판단한다.
        /// </summary>
        /// <param name="coordinate">찾으려는 좌표</param>
        /// <returns></returns>
        ShapeLocation Contains(XyCoordinate coordinate);

        /// <summary>
        /// 주어진 점이 도형의 어느 위치에 있는지를 판단한다.
        /// </summary>
        /// <param name="shape">찾으려는 도형</param>
        /// <returns></returns>
        ShapeLocation Contains(IShape shape);

        /// <summary>
        /// 지정한 도형과 현재 도형이 같은지 여부를 확인합니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Equals(IShape value);
    }
}
