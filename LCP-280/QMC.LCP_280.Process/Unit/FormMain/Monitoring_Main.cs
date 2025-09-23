using QMC.LCP_280.Process.Unit.FormMain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    public partial class Monitoring_Main : Form
    {
        public Monitoring_Main()
        {
            InitializeComponent();
        }

        private void Monitoring_Main_Load(object sender, EventArgs e)
        {
            #region Input Control
            dieInputControl1.SetWaferId("WAFER 098123");

            var dies = new List<DieInputControl.Die>();
            int idx = 0;

            int radius = 50;

            // 원하는 개수 근사치
            int targetCount = 10000;

            // 격자 step 계산 (원의 면적 / targetCount = 1칩당 면적)
            double area = Math.PI * radius * radius;    // ≈ 7853
            double dieArea = area / targetCount;        // ≈ 0.785
            double step = Math.Sqrt(dieArea);           // ≈ 0.89

            for (double x = -radius; x <= radius; x += step)
            {
                for (double y = -radius; y <= radius; y += step)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        dies.Add(new DieInputControl.Die
                        {
                            Index = idx++,
                            Position = new Point((int)Math.Round(x), (int)Math.Round(y)),
                            State = DieInputControl.DieState.Present
                        });
                    }
                }
            }

            // 디버그 출력
            Console.WriteLine($"총 칩 개수 = {dies.Count}");
            
            
            dieInputControl1.SetDieList(dies);


            //var dies = new List<DieInputControl.Die>();
            //int idx = 0;
            //for (int x = -50; x <= 50; x += 5)
            //{
            //    for (int y = -50; y <= 50; y += 5)
            //    {
            //        if (x * x + y * y <= 50 * 50) // 원형 영역 안에 있는 칩만
            //        {
            //            dies.Add(new DieInputControl.Die
            //            {
            //                Index = idx++,
            //                Position = new Point(x, y),
            //                State = DieInputControl.DieState.Present
            //            });
            //        }
            //    }
            //}
            //dieInputControl1.SetDieList(dies);

            // 특정 칩 상태 변경 (중심 칩 가져감)
            dieInputControl1.UpdateDie(new Point(0, 0), DieInputControl.DieState.Picked);
            #endregion

            #region Output Control
            dieOutputControl1.SetWaferId("WAFER 098123");

            var dies_Output = new List<DieOutputControl.Die>();
            idx = 0;
            
            radius = 50;

            // 원하는 개수 근사치
            targetCount = 10000;

            // 격자 step 계산 (원의 면적 / targetCount = 1칩당 면적)
            area = Math.PI * radius * radius;    // ≈ 7853
            dieArea = area / targetCount;        // ≈ 0.785
            step = Math.Sqrt(dieArea);           // ≈ 0.89

            for (double x = -radius; x <= radius; x += step)
            {
                for (double y = -radius; y <= radius; y += step)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        dies_Output.Add(new DieOutputControl.Die
                        {
                            Index = idx++,
                            Position = new Point((int)Math.Round(x), (int)Math.Round(y)),
                            State = DieOutputControl.DieState.Empty
                        });
                    }
                }
            }

            // 디버그 출력
            Console.WriteLine($"총 칩 개수 = {dies_Output.Count}");


            dieOutputControl1.SetDieList(dies_Output);


            //var dies = new List<DieInputControl.Die>();
            //int idx = 0;
            //for (int x = -50; x <= 50; x += 5)
            //{
            //    for (int y = -50; y <= 50; y += 5)
            //    {
            //        if (x * x + y * y <= 50 * 50) // 원형 영역 안에 있는 칩만
            //        {
            //            dies.Add(new DieInputControl.Die
            //            {
            //                Index = idx++,
            //                Position = new Point(x, y),
            //                State = DieInputControl.DieState.Present
            //            });
            //        }
            //    }
            //}
            //dieOutputControl1.SetDieList(dies);

            // 특정 칩 상태 변경 (중심 칩 가져감)
            dieOutputControl1.UpdateDie(new Point(0, 0), DieOutputControl.DieState.Picked);
            #endregion


            #region InputWaferCarrierControl
            inputWaferCarrierControl1.SetWaferCarrierId("1234");
            inputWaferCarrierControl1.UpdateWaferCount(2);
            #endregion

            #region OutputWaferCarrierControl
            outputWaferCarrierControl1.SetWaferCarrierId("5678");
            outputWaferCarrierControl1.UpdateWaferCount(3);
            #endregion
        }
    }
}
