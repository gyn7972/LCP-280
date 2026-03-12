using Org.BouncyCastle.Bcpg;
using QMC.Common;
using QMC.Common.Account;
using QMC.Common.Controls;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using static QMC.Common.Material;
using static QMC.LCP_280.Process.Component.MeasurementRecipe;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieInputControl : UserControl
    {
        public event EventHandler<DisplayView_DieInput.DisplayItemEventArgs> MotorMoveRequested;
        private List<MaterialDie> _dies = new List<MaterialDie>();


        #region 장비 좌표와 View 좌표 맴핑. 하드코딩으로 처리.
        // 화면 표시를 맵 중심(pivot) 기준 상대좌표(0,0 원점)로 보낼지 여부
        private bool _centerOnPivot = true;
        public bool CenterOnPivot
        {
            get => _centerOnPivot;
            set
            {
                if (_centerOnPivot == value) return;
                _centerOnPivot = value;
                SetDieList(_dies);
            }
        }
        // 회전/센터 기준 (MapX / MapY 범위 중심)
        private double _pivotX;
        private double _pivotY;
        private bool _hasPivot;
        // ===== [NEW] 장비↔뷰 고정 축 매핑 (하드코딩) =====
        // 장비의 X,Y를 View 좌표와 1:1 맞추기 위한 고정 변환을 먼저 적용합니다.
        // 필요에 따라 아래 상수를 설정하십시오.
        private sealed class AxisMap
        {
            public bool SwapXY;      // true면 X<->Y 교환
            public bool InvertX;     // true면 X 부호 반전
            public bool InvertY;     // true면 Y 부호 반전
        }
        // 장비→뷰로 보여줄 때 적용할 매핑
        private static readonly AxisMap EquipmentToView = new AxisMap
        {
            //기준.
            SwapXY = false,
            InvertX = true,
            InvertY = false
        };
        // 뷰→장비로 좌표를 돌려줄 때 적용할 역매핑
        private static readonly AxisMap ViewToEquipment = new AxisMap
        {
            //기준.
            SwapXY = false,
            InvertX = true,
            InvertY = false
        };
        private static PointD ApplyAxisMap(PointD p, AxisMap m)
        {
            double x = p.X, y = p.Y;
            if (m.SwapXY) { var t = x; x = y; y = t; }
            if (m.InvertX) x = -x;
            if (m.InvertY) y = -y;
            return new PointD(x, y);
        }
        private void RecalcPivot()
        {
            _hasPivot = false;
            if (_dies == null || _dies.Count == 0)
                return;

            double minX = _dies.Min(c => c.MapX);
            double maxX = _dies.Max(c => c.MapX);
            double minY = _dies.Min(c => c.MapY);
            double maxY = _dies.Max(c => c.MapY);

            _pivotX = (minX + maxX) * 0.5;
            _pivotY = (minY + maxY) * 0.5;
            _hasPivot = true;
        }
        #endregion

        private MapRotateOption _rotateDisplay = MapRotateOption.None;
        private MapMirrorOption _mirrorDisplay = MapMirrorOption.None;
        private MapPathStartCorner _pathStartCorner = MapPathStartCorner.BottomLeft;
        private MapPathPrimaryAxis _pathPrimaryAxis = MapPathPrimaryAxis.XFirst;
        private MapPathTraversalMode _pathTraversalMode = MapPathTraversalMode.Serpentine;
        public MapRotateOption WaferRotateView
        {
            get => _rotateDisplay;
            set
            {
                if (_rotateDisplay == value) 
                    return;
                _rotateDisplay = value;
            }
        }
        public MapMirrorOption WaferMirrorView
        {
            get => _mirrorDisplay;
            set
            {
                if (_mirrorDisplay == value) 
                    return;
                _mirrorDisplay = value;
            }
        }
        public MapPathStartCorner WaferPathStartCorner
        {
            get => _pathStartCorner;
            set
            {
                if (_pathStartCorner == value) 
                    return;
                _pathStartCorner = value;
            }
        }
        public MapPathPrimaryAxis WaferPathPrimaryAxis
        {
            get => _pathPrimaryAxis;
            set
            {
                if (_pathPrimaryAxis == value) 
                    return;
                _pathPrimaryAxis = value;
            }
        }
        public MapPathTraversalMode WaferPathTraversalMode
        {
            get => _pathTraversalMode;
            set
            {
                if (_pathTraversalMode == value) 
                    return;
                _pathTraversalMode = value;
            }
        }
        private void SyncWaferViewFromRecipe()
        {
            try
            {
                var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                if (recipe != null)
                {
                    //WaferRotateView = recipe.WaferRotate;
                    //WaferMirrorView = recipe.WaferMirror;
                    WaferPathStartCorner = recipe.WaferPathStartCorner;
                    WaferPathPrimaryAxis = recipe.WaferPathPrimaryAxis;
                    WaferPathTraversalMode = recipe.WaferPathTraversalMode;
                }
            }
            catch { /* ignore */ }
        }


        // 모델(MapX/MapY) -> 디스플레이 좌표
        private PointD ToDisplay(PointD model)
        {
            // 1) 레시피 옵션 동기화
            SyncWaferViewFromRecipe();

            // [수정 1] 원본 좌표에서 먼저 Pivot을 빼서 상대좌표(0,0)로 맞춥니다.
            PointD rel = _centerOnPivot
                ? new PointD(model.X - _pivotX, model.Y - _pivotY)
                : model;

            // [수정 2] 중심이 맞춰진 상태에서 장비→뷰 축 변환(Invert, Swap 등)을 적용합니다.
            PointD mapped = ApplyAxisMap(rel, EquipmentToView);

            return mapped;

            //// 1) 레시피 옵션 동기화
            //SyncWaferViewFromRecipe();
            //// 2) 고정 장비→뷰 매핑 먼저 적용
            //PointD mapped = ApplyAxisMap(model, EquipmentToView);

            //// 1) 센터 상대좌표 변환
            //PointD rel = _centerOnPivot
            //    ? new PointD(mapped.X - _pivotX, mapped.Y - _pivotY)
            //    : mapped;

            //return rel;
        }

        // 디스플레이 -> 모델(MapX/MapY)
        private PointD FromDisplay(PointD display)
        {
            // 1) 레시피 옵션 동기화
            SyncWaferViewFromRecipe();

            if (!_hasPivot)
            {
                return ApplyAxisMap(display, ViewToEquipment);
            }

            // [수정 3] 화면 좌표에서 먼저 뷰→장비 축 변환(역매핑)을 적용해 부호/방향을 원래대로 돌립니다.
            PointD unmapped = ApplyAxisMap(display, ViewToEquipment);

            // [수정 4] 그 다음 Pivot을 더해 절대 좌표로 복원합니다.
            PointD model = _centerOnPivot
                ? new PointD(unmapped.X + _pivotX, unmapped.Y + _pivotY)
                : unmapped;

            return model;

            //// 1) 레시피 옵션 동기화
            //SyncWaferViewFromRecipe();

            //// 레시피 동기화는 동일하게 유지
            //if (!_hasPivot)
            //{
            //    // 뷰→장비 축 역매핑 적용만
            //    return ApplyAxisMap(display, ViewToEquipment);
            //}

            //PointD rel = display;
            ////rel = new PointD(-rel.X, rel.Y);

            //// 1) 절대 좌표 복원 (pivot)
            //PointD modelLikeView = _centerOnPivot
            //    ? new PointD(rel.X + _pivotX, rel.Y + _pivotY)
            //    : rel;

            //// 0) 마지막으로 뷰→장비 고정 축 역매핑 적용
            //PointD model = ApplyAxisMap(modelLikeView, ViewToEquipment);
            //return model;
        }

        public DieInputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested -= OnDisplayView_MotorMoveRequested;
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;

            // WaferId 라벨 더블클릭으로 입력 처리
            if (lblWaferIdValue != null)
            {
                lblWaferIdValue.DoubleClick -= lblWaferIdValue_DoubleClick;
                lblWaferIdValue.DoubleClick += lblWaferIdValue_DoubleClick;
            }

            SyncWaferViewFromRecipe();
        }

        // Equipment에서 유닛 얻기 헬퍼
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
        // 더블클릭 → WaferId 입력 및 스테이지 데이터 신규 생성
        private void lblWaferIdValue_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string current = lblWaferIdValue?.Text?.Trim() ?? "";
                string waferId;

                // FIX: owner를 IWin32Window로 통일하여 ?? 사용
                IWin32Window owner = this.FindForm() as IWin32Window ?? this;
                if (!FormInputWaferID.TryGetWaferId(owner, current, out waferId))
                    return;

                // 라벨은 즉시 반영
                SetWaferId(waferId);

                // 입력 스테이지 접근
                var inputStage = TryGetUnit<InputStage>(Equipment.UnitKeys.InputStage);
                if (inputStage == null)
                    return;

                var wafer = inputStage.GetMaterialWafer();

                // “스테이지에 있고 Data가 없으면 신규 생성”
                bool needCreate =
                    wafer == null ||
                    string.IsNullOrWhiteSpace(wafer.WaferId) ||
                    wafer.Dies == null || wafer.Dies.Count == 0;

                if (needCreate)
                {
                    var newWafer = new MaterialWafer
                    {
                        WaferId = waferId,
                        WaferDate = DateTime.Now.ToString("yyyyMMdd"),
                        CarrierId = wafer?.CarrierId ?? string.Empty,
                        Presence = Material.MaterialPresence.Exist,
                        ProcessSatate = Material.MaterialProcessSatate.Ready,
                    };

                    // 스테이지에 신규 데이터 바인딩 (InputStage가 UI 이벤트를 발생시킴)
                    inputStage.SetMaterial(newWafer);

                    // 즉시 화면도 초기화
                    SetDieList(new List<MaterialDie>());
                }
                else
                {
                    // 기존 웨이퍼가 있으면 ID만 갱신
                    wafer.WaferId = waferId;
                    wafer.Presence = Material.MaterialPresence.Exist;
                    wafer.ProcessSatate = Material.MaterialProcessSatate.Processing;
                    inputStage.SetMaterial(wafer);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"Wafer ID 설정 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // 기존 UpdateDieCount 교체(표시 분리할 경우)
        private void UpdateDieCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateDieCount));
                return;
            }

            int total = _dies.Count;            // 전체 셀(Exist + NotExist)
            int present = _dies.Count(d =>
                   d.State == DieProcessState.Picked
                   || d.State == DieProcessState.Rejected
                   || d.State == DieProcessState.Placed
                   || d.State == DieProcessState.Inspecting
                   || d.State == DieProcessState.Inspected
                   || d.State == DieProcessState.Skip);

            //if (present != 0)
            //{
            //    present = present + 1;
            //}
            lblDieCountValue.Text = present.ToString() + "/" + total.ToString();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. 현재 상태가 AutoRunning이면 동작 차단
                if (Equipment.Instance.EqState == EquipmentState.AutoRunning ||
                    Equipment.Instance.EqState == EquipmentState.Starting)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Warring", "장비가 자동 운전 중입니다. 정지 후 시도하세요.");
                    return;
                }

                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("Question", "Wafer 정보를 초기화하시겠습니까?") != DialogResult.Yes)
                {
                    return;
                }
                var inputStage = TryGetUnit<InputStage>(Equipment.UnitKeys.InputStage);
                if (inputStage == null)
                {
                    Log.Write("DieInputControl", "btnReset_Click", "InputStage unit not found.");
                    return;
                }

                // 신규 런 초기화 + 스테이지 머티리얼 제거
                inputStage.ResetForNewRun(moveToSafeReady: false, clearOffsets: true, clearStageMaterial: true);

                // UI도 같이 초기화(표시/히스토리)
                ResetPickedMarks();
                SetWaferId(string.Empty);
                SetDieList(new List<MaterialDie>());
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"Reset 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void SetWaferId(string waferId)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetWaferId(waferId)));
                return;
            }
            lblWaferIdValue.Text = waferId;
        }
        public void SetDieList(List<MaterialDie> dies)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetDieList(dies)));
                return;
            }

            var incoming = dies ?? new List<MaterialDie>();
            // 픽업 히스토리 적용 (Exist만 Picked 유지)
            foreach (var c in incoming)
            {
                var pInt = new PointD((int)c.MapX, (int)c.MapY);
                if (_pickedCoords.Contains(pInt) &&
                    c.State != DieProcessState.Rejected &&
                    c.Presence == MaterialPresence.Exist)
                {
                    c.State = DieProcessState.Picked;
                }
            }
            _dies = incoming;
            RecalcPivot();


            bool highlightCurrent = _showPickedHistory && GetFoundDieCount() > 0;
            var items = _dies.Select(c =>
            {
                var MapPos = new PointD(c.MapX, c.MapY);
                var displayPos = ToDisplay(MapPos);
                var pInt = new PointD(MapPos.X, MapPos.Y);

                DisplayView_DieInput.ItemState state;
                if (c.Presence == MaterialPresence.NotExist)
                {
                    // 못 찾은 자리 → 빈 슬롯
                    state = DisplayView_DieInput.ItemState.Empty;
                }
                else
                {
                    // 감지된 자리
                    state = ConvertState(c.State);
                    // 현재 픽업 강조
                    if (highlightCurrent &&
                        _lastPickedCoord.HasValue &&
                        Math.Abs(_lastPickedCoord.Value.X - MapPos.X) < 0.001 &&
                        Math.Abs(_lastPickedCoord.Value.Y - MapPos.Y) < 0.001)
                    {
                        state = DisplayView_DieInput.ItemState.PickedUp;
                    }
                    else if (c.State == DieProcessState.Rejected)
                    {
                        state = DisplayView_DieInput.ItemState.Rejected;
                    }
                    else if (c.State == DieProcessState.Skip)
                    {
                        state = DisplayView_DieInput.ItemState.Skip;
                    }
                    else
                    {
                        if (_showPickedHistory && _pickedCoords.Contains(pInt))
                            state = DisplayView_DieInput.ItemState.AfterPickUp;
                        // 히스토리 숨김 옵션이어도 찾은 다이는 계속 표시
                    }
                }

                return new DisplayView_DieInput.DisplayItem
                {
                    Position = displayPos, //modelPos, //displayPos,
                    DieMap = MapPos,
                    State = state,
                    DieId = c.Index,
                    Info = c.Name
                };
            }).ToList();

            displayView1.SetItems(items);
            UpdateDieCount();
            displayView1.Refresh();
        }


        // 픽업된 좌표를 누적 보관 (UI 강제 유지용)
        private readonly HashSet<PointD> _pickedCoords = new HashSet<PointD>();
        // 마지막 실제 Pick 좌표
        private PointD? _lastPickedCoord;
        // 기존 Pick 완료 좌표
        private PointD? _AfterPickCoord;

        // 히스토리 표시 여부
        private bool _showPickedHistory = true;
        public bool ShowPickedHistory
        {
            get => _showPickedHistory;
            set
            {
                if (_showPickedHistory == value) return;
                _showPickedHistory = value;
                SetDieList(_dies);
            }
        }


        // Found (감지된 다이) 개수: Presence==Exist 기준
        private int GetFoundDieCount()
        {
            return _dies.Count(d => d.Presence == MaterialPresence.Exist);
        }

        

        

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView_DieInput.DisplayItemEventArgs e)
        {
            if (e?.Item != null && _hasPivot)
            {
                // 화면 좌표 → 원래 Map 기준 좌표
                var modelPos = FromDisplay((PointD)e.Item.Position);
                var correctedItem = new DisplayView_DieInput.DisplayItem
                {
                    Position = modelPos,      // 외부로는 Map 좌표 전달
                    DieMap = e.Item.DieMap,
                    State = e.Item.State,
                    DieId = e.Item.DieId,
                    Info = e.Item.Info
                };
                var args = new DisplayView_DieInput.DisplayItemEventArgs
                {
                    Item = correctedItem,
                    ScreenPosition = e.ScreenPosition
                };
                MotorMoveRequested?.Invoke(this, args);
                return;
            }
            MotorMoveRequested?.Invoke(this, e);
        }

        


        public void OnWaferExchangeStart()
        {
            ResetPickedMarks();          // 히스토리 좌표 제거
            _showPickedHistory = false;  // 교체 중 히스토리 숨김(원하면 true로 유지 가능)
        }

        private DisplayView_DieInput.ItemState ConvertState(DieProcessState state)
        {
            switch (state)
            {
                case DieProcessState.Picked:
                case DieProcessState.Inspecting:
                case DieProcessState.Inspected:
                case DieProcessState.Placed:
                //case DieProcessState.Rejected:
                    return DisplayView_DieInput.ItemState.AfterPickUp;
                case DieProcessState.Mapped:
                    return DisplayView_DieInput.ItemState.Present;
                case DieProcessState.Rejected:
                    return DisplayView_DieInput.ItemState.Rejected;
                default:
                    return DisplayView_DieInput.ItemState.Empty;
            }
        }

        public void UpdateChip(PointD mapCoord, DieProcessState state)
        {
            var chip = _dies.FirstOrDefault(c => (int)c.MapX == mapCoord.X &&
                                                  (int)c.MapY == mapCoord.Y);
            if (chip != null)
            {
                chip.State = state;

                if (state == DieProcessState.Picked)
                    _pickedCoords.Add(mapCoord);

                UpdateDieCount();
                SetDieList(_dies);
            }
        }

        public void MarkAfterPickUp(PointD coord)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkAfterPickUp(coord)));
                return;
            }
            _AfterPickCoord = coord;
            SetDieList(_dies);
        }

        public void ClearAfterPickUp()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClearAfterPickUp));
                return;
            }
            _AfterPickCoord = null;
            SetDieList(_dies);
        }

        public void MarkCurrentPicked(PointD current)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkCurrentPicked(current)));
                return;
            }

            _pickedCoords.Add(current);
            _lastPickedCoord = current;

            if (_AfterPickCoord.HasValue && _AfterPickCoord.Value == current)
                _AfterPickCoord = null;

            SetDieList(_dies);
        }

        public void MarkCurrentPicked(PointD current, bool clearPrevious)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkCurrentPicked(current, clearPrevious)));
                return;
            }

            MarkDieRemoved(current, showAsPicked: true);
            _lastPickedCoord = current;
        }

        public void MarkDieRemoved(PointD mapCoord, bool showAsPicked = true)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkDieRemoved(mapCoord, showAsPicked)));
                return;
            }

            var chip = _dies.FirstOrDefault(c => (int)c.MapX == mapCoord.X &&
                                                  (int)c.MapY == mapCoord.Y);
            if (chip == null) return;

            if (showAsPicked)
                _pickedCoords.Add(mapCoord);
            else
                _pickedCoords.Remove(mapCoord);

            chip.State = showAsPicked ? DieProcessState.Picked : DieProcessState.None;

            UpdateDieCount();
            SetDieList(_dies);
        }

        public void MarkDiesRemoved(IEnumerable<PointD> removedCoords, bool showAsPicked = true)
        {
            if (removedCoords == null) return;
            var removed = removedCoords as IList<PointD> ?? removedCoords.ToList();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkDiesRemoved(removed, showAsPicked)));
                return;
            }

            foreach (var pt in removed)
            {
                if (showAsPicked) _pickedCoords.Add(pt);
                else _pickedCoords.Remove(pt);
            }

            foreach (var chip in _dies)
            {
                var pInt = new PointD((int)chip.MapX, (int)chip.MapY);
                if (removed.Contains(pInt))
                {
                    chip.State = showAsPicked ? DieProcessState.Picked : DieProcessState.None;
                }
            }

            UpdateDieCount();
            SetDieList(_dies);
        }

        public void ApplyPresenceSnapshot(IEnumerable<PointD> presentCoords)
        {
            var presentList = (presentCoords ?? Enumerable.Empty<PointD>()).ToList();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyPresenceSnapshot(presentList)));
                return;
            }

            var presentSet = new HashSet<PointD>(presentList);

            foreach (var chip in _dies)
            {
                var mapInt = new PointD((int)chip.MapX, (int)chip.MapY);

                if (_pickedCoords.Contains(mapInt))
                {
                    chip.State = DieProcessState.Picked; // 강제 유지
                    continue;
                }

                if (presentSet.Contains(mapInt))
                {
                    if (chip.State != DieProcessState.Picked)
                        chip.State = DieProcessState.Mapped;
                }
                else
                {
                    chip.State = DieProcessState.None;
                }
            }

            UpdateDieCount();
            SetDieList(_dies);
        }

        public void ResetPickedMarks()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ResetPickedMarks));
                return;
            }

            _pickedCoords.Clear();
            _lastPickedCoord = null;
            SetDieList(_dies);
        }

        
    }
}
