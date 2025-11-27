using QMC.Common;
using QMC.Common.Controls;                 // DisplayView_DieOutputControl
using QMC.LCP_280.Process.Component;       // MaterialDie, DieProcessState
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static QMC.Common.Unit.BaseUnit;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieOutputControl : UserControl
    {
        // 기존 내부 Die / DieState 제거
        //  - DieInputControl 과 동일하게 공정 데이터 모델 MaterialDie 사용
        //  - Display 표시는 DisplayView_DieOutputControl 전용 ItemState 로 변환

        public event EventHandler<DisplayView_DieOutputControl.DisplayItemEventArgs> MotorMoveRequested;

        private List<MaterialDie> _dies = new List<MaterialDie>();

        // 화면 표시용 180도 회전 토글 (기본: ON)
        private bool _rotate180ForDisplay = false;
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

        // 화면 표시를 센터 상대좌표(0,0 중심)로 보낼지 여부
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

        // 회전(반전) 기준(맵 범위의 중심) - Bin 인덱스 기준
        private double _pivotX;
        private double _pivotY;
        private bool _hasPivot;

        private void RecalcPivot()
        {
            _hasPivot = false;
            if (_dies == null || _dies.Count == 0) return;

            double minX = _dies.Min(d => d.BinX);
            double maxX = _dies.Max(d => d.BinX);
            double minY = _dies.Min(d => d.BinY);
            double maxY = _dies.Max(d => d.BinY);

            _pivotX = (minX + maxX) * 0.5;
            _pivotY = (minY + maxY) * 0.5;
            _hasPivot = true;
        }

        // 모델(Bin 인덱스) -> 디스플레이 좌표
        private PointD ToDisplay(PointD model)
        {
            if (!_hasPivot) return model;

            // 1) 센터 기준 상대좌표
            PointD rel = _centerOnPivot
                ? new PointD(model.X - _pivotX, model.Y - _pivotY)
                : model;

            // 2) 180도 회전 (원점 기준 점대칭)
            Rotate180View = true;
            if (_rotate180ForDisplay)
            {
                rel = new PointD(-rel.X, -rel.Y);
            }

            return rel;
        }

        // 디스플레이 -> 모델 (Bin 인덱스)
        private PointD FromDisplay(PointD display)
        {
            if (!_hasPivot) return display;

            PointD rel = display;

            // 1) 역회전
            Rotate180View = true;
            if (_rotate180ForDisplay)
            {
                rel = new PointD(-rel.X, -rel.Y);
            }

            // 2) 절대 Bin 인덱스로 복원
            PointD model = _centerOnPivot
                ? new PointD(rel.X + _pivotX, rel.Y + _pivotY)
                : rel;

            return model;
        }


        private ToolTip _dieTooltip = new ToolTip();
        private ContextMenuStrip _dieContextMenu;
        private MaterialDie _lastRightClickedDie;

        private void InitDieOutputExtensions()
        {
            // ToolTip 기본 옵션
            _dieTooltip.AutoPopDelay = 8000;
            _dieTooltip.InitialDelay = 300;
            _dieTooltip.ReshowDelay = 100;
            _dieTooltip.ShowAlways = true;

            // 컨텍스트 메뉴 (상세보기)
            _dieContextMenu = new ContextMenuStrip();
            var miDetail = new ToolStripMenuItem("Die 상세 정보...");
            miDetail.Click += (s, e) =>
            {
                if (_lastRightClickedDie != null)
                    ShowDieInfoForm(_lastRightClickedDie);
            };
            _dieContextMenu.Items.Add(miDetail);

            //// 마우스 이벤트 등록
            //displayView1.MouseDown += DisplayView1_MouseDown;
            //displayView1.MouseDoubleClick += DisplayView1_MouseDoubleClick;
            // 중복 바인딩 방지 후 재바인딩
            displayView1.MouseDown -= DisplayView1_MouseDown;
            displayView1.MouseDoubleClick -= DisplayView1_MouseDoubleClick;
            displayView1.MouseDown += DisplayView1_MouseDown;
            displayView1.MouseDoubleClick += DisplayView1_MouseDoubleClick;
        }


        public DieOutputControl()
        {
            InitializeComponent();
            displayView1.MotorMoveRequested += OnDisplayView_MotorMoveRequested;

            // [ADD] WaferId 라벨 더블클릭으로 입력 처리
            if (lblWaferIdValue != null)
                lblWaferIdValue.DoubleClick += lblWaferIdValue_DoubleClick;

            // [중요] 우클릭/더블클릭 핸들러 바인딩
            InitDieOutputExtensions();
        }

        // [ADD] Equipment에서 유닛 얻기 헬퍼
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

        

        // 더블클릭 → WaferId 입력 및 출력 스테이지 데이터 신규 생성 / ID 변경
        private void lblWaferIdValue_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                string current = lblWaferIdValue?.Text?.Trim() ?? "";
                string waferId;
                IWin32Window owner = this.FindForm() as IWin32Window ?? this;
                if (!FormInputWaferID.TryGetWaferId(owner, current, out waferId))
                    return;

                SetWaferId(waferId);

                var outputStage = TryGetUnit<OutputStage>(Equipment.UnitKeys.OutputStage);
                if (outputStage == null)
                    return;

                var wafer = outputStage.GetMaterialWafer();

                bool needCreate = wafer == null || wafer.Dies == null || wafer.Dies.Count == 0;
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

                    // OutputFeeder의 MakePath() 재사용
                    var feeder = TryGetUnit<OutputFeeder>(Equipment.UnitKeys.OutputFeeder);
                    if (feeder != null)
                    {
                        bool feederBusy = feeder.RunUnitStatus == UnitStatus.AutoRunning;
                        if (!feederBusy)
                        {
                            var backup = feeder.GetMaterial() as MaterialWafer;
                            feeder.SetMaterial(newWafer);          // Feeder에 임시 장착
                            feeder.MakePath();                     // 내부 레시피 / 패턴 / 경로 로직 그대로 적용
                            // Dies 생성 후 복원
                            feeder.SetMaterial(backup);
                        }
                        else
                        {
                            // 실행 중이면 간섭 피하고 동일 로직을 로컬로 재현(Fallback)
                            newWafer.Dies = BuildDiesWithFeederLogicFallback(feeder);
                        }
                    }
                    else
                    {
                        // Feeder 없음 → 최소 기본 맵(Fallback)
                        newWafer.Dies = BuildDefaultRectGridFallback();
                    }

                    outputStage.SetMaterial(newWafer);
                    outputStage.UpdateUI();
                    //SetDieList(newWafer.Dies);
                }
                else
                {
                    wafer.WaferId = waferId;
                    SetDieList(wafer.Dies);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show($"Wafer ID 설정 중 오류: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Fallback 1: OutputFeeder 설정(StartCorner/PrimaryAxis/Traversal) 반영 (Feeder 실행 중이어서 직접 호출 못할 때)
        private List<MaterialDie> BuildDiesWithFeederLogicFallback(OutputFeeder feeder)
        {
            var dies = new List<MaterialDie>();
            try
            {
                var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                int cntX = (recipe?.BinCountX > 0) ? recipe.BinCountX : 1;
                int cntY = (recipe?.BinCountY > 0) ? recipe.BinCountY : 1;

                // OutputFeeder 내부 enum 접근
                var startCorner = feeder.StartCorner;
                var primary = feeder.PrimaryAxis;
                var traversal = feeder.Traversal;

                double centerX = (cntX - 1) / 2.0;
                double centerY = (cntY - 1) / 2.0;

                int xStart, yStart, xDir, yDir;
                switch (startCorner)
                {
                    default:
                    case OutputFeeder.PathStartCorner.BottomLeft:
                        xStart = 0; yStart = 0; xDir = +1; yDir = +1; break;
                    case OutputFeeder.PathStartCorner.BottomRight:
                        xStart = cntX - 1; yStart = 0; xDir = -1; yDir = +1; break;
                    case OutputFeeder.PathStartCorner.TopLeft:
                        xStart = 0; yStart = cntY - 1; xDir = +1; yDir = -1; break;
                    case OutputFeeder.PathStartCorner.TopRight:
                        xStart = cntX - 1; yStart = cntY - 1; xDir = -1; yDir = -1; break;
                }

                IEnumerable<int> RangeDir(int start, int count, int dir)
                {
                    if (dir > 0)
                        for (int i = 0; i < count; i++) yield return start + i;
                    else
                        for (int i = 0; i < count; i++) yield return start - i;
                }

                var xLineForward = RangeDir(xStart, cntX, xDir).ToList();
                var xLineReverse = xLineForward.AsEnumerable().Reverse().ToList();
                var yLineForward = RangeDir(yStart, cntY, yDir).ToList();
                var yLineReverse = yLineForward.AsEnumerable().Reverse().ToList();

                int index = 0;
                if (primary == OutputFeeder.PathPrimaryAxis.XFirst)
                {
                    for (int row = 0; row < cntY; row++)
                    {
                        var xSeq = (traversal == OutputFeeder.PathTraversalMode.Serpentine && (row % 2 == 1))
                            ? xLineReverse
                            : xLineForward;
                        int yBin = yLineForward[row]; // y 진행 방향 자체는 위 시퀀스 정의에 포함

                        foreach (int xBin in xSeq)
                        {
                            dies.Add(new MaterialDie
                            {
                                Index = index++,
                                Presence = Material.MaterialPresence.NotExist,
                                ProcessSatate = Material.MaterialProcessSatate.Unknown,
                                State = DieProcessState.None,
                                BinX = xBin,
                                BinY = yBin,
                                MapX = (int)(xBin - centerX),
                                MapY = (int)(yBin - centerY)
                            });
                        }
                    }
                }
                else // YFirst
                {
                    for (int col = 0; col < cntX; col++)
                    {
                        var ySeq = (traversal == OutputFeeder.PathTraversalMode.Serpentine && (col % 2 == 1))
                            ? yLineReverse
                            : yLineForward;
                        int xBin = xLineForward[col];

                        foreach (int yBin in ySeq)
                        {
                            dies.Add(new MaterialDie
                            {
                                Index = index++,
                                Presence = Material.MaterialPresence.NotExist,
                                ProcessSatate = Material.MaterialProcessSatate.Unknown,
                                State = DieProcessState.None,
                                BinX = xBin,
                                BinY = yBin,
                                MapX = (int)(xBin - centerX),
                                MapY = (int)(yBin - centerY)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("DieOutputControl", "Fallback MakePath", ex.Message);
            }
            return dies;
        }

        // Fallback 2: 아주 단순한 직사격자 (Feeder 없음)
        private List<MaterialDie> BuildDefaultRectGridFallback()
        {
            var dies = new List<MaterialDie>();
            int cntX = 10, cntY = 10;
            try
            {
                var recipe = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                if (recipe != null)
                {
                    if (recipe.BinCountX > 0) cntX = recipe.BinCountX;
                    if (recipe.BinCountY > 0) cntY = recipe.BinCountY;
                }
            }
            catch { }

            int cx =(int) ((cntX - 1) / 2.0);
            int cy = (int)((cntY - 1) / 2.0);
            int index = 0;
            for (int y = 0; y < cntY; y++)
            {
                for (int x = 0; x < cntX; x++)
                {
                    dies.Add(new MaterialDie
                    {
                        Index = index++,
                        Presence = Material.MaterialPresence.NotExist,
                        ProcessSatate = Material.MaterialProcessSatate.Unknown,
                        State = DieProcessState.None,
                        BinX = x,
                        BinY = y,
                        MapX = x - cx,
                        MapY = y - cy
                    });
                }
            }
            return dies;
        }

        private void OnDisplayView_MotorMoveRequested(object sender, DisplayView_DieOutputControl.DisplayItemEventArgs e)
        {
            if (e?.Item != null && _hasPivot)
            {
                // 화면 좌표 -> 모델 Bin 좌표
                var modelPos = FromDisplay(e.Item.Position);
                var correctedItem = new DisplayView_DieOutputControl.DisplayItem
                {
                    Position = modelPos, // 외부로는 Bin 좌표 사용
                    State = e.Item.State,
                    DieId = e.Item.DieId,
                    Info = e.Item.Info
                };
                var args = new DisplayView_DieOutputControl.DisplayItemEventArgs
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

        private void UpdateDieCount()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateDieCount));
                return;
            }

            // 출력 쪽 카운트 정의:
            //  - Placed : 이미 출력 영역(또는 배출 완료 위치)에 놓인 상태
            //  - Picked : (필요 시) 픽 완료 후 상태 유지되는 경우 포함
            // 프로젝트 정책에 따라 조정 가능
            int total = _dies.Count;
            int present = _dies.Count(d =>
                   d.State == DieProcessState.Placed
                || d.State == DieProcessState.Picked
                || d.State == DieProcessState.Inspected    // 필요하면 표시
                || d.State == DieProcessState.Rejected);   // 필요 시 제외 가능

            lblDieCountValue.Text = present.ToString() + "/" + total.ToString();
        }

        public void SetDieList(List<MaterialDie> dies)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetDieList(dies)));
                return;
            }

            _dieTooltip.Hide(displayView1);

            _dies = dies ?? new List<MaterialDie>();
            UpdateDieCount();
            RecalcPivot();

            // 자동 회전 추정 제거: Rotate180View(또는 필드) 값 그대로 사용
            var items = _dies.Select(d =>
            {
                var modelPos = new PointD(d.BinX, d.BinY);
                var displayPos = ToDisplay(modelPos);
                return new DisplayView_DieOutputControl.DisplayItem
                {
                    Position = displayPos,
                    State = ConvertState(d.State),
                    DieId = d.Index,
                    Info = d.TesterResult?.BinningResult?.BinLabel
                };
            }).ToList();

            displayView1.SetItems(items);
            displayView1.Refresh();
        }

        /// <summary>
        /// 개별 다이 상태 업데이트 (Bin 좌표로 탐색)
        /// </summary>
        public void UpdateDie(Point binCoord, DieProcessState newState)
        {
            var die = _dies.FirstOrDefault(d =>
                (int)Math.Round(d.BinX) == binCoord.X &&
                (int)Math.Round(d.BinY) == binCoord.Y);
            if (die == null) return;

            die.State = newState;

            UpdateDieCount();
            SetDieList(_dies);
        }

        private DisplayView_DieOutputControl.ItemState ConvertState(DieProcessState state)
        {
            // DisplayView_DieOutputControl.ItemState : Empty / Present / Picked
            switch (state)
            {
                case DieProcessState.Picked:
                case DieProcessState.Placed:
                    return DisplayView_DieOutputControl.ItemState.Placed;

                case DieProcessState.Inspected:
                case DieProcessState.Inspecting:
                case DieProcessState.Mapped:
                    return DisplayView_DieOutputControl.ItemState.Present;

                case DieProcessState.Rejected:
                    return DisplayView_DieOutputControl.ItemState.Error;

                default:
                    return DisplayView_DieOutputControl.ItemState.Empty;
            }
        }

        #region (선택) 호환용 Legacy Wrapper
        // 과거 코드에서 DieOutputControl.Die 사용하던 부분이 있다면
        // 아래 Legacy 구조체/메서드로 일시적 호환 가능. 필요 없으면 제거.
        [Obsolete("MaterialDie 로 교체됨. SetDieList(List<MaterialDie>) 사용.")]
        public class LegacyDie
        {
            public int Index { get; set; }
            public Point Position { get; set; }
            public bool Present { get; set; }
        }
        #endregion


        //private void DisplayView1_MouseDown(object sender, MouseEventArgs e)
        //{
        //    if (e.Button != MouseButtons.Right) return;

        //    // 화면 좌표 → DisplayView 내부 좌표
        //    Point clientPt = e.Location;

        //    // DisplayView_DieOutputControl 가 노출하는 Items 컬렉션이 있다고 가정
        //    // 없으면 DisplayView_DieOutputControl에 public IReadOnlyList<DisplayItem> Items 추가 필요
        //    var items = displayView1.Items; // (필요 시 해당 컨트롤 수정)

        //    if (items == null || items.Count == 0) return;

        //    // 가장 가까운 아이템 찾기 (간단한 거리 기반)
        //    DisplayView_DieOutputControl.DisplayItem hit = null;
        //    double bestDist = double.MaxValue;
        //    foreach (var it in items)
        //    {
        //        // DisplayView 내부의 좌표 → 화면 픽셀 변환이 별도로 없다면
        //        // Position을 그대로 비교 (컨트롤이 중앙 (0,0) 기준이라면 스케일 고려 필요)
        //        // 여기서는 Position이 이미 그려지는 논리 좌표라 가정
        //        var p = it.Position;
        //        // 단순히 X,Y가 픽셀이라고 가정 (실제 구현에서 Scale/Offset 필요 시 수정)
        //        double dx = p.X - clientPt.X;
        //        double dy = p.Y - clientPt.Y;
        //        double dist = dx * dx + dy * dy;
        //        if (dist < bestDist)
        //        {
        //            bestDist = dist;
        //            hit = it;
        //        }
        //    }

        //    if (hit == null) return;

        //    // Display 좌표 → 모델 Bin 좌표로 역변환
        //    var modelPos = FromDisplay(hit.Position);

        //    // 해당 Bin 좌표로 MaterialDie 찾기
        //    var die = _dies.FirstOrDefault(d =>
        //        Math.Round(d.BinX) == Math.Round(modelPos.X) &&
        //        Math.Round(d.BinY) == Math.Round(modelPos.Y));

        //    if (die == null) return;

        //    _lastRightClickedDie = die;

        //    // Shift + RightClick 시 상세 Form, 일반 RightClick 시 ToolTip
        //    if ((ModifierKeys & Keys.Shift) == Keys.Shift)
        //    {
        //        ShowDieInfoForm(die);
        //    }
        //    else
        //    {
        //        ShowDieTooltip(die, clientPt);
        //        // 컨텍스트 메뉴도 함께 (선택적으로)
        //        _dieContextMenu.Show(displayView1, clientPt);
        //    }
        //}
        // 기존 DisplayView1_MouseDown → HitTest 사용으로 단순화
        private void DisplayView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) 
                return;

            // HitTest 사용 (DisplayView_DieOutputControl에 public HitTest 추가됨)
            var hit = displayView1.HitTest(e.Location);
            if (hit == null) return;

            // 화면 좌표(디스플레이 기준) → 모델 Bin 좌표 변환
            var modelPos = FromDisplay(hit.Position);

            var die = _dies.FirstOrDefault(d =>
                Math.Round(d.BinX) == Math.Round(modelPos.X) &&
                Math.Round(d.BinY) == Math.Round(modelPos.Y));

            if (die == null) return;
            _lastRightClickedDie = die;

            if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                ShowDieInfoForm(die); // Shift + 오른쪽 클릭 = 상세 창
            }
            else
            {
                // 간단 Tooltip + ContextMenu
                ShowDieTooltip(die, e.Location);
                _dieContextMenu.Show(displayView1, e.Location);
            }
        }

        // 더블클릭(좌측) → 모터 이동: DisplayView_DieOutputControl ItemDoubleClicked 이벤트 활용 권장
        private void DisplayView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) 
                return;
            var hit = displayView1.HitTest(e.Location);
            if (hit == null) return;

            // 모터 이동 요청 이벤트 발생 (기존 구조 유지)
            MotorMoveRequested?.Invoke(this, new DisplayView_DieOutputControl.DisplayItemEventArgs
            {
                Item = hit,
                ScreenPosition = e.Location
            });
        }


        private void ShowDieTooltip(MaterialDie die, Point clientPoint)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Index: {die.Index}");
                sb.AppendLine($"WaferId: {die?.TesterResult?.BinningResult?.BinLabel ?? GetOutputWaferId()}");
                sb.AppendLine($"Bin (X,Y): {die.BinX},{die.BinY}");
                sb.AppendLine($"Map (X,Y): {die.MapX},{die.MapY}");
                sb.AppendLine($"Center (X,Y): {die.CenterX},{die.CenterY}");
                sb.AppendLine($"Angle: {die.Angle:0.###}");
                sb.AppendLine($"State: {die.State}");
                sb.AppendLine($"Rank: {die.Rank} / {die.RankName}");
                sb.AppendLine($"Pass: {die.IsPass}");
                if (!die.IsPass && !string.IsNullOrWhiteSpace(die.RejectReason))
                    sb.AppendLine($"RejectReason: {die.RejectReason}");

                var binRes = die.TesterResult?.BinningResult;
                if (binRes != null)
                {
                    sb.AppendLine($"BinNo: {binRes.BinNo}");
                    sb.AppendLine($"BinLabel: {binRes.BinLabel}");
                    sb.AppendLine($"BinType: {binRes.BinType}");
                }

                // 측정값 요약 (최대 6개)
                if (die.TesterResult?.Items != null && die.TesterResult.Items.Count > 0)
                {
                    sb.AppendLine("Items:");
                    foreach (var kv in die.TesterResult.Items.Take(6))
                        sb.AppendLine($"  {kv.Key}: {kv.Value.Value:0.###}");
                }

                _dieTooltip.Show(sb.ToString(), displayView1, clientPoint, 8000);
            }
            catch (Exception ex)
            {
                Log.Write("DieOutputControl", $"ShowDieTooltip Exception: {ex.Message}");
            }
        }

        private string GetOutputWaferId()
        {
            try
            {
                var outputStage = TryGetUnit<OutputStage>(Equipment.UnitKeys.OutputStage);
                return outputStage?.GetMaterialWafer()?.WaferId ?? "";
            }
            catch { return ""; }
        }

        private void ShowDieInfoForm(MaterialDie die)
        {
            var f = new DieInfoForm(die, GetOutputWaferId());
            f.StartPosition = FormStartPosition.CenterParent;
            f.Show(this.FindForm());
        }

        // 상세 Form
        private sealed class DieInfoForm : Form
        {
            public DieInfoForm(MaterialDie die, string waferId)
            {
                Text = $"Die 상세 정보 - Index {die.Index}";
                Width = 520;
                Height = 640;

                var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
                Controls.Add(panel);

                var txt = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Fill,
                    Font = new Font("Consolas", 9F)
                };
                panel.Controls.Add(txt);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"WaferId: {waferId}");
                sb.AppendLine($"Index            : {die.Index}");
                sb.AppendLine($"Bin (X,Y)        : {die.BinX},{die.BinY}");
                sb.AppendLine($"Map (X,Y)        : {die.MapX},{die.MapY}");
                sb.AppendLine($"Center (X,Y)     : {die.CenterX},{die.CenterY}");
                sb.AppendLine($"Angle            : {die.Angle:0.###}");
                sb.AppendLine($"State            : {die.State}");
                sb.AppendLine($"Rank             : {die.Rank} / {die.RankName}");
                sb.AppendLine($"Pass             : {die.IsPass}");
                if (!die.IsPass && !string.IsNullOrWhiteSpace(die.RejectReason))
                    sb.AppendLine($"RejectReason     : {die.RejectReason}");

                var binRes = die.TesterResult?.BinningResult;
                if (binRes != null)
                {
                    sb.AppendLine($"Binning BinNo    : {binRes.BinNo}");
                    sb.AppendLine($"Binning Label    : {binRes.BinLabel}");
                    sb.AppendLine($"Binning Type     : {binRes.BinType}");
                }

                // 측정 항목 테이블 형태
                sb.AppendLine();
                sb.AppendLine("=== Test Items ===");
                if (die.TesterResult?.Items != null && die.TesterResult.Items.Count > 0)
                {
                    int nameWidth = die.TesterResult.Items.Keys.Max(k => k.Length);
                    foreach (var kv in die.TesterResult.Items)
                    {
                        sb.AppendLine($"{kv.Key.PadRight(nameWidth)} : {kv.Value.Value:0.###}");
                    }
                }
                else
                {
                    sb.AppendLine("(No items)");
                }

                // MeasureValues 확장
                if (die.MeasureValues != null && die.MeasureValues.Count > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("=== MeasureValues (Raw/Range/Meta) ===");
                    int nameWidth = die.MeasureValues.Keys.Max(k => k.Length);
                    foreach (var kv in die.MeasureValues.OrderBy(k => k.Key))
                    {
                        sb.AppendLine($"{kv.Key.PadRight(nameWidth)} : {kv.Value}");
                    }
                }

                txt.Text = sb.ToString();
            }
        }

    }
}
