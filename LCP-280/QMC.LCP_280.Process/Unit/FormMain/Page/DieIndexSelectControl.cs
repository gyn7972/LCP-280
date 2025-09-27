using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    public partial class DieIndexSelectControl : UserControl
    {
        public enum DieState
        {
            Empty,    // 회색
            Picked    // 초록색
        }

        public class Die
        {
            public int Number { get; set; }      // 1~8번
            public string Id { get; set; }       // Die ID
            public PointF Position { get; set; }  // 실제 위치 (변환 전)
            public DieState State { get; set; }
            public string Info { get; set; } = ""; // 추가 정보

            public RectangleF GetBounds(float size)
            {
                return new RectangleF(
                    Position.X - size / 2,
                    Position.Y - size / 2,
                    size,
                    size);
            }
        }

        // Events
        public event EventHandler<Die> DieClicked;
        public event EventHandler<Die> DieStateChanged;
        public event EventHandler<Die> DieSelected;
        public event EventHandler<Die> DieHovered;
        public event EventHandler<int> RotationRequested; // 회전 요청 이벤트

        private List<Die> _dies = new List<Die>();
        private Die _selectedDie = null;
        private Die _hoveredDie = null;
        private bool _isAutoSequencing = false;
        private int _rotationOffset = 0; // 현재 회전 오프셋 (0~7)

        // ToolTip 관련
        private ToolTip _toolTip;
        private Timer _hoverTimer;

        // Transform 관련
        private float _scale = 1.0f;
        private PointF _offset = PointF.Empty;
        private bool _isDragging = false;
        private Point _lastMousePos;
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 3.0f;
        private const float SCALE_FACTOR = 1.1f;
        private const float BASE_DIE_SIZE = 40f; // 기본 다이 크기를 작게 조정

        // 고정 라벨 시스템 - 절대 위치로 관리
        private Dictionary<int, PointF> _fixedLabelPositions = new Dictionary<int, PointF>();
        private bool _labelPositionsInitialized = false;
        private SizeF _baseSize = new SizeF(400, 300); // 기준 크기 고정

        public DieIndexSelectControl()
        {
            InitializeComponent();
            InitializeToolTip();
            InitializeDies();
            SetupMouseHandlers();
        }

        private void InitializeToolTip()
        {
            _toolTip = new ToolTip();
            _toolTip.AutoPopDelay = 5000;
            _toolTip.InitialDelay = 300;
            _toolTip.ReshowDelay = 100;

            _hoverTimer = new Timer();
            _hoverTimer.Interval = 100;
            _hoverTimer.Tick += HoverTimer_Tick;
        }

        private void SetupMouseHandlers()
        {
            // 마우스 휠 이벤트 추가
            displayPanel.MouseWheel += DisplayPanel_MouseWheel;
            displayPanel.MouseDown += DisplayPanel_MouseDown;
            displayPanel.MouseUp += DisplayPanel_MouseUp;
            displayPanel.MouseMove += DisplayPanel_MouseMove;
            displayPanel.MouseClick += DisplayPanel_MouseClick;
            displayPanel.MouseDoubleClick += DisplayPanel_MouseDoubleClick;
            displayPanel.MouseLeave += DisplayPanel_MouseLeave;
        }

        private void InitializeDies()
        {
            _dies.Clear();

            // 8개 Die 초기화
            for (int i = 0; i < 8; i++)
            {
                _dies.Add(new Die
                {
                    Number = i + 1,
                    Id = $"DIE_{(i + 1):D3}",
                    Position = PointF.Empty, // CalculateDiePositions에서 설정
                    State = DieState.Empty,
                    Info = $"Index: {i + 1}, Status: Ready"
                });
            }

            CalculateDiePositions();
            InitializeFixedLabelPositions();
        }

        /// <summary>반시계방향 위치 계산</summary>
        private void CalculateDiePositions()
        {
            if (_dies.Count == 0) return;

            // 기준 크기를 사용하여 일관된 중심점 계산
            float centerX = _baseSize.Width / 2f;
            float centerY = _baseSize.Height / 2f;
            float radius = 80f;

            for (int i = 0; i < _dies.Count; i++)
            {
                // 9시 방향(180도)에서 시작하여 반시계방향으로 45도씩
                // 180도에서 시작: 180, 225, 270, 315, 0(360), 45, 90, 135
                double startAngle = 180; // 9시 방향
                double angleRadians = (startAngle + (i + _rotationOffset) * 45) * Math.PI / 180;

                float x = centerX + radius * (float)Math.Cos(angleRadians);
                float y = centerY - radius * (float)Math.Sin(angleRadians); // Y축 반전

                _dies[i].Position = new PointF(x, y);
            }
        }

        /// <summary>고정 라벨 위치 초기화 - 최초 한 번만 실행</summary>
        private void InitializeFixedLabelPositions()
        {
            if (_labelPositionsInitialized) return;

            _fixedLabelPositions.Clear();
            float centerX = _baseSize.Width / 2f;
            float centerY = _baseSize.Height / 2f;
            float radius = 80f;
            float dieSize = BASE_DIE_SIZE;

            // 각 Die 번호별로 고정 위치 계산 (회전 없는 초기 상태 기준)
            for (int dieNumber = 1; dieNumber <= 8; dieNumber++)
            {
                // Die 번호에 따른 초기 각도 (회전 없는 상태)
                int arrayIndex = dieNumber - 1;
                double startAngle = 180; // 9시 방향
                double angleRadians = (startAngle + arrayIndex * 45) * Math.PI / 180;

                float dieX = centerX + radius * (float)Math.Cos(angleRadians);
                float dieY = centerY - radius * (float)Math.Sin(angleRadians);
                PointF diePosition = new PointF(dieX, dieY);

                // 라벨 위치 계산
                PointF labelPosition = GetLabelPosition(diePosition, dieNumber, dieSize);
                _fixedLabelPositions[dieNumber] = labelPosition;
            }

            _labelPositionsInitialized = true;
        }

        #region ToolTip

        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();

            if (_hoveredDie != null)
            {
                string tooltipText = $"Die Number: {_hoveredDie.Number}\n";
                tooltipText += $"State: {_hoveredDie.State}\n";
                tooltipText += $"Die ID: {_hoveredDie.Id}";

                if (!string.IsNullOrEmpty(_hoveredDie.Info))
                    tooltipText += $"\nInfo: {_hoveredDie.Info}";

                Point mousePos = displayPanel.PointToClient(Cursor.Position);
                _toolTip.Show(tooltipText, displayPanel, mousePos.X + 10, mousePos.Y - 10);

                // 호버 이벤트 발생
                DieHovered?.Invoke(this, _hoveredDie);
            }
            else
            {
                _toolTip.Hide(displayPanel);
            }
        }

        #endregion

        #region Mouse Events

        private void DisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            //비트맵에 그리고
            //더블버퍼처리해

            Bitmap bmp = new Bitmap(displayPanel.Width, displayPanel.Height);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            g.TranslateTransform(_offset.X + (displayPanel.Width - _baseSize.Width) / 2f, _offset.Y + (displayPanel.Height - _baseSize.Height) / 2f);
            g.ScaleTransform(_scale, _scale);
            float dieSize = BASE_DIE_SIZE;
            foreach (var die in _dies)
            {
                DrawSocket(g, die, dieSize);
            }
            DrawFixedLabels(g, dieSize);
            e.Graphics.DrawImage(bmp, 0, 0);
            g.Dispose();
            bmp.Dispose();
        }

        private void DrawSocket(Graphics g, Die die, float size)
        {
            // 다이 색상 결정
            Color dieColor = Color.Black;
            Color textColor = Color.Lime;

            switch (die.State)
            {
                case DieState.Empty:
                    dieColor = Color.FromArgb(153, 153, 153);
                    textColor = Color.FromArgb(85, 85, 85);
                    break;
                case DieState.Picked:
                    dieColor = Color.FromArgb(0, 170, 0);
                    textColor = Color.White;
                    break;
            }

            RectangleF bounds = die.GetBounds(size);

            // 선택된 다이 강조
            if (die == _selectedDie)
            {
                using (Pen highlightPen = new Pen(Color.Yellow, 3f / _scale))
                {
                    float inflateSize = 3f / _scale;
                    RectangleF highlightBounds = new RectangleF(
                        bounds.X - inflateSize,
                        bounds.Y - inflateSize,
                        bounds.Width + inflateSize * 2,
                        bounds.Height + inflateSize * 2);
                    g.DrawEllipse(highlightPen, highlightBounds);
                }
            }

            // 다이 원 그리기
            using (Brush brush = new SolidBrush(dieColor))
            {
                g.FillEllipse(brush, bounds);
            }

            // 다이 테두리
            using (Pen pen = new Pen(Color.FromArgb(51, 51, 51), 1.5f / _scale))
            {
                g.DrawEllipse(pen, bounds);
            }

            // 다이 번호 표시
            using (Brush textBrush = new SolidBrush(textColor))
            {
                string text = die.Number.ToString();
                float fontSize = size * 0.5f; // 다이 크기에 비례한 폰트 크기
                using (Font font = new Font("Arial", fontSize, FontStyle.Bold))
                {
                    SizeF textSize = g.MeasureString(text, font);
                    float textX = die.Position.X - textSize.Width / 2;
                    float textY = die.Position.Y - textSize.Height / 2;
                    g.DrawString(text, font, textBrush, textX, textY);
                }
            }

            // Glow 효과 (Picked 상태)
            if (die.State == DieState.Picked)
            {
                using (Pen glowPen = new Pen(Color.FromArgb(100, 0, 255, 0), 3f / _scale))
                {
                    g.DrawEllipse(glowPen, bounds);
                }
            }
        }

        /// <summary>고정된 위치에 라벨 표시</summary>
        private void DrawFixedLabels(Graphics g, float dieSize)
        {
            if (!_labelPositionsInitialized) return;

            foreach (var kvp in _fixedLabelPositions)
            {
                int dieNumber = kvp.Key;
                PointF labelPosition = kvp.Value;
                string labelText = GetDieLabelText(dieNumber);

                if (!string.IsNullOrEmpty(labelText))
                {
                    // 라벨 텍스트 그리기
                    using (var labelBrush = new SolidBrush(Color.Black))
                    using (var labelFont = new Font("맑은 고딕", 8f / _scale, FontStyle.Regular))
                    {
                        using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        {
                            g.DrawString(labelText, labelFont, labelBrush, labelPosition, sf);
                        }
                    }
                }
            }
        }

        /// <summary>Die 번호에 따른 라벨 텍스트 반환</summary>
        private string GetDieLabelText(int dieNumber)
        {
            switch (dieNumber)
            {
                case 1: return "로더";
                case 2: return "메카";
                case 3: return "프로버\n검사";
                case 4: return "";
                case 5: return "언로더\n비전얼라인";
                case 6: return "";
                case 7: return "";
                case 8: return "";
                default: return "";
            }
        }

        /// <summary>Die 위치에 따른 라벨 위치 계산</summary>
        private PointF GetLabelPosition(PointF diePosition, int dieNumber, float dieSize)
        {
            float labelDistance = dieSize * 1.8f;

            switch (dieNumber)
            {
                case 1: // 9시 방향 -> 왼쪽에 라벨
                    return new PointF(diePosition.X - labelDistance, diePosition.Y);

                case 2: // 8시 방향 -> 왼쪽 아래에 라벨
                    return new PointF(diePosition.X - labelDistance * 0.7f, diePosition.Y + labelDistance * 0.7f);

                case 3: // 6시 방향 -> 아래에 라벨
                    return new PointF(diePosition.X, diePosition.Y + labelDistance);

                case 5: // 3시 방향 -> 오른쪽에 라벨
                    return new PointF(diePosition.X + labelDistance, diePosition.Y);

                default:
                    return diePosition;
            }
        }

        private void DisplayPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // 폼 크기 변경을 고려한 마우스 위치 계산
            float centerOffsetX = (displayPanel.Width - _baseSize.Width) / 2f;
            float centerOffsetY = (displayPanel.Height - _baseSize.Height) / 2f;
            PointF mousePos = new PointF(e.X - centerOffsetX, e.Y - centerOffsetY);

            float oldScale = _scale;

            if (e.Delta > 0)
            {
                _scale *= SCALE_FACTOR;
            }
            else
            {
                _scale /= SCALE_FACTOR;
            }

            // 스케일 제한
            _scale = Math.Max(MIN_SCALE, Math.Min(MAX_SCALE, _scale));

            // 마우스 위치를 중심으로 스케일 조정
            if (oldScale != _scale)
            {
                float scaleChange = _scale / oldScale;
                _offset.X = mousePos.X - (mousePos.X - _offset.X) * scaleChange;
                _offset.Y = mousePos.Y - (mousePos.Y - _offset.Y) * scaleChange;
            }

            displayPanel.Invalidate();
        }

        private void DisplayPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 클릭된 다이 확인
                Die clickedDie = GetDieAtPoint(e.Location);

                if (clickedDie != null)
                {
                    // 다이 클릭 처리
                    OnDieClick(clickedDie);
                }
                else
                {
                    // 빈 공간 클릭 시 드래그 시작
                    _isDragging = true;
                    _lastMousePos = e.Location;
                    displayPanel.Cursor = Cursors.Hand;
                }
            }
        }

        private void DisplayPanel_MouseUp(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            UpdateCursor(e.Location);
        }

        private void Display_MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;

                case MouseButtons.Right:
                    break;
            }
        }

        private void Display_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;

                case MouseButtons.Right:
                    _scale = 1.0f;
                    _offset = PointF.Empty;
                    displayPanel.Invalidate();
                    break;
            }

        }


        private void DisplayPanel_MouseClick(object sender, MouseEventArgs e)
        {
            // MouseDown에서 처리하므로 여기서는 처리하지 않음
        }

        private void DisplayPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                // 드래그 처리
                _offset.X += e.X - _lastMousePos.X;
                _offset.Y += e.Y - _lastMousePos.Y;
                _lastMousePos = e.Location;
                displayPanel.Invalidate();
            }
            else
            {
                // 호버 처리
                Die hoveredDie = GetDieAtPoint(e.Location);
                if (hoveredDie != _hoveredDie)
                {
                    _hoveredDie = hoveredDie;
                    _hoverTimer.Stop();
                    _hoverTimer.Start();
                    displayPanel.Invalidate();
                }
                UpdateCursor(e.Location);
            }
        }

        private void DisplayPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // 우클릭 더블클릭으로 뷰 리셋
                ResetView();
            }
        }

        private void DisplayPanel_MouseLeave(object sender, EventArgs e)
        {
            _hoveredDie = null;
            _toolTip.Hide(displayPanel);
            _hoverTimer.Stop();
            displayPanel.Invalidate();
        }
        #endregion

        private void UpdateCursor(Point location)
        {
            Die die = GetDieAtPoint(location);
            displayPanel.Cursor = die != null ? Cursors.Hand : Cursors.Default;
        }

        private Die GetDieAtPoint(Point screenPoint)
        {
            // 화면 좌표를 실제 좌표로 변환 (중심 오프셋 고려)
            PointF realPoint = ScreenToReal(screenPoint);
            float dieSize = BASE_DIE_SIZE;

            foreach (var die in _dies)
            {
                RectangleF bounds = die.GetBounds(dieSize);

                // 원형 충돌 검사
                float dx = realPoint.X - die.Position.X;
                float dy = realPoint.Y - die.Position.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                if (distance <= dieSize / 2)
                {
                    return die;
                }
            }

            return null;
        }

        private PointF ScreenToReal(Point screenPoint)
        {
            // 폼 크기 변경을 고려한 화면 좌표를 실제 좌표로 변환
            float centerOffsetX = (displayPanel.Width - _baseSize.Width) / 2f;
            float centerOffsetY = (displayPanel.Height - _baseSize.Height) / 2f;

            float x = (screenPoint.X - _offset.X - centerOffsetX) / _scale;
            float y = (screenPoint.Y - _offset.Y - centerOffsetY) / _scale;
            return new PointF(x, y);
        }

        private void OnDieClick(Die die)
        {
            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("Die Index 선택", $"현재 상태: '{die.State.ToString()}', Die Index: '{die.Number}'번으로 선택하시겠습니까?") == DialogResult.Yes)
            {
                if (_isAutoSequencing) return;

                // 상태 변경 (Picked -> Empty -> Picked -> Empty)
                switch (die.State)
                {
                    case DieState.Picked:
                        die.State = DieState.Empty;
                        break;
                    case DieState.Empty:
                        die.State = DieState.Picked;
                        break;
                }

                // 선택된 다이 업데이트
                SelectDie(die);

                // 이벤트 발생
                DieClicked?.Invoke(this, die);
                DieStateChanged?.Invoke(this, die);

                displayPanel.Invalidate();
            }
        }

        private void DisplayPanel_Resize(object sender, EventArgs e)
        {
            // 크기 변경 시에도 동일한 기준 크기를 사용하므로 Die와 라벨 위치는 변경되지 않음
            // 단지 화면에서 중심 조정만 수행
            displayPanel.Invalidate();
        }

        private void SelectDie(Die die)
        {
            _selectedDie = die;
            UpdateDieInfo();
            DieSelected?.Invoke(this, die);
        }

        private void UpdateDieInfo()
        {
            if (_selectedDie != null)
            {
                if (_selectedDie.State != DieState.Empty)
                {
                    lblDieIdValue.Text = _selectedDie.Id;
                    lblDieNumberValue.Text = _selectedDie.Number.ToString();
                }
                else
                {
                    lblDieIdValue.Text = "EMPTY";
                    lblDieNumberValue.Text = _selectedDie.Number.ToString();
                }
            }
            else
            {
                lblDieIdValue.Text = "N/A";
                lblDieNumberValue.Text = "0";
            }
        }

        private void btnRotateCounterClockwise_Click(object sender, EventArgs e)
        {
            if (_isAutoSequencing) return;

            RotateCounterClockwise();
        }

        private async void BtnAutoSequence_Click(object sender, EventArgs e)
        {
            if (_isAutoSequencing) return;

            _isAutoSequencing = true;
            btnAutoSequence.Enabled = false;
            btnReset.Enabled = false;

            try
            {
                // 1번부터 8번까지 순차적으로 Picked 상태로 변경
                foreach (var die in _dies.OrderBy(d => d.Number))
                {
                    die.State = DieState.Picked;
                    SelectDie(die);
                    DieStateChanged?.Invoke(this, die);
                    displayPanel.Invalidate();
                    await Task.Delay(500);
                }
            }
            finally
            {
                _isAutoSequencing = false;
                btnAutoSequence.Enabled = true;
                btnReset.Enabled = true;
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        #region Public Methods

        /// <summary>반시계방향으로 한 칸 회전</summary>
        public void RotateCounterClockwise()
        {
            RotateOneStep();
        }

        /// <summary>반시계방향 한 스텝 회전 (45도)</summary>
        private void RotateOneStep()
        {
            // 회전 요청 이벤트 발생 (실제 장비 제어는 Monitoring_Main에서)
            // 로딩 시에 현재 스텝 위치도 넣어주면 좋을듯..?
            RotationRequested?.Invoke(this, _rotationOffset);

            Console.WriteLine($"현재 Step {_rotationOffset})");
        }

        public void UpdateRotationUI(int newRotationOffset)
        {
            _rotationOffset = newRotationOffset % 8;
            CalculateDiePositions();
            displayPanel.Invalidate();
        }

        public void UpdateLoadingNumber(int num)
        {
            _rotationOffset = num;
            UpdateRotationUI(_rotationOffset);
        }

        public void SetDieState(int dieNumber, DieState state)
        {
            var die = _dies.FirstOrDefault(d => d.Number == dieNumber);
            if (die != null)
            {
                die.State = state;
                if (_selectedDie == die)
                {
                    UpdateDieInfo();
                }
                displayPanel.Invalidate();
                DieStateChanged?.Invoke(this, die);
            }
        }

        public DieState GetDieState(int dieNumber)
        {
            var die = _dies.FirstOrDefault(d => d.Number == dieNumber);
            return die?.State ?? DieState.Empty;
        }

        public List<Die> GetAllDies()
        {
            return new List<Die>(_dies);
        }

        public Die GetSelectedDie()
        {
            return _selectedDie;
        }

        public void Reset()
        {
            foreach (var die in _dies)
            {
                die.State = DieState.Empty;
            }
            _selectedDie = null;
            UpdateDieInfo();
            displayPanel.Invalidate();
        }

        public void ResetView()
        {
            _scale = 1.0f;
            _offset = PointF.Empty;
            displayPanel.Invalidate();
        }

        public void ClearSelection()
        {
            _selectedDie = null;
            UpdateDieInfo();
            displayPanel.Invalidate();
        }

        /// <summary>라벨 위치를 강제로 재초기화</summary>
        public void ResetLabelPositions()
        {
            _labelPositionsInitialized = false;
            _fixedLabelPositions.Clear();
            InitializeFixedLabelPositions();
            displayPanel.Invalidate();
        }

        #endregion
    }
} 