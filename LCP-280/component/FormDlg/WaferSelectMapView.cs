using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using static QMC.Common.Material;

namespace QMC.LCP_280.Process.Component
{
    public partial class WaferSelectMapView : UserControl
    {
        // 슬롯 상태 (MaterialCassette 기반)
        public enum SlotDisplayState
        {
            Empty,      // 없음 - 회색
            Present     // 있음 - 초록
        }

        // 이벤트 정의
        public event EventHandler<SlotClickedEventArgs> SlotClicked;
        public event EventHandler<SlotSelectionChangedEventArgs> SlotSelectionChanged;
        public event EventHandler AllApplyRequested;    // 전체 Exist/Ready 적용 요청
        public event EventHandler ResetCassetteRequested; // Cassette 리셋 요청

        public class SlotClickedEventArgs : EventArgs
        {
            public int SlotNumber { get; set; }
            public Rectangle SlotBounds { get; set; }
            public Point ClickPosition { get; set; }
            public SlotDisplayState State { get; set; }
        }

        public class SlotSelectionChangedEventArgs : EventArgs
        {
            public int SlotNumber { get; set; }
            public bool IsSelected { get; set; }
            public int SelectionOrder { get; set; } // 선택된 순서 (1, 2, 3...)
            public SlotDisplayState State { get; set; }
        }

        private MaterialCassette _materialCassette;

        // [변경] 고정 셀 높이 사용 (ID 표시를 위해 키움)
        private int _fixedCellHeight = 25;
        private int _currentCellSize = 25; // 실제 그리기 계산용

        // 선택 관련
        private readonly HashSet<int> _selectedSlots = new HashSet<int>();
        private readonly Dictionary<int, int> _selectionOrder = new Dictionary<int, int>(); // 슬롯번호 -> 선택순서
        private int _nextSelectionNumber = 1;

        // 툴팁 관련
        private ToolTip _toolTip;
        private Timer _hoverTimer;
        private int _hoveredSlot = -1;

        private readonly object _selectionLock = new object();

        public bool IsEditEnabled { get; set; } = true;
        public bool EnableSelectionOnClick { get; set; } = false; // 기본 false

        // 렌더링 타이머 (실시간 갱신용)
        private Timer _renderTimer;
        private bool _liveRenderEnabled;

        public WaferSelectMapView()
        {
            InitializeComponent();

            // [변경] 스크롤 활성화
            this.AutoScroll = false;

            // 더블 버퍼링 설정
            SetupDoubleBuffering();
            // 툴팁 초기화
            InitializeToolTip();
            // 우측 패널/버튼 초기화 (필요시 사용)
            InitializeTestButtons();

            if (pWaferImage != null)
            {
                // [중요 변경] 패널 내부 스크롤만 활성화
                pWaferImage.AutoScroll = true;

                // pWaferImage 패널 제거 혹은 숨김 처리 권장 (UserControl 자체 스크롤 사용)
                // 만약 디자이너에 있다면 아래처럼 이벤트 연결
                pWaferImage.Paint -= WaferImage_Paint;
                pWaferImage.Paint += WaferImage_Paint;

                pWaferImage.MouseClick += WaferImage_MouseClick;
                pWaferImage.MouseMove += WaferImage_MouseMove;
                pWaferImage.MouseLeave += WaferImage_MouseLeave;

                // [변경] 리사이즈 이벤트에서 스크롤 사이즈 갱신
                pWaferImage.Resize += (s, e) =>
                {
                    UpdateScrollSize();
                    SafeInvalidate();
                };

                EnablePanelDoubleBuffering(pWaferImage);
            }

            // 실시간 렌더링 타이머 설정
            _renderTimer = new Timer { Interval = 100 };
            _renderTimer.Tick += (s, e) =>
            {
                if (!_liveRenderEnabled) return;
                if (!Visible) return;
                SafeInvalidate();
            };

            EnableLiveRender(true, 30);
        }

        // 컨트롤 표시 상태 바뀔 때 즉시 1회 리프레시
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                RefreshMapImmediate();
            }
        }

        public void EnableLiveRender(bool enabled, int fps = 30)
        {
            _liveRenderEnabled = enabled;
            int interval = Math.Max(10, 1000 / Math.Max(1, fps));
            _renderTimer.Interval = interval;
            _renderTimer.Enabled = enabled;
        }

        private void SetupDoubleBuffering()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw,
                          true);
            this.UpdateStyles();
        }

        private void EnablePanelDoubleBuffering(Control control)
        {
            if (control == null) return;
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private void InitializeTestButtons()
        {
            // (기존 코드 유지)
            // 필요 시 UI 요소 추가
        }

        private void InitializeToolTip()
        {
            _toolTip = new ToolTip();
            _toolTip.AutoPopDelay = 5000;
            _toolTip.InitialDelay = 300;
            _toolTip.ReshowDelay = 100;

            _hoverTimer = new Timer();
            _hoverTimer.Interval = 200;
            _hoverTimer.Tick += HoverTimer_Tick;
        }

        #region 버튼 이벤트 핸들러
        private void btn_All_Click(object sender, EventArgs e)
        {
            if (!IsEditEnabled) return;
            AllApplyRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btn_ResetAll_Click(object sender, EventArgs e)
        {
            if (!IsEditEnabled) return;
            ResetCassetteRequested?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Cassette 바인딩/갱신

        public void SetMaterialCassette(MaterialCassette materialCassette)
        {
            if (materialCassette == null)
                throw new ArgumentNullException(nameof(materialCassette));

            _materialCassette = materialCassette;

            if (_materialCassette.Slots == null || _materialCassette.Slots.Count != _materialCassette.SlotCount)
            {
                _materialCassette.Slots = Enumerable.Repeat<MaterialWafer>(null, _materialCassette.SlotCount).ToList();
            }

            ClearSelection();
            UpdateScrollSize();
            SafeInvalidate();
        }

        public void NotifyCassetteChanged()
        {
            UpdateScrollSize();
            SafeInvalidate();
        }

        public void RefreshMapImmediate()
        {
            if (InvokeRequired)
            {
                try { BeginInvoke((Action)RefreshMapImmediate); } catch { }
                return;
            }
            UpdateScrollSize();
            SafeInvalidate();
        }

        private void EnsureSlotInitialized(int index)
        {
            if (_materialCassette == null) return;
            if (index < 0 || index >= _materialCassette.SlotCount) return;

            var wafer = _materialCassette.GetWafer(index);
            if (wafer == null)
            {
                wafer = new MaterialWafer
                {
                    Presence = MaterialPresence.NotExist,
                    ProcessSatate = MaterialProcessSatate.Unknown,
                    CarrierId = _materialCassette.CarrierId,
                    SlotIndex = index
                };
                _materialCassette.SetWafer(index, wafer);
            }
        }

        /// <summary>
        /// [변경] pWaferImage 패널의 가상 크기 설정
        /// </summary>
        private void UpdateScrollSize()
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0) return;
            if (pWaferImage == null) return;

            _currentCellSize = _fixedCellHeight;

            int topMargin = 14;
            int totalHeight = topMargin + (_currentCellSize * _materialCassette.SlotCount) + 10;

            // [핵심] pWaferImage의 AutoScrollMinSize를 설정하여 스크롤바 생성
            pWaferImage.AutoScrollMinSize = new Size(0, totalHeight);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollSize();
            SafeInvalidate();
        }

        private void SafeInvalidate()
        {
            if (!IsHandleCreated) 
                return;
            if (InvokeRequired) { try { BeginInvoke((Action)SafeInvalidate); } catch { } return; }

            // 패널만 갱신
            if (pWaferImage != null)
            {
                pWaferImage.Invalidate();
                pWaferImage.Update();
            }
        }
        #endregion

        #region 마우스 이벤트 처리 (스크롤 고려)

        private void WaferImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (_materialCassette == null) 
                return;

            if (!IsEditEnabled) 
                return;

            if (e.Button == MouseButtons.Left)
            {
                int slotNumber = GetSlotNumberAtPosition(e.Location);
                if (slotNumber > 0)
                {
                    HandleSlotClick(slotNumber, e.Location);
                }
            }
        }

        private void WaferImage_MouseMove(object sender, MouseEventArgs e)
        {
            int slotNumber = GetSlotNumberAtPosition(e.Location);
            if (slotNumber != _hoveredSlot)
            {
                _hoveredSlot = slotNumber;
                _hoverTimer.Stop();
                if (_hoveredSlot > 0)
                {
                    _hoverTimer.Start();
                }
                else
                {
                    // [수정] 캐스팅 오류 해결: 명시적 (Control) 캐스팅 사용
                    _toolTip.Hide((Control)pWaferImage ?? (Control)this);
                }
            }
        }

        private void WaferImage_MouseLeave(object sender, EventArgs e)
        {
            _hoveredSlot = -1;
            // [수정] 캐스팅 오류 해결: 명시적 (Control) 캐스팅 사용
            _toolTip.Hide((Control)pWaferImage ?? (Control)this);
            _hoverTimer.Stop();
        }

        /// <summary>
        /// 마우스 위치에서 슬롯 번호 계산 (스크롤 오프셋 반영)
        /// </summary>
        private int GetSlotNumberAtPosition(Point mousePos)
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0) return -1;
            if (pWaferImage == null) return -1;

            // [중요] pWaferImage의 스크롤 위치 가져오기
            // Panel의 AutoScrollPosition은 음수로 반환되므로 부호를 뒤집거나 그대로 더합니다.
            // e.Location은 클라이언트 기준(보이는 화면 좌상단 0,0) 좌표입니다.
            // 논리 좌표 = 마우스Y - AutoScrollPosition.Y (음수) => 마우스Y + 스크롤된만큼

            int scrollY = pWaferImage.AutoScrollPosition.Y;

            int topMargin = 14;
            int totalHeight = (_currentCellSize * _materialCassette.SlotCount);

            // 논리 Y좌표 (스크롤 위쪽 숨겨진 영역 포함)
            int logicalY = mousePos.Y - scrollY;

            if (logicalY < topMargin) return -1;
            if (logicalY > topMargin + totalHeight) return -1;

            // 바닥부터 1번
            int drawBottom = topMargin + totalHeight;
            int fromBottom = drawBottom - logicalY;

            if (fromBottom < 0) return -1;

            int slotIndex = fromBottom / _currentCellSize;
            if (slotIndex >= 0 && slotIndex < _materialCassette.SlotCount)
                return slotIndex + 1;

            return -1;
        }

        private void HandleSlotClick(int slotNumber, Point clickPosition)
        {
            if (_materialCassette == null) return;
            if (!IsEditEnabled) return;

            var wafer = _materialCassette.GetWafer(slotNumber - 1);
            var state = GetSlotDisplayState(wafer);

            SlotClicked?.Invoke(this, new SlotClickedEventArgs
            {
                SlotNumber = slotNumber,
                SlotBounds = GetSlotBounds(slotNumber),
                ClickPosition = clickPosition,
                State = state
            });

            if (EnableSelectionOnClick) 
                ToggleSlotSelection(slotNumber, state);
        }

        private void ToggleSlotSelection(int slotNumber, SlotDisplayState state)
        {
            if (_selectedSlots.Contains(slotNumber)) 
                DeselectSlot(slotNumber, state);

            else SelectSlot(slotNumber, state);
        }

        private void SelectSlot(int slotNumber, SlotDisplayState state)
        {
            lock (_selectionLock)
            {
                if (_selectedSlots.Contains(slotNumber)) return;
                _selectedSlots.Add(slotNumber);
                _selectionOrder[slotNumber] = _nextSelectionNumber++;
            }
            SafeInvalidate();
            RunOnUI(UpdateUI);
            SlotSelectionChanged?.Invoke(this, new SlotSelectionChangedEventArgs { SlotNumber = slotNumber, IsSelected = true, SelectionOrder = GetSlotSelectionOrder(slotNumber), State = state });
        }

        private void DeselectSlot(int slotNumber, SlotDisplayState state)
        {
            lock (_selectionLock)
            {
                if (!_selectionOrder.ContainsKey(slotNumber)) return;
                int currentOrder = _selectionOrder[slotNumber];
                _selectedSlots.Remove(slotNumber);
                _selectionOrder.Remove(slotNumber);
                var toUpdate = _selectionOrder.Where(kvp => kvp.Value > currentOrder).ToList();
                foreach (var kvp in toUpdate) _selectionOrder[kvp.Key] = kvp.Value - 1;
                _nextSelectionNumber = _selectionOrder.Count > 0 ? _selectionOrder.Values.Max() + 1 : 1;
            }
            SafeInvalidate();
            RunOnUI(UpdateUI);
            SlotSelectionChanged?.Invoke(this, new SlotSelectionChangedEventArgs { SlotNumber = slotNumber, IsSelected = false, SelectionOrder = 0, State = state });
        }

        private Rectangle GetSlotBounds(int slotNumber)
        {
            if (_materialCassette == null || slotNumber <= 0 || slotNumber > _materialCassette.SlotCount || pWaferImage == null)
                return Rectangle.Empty;

            int width = pWaferImage.ClientSize.Width - 4;
            int topMargin = 14;
            int totalHeight = (_currentCellSize * _materialCassette.SlotCount);
            int slotIndex = slotNumber - 1;

            // 논리 Y
            int yFromTop = topMargin + totalHeight - ((slotIndex + 1) * _currentCellSize);

            // 화면 Y (스크롤 적용)
            int screenY = yFromTop + pWaferImage.AutoScrollPosition.Y;

            return new Rectangle(2, screenY, width, _currentCellSize);
        }
        #endregion

        #region 툴팁
        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();
            if (_hoveredSlot > 0 && _materialCassette != null)
            {
                var wafer = _materialCassette.GetWafer(_hoveredSlot - 1);

                string tooltipText = $"Slot {_hoveredSlot}";
                if (!string.IsNullOrEmpty(wafer?.WaferId))
                    tooltipText += $"\nID: {wafer.WaferId}";

                // [수정] 캐스팅 오류 해결: 명시적 (Control) 캐스팅 사용
                Point mousePos = ((Control)pWaferImage ?? (Control)this).PointToClient(Cursor.Position);
                _toolTip.Show(tooltipText, this, mousePos.X + 10, mousePos.Y - 10);
            }
        }
        #endregion

        #region 그리기

        private void WaferImage_Paint(object sender, PaintEventArgs e)
        {
            // Panel의 Paint 이벤트 핸들러
            DrawMap(e.Graphics, pWaferImage.ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (pWaferImage == null)
                DrawMap(e.Graphics, ClientRectangle);
        }

        private void DrawMap(Graphics g, Rectangle bounds)
        {
            g.Clear(Color.Black);

            if (_materialCassette == null || _materialCassette.SlotCount <= 0)
            {
                g.ResetTransform();
                DrawCenterMessage(g, bounds, "No Data");
                return;
            }

            // [중요] pWaferImage의 AutoScrollPosition 적용
            // Panel 내부에서는 이미 스크롤 오프셋만큼 좌표계가 이동되어 있지 않음 (UserPaint 방식일 때)
            // 따라서 직접 TranslateTransform을 해야 함.
            if (pWaferImage != null)
            {
                g.TranslateTransform(pWaferImage.AutoScrollPosition.X, pWaferImage.AutoScrollPosition.Y);
            }

            int topMargin = 14;
            int totalHeight = (_currentCellSize * _materialCassette.SlotCount);
            // 스크롤바 너비를 고려하지 않고 ClientRectangle 너비 사용 (이미 스크롤바 제외된 크기임)
            int width = bounds.Width - 4;
            int total = _materialCassette.SlotCount;

            using (var textBrush = new SolidBrush(Color.Black))
            using (var font = new Font("Arial", 10f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i < total; i++)
                {
                    int slotNumber = i + 1;
                    var wafer = _materialCassette.GetWafer(i);
                    Color cellColor = GetSlotColor(wafer);

                    // 논리적 Y 좌표 계산
                    int y = topMargin + totalHeight - (slotNumber * _currentCellSize);
                    var rect = new Rectangle(2, y, width, _currentCellSize);

                    // 그리기
                    using (var brush = new SolidBrush(cellColor)) g.FillRectangle(brush, rect);
                    using (var pen = new Pen(Color.Black)) g.DrawRectangle(pen, rect);

                    if (wafer != null && !string.IsNullOrEmpty(wafer.WaferId))
                    {
                        var state = g.Save();
                        g.SetClip(rect);
                        g.DrawString(wafer.WaferId, font, textBrush, rect, sf);
                        g.Restore(state);
                    }

                    if (_selectedSlots.Contains(slotNumber) && _selectionOrder.ContainsKey(slotNumber))
                        DrawSelectionOrder(g, rect, _selectionOrder[slotNumber]);

                    DrawSlotNumber(g, rect, slotNumber);
                }
            }
            g.ResetTransform();
        }
        private Color GetSlotColor(MaterialWafer wafer)
        {
            if (wafer == null) return Color.Gray;

            Color cellColor;
            switch (wafer.Presence)
            {
                case MaterialPresence.Exist: cellColor = Color.LimeGreen; break;
                case MaterialPresence.NotExist: cellColor = Color.Gray; break;
                default: cellColor = Color.Gray; break;
            }

            switch (wafer.ProcessSatate)
            {
                case MaterialProcessSatate.Ready: cellColor = Color.LightSkyBlue; break; // [변경] 가독성 위해 밝은 파랑
                case MaterialProcessSatate.Processing: cellColor = Color.Orange; break;
                case MaterialProcessSatate.Completed: cellColor = Color.Green; break;
                default: break;
            }
            return cellColor;
        }

        private SlotDisplayState GetSlotDisplayState(MaterialWafer wafer)
        {
            if (wafer == null) return SlotDisplayState.Empty;
            return wafer.Presence == MaterialPresence.Exist ? SlotDisplayState.Present : SlotDisplayState.Empty;
        }

        private void DrawSelectionOrder(Graphics g, Rectangle rect, int order)
        {
            int circleSize = Math.Min(20, rect.Height - 4); // [변경] 크기 제한
            int circleX = rect.Right - circleSize - 4;
            int circleY = rect.Top + 2;
            var circleRect = new Rectangle(circleX, circleY, circleSize, circleSize);

            using (var brush = new SolidBrush(Color.White))
                g.FillEllipse(brush, circleRect);
            using (var pen = new Pen(Color.Black))
                g.DrawEllipse(pen, circleRect);

            using (var font = new Font("Arial", 8f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(order.ToString(), font, brush, circleRect, sf);
            }
        }

        private void DrawSlotNumber(Graphics g, Rectangle rect, int slotNumber)
        {
            using (var font = new Font("Arial", 7f, FontStyle.Bold))
            using (var path = new GraphicsPath())
            using (var outlinePen = new Pen(Color.White, 3f))
            using (var fillBrush = new SolidBrush(Color.Black))
            {
                string text = slotNumber.ToString();
                float x = rect.Left + 4;
                float y = rect.Top + 2;

                path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new PointF(x, y), StringFormat.GenericDefault);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(outlinePen, path);
                g.FillPath(fillBrush, path);
                g.SmoothingMode = SmoothingMode.Default;
            }
        }

        private void DrawCenterMessage(Graphics g, Rectangle area, string text)
        {
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var f = new Font(Font.FontFamily, 9f, FontStyle.Italic))
            using (var b = new SolidBrush(Color.DarkGray))
            {
                g.DrawString(text, f, b, area, sf);
            }
        }
        #endregion

        #region UI 업데이트 / 공개 메서드
        private void RunOnUI(Action action)
        {
            if (action == null) return;
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired)
                try { BeginInvoke(action); } catch { }
            else
                action();
        }

        private void UpdateUI()
        {
            // 필요 시 우측 정보 패널 업데이트 로직 복원
        }

        public void ClearSelection()
        {
            lock (_selectionLock)
            {
                _selectedSlots.Clear();
                _selectionOrder.Clear();
                _nextSelectionNumber = 1;
            }
            SafeInvalidate();
            RunOnUI(UpdateUI);
        }

        public List<int> GetSelectedSlots()
        {
            lock (_selectionLock) return _selectedSlots.ToList();
        }

        public List<int> GetSelectedSlotsInOrder()
        {
            lock (_selectionLock) return _selectionOrder.OrderBy(k => k.Value).Select(k => k.Key).ToList();
        }

        public int GetSlotSelectionOrder(int slotNumber)
        {
            lock (_selectionLock) return _selectionOrder.ContainsKey(slotNumber) ? _selectionOrder[slotNumber] : 0;
        }

        public int GetSelectedCount()
        {
            lock (_selectionLock) return _selectedSlots.Count;
        }

        public void SetSlotPresent(int slotNumber, bool present)
        {
            if (_materialCassette == null) return;
            int idx = slotNumber - 1;
            if (idx < 0 || idx >= _materialCassette.SlotCount) return;

            EnsureSlotInitialized(idx);
            var wafer = _materialCassette.GetWafer(idx);
            wafer.Presence = present ? MaterialPresence.Exist : MaterialPresence.NotExist;
            NotifyCassetteChanged();
        }

        public void ToggleSlotPresent(int slotNumber)
        {
            if (_materialCassette == null) return;
            int idx = slotNumber - 1;
            if (idx < 0 || idx >= _materialCassette.SlotCount) return;

            EnsureSlotInitialized(idx);
            var wafer = _materialCassette.GetWafer(idx);
            wafer.Presence = wafer.Presence == MaterialPresence.Exist ? MaterialPresence.NotExist : MaterialPresence.Exist;
            NotifyCassetteChanged();
        }

        public void CreateTestCassette(int slotCount = 25)
        {
            var testCassette = new MaterialCassette
            {
                SlotCount = slotCount,
                Slots = Enumerable.Repeat<MaterialWafer>(null, slotCount).ToList()
            };
            for (int i = 0; i < slotCount; i++)
            {
                var wafer = new MaterialWafer()
                {
                    Presence = (i % 2 == 0) ? MaterialPresence.Exist : MaterialPresence.NotExist,
                    SlotIndex = i,
                    WaferId = $"TEST_WAFER_{i + 1:00}" // 테스트용 ID
                };
                testCassette.SetWafer(i, wafer);
            }
            SetMaterialCassette(testCassette);
        }
        #endregion
    }
}