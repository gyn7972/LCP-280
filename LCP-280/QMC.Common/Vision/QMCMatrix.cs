
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace QMC.Common.Vision
{
    public class QMCMatrix
    {
        public int col;
        public int row;
        public double[,] m_dMatrix;
        const double EPS = 0.000000000000001;
        public QMCMatrix(int row, int col)
        {
            this.row = row;
            this.col = col;
            m_dMatrix = new double[row, col];
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    m_dMatrix[i, j] = 0;
                }
            }
        }

        public QMCMatrix Matrix_inv()
        {
            QMCMatrix qMCMatrix = new QMCMatrix(this.row, this.col);
            QMCMatrix qMCMatrixN = new QMCMatrix(this.row, this.col * 2);
            int iter, i, j, k;

            double v;

            double tmp;
            int max_key;

            if (this.col != this.row)
                return qMCMatrix;

            iter = this.row;

            // copy it
            for (j = 0; j < iter; j++)
                for (i = 0; i < iter; i++)
                    qMCMatrixN.m_dMatrix[j, i] = this.m_dMatrix[j, i];

            // insert identity matrix
            for (i = 0; i < iter; i++)
                qMCMatrixN.m_dMatrix[i, i + iter] = 1.0;

            // start gauss elimination
            for (i = 0; i < iter; i++)
            {

                // find max
                max_key = i;
                for (j = i + 1; j < iter; j++)
                    if (qMCMatrixN.m_dMatrix[j, i] > qMCMatrixN.m_dMatrix[max_key, i])
                        max_key = j;

                // swap with current row
                if (max_key != i)
                {
                    for (j = 0; j < iter * 2; j++)
                    {
                        tmp = qMCMatrixN.m_dMatrix[i, j];
                        qMCMatrixN.m_dMatrix[i, j] = qMCMatrixN.m_dMatrix[max_key, j];
                        qMCMatrixN.m_dMatrix[max_key, j] = tmp;
                    }
                }

                // normalize
                v = qMCMatrixN.m_dMatrix[i, i];
                for (j = i + 1; j < iter * 2; j++)
                    qMCMatrixN.m_dMatrix[i, j] /= v + EPS;

                for (j = i + 1; j < iter; j++)
                {
                    v = qMCMatrixN.m_dMatrix[j, i];
                    qMCMatrixN.m_dMatrix[j, i] = 0.0;
                    for (k = i + 1; k < iter * 2; k++)
                    {
                        qMCMatrixN.m_dMatrix[j, k] -= qMCMatrixN.m_dMatrix[i, k] * v;
                    }
                }

            }
            for (i = iter - 2; i >= 0; i--)
            {

                for (j = i; j >= 0; j--)
                {
                    v = qMCMatrixN.m_dMatrix[j, i + 1];
                    for (k = 0; k < iter * 2; k++)
                    {
                        qMCMatrixN.m_dMatrix[j, k] -= qMCMatrixN.m_dMatrix[i + 1, k] * v;
                    }
                }
            }

            // copy it
            for (j = 0; j < iter; j++)
                for (i = 0; i < iter; i++)
                    qMCMatrix.m_dMatrix[j, i] = qMCMatrixN.m_dMatrix[j, i + iter];



            return qMCMatrix;

        }
        public QMCMatrix Matrix_Multi(QMCMatrix qMCTarget)
        {
            QMCMatrix qMCMatrix = new QMCMatrix(this.row, this.col);


            int col, row, iter;
            int i, j, k;

            if (this.col != qMCTarget.row)
                return qMCMatrix;

            row = this.row;
            col = qMCTarget.col;

            iter = this.col;


            for (j = 0; j < row; j++)
            {
                for (i = 0; i < col; i++)
                {
                    for (k = 0; k < iter; k++)
                    {
                        qMCMatrix.m_dMatrix[j, i] += this.m_dMatrix[j, k] * qMCTarget.m_dMatrix[k, i];
                    }
                }
            }



            return qMCMatrix;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < this.row; i++)
            {
                for (int j = 0; j < this.col; j++)
                {
                    stringBuilder.AppendFormat("{0}\t", m_dMatrix[i, j]);

                }
                stringBuilder.AppendFormat("\n");
            }

            return stringBuilder.ToString();
        }

    }

    public class PerspectiveProjection
    {
        public double m_dOffsetX = 300;
        public double m_dOffsetY = 300;

        public QMCMatrix projection_matrix(XyCoordinateCollection coordinateSourceOrg, XyCoordinateCollection coordinateTargetOrg)
        {
            for (int i = 0; i < coordinateSourceOrg.Count; i++)
            {
                XyCoordinate v = coordinateSourceOrg[i];
                v.X = v.X + m_dOffsetX;
                v.Y = v.Y + m_dOffsetY;
                coordinateSourceOrg[i] = v;
            }

            for (int i = 0; i < coordinateTargetOrg.Count; i++)
            {
                XyCoordinate v = coordinateTargetOrg[i];
                v.X = v.X + m_dOffsetX;
                v.Y = v.Y + m_dOffsetY;
                coordinateTargetOrg[i] = v;
            }

            QMCMatrix qMCMatrixA = new QMCMatrix(8, 8);
            QMCMatrix qMCMatrixB = new QMCMatrix(8, 1);
            QMCMatrix qMCMatrixC = new QMCMatrix(8, 8);
            QMCMatrix qMCMatrixProjection = new QMCMatrix(3, 3);

            if (coordinateSourceOrg != null && coordinateTargetOrg != null)
            {
                if (coordinateSourceOrg.Count == coordinateTargetOrg.Count)
                {
                    for (int iter = 0; iter < coordinateSourceOrg.Count; iter++)
                    {
                        qMCMatrixA.m_dMatrix[iter, 0] = coordinateSourceOrg[iter].X;//[0][0]  a->var[0][0] = x[0];
                        qMCMatrixA.m_dMatrix[iter, 1] = coordinateSourceOrg[iter].Y;//[0][1]	a->var[0][1] = y[0];
                        qMCMatrixA.m_dMatrix[iter, 2] = 1;//[0][2]  a->var[0][2] = 1.0;
                        qMCMatrixA.m_dMatrix[iter, 3] = 0;//[0][3]
                        qMCMatrixA.m_dMatrix[iter, 4] = 0;//[0][4]
                        qMCMatrixA.m_dMatrix[iter, 5] = 0;//[0][5]
                        qMCMatrixA.m_dMatrix[iter, 6] = -1 * coordinateTargetOrg[iter].X * coordinateSourceOrg[iter].X;//[0][6]  a->var[0][6] = -1 * _x[0] * x[0];
                        qMCMatrixA.m_dMatrix[iter, 7] = -1 * coordinateTargetOrg[iter].X * coordinateSourceOrg[iter].Y;//[0][7]  a->var[0][7] = -1 * _x[0] * y[0];

                    }

                    for (int iter = 0; iter < coordinateSourceOrg.Count; iter++)
                    {
                        qMCMatrixA.m_dMatrix[iter + 4, 0] = 0;//[0][0]  a->var[0][0] = x[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 1] = 0;//[0][1]	a->var[0][1] = y[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 2] = 0;//[0][2]  a->var[0][2] = 1.0;
                        qMCMatrixA.m_dMatrix[iter + 4, 3] = coordinateSourceOrg[iter].X;//[0][3]
                        qMCMatrixA.m_dMatrix[iter + 4, 4] = coordinateSourceOrg[iter].Y;//[0][4]
                        qMCMatrixA.m_dMatrix[iter + 4, 5] = 1;//[0][5]
                        qMCMatrixA.m_dMatrix[iter + 4, 6] = -1 * coordinateSourceOrg[iter].X * coordinateTargetOrg[iter].Y;//[0][6]  a->var[0][6] = -1 * _x[0] * x[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 7] = -1 * coordinateSourceOrg[iter].Y * coordinateTargetOrg[iter].Y;//[0][7]  a->var[0][7] = -1 * _x[0] * y[0];

                    }
                    for (int iter = 0; iter < coordinateTargetOrg.Count; iter++)
                    {
                        qMCMatrixB.m_dMatrix[iter, 0] = coordinateTargetOrg[iter].X;

                    }
                    for (int iter = 0; iter < coordinateTargetOrg.Count; iter++)
                    {
                        qMCMatrixB.m_dMatrix[iter + 4, 0] = coordinateTargetOrg[iter].Y;

                    }


                    QMCMatrix qMCMatrix_inv;
                    qMCMatrix_inv = qMCMatrixA.Matrix_inv();
                    qMCMatrixC = qMCMatrix_inv.Matrix_Multi(qMCMatrixB);


                    qMCMatrixProjection.m_dMatrix[0, 0] = qMCMatrixC.m_dMatrix[0, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[0, 1] = qMCMatrixC.m_dMatrix[1, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[0, 2] = qMCMatrixC.m_dMatrix[2, 0];//[0][2]

                    qMCMatrixProjection.m_dMatrix[1, 0] = qMCMatrixC.m_dMatrix[3, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[1, 1] = qMCMatrixC.m_dMatrix[4, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[1, 2] = qMCMatrixC.m_dMatrix[5, 0];//[0][2]

                    qMCMatrixProjection.m_dMatrix[2, 0] = qMCMatrixC.m_dMatrix[6, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[2, 1] = qMCMatrixC.m_dMatrix[7, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[2, 2] = 1;//[0][2]

                }
            }


            return qMCMatrixProjection;
        }
        public QMCMatrix projection_matrix(XytCoordinateCollection coordinateSourceOrg, XytCoordinateCollection coordinateTargetOrg)
        {
            XytCoordinateCollection coordinateSource = new XytCoordinateCollection();
            XytCoordinateCollection coordinateTarget = new XytCoordinateCollection();
            foreach (var v in coordinateSourceOrg)
            {
                coordinateSource.Add(new XytCoordinate(v.X + m_dOffsetX, v.Y + m_dOffsetY, v.T));
            }

            foreach (var v in coordinateTargetOrg)
            {
                coordinateTarget.Add(new XytCoordinate(v.X + m_dOffsetX, v.Y + m_dOffsetY, v.T));
            }
            QMCMatrix qMCMatrixA = new QMCMatrix(8, 8);
            QMCMatrix qMCMatrixB = new QMCMatrix(8, 1);
            QMCMatrix qMCMatrixC = new QMCMatrix(8, 8);
            QMCMatrix qMCMatrixProjection = new QMCMatrix(3, 3);

            if (coordinateSource != null && coordinateTarget != null)
            {
                if (coordinateSource.Count == coordinateTarget.Count)
                {
                    for (int iter = 0; iter < coordinateSource.Count; iter++)
                    {
                        qMCMatrixA.m_dMatrix[iter, 0] = coordinateSource[iter].X;//[0][0]  a->var[0][0] = x[0];
                        qMCMatrixA.m_dMatrix[iter, 1] = coordinateSource[iter].Y;//[0][1]	a->var[0][1] = y[0];
                        qMCMatrixA.m_dMatrix[iter, 2] = 1;//[0][2]  a->var[0][2] = 1.0;
                        qMCMatrixA.m_dMatrix[iter, 3] = 0;//[0][3]
                        qMCMatrixA.m_dMatrix[iter, 4] = 0;//[0][4]
                        qMCMatrixA.m_dMatrix[iter, 5] = 0;//[0][5]
                        qMCMatrixA.m_dMatrix[iter, 6] = -1 * coordinateTarget[iter].X * coordinateSource[iter].X;//[0][6]  a->var[0][6] = -1 * _x[0] * x[0];
                        qMCMatrixA.m_dMatrix[iter, 7] = -1 * coordinateTarget[iter].X * coordinateSource[iter].Y;//[0][7]  a->var[0][7] = -1 * _x[0] * y[0];

                    }

                    for (int iter = 0; iter < coordinateSource.Count; iter++)
                    {
                        qMCMatrixA.m_dMatrix[iter + 4, 0] = 0;//[0][0]  a->var[0][0] = x[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 1] = 0;//[0][1]	a->var[0][1] = y[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 2] = 0;//[0][2]  a->var[0][2] = 1.0;
                        qMCMatrixA.m_dMatrix[iter + 4, 3] = coordinateSource[iter].X;//[0][3]
                        qMCMatrixA.m_dMatrix[iter + 4, 4] = coordinateSource[iter].Y;//[0][4]
                        qMCMatrixA.m_dMatrix[iter + 4, 5] = 1;//[0][5]
                        qMCMatrixA.m_dMatrix[iter + 4, 6] = -1 * coordinateSource[iter].X * coordinateTarget[iter].Y;//[0][6]  a->var[0][6] = -1 * _x[0] * x[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 7] = -1 * coordinateSource[iter].Y * coordinateTarget[iter].Y;//[0][7]  a->var[0][7] = -1 * _x[0] * y[0];

                    }
                    for (int iter = 0; iter < coordinateTarget.Count; iter++)
                    {
                        qMCMatrixB.m_dMatrix[iter, 0] = coordinateTarget[iter].X;

                    }
                    for (int iter = 0; iter < coordinateTarget.Count; iter++)
                    {
                        qMCMatrixB.m_dMatrix[iter + 4, 0] = coordinateTarget[iter].Y;

                    }


                    QMCMatrix qMCMatrix_inv;
                    qMCMatrix_inv = qMCMatrixA.Matrix_inv();
                    qMCMatrixC = qMCMatrix_inv.Matrix_Multi(qMCMatrixB);


                    qMCMatrixProjection.m_dMatrix[0, 0] = qMCMatrixC.m_dMatrix[0, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[0, 1] = qMCMatrixC.m_dMatrix[1, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[0, 2] = qMCMatrixC.m_dMatrix[2, 0];//[0][2]

                    qMCMatrixProjection.m_dMatrix[1, 0] = qMCMatrixC.m_dMatrix[3, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[1, 1] = qMCMatrixC.m_dMatrix[4, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[1, 2] = qMCMatrixC.m_dMatrix[5, 0];//[0][2]

                    qMCMatrixProjection.m_dMatrix[2, 0] = qMCMatrixC.m_dMatrix[6, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[2, 1] = qMCMatrixC.m_dMatrix[7, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[2, 2] = 1;//[0][2]

                }
            }


            return qMCMatrixProjection;
        }
        public QMCMatrix projection_matrix(List<PointF> ptSource, List<PointF> ptTarget)
        {

            QMCMatrix qMCMatrixA = new QMCMatrix(8, 8);
            QMCMatrix qMCMatrixB = new QMCMatrix(8, 1);
            QMCMatrix qMCMatrixC = new QMCMatrix(8, 8);
            QMCMatrix qMCMatrixProjection = new QMCMatrix(3, 3);

            if (ptSource != null && ptTarget != null)
            {
                if (ptSource.Count == ptTarget.Count)
                {
                    for (int iter = 0; iter < ptSource.Count; iter++)
                    {
                        qMCMatrixA.m_dMatrix[iter, 0] = ptSource[iter].X;//[0][0]  a->var[0][0] = x[0];
                        qMCMatrixA.m_dMatrix[iter, 1] = ptSource[iter].Y;//[0][1]	a->var[0][1] = y[0];
                        qMCMatrixA.m_dMatrix[iter, 2] = 1;//[0][2]  a->var[0][2] = 1.0;
                        qMCMatrixA.m_dMatrix[iter, 3] = 0;//[0][3]
                        qMCMatrixA.m_dMatrix[iter, 4] = 0;//[0][4]
                        qMCMatrixA.m_dMatrix[iter, 5] = 0;//[0][5]
                        qMCMatrixA.m_dMatrix[iter, 6] = -1 * ptTarget[iter].X * ptSource[iter].X;//[0][6]  a->var[0][6] = -1 * _x[0] * x[0];
                        qMCMatrixA.m_dMatrix[iter, 7] = -1 * ptTarget[iter].X * ptSource[iter].Y;//[0][7]  a->var[0][7] = -1 * _x[0] * y[0];

                    }

                    for (int iter = 0; iter < ptSource.Count; iter++)
                    {
                        qMCMatrixA.m_dMatrix[iter + 4, 0] = 0;//[0][0]  a->var[0][0] = x[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 1] = 0;//[0][1]	a->var[0][1] = y[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 2] = 0;//[0][2]  a->var[0][2] = 1.0;
                        qMCMatrixA.m_dMatrix[iter + 4, 3] = ptSource[iter].X;//[0][3]
                        qMCMatrixA.m_dMatrix[iter + 4, 4] = ptSource[iter].Y;//[0][4]
                        qMCMatrixA.m_dMatrix[iter + 4, 5] = 1;//[0][5]
                        qMCMatrixA.m_dMatrix[iter + 4, 6] = -1 * ptSource[iter].X * ptTarget[iter].Y;//[0][6]  a->var[0][6] = -1 * _x[0] * x[0];
                        qMCMatrixA.m_dMatrix[iter + 4, 7] = -1 * ptSource[iter].Y * ptTarget[iter].Y;//[0][7]  a->var[0][7] = -1 * _x[0] * y[0];

                    }
                    for (int iter = 0; iter < ptTarget.Count; iter++)
                    {
                        qMCMatrixB.m_dMatrix[iter, 0] = ptTarget[iter].X;

                    }
                    for (int iter = 0; iter < ptTarget.Count; iter++)
                    {
                        qMCMatrixB.m_dMatrix[iter + 4, 0] = ptTarget[iter].Y;

                    }


                    QMCMatrix qMCMatrix_inv;
                    qMCMatrix_inv = qMCMatrixA.Matrix_inv();
                    qMCMatrixC = qMCMatrix_inv.Matrix_Multi(qMCMatrixB);


                    qMCMatrixProjection.m_dMatrix[0, 0] = qMCMatrixC.m_dMatrix[0, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[0, 1] = qMCMatrixC.m_dMatrix[1, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[0, 2] = qMCMatrixC.m_dMatrix[2, 0];//[0][2]

                    qMCMatrixProjection.m_dMatrix[1, 0] = qMCMatrixC.m_dMatrix[3, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[1, 1] = qMCMatrixC.m_dMatrix[4, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[1, 2] = qMCMatrixC.m_dMatrix[5, 0];//[0][2]

                    qMCMatrixProjection.m_dMatrix[2, 0] = qMCMatrixC.m_dMatrix[6, 0];//[0][0]
                    qMCMatrixProjection.m_dMatrix[2, 1] = qMCMatrixC.m_dMatrix[7, 0];//[0][1]
                    qMCMatrixProjection.m_dMatrix[2, 2] = 1;//[0][2]

                }
            }


            return qMCMatrixProjection;
        }

        public XyCoordinate GetPerspectiveProjectionPoint(XyCoordinate xyCoordinate, QMCMatrix qMCMatrix)
        {
            PointF point = GetPerspectiveProjectionPoint(xyCoordinate.X, xyCoordinate.Y, qMCMatrix);
            XyCoordinate coordinate = new XyCoordinate(point.X, point.Y);
            return coordinate;
        }
        public PointF GetPerspectiveProjectionPoint(double dX, double dY, QMCMatrix qMCMatrix)
        {
            PointF point = new PointF();
            dX += m_dOffsetX;
            dY += m_dOffsetY;

            double W = qMCMatrix.m_dMatrix[2, 0] * dX + qMCMatrix.m_dMatrix[2, 1] * dY + qMCMatrix.m_dMatrix[2, 2];

            point.X = (float)((qMCMatrix.m_dMatrix[0, 0] * dX + qMCMatrix.m_dMatrix[0, 1] * dY + qMCMatrix.m_dMatrix[0, 2]) / W - m_dOffsetX);
            point.Y = (float)((qMCMatrix.m_dMatrix[1, 0] * dX + qMCMatrix.m_dMatrix[1, 1] * dY + qMCMatrix.m_dMatrix[1, 2]) / W - m_dOffsetY);

            return point;
        }
        public void fwarping(ref byte[] dst, byte[] src, QMCMatrix matrix, int w, int h)
        {

            int i, j, x, y;
            double W;

            if (matrix.row != 3 || matrix.col != 3)
                return;
            for (j = 0; j < h; j++)
            {
                for (i = 0; i < w; i++)
                {
                    dst[j * w + i] = 255;
                }
            }
            for (j = 0; j < h; j++)
            {
                for (i = 0; i < w; i++)
                {
                    PointF pt = GetPerspectiveProjectionPoint(i, j, matrix);

                    //W = matrix.m_dMatrix[2,0] * i + matrix.m_dMatrix[2,1] * j + matrix.m_dMatrix[2,2];

                    //x = (int)((matrix.m_dMatrix[0,0] * i + matrix.m_dMatrix[0,1] * j + matrix.m_dMatrix[0,2]) / W);
                    //y = (int)((matrix.m_dMatrix[1,0] * i + matrix.m_dMatrix[1,1] * j + matrix.m_dMatrix[1,2]) / W);
                    x = (int)pt.X;
                    y = (int)pt.Y;
                    if (x >= w || y >= h || x < 0 || y < 0)
                    {
                        continue;
                    }


                    dst[y * w + x] = src[(j * w) + i];
                }
            }

        }
    }
}
