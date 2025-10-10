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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl;


namespace QMC.LCP_280.Process
{
    [FormOrder(1)]
    public partial class Monitoring_Main : Form
    {
        private bool _autoReady = false;
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
        private OutputFeeder OutputFeeder { get; set; }
        private InputStageEjector InputStageEjector {get; set; }


        public Monitoring_Main() : this(
            TryGetUnit<InputCassetteLifter>("InputCassetteLifter"),
            TryGetUnit<InputFeeder>("InputFeeder"),
            TryGetUnit<InputStage>("InputStage"),
            TryGetUnit<InputDieTransfer>("InputDieTransfer"),
            TryGetUnit<Rotary>("Rotary"),
            TryGetUnit<OutputDieTransfer>("OutputDieTransfer"),
            TryGetUnit<OutputStage>("OutputStage"),
            TryGetUnit<OutputCassetteLifter>("OutputCassetteLifter"),
            TryGetUnit<IndexLoadAligner>("IndexLoadAligner"),
            TryGetUnit<IndexChipProbeController>("IndexChipProbeController"),
            TryGetUnit<IndexUnloadAligner>("IndexUnloadAligner"),
            TryGetUnit<OutputFeeder>("OutputFeeder"),
            TryGetUnit<InputStageEjector>("InputStageEjector"))
        {

        }

        public Monitoring_Main(InputCassetteLifter inputcassetteLifter, InputFeeder inputFeeder,
            InputStage inputStage, InputDieTransfer inputDieTransfer, Rotary rotary,
            OutputDieTransfer outputDieTransfer, OutputStage outputStage, OutputCassetteLifter outputCassetteLifter,
            IndexLoadAligner indexLoadAligner, IndexChipProbeController indexChipProbeController,
            IndexUnloadAligner indexUnloadAligner, OutputFeeder outputFeeder, InputStageEjector inputStageEjector)
        {
            InitializeComponent();

            #region Chart
            InputCassetteLifter = inputcassetteLifter;
            InputFeeder = inputFeeder;
            InputStage = inputStage;
            InputDieTransfer = inputDieTransfer;
            Rotary = rotary;
            OutputDieTransfer = outputDieTransfer;
            OutputStage = outputStage;
            OutputCassetteLifter = outputCassetteLifter;
            IndexLoadAligner = indexLoadAligner;
            IndexChipProbeController = indexChipProbeController;
            IndexUnloadAligner = indexUnloadAligner;
            OutputFeeder = outputFeeder;
            InputStageEjector = inputStageEjector;

            _readySequences = new HashSet<string>();
            _startSequences = new HashSet<string>();
            sequenceAutoControl.SequenceButtonRequested += OnAutoSequenceButtonRequested;
            
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
            dieIndexSelectControl1.RotationRequested += OnDieRotation_Requested;


            inputStage.EventUpdateUIWafer += InputStage_EventUpdateUIWafer;
            outputStage.EventUpdateUIWafer += OutputStage_EventUpdateUIWafer;
            InputCassetteLifter.EventUpdateUICassette += InputCassetteLifter_EventUpdateUICassette;
            OutputCassetteLifter.EventUpdateUICassette += OutputCassetteLifter_EventUpdateUICassette;

            // 이벤트 구독: 픽업 완료 시 입력 뷰에서 해당 다이를 제거(Picked/Empty)로 반영
            if (InputDieTransfer != null)
                InputDieTransfer.DiePicked += InputDieTransfer_DiePicked;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (InputDieTransfer != null)
                InputDieTransfer.DiePicked -= InputDieTransfer_DiePicked;

            base.OnFormClosed(e);
        }

        private void OutputStage_EventUpdateUIWafer(MaterialWafer wafer)
        {
            this.dieOutputControl1.SetDieList(wafer.Dies);
        }

        private void InputStage_EventUpdateUIWafer(MaterialWafer wafer)
        {
            if(wafer.WaferId == string.Empty)
                wafer.WaferId = string.Format("QMC_{0}", wafer.Dies.Count);

            dieInputControl1.ResetPickedMarks();

            this.dieInputControl1.SetWaferId(wafer.WaferId);
            this.dieInputControl1.SetDieList(wafer.Dies);

        }
        private void InputDieTransfer_DiePicked(object sender, InputDieTransfer.DiePickedEventArgs e)
        {
            dieInputControl1.MarkCurrentPicked(new System.Drawing.Point(e.MapX, e.MapY));
            // MarkDieRemoved는 내부적으로 Invoke 처리하므로 바로 호출해도 안전
            //dieInputControl1.MarkDieRemoved(new System.Drawing.Point(e.MapX, e.MapY), showAsPicked: true);
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
            _autoReady = !_autoReady;
            NotifyAutoSequenceStateChanged("Ready", _autoReady);

            if (_autoReady)
            {
                _autoReadyCts?.Cancel();
                _autoReadyCts = new CancellationTokenSource();
                var ct = _autoReadyCts.Token;
                var prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                ExecuteAutoReadyAsync(ct).ContinueWith(t =>
                {
                    try
                    {
                        if (IsDisposed || Disposing) return;
                        BeginInvoke(new Action(() =>
                        {
                            Cursor.Current = prev;
                            if (t.IsFaulted)
                                Log.Write("Operator_Main", $"Auto Ready 예외: {t.Exception?.GetBaseException().Message}");
                            if (t.IsCanceled)
                                Log.Write("Operator_Main", "Auto Ready 취소됨");
                        }));
                    }
                    catch { }
                });
                Log.Write("Operator_Main", "Auto Ready ON");
            }
            else
            {
                _autoReadyCts?.Cancel();
                Log.Write("Operator_Main", "Auto Ready OFF");
            }
        }

        private async void HandleAutoStart()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;

            try
            {
                // UI 토글 알림(즉시 반영), 최종 상태는 Eq.StateChanged에서 수렴
                NotifyAutoSequenceStateChanged("Start", true);

                // 설비 전체 시작
                var ok = await eq.StartAllUnitsAsync().ConfigureAwait(true);
                if (!ok)
                {
                    NotifyAutoSequenceStateChanged("Start", false);
                    MessageBox.Show("설비 시작 실패", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _autoStarting = false;
                    return;
                }

                // 성공 시 내부 플래그 정리
                _autoReady = false;
                _autoStarting = true;
                Log.Write("Operator_Main", "Auto Start 완료 (Equipment.StartAllUnitsAsync)");
            }
            catch (Exception ex)
            {
                NotifyAutoSequenceStateChanged("Start", false);
                _autoStarting = false;
                Log.Write(ex);
                MessageBox.Show($"설비 시작 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async void HandleAutoStop()
        {
            var eq = Equipment.Instance;
            if (eq == null) return;

            try
            {
                NotifyAutoSequenceStateChanged("Stop", true);

                // 로컬 시퀀스 토글/상태 정리(UI만)
                _autoReady = false;
                _autoStarting = false;
                _readySequences.Clear();
                _startSequences.Clear();
                try { sequenceAutoControl.ResetAllButtons(); } catch { }

                // 설비 전체 정지
                var ok = await eq.StopAllUnitsAsync().ConfigureAwait(true);
                if (!ok)
                {
                    MessageBox.Show("설비 정지 실패(일부 유닛 타임아웃 가능)", "오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Log.Write("Operator_Main", "Auto Stop 완료 (Equipment.StopAllUnitsAsync)");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"설비 정지 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // 최종 UI 토글 해제, 실제 상태는 Eq.StateChanged에서 최종 수렴
                NotifyAutoSequenceStateChanged("Stop", false);
            }

            //_autoReady = false;
            //_autoStarting = false;
            //_readySequences.Clear();
            //_startSequences.Clear();
            //sequenceAutoControl.ResetAllButtons();
            //sequenceManualControl.ResetAllButtons();
            //NotifyAutoSequenceStateChanged("Stop", true);
            //Task.Delay(500).ContinueWith(_ =>
            //{
            //    this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("Stop", false); }));
            //});
            //ExecuteAutoStop();
            //Log.Write("Operator_Main", "Auto Stop 실행 - 모든 Sequence 초기화");
        }

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
            NotifyAutoSequenceStateChanged("Reset", true);
            Task.Delay(500).ContinueWith(_ =>
            {
                this.Invoke(new Action(() => { NotifyAutoSequenceStateChanged("Reset", false); }));
            });
            ExecuteAutoReset();
            Log.Write("Operator_Main", "Auto Reset 실행");
        }

        private void NotifyAutoSequenceStateChanged(string command, bool isActive)
        {
            sequenceAutoControl.OnAutoSequenceStateChanged(new AutoSequenceStateChangedEventArgs
            {
                Command = command,
                IsActive = isActive
            });
        }

        private async Task ExecuteAutoReadyAsync(CancellationToken ct)
        {
            Log.Write("Operator_Main", "Auto Ready 시작 (공통 로직 사용)");
            bool ok = await ReadyAllSequencesAsync(ct);
            if (ok) Log.Write("Operator_Main", "Auto Ready 완료 (모든 시퀀스 Ready ON)");
            else Log.Write("Operator_Main", "Auto Ready 실패");
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
                //NotifySequenceStateChanged(sequenceName, "Ready", true, false);
            }
            return ok;
        }
        private async Task<bool> TryReadySequenceAsync(string sequenceName, CancellationToken ct)
        {
            int rc;
            switch (sequenceName)
            {
                case "InputWafer": rc = await HandleInputWaferReadyAsync(ct); break;
                case "ChipLoading": rc = await HandleChipLoadingReadyAsync(ct); break;
                case "Process": rc = await HandleProcessReadyAsync(ct); break;
                case "ChipUnloading": rc = await HandleChipUnloadingReadyAsync(ct); break;
                case "OutputWafer": rc = await HandleOutputWaferReadyAsync(ct); break;
                default:
                    Log.Write("Operator_Main", $"알 수 없는 Sequence '{sequenceName}' Ready 요청");
                    return false;
            }
            if (rc != 0)
            {
                Log.Write("Operator_Main", $"{sequenceName} Ready 실패(rc={rc})");
                return false;
            }
            return true;
        }
        #region Handle Manual Async Ready
        private Task<int> HandleInputWaferReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                int nRet = InputFeeder?.EnsureReady() ?? -1;
                if (nRet != 0) return nRet;
                ct.ThrowIfCancellationRequested();
                nRet = InputStageEjector?.CheckReady() ?? -1;
                return nRet;
            }, ct);
        }
        private Task<int> HandleChipLoadingReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return InputDieTransfer?.EnsureReady() ?? -1;
            }, ct);
        }
        private Task<int> HandleProcessReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                int nRet = IndexLoadAligner?.EnsureReady() ?? -1;
                if (nRet != 0) return nRet;
                ct.ThrowIfCancellationRequested();
                nRet = IndexChipProbeController?.EnsureReady() ?? -1;
                return nRet;
            }, ct);
        }
        private Task<int> HandleChipUnloadingReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return OutputDieTransfer?.EnsureReady() ?? -1;
            }, ct);
        }
        private Task<int> HandleOutputWaferReadyAsync(CancellationToken ct)
        {
            return Task.Run(() =>
            {
                ct.ThrowIfCancellationRequested();
                return OutputFeeder?.EnsureReady() ?? -1;
            }, ct);
        }
        #endregion


        private void ExecuteAutoCycleStop()
        {
        }

        private void ExecuteAutoReset()
        {
            InputCassetteLifter.SetMaterial(new Material());
            InputFeeder.SetMaterial(new Material());
            InputStage.SetMaterial(new Material());
            InputDieTransfer.SetMaterial(new Material());
            Rotary.SetMaterial(new Material());
            IndexLoadAligner.SetMaterial(new Material());
            IndexChipProbeController.SetMaterial(new Material());
            IndexUnloadAligner.SetMaterial(new Material());
            OutputDieTransfer.SetMaterial(new Material());
            OutputStage.SetMaterial(new Material());
            OutputFeeder.SetMaterial(new Material());
            OutputCassetteLifter.SetMaterial(new Material());
        }


        #endregion

    }
}
