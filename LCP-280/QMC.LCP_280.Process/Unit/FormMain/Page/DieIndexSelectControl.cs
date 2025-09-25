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

        private List<Die> _dies = new List<Die>();
        private Die _selectedDie = null;
        private bool _isAutoSequencing = false;

        // Transform 관련
        private float _scale = 1.0f;
        private PointF _offset = PointF.Empty;
        private bool _isDragging = false;
        private Point _lastMousePos;
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 3.0f;
        private const float SCALE_FACTOR = 1.1f;
        private const float BASE_DIE_SIZE = 40f; // 기본 다이 크기를 작게 조정

        public DieIndexSelectControl()
        {
            InitializeComponent();
            InitializeDies();
            SetupMouseHandlers();
        }

        private void SetupMouseHandlers()
        {
            // 마우스 휠 이벤트 추가
            displayPanel.MouseWheel += DisplayPanel_MouseWheel;
            displayPanel.MouseDown += DisplayPanel_MouseDown;
            displayPanel.MouseUp += DisplayPanel_MouseUp;
            displayPanel.MouseClick += Display_MouseClick;
            displayPanel.MouseDoubleClick += Display_MouseDoubleClick;
        }

        private void InitializeDies()
        {
            _dies.Clear();

            // 패널의 중심점 (실제 좌표계)
            float centerX = displayPanel.Width / 2f; // 320의 절반
            float centerY = displayPanel.Height / 2f; // 300의 절반
            float radius = 80f;   // 반경을 작게 조정

            // 8개 위치 (이미지와 동일한 배치)
            var positions = new[]
            {
                new PointF(centerX - radius, centerY),                    // 1번: 왼쪽
                new PointF(centerX - radius * 0.7f, centerY + radius * 0.7f), // 2번: 왼쪽 아래
                new PointF(centerX, centerY + radius),                    // 3번: 아래
                new PointF(centerX + radius * 0.7f, centerY + radius * 0.7f), // 4번: 오른쪽 아래
                new PointF(centerX + radius, centerY),                    // 5번: 오른쪽
                new PointF(centerX + radius * 0.7f, centerY - radius * 0.7f), // 6번: 오른쪽 위
                new PointF(centerX, centerY - radius),                    // 7번: 위
                new PointF(centerX - radius * 0.7f, centerY - radius * 0.7f)  // 8번: 왼쪽 위
            };

            for (int i = 0; i < 8; i++)
            {
                _dies.Add(new Die
                {
                    Number = i + 1,
                    Id = $"DIE_{(i + 1):D3}",
                    Position = positions[i],
                    State = DieState.Empty
                });
            }
        }

        private void DisplayPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(displayPanel.BackColor);

            // 변환 적용
            g.TranslateTransform(_offset.X, _offset.Y);
            g.ScaleTransform(_scale, _scale);

            // 다이 그리기
            float dieSize = BASE_DIE_SIZE;
            foreach (var die in _dies)
            {
                DrawDie(g, die, dieSize);
            }

            g.ResetTransform();
        }

        private void DrawDie(Graphics g, Die die, float size)
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

        private void DisplayPanel_MouseWheel(object sender, MouseEventArgs e)
        {
            // 마우스 위치를 중심으로 줌
            PointF mousePos = new PointF(e.X, e.Y);

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
                // 커서 업데이트
                UpdateCursor(e.Location);
            }
        }

        private void UpdateCursor(Point location)
        {
            Die die = GetDieAtPoint(location);
            displayPanel.Cursor = die != null ? Cursors.Hand : Cursors.Default;
        }

        private Die GetDieAtPoint(Point screenPoint)
        {
            // 화면 좌표를 실제 좌표로 변환
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
            // 화면 좌표를 실제 좌표로 변환
            float x = (screenPoint.X - _offset.X) / _scale;
            float y = (screenPoint.Y - _offset.Y) / _scale;
            return new PointF(x, y);
        }

        private void OnDieClick(Die die)
        {
            var result = MessageBox.Show(
                $"현재 상태: '{die.State.ToString()}', Die Index: '{die.Number}'번으로 선택하시겠습니까?",
                "Die Index Select",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
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
            // 크기 변경 시 다이 위치 재계산
            InitializeDies();
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

        #endregion
    }
}
