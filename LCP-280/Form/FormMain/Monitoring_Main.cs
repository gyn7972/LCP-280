using Newtonsoft.Json.Linq;
using QMC.Common;
using QMC.Common.Controls;
using QMC.Common.Motions;
using QMC.Common.PKGTester;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Component.FormDlg;
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
        private readonly System.Windows.Forms.Timer _seqUiTimer = new System.Windows.Forms.Timer();
        private EventHandler _seqUiChangedHandler;
        private static readonly string[] _seqOrder =
        {
            "Wafer",
            "LoadArm",
            "Index",
            "UnloadArm",
            "Bin"
        };

        private CancellationTokenSource _autoReadyCts;
       
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

        private TaktMonitorDialog _taktDialog;
        private WaferTotalSummaryViewerForm _waferTotalSummaryViewerForm;

        // Add
        private PKGTester Tester => Equipment.Instance.Tester;
        // 입력 스테이지 최신 웨이퍼 캐시
        private MaterialWafer _lastInputWafer;

        // [ADD] Home(축 초기화) 전에는 모든 모션을 차단하는 공통 Guard
        private bool EnsureAxisReadyOrShowMessage(string actionName)
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스를 찾을 수 없습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                bool bRet = eq.EnsureAxisReadyForAutoOrMove(actionName);
                return bRet;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "초기화 필요",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

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
            if (Tester != null)
            {
                Tester.OnMeasureCompleted += Tester_OnMeasureCompleted;
                Tester.OnMeasureAborted += Tester_OnMeasureAborted;
                casSpectrumViewer1.AttachSpectrometer(Tester.Spectrometer);
            }

            if (InputFeeder != null)
            {
                InputFeeder.WaferIdChanged -= InputFeeder_WaferIdChanged;
                InputFeeder.WaferIdChanged += InputFeeder_WaferIdChanged;
            }

            if (OutputFeeder != null)
            {
                OutputFeeder.BinIdChanged -= OutputFeeder_WaferIdChanged;
                OutputFeeder.BinIdChanged += OutputFeeder_WaferIdChanged;
            }

            // [ADD] Equipment snapshot 기반으로 Auto 버튼 상태 동기화(Ready/Start/Stop 표시 포함)
            try
            {
                var eq = Equipment.Instance;

                _seqUiChangedHandler = (s, e) =>
                {
                    if (IsDisposed || Disposing) return;
                    if (InvokeRequired) BeginInvoke(new Action(RefreshAutoButtonsFromEquipment));
                    else RefreshAutoButtonsFromEquipment();
                };

                eq.SequenceUiStateChanged += _seqUiChangedHandler;

                _seqUiTimer.Interval = 200;
                _seqUiTimer.Tick += (s, e) => RefreshAutoButtonsFromEquipment();
                _seqUiTimer.Start();
            }
            catch { }

        }

        private void InputFeeder_WaferIdChanged(string waferId)
        {
            if (IsDisposed) 
                return;

            Action ui = () =>
            {
                if (IsDisposed) return;
                dieInputControl1?.SetWaferId(string.IsNullOrWhiteSpace(waferId) ? "N/A" : waferId);
            };

            if (InvokeRequired) BeginInvoke(ui);
            else ui();
        }

        private void OutputFeeder_WaferIdChanged(string waferId)
        {
            if (IsDisposed)
                return;

            Action ui = () =>
            {
                if (IsDisposed) 
                    return;
                dieOutputControl1?.SetWaferId(string.IsNullOrWhiteSpace(waferId) ? "N/A" : waferId);
            };

            if (InvokeRequired) BeginInvoke(ui);
            else ui();
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
            try
            {
                _seqUiTimer.Stop();
                var eq = Equipment.Instance;
                if (eq != null && _seqUiChangedHandler != null)
                    eq.SequenceUiStateChanged -= _seqUiChangedHandler;
            }
            catch { }

            try
            {
                if (_taktDialog != null && !_taktDialog.IsDisposed)
                {
                    _taktDialog.Close();
                    _taktDialog = null;
                }
            }
            catch { }

            //_waferTotalSummaryViewerForm
            try
            {
                if (_waferTotalSummaryViewerForm != null && !_waferTotalSummaryViewerForm.IsDisposed)
                {
                    _waferTotalSummaryViewerForm.Close();
                    _waferTotalSummaryViewerForm = null;
                }
            }
            catch { }

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

            try
            {
                if (InputFeeder != null)
                    InputFeeder.WaferIdChanged -= InputFeeder_WaferIdChanged;
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
                    dieOutputControl1.Refresh();
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
        }

        private void OutputStage_DiePlaced(object sender, OutputStage.DiePlacedEventArgs e)
        {
            //Console.WriteLine($"[Out] Placed at Bin ({e.BinX},{e.BinY})");
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

            var presentCount = GetPresentWaferCount(Cassette);
            this.inputWaferCarrierControl1.UpdateWaferCount(presentCount);
            Log.Write("Monitoring_Main", $"Input Cassette Updated: ID={Cassette.CarrierId}, Slots={Cassette.SlotCount}, Present={presentCount}");
        }

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
            HandleWaferSlotSelection(e.SlotNumber, e.State);
        }

        private void OnInputWaferSlot_SelectionChanged(object sender, WaferSelectMapView.SlotSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                Console.WriteLine($"[WaferMap] Slot {e.SlotNumber} selected with order {e.SelectionOrder}. State: {e.State}");
                OnWaferSlotSelected(e.SlotNumber, e.SelectionOrder, e.State);
            }
            else
            {
                Console.WriteLine($"[WaferMap] Slot {e.SlotNumber} deselected");
                OnWaferSlotDeselected(e.SlotNumber);
            }

            UpdateWaferSelectionStatus();
        }

        private void HandleWaferSlotSelection(int slotNumber, WaferSelectMapView.SlotDisplayState state)
        {
            switch (state)
            {
                case WaferSelectMapView.SlotDisplayState.Present:
                    Console.WriteLine($"웨이퍼가 있는 Slot {slotNumber} 처리");
                    break;

                case WaferSelectMapView.SlotDisplayState.Empty:
                    Console.WriteLine($"빈 Slot {slotNumber} 처리");
                    break;
            }
        }

        private void OnWaferSlotSelected(int slotNumber, int selectionOrder, WaferSelectMapView.SlotDisplayState state)
        {
            Console.WriteLine($"웨이퍼 처리 순서 {selectionOrder}: Slot {slotNumber}");
            UpdateStatusInfo($"Slot {slotNumber} 선택됨 (순서: {selectionOrder})");
        }

        private void OnWaferSlotDeselected(int slotNumber)
        {
            Console.WriteLine($"Slot {slotNumber} 선택 해제");
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
            }
        }

        #endregion

        #region Input Die 이벤트 처리
        private void OnDieInput_MotorMoveRequested(object sender, DisplayView_DieInput.DisplayItemEventArgs e)
        {
            if (e == null || e.Item == null)
            {
                Console.WriteLine("[Input] 모터 이동 요청: 이벤트/아이템 NULL");
                return;
            }

            var die = FindInputDieByDisplayItem(e.Item);
            if (die == null)
            {
                Console.WriteLine($"[Input] 매핑 실패 → 맵 좌표 사용 이동. Map({e.Item.Position.X},{e.Item.Position.Y})");
                MovePickMotorTo(e.Item.Position.X, e.Item.Position.Y, new Point((e.Item.Position.X),
                                                                               (e.Item.Position.Y)));
                ShowMotorMovingStatus($"Input 모터가 맵좌표 ({e.Item.Position.X:0.###},{e.Item.Position.Y:0.###})로 이동 중...(Die 매핑 실패)");
                return;
            }

            double stageX = die.CenterX;
            double stageY = die.CenterY;

            if (Math.Abs(stageX) < 0.0001 && Math.Abs(stageY) < 0.0001)
            {
                stageX = die.MapX;
                stageY = die.MapY;
            }

            Console.WriteLine(
                $"[Input] 모터 이동 요청(DieId={die.Index}) " +
                $"Map({die.MapX:0.###},{die.MapY:0.###}) → Stage({stageX:0.###},{stageY:0.###}) Center({die.CenterX:0.###},{die.CenterY:0.###})");

            var mapPoint = new Point((int)die.MapX, (int)die.MapY);
            MovePickMotorTo(stageX, stageY, mapPoint);
        }

        private MaterialDie FindInputDieByDisplayItem(DisplayView_DieInput.DisplayItem displayItem)
        {
            try
            {
                var dies = _lastInputWafer?.Dies;
                if (dies == null) 
                    return null;

                if (displayItem.DieId >= 0)
                {
                    var byIndex = dies.FirstOrDefault(d => d.Index == displayItem.DieId);
                    if (byIndex != null) 
                        return byIndex;
                }

                var mx = (displayItem.Position.X);
                var my = (displayItem.Position.Y);
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

        private void MovePickMotorTo(double stageX, double stageY, Point? mapPointOverride = null)
        {
            try
            {
                if (!EnsureAxisReadyOrShowMessage("Monitoring.PickMove")) return;

                Console.WriteLine($"Pick 모터 이동(Stage): ({stageX:0.###}, {stageY:0.###})");

                try
                {
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

                System.Threading.Tasks.Task.Delay(1000).ContinueWith(t =>
                {
                    if (IsDisposed) return;
                    this.Invoke(new Action(() =>
                    {
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
        private void OnDieRotation_Requested(object sender, int rotationOffset)
        {
            if (!EnsureAxisReadyOrShowMessage("Monitoring.RotaryMove")) return;

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

            UpdateRotationStatus(Rotary.GetLoadIndexNo() + 1);
        }

        private void UpdateRotationStatus(int offset)
        {
            Console.WriteLine($"로더 위치는 '{offset}'입니다.");
            dieIndexSelectControl1.UpdateRotationUI(offset);
        }

        #endregion

        #region 헬퍼 메서드
        private void UpdateStatusInfo(string message)
        {
            Console.WriteLine($"[Status] {message}");
        }

        private void ShowMotorMovingStatus(string message)
        {
            var mb = new MessageBoxOk();
            mb.ShowDialog("Info.", message);
            Console.WriteLine($"[Motor] {message}");
        }

        private void MovePickMotorTo(double x, double y)
        {
            try
            {
                Console.WriteLine($"Pick 모터 이동: ({x}, {y})");

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
            inputWaferCarrierControl1.SetWaferCarrierId("N/A");
            inputWaferCarrierControl1.UpdateWaferCount(0);

            outputWaferCarrierControl1.SetWaferCarrierId("N/A");
            outputWaferCarrierControl1.UpdateWaferCount(0);

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

            if (_autoStarting)
            {
                Log.Write("Monitoring_Main", "Auto Start 작업 진행 중 - 요청 무시");
                return;
            }

            if (EnsureAxisReadyOrShowMessage("AutoReady") == false)
            {
                Log.Write("Monitoring_Main", "Auto Ready 차단: 축 Home/초기화 필요");
                NotifyAutoSequenceStateChanged("Ready", false);
                return;
            }

            try
            {
                sequenceAutoControl.SetButtonEnabled("Ready", false);
                sequenceAutoControl.Enabled = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            _autoReadyBusy = true;
            _autoReady = true;

            _autoReadyCts?.Cancel();
            _autoReadyCts = new CancellationTokenSource();
            var ct = _autoReadyCts.Token;

            Task<int> readyTask = Task.Run(() =>
            {
                try
                {
                    var ok = Equipment.Instance.SequenceReadyAllAsync(ct).GetAwaiter().GetResult();
                    return ok ? 0 : -1;
                }
                catch (OperationCanceledException)
                {
                    return -2;
                }
                catch (Exception ex)
                {
                    Log.Write("Monitoring_Main", $"Auto Ready 예외: {ex.Message}");
                    return -1;
                }
            }, ct);

            var form = new ProgressForm("Auto Ready", "SequenceReadyAllAsync", readyTask, this.Rotary);

            bool success = false;
            try
            {
                form.ShowDialog(this);

                if (form.DialogResult == DialogResult.Cancel)
                {
                    try { _autoReadyCts.Cancel(); } catch { }
                    Log.Write("Monitoring_Main", "Auto Ready 취소 요청");
                }

                if (readyTask.Status == TaskStatus.RanToCompletion)
                {
                    if (readyTask.Result == 0)
                    {
                        success = true;
                    }
                    else if (readyTask.Result == -2)
                    {
                        // 취소
                        success = false;
                    }
                    else
                    {
                        MessageBox.Show("Auto Ready 실패(동시 실행 중/Ready 실패)", "오류",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        success = false;
                    }
                }
                else
                {
                    success = false;
                }
            }
            finally
            {
                try { sequenceAutoControl.SetButtonEnabled("Ready", true); } catch { }
                sequenceAutoControl.Enabled = true;

                _autoReadyBusy = false;
                _autoReady = false;

                // 성공하면 true 유지, 실패/취소면 false
                NotifyAutoSequenceStateChanged("Ready", success);
            }
        }

        private async void HandleAutoStart()
        {
            if (_autoStarting)
                return;

            if (!EnsureAxisReadyOrShowMessage("AutoStart"))
            {
                Log.Write("Monitoring_Main", "Auto Start 차단: 축 Home/초기화 필요");
                NotifyAutoSequenceStateChanged("Start", false);
                return;
            }

            // 중복 진입 방지: 호출 직전에 true
            _autoStarting = true;

            bool success = false;
            try
            {
                var cts = new CancellationTokenSource();
                bool ok = await Equipment.Instance.SequenceStartAllAsync(cts.Token).ConfigureAwait(true);
                if (!ok)
                {
                    MessageBox.Show("Auto Start 실패(동시 실행 중이거나 Start 실패)", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                    return;
                }

                _autoReady = false;
                success = true;
                Log.Write("Monitoring_Main", "Auto Start 완료 (Equipment.SequenceStartAllAsync)");
            }
            catch (OperationCanceledException)
            {
                Log.Write("Monitoring_Main", "Auto Start 취소됨");
                success = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                success = false;
            }
            finally
            {
                // 성공하면 true 유지, 실패/취소면 false
                NotifyAutoSequenceStateChanged("Start", success);

                // Start는 성공 후에도 “실행 중”으로 간주하므로 _autoStarting은 유지할지 정책 필요.
                // 기존 로직을 따르되, 성공이면 true 유지 / 실패면 false
                _autoStarting = success;
            }
        }

        // 2) AutoStop 대기부: "State == Stop" 기준으로 수정 (축 정지와 함께)
        private async void HandleAutoStop()
        {
            var eq = Equipment.Instance;
            if (eq == null)
                return;

            bool success = false;

            try
            {
                // Stop 실행 중 표시 (성공 유지 여부는 finally의 success로 결정)
                // (단일 활성 정책 때문에 other 버튼은 자동으로 default 처리됨)
                NotifyAutoSequenceStateChanged("Stop", true);

                _autoReady = false;
                _autoStarting = false;
                sequenceAutoControl.ResetAllButtons();

                var cts = new CancellationTokenSource();
                bool ok = await Equipment.Instance.SequenceStopAllAsync(cts.Token).ConfigureAwait(true);
                if (!ok)
                {
                    MessageBox.Show("Auto Stop 실패(동시 실행 중이거나 Stop 실패)", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                    return;
                }

                // 물리적 완전 정지 대기(기존 로직 유지)
                var axisMgr = eq.AxisManager;
                var waitCts = new CancellationTokenSource();
                int timeoutMs = 50000;
                int lastPercent = 0;
                string lastDetail = string.Empty;

                Action<int, string> progressCb = (p, info) =>
                {
                    lastPercent = p;
                    lastDetail = info;
                };

                var waitTask = WaitForFullPhysicalStopAsync(
                    eq,
                    axisMgr,
                    timeoutMs,
                    waitCts.Token,
                    progressCb,
                    emergencyOnCancel: true);

                var form = new ProgressForm("Auto Stop",
                    "모션/시퀀스 완전 정지 대기 중...",
                    waitTask,
                    this);

                form.CustomStatusProvider = () => new Tuple<int, string>(lastPercent, lastDetail);

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
                }

                Log.Write("Operator_Main", $"Auto Stop 종료(rc={rc}) detail='{lastDetail}'");

                if (rc == 0)
                {
                    TryTransitionToManualIfStopped(eq);
                    success = true;
                }
                else
                {
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
                    else
                    {
                        MessageBox.Show("완전 정지 확인 실패.", "경고",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"설비 정지 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                success = false;
            }
            finally
            {
                // 성공하면 true 유지, 실패면 false
                NotifyAutoSequenceStateChanged("Stop", success);
            }
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
                int totalAxes = axes.Length; // 마지막 축은 나중에 적용하자.

                while (true)
                {
                    token.ThrowIfCancellationRequested();

                    // 1) Unit Stop / Busy 판정
                    int stoppedUnits = 0;
                    var busyUnits = new List<string>();
                    foreach (var u in units)
                    {
                        bool isStopped = (u.RunUnitStatus == BaseUnit.UnitStatus.Stopped
                                          || u.RunUnitStatus == BaseUnit.UnitStatus.Error)
                                        && ((u.State == BaseUnit.ProcessState.Stop
                                        && u.IsStop) || u.State == BaseUnit.ProcessState.None
                                        || u.State == BaseUnit.ProcessState.Error);

                        if (isStopped)
                            stoppedUnits++;
                        else
                            busyUnits.Add(u.UnitName);
                    }

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
                        try 
                        {
                            if (ax.AxisNo == 26 || ax.AxisNo == 25)
                            {
                                done = ax.IsMoveDone(); 
                            }
                            else
                            {
                                done = ax.IsMoveDone();
                            }
                        }
                        catch { done = false; }

                        if (done)
                        {
                            //26번은 나중에 적용
                            //if (ax.AxisNo != 26)
                            {
                                idleAxes++;
                            }
                        }
                        else
                        {
                            //26번은 나중에 적용
                            //if(ax.AxisNo != 26)
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
                if (!IsAllUnitsStopped(eq)) 
                    return;

                if (!AreAllAxesIdle(eq.AxisManager)) 
                    return;

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

        private void HandleAutoCycleStop()
        {
            NotifyAutoSequenceStateChanged("CycleStop", true);
            Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("CycleStop", false); }));
            });

            Log.Write("Operator_Main", "Auto CycleStop 실행");
        }

        private void HandleAutoReset()
        {
            if (_autoStarting)
            {
                Log.Write("Monitoring_Main", "Auto Start 작업 진행 중 - 요청 무시");
                return;
            }

            if (_autoReady)
            {
                Log.Write("Monitoring_Main", "Auto Ready 작업 진행 중 - 요청 무시");
                return;
            }

            HandleAutoCycleStop();

            if (EnsureAxisReadyOrShowMessage("AutoReset") == false)
            {
                Log.Write("Monitoring_Main", "Auto Reset 차단: 축 Home/초기화 필요");
                NotifyAutoSequenceStateChanged("Reset", false);
                return;
            }

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

            var form = new ProgressForm("Auto Reset", "장비 상태 초기화 중...", resetTask, this);
            form.StopProcess += _ =>
            {
                MessageBox.Show("Auto Reset은 취소를 지원하지 않습니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            bool success = false;

            try
            {
                // Reset 실행 중 표시
                NotifyAutoSequenceStateChanged("Reset", true);

                form.ShowDialog(this);

                if (resetTask.IsFaulted)
                {
                    var ex = resetTask.Exception?.GetBaseException();
                    MessageBox.Show($"Auto Reset 중 예외: {ex?.Message}", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                }
                else if (resetTask.Status == TaskStatus.RanToCompletion && resetTask.Result != 0)
                {
                    MessageBox.Show("Auto Reset 실패", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    success = false;
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                success = false;
            }
            finally
            {
                // 성공하면 true 유지, 실패면 false
                NotifyAutoSequenceStateChanged("Reset", success);
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

        private void RefreshAutoButtonsFromEquipment()
        {
            if (IsDisposed || Disposing) return;
            if (sequenceAutoControl == null) return;

            var eq = Equipment.Instance;
            var snap = eq?.GetSequenceUiSnapshot();
            if (snap == null) return;

            var ready = new HashSet<string>(snap.Ready ?? new string[0], StringComparer.OrdinalIgnoreCase);
            var running = new HashSet<string>(snap.Running ?? new string[0], StringComparer.OrdinalIgnoreCase);

            bool allReady = _seqOrder.All(s => ready.Contains(s));
            bool anyRunning = running.Count > 0;
            bool allStopped = running.Count == 0;

            // 표시 통일: Equipment snapshot이 "진짜 상태"
            if(anyRunning)
            {
                NotifyAutoSequenceStateChanged("Ready", false);
                NotifyAutoSequenceStateChanged("Start", true);
                NotifyAutoSequenceStateChanged("Stop", false);
            }
            else if(allReady)
            {
                NotifyAutoSequenceStateChanged("Ready", true);
                NotifyAutoSequenceStateChanged("Start", false); // Running 중이면 Start ON(실행 중 표시)
                NotifyAutoSequenceStateChanged("Stop", false);  // Running 없으면 Stop ON(정지 상태 표시)
            }
            else if (allStopped)
            {
                NotifyAutoSequenceStateChanged("Ready", false);
                NotifyAutoSequenceStateChanged("Start", false); // Running 중이면 Start ON(실행 중 표시)
                NotifyAutoSequenceStateChanged("Stop", true);  // Running 없으면 Stop ON(정지 상태 표시)
            }
            else
            {
                NotifyAutoSequenceStateChanged("Ready", allReady);
                NotifyAutoSequenceStateChanged("Start", anyRunning); // Running 중이면 Start ON(실행 중 표시)
                NotifyAutoSequenceStateChanged("Stop", allStopped);  // Running 없으면 Stop ON(정지 상태 표시)
            }

            //NotifyAutoSequenceStateChanged("Ready", allReady);
            //NotifyAutoSequenceStateChanged("Start", anyRunning); // Running 중이면 Start ON(실행 중 표시)
            //NotifyAutoSequenceStateChanged("Stop", allStopped);  // Running 없으면 Stop ON(정지 상태 표시)

            // 로컬 플래그도 화면 표시와 엇갈리지 않게 보정(버튼 중복동작 방지 목적)
            _autoReady = allReady;
            _autoStarting = anyRunning;
        }

        #endregion

        private void btnTack_Click(object sender, EventArgs e)
        {
            try
            {
                if (OutputStage == null)
                    return;

                if (_taktDialog != null && !_taktDialog.IsDisposed)
                {
                    _taktDialog.RefreshView();
                    _taktDialog.BringToFront();
                    _taktDialog.Activate();
                    return;
                }

                _taktDialog = new TaktMonitorDialog("DiePlace Takt", OutputStage.DiePlaceTaktTimer);
                _taktDialog.FormClosed += (s, ev) => { _taktDialog = null; };
                _taktDialog.Show(this);
            }
            catch (Exception ex)
            {
                Log.Write("Monitoring_Main", $"btnTack_Click exception: {ex}");
                MessageBox.Show(ex.Message, "Takt dialog error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _taktDialog = null;
            }
        }

        private void btnMapMatch_Click(object sender, EventArgs e)
        {
            if(true)
            {
                var dlg = new QMC.LCP_280.Process.Component.FormMapMatchManual();
                dlg.BindEquipmentInStageCamera();

                // [ADD] 장비가 실제 사용하는 InputStage wafer를 dlg에 주입
                try
                {
                    var wafer = InputStage?.GetMaterialWafer();
                    if (wafer != null)
                        dlg.BindTargetWafer(wafer);
                }
                catch { }

                dlg.ShowDialog(this);
            }
            else
            {
                var dlg = new QMC.LCP_280.Process.Component.FormMapMatchManual();
                dlg.BindEquipmentInStageCamera();

                // [ADD] 현재 InputStage 웨이퍼 -> ScanItems 주입 (클릭 Pick 가능해짐)
                try
                {
                    var wafer = InputStage?.GetMaterialWafer();
                    var dies = wafer?.Dies;

                    if (dies != null && dies.Count > 0)
                    {
                        var scanItems = new List<QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem>(dies.Count);
                        lock (dies)
                        {
                            for (int i = 0; i < dies.Count; i++)
                            {
                                var d = dies[i];
                                if (d == null) continue;

                                scanItems.Add(new QMC.Common.Controls.DisplayView_DieScanMap.DisplayItem
                                {
                                    DieId = d.Index,
                                    Info = wafer?.WaferId ?? "SCAN",
                                    // DisplayView에서 hit-test/그리기 기준은 Position이지만,
                                    // FormMapMatchManual은 Pick 시 DieMap을 읽어 사용함.
                                    // 따라서 둘 다 일단 Map으로 통일.
                                    Position = new Point((int)d.MapX, (int)d.MapY),
                                    DieMap = new Point((int)d.MapX, (int)d.MapY),
                                    State = QMC.Common.Controls.DisplayView_DieScanMap.ItemState.Present
                                });
                            }
                        }

                        dlg.SetScanItems(scanItems);
                    }
                }
                catch { }

                // Download는 dlg 내부에서 btnPickDownload 누르면 파일 chooser로 로드/표시 가능
                dlg.ShowDialog(this);
            }
        }

        private void btnProcessStatus_Click(object sender, EventArgs e)
        {
            //_waferTotalSummaryViewerForm
            try
            {
                if (_waferTotalSummaryViewerForm != null && !_waferTotalSummaryViewerForm.IsDisposed)
                {
                    //_waferTotalSummaryViewerForm.RefreshView();
                    _waferTotalSummaryViewerForm.BringToFront();
                    _waferTotalSummaryViewerForm.Activate();
                    return;
                }

                _waferTotalSummaryViewerForm = new WaferTotalSummaryViewerForm();
                _waferTotalSummaryViewerForm.FormClosed += (s, ev) => { _taktDialog = null; };
                _waferTotalSummaryViewerForm.Show(this);
            }
            catch (Exception ex)
            {
                Log.Write("Monitoring_Main", $"btnProcessStatus_Click exception: {ex}");
                MessageBox.Show(ex.Message, "_waferTotalSummaryViewerForm error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _waferTotalSummaryViewerForm = null;
            }

        }
    }
}
