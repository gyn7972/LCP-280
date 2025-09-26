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

        private Rotary Rotary;

        public Monitoring_Main() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<Rotary>("Rotary"))
        {

        }

        public Monitoring_Main(InputCassetteLifter cassetteLifter, InputFeeder ringTransfer, InputStage inputStage, Rotary rotary)
        {
            InitializeComponent();

            #region Chart
            CassetteLifter = cassetteLifter;
            Feeder = ringTransfer;
            Stage = inputStage;
            Rotary = rotary;

            var materialCassette = CassetteLifter?.GetMaterialCassette();

            // WaferSelectMapView 이벤트 구독
            if (inputWaferCarrierControl1?.GetWaferSelectMapView() != null)
            {
                inputWaferCarrierControl1.GetWaferSelectMapView().SlotClicked += OnInputWaferSlot_Clicked;
                inputWaferCarrierControl1.GetWaferSelectMapView().SlotSelectionChanged += OnInputWaferSlot_SelectionChanged;

                if (materialCassette != null)
                {
                    inputWaferCarrierControl1.GetWaferSelectMapView().SetMaterialCassette(materialCassette);
                }
                else
                {
                    // 테스트용 카세트 생성
                    inputWaferCarrierControl1.GetWaferSelectMapView().CreateTestCassette(20);
                }
            }

            if (outputWaferCarrierControl1?.GetWaferSelectMapView() != null)
            {
                outputWaferCarrierControl1.GetWaferSelectMapView().SlotClicked += OnInputWaferSlot_Clicked;
                outputWaferCarrierControl1.GetWaferSelectMapView().SlotSelectionChanged += OnInputWaferSlot_SelectionChanged;

                if (materialCassette != null)
                {
                    outputWaferCarrierControl1.GetWaferSelectMapView().SetMaterialCassette(materialCassette);
                }
                else
                {
                    // 테스트용 카세트 생성
                    outputWaferCarrierControl1.GetWaferSelectMapView().CreateTestCassette(20);
                }
            }
            #endregion

            // 이벤트 - Input Control
            dieInputControl1.MotorMoveRequested += OnDieInput_MotorMoveRequested;

            // 이벤트 - Select Control
            dieIndexSelectControl1.DieClicked += OnDieClick_Requested;
            dieIndexSelectControl1.RotationRequested += OnDieRotation_Requested;
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

        #region Wafer Select Map 이벤트 처리

        private void OnInputWaferSlot_Clicked(object sender, WaferSelectMapView.SlotClickedEventArgs e)
        {
            Console.WriteLine($"[WaferMap] Slot {e.SlotNumber} clicked. State: {e.State}");

            // 실제 웨이퍼 선택 로직
            // 예: 선택된 웨이퍼 정보를 다른 시스템에 전달
            HandleWaferSlotSelection(e.SlotNumber, e.State);
        }

        private void OnInputWaferSlot_SelectionChanged(object sender, WaferSelectMapView.SlotSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                Console.WriteLine($"[WaferMap] Slot {e.SlotNumber} selected with order {e.SelectionOrder}. State: {e.State}");

                // 웨이퍼 선택 시 처리
                OnWaferSlotSelected(e.SlotNumber, e.SelectionOrder, e.State);
            }
            else
            {
                Console.WriteLine($"[WaferMap] Slot {e.SlotNumber} deselected");

                // 웨이퍼 선택 해제 시 처리
                OnWaferSlotDeselected(e.SlotNumber);
            }

            // UI 상태 업데이트
            UpdateWaferSelectionStatus();
        }

        private void HandleWaferSlotSelection(int slotNumber, WaferSelectMapView.SlotDisplayState state)
        {
            // 실제 웨이퍼 슬롯 선택 시 비즈니스 로직
            switch (state)
            {
                case WaferSelectMapView.SlotDisplayState.Present:
                    // 웨이퍼가 있는 슬롯 선택 시 처리
                    Console.WriteLine($"웨이퍼가 있는 Slot {slotNumber} 처리");
                    break;

                case WaferSelectMapView.SlotDisplayState.Empty:
                    // 빈 슬롯 선택 시 처리
                    Console.WriteLine($"빈 Slot {slotNumber} 처리");
                    break;
            }
        }

        private void OnWaferSlotSelected(int slotNumber, int selectionOrder, WaferSelectMapView.SlotDisplayState state)
        {
            // 웨이퍼 선택 시 실제 장비 제어 로직

            // 예: 선택 순서에 따른 처리 계획 수립
            Console.WriteLine($"웨이퍼 처리 순서 {selectionOrder}: Slot {slotNumber}");

            // 실제 장비 연동
            // 예: CassetteLifter?.PrepareSlot(slotNumber);

            // 상태 업데이트
            UpdateStatusInfo($"Slot {slotNumber} 선택됨 (순서: {selectionOrder})");
        }

        private void OnWaferSlotDeselected(int slotNumber)
        {
            // 웨이퍼 선택 해제 시 처리
            Console.WriteLine($"Slot {slotNumber} 선택 해제");

            // 실제 장비 연동
            // 예: CassetteLifter?.CancelSlotPreparation(slotNumber);

            // 상태 업데이트
            UpdateStatusInfo($"Slot {slotNumber} 선택 해제됨");
        }

        private void UpdateWaferSelectionStatus()
        {
            var waferMapView = inputWaferCarrierControl1?.GetWaferSelectMapView();
            if (waferMapView != null)
            {
                var selectedSlots = waferMapView.GetSelectedSlotsInOrder();
                var selectedCount = waferMapView.GetSelectedCount();

                Console.WriteLine($"현재 선택된 웨이퍼: {selectedCount}개");
                Console.WriteLine($"처리 순서: {string.Join(" → ", selectedSlots.Select(s => $"Slot{s}"))}");

                // 실제 UI 상태 업데이트
                // 예: statusLabel.Text = $"선택된 웨이퍼: {selectedCount}개";
            }
        }

        #endregion

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

        private void OnDieRotation_Requested(object sender, int rotationOffset)
        {
            // 실제 회전 처리 로직
            // 예: 회전 테이블 제어
            // RotationTable?.RotateToPosition(rotationOffset);

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "다음 소켓으로 구동 하시겠습니까?") != DialogResult.Yes)
                return;

            Rotary.MovePositionRotate();

            // 회전 완료 후 상태 업데이트
            UpdateRotationStatus(Rotary.GetLoadIndexNo() + 1);
        }

        private void UpdateRotationStatus(int offset)
        {
            // 상태 표시 업데이트
            Console.WriteLine($"로더 위치는 '{offset}'입니다.");

            dieIndexSelectControl1.UpdateRotationUI(offset);
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
