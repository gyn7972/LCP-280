using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.Controls;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.UI;
using QMC.Common.Unit;
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;


namespace QMC.LCP_280.Process
{
    [FormOrder(1)]
    public partial class Monitoring_Main : Form
    {
        private bool _autoReady = false;
        private bool _autoReadyBusy = false;

        private bool _autoStarting = false;
        private CancellationTokenSource _autoReadyCts;
        private HashSet<string> _readySequences;
        private HashSet<string> _startSequences;


        private InputCassetteLifter InputCassetteLifter { get; set; }
        private InputFeeder InputFeeder { get; set; }
        private InputStage InputStage { get; set; }
        private InputDieTransfer InputDieTransfer { get; set; }
        private Rotary Rotary { get; set; }
        private OutputDieTransfer OutputDieTransfer { get; set; }
        private OutputStage OutputStage { get; set; }
        private OutputCassetteLifter OutputCassetteLifter { get; set; }

        private IndexLoadAligner IndexLoadAligner { get; set; }
        private IndexUnloadAligner IndexUnloadAligner { get; set; }
        private IndexChipProbeController IndexChipProbeController { get; set; }
        private IndexChipProber IndexChipProber { get; set; }
        private OutputFeeder OutputFeeder { get; set; }
        private InputStageEjector InputStageEjector {get; set; }

        // Add
        private PKGTester Tester => Equipment.Instance.Tester;
        // 입력 스테이지 최신 웨이퍼 캐시
        private MaterialWafer _lastInputWafer;

        public Monitoring_Main() : this(
            TryGetUnit<InputFeeder>(Equipment.UnitKeys.InputFeeder),
            TryGetUnit<InputDieTransfer>(Equipment.UnitKeys.InputDieTransfer),
            TryGetUnit<Rotary>(Equipment.UnitKeys.Rotary),
            TryGetUnit<OutputDieTransfer>(Equipment.UnitKeys.OutputDieTransfer),
            TryGetUnit<OutputFeeder>(Equipment.UnitKeys.OutputFeeder),
            TryGetUnit<InputStage>(Equipment.UnitKeys.InputStage),
            TryGetUnit<IndexUnloadAligner>(Equipment.UnitKeys.IndexUnloadAligner),
            TryGetUnit<OutputStage>(Equipment.UnitKeys.OutputStage),
            TryGetUnit<InputCassetteLifter>(Equipment.UnitKeys.InputCassetteLifter),
            TryGetUnit<IndexLoadAligner>(Equipment.UnitKeys.IndexLoadAligner),
            TryGetUnit<IndexChipProbeController>(Equipment.UnitKeys.IndexChipProbeController),
            TryGetUnit<OutputCassetteLifter>(Equipment.UnitKeys.OutputCassetteLifter),
            TryGetUnit<InputStageEjector>(Equipment.UnitKeys.InputStageEjector),
            TryGetUnit<IndexChipProber>(Equipment.UnitKeys.IndexChipProber))
        {
        }

        public Monitoring_Main(InputFeeder inputFeeder, InputDieTransfer inputDieTransfer, Rotary rotary,
                            OutputDieTransfer outputDieTransfer, OutputFeeder outputFeeder,
                            InputStage inputStage, IndexUnloadAligner indexUnloadAligner, OutputStage outputStage,
                            InputCassetteLifter inputCassetteLifter, IndexLoadAligner indexLoadAligner,
                            IndexChipProbeController indexChipProbeController, OutputCassetteLifter outputCassetteLifter,
                            InputStageEjector inputStageEjector, IndexChipProber indexChipProber)
        {
            InitializeComponent();

            #region Chart
            InputFeeder = inputFeeder;
            InputDieTransfer = inputDieTransfer;
            Rotary = rotary;
            OutputDieTransfer = outputDieTransfer;
            OutputFeeder = outputFeeder;
            InputStage = inputStage;
            IndexUnloadAligner = indexUnloadAligner;
            OutputStage = outputStage;

            InputCassetteLifter = inputCassetteLifter;
            IndexLoadAligner = indexLoadAligner;
            IndexChipProbeController = indexChipProbeController;
            OutputCassetteLifter = outputCassetteLifter;

            InputStageEjector = inputStageEjector;
            IndexChipProber = indexChipProber;

            _readySequences = new HashSet<string>();
            _startSequences = new HashSet<string>();
            sequenceAutoControl.SequenceButtonRequested += OnAutoSequenceButtonRequested;
            
            // WaferSelectMapView 이벤트 구독
            if (inputWaferCarrierControl1?.GetWaferSelectMapView() != null)
            {
                var materialCassette = InputCassetteLifter?.GetMaterialCassette();
                InputCassetteLifter.EventUpdateUICassette += InputCassetteLifter_EventUpdateUICassette;
                
                inputWaferCarrierControl1.GetWaferSelectMapView().SlotClicked += OnInputWaferSlot_Clicked;
                inputWaferCarrierControl1.GetWaferSelectMapView().SlotSelectionChanged += OnInputWaferSlot_SelectionChanged;

                if (materialCassette != null)
                {
                    inputWaferCarrierControl1.GetWaferSelectMapView().SetMaterialCassette(materialCassette);
                }
                else
                {
                    // 테스트용 카세트 생성
                    inputWaferCarrierControl1.GetWaferSelectMapView().CreateTestCassette(25);
                }
            }

            if (outputWaferCarrierControl1?.GetWaferSelectMapView() != null)
            {
                var materialCassette = OutputCassetteLifter?.GetMaterialCassette();
                OutputCassetteLifter.EventUpdateUICassette += OutputCassetteLifter_EventUpdateUICassette;

                outputWaferCarrierControl1.GetWaferSelectMapView().SlotClicked += OnInputWaferSlot_Clicked;
                outputWaferCarrierControl1.GetWaferSelectMapView().SlotSelectionChanged += OnInputWaferSlot_SelectionChanged;

                if (materialCassette != null)
                {
                    outputWaferCarrierControl1.GetWaferSelectMapView().SetMaterialCassette(materialCassette);
                }
                else
                {
                    // 테스트용 카세트 생성
                    outputWaferCarrierControl1.GetWaferSelectMapView().CreateTestCassette(25);
                }
            }
            #endregion


            // 이벤트 - Input Control
            dieInputControl1.MotorMoveRequested += OnDieInput_MotorMoveRequested;
            dieIndexSelectControl1.RotationRequested += OnDieRotation_Requested;

            inputStage.EventUpdateUIWafer -= InputStage_EventUpdateUIWafer;
            inputStage.EventUpdateUIWafer += InputStage_EventUpdateUIWafer;

            outputStage.EventUpdateUIWafer -= OutputStage_EventUpdateUIWafer;
            outputStage.EventUpdateUIWafer += OutputStage_EventUpdateUIWafer;
            

            // 이벤트 구독: 픽업 완료 시 입력 뷰에서 해당 다이를 제거(Picked/Empty)로 반영
            if (InputDieTransfer != null)
                InputDieTransfer.DiePicked += InputDieTransfer_DiePicked;

            if (OutputStage != null)
                OutputStage.DiePlaced += OutputStage_DiePlaced;

            // Add
            if(Tester != null)
            {
                Tester.OnMeasureCompleted += Tester_OnMeasureCompleted;
                Tester.OnMeasureAborted += Tester_OnMeasureAborted;
                casSpectrumViewer1.AttachSpectrometer(Tester.Spectrometer);
            }
        }

        private void Tester_OnMeasureCompleted(object sender)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;

                var t = Tester;
                if (t == null) return;

                // 스냅샷
                var res = t.Result;
                if (res == null) return;

                var bin = res.BinningResult; // 생성 시점에 항상 존재하지만 방어적으로 처리
                var binType = bin?.BinType ?? BinningType.None;
                var binNo = bin?.BinNo ?? -1;
                var binLabel = bin?.BinLabel ?? string.Empty;

                Action ui = () =>
                {
                    if (IsDisposed) 
                        return;

                    switch (binType)
                    {
                        case BinningType.GoodBin:
                            lbResultValue.Text = $"{binNo}. {binLabel}";
                            lbResultValue.ForeColor = Color.Lime;
                            break;
                        case BinningType.NgBin:
                            lbResultValue.Text = "NG";
                            lbResultValue.ForeColor = Color.Red;
                            break;
                        default:
                            lbResultValue.Text = "UNKNOWN";
                            lbResultValue.ForeColor = Color.Gray;
                            break;
                    }
                };

                if (InvokeRequired) BeginInvoke(ui);
                else ui();


            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            //PKGTesterResult result = Tester.Result;
            //BinningResult binningResult = result.BinningResult;
            //switch (binningResult.BinType)
            //{
            //    case BinningType.GoodBin:
            //        lbResultValue.Text = $"{binningResult.BinNo}. {binningResult.BinLabel}";
            //        lbResultValue.ForeColor = Color.Lime;
            //        break;
            //    case BinningType.NgBin:
            //        lbResultValue.Text = "NG";
            //        lbResultValue.ForeColor = Color.Red;
            //        break;
            //    default:
            //        lbResultValue.Text = "UNKNOWN";
            //        lbResultValue.ForeColor = Color.Gray;
            //        break;
            //}
        }

        private void Tester_OnMeasureAborted(object sender)
        {
            try
            {
                if (IsDisposed || !IsHandleCreated) return;
                Action ui = () =>
                {
                    if (IsDisposed) return;
                    lbResultValue.Text = "ABORT";
                    lbResultValue.ForeColor = Color.OrangeRed;
                };
                if (InvokeRequired) BeginInvoke(ui);
                else ui();
            }
            catch (Exception ex)
            {
                try { Log.Write("Monitoring_Main", $"Tester_OnMeasureAborted 예외: {ex.Message}"); } catch { }
            }
        }


        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (InputDieTransfer != null)
                InputDieTransfer.DiePicked -= InputDieTransfer_DiePicked;

            if (OutputDieTransfer != null)
                OutputStage.DiePlaced -= OutputStage_DiePlaced;

            if (InputStage != null)
                InputStage.EventUpdateUIWafer -= InputStage_EventUpdateUIWafer;

            if (OutputStage != null)
                OutputStage.EventUpdateUIWafer -= OutputStage_EventUpdateUIWafer;

            // 추가: Tester 이벤트 해제(폼 dispose 이후 이벤트 유입 방지)
            try
            {
                if (Tester != null)
                {
                    Tester.OnMeasureCompleted -= Tester_OnMeasureCompleted;
                    Tester.OnMeasureAborted -= Tester_OnMeasureAborted;
                }
            }
            catch { }

            base.OnFormClosed(e);
        }

        private void OutputStage_EventUpdateUIWafer(MaterialWafer wafer)
        {
            Action ui = () =>
            {
                if (IsDisposed)
                    return;

                if (wafer == null)
                {
                    dieOutputControl1.SetWaferId("N/A");
                    dieOutputControl1.SetDieList(new List<MaterialDie>());
                    return;
                }
                lock (wafer.Dies)
                {
                    if (string.IsNullOrWhiteSpace(wafer.WaferId))
                        wafer.WaferId = $"QMC_BIN_{wafer.Dies.Count}";

                    // 새 웨이퍼가 올라온 경우에만 픽업 히스토리 리셋 (조건 예시)
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                        dieInputControl1.ResetPickedMarks();

                    dieOutputControl1.SetWaferId(wafer.WaferId);
                    dieOutputControl1.SetDieList(wafer.Dies ?? new List<MaterialDie>());
                }
            };

            if (InvokeRequired) BeginInvoke(ui);
            else ui();
        }

        private void InputStage_EventUpdateUIWafer(MaterialWafer wafer)
        {
            _lastInputWafer = wafer; // 장비 좌표 보유 다이 검색용 캐시
            Action ui = () =>
            {
                if (IsDisposed) 
                    return;

                if (wafer == null)
                {
                    dieInputControl1.SetWaferId("N/A");
                    dieInputControl1.SetDieList(new List<MaterialDie>());
                    return;
                }
                lock (wafer.Dies)
                {
                    if (string.IsNullOrWhiteSpace(wafer.WaferId))
                        wafer.WaferId = $"QMC_WAFER_{wafer.Dies.Count}";

                    // 새 웨이퍼가 올라온 경우에만 픽업 히스토리 리셋 (조건 예시)
                    if (wafer.ProcessSatate == Material.MaterialProcessSatate.Ready)
                        dieInputControl1.ResetPickedMarks();

                    dieInputControl1.SetWaferId(wafer.WaferId);
                    dieInputControl1.SetDieList(wafer.Dies ?? new List<MaterialDie>());
                }
            };

            if (InvokeRequired) BeginInvoke(ui);
            else ui();
        }
        private void InputDieTransfer_DiePicked(object sender, InputDieTransfer.DiePickedEventArgs e)
        {
            dieInputControl1.MarkCurrentPicked(new PointD(e.MapX, e.MapY));
            // MarkDieRemoved는 내부적으로 Invoke 처리하므로 바로 호출해도 안전
            //dieInputControl1.MarkDieRemoved(new System.Drawing.Point(e.MapX, e.MapY), showAsPicked: true);
        }

        private void OutputStage_DiePlaced(object sender, OutputStage.DiePlacedEventArgs e)
        {
            // 예: 로그/강조 처리. 실제 UI는 EventUpdateUIWafer로 전체 갱신됨.
            Console.WriteLine($"[Out] Placed at Bin ({e.BinX},{e.BinY})");

            // Placed를 초록색으로 보여주고 싶으면 Picked로 매핑
            dieOutputControl1.UpdateDie(new PointD(e.BinX, e.BinY), DieProcessState.Placed);
        }

        private void OutputCassetteLifter_EventUpdateUICassette(MaterialCassette Cassette)
        {
            this.outputWaferCarrierControl1.GetWaferSelectMapView()?.SetMaterialCassette(Cassette);
            if (Cassette.CarrierId == string.Empty)
            {
                Cassette.CarrierId = string.Format("QMC_OUT_CASSETTE_{0}", Cassette.SlotCount);
            }

            this.outputWaferCarrierControl1.SetWaferCarrierId(Cassette.CarrierId);
            // 실제 존재하는 웨이퍼 수로 갱신
            var presentCount = GetPresentWaferCount(Cassette);
            this.outputWaferCarrierControl1.UpdateWaferCount(presentCount);
            Log.Write("Monitoring_Main", $"Output Cassette Updated: ID={Cassette.CarrierId}, Slots={Cassette.SlotCount}, Present={presentCount}");
        }

        private void InputCassetteLifter_EventUpdateUICassette(MaterialCassette Cassette)
        {
            this.inputWaferCarrierControl1.GetWaferSelectMapView()?.SetMaterialCassette(Cassette);
            if (Cassette.CarrierId == string.Empty)
            {
                Cassette.CarrierId = string.Format("QMC_IN_CASSETTE_{0}", Cassette.SlotCount);
            }

            this.inputWaferCarrierControl1.SetWaferCarrierId(Cassette.CarrierId);

            // 실제 존재하는 웨이퍼 수로 갱신
            var presentCount = GetPresentWaferCount(Cassette);
            this.inputWaferCarrierControl1.UpdateWaferCount(presentCount);
            Log.Write("Monitoring_Main", $"Input Cassette Updated: ID={Cassette.CarrierId}, Slots={Cassette.SlotCount}, Present={presentCount}");
        }

        // 헬퍼: 실제 존재(Exist) 웨이퍼 개수 계산
        private static int GetPresentWaferCount(MaterialCassette cassette)
        {
            if (cassette == null || cassette.SlotCount <= 0) return 0;

            int count = 0;
            for (int i = 0; i < cassette.SlotCount; i++)
            {
                var w = cassette.GetWafer(i);
                if (w != null && w.Presence == MaterialPresence.Exist)
                    count++;
            }
            return count;
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
            if (e == null || e.Item == null)
            {
                Console.WriteLine("[Input] 모터 이동 요청: 이벤트/아이템 NULL");
                return;
            }

            // DisplayItem ↔ 실제 MaterialDie 매핑
            var die = FindInputDieByDisplayItem(e.Item);
            if (die == null)
            {
                Console.WriteLine($"[Input] 매핑 실패 → 맵 좌표 사용 이동. Map({e.Item.Position.X},{e.Item.Position.Y})");
                MovePickMotorTo(e.Item.Position.X, e.Item.Position.Y, new Point((int)Math.Round(e.Item.Position.X),
                                                                               (int)Math.Round(e.Item.Position.Y)));
                ShowMotorMovingStatus($"Input 모터가 맵좌표 ({e.Item.Position.X:0.###},{e.Item.Position.Y:0.###})로 이동 중...(Die 매핑 실패)");
                return;
            }

            // 장비 좌표 후보 (CenterX/CenterY 우선)
            double stageX = die.CenterX;
            double stageY = die.CenterY;

            // Vision/장비 좌표가 아직 유효하지 않을 경우(0 근처) fallback → Map 좌표
            if (Math.Abs(stageX) < 0.0001 && Math.Abs(stageY) < 0.0001)
            {
                stageX = die.MapX;
                stageY = die.MapY;
            }

            // Log 상세
            Console.WriteLine(
                $"[Input] 모터 이동 요청(DieId={die.Index}) " +
                $"Map({die.MapX:0.###},{die.MapY:0.###}) → Stage({stageX:0.###},{stageY:0.###}) Center({die.CenterX:0.###},{die.CenterY:0.###})");

            // 맵 좌표는 UI 업데이트용 따로 보존
            var mapPoint = new Point((int)die.MapX, (int)die.MapY);

            // 실제 모터 이동 (장비 좌표)
            MovePickMotorTo(stageX, stageY, mapPoint);

            //ShowMotorMovingStatus(
            //    $"Input 모터 이동 중... Die:{die.Index} Map({die.MapX:0.###},{die.MapY:0.###}) → Stage({stageX:0.###},{stageY:0.###})");
        }

        // DisplayItem → MaterialDie 검색 헬퍼
        private MaterialDie FindInputDieByDisplayItem(DisplayView.DisplayItem displayItem)
        {
            try
            {
                var dies = _lastInputWafer?.Dies;
                if (dies == null) 
                    return null;

                // 1) Index(DieId) 매칭 시도
                if (displayItem.DieId >= 0)
                {
                    var byIndex = dies.FirstOrDefault(d => d.Index == displayItem.DieId);
                    if (byIndex != null) 
                        return byIndex;
                }

                // 2) Map 좌표(반올림) 매칭
                var mx = Math.Round(displayItem.Position.X);
                var my = Math.Round(displayItem.Position.Y);
                var byMap = dies.FirstOrDefault(d => d.MapX == mx && d.MapY == my);
                if (byMap != null) 
                    return byMap;

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Input] FindInputDieByDisplayItem 예외: {ex.Message}");
                return null;
            }
        }

        // 장비 좌표 이동 (UI 반영 시 맵좌표 유지 위해 mapPointOverride 사용)
        private void MovePickMotorTo(double stageX, double stageY, Point? mapPointOverride = null)
        {
            try
            {
                Console.WriteLine($"Pick 모터 이동(Stage): ({stageX:0.###}, {stageY:0.###})");

                // 실제 Stage 이동 메서드가 있으면 반사 호출 (존재 시)
                try
                {
                    // 직접 InputStage API 호출 (신규 추가된 MoveToPosition 사용)
                    if (InputStage != null)
                    {
                        int rc = InputStage.MoveStage(stageX, stageY);
                        if (rc != 0)
                        {
                            Console.WriteLine("[Motor] InputStage.MoveToPosition 실패");
                            ShowMotorMovingStatus("모터 이동 실패(인터락 또는 축 오류)");
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Motor] InputStage NULL - 이동 불가");
                        return;
                    }
                }
                catch (Exception rex)
                {
                    Console.WriteLine($"[Motor] MoveToPosition 호출 실패(시뮬레이션 처리): {rex.Message}");
                }

                // 이동 완료 후 UI 반영 (맵 좌표로 상태 업데이트)
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                {
                    if (IsDisposed) return;
                    this.Invoke(new Action(() =>
                    {
                        //PointD updatePoint;
                        //if (mapPointOverride.HasValue)
                        //    updatePoint = new PointD(mapPointOverride.Value.X, mapPointOverride.Value.Y);
                        //else
                        //    updatePoint = new PointD(stageX, stageY); // Fallback

                        //dieInputControl1.UpdateChip(updatePoint, DieProcessState.Picked);
                        //ShowMotorMovingStatus($"Pick 모터 이동 완료(Stage: {stageX:0.###}, {stageY:0.###})");
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"모터 이동 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            nRet = Rotary.WaitIndexMoveDone();
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
            var mb = new MessageBoxOk();
            mb.ShowDialog("Info.", message);

            // progressBar1.Visible = true; // 예시
            Console.WriteLine($"[Motor] {message}");


        }

        private void MovePickMotorTo(double x, double y)
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
                        dieInputControl1.UpdateChip(new PointD(x, y), DieProcessState.Picked);
                        ShowMotorMovingStatus("Pick 모터 이동 완료");
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

            #endregion

            #region Output Control - Square Shape
            // 기존 출력 컨트롤 로직은 그대로 (DieOutputControl 은 별도 변환 필요 시 추후 적용)
            #endregion

            #region InputWaferCarrierControl
            inputWaferCarrierControl1.SetWaferCarrierId("N/A");
            inputWaferCarrierControl1.UpdateWaferCount(0);
            #endregion

            #region OutputWaferCarrierControl
            outputWaferCarrierControl1.SetWaferCarrierId("N/A");
            outputWaferCarrierControl1.UpdateWaferCount(0);
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

            if (sequenceAutoControl != null)
                sequenceAutoControl.SequenceButtonRequested -= OnAutoSequenceButtonRequested;

        }


        #region Auto Sequence 처리
        private void OnAutoSequenceButtonRequested(object sender, AutoSequenceEventArgs e)
        {
            Log.Write("Operator_Main", $"Auto Sequence {e.Command} 요청");
            switch (e.Command)
            {
                case "Ready":
                    HandleAutoReady();
                    break;

                case "Start":
                    HandleAutoStart(); // 설비 전체 Start 위임
                    break;

                case "Stop":
                    HandleAutoStop();  // 설비 전체 Stop 위임
                    break;

                case "CycleStop":
                    HandleAutoCycleStop();
                    break;

                case "Reset":
                    HandleAutoReset();
                    break;
            }
        }

        private void HandleAutoReady()
        {
            if (_autoReadyBusy)
            {
                Log.Write("Monitoring_Main", "Auto Ready 작업 진행 중 - 요청 무시");
                return;
            }

            // Ready 버튼 비활성화
            try 
            { 
                sequenceAutoControl.SetButtonEnabled("Ready", false); 
            } 
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            sequenceAutoControl.Enabled = false;
            _autoReadyBusy = true;

            // 상태 토글 ON (UI 하이라이트용)
            _autoReady = true;
            NotifyAutoSequenceStateChanged("Ready", true);

            // 매번 강제로 Ready 절차를 수행하도록 캐시 초기화
            _readySequences.Clear();

            _autoReadyCts?.Cancel();
            _autoReadyCts = new CancellationTokenSource();
            var ct = _autoReadyCts.Token;

            // ProgressForm 이 요구하는 Task<int>로 래핑
            Task<int> readyTask = Task.Run(() =>
            {
                try
                {
                    // 실제 Ready 실행 (취소 지원)
                    var ok = ReadyAllSequencesAsync(ct).GetAwaiter().GetResult();
                    return ok ? 0 : -1;
                }
                catch (OperationCanceledException)
                {
                    return -2; // 취소 코드
                }
                catch (Exception ex)
                {
                    Log.Write("Monitoring_Main", $"Auto Ready 예외: {ex.Message}");
                    return -1;
                }
            }, ct);

            var form = new ProgressForm("Auto Ready", "ReadyAllSequences", readyTask, this.Rotary);
            try
            {
                form.ShowDialog(this);

                if (form.DialogResult == DialogResult.Cancel)
                {
                    try { _autoReadyCts.Cancel(); } catch { }
                    Log.Write("Monitoring_Main", "Auto Ready 취소 요청");
                }

                // 작업 결과 확인 (취소는 정상 흐름으로 간주)
                if (readyTask.IsFaulted)
                {
                    var ex = readyTask.Exception?.GetBaseException();
                    if (!(ex is OperationCanceledException))
                    {
                        MessageBox.Show($"Auto Ready 오류: {ex?.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    int rc = readyTask.Status == TaskStatus.RanToCompletion ? readyTask.Result : -1;
                    if (rc == -1)
                    {
                        MessageBox.Show("Auto Ready 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    // rc == 0: 성공, rc == -2: 취소 → 메시지 표시 없음
                }
            }
            finally
            {
                // Ready 버튼/컨트롤 복구
                try { sequenceAutoControl.SetButtonEnabled("Ready", true); } catch { }
                sequenceAutoControl.Enabled = true;

                _autoReadyBusy = false;
                _autoReady = false;
                NotifyAutoSequenceStateChanged("Ready", false);
            }
        }

        private async void HandleAutoStart()
        {
            var eq = Equipment.Instance;
            if (eq == null) 
                return;

            try
            {
                // UI 토글 알림(즉시 반영), 최종 상태는 Eq.StateChanged에서 수렴
                NotifyAutoSequenceStateChanged("Start", true);

                // 설비 전체 시작
                var cts = new CancellationTokenSource();
                bool ok = await StartAllSequencesAsync(cts.Token).ConfigureAwait(true);
                if (!ok)
                {
                    NotifyAutoSequenceStateChanged("Start", false);
                    MessageBox.Show("Auto Start 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _autoStarting = false;
                    return;
                }

                _autoReady = false;
                _autoStarting = true;
                Log.Write("Operator_Main", "Auto Start 완료 (시퀀스 기반 다중 Unit Start)");
            }
            catch (OperationCanceledException)
            {
                NotifyAutoSequenceStateChanged("Start", false);
                _autoStarting = false;
                Log.Write("Operator_Main", "Auto Start 취소됨");
            }
            catch (Exception ex)
            {
                NotifyAutoSequenceStateChanged("Start", false);
                _autoStarting = false;
                Log.Write(ex);
            }

            //if (!_autoStarting)
            //{
            //    if (!_autoReady)
            //    {
            //        MessageBox.Show("Auto Ready를 먼저 실행해주세요.");
            //        return;
            //    }
            //    _autoReady = false;
            //    _autoStarting = true;
            //    NotifyAutoSequenceStateChanged("Ready", false);
            //    NotifyAutoSequenceStateChanged("Start", true);
            //    ExecuteAutoStart();
            //    Log.Write("Operator_Main", "Auto Start 실행 (Ready OFF)");
            //}
            //else
            //{
            //    _autoStarting = false;
            //    NotifyAutoSequenceStateChanged("Start", false);
            //    Log.Write("Operator_Main", "Auto Start OFF");
            //}
        }

        // 2) AutoStop 대기부: "State == Stop" 기준으로 수정 (축 정지와 함께)
        private async void HandleAutoStop()
        {
            var eq = Equipment.Instance;
            if (eq == null)
                return;

            try
            {
                NotifyAutoSequenceStateChanged("Stop", true);

                _autoReady = false;
                _autoStarting = false;
                _readySequences.Clear();
                _startSequences.Clear();
                try 
                { 
                    sequenceAutoControl.ResetAllButtons(); 
                } 
                catch { }

                // 1) 논리 시퀀스 Stop
                await StopAllSequencesAsync().ConfigureAwait(true);

                // 2) 설비 전체 정지(보강) – EquipmentStatus는 제외
                if (eq.Units != null)
                {
                    foreach (var u in eq.Units.Values.OfType<BaseUnit>())
                    {
                        if (IsStopExemptUnit(u)) 
                            continue;

                        try
                        {
                            if (!string.IsNullOrEmpty(u.UnitName))
                                await eq.StopUnitAsync(u.UnitName).ConfigureAwait(true);
                        }
                        catch (Exception ex) { Log.Write(ex); }
                    }
                }

                // 3) 물리적 정지 대기 (ProgressForm)
                var axisMgr = eq.AxisManager;
                var waitCts = new CancellationTokenSource();
                int timeoutMs = 50000;

                int lastPercent = 0;
                string lastDetail = string.Empty;

                // 진행 상황을 받아오는 콜백
                Action<int, string> progressCb = (p, info) =>
                {
                    lastPercent = p;
                    lastDetail = info;
                    // ProgressForm 내부에서 Poll 방식으로 가져갈 예정
                };

                var waitTask = WaitForFullPhysicalStopAsync(
                    eq,
                    axisMgr,
                    timeoutMs,
                    waitCts.Token,
                    progressCb,
                    emergencyOnCancel: true);

                // ProgressForm 생성
                var form = new ProgressForm("Auto Stop",
                    "모션/시퀀스 완전 정지 대기 중...",
                    waitTask,
                    this);

                // 폼이 주기적으로 percent/detail을 표시할 수 있도록 Tick 이벤트(혹은 기존 구현 확장) 활용
                form.CustomStatusProvider = () =>
                {
                    return new Tuple<int, string>(lastPercent, lastDetail);
                };

                form.StopProcess += _ =>
                {
                    try { waitCts.Cancel(); } catch { }
                };

                form.ShowDialog(this);

                int rc;
                if (waitTask.IsFaulted)
                {
                    rc = -1;
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("정지 대기 중 예외!", waitTask.Exception?.GetBaseException().Message);
                }
                else
                {
                    rc = waitTask.Status == TaskStatus.RanToCompletion ? waitTask.Result : -1;
                    if (rc == -2)
                    {
                        MessageBox.Show("사용자 취소 (일부 축 강제 Stop 적용).", "알림",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (rc == -3)
                    {
                        MessageBox.Show("타임아웃: 일부 축 또는 유닛이 완전 정지하지 않았습니다.", "경고",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else if (rc != 0)
                    {
                        MessageBox.Show("완전 정지 확인 실패.", "경고",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }

                Log.Write("Operator_Main", $"Auto Stop 종료(rc={rc}) detail='{lastDetail}'");

                if (rc == 0)
                {
                    // 완전 정지 후 Manual 모드 전환
                    TryTransitionToManualIfStopped(eq);
                    //MessageBox.Show("완전히 정지되었습니다.", "Stop 완료",
                    //    MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"설비 정지 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                NotifyAutoSequenceStateChanged("Stop", false);
            }

            //// 3) “강제정지 없이” 모든 유닛(State==Stop) + 모든 축 Idle 대기
            //var axisMgr = eq.AxisManager;
            //var waitCts = new System.Threading.CancellationTokenSource();
            //int totalTimeoutMs = 15000; // 총 대기 타임아웃
            //int pollMs = 50;

            //Task<int> tWaitFinish = Task.Run(async () =>
            //{
            //    var sw = System.Diagnostics.Stopwatch.StartNew();
            //    try
            //    {
            //        while (true)
            //        {
            //            waitCts.Token.ThrowIfCancellationRequested();

            //            bool unitsFinished = IsAllUnitsStopped(eq);
            //            bool axesIdle = AreAllAxesIdle(axisMgr);

            //            if (unitsFinished && axesIdle)
            //                return 0;

            //            if (sw.ElapsedMilliseconds > totalTimeoutMs)
            //                return -3; // 타임아웃

            //            await Task.Delay(pollMs, waitCts.Token).ConfigureAwait(false);
            //        }

            //        //while (true)
            //        //{
            //        //    waitCts.Token.ThrowIfCancellationRequested();

            //        //    // (a) 모든 유닛이 State == Stop (예외 유닛 제외)
            //        //    bool unitsFinished = true;
            //        //    try
            //        //    {
            //        //        var units = eq?.Units?.Values?.OfType<BaseUnit>()
            //        //                      .Where(u => !IsStopExemptUnit(u))
            //        //                      .ToList() ?? new List<BaseUnit>();

            //        //        for (int i = 0; i < units.Count; i++)
            //        //        {
            //        //            var u = units[i];
            //        //            bool stoppedByState = false;
            //        //            try 
            //        //            { 
            //        //                stoppedByState = (u.State == BaseUnit.ProcessState.Stop); 
            //        //            } 
            //        //            catch { }

            //        //            if (!stoppedByState)
            //        //            {
            //        //                unitsFinished = false;
            //        //                break;
            //        //            }
            //        //        }
            //        //    }
            //        //    catch { unitsFinished = false; }

            //        //    // (b) 모든 축 정지(IsMoveDone)
            //        //    bool axesIdle = true;
            //        //    try
            //        //    {
            //        //        var axes = axisMgr?.GetAll();
            //        //        if (axes != null)
            //        //        {
            //        //            //todo: 축추가확인!!!
            //        //            //for (int i = 0; i < axes.Length; i++)
            //        //            for (int i = 0; i < 25; i++)    //마지막 축 추가하고 위에꺼로 적용!!!
            //        //            {
            //        //                var ax = axes[i];
            //        //                if (ax == null) continue;
            //        //                bool done = false;
            //        //                try 
            //        //                { 
            //        //                    done = ax.IsMoveDone(); 
            //        //                } 
            //        //                catch
            //        //                { 
            //        //                    done = false; 
            //        //                }
            //        //                if (!done) 
            //        //                { 
            //        //                    axesIdle = false; 
            //        //                    break; 
            //        //                }
            //        //            }
            //        //        }
            //        //    }
            //        //    catch { axesIdle = false; }

            //        //    if (unitsFinished && axesIdle)
            //        //        return 0;

            //        //    await Task.Delay(50, waitCts.Token).ConfigureAwait(false);
            //        //}
            //    }
            //    catch (OperationCanceledException)
            //    {
            //        return -2; // 대기 취소
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.Write(ex);
            //        return -1;
            //    }
            //}, waitCts.Token);

            //var form = new ProgressForm("Auto Stop", "시퀀스 종료(State=Stop) 및 축 정지 대기 중...", tWaitFinish, this);
            //// Stop 버튼 → 대기 취소(강제정지 금지)
            //form.StopProcess += _ =>
            //{
            //    try { waitCts.Cancel(); } catch { }
            //};

            //form.ShowDialog();
            //int finalRc;
            //if (tWaitFinish.IsFaulted)
            //{
            //    finalRc = -1;
            //    var mb = new MessageBoxOk();
            //    mb.ShowDialog("정지 대기 중 예외!", tWaitFinish.Exception?.GetBaseException().Message);
            //}
            //else
            //{
            //    finalRc = tWaitFinish.Status == TaskStatus.RanToCompletion ? tWaitFinish.Result : -1;
            //    if (finalRc == -2)
            //    {
            //        MessageBox.Show("정지 대기가 취소되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    }
            //    else if (finalRc == -3)
            //    {
            //        MessageBox.Show("정지 대기 타임아웃 (일부 유닛/축 미정지)", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    }
            //    else if (finalRc != 0)
            //    {
            //        MessageBox.Show("일부 유닛 또는 축의 정지를 확인하지 못했습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    }
            //}

            //Log.Write("Operator_Main", $"Auto Stop 완료 rc={finalRc}");

            //// 4) 완전 정지 시 Manual 모드 전환 시도
            //if (finalRc == 0)
            //{
            //    TryTransitionToManualIfStopped(eq);
            //}

            //if (tWaitFinish.IsFaulted)
            //{
            //    var mb = new MessageBoxOk();
            //    mb.ShowDialog("정지 대기 중 예외!", tWaitFinish.Exception?.GetBaseException().Message);
            //}
            //else
            //{
            //    var rc = tWaitFinish.Status == TaskStatus.RanToCompletion ? tWaitFinish.Result : -1;
            //    if (rc == -2)
            //    {
            //        MessageBox.Show("정지 대기가 사용자에 의해 취소되었습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    }
            //    else if (rc != 0)
            //    {
            //        MessageBox.Show("일부 유닛(State) 또는 축의 정지를 확인하지 못했습니다.", "경고", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    }
            //}
            //Log.Write("Operator_Main", "Auto Stop 완료 (State=Stop 기준 + 축 정지 확인, 강제정지 없음)");
            //}
            //catch (Exception ex)
            //{
            //    Log.Write(ex);
            //    MessageBox.Show($"설비 정지 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //finally
            //{
            //    NotifyAutoSequenceStateChanged("Stop", false);
            //}
        }

        // === 확장: 논리 Stop 후 물리적(축/시퀀스) 완전 정지 대기 + Progress 표시 ===
        private Task<int> WaitForFullPhysicalStopAsync(
            Equipment eq,
            MotionAxisManager axisMgr,
            int totalTimeoutMs,
            CancellationToken token,
            Action<int, string> reportProgress,
            bool emergencyOnCancel = false)
        {
            return Task.Run(async () =>
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                // 캐시
                var units = eq?.Units?.Values?.OfType<BaseUnit>()
                               .Where(u => !IsStopExemptUnit(u))
                               .ToList() ?? new List<BaseUnit>();

                var axes = axisMgr?.GetAll() ?? new MotionAxis[0];
                int totalUnits = units.Count;
                int totalAxes = 26; // axes.Length; // 마지막 축은 나중에 적용하자.

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    // 1) Unit Stop / Busy 판정
                    int stoppedUnits = 0;
                    var busyUnits = new List<string>();
                    foreach (var u in units)
                    {
                        //bool isStopped = (u.RunUnitStatus == BaseUnit.UnitStatus.Stopped) 
                        //                 && (u.State == BaseUnit.ProcessState.Stop 
                        //                 || u.State == BaseUnit.ProcessState.Complete 
                        //                 || u.IsStop);

                        bool isStopped = (u.RunUnitStatus == BaseUnit.UnitStatus.Stopped)
                                        && (u.State == BaseUnit.ProcessState.Stop
                                        || u.IsStop);

                        if (isStopped)
                            stoppedUnits++;
                        else
                            busyUnits.Add(u.UnitName);
                    }
                    //Unit 별로.. 시컨스 완료되었을때 변수를 넣을까.
                    //시컨스가 완전히 멈춰야 아래 축도 움직이지 않으니깐.

                    // 2) 축 Idle 판정
                    int idleAxes = 0;
                    var movingAxes = new List<string>();
                    for (int i = 0; i < axes.Length; i++)
                    {
                        var ax = axes[i];
                        if (ax == null)
                        {
                            idleAxes++;
                            continue;
                        }
                        bool done;
                        try { done = ax.IsMoveDone(); }
                        catch { done = false; }

                        if (done)
                        {
                            //26번은 나중에 적용
                            if (ax.AxisNo != 26)
                            {
                                idleAxes++;
                            }
                        }
                        else
                        {
                            //26번은 나중에 적용
                            if(ax.AxisNo != 26)
                            {
                                movingAxes.Add(ax.Name);
                            }
                        }
                    }

                    // 3) 종료 조건
                    bool allUnitsStopped = stoppedUnits == totalUnits;
                    bool allAxesIdle = idleAxes == totalAxes;

                    // 진행률 계산
                    double unitRatio = totalUnits > 0 ? (double)stoppedUnits / totalUnits : 1.0;
                    double axisRatio = totalAxes > 0 ? (double)idleAxes / totalAxes : 1.0;
                    int percent = (int)Math.Round((unitRatio * 0.6 + axisRatio * 0.4) * 100.0);

                    // 진행 텍스트 구성
                    string detail = $"Units {stoppedUnits}/{totalUnits} | Axes {idleAxes}/{totalAxes}";
                    if (busyUnits.Count > 0)
                        detail += $" | BusyUnits: {string.Join(",", busyUnits)}";
                    if (movingAxes.Count > 0)
                        detail += $" | MovingAxes: {string.Join(",", movingAxes)}";

                    reportProgress(percent, detail);

                    if (allUnitsStopped && allAxesIdle)
                    {
                        // 여기에서.. 축이 정말로 전부 정지되었는지 몇번재확인이 필요한디. 
                        // 확인할때는 멈췄다고 확인되었다가 다음 스텝 움직일수있어..

                        return 0;
                    }

                    if (sw.ElapsedMilliseconds > totalTimeoutMs)
                        return -3; // 타임아웃

                    await Task.Delay(80, token).ConfigureAwait(false);
                }
            }, token).ContinueWith(t =>
            {
                // 취소 시 EmergencyStop 선택 적용
                if (t.IsCanceled && emergencyOnCancel && axisMgr != null)
                {
                    try
                    {
                        var axes = axisMgr.GetAll();
                        foreach (var ax in axes)
                        {
                            try { ax?.Stop(); } catch { }
                        }
                    }
                    catch { }
                    return -2;
                }
                if (t.IsFaulted)
                    return -1;
                return t.Result;
            });
        }

        private bool IsAllUnitsStopped(Equipment eq)
        {
            try
            {
                var list = eq?.Units?.Values?.OfType<BaseUnit>()
                             .Where(u => !IsStopExemptUnit(u))
                             .ToList();
                if (list == null) return true;
                foreach (var u in list)
                {
                    if (u.State != BaseUnit.ProcessState.Stop)
                        return false;
                }
                return true;
            }
            catch { return false; }
        }

        private bool AreAllAxesIdle(MotionAxisManager axisMgr)
        {
            try
            {
                var axes = axisMgr?.GetAll();
                if (axes == null || axes.Length == 0)
                    return true; // 축이 없으면 Idle 간주

                for (int i = 0; i < axes.Length; i++)
                {
                    var ax = axes[i];
                    if (ax == null) continue;

                    bool done;
                    try
                    {
                        done = ax.IsMoveDone();
                    }
                    catch
                    {
                        done = false;
                    }

                    if (!done)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write("Monitoring_Main", $"AreAllAxesIdle 예외: {ex.Message}");
                return false;
            }
        }

        private void TryTransitionToManualIfStopped(Equipment eq)
        {
            try
            {
                // 모든 유닛 Stop 검증(이중 확인)
                if (!IsAllUnitsStopped(eq)) return;
                if (!AreAllAxesIdle(eq.AxisManager)) return;

                // 실제 모드 전환 (예: eq.SetRunMode(Manual) 또는 각 Unit RunMode 변경)
                foreach (var u in eq.Units.Values.OfType<BaseUnit>())
                {
                    if (IsStopExemptUnit(u)) continue;
                    // AutoRunning 상태 해제 → Manual
                    if (u.RunMode == BaseUnit.UnitRunMode.Auto)
                        u.RunMode = BaseUnit.UnitRunMode.Manual;
                }
                Log.Write("Monitoring_Main", "모든 유닛/축 정지 확인 → Manual 모드로 전환 완료");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }


        // 클래스 내부 아무 위치(예: Helper 메서드 영역)에 추가
        private static bool IsStopExemptUnit(BaseUnit u)
        {
            if (u == null) return false;
            if (u is EquipmentStatus) return true;

            var name = u.UnitName;
            if (!string.IsNullOrEmpty(name) &&
                name.Equals("EquipmentStatus", StringComparison.OrdinalIgnoreCase))
                return true;

            var typeName = u.GetType().Name;
            if (string.Equals(typeName, "EquipmentStatus", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        // EquipmentStatus를 제외하고 개별 Stop 수행
        private async Task StopAllUnitsExceptEquipmentStatusAsync()
        {
            var eq = Equipment.Instance;
            if (eq?.Units == null) return;

            foreach (var kv in eq.Units)
            {
                var u = kv.Value as BaseUnit;
                if (u == null) continue;
                if (IsStopExemptUnit(u)) continue;

                try
                {
                    if (!string.IsNullOrEmpty(u.UnitName))
                        await eq.StopUnitAsync(u.UnitName).ConfigureAwait(true);
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                }
            }
        }

        //private async void HandleAutoStop()
        //{
        //    var eq = Equipment.Instance;
        //    if (eq == null) return;

        //    try
        //    {
        //        NotifyAutoSequenceStateChanged("Stop", true);

        //        // 로컬 시퀀스 토글/상태 정리(UI만)
        //        _autoReady = false;
        //        _autoStarting = false;
        //        _readySequences.Clear();
        //        _startSequences.Clear();
        //        try { sequenceAutoControl.ResetAllButtons(); } catch { }

        //        // 시퀀스 기반 정지
        //        await StopAllSequencesAsync().ConfigureAwait(true);

        //        // 여기에서 
        //        // 보강: 설비 전체 정지 호출(미정지 유닛 대비)
        //        var ok = await eq.StopAllUnitsAsync().ConfigureAwait(true);
        //        if (!ok)
        //        {
        //            MessageBox.Show("설비 정지 실패(일부 유닛 타임아웃 가능)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        }

        //        Log.Write("Operator_Main", "Auto Stop 완료 (시퀀스 기반 + 설비 전체 보강 Stop)");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Write(ex);
        //        MessageBox.Show($"설비 정지 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        // 최종 UI 토글 해제, 실제 상태는 Eq.StateChanged에서 최종 수렴
        //        NotifyAutoSequenceStateChanged("Stop", false);
        //    }
        //}

        private void HandleAutoCycleStop()
        {
            NotifyAutoSequenceStateChanged("CycleStop", true);
            Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("CycleStop", false); }));
            });
            ExecuteAutoCycleStop();
            Log.Write("Operator_Main", "Auto CycleStop 실행");
        }

        private void HandleAutoReset()
        {
            // UI 토글 ON
            NotifyAutoSequenceStateChanged("Reset", true);

            // Auto Reset 작업을 Task<int>로 래핑 (ProgressForm에 전달)
            var resetTask = Task.Run(() =>
            {
                try
                {
                    ExecuteAutoReset();
                    return 0;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return -1;
                }
            });

            // 진행 다이얼로그 표시
            var form = new ProgressForm("Auto Reset", "장비 상태 초기화 중...", resetTask, this);
            form.StopProcess += _ =>
            {
                // 현재 ExecuteAutoReset은 취소 미지원. 필요 시 취소 가능한 구조로 확장.
                MessageBox.Show("Auto Reset은 취소를 지원하지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            try
            {
                form.ShowDialog(this);

                if (resetTask.IsFaulted)
                {
                    var ex = resetTask.Exception?.GetBaseException();
                    MessageBox.Show($"Auto Reset 중 예외: {ex?.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (resetTask.Status == TaskStatus.RanToCompletion && resetTask.Result != 0)
                {
                    MessageBox.Show("Auto Reset 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // 정상 완료 시 완료 안내 (원치 않으면 생략 가능)
                    MessageBox.Show("Auto Reset 완료", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            finally
            {
                // UI 토글 OFF
                NotifyAutoSequenceStateChanged("Reset", false);
                Log.Write("Operator_Main", "Auto Reset 실행 완료");
            }
        }

        private void NotifyAutoSequenceStateChanged(string command, bool isActive)
        {
            sequenceAutoControl.OnAutoSequenceStateChanged(new AutoSequenceStateChangedEventArgs
            {
                Command = command,
                IsActive = isActive
            });
        }

        // 실행 순서(필요하면 Config 로 대체 가능)
        private static readonly string[] _sequenceOrder =
        {
            "InputWafer","ChipLoading","Process","ChipUnloading","OutputWafer"
        };

        private async Task<bool> ReadyAllSequencesAsync(CancellationToken ct)
        {
            foreach (var seq in _sequenceOrder)
            {
                if (!await ReadySequenceAsync(seq, ct))
                {
                    Log.Write("Operator_Main", $"Auto Ready 중단 - {seq} 실패");
                    return false;
                }
            }
            return true;
        }
        private async Task<bool> ReadySequenceAsync(string sequenceName, CancellationToken ct)
        {
            if (_readySequences.Contains(sequenceName))
                return true; // 이미 Ready

            bool ok = await TryReadySequenceAsync(sequenceName, ct);
            if (ok)
            {
                _readySequences.Add(sequenceName);
            }
            return ok;
        }

        // ====== 여기부터 Operator_Main과 동일한 다중 Unit 구조 ======
        #region Unified Sequence Helpers (Ready/Start/Stop)
        // 시퀀스명 → Units 매핑 (복수 유닛 지원)
        private IEnumerable<BaseUnit> GetUnitsForSequence(string sequenceName)
        {
            switch (sequenceName)
            {
                case "InputWafer":
                    return new BaseUnit[]
                    {
                        InputFeeder,
                        InputCassetteLifter,
                        InputStage
                    }.Where(u => u != null);

                case "ChipLoading":
                    return new BaseUnit[]
                    {
                        InputDieTransfer,
                        InputStageEjector
                    }.Where(u => u != null);

                case "Process":
                    return new BaseUnit[]
                    {
                        Rotary,
                        IndexLoadAligner,
                        IndexChipProbeController,
                        IndexChipProber,
                        IndexUnloadAligner
                    }.Where(u => u != null);

                case "ChipUnloading":
                    return new BaseUnit[]
                    {
                        OutputDieTransfer
                    }.Where(u => u != null);

                case "OutputWafer":
                    return new BaseUnit[]
                    {
                        OutputFeeder,
                        OutputCassetteLifter,
                        OutputStage
                    }.Where(u => u != null);
            }
            return Enumerable.Empty<BaseUnit>();
        }

        // 공통 Start (단일 Unit)
        private async Task<bool> TryStartUnitAsync(BaseUnit unit)
        {
            if (unit == null) return false;

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    Log.Write("Monitoring_Main", "TryStartUnitAsync 실패 - Equipment 인스턴스 없음");
                    return false;
                }
                var unitName = unit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    Log.Write("Monitoring_Main", "TryStartUnitAsync 실패 - UnitName 비어있음");
                    return false;
                }
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning)
                {
                    Log.Write("Monitoring_Main", $"TryStartUnitAsync - Unit '{unitName}' 이미 실행 중");
                    return true;
                }

                bool ok = await eq.StartUnitAsync(unitName).ConfigureAwait(true);
                if (!ok)
                {
                    Log.Write("Monitoring_Main", $"TryStartUnitAsync 실패 - Unit '{unitName}' 시작 실패");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        // Unit Running 대기
        private async Task<bool> WaitForUnitRunningAsync(BaseUnit unit, int timeoutMs, CancellationToken ct)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                ct.ThrowIfCancellationRequested();
                if (unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning)
                    return true;

                await Task.Delay(100, ct).ConfigureAwait(true);
            }
            return unit.RunUnitStatus == BaseUnit.UnitStatus.AutoRunning || unit.IsRunning;
        }

        // 시퀀스 단위 다중 시작 + Running 전이 대기
        private async Task<bool> StartUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel = true)
        {
            var units = GetUnitsForSequence(sequenceName).ToList();
            if (units.Count == 0)
            {
                Log.Write("Monitoring_Main", $"StartUnitsForSequenceAsync - '{sequenceName}' 매핑된 Unit 없음");
                return false;
            }

            // UnitName 기준 중복 제거
            var distinctUnits = new List<BaseUnit>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in units)
            {
                var key = string.IsNullOrEmpty(u.UnitName) ? u.GetHashCode().ToString() : u.UnitName;
                if (seen.Add(key))
                    distinctUnits.Add(u);
            }

            if (parallel)
            {
                var startTasks = distinctUnits.Select(TryStartUnitAsync).ToArray();
                var started = await Task.WhenAll(startTasks).ConfigureAwait(true);
                if (!started.All(r => r)) return false;

                var waitTasks = distinctUnits.Select(u => WaitForUnitRunningAsync(u, 5000, ct)).ToArray();
                var waited = await Task.WhenAll(waitTasks).ConfigureAwait(true);
                return waited.All(r => r);
            }
            else
            {
                foreach (var u in distinctUnits)
                {
                    ct.ThrowIfCancellationRequested();
                    var ok = await TryStartUnitAsync(u).ConfigureAwait(true);
                    if (!ok) return false;

                    var running = await WaitForUnitRunningAsync(u, 5000, ct).ConfigureAwait(true);
                    if (!running) return false;
                }
                return true;
            }
        }

        // 시퀀스 단위 다중 정지
        private async Task StopUnitsForSequenceAsync(string sequenceName)
        {
            var eq = Equipment.Instance;
            var units = GetUnitsForSequence(sequenceName).ToList();
            foreach (var u in units)
            {
                try
                {
                    if (u != null && !string.IsNullOrEmpty(u.UnitName))
                        await eq.StopUnitAsync(u.UnitName).ConfigureAwait(true);
                }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        // Start 공통 (시퀀스 단위)
        private async Task<bool> StartSequenceAsync(string sequenceName, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return await StartUnitsForSequenceAsync(sequenceName, ct, parallel: true).ConfigureAwait(true);
        }

        // 모든 시퀀스 Start (Auto)
        private async Task<bool> StartAllSequencesAsync(CancellationToken ct)
        {
            foreach (var seq in _sequenceOrder)
            {
                ct.ThrowIfCancellationRequested();

                // Ready 보정
                if (!_readySequences.Contains(seq))
                {
                    if (!await ReadySequenceAsync(seq, ct))
                    {
                        Log.Write("Monitoring_Main", $"Auto Start 중 Ready 실패 - {seq}");
                        return false;
                    }
                }

                // Ready 상태에서 Start
                if (!await StartSequenceAsync(seq, ct))
                {
                    Log.Write("Monitoring_Main", $"Auto Start 실패 - {seq}");
                    return false;
                }
                Log.Write("Monitoring_Main", $"Auto Start OK - {seq}");
            }
            return true;
        }

        // 모든 시퀀스 Stop (Auto)
        private async Task StopAllSequencesAsync()
        {
            // 역순 정지 필요 시 Array.Reverse(_sequenceOrder) 고려
            foreach (var seq in _sequenceOrder)
            {
                try { await StopUnitsForSequenceAsync(seq).ConfigureAwait(true); }
                catch (Exception ex) { Log.Write(ex); }
            }
        }

        // ===== Ready: 시퀀스별 다중 Ready 작업 빌드/실행 =====
        // rc == 0 성공, 그 외 실패
        private IEnumerable<Func<CancellationToken, Task<int>>> BuildReadyTasks(string sequenceName)
        {
            switch (sequenceName)
            {
                case "InputWafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return InputFeeder?.EnsureReady() ?? -1;
                        }, ct),
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return InputStageEjector?.CheckReady() ?? -1;
                        }, ct),
                    };

                case "ChipLoading":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return InputDieTransfer?.EnsureReady() ?? -1;
                        }, ct)
                    };

                case "Process":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return IndexLoadAligner?.EnsureReady() ?? -1;
                        }, ct),
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return IndexChipProbeController?.EnsureReady() ?? -1;
                        }, ct),
                    };

                case "ChipUnloading":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return OutputDieTransfer?.EnsureReady() ?? -1;
                        }, ct)
                    };

                case "OutputWafer":
                    return new Func<CancellationToken, Task<int>>[]
                    {
                        ct => Task.Run(() =>
                        {
                            ct.ThrowIfCancellationRequested();
                            return OutputFeeder?.EnsureReady() ?? -1;
                        }, ct)
                    };
            }

            return Enumerable.Empty<Func<CancellationToken, Task<int>>>();
        }

        private async Task<bool> ReadyUnitsForSequenceAsync(string sequenceName, CancellationToken ct, bool parallel = true)
        {
            var tasksFactory = BuildReadyTasks(sequenceName).ToList();
            if (tasksFactory.Count == 0)
            {
                Log.Write("Monitoring_Main", $"ReadyUnitsForSequenceAsync - '{sequenceName}' Ready 작업 없음");
                return false;
            }

            try
            {
                if (parallel)
                {
                    var tasks = tasksFactory.Select(f => f(ct)).ToArray();
                    var rcs = await Task.WhenAll(tasks).ConfigureAwait(true);
                    if (rcs.Any(rc => rc != 0))
                    {
                        Log.Write("Monitoring_Main", $"{sequenceName} Ready 실패(rc들: {string.Join(",", rcs)})");
                        return false;
                    }
                }
                else
                {
                    foreach (var f in tasksFactory)
                    {
                        ct.ThrowIfCancellationRequested();
                        var rc = await f(ct).ConfigureAwait(true);
                        if (rc != 0)
                        {
                            Log.Write("Monitoring_Main", $"{sequenceName} Ready 실패(rc={rc})");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (OperationCanceledException)
            {
                Log.Write("Monitoring_Main", $"{sequenceName} Ready 취소됨");
                throw;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                return false;
            }
        }

        // [MOD] 기존 switch 기반 -> 공통 Ready 실행 호출
        private async Task<bool> TryReadySequenceAsync(string sequenceName, CancellationToken ct)
        {
            return await ReadyUnitsForSequenceAsync(sequenceName, ct, parallel: true).ConfigureAwait(true);
        }
        #endregion

        private void ExecuteAutoCycleStop()
        {
        }



        private CancellationTokenSource _homeCts;
        private CancellationToken PrepareNewHomeToken()
        {
            _homeCts?.Cancel();
            _homeCts?.Dispose();
            _homeCts = new CancellationTokenSource();
            return _homeCts.Token;
        }
        private async void ExecuteAutoReset()
        {
            //InputCassetteLifter.ResetForNewRun();
            //InputCassetteLifter.SetMaterial(null);

            //InputFeeder.ResetForNewRun();
            //InputFeeder.SetMaterial(null);

            //InputStage.ResetForNewRun();
            //InputStage.SetMaterial(null);

            //InputStageEjector.ResetForNewRun();
            //InputStageEjector.SetMaterial(null);    // 가지고 있는게 없지만.

            //InputDieTransfer.ResetForNewRun();
            //InputDieTransfer.SetMaterial(null);

            //Rotary.ResetForNewRun();
            //Rotary.SetMaterial(null);

            //IndexLoadAligner.ResetForNewRun();
            //IndexLoadAligner.SetMaterial(null);

            //IndexChipProber.ResetForNewRun();
            //IndexChipProbeController.SetMaterial(null);

            //IndexUnloadAligner.ResetForNewRun();
            //IndexUnloadAligner.SetMaterial(null);

            //OutputDieTransfer.ResetForNewRun();
            //OutputDieTransfer.SetMaterial(null);

            //OutputStage.ResetForNewRun();
            //OutputStage.SetMaterial(null);

            //OutputFeeder.ResetForNewRun();
            //OutputFeeder.SetMaterial(null);

            //OutputCassetteLifter.ResetForNewRun();
            //OutputCassetteLifter.SetMaterial(null);

            //물어보면 안됨.
            //var ask = new MessageBoxYesNo();
            //if(ask.ShowDialog("확인", "Reset을 진행하시겠습니까?") != DialogResult.Yes)
            //{
            //    return;
            //}

            // 2) 취소 토큰 준비
            var token = PrepareNewHomeToken();

            // 6) 전 유닛 Ready 이동 → int 반환
            var rcReady = await MoveUnitsToReadyAsync(token).ConfigureAwait(true);
            if (rcReady != 0)
            {
                MessageBox.Show("일부 유닛 Ready 이동 실패", "Ready 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private async Task<int> MoveUnitsToReadyAsync(CancellationToken token)
        {
            string failureSummary = null;

            var t = Task.Run(async () =>
            {
                try
                {
                    // 1) InputDieTransfer + OutputDieTransfer (동시)
                    {
                        token.ThrowIfCancellationRequested();

                        var tasks = new List<Task<int>>();
                        var names = new List<string>();

                        if (InputDieTransfer != null)
                        {
                            tasks.Add(Task.Run(() => InputDieTransfer.EnsureReady(), token));
                            names.Add("InputDieTransfer");
                        }
                        if (OutputDieTransfer != null)
                        {
                            tasks.Add(Task.Run(() => OutputDieTransfer.EnsureReady(), token));
                            names.Add("OutputDieTransfer");
                        }

                        if (tasks.Count > 0)
                        {
                            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (results[i] != 0)
                                {
                                    failureSummary = names[i];
                                    return -1; // 첫 실패 시 즉시 NG
                                }
                            }
                        }
                        InputDieTransfer.ResetForNewRun();
                        OutputDieTransfer.ResetForNewRun();

                        InputDieTransfer.SetMaterial(null);
                        OutputDieTransfer.SetMaterial(null);

                    }

                    // 2) Rotary (단독)
                    if (Rotary != null)
                    {
                        token.ThrowIfCancellationRequested();

                        var rc = await Task.Run(() => Rotary.ExecuteUnitActionReady(), token).ConfigureAwait(false);
                        if (rc != 0)
                        {
                            failureSummary = "Rotary(ExecuteUnitActionReady)";
                            return -1;
                        }

                        IndexLoadAligner.ResetForNewRun();
                        //IndexChipProbeController.ResetForNewRun();
                        IndexChipProber.ResetForNewRun();
                        IndexChipProber.ResetForNewRun();
                        IndexUnloadAligner.ResetForNewRun();
                        Rotary.ResetForNewRun();

                        IndexLoadAligner.SetMaterial(null);
                        IndexChipProbeController.SetMaterial(null);
                        IndexChipProber.SetMaterial(null);
                        IndexUnloadAligner.SetMaterial(null);

                        Rotary.SetMaterial(null);
                        
                    }

                    // 3) InputStageEjector (단독, CheckReady)
                    if (InputStageEjector != null)
                    {
                        token.ThrowIfCancellationRequested();

                        var rc = await Task.Run(() => InputStageEjector.CheckReady(), token).ConfigureAwait(false);
                        if (rc != 0)
                        {
                            failureSummary = "InputStageEjector(CheckReady)";
                            return -1;
                        }

                        InputStageEjector.ResetForNewRun();
                        InputStageEjector.SetMaterial(null);
                    }

                    // 4) InputFeeder + OutputFeeder (동시)
                    {
                        token.ThrowIfCancellationRequested();

                        var tasks = new List<Task<int>>();
                        var names = new List<string>();

                        if (InputFeeder != null)
                        {
                            tasks.Add(Task.Run(() => InputFeeder.EnsureReady(), token));
                            names.Add("InputFeeder");
                        }
                        if (OutputFeeder != null)
                        {
                            tasks.Add(Task.Run(() => OutputFeeder.EnsureReady(), token));
                            names.Add("OutputFeeder");
                        }

                        if (tasks.Count > 0)
                        {
                            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (results[i] != 0)
                                {
                                    failureSummary = names[i];
                                    return -1;
                                }
                            }
                        }

                        InputFeeder.ResetForNewRun();
                        OutputFeeder.ResetForNewRun();
                        InputFeeder.SetMaterial(null);
                        OutputFeeder.SetMaterial(null);
                    }

                    // 5) InputStage + OutputStage (동시)
                    {
                        token.ThrowIfCancellationRequested();

                        var tasks = new List<Task<int>>();
                        var names = new List<string>();

                        if (InputStage != null)
                        {
                            tasks.Add(Task.Run(() => InputStage.MoveToStageLoadPosition(), token));
                            names.Add("InputStage");
                        }
                        if (OutputStage != null)
                        {
                            tasks.Add(Task.Run(() => OutputStage.MoveToStageLoadPosition(), token));
                            names.Add("OutputStage");
                        }

                        if (tasks.Count > 0)
                        {
                            var results = await Task.WhenAll(tasks).ConfigureAwait(false);
                            for (int i = 0; i < results.Length; i++)
                            {
                                if (results[i] != 0)
                                {
                                    failureSummary = names[i];
                                    return -1;
                                }
                            }
                        }

                        InputStage.ResetForNewRun();
                        InputStage.SetMaterial(null);
                        OutputStage.ResetForNewRun();
                        OutputStage.SetMaterial(null);

                        InputCassetteLifter.ResetForNewRun();
                        InputCassetteLifter.SetMaterial(null);

                        OutputCassetteLifter.ResetForNewRun();
                        OutputCassetteLifter.SetMaterial(null);

                    }

                    return 0; // 모두 OK
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    failureSummary = "Ready 이동 중 예외";
                    return -1;
                }
            }, token);

            var form = new ProgressForm("Manual Running", "MoveUnitsToReady", t, null);
            try
            {
                form.ShowDialog();

                if (form.DialogResult == DialogResult.Cancel)
                {
                    // 사용자가 Stop을 눌렀을 가능성 → 상위 토큰 취소 시도(있으면)
                    //try { _homeCts?.Cancel(); } catch { }
                    //return -1;
                }

                if (t.IsFaulted)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Ready 이동 중 예외!", t.Exception?.GetBaseException().Message);
                    return -1;
                }

                if (t.IsCanceled)
                {
                    throw new OperationCanceledException();
                }

                var rc = await t.ConfigureAwait(true);
                if (rc != 0)
                {
                    MessageBox.Show("Ready 이동 실패: " + (failureSummary ?? string.Empty),
                        "Ready 실패", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return rc;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("Ready 이동 처리 중 예외: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
        }


        #endregion

    }
}
