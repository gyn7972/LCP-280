using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D; // 그래픽 경로 사용을 위해 추가
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    public partial class ManualSequenceControl : UserControl
    {
        private BaseUnit m_ParentUnit;
        int SelectedIndex = -1;

        // ==========================================
        // [디자인 설정] 애플 스타일 (Apple-like)
        // ==========================================
        // 기본 배경: 아주 연한 회색 (깔끔함 유지)
        private Color _normalBackColor = Color.White;
        private Color _normalBorderColor = Color.FromArgb(220, 220, 220);
        private Color _normalTextColor = Color.FromArgb(50, 50, 50);

        // 선택 배경: 애플 특유의 부드러운 파란색
        private Color _selectedBackColor = Color.FromArgb(0, 122, 255);
        private Color _selectedBorderColor = Color.FromArgb(0, 122, 255);
        private Color _selectedTextColor = Color.White;

        // 현재 실행 중인 항목 (녹색 계열 포인트)
        private Color _runningTextColor = Color.FromArgb(52, 199, 89);

        // 폰트: 맑은 고딕
        private Font _itemFont = new Font("맑은 고딕", 10f, FontStyle.Regular);
        private Font _selectedItemFont = new Font("맑은 고딕", 10f, FontStyle.Bold);

        // [추가] 그래픽 객체 캐싱 (미리 선언)
        private SolidBrush _brushNormalBack;
        private SolidBrush _brushSelectedBack;
        private Pen _penNormalBorder;
        private Pen _penSelectedBorder;
        private SolidBrush _brushNormalText;
        private SolidBrush _brushSelectedText;
        private SolidBrush _brushRunningText;

        public BaseUnit ParentUnit
        {
            get { return m_ParentUnit; }
            set
            {
                m_ParentUnit = value;
                UpdateSeqList();
            }
        }

        private void UpdateSeqList()
        {
            if (m_ParentUnit == null) return;
            this._lstSteps.Items.Clear();
            SelectedIndex = -1;
            foreach (var v in m_ParentUnit.SequencePlayers)
            {
                int Index = this._lstSteps.Items.Add(v.Method.Name);
                if (m_ParentUnit.CurrentFunc != null)
                {
                    if (m_ParentUnit.CurrentFunc.Method.Name == v.Method.Name)
                    {
                        SelectedIndex = Index;
                    }
                }
            }
            this._lstSteps.SelectedIndex = SelectedIndex;
            this._lstSteps.Invalidate(); // 리스트 갱신 시 다시 그리기
        }

        public ManualSequenceControl()
        {
            InitializeComponent();

            // [추가] 그래픽 객체 초기화
            _brushNormalBack = new SolidBrush(_normalBackColor);
            _brushSelectedBack = new SolidBrush(_selectedBackColor);
            _penNormalBorder = new Pen(_normalBorderColor, 1);
            _penSelectedBorder = new Pen(_selectedBorderColor, 1);
            _brushNormalText = new SolidBrush(_normalTextColor);
            _brushSelectedText = new SolidBrush(_selectedTextColor);
            _brushRunningText = new SolidBrush(_runningTextColor);

            InitializeListBoxStyle(); // 스타일 초기화 호출
        }

        // [추가] 컨트롤 소멸 시 리소스 해제 필수
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _brushNormalBack?.Dispose();
                _brushSelectedBack?.Dispose();
                _penNormalBorder?.Dispose();
                _penSelectedBorder?.Dispose();
                _brushNormalText?.Dispose();
                _brushSelectedText?.Dispose();
                _brushRunningText?.Dispose();

                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        // 리스트박스 초기화 설정
        private void InitializeListBoxStyle()
        {
            // 아이템 높이를 넉넉하게 설정하여 버튼 느낌 제공
            _lstSteps.ItemHeight = 36;

            // 사용자 정의 그리기 모드 활성화
            _lstSteps.DrawMode = DrawMode.OwnerDrawFixed;

            // 테두리 없애기 (Flat 스타일)
            _lstSteps.BorderStyle = BorderStyle.None;
            _lstSteps.BackColor = Color.FromArgb(245, 245, 247); // 전체 배경색 (연한 회색)

            // 그리기 이벤트 연결
            _lstSteps.DrawItem += _lstSteps_DrawItem;
        }

        // [수정] DrawItem 메서드 최적화
        private void _lstSteps_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // 배경을 매번 지우지 않고 우리가 직접 다 그리므로 DrawBackground 생략 가능하지만, 안전하게 유지
            e.DrawBackground();
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            string text = _lstSteps.Items[e.Index].ToString();
            bool isRunning = false;

            if (m_ParentUnit != null && m_ParentUnit.CurrentFunc != null &&
                m_ParentUnit.CurrentFunc.Method.Name == text)
            {
                isRunning = true;
            }

            Rectangle bounds = e.Bounds;
            Rectangle buttonRect = new Rectangle(bounds.X + 4, bounds.Y + 2, bounds.Width - 8, bounds.Height - 4);

            // [변경] 캐싱된 객체 사용
            SolidBrush backBrush = isSelected ? _brushSelectedBack : _brushNormalBack;
            Pen borderPen = isSelected ? _penSelectedBorder : _penNormalBorder;
            SolidBrush textBrush;
            Font currentFont;

            if (isSelected)
            {
                textBrush = _brushSelectedText;
                currentFont = _selectedItemFont;
            }
            else
            {
                textBrush = isRunning ? _brushRunningText : _brushNormalText;
                currentFont = _itemFont;
            }

            // using 블록 제거 (캐싱된 객체이므로 Dispose 하면 안 됨)
            using (GraphicsPath path = GetRoundedRect(buttonRect, 8))
            {
                g.FillPath(backBrush, path);
                g.DrawPath(borderPen, path);
            }

            if (isRunning) text = "▶ " + text;

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;

            // TextRenderer는 Color 구조체를 받으므로 Brush.Color 사용
            TextRenderer.DrawText(g, text, currentFont, buttonRect, textBrush.Color, flags);
        }

        // 둥근 사각형 경로 생성 헬퍼
        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            // Top-Left
            path.AddArc(arc, 180, 90);

            // Top-Right
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);

            // Bottom-Right
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // Bottom-Left
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        private void _btnNext_Click(object sender, EventArgs e)
        {
            if (m_ParentUnit == null)
                return;

            this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
            this._lstSteps.SelectedIndex = this.SelectedIndex;
            if (this._lstSteps.SelectedIndex < 0)
            {
                this._lstSteps.SelectedIndex = 0;
            }

            if (this._lstSteps.SelectedIndex < m_ParentUnit.SequencePlayers.Count)
            {
                var func = m_ParentUnit.SequencePlayers[this._lstSteps.SelectedIndex];
                if (func != null)
                {
                    Task<int> t = m_ParentUnit.RunManualFunction(func);
                    m_ParentUnit.RunUnitStatus = BaseUnit.UnitStatus.ManualRunning;

                    UpdateSeqList();
                    ProgressForm form = new ProgressForm("Manual Running", func.Method.Name, t, m_ParentUnit);
                    if (t != null)
                    {
                        try
                        {
                            form.ShowDialog();
                            if (form.DialogResult == DialogResult.Cancel)
                            {
                                m_ParentUnit.CancelSequence();
                            }

                            if (t.Status == TaskStatus.RanToCompletion && t.Result == 0)
                            {
                                this.SelectedIndex++;
                                this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
                                this._lstSteps.SelectedIndex = this.SelectedIndex;
                                Log.Write("LCP_280", "_btnNext_Click", $"{func.ToString()},{this.SelectedIndex}");
                            }
                            else if (t.IsFaulted)
                            {
                                // 예외 메시지 표시
                                var mb = new MessageBoxOk();
                                mb.ShowDialog("Manual Run Error!", t.Exception?.GetBaseException().Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                    m_ParentUnit.RunUnitStatus = BaseUnit.UnitStatus.Stopped;
                }
            }
        }


        private void _btnRun_Click(object sender, EventArgs e)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Question", "시작 하시겠습니까?") != DialogResult.Yes)
            {
                return;
            }

            if (m_ParentUnit == null)
                return;

            if (this._lstSteps.SelectedIndex < 0)
            {
                this._lstSteps.SelectedIndex = 0;
            }

            if (this._lstSteps.SelectedIndex < m_ParentUnit.SequencePlayers.Count)
            {
                var func = m_ParentUnit.SequencePlayers[this._lstSteps.SelectedIndex];

                Task<int> t = m_ParentUnit.RunManualFunction(func);
                m_ParentUnit.RunUnitStatus = BaseUnit.UnitStatus.ManualRunning;

                SelectedIndex = this._lstSteps.SelectedIndex;
                UpdateSeqList();
                ProgressForm form = new ProgressForm("Manual Running", func.Method.Name, t, m_ParentUnit);
                form.ShowDialog();
                if (form.DialogResult == DialogResult.Cancel)
                {
                    m_ParentUnit.CancelSequence();
                }

                m_ParentUnit.RunUnitStatus = BaseUnit.UnitStatus.Stopped;
            }
        }


        private async void _btnPlay_Click(object sender, EventArgs e)
        {
            if (m_ParentUnit == null)
                return;

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", $"Equipment 인스턴스가 초기화되지 않았습니다.");
                    return;
                }

                if (!eq.EnsureAxisReadyForAutoOrMove("Play"))
                    return;

                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("확인", "시컨스를 진행하시겠습니까?") != DialogResult.Yes)
                    return;

                var unitName = m_ParentUnit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", $"UnitName 이 비어있습니다.");
                    return;
                }

                // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                if (m_ParentUnit.RunUnitStatus == BaseUnit.UnitStatus.ManualRunning)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Info!", $"Unit '{unitName}' 는 이미 실행 중입니다.");

                    return;
                }

                var btn = sender as Button;
                bool restore = false;
                if (btn != null && btn.Enabled)
                {
                    btn.Enabled = false;
                    restore = true;
                }
                Cursor prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                //bool ok = await eq.StartUnitAsync(unitName);
                bool ok = await eq.SequenceStartAsync(unitName, CancellationToken.None);
                if (!ok)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", $"Unit '{unitName}' 시작 실패.");
                }
                else
                {
                    // 필요 시 목록/상태 갱신
                    UpdateSeqList();
                }

                Cursor.Current = prev;
                if (restore)
                    btn.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private async void _btnStop_Click(object sender, EventArgs e)
        {
            if (m_ParentUnit == null)
                return;

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", $"Equipment 인스턴스가 없습니다.");

                    return;
                }

                var unitName = m_ParentUnit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", $"UnitName 이 비어 있습니다.");
                    return;
                }

                var btn = sender as Button;
                bool restore = false;
                if (btn != null && btn.Enabled)
                {
                    btn.Enabled = false;
                    restore = true;
                }
                var prevCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                //bool ok = await eq.StopUnitAsync(unitName);
                await eq.SequenceStopAsync(unitName, CancellationToken.None);
                //if (!ok)
                //{
                //    var mb = new MessageBoxOk();
                //    mb.ShowDialog("Error!", $"Unit '{unitName}' 정지 실패.");
                //}
                //else
                {
                    UpdateSeqList();
                }

                Cursor.Current = prevCursor;
                if (restore) btn.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);

                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", ex.Message);
            }
        }
    }
}