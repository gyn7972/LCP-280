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
using static QMC.Common.Material;

namespace QMC.LCP_280.Process
{
    [FormOrder(2)]
    public partial class Monitoring_Main : Form
    {
        private InputCassetteLifter InputCassetteLifter { get; set; }
        private InputFeeder Feeder { get; set; }
        private InputStage inputStage { get; set; }

        private Rotary Rotary;

        private OutputStage OutputStage { get; set; }
        private OutputCassetteLifter OutputCassetteLifter { get; set; }


        public Monitoring_Main() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<Rotary>("Rotary"),
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter"))
        {

        }

        public Monitoring_Main(InputCassetteLifter inputcassetteLifter, InputFeeder ringTransfer,
            InputStage inputStage, Rotary rotary, OutputStage outputStage, OutputCassetteLifter outputCassetteLifter)
        {
            InitializeComponent();

            #region Chart
            InputCassetteLifter = inputcassetteLifter;
            Feeder = ringTransfer;
            this.inputStage = inputStage;
            Rotary = rotary;
            OutputStage = outputStage;
            OutputCassetteLifter = outputCassetteLifter;

            var materialCassette = InputCassetteLifter?.GetMaterialCassette();

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
            //dieIndexSelectControl1.DieClicked += OnDieClick_Requested;
            dieIndexSelectControl1.RotationRequested += OnDieRotation_Requested;
            inputStage.EventUpdateUIWafer += InputStage_EventUpdateUIWafer;
            outputStage.EventUpdateUIWafer += OutputStage_EventUpdateUIWafer;

            InputCassetteLifter.EventUpdateUICassette += InputCassetteLifter_EventUpdateUICassette;
            OutputCassetteLifter.EventUpdateUICassette += OutputCassetteLifter_EventUpdateUICassette;

        }

        private void OutputCassetteLifter_EventUpdateUICassette(MaterialCassette Cassette)
        {
            this.outputWaferCarrierControl1.GetWaferSelectMapView()?.SetMaterialCassette(Cassette);
            //this.outputWaferCarrierControl1.GetWaferSelectMapView()?.Refresh();

            this.outputWaferCarrierControl1.SetWaferCarrierId(Cassette.CarrierId);
            this.outputWaferCarrierControl1.UpdateWaferCount(Cassette.SlotCount);
        }

        private void InputCassetteLifter_EventUpdateUICassette(MaterialCassette Cassette)
        {
            this.inputWaferCarrierControl1.GetWaferSelectMapView()?.SetMaterialCassette(Cassette);
            //this.inputWaferCarrierControl1.GetWaferSelectMapView()?.Refresh();

            this.inputWaferCarrierControl1.SetWaferCarrierId(Cassette.CarrierId);
            this.inputWaferCarrierControl1.UpdateWaferCount(Cassette.SlotCount);
        }

        private void OutputStage_EventUpdateUIWafer(MaterialWafer wafer)
        {
            this.dieOutputControl1.SetDieList(wafer.Dies);
        }

        private void InputStage_EventUpdateUIWafer(MaterialWafer wafer)
        {
            this.dieInputControl1.SetDieList(wafer.Dies);
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
        //private void OnDieClick_Requested(object sender, DieIndexSelectControl.Die e)
        //{
        //    Console.WriteLine($"[Select] Die Num: {e.Number}");
        //}

        private void OnDieRotation_Requested(object sender, int rotationOffset)
        {
            // 실제 회전 처리 로직
            // 예: 회전 테이블 제어
            // RotationTable?.RotateToPosition(rotationOffset);

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "다음 소켓으로 구동 하시겠습니까?") != DialogResult.Yes)
                return;

            int nRet = Rotary.MovePositionRotate();
            if (nRet != 0)
            {
                MessageBox.Show("로터리 모터 구동 실패 인터락을 확인 하세요.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
                        dieInputControl1.UpdateChip(new Point(x, y), DieProcessState.Picked);
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
                        dieOutputControl1.UpdateDie(new Point(x, y), DieProcessState.Picked);
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

            var chips = new List<MaterialDie>();
            int idx = 0;
            int radius = 50;
            int targetCount = 10000;
            double area = Math.PI * radius * radius;
            double dieArea = area / targetCount;
            double step = Math.Sqrt(dieArea);
            for (double x = -radius; x <= radius; x += step)
            {
                for (double y = -radius; y <= radius; y += step)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        chips.Add(new MaterialDie
                        {
                            Index = idx++,
                            MapX = (int)Math.Round(x),
                            MapY = (int)Math.Round(y),
                            State = DieProcessState.Mapped,
                            Presence = MaterialPresence.Exist,
                        });
                    }
                }
            }
            Console.WriteLine($"총 칩 개수 = {chips.Count}");
            dieInputControl1.SetDieList(chips);
            dieInputControl1.UpdateChip(new Point(0, 0), DieProcessState.Picked);
            #endregion

            #region Output Control - Square Shape
            // 기존 출력 컨트롤 로직은 그대로 (DieOutputControl 은 별도 변환 필요 시 추후 적용)
            #endregion

            #region InputWaferCarrierControl
            inputWaferCarrierControl1.SetWaferCarrierId("1234");
            inputWaferCarrierControl1.UpdateWaferCount(2);
            #endregion

            #region OutputWaferCarrierControl
            outputWaferCarrierControl1.SetWaferCarrierId("5678");
            outputWaferCarrierControl1.UpdateWaferCount(3);
            #endregion

            if (Rotary != null)
            {
                dieIndexSelectControl1.BindRotary(Rotary);
            }
        }

        private void Monitoring_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Rotary != null)
            {
                Rotary.LoadIndexChanged -= dieIndexSelectControl1.Rotary_LoadIndexChanged; // 내부 메서드가 private이면 래퍼 필요
            }
        }




    }
}
