using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMC.Common
{
    public static class qMath
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double DegreeToRadian(double degree)
        {
            return degree * (Math.PI / 180);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radian"></param>
        /// <returns></returns>
        public static double RadianToDegree(double radian)
        {
            return radian * (180.0 / Math.PI);
        }

        /// <summary>
        /// 지정된 점의 x,y 값의 지정된 거듭제곱을 반환합니다.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static PointD Pow(PointD point, double power)
        {
            return new PointD(Math.Pow(point.X, power), Math.Pow(point.Y, power));
        }

        /// <summary>
        /// 점들의 평균값을 반환합니다.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static PointD Average(params PointD[] points)
        {
            return qMath.Average(new PointDCollection(points));
        }

        /// <summary>
        /// 점들의 평균값을 반환합니다.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static PointD Average(PointDCollection points)
        {
            PointD average = PointD.Empty;

            for (int i = 0; i < points.Count; i++)
            {
                average += points[i] / points.Count;
            }

            return average;
        }

        /// <summary>
        /// 표준편차를 반환합니다.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double GetStandardDeviation(double[] value)
        {
            double standardDeviation = 0.0;
            double variance = 0.0;

            if (value.Length <= 0) return 0;
            for (int i = 0; i < value.Length; i++)
            {
                variance += Math.Pow(value[i] - value.Average(), 2);
            }
            standardDeviation = Math.Sqrt(variance / value.Length);
            return standardDeviation;
        }

        /// <summary>
        /// 입력한 표본의 평균으로부터 +/-N표준편차값 내에 들어오는 데이터의 평균를 반환합니다.
        /// </summary>
        /// <param name="value">표본데이터</param>
        /// <param name="number">표준편차 배수</param>
        /// <returns>평균</returns>
        public static double GetAverageWithinNSigma(double[] value, int number)
        {
            double nStandardDeviation = qMath.GetStandardDeviation(value) * number;
            double variance = 0.0;
            int count = 0;

            if (value.Length <= 0 || number < 0) return 0;
            for (int i = 0; i < value.Length; i++)
            {
                if(nStandardDeviation == 0)
                {
                    return value.Average();
                }
                else if (value.Average() - nStandardDeviation <= value[i] && value[i] <= value.Average() + nStandardDeviation)
                {
                    count++;
                    variance += value[i];
                }
            }
            if (count == 0) return 0;

            return variance/count;
        }

        /// <summary>
        /// 가우스-조던 소거법을 이용하여 계산한 역행렬을 배열로 반환합니다.
        /// </summary>
        /// <param name="matrix">행과 열의 수가 같은 행렬</param>
        /// <returns>2차원 배열</returns>
        public static double[,] InverseMatrix(double[,] matrix)
        {
            int n = matrix.GetLength(0);

            double[,] result = new double[n, n];
            double[,] tmpWork = new double[n, n];
            Array.Copy(matrix, tmpWork, n * n);  // 기존값을 보존하기 위함.  

            // 계산 결과가 저장되는 result 행렬을 단위행렬로 초기화
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    result[i, j] = (i == j) ? 1 : 0;

            // 대각 요소를 0 이 아닌 수로 만듦
            const double ERROR = 1.0e-10;
            for (int i = 0; i < n; i++)
                if (-ERROR < tmpWork[i, i] && tmpWork[i, i] < ERROR) //if (-ERROR < tmpWork[i, i] && tmpWork[i, i] < ERROR)
                {
                    for (int k = 0; k < n; k++)
                    {
                        if (-ERROR < tmpWork[k, i] && tmpWork[k, i] < ERROR) continue;
                        for (int j = 0; j < n; j++)
                        {
                            tmpWork[i, j] += tmpWork[k, j];
                            result[i, j] += result[k, j];  // result[i*n+j] += result[k*n+j];
                        }
                        break;
                    }
                    if (-ERROR < tmpWork[i, i] && tmpWork[i, i] < ERROR) return result;
                }

            // Gauss-Jordan eliminatio
            for (int i = 0; i < n; i++)
            {
                // 대각 요소를 1로 만듦
                double constant = tmpWork[i, i];      // 대각 요소의 값 저장
                for (int j = 0; j < n; j++)
                {
                    tmpWork[i, j] /= constant;   // tmpWork[i][i] 를 1 로 만드는 작업
                    result[i, j] /= constant; // result[i*n+j] /= constant;   // i 행 전체를 tmpWork[i][i] 로 나눔
                }

                // i 행을 제외한 k 행에서 tmpWork[k][i] 를 0 으로 만드는 단계
                for (int k = 0; k < n; k++)
                {
                    if (k == i) continue;      // 자기 자신의 행은 건너뜀
                    if (tmpWork[k, i] == 0) continue;   // 이미 0 이 되어 있으면 건너뜀

                    // tmpWork[k][i] 행을 0 으로 만듦
                    constant = tmpWork[k, i];
                    for (int j = 0; j < n; j++)
                    {
                        tmpWork[k, j] = tmpWork[k, j] - tmpWork[i, j] * constant;
                        result[k, j] = result[k, j] - result[i, j] * constant;  // result[k*n+j] = result[k*n+j] - result[i*n+j] * constant;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 두 행렬의 곱셈값을 반환합니다.
        /// </summary>
        /// <param name="matrix1">m1의 열의 갯수가 m2의 행의 갯수와 같아야 합니다.</param>
        /// <param name="matrix2">m2의 행의 갯수가 m1의 열의 갯수와 같아야 합니다.</param>
        /// <returns>2차원 배열</returns>
        public static double[,] MatrixMultiplication(double[,] matrix1, double[,] matrix2)
        {
            double[,] result = new double[matrix1.GetLength(0), matrix2.GetLength(1)];

            if (matrix1.GetLength(1) == matrix2.GetLength(0))
            {
                for (int i = 0; i < result.GetLength(0); i++)
                {
                    for (int j = 0; j < result.GetLength(1); j++)
                    {
                        result[i, j] = 0;
                        for (int k = 0; k < matrix1.GetLength(1); k++)
                            result[i, j] = result[i, j] + matrix1[i, k] * matrix2[k, j];
                    }
                }
            }
            return result;
        }
    }
}