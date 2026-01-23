using QMC.Common;
using QMC.Common.Controls;
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

namespace QMC.LCP_280.Process.Component
{
    public partial class DieScanMapControl : UserControl
    {
        public event EventHandler<DisplayView_DieScanMap.DisplayItemEventArgs> MotorMoveRequested;

        // [ADD] 모터이동 없이 "클릭 선택" 이벤트
        public event EventHandler<DisplayView_DieScanMap.DisplayItemEventArgs> ItemClicked;

        private List<MaterialDie> _dies = new List<MaterialDie>();

        // pivot: MapX/MapY 범위 중심(0.0 포함 가능)
        private int _pivotX;
        private int _pivotY;
        private bool _hasPivot;

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


        public DieScanMapControl()
        {
            InitializeComponent();

            // 클릭 시 모터이동 이벤트 전달(좌표 역변환 포함)
            if (displayView1 != null)
            {
                // 기존: 모터 이동 이벤트
                displayView1.MotorMoveRequested += DisplayView1_MotorMoveRequested;

                // [ADD] 클릭 선택 이벤트 (DisplayView는 ItemDoubleClicked 이름이지만 실제로 단일 클릭)
                displayView1.ItemDoubleClicked += DisplayView1_ItemDoubleClicked;
            }
        }

        // ===== Pivot 계산 =====
        private void RecalcPivot()
        {
            _hasPivot = false;
            if (_dies == null || _dies.Count == 0) return;

            int minX = _dies.Min(c => c.MapX);
            int maxX = _dies.Max(c => c.MapX);
            int minY = _dies.Min(c => c.MapY);
            int maxY = _dies.Max(c => c.MapY);

            _pivotX = (minX + maxX) * 1;
            _pivotY = (minY + maxY) * 1;
            _hasPivot = true;
        }

        // 모델(MapX/MapY) -> 디스플레이 좌표
        private Point ToDisplay(Point map)
        {
            if (!_centerOnPivot || !_hasPivot)
                return map;

            Point rel = _centerOnPivot
                ? new Point(map.X - _pivotX, map.Y - _pivotY)
                : map;

            //180도 회전해야. 장비랑 비젼이랑 1:1.
            rel = new Point(-rel.X, -rel.Y);
            //Y반전.
            rel = new Point(-rel.X, rel.Y);

            return rel;
        }

        // 디스플레이 -> 모델(MapX/MapY)
        private Point FromDisplay(Point display)
        {
            if (!_centerOnPivot || !_hasPivot)
                return display;

            Point rel = display;

            //Y반전도.
            rel = new Point(-rel.X, rel.Y);
            //180도 회전해야. 장비랑 비젼이랑 1:1
            rel = new Point(-rel.X, -rel.Y);
            
            return rel;
        }

        public void SetDieList(List<MaterialDie> dies)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetDieList(dies)));
                return;
            }

            var incoming = dies ?? new List<MaterialDie>();
            
            _dies = incoming;
            RecalcPivot();
            var items = _dies.Where(d => d != null).Select(c =>
            {
                var modelPos = new PointD(c.MapX, c.MapY);
                var displayPos = ToDisplay(modelPos);

                var pInt = new Point((int)Math.Round(modelPos.X), (int)Math.Round(modelPos.Y));

                DisplayView_DieScanMap.ItemState state = DisplayView_DieScanMap.ItemState.Empty;
                if (c.Presence == MaterialPresence.NotExist)
                {
                    state = DisplayView_DieScanMap.ItemState.Empty;
                }

                return new DisplayView_DieScanMap.DisplayItem
                {
                    Position = displayPos, // 화면에는 상대좌표
                    DieMap = modelPos,
                    State = state,
                    DieId = c.Index,       // DieId는 Index 유지(외부 매칭 안정)
                    Info = c.Name ?? string.Empty
                };
            }).ToList();

            displayView1.SetItems(items);
            displayView1.Refresh();
            UpdateDieCount();
        }

        // ===== DisplayView 이벤트: 클릭 좌표를 모델 좌표로 복원해서 외부로 전달 =====
        private void DisplayView1_MotorMoveRequested(object sender, DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null)
            {
                MotorMoveRequested?.Invoke(this, e);
                return;
            }

            // DisplayView에는 "상대좌표"가 들어있으므로 Map 좌표로 환원
            var modelPos = FromDisplay(e.Item.Position);

            var corrected = new DisplayView_DieScanMap.DisplayItem
            {
                Position = modelPos,
                State = e.Item.State,
                DieId = e.Item.DieId,
                Info = e.Item.Info
            };

            MotorMoveRequested?.Invoke(this, new DisplayView_DieScanMap.DisplayItemEventArgs
            {
                Item = corrected,
                ScreenPosition = e.ScreenPosition
            });
        }

        // [ADD] 내부 클릭 이벤트를 외부로 전달
        private void DisplayView1_ItemDoubleClicked(object sender, DisplayView_DieScanMap.DisplayItemEventArgs e)
        {
            if (e?.Item == null)
            {
                ItemClicked?.Invoke(this, e);
                return;
            }

            // MotorMoveRequested와 동일하게 "모델 좌표"로 보정해서 넘겨야
            // FormMapMatchManual에서 Map 좌표로 정확히 Pick 됨
            var modelPos = FromDisplay(e.Item.Position);

            var corrected = new DisplayView_DieScanMap.DisplayItem
            {
                Position = modelPos,
                DieMap = e.Item.DieMap,          // ★ 중요: DieMap도 채워줘야 Pick이 안정적
                State = e.Item.State,
                DieId = e.Item.DieId,
                Info = e.Item.Info,
                GroupId = e.Item.GroupId
            };

            ItemClicked?.Invoke(this, new DisplayView_DieScanMap.DisplayItemEventArgs
            {
                Item = corrected,
                ScreenPosition = e.ScreenPosition
            });
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

        // Found (감지된 다이) 개수: Presence==Exist 기준
        private int GetFoundDieCount()
        {
            if (_dies == null) return 0;
            return _dies.Count(d => d != null && d.Presence == MaterialPresence.Exist);
        }

        private void UpdateDieCount()
        {
            // 디자이너에 lblDieCountValue가 없을 수도 있으니 방어
            try
            {
                if (InvokeRequired) { BeginInvoke(new Action(UpdateDieCount)); return; }

                int found = GetFoundDieCount();
                int total = _dies?.Count ?? 0;

                // 라벨이 있다면 표시
                var prop = GetType().GetField("lblDieCountValue",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);

                var lbl = prop?.GetValue(this) as Label;
                if (lbl != null)
                    lbl.Text = $"{found} / {total}";
            }
            catch { }
        }

        public void SetDieListOverlay(List<MaterialDie> downloadDies, List<MaterialDie> scanDies)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => SetDieListOverlay(downloadDies, scanDies)));
                return;
            }

            var d1 = downloadDies ?? new List<MaterialDie>();
            var d2 = scanDies ?? new List<MaterialDie>();

            // Pivot은 둘 다 합친 것으로 계산해야 “같은 기준”으로 겹쳐짐
            _dies = d1.Concat(d2).ToList();
            RecalcPivot();

            var items = new List<DisplayView_DieScanMap.DisplayItem>(_dies.Count);

            // Download = GroupId 1 (Blue)
            for (int i = 0; i < d1.Count; i++)
            {
                var c = d1[i];
                if (c == null) continue;

                items.Add(new DisplayView_DieScanMap.DisplayItem
                {
                    Position = ToDisplay(new PointD(c.MapX, c.MapY)),
                    DieMap = new PointD(c.MapX, c.MapY),
                    State = DisplayView_DieScanMap.ItemState.Present,
                    DieId = c.Index,
                    Info = c.Name ?? "",
                    GroupId = 1
                });
            }

            // Scan = GroupId 2 (Red)
            for (int i = 0; i < d2.Count; i++)
            {
                var c = d2[i];
                if (c == null) continue;

                items.Add(new DisplayView_DieScanMap.DisplayItem
                {
                    Position = ToDisplay(new PointD(c.MapX, c.MapY)),
                    DieMap = new PointD(c.MapX, c.MapY),
                    State = DisplayView_DieScanMap.ItemState.Present,
                    DieId = c.Index,
                    Info = c.Name ?? "",
                    GroupId = 2
                });
            }

            displayView1.SetItems(items);
            displayView1.Refresh();
            UpdateDieCount();
        }
    }
}
