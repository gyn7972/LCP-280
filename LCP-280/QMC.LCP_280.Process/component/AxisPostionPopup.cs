using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Motions;

namespace QMC.LCP_280.Process.Component
{
    /// <summary>
    /// 26축 Actual Position 모니터 테이블 (Axis Name / Pos. (mm))
    /// Motion_Setup 에서 이미 Status.PV 가 갱신된다는 가정 하에 그대로 표시
    /// </summary>
    public partial class AxisPostionPopup : Form
    {
        private Equipment _equipment;
        private MotionAxisManager _axisManager;
        private readonly List<AxisRow> _rows = new List<AxisRow>();
        private Timer _timer;

        private class AxisRow
        {
            public MotionAxis Axis;
            public ListViewItem Item;
        }

        public AxisPostionPopup()
        {
            InitializeComponent();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;

            this.ShowInTaskbar = true;
            this.TopLevel = true;
            this.Owner = null;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Axis Position Monitor";
            try { this.Icon = Properties.Resources.JogPanel_ico; } catch { }

            // 행 높이(예: 28px) 늘리기 위한 더미 SmallImageList
            var dummy = new ImageList();
            dummy.ImageSize = new Size(1, 24); // 원하는 높이로 조정 (기본보다 크게)
            listViewAxis.SmallImageList = dummy;

            EnableListViewDoubleBuffer(listViewAxis);

            FormClosed -= AxisPostionPopup_FormClosed;
            FormClosed += AxisPostionPopup_FormClosed;

            Init();
        }

        private void AxisPostionPopup_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                _timer?.Stop();
                if (_timer != null)
                {
                    _timer.Tick -= Timer_Tick;
                    _timer.Dispose();
                    _timer = null;
                }
            }
            catch { }
        }

        private void Init()
        {
            try
            {
                _equipment = Equipment.Instance;
                _axisManager = _equipment?.AxisManager;
                BuildAxisList();
                StartTimer();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show("초기화 실패: " + ex.Message);
            }
        }

        private void BuildAxisList()
        {
            listViewAxis.BeginUpdate();
            listViewAxis.Items.Clear();
            _rows.Clear();

            var axes = _axisManager?.GetAll();
            if (axes != null && axes.Length > 0)
            {
                // Axis 번호(Setup.AxisNo) 또는 이름 정렬
                foreach (var ax in axes
                         .Where(a => a != null)
                         .OrderBy(a => a.AxisNo))
                {
                    var item = new ListViewItem(ax.Name ?? "(Unnamed)");
                    item.SubItems.Add("----");
                    listViewAxis.Items.Add(item);

                    _rows.Add(new AxisRow
                    {
                        Axis = ax,
                        Item = item
                    });
                }
            }

            // 26축 표준 요구인데 실제 축 수 < 26 인 경우 자리 채우기 (옵션)
            int target = 26;
            if (_rows.Count < target)
            {
                for (int i = _rows.Count; i < target; i++)
                {
                    var item = new ListViewItem("(Empty)");
                    item.SubItems.Add("-");
                    listViewAxis.Items.Add(item);
                }
            }

            listViewAxis.EndUpdate();
        }

        private void StartTimer()
        {
            _timer?.Stop();
            if (_timer != null) _timer.Tick -= Timer_Tick;

            _timer = new Timer { Interval = 200 };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try { RefreshPositions(); }
            catch (Exception ex) { Log.Write(ex); }
        }

        private void RefreshPositions()
        {
            if (_rows.Count == 0) return;

            foreach (var r in _rows)
            {
                if (r.Axis == null) continue;

                double pos = 0;
                bool ok = false;

                try
                {
                    // Status.PV (이미 다른 곳에서 갱신) 우선
                    var st = r.Axis.Status;
                    if (st != null && st.PV != null)
                    {
                        pos = st.PV.ActualPosition;
                        ok = true;
                    }
                }
                catch { }

                if (!ok)
                {
                    try
                    {
                        pos = r.Axis.GetPosition();
                        ok = true;
                    }
                    catch { }
                }

                string txt = ok ? pos.ToString("0.000", CultureInfo.InvariantCulture) : "ERR";
                // SubItem[1] = position
                if (r.Item.SubItems.Count > 1 && r.Item.SubItems[1].Text != txt)
                    r.Item.SubItems[1].Text = txt;
            }
        }

        #region Owner Draw (헤더/셀 스타일)
        private readonly Font _headerFont = new Font("맑은 고딕", 11f, FontStyle.Bold);
        private readonly Font _rowFont = new Font("맑은 고딕", 11f, FontStyle.Bold);
        private readonly Brush _headerBack = new SolidBrush(Color.White);
        private readonly Pen _borderPen = new Pen(Color.FromArgb(0, 90, 180), 2); // 파란 테두리
        private readonly Brush _rowBack = new SolidBrush(Color.Black);
        private readonly Brush _rowText = new SolidBrush(Color.FromArgb(0, 200, 120)); // 녹-청 중간
        private readonly Brush _posText = new SolidBrush(Color.FromArgb(0, 220, 130));
        private readonly Brush _headerText = new SolidBrush(Color.Black);

        private void listViewAxis_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(_headerBack, e.Bounds);
            e.Graphics.DrawRectangle(_borderPen, e.Bounds);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
            if (e.Header.TextAlign == HorizontalAlignment.Right)
                flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Right;
            TextRenderer.DrawText(e.Graphics, e.Header.Text, _headerFont, e.Bounds, Color.Black, flags);
        }

        private void listViewAxis_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            // DrawSubItem 에서 전체 처리 → 여기서는 아무 것도 안 함
        }

        private void listViewAxis_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            var bounds = e.Bounds;
            e.Graphics.FillRectangle(_rowBack, bounds);
            e.Graphics.DrawRectangle(_borderPen, bounds);

            string text = e.SubItem.Text;
            var font = _rowFont;
            var brush = (e.ColumnIndex == 0) ? _rowText : _posText;

            var flags = TextFormatFlags.VerticalCenter | (e.ColumnIndex == 0 ? TextFormatFlags.Left : TextFormatFlags.Right);
            TextRenderer.DrawText(e.Graphics, text, font, bounds, ((SolidBrush)brush).Color, flags);
        }
        #endregion

        private static void EnableListViewDoubleBuffer(ListView lv)
        {
            try
            {
                typeof(ListView).InvokeMember("DoubleBuffered",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                    null, lv, new object[] { true });
            }
            catch { }
        }
    }
}