using QMC.Common;
using QMC.Common.Controls;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static QMC.Common.Material;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieInputControl : UserControl
    {
        public event EventHandler<DisplayView.DisplayItemEventArgs> MotorMoveRequested;
        private List<MaterialDie> _dies = new List<MaterialDie>();
        // 픽업된 좌표를 누적 보관 (UI 강제 유지용)
        private readonly HashSet<Point> _pickedCoords = new HashSet<Point>();
        // 마지막 실제 Pick 좌표
        private Point? _lastPickedCoord;
        // 기존 Pick 완료 좌표
        private Point? _AfterPickCoord;
        // 화면 표시용 180도 회전 토글 (기본: ON)
        private bool _rotate180ForDisplay = true;
        public bool Rotate180View
        {
            get => _rotate180ForDisplay;
            set
            {
                if (_rotate180ForDisplay == value) return;
                _rotate180ForDisplay = value;
                SetDieList(_dies);
            }
        }
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

        private void RecalcPivot()
        {
            _hasPivot = false;
            if (_dies == null || _dies.Count == 0) return;

            double minX = _dies.Min(c => c.MapX);
            double maxX = _dies.Max(c => c.MapX);
            double minY = _dies.Min(c => c.MapY);
            double maxY = _dies.Max(c => c.MapY);

            _pivotX = (minX + maxX) * 0.5;
            _pivotY = (minY + maxY) * 0.5;
            _hasPivot = true;

            //double minX = _dies.Min(c => c.MapX);
            //double maxX = _dies.Max(c => c.MapX);
            //double minY = _dies.Min(c => c.MapY);
            //double maxY = _dies.Max(c => c.MapY);

            //_pivotX = (minX + maxX) * 0.5;
            //_pivotY = (minY + maxY) * 0.5;
            //_hasPivot = true;
        }

        // 모델(MapX/MapY) -> 디스플레이 좌표
        private PointD ToDisplay(PointD model)
        {
            if (!_hasPivot) return model;

            // 1) 센터 상대좌표 변환
            PointD rel = _centerOnPivot
                ? new PointD(model.X - _pivotX, model.Y - _pivotY)
                : model;

            // 2) 180도 회전 (원점 기준 점대칭)
            //Rotate180View = true;
            //if (Rotate180View)
            //{
            //    rel = new PointD(-rel.X, -rel.Y);
            //}

            // 2) X 방향 반전만 적용 (Y는 그대로)
            rel = new PointD(rel.X, -rel.Y);

            return rel;
        }

        // 디스플레이 -> 모델(MapX/MapY)
        private PointD FromDisplay(PointD display)
        {
            if (!_hasPivot) return display;
            PointD rel = display;

            //Rotate180View = true;
            //// 1) 역회전
            //if (Rotate180View)
            //{
            //    rel = new PointD(-rel.X, -rel.Y);
            //}

            // 1) X 방향 반전의 역변환 (Y는 그대로)
            rel = new PointD(display.X, -display.Y);

            // 2) 절대 좌표 복원
            PointD model = _centerOnPivot
                ? new PointD(rel.X + _pivotX, rel.Y + _pivotY)
                : rel;

            return model;
        }

        //// 모델(MapX/MapY) -> 디스플레이 좌표
        //private PointD ToDisplay(PointD model)
        //{
        //    if (!_hasPivot) return model;

        //    // 1) 센터 상대좌표 변환
        //    PointD rel = _centerOnPivot
        //        ? new PointD(model.X - _pivotX, model.Y - _pivotY)
        //        : model;

        //    // 2) 180도 회전 (원점 기준 점대칭)
        //    Rotate180View = true;
        //    if (Rotate180View)
        //    {
        //        rel = new PointD(-rel.X, -rel.Y);
        //    }
        //    return rel;
        //}

        //// 디스플레이 -> 모델(MapX/MapY)
        //private PointD FromDisplay(PointD display)
        //{
        //    if (!_hasPivot) return display;
        //    PointD rel = display;

        //    Rotate180View = true;
        //    // 1) 역회전
        //    if (Rotate180View)
        //    {
        //        rel = new PointD(-rel.X, -rel.Y);
        //    }

        //    // 2) 절대 좌표 복원
        //    PointD model = _centerOnPivot
        //        ? new PointD(rel.X + _pivotX, rel.Y + _pivotY)
        //        : rel;

        //    return model;
        //}

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

        // 남은 다이(실제 웨이퍼 위) 계산
        // => "찾은(Exist) 다이 수"로 정의 (라벨 Found 계산에 사용)
        private int GetRemainingDieCount()
        {
            //return _dies.Count(d => d.State == DieProcessState.Mapped);
            return _dies.Count(d => d.Presence == MaterialPresence.Exist);
        }

        // Found (감지된 다이) 개수: Presence==Exist 기준
        private int GetFoundDieCount()
        {
            return _dies.Count(d => d.Presence == MaterialPresence.Exist);
        }

        // 기존 UpdateDieCount 교체(표시 분리할 경우)
        private void UpdateDieCount()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateDieCount));
                return;
            }
            int found = GetFoundDieCount();     // 감지된(Exist) 다이 총수
            int total = _dies.Count;            // 전체 셀(Exist + NotExist)
            lblDieCountValue.Text = $"{found} / {total}";

            //if (this.InvokeRequired)
            //{
            //    this.Invoke(new Action(UpdateDieCount));
            //    return;
            //}
            //int remaining = GetRemainingDieCount();
            //int pickedHist = _pickedCoords.Count;
            //// 필요 시 UI Label 2개로 분리하거나 하나에 "남음/총" 형태로
            //lblDieCountValue.Text = $"{remaining} / {pickedHist + remaining}";
            ////lblDieCountValue.Text = remaining.ToString();
            //// 남은 다이가 0이면 히스토리/강조 숨김 모드로 전환(표시 일관성)
            //if (remaining == 0)
            //{
            //    _lastPickedCoord = null;
            //    // 히스토리는 유지하고 싶지 않다면 아래 주석 해제
            //    // _pickedCoords.Clear();
            //    _showPickedHistory = false;
            //}
        }
        //private void UpdateDieCount()
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.Invoke(new Action(() => UpdateDieCount()));
        //        return;
        //    }
        //    int count = _dies.Count(c => c.State == DieProcessState.Mapped || c.State == DieProcessState.Picked);
        //    lblDieCountValue.Text = count.ToString();
        //}

        public DieInputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;

            // WaferId 라벨 더블클릭으로 입력 처리
            if (lblWaferIdValue != null)
                lblWaferIdValue.DoubleClick += lblWaferIdValue_DoubleClick;
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
                    wafer.Summary == null ||
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

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView.DisplayItemEventArgs e)
        {
            if (e?.Item != null && _hasPivot)
            {
                // 화면 좌표 → 원래 Map 기준 좌표
                var modelPos = FromDisplay((PointD)e.Item.Position);
                var correctedItem = new DisplayView.DisplayItem
                {
                    Position = modelPos,      // 외부로는 Map 좌표 전달
                    State = e.Item.State,
                    DieId = e.Item.DieId,
                    Info = e.Item.Info
                };
                var args = new DisplayView.DisplayItemEventArgs
                {
                    Item = correctedItem,
                    ScreenPosition = e.ScreenPosition
                };
                MotorMoveRequested?.Invoke(this, args);
                return;
            }
            MotorMoveRequested?.Invoke(this, e);
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

        //// 모델에서 새 리스트가 들어와도, 이미 픽업된 좌표는 강제로 Picked 유지
        //public void SetDieList(List<MaterialDie> chips)
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.Invoke(new Action(() => SetDieList(chips)));
        //        return;
        //    }

        //    var incoming = chips ?? new List<MaterialDie>();

        //    // 좌표 기준으로 Picked 강제 재적용
        //    foreach (var c in incoming)
        //    {
        //        var p = new PointD(c.MapX, c.MapY);
        //        if (_pickedCoords.Contains(p))
        //        {
        //            if(c.State == DieProcessState.Error)
        //                c.State = DieProcessState.Error;
        //            else
        //                c.State = DieProcessState.Picked;
        //        }
        //    }

        //    _chips = incoming;
        //    UpdateDieCount();

        //    var items = _chips.Select(c =>
        //    {
        //        var p = new PointD(c.MapX, c.MapY);

        //        // 기본 상태
        //        var state = ConvertState(c.State);

        //        // 1) 현재 픽업 좌표는 최우선으로 PickedUp
        //        if (_lastPickedCoord.HasValue && _lastPickedCoord.Value == p)
        //        {
        //            state = DisplayView.ItemState.PickedUp;
        //        }
        //        else if (c.State == DieProcessState.Error)
        //        {
        //            state = DisplayView.ItemState.Empty;
        //        }
        //        // 2) 누적된(과거) 픽업 좌표는 AfterPickUp
        //        else if (_pickedCoords.Contains(p))
        //        {
        //            state = DisplayView.ItemState.AfterPickUp;
        //        }
        //        //// 3) 아직 픽업 전이면서 다음 예정 좌표는 BeforePickUp
        //        //else if (_beforePickCoord.HasValue && _beforePickCoord.Value == p)
        //        //{
        //        //    state = DisplayView.ItemState.BeforePickUp;
        //        //}

        //        return new DisplayView.DisplayItem
        //        {
        //            Position = p,
        //            State = state,
        //            DieId = c.Index, //c.Index + 1,
        //            Info = c.Name
        //        };
        //    }).ToList();

        //    displayView1.SetItems(items);
        //    displayView1.Refresh();
        //}

        // NotExist도 Empty로 표시하도록 전체 다이 사용
        public void SetDieList(List<MaterialDie> chips)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetDieList(chips)));
                return;
            }

            var incoming = chips ?? new List<MaterialDie>();
            // 픽업 히스토리 적용 (Exist만 Picked 유지)
            foreach (var c in incoming)
            {
                var pInt = new Point((int)c.MapX, (int)c.MapY);
                if (_pickedCoords.Contains(pInt) &&
                    c.State != DieProcessState.Error &&
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
                var modelPos = new PointD(c.MapX, c.MapY);
                var displayPos = ToDisplay(modelPos);
                var pInt = new Point((int)Math.Round(modelPos.X), (int)Math.Round(modelPos.Y));

                DisplayView.ItemState state;
                if (c.Presence == MaterialPresence.NotExist)
                {
                    // 못 찾은 자리 → 빈 슬롯
                    state = DisplayView.ItemState.Empty;
                }
                else
                {
                    // 감지된 자리
                    state = ConvertState(c.State);
                    // 현재 픽업 강조
                    if (highlightCurrent &&
                        _lastPickedCoord.HasValue &&
                        Math.Abs(_lastPickedCoord.Value.X - modelPos.X) < 0.001 &&
                        Math.Abs(_lastPickedCoord.Value.Y - modelPos.Y) < 0.001)
                    {
                        state = DisplayView.ItemState.PickedUp;
                    }
                    else if (c.State == DieProcessState.Error)
                    {
                        state = DisplayView.ItemState.Error;
                    }
                    else
                    {
                        if (_showPickedHistory && _pickedCoords.Contains(pInt))
                            state = DisplayView.ItemState.AfterPickUp;
                        // 히스토리 숨김 옵션이어도 찾은 다이는 계속 표시
                    }
                }

                return new DisplayView.DisplayItem
                {
                    Position = displayPos,
                    State = state,
                    DieId = c.Index,
                    Info = c.Name
                };
            }).ToList();

            displayView1.SetItems(items);
            displayView1.Refresh();
            UpdateDieCount();
        }

        //public void SetDieList(List<MaterialDie> chips)
        //{
        //    if (this.InvokeRequired)
        //    {
        //        this.Invoke(new Action(() => SetDieList(chips)));
        //        return;
        //    }

        //    var incoming = chips ?? new List<MaterialDie>();
        //    // 기존 Pick 유지
        //    foreach (var c in incoming)
        //    {
        //        var pInt = new Point((int)Math.Round(c.MapX), (int)Math.Round(c.MapY));
        //        if (_pickedCoords.Contains(pInt))
        //        {
        //            if (c.State != DieProcessState.Error)
        //                c.State = DieProcessState.Picked;
        //        }
        //    }

        //    _dies = incoming;
        //    UpdateDieCount();
        //    RecalcPivot(); // 중심 재계산

        //    var items = _dies.Select(c =>
        //    {
        //        var modelPos = new PointD(c.MapX, c.MapY);
        //        var displayPos = ToDisplay(modelPos);

        //        var state = ConvertState(c.State);

        //        // 현재 픽업된 좌표 강조
        //        if (_lastPickedCoord.HasValue &&
        //            Math.Abs(_lastPickedCoord.Value.X - modelPos.X) < 0.001 &&
        //            Math.Abs(_lastPickedCoord.Value.Y - modelPos.Y) < 0.001)
        //        {
        //            state = DisplayView.ItemState.PickedUp;
        //        }
        //        else if (c.State == DieProcessState.Error)
        //        {
        //            state = DisplayView.ItemState.Empty;
        //        }
        //        else
        //        {
        //            var pInt = new Point((int)Math.Round(modelPos.X), (int)Math.Round(modelPos.Y));
        //            if (_showPickedHistory && _pickedCoords.Contains(pInt))
        //                state = DisplayView.ItemState.AfterPickUp;
        //            else if (!_showPickedHistory && c.State == DieProcessState.Picked)
        //                state = DisplayView.ItemState.Empty; // 히스토리 숨김 시 이미 뺀 자리 비우기
        //        }

        //        return new DisplayView.DisplayItem
        //        {
        //            Position = displayPos,
        //            State = state,
        //            DieId = c.Index,
        //            Info = c.Name
        //        };
        //    }).ToList();

        //    //var items = _dies.Select(c =>
        //    //{
        //    //    var modelPos = new PointD(c.MapX, c.MapY);
        //    //    var displayPos = ToDisplay(modelPos);

        //    //    var state = ConvertState(c.State);

        //    //    // 현재 픽업 좌표 강조
        //    //    if (_lastPickedCoord.HasValue &&
        //    //        Math.Abs(_lastPickedCoord.Value.X - modelPos.X) < 0.001 &&
        //    //        Math.Abs(_lastPickedCoord.Value.Y - modelPos.Y) < 0.001)
        //    //    {
        //    //        state = DisplayView.ItemState.PickedUp;
        //    //    }
        //    //    else if (c.State == DieProcessState.Error)
        //    //    {
        //    //        state = DisplayView.ItemState.Empty;
        //    //    }
        //    //    else
        //    //    {
        //    //        var pInt = new Point((int)Math.Round(modelPos.X), (int)Math.Round(modelPos.Y));
        //    //        if (_pickedCoords.Contains(pInt))
        //    //            state = DisplayView.ItemState.AfterPickUp;
        //    //    }

        //    //    return new DisplayView.DisplayItem
        //    //    {
        //    //        Position = displayPos,
        //    //        State = state,
        //    //        DieId = c.Index,
        //    //        Info = c.Name
        //    //    };
        //    //}).ToList();

        //    // (필요 시) DisplayView가 음수 좌표를 잘라낸다면 여기서 중앙 정렬 offset 적용 가능
        //    // TranslateToViewCenter(items);

        //    displayView1.SetItems(items);
        //    displayView1.Refresh();
        //}

        public void OnWaferExchangeStart()
        {
            ResetPickedMarks();          // 히스토리 좌표 제거
            _showPickedHistory = false;  // 교체 중 히스토리 숨김(원하면 true로 유지 가능)
        }
        private DisplayView.ItemState ConvertState(DieProcessState state)
        {
            switch (state)
            {
                case DieProcessState.Picked:
                case DieProcessState.Inspecting:
                case DieProcessState.Inspected:
                case DieProcessState.Placed:
                //case DieProcessState.Rejected:
                    return DisplayView.ItemState.AfterPickUp;
                case DieProcessState.Mapped:
                    return DisplayView.ItemState.Present;
                case DieProcessState.Rejected:
                    return DisplayView.ItemState.Error;
                default:
                    return DisplayView.ItemState.Empty;
            }
        }

        public void UpdateChip(Point mapCoord, DieProcessState state)
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

        public void MarkAfterPickUp(Point coord)
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

        public void MarkCurrentPicked(Point current)
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

        public void MarkCurrentPicked(Point current, bool clearPrevious)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => MarkCurrentPicked(current, clearPrevious)));
                return;
            }

            MarkDieRemoved(current, showAsPicked: true);
            _lastPickedCoord = current;
        }

        public void MarkDieRemoved(Point mapCoord, bool showAsPicked = true)
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

        public void MarkDiesRemoved(IEnumerable<Point> removedCoords, bool showAsPicked = true)
        {
            if (removedCoords == null) return;
            var removed = removedCoords as IList<Point> ?? removedCoords.ToList();

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
                var pInt = new Point((int)chip.MapX, (int)chip.MapY);
                if (removed.Contains(pInt))
                {
                    chip.State = showAsPicked ? DieProcessState.Picked : DieProcessState.None;
                }
            }

            UpdateDieCount();
            SetDieList(_dies);
        }

        public void ApplyPresenceSnapshot(IEnumerable<Point> presentCoords)
        {
            var presentList = (presentCoords ?? Enumerable.Empty<Point>()).ToList();

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ApplyPresenceSnapshot(presentList)));
                return;
            }

            var presentSet = new HashSet<Point>(presentList);

            foreach (var chip in _dies)
            {
                var mapInt = new Point((int)chip.MapX, (int)chip.MapY);

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

        private void buttonOpen_Click(object sender, EventArgs e)
        {

        }

        // (선택) 화면 중앙 정렬 유틸 (DisplayView 구현에 따라 필요 시 사용)
        // private void TranslateToViewCenter(List<DisplayView.DisplayItem> items)
        // {
        //     if (items == null || items.Count == 0) return;
        //     double minX = items.Min(i => i.Position.X);
        //     double maxX = items.Max(i => i.Position.X);
        //     double minY = items.Min(i => i.Position.Y);
        //     double maxY = items.Max(i => i.Position.Y);
        //     double currentCenterX = (minX + maxX) * 0.5;
        //     double currentCenterY = (minY + maxY) * 0.5;
        //     double targetCenterX = 0;
        //     double targetCenterY = 0;
        //     double dx = targetCenterX - currentCenterX;
        //     double dy = targetCenterY - currentCenterY;
        //     foreach (var it in items)
        //         it.Position = new PointD(it.Position.X + dx, it.Position.Y + dy);
        // }
    }
}
