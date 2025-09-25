using QMC.Common;
using QMC.Common.Controls;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit;
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
    [FormOrder(1)]
    public partial class Monitoring_Main : Form
    {
        private InputCassetteLifter CassetteLifter { get; set; }
        private InputFeeder Feeder { get; set; }
        private InputStage Stage { get; set; }

        public Monitoring_Main() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage")
            )
        {

        }

        public Monitoring_Main(InputCassetteLifter cassetteLifter, InputFeeder ringTransfer, InputStage inputStage)
        {
            InitializeComponent();

            #region Chart
            CassetteLifter = cassetteLifter;
            Feeder = ringTransfer;
            Stage = inputStage;

            var materialCassette = CassetteLifter.GetMaterialCassette();

            inputWaferCarrierControl1.GetWaferMapView().SetMaterialCassette(materialCassette);
            outputWaferCarrierControl1.GetWaferMapView().SetMaterialCassette(materialCassette);
            #endregion

            // 이벤트 - Input Control
            dieInputControl1.MotorMoveRequested += OnDieInput_MotorMoveRequested;

            // 이벤트 - Select Control
            dieIndexSelectControl1.DieClicked += OnDieClick_Requested;
        }

        private static T TryGetUnit<T>(string unitName) where T : class
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq?.Units != null && eq.Units.TryGetValue(unitName, out var u))
                    return u as T;
            }
            catch { }
            return null;

        }

        #region Input Die 이벤트 처리
        private void OnDieInput_MotorMoveRequested(object sender, DisplayView.DisplayItemEventArgs e)
        {
            Console.WriteLine($"[Input] 모터 이동 요청: X={e.Item.Position.X}, Y={e.Item.Position.Y}");

            // 실제 모터 제어 로직
            MovePickMotorTo(e.Item.Position.X, e.Item.Position.Y);

            // UI 업데이트
            ShowMotorMovingStatus($"Input 모터가 ({e.Item.Position.X}, {e.Item.Position.Y})로 이동 중...");
        }
        #endregion

        #region 이벤트 처리
        private void OnDieClick_Requested(object sender, DieIndexSelectControl.Die e)
        {
            Console.WriteLine($"[Select] Die Num: {e.Number}");
        }

        #endregion

        #region 헬퍼 메서드
        private void UpdateStatusInfo(string message)
        {
            // 상태바나 정보 패널 업데이트
            // statusStrip1.Text = message; // 예시
            Console.WriteLine($"[Status] {message}");
        }

        private void ShowMotorMovingStatus(string message)
        {
            // 모터 이동 상태 표시
            // progressBar1.Visible = true; // 예시
            Console.WriteLine($"[Motor] {message}");
        }

        private void MovePickMotorTo(int x, int y)
        {
            try
            {
                // 실제 Pick 모터 제어 코드
                // Stage?.MoveToPosition(x, y);
                Console.WriteLine($"Pick 모터 이동: ({x}, {y})");

                // 이동 완료 후 다이 상태 업데이트
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                {
                    this.Invoke(new Action(() =>
                    {
                        dieInputControl1.UpdateDie(new Point(x, y), DieInputControl.DieState.Picked);
                        ShowMotorMovingStatus("Pick 모터 이동 완료");
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"모터 이동 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MovePlaceMotorTo(int x, int y)
        {
            try
            {
                // 실제 Place 모터 제어 코드
                Console.WriteLine($"Place 모터 이동: ({x}, {y})");

                // 이동 완료 후 다이 상태 업데이트
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                {
                    this.Invoke(new Action(() =>
                    {
                        dieOutputControl1.UpdateDie(new Point(x, y), DieOutputControl.DieState.Present);
                        ShowMotorMovingStatus("Place 모터 이동 완료");
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"모터 이동 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

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

            #region Output Control - Square Shape
            dieOutputControl1.SetWaferId("WAFER 098123");

            var dies_Output = new List<DieOutputControl.Die>();
            idx = 0;

            // 사각형 크기 설정
            int squareSize = 50; // -50 ~ +50 범위 (100x100 사각형)

            // 원하는 개수 근사치
            targetCount = 10000;

            // 격자 step 계산 (사각형 면적 / targetCount = 1칩당 면적)
            area = (squareSize * 2) * (squareSize * 2);  // 100 * 100 = 10000
            dieArea = area / targetCount;                 // 1.0
            step = Math.Sqrt(dieArea);                    // 1.0

            Console.WriteLine($"Square area: {area}, step: {step}");

            for (double x = -squareSize; x <= squareSize; x += step)
            {
                for (double y = -squareSize; y <= squareSize; y += step)
                {
                    // 사각형 영역 내부 조건 (원형 조건 제거)
                    dies_Output.Add(new DieOutputControl.Die
                    {
                        Index = idx++,
                        Position = new Point((int)Math.Round(x), (int)Math.Round(y)),
                        State = DieOutputControl.DieState.Empty
                    });
                }
            }

            // 디버그 출력
            Console.WriteLine($"총 칩 개수 (Square) = {dies_Output.Count}");

            dieOutputControl1.SetDieList(dies_Output);

            // 특정 칩 상태 변경 (중심 칩)
            dieOutputControl1.UpdateDie(new Point(0, 0), DieOutputControl.DieState.Present);
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
