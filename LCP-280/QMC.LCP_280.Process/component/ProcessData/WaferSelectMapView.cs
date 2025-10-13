using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
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
        private int _cellSize = 20;

        // 선택 관련
        private readonly HashSet<int> _selectedSlots = new HashSet<int>();
        private readonly Dictionary<int, int> _selectionOrder = new Dictionary<int, int>(); // 슬롯번호 -> 선택순서
        private int _nextSelectionNumber = 1;

        // 툴팁 관련
        private ToolTip _toolTip;
        private Timer _hoverTimer;
        private int _hoveredSlot = -1;

        // 테스트 버튼들
        //private Button btnAll;
        //private Button btnResetAll;

        //private Label lblSelectedCount;
        //private Label lblSelectedCountValue;
        //private Label lblNextOrder;
        //private Label lblNextOrderValue;
        //private ListBox listSelectedSlots;
        //private Label lblSelectedList;

        private readonly object _selectionLock = new object();

        // 렌더링 타이머 (실시간 갱신용)
        private Timer _renderTimer;
        private bool _liveRenderEnabled;

        public WaferSelectMapView()
        {
            InitializeComponent();

            // 더블 버퍼링 설정
            SetupDoubleBuffering();

            // 툴팁 초기화
            InitializeToolTip();

            // 우측 패널/버튼 초기화
            InitializeTestButtons();

            if (pWaferImage != null)
            {
                pWaferImage.Paint -= WaferImage_Paint;
                pWaferImage.Paint += WaferImage_Paint;

                // 마우스 이벤트 추가
                pWaferImage.MouseClick += WaferImage_MouseClick;
                pWaferImage.MouseMove += WaferImage_MouseMove;
                pWaferImage.MouseLeave += WaferImage_MouseLeave;

                // 사이즈 변경 시 셀 크기 재계산 + 다시 그리기
                pWaferImage.Resize += (s, e) =>
                {
                    AdjustCellSize();
                    SafeInvalidate();
                };

                // pWaferImage 패널의 더블 버퍼링 활성화
                EnablePanelDoubleBuffering(pWaferImage);
            }

            // 실시간 렌더링 타이머 설정 (기본 30 FPS)
            _renderTimer = new Timer { Interval = 33 };
            _renderTimer.Tick += (s, e) =>
            {
                if (!_liveRenderEnabled) return;
                if (!Visible) return;

                if (pWaferImage != null && pWaferImage.IsHandleCreated)
                {
                    pWaferImage.Invalidate();
                }
                else
                {
                    Invalidate();
                }
            };

            // 기본적으로 실시간 갱신 활성화
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

        // 실시간 렌더링 on/off 및 FPS 설정
        public void EnableLiveRender(bool enabled, int fps = 30)
        {
            _liveRenderEnabled = enabled;
            int interval = Math.Max(10, 1000 / Math.Max(1, fps));
            _renderTimer.Interval = interval;
            _renderTimer.Enabled = enabled;
        }

        // 더블 버퍼링 설정 메서드
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

        // Panel의 더블 버퍼링 활성화
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
            if (tableLayoutPanel1.ColumnCount < 2) return;

            // 오른쪽 패널 (30% 영역)에 버튼들 추가
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 249, 250),
                Padding = new Padding(5)
            };

            // 더블 버퍼링
            EnablePanelDoubleBuffering(rightPanel);

            //int yPos = 10;
            //int buttonHeight = 25;
            //int spacing = 30;
            //int buttonWidth = 80;

            //// 전체 선택 버튼 (현재 데이터 기준 선택만, Presence 변경 없음)
            //btnAll = new Button
            //{
            //    Text = "전체 선택",
            //    Location = new Point(5, yPos),
            //    Size = new Size(buttonWidth, buttonHeight),
            //    BackColor = Color.DarkGray,
            //    ForeColor = Color.White,
            //    FlatStyle = FlatStyle.Flat,
            //    Font = new Font("Arial", 8)
            //};

            //btnAll.Click += BtnAll_Click;
            //rightPanel.Controls.Add(btnAll);
            //yPos += spacing;

            //// 전체 초기화 버튼 (슬롯 Presence를 NotExist로 초기화)
            //btnResetAll = new Button
            //{
            //    Text = "전체 초기화",
            //    Location = new Point(5, yPos),
            //    Size = new Size(buttonWidth, buttonHeight),
            //    BackColor = Color.DarkGray,
            //    ForeColor = Color.White,
            //    FlatStyle = FlatStyle.Flat,
            //    Font = new Font("Arial", 8)
            //};
            //btnResetAll.Click += BtnResetAll_Click;
            //rightPanel.Controls.Add(btnResetAll);
            //yPos += spacing;

            //// 선택된 슬롯 수 라벨
            //lblSelectedCount = new Label
            //{
            //    Text = "선택:",
            //    Location = new Point(5, yPos),
            //    Size = new Size(35, 15),
            //    Font = new Font("Arial", 7, FontStyle.Bold)
            //};
            //rightPanel.Controls.Add(lblSelectedCount);

            //lblSelectedCountValue = new Label
            //{
            //    Text = "0",
            //    Location = new Point(45, yPos),
            //    Size = new Size(30, 15),
            //    Font = new Font("Arial", 7),
            //    ForeColor = Color.Blue
            //};
            //rightPanel.Controls.Add(lblSelectedCountValue);
            //yPos += 20;

            //// 다음 순서 라벨
            //lblNextOrder = new Label
            //{
            //    Text = "순서:",
            //    Location = new Point(5, yPos),
            //    Size = new Size(35, 15),
            //    Font = new Font("Arial", 7, FontStyle.Bold)
            //};
            //rightPanel.Controls.Add(lblNextOrder);

            //lblNextOrderValue = new Label
            //{
            //    Text = "1",
            //    Location = new Point(45, yPos),
            //    Size = new Size(30, 15),
            //    Font = new Font("Arial", 7),
            //    ForeColor = Color.Blue
            //};
            //rightPanel.Controls.Add(lblNextOrderValue);
            //yPos += 25;

            //// 선택목록 라벨
            //lblSelectedList = new Label
            //{
            //    Text = "선택목록:",
            //    Location = new Point(5, yPos),
            //    Size = new Size(buttonWidth, 15),
            //    Font = new Font("Arial", 7, FontStyle.Bold)
            //};
            //rightPanel.Controls.Add(lblSelectedList);
            //yPos += 15;

            //// 선택목록 리스트박스
            //listSelectedSlots = new ListBox
            //{
            //    Location = new Point(5, yPos),
            //    Size = new Size(buttonWidth, 60),
            //    Font = new Font("Consolas", 6),
            //    BackColor = Color.White,
            //    BorderStyle = BorderStyle.FixedSingle
            //};
            //rightPanel.Controls.Add(listSelectedSlots);

            //// 테이블 레이아웃의 두 번째 열에 추가
            //tableLayoutPanel1.Controls.Add(rightPanel, 1, 0);
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
            if (_materialCassette == null) return;

            ClearSelection();

            // 현재 카세트 상태 기준으로 전체 선택
            for (int i = 0; i < _materialCassette.SlotCount; i++)
            {
                var wafer = _materialCassette.GetWafer(i); // 0-based
                var state = GetSlotDisplayState(wafer);
                SelectSlot(i + 1, state); // 1-based 뷰
            }
        }

        private void btn_ResetAll_Click(object sender, EventArgs e)
        {
            if (_materialCassette == null) return;

            ClearSelection();

            // 모든 슬롯을 "없음" 상태로 초기화
            for (int i = 0; i < _materialCassette.SlotCount; i++)
            {
                EnsureSlotInitialized(i);
                var wafer = _materialCassette.GetWafer(i);
                wafer.Presence = MaterialPresence.NotExist;
                wafer.ProcessSatate = MaterialProcessSatate.Unknown;
            }

            NotifyCassetteChanged();
        }
        #endregion

        #region Cassette 바인딩/갱신

        /// <summary>
        /// MaterialCassette 데이터를 설정합니다.
        /// </summary>
        public void SetMaterialCassette(MaterialCassette materialCassette)
        {
            if (materialCassette == null) 
                throw new ArgumentNullException(nameof(materialCassette));

            _materialCassette = materialCassette;

            // Slots 리스트와 SlotCount 불일치 방어
            if (_materialCassette.Slots == null || _materialCassette.Slots.Count != _materialCassette.SlotCount)
            {
                _materialCassette.Slots = Enumerable.Repeat<MaterialWafer>(null, _materialCassette.SlotCount).ToList();
            }

            // 기존 선택 상태 초기화
            ClearSelection();

            AdjustCellSize();
            SafeInvalidate();
        }

        public void NotifyCassetteChanged()
        {
            // 외부에서 내부 슬롯 상태가 바뀐 후 호출
            AdjustCellSize();
            SafeInvalidate();
        }

        public void RefreshMapImmediate()
        {
            if (InvokeRequired)
            {
                try { BeginInvoke((Action)RefreshMapImmediate); } catch { }
                return;
            }
            AdjustCellSize();
            if (pWaferImage != null)
            {
                pWaferImage.Invalidate();
                pWaferImage.Update();
            }
            else
            {
                Invalidate();
                Update();
            }
        }

        /// <summary>슬롯 리스트가 null이면 NotExist 상태로 초기화</summary>
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

        /// <summary>셀이 표시될 픽셀 크기 조정</summary>
        private void AdjustCellSize()
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0)
                return;
            int h = (pWaferImage?.ClientSize.Height ?? ClientSize.Height);
            if (h <= 0)
                return;

            // groupBox 상단 텍스트 영역 유사 마진 적용(=14)
            _cellSize = Math.Max(1, (h - 14 - 2) / _materialCassette.SlotCount);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustCellSize();
            SafeInvalidate();
        }

        private void SafeInvalidate()
        {
            if (!IsHandleCreated)
                return;

            if (InvokeRequired)
            {
                try { BeginInvoke((Action)SafeInvalidate); } catch { }
                return;
            }

            if (pWaferImage != null)
            {
                pWaferImage.Invalidate();
                pWaferImage.Refresh();
            }
            else
            {
                Invalidate();
                Refresh();
            }
        }
        #endregion

        #region 마우스 이벤트 처리
        private void WaferImage_MouseClick(object sender, MouseEventArgs e)
        {
            if (_materialCassette == null) return;

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
                    _toolTip.Hide(pWaferImage);
                }
            }
        }

        private void WaferImage_MouseLeave(object sender, EventArgs e)
        {
            _hoveredSlot = -1;
            _toolTip.Hide(pWaferImage);
            _hoverTimer.Stop();
        }

        /// <summary>마우스 위치에서 슬롯 번호 계산 (아래부터 1번)</summary>
        private int GetSlotNumberAtPosition(Point mousePos)
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0) return -1;

            var bounds = pWaferImage?.ClientRectangle ?? ClientRectangle;
            int topMargin = 14;
            var drawRect = new Rectangle(bounds.X + 2, bounds.Y + topMargin, bounds.Width - 4, bounds.Height - topMargin - 2);

            if (!drawRect.Contains(mousePos)) return -1;

            // Y 좌표를 기반으로 슬롯 번호 계산 (아래부터 1번)
            int relativeY = drawRect.Bottom - mousePos.Y;
            int slotIndex = relativeY / _cellSize;

            if (slotIndex >= 0 && slotIndex < _materialCassette.SlotCount)
            {
                return slotIndex + 1; // 1-based numbering
            }

            return -1;
        }

        private void HandleSlotClick(int slotNumber, Point clickPosition)
        {
            if (_materialCassette == null) return;

            var wafer = _materialCassette.GetWafer(slotNumber - 1); // 0-based
            var state = GetSlotDisplayState(wafer);

            // 슬롯 클릭 이벤트 발생
            SlotClicked?.Invoke(this, new SlotClickedEventArgs
            {
                SlotNumber = slotNumber,
                SlotBounds = GetSlotBounds(slotNumber),
                ClickPosition = clickPosition,
                State = state
            });

            // 선택 상태 토글 (데이터는 외부 로직에서 변경)
            ToggleSlotSelection(slotNumber, state);
        }

        private void ToggleSlotSelection(int slotNumber, SlotDisplayState state)
        {
            if (_selectedSlots.Contains(slotNumber))
            {
                // 선택 해제
                DeselectSlot(slotNumber, state);
            }
            else
            {
                // 선택
                SelectSlot(slotNumber, state);
            }
        }

        private void SelectSlot(int slotNumber, SlotDisplayState state)
        {
            lock (_selectionLock)
            {
                if (_selectedSlots.Contains(slotNumber)) return;
                _selectedSlots.Add(slotNumber);
                _selectionOrder[slotNumber] = _nextSelectionNumber;
                _nextSelectionNumber++;
            }

            SafeInvalidate();
            RunOnUI(UpdateUI);

            // 선택 변경 이벤트 발생
            SlotSelectionChanged?.Invoke(this, new SlotSelectionChangedEventArgs
            {
                SlotNumber = slotNumber,
                IsSelected = true,
                SelectionOrder = GetSlotSelectionOrder(slotNumber),
                State = state
            });
        }

        private void DeselectSlot(int slotNumber, SlotDisplayState state)
        {
            int currentOrder;
            lock (_selectionLock)
            {
                if (!_selectionOrder.ContainsKey(slotNumber)) return;

                currentOrder = _selectionOrder[slotNumber];
                _selectedSlots.Remove(slotNumber);
                _selectionOrder.Remove(slotNumber);

                var toUpdate = _selectionOrder.Where(kvp => kvp.Value > currentOrder).ToList();
                foreach (var kvp in toUpdate)
                    _selectionOrder[kvp.Key] = kvp.Value - 1;

                _nextSelectionNumber = _selectionOrder.Count > 0 ? _selectionOrder.Values.Max() + 1 : 1;
            }

            SafeInvalidate();
            RunOnUI(UpdateUI);

            // 선택 변경 이벤트 발생
            SlotSelectionChanged?.Invoke(this, new SlotSelectionChangedEventArgs
            {
                SlotNumber = slotNumber,
                IsSelected = false,
                SelectionOrder = 0,
                State = state
            });
        }

        private Rectangle GetSlotBounds(int slotNumber)
        {
            if (_materialCassette == null || slotNumber <= 0 || slotNumber > _materialCassette.SlotCount)
                return Rectangle.Empty;

            var bounds = pWaferImage?.ClientRectangle ?? ClientRectangle;
            int topMargin = 14;
            var drawRect = new Rectangle(bounds.X + 2, bounds.Y + topMargin, bounds.Width - 4, bounds.Height - topMargin - 2);

            int slotIndex = slotNumber - 1; // 0-based
            int y = drawRect.Bottom - (slotIndex + 1) * _cellSize;

            return new Rectangle(drawRect.Left, y, drawRect.Width, _cellSize);
        }
        #endregion

        #region 툴팁
        private void HoverTimer_Tick(object sender, EventArgs e)
        {
            _hoverTimer.Stop();

            if (_hoveredSlot > 0 && _materialCassette != null)
            {
                var wafer = _materialCassette.GetWafer(_hoveredSlot - 1);
                SlotDisplayState state = GetSlotDisplayState(wafer);

                string tooltipText = $"Slot {_hoveredSlot}\n";
                //tooltipText += $"State: {(state == SlotDisplayState.Present ? "있음" : "없음")}\n";
                if (_selectedSlots.Contains(_hoveredSlot))
                {
                    lock (_selectionLock)
                    {
                        if (_selectionOrder.ContainsKey(_hoveredSlot))
                            tooltipText += $"Selection Order: {_selectionOrder[_hoveredSlot]}\n";
                    }
                }
                else
                {
                    tooltipText += "Selection Order: ---\n";
                }

                if(wafer != null)
                {
                    tooltipText += $"Status: {(wafer.ProcessSatate.ToString())}\n";
                }
                else
                {
                    tooltipText += $"Status: ---\n";
                }

                    Point mousePos = pWaferImage?.PointToClient(Cursor.Position) ?? PointToClient(Cursor.Position);
                _toolTip.Show(tooltipText, this, mousePos.X + 10, mousePos.Y - 10);
            }
        }
        #endregion

        #region 그리기
        private void WaferImage_Paint(object sender, PaintEventArgs e)
        {
            DrawMap(e.Graphics, ((Control)sender).ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (pWaferImage == null) 
                DrawMap(e.Graphics, ClientRectangle);

        }

        private void DrawMap(Graphics g, Rectangle bounds)
        {
            int topMargin = 14;
            var drawRect = new Rectangle(bounds.X + 2, bounds.Y + topMargin, bounds.Width - 4, bounds.Height - topMargin - 2);
            if (drawRect.Width <= 0 || drawRect.Height <= 0)
            {
                return;
            }

            g.Clear(Color.Black);

            if (_materialCassette == null || _materialCassette.SlotCount <= 0)
            {
                DrawCenterMessage(g, drawRect, "No Data");
                return;
            }

            int total = _materialCassette.SlotCount;
            if (_cellSize <= 0) 
                return;

            int width = drawRect.Width;

            for (int i = 0; i < total; i++)
            {
                int slotNumber = i + 1;
                var wafer = _materialCassette.GetWafer(i);

                // 슬롯 상태 색상 (WaferMapView와 동일 정책)
                Color cellColor = GetSlotColor(wafer);

                // 선택된 슬롯이면 오버레이(밝은 초록)
                if (_selectedSlots.Contains(slotNumber))
                {
                    cellColor = Color.LimeGreen;
                }

                int y = drawRect.Bottom - (i + 1) * _cellSize;
                var rect = new Rectangle(drawRect.Left, y, width, _cellSize);

                // 슬롯 배경
                using (var brush = new SolidBrush(cellColor))
                {
                    g.FillRectangle(brush, rect);
                }

                // 테두리
                using (var pen = new Pen(Color.Black))
                {
                    g.DrawRectangle(pen, rect);
                }

                // 선택 순서 표시
                if (_selectedSlots.Contains(slotNumber) && _selectionOrder.ContainsKey(slotNumber))
                {
                    DrawSelectionOrder(g, rect, _selectionOrder[slotNumber]);
                }

                // 슬롯 번호 표시 (작게)
                DrawSlotNumber(g, rect, slotNumber);
            }
        }

        private Color GetSlotColor(MaterialWafer wafer)
        {
            if (wafer == null)
                return Color.Gray; // 없음 - 회색

            // Presence 기준 기본 색상
            Color cellColor;
            switch (wafer.Presence)
            {
                case MaterialPresence.Exist: cellColor = Color.LimeGreen; break;
                case MaterialPresence.NotExist: cellColor = Color.Gray; break;
                case MaterialPresence.Unknown:
                default: cellColor = Color.Yellow; break;
            }

            // ProcessSatate가 명확하면 그 색으로 override (WaferMapView와 동일)
            switch (wafer.ProcessSatate)
            {
                case MaterialProcessSatate.Ready: cellColor = Color.Blue; break;
                case MaterialProcessSatate.Processing: cellColor = Color.Orange; break;
                case MaterialProcessSatate.Completed: cellColor = Color.Green; break;
                case MaterialProcessSatate.Unknown:
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
            // 원형 배경
            int circleSize = Math.Min(rect.Width, rect.Height) / 3;
            int circleX = rect.Right - circleSize - 2;
            int circleY = rect.Top + 2;
            var circleRect = new Rectangle(circleX, circleY, circleSize, circleSize);

            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, circleRect);
            }

            using (var pen = new Pen(Color.Black))
            {
                g.DrawEllipse(pen, circleRect);
            }

            // 순서 번호 텍스트
            using (var font = new Font("Arial", circleSize * 0.4f, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Black))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString(order.ToString(), font, brush, circleRect, sf);
            }
        }

        private void DrawSlotNumber(Graphics g, Rectangle rect, int slotNumber)
        {
            using (var font = new Font("Arial", Math.Max(6, _cellSize * 0.3f), FontStyle.Regular))
            using (var brush = new SolidBrush(Color.Black))
            {
                g.DrawString(slotNumber.ToString(), font, brush, rect.Left + 2, rect.Top + 2);
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

        #region UI 업데이트
        // UI 스레드 실행 헬퍼
        private void RunOnUI(Action action)
        {
            if (action == null) return;
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(action); } catch { /* 컨트롤 dispose 중 */ }
            }
            else
            {
                action();
            }
        }

        private void UpdateUI()
        {
            if (!IsHandleCreated || IsDisposed) return;
            if (InvokeRequired)
            {
                RunOnUI(UpdateUI);
                return;
            }

            if (lbl_SelectedCountValue == null || lbl_NextOrderValue == null || list_SelectedSlots == null || list_SelectedSlots.IsDisposed)
                return;

            List<int> selectedSlots;
            List<int> selectedInOrder;
            int nextSel;

            lock (_selectionLock)
            {
                selectedSlots = _selectedSlots.ToList();
                selectedInOrder = _selectionOrder.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
                nextSel = _nextSelectionNumber;
            }

            string countText = selectedSlots.Count.ToString();
            if (lbl_SelectedCountValue.Text != countText)
                lbl_SelectedCountValue.Text = countText;

            string nextOrderText = (_materialCassette != null && nextSel > _materialCassette.SlotCount) ? "1" : nextSel.ToString();
            if (lbl_NextOrderValue.Text != nextOrderText)
                lbl_NextOrderValue.Text = nextOrderText;

            list_SelectedSlots.BeginUpdate();
            try
            {
                list_SelectedSlots.Items.Clear();
                if (selectedInOrder.Count == 0)
                {
                    list_SelectedSlots.Items.Add("없음");
                }
                else
                {
                    foreach (var slotNumber in selectedInOrder)
                    {
                        int order;
                        lock (_selectionLock)
                            order = _selectionOrder.ContainsKey(slotNumber) ? _selectionOrder[slotNumber] : 0;
                        list_SelectedSlots.Items.Add($"{order}: Slot{slotNumber}");
                    }
                }
            }
            finally
            {
                list_SelectedSlots.EndUpdate();
            }
        }
        #endregion

        #region 공개 메서드 (외부 제어 API)
        /// <summary>모든 선택 해제</summary>
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

        /// <summary>선택된 슬롯 번호들 반환</summary>
        public List<int> GetSelectedSlots()
        {
            lock (_selectionLock) return _selectedSlots.ToList();
        }

        /// <summary>선택 순서대로 슬롯 번호들 반환</summary>
        public List<int> GetSelectedSlotsInOrder()
        {
            lock (_selectionLock) return _selectionOrder.OrderBy(k => k.Value).Select(k => k.Key).ToList();
        }

        /// <summary>특정 슬롯의 선택 순서 반환</summary>
        public int GetSlotSelectionOrder(int slotNumber)
        {
            lock (_selectionLock) return _selectionOrder.ContainsKey(slotNumber) ? _selectionOrder[slotNumber] : 0;
        }

        /// <summary>선택된 슬롯 개수 반환</summary>
        public int GetSelectedCount()
        {
            lock (_selectionLock) return _selectedSlots.Count;
        }

        /// <summary>지정 슬롯 Presence 설정 (1-based)</summary>
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

        /// <summary>지정 슬롯 Presence 토글 (1-based)</summary>
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

        /// <summary>테스트용 카세트 생성</summary>
        public void CreateTestCassette(int slotCount = 25)
        {
            var testCassette = new MaterialCassette
            {
                SlotCount = slotCount,
                Slots = Enumerable.Repeat<MaterialWafer>(null, slotCount).ToList()
            };

            // 교대로 있음/없음
            for (int i = 0; i < slotCount; i++)
            {
                var wafer = new MaterialWafer()
                {
                    Presence = (i % 2 == 0) ? MaterialPresence.Exist : MaterialPresence.NotExist,
                    SlotIndex = i
                };
                testCassette.SetWafer(i, wafer);
            }

            SetMaterialCassette(testCassette);
        }
        #endregion


    }
}