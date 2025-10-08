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
        // 슬롯 상태 (기존 MaterialCassette 기반)
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
        private HashSet<int> _selectedSlots = new HashSet<int>();
        private Dictionary<int, int> _selectionOrder = new Dictionary<int, int>(); // 슬롯번호 -> 선택순서
        private int _nextSelectionNumber = 1;

        // 툴팁 관련
        private ToolTip _toolTip;
        private Timer _hoverTimer;
        private int _hoveredSlot = -1;

        // 테스트 버튼들
        private Button btnAll;
        private Button btnResetAll;

        private Label lblSelectedCount;
        private Label lblSelectedCountValue;
        private Label lblNextOrder;
        private Label lblNextOrderValue;
        private ListBox listSelectedSlots;
        private Label lblSelectedList;

        public WaferSelectMapView()
        {
            InitializeComponent();

            // 더블 버퍼링 설정 강화
            SetupDoubleBuffering();

            // 툴팁 초기화
            InitializeToolTip();

            // 테스트 버튼들 초기화
            InitializeTestButtons();

            if (pWaferImage != null)
            {
                pWaferImage.Paint -= WaferImage_Paint;
                pWaferImage.Paint += WaferImage_Paint;

                // 마우스 이벤트 추가
                pWaferImage.MouseClick += WaferImage_MouseClick;
                pWaferImage.MouseMove += WaferImage_MouseMove;
                pWaferImage.MouseLeave += WaferImage_MouseLeave;

                // pWaferImage 패널의 더블 버퍼링 활성화
                EnablePanelDoubleBuffering(pWaferImage);
            }
        }

        // 더블 버퍼링 설정 메서드
        private void SetupDoubleBuffering()
        {
            // UserControl 자체의 더블 버퍼링
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

            // 우측 패널도 더블 버퍼링 활성화
            EnablePanelDoubleBuffering(rightPanel);

            int yPos = 10;
            int buttonHeight = 25;
            int spacing = 30;
            int buttonWidth = 80;

            // 전체 선택 버튼
            btnAll = new Button
            {
                Text = "전체 선택",
                Location = new Point(5, yPos),
                Size = new Size(buttonWidth, buttonHeight),
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 8)
            };
            btnAll.Click += BtnAll_Click;
            rightPanel.Controls.Add(btnAll);
            yPos += spacing;

            // 전체 초기화 버튼
            btnResetAll = new Button
            {
                Text = "전체 초기화",
                Location = new Point(5, yPos),
                Size = new Size(buttonWidth, buttonHeight),
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 8)
            };
            btnResetAll.Click += BtnResetAll_Click;
            rightPanel.Controls.Add(btnResetAll);
            yPos += spacing;

            // 선택된 슬롯 수 라벨
            lblSelectedCount = new Label
            {
                Text = "선택:",
                Location = new Point(5, yPos),
                Size = new Size(35, 15),
                Font = new Font("Arial", 7, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblSelectedCount);

            lblSelectedCountValue = new Label
            {
                Text = "0",
                Location = new Point(45, yPos),
                Size = new Size(30, 15),
                Font = new Font("Arial", 7),
                ForeColor = Color.Blue
            };
            rightPanel.Controls.Add(lblSelectedCountValue);
            yPos += 20;

            // 다음 순서 라벨
            lblNextOrder = new Label
            {
                Text = "순서:",
                Location = new Point(5, yPos),
                Size = new Size(35, 15),
                Font = new Font("Arial", 7, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblNextOrder);

            lblNextOrderValue = new Label
            {
                Text = "1",
                Location = new Point(45, yPos),
                Size = new Size(30, 15),
                Font = new Font("Arial", 7),
                ForeColor = Color.Blue
            };
            rightPanel.Controls.Add(lblNextOrderValue);
            yPos += 25;

            // 선택된 슬롯 목록 라벨
            lblSelectedList = new Label
            {
                Text = "선택목록:",
                Location = new Point(5, yPos),
                Size = new Size(buttonWidth, 15),
                Font = new Font("Arial", 7, FontStyle.Bold)
            };
            rightPanel.Controls.Add(lblSelectedList);
            yPos += 15;

            // 선택된 슬롯 목록 리스트박스
            listSelectedSlots = new ListBox
            {
                Location = new Point(5, yPos),
                Size = new Size(buttonWidth, 60),
                Font = new Font("Consolas", 6),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            rightPanel.Controls.Add(listSelectedSlots);

            // 테이블 레이아웃의 두 번째 열에 추가
            tableLayoutPanel1.Controls.Add(rightPanel, 1, 0);
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
        private void BtnAll_Click(object sender, EventArgs e)
        {
            if (_materialCassette == null) return;

            ClearSelection();

            // 모든 슬롯을 "있는" 상태로 초기화
            for (int i = 0; i < _materialCassette.SlotCount; i++)
            {
                var wafer = _materialCassette.GetWafer(i + 1);
                SlotDisplayState state = GetSlotDisplayState(wafer);
                SelectSlot(i + 1, state);
            }
        }

        private void BtnResetAll_Click(object sender, EventArgs e)
        {
            if (_materialCassette == null) return;

            ClearSelection();

            // 모든 슬롯을 "없음" 상태로 초기화
            for (int i = 0; i < _materialCassette.SlotCount + 1; i++)
            {
                var wafer = _materialCassette.GetWafer(i);
                if (wafer != null)
                {
                    wafer.Presence = MaterialPresence.NotExist;
                }
            }

            NotifyCassetteChanged();
            Console.WriteLine("전체 초기화 완료");
        }

        #endregion

        #region 기존 메서드 유지

        /// <summary>
        /// MaterialCassette 데이터를 설정합니다.
        /// </summary>
        /// <param name="materialCassette">MaterialCassette 객체</param>
        public void SetMaterialCassette(MaterialCassette materialCassette)
        {
            if (materialCassette == null) throw new ArgumentNullException(nameof(materialCassette));
            _materialCassette = materialCassette;

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

        /// <summary>
        /// 셀 크기를 조정합니다.
        /// </summary>
        private void AdjustCellSize()
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0) return;
            int h = (pWaferImage?.ClientSize.Height ?? ClientSize.Height);
            if (h <= 0) return;
            _cellSize = Math.Max(1, (h - 14 - 2) / _materialCassette.SlotCount); // topMargin 고려
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustCellSize();
            SafeInvalidate();
        }

        private void SafeInvalidate()
        {
            if (!IsHandleCreated) return;
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

            
        }

        #endregion

        #region 마우스 이벤트 처리

        private void WaferImage_MouseClick(object sender, MouseEventArgs e)
        {
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
                // 호버 시 Invalidate 제거 - 툴팁만 표시
            }
        }

        private void WaferImage_MouseLeave(object sender, EventArgs e)
        {
            _hoveredSlot = -1;
            _toolTip.Hide(pWaferImage);
            _hoverTimer.Stop();
            // Invalidate 제거
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

            var wafer = _materialCassette.GetWafer(slotNumber - 1);
            SlotDisplayState state = GetSlotDisplayState(wafer);

            // 슬롯 클릭 이벤트 발생
            SlotClicked?.Invoke(this, new SlotClickedEventArgs
            {
                SlotNumber = slotNumber,
                SlotBounds = GetSlotBounds(slotNumber),
                ClickPosition = clickPosition,
                State = state
            });

            // 선택 상태 토글
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
            _selectedSlots.Add(slotNumber);
            _selectionOrder[slotNumber] = _nextSelectionNumber;
            _nextSelectionNumber++;

            SafeInvalidate();
            UpdateUI();

            // 선택 변경 이벤트 발생
            SlotSelectionChanged?.Invoke(this, new SlotSelectionChangedEventArgs
            {
                SlotNumber = slotNumber,
                IsSelected = true,
                SelectionOrder = _selectionOrder[slotNumber],
                State = state
            });
        }

        private void DeselectSlot(int slotNumber, SlotDisplayState state)
        {
            if (!_selectionOrder.ContainsKey(slotNumber)) return;

            int currentOrder = _selectionOrder[slotNumber];
            _selectedSlots.Remove(slotNumber);
            _selectionOrder.Remove(slotNumber);

            // 순서 재정렬 (선택 해제된 것보다 뒤의 순서들을 앞으로 당김)
            var toUpdate = _selectionOrder.Where(kvp => kvp.Value > currentOrder).ToList();
            foreach (var kvp in toUpdate)
            {
                _selectionOrder[kvp.Key] = kvp.Value - 1;
            }

            // 다음 선택 번호 업데이트
            _nextSelectionNumber = _selectionOrder.Count > 0 ? _selectionOrder.Values.Max() + 1 : 1;

            SafeInvalidate();
            UpdateUI();

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
                tooltipText += $"State: {(state == SlotDisplayState.Present ? "있음" : "없음")}\n";

                if (_selectedSlots.Contains(_hoveredSlot))
                {
                    tooltipText += $"Selection Order: {_selectionOrder[_hoveredSlot]}";
                }
                else
                {
                    tooltipText += "Not Selected";
                }

                Point mousePos = pWaferImage?.PointToClient(Cursor.Position) ?? PointToClient(Cursor.Position);
                _toolTip.Show(tooltipText, this, mousePos.X + 10, mousePos.Y - 10);
            }
        }

        #endregion

        #region 그리기 (기존 DrawMap 메서드 수정)

        private void WaferImage_Paint(object sender, PaintEventArgs e)
        {
            DrawMap(e.Graphics, ((Control)sender).ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (pWaferImage == null) DrawMap(e.Graphics, ClientRectangle);
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
            if (_cellSize <= 0) return;

            int width = drawRect.Width;

            for (int i = 0; i < total; i++)
            {
                int slotNumber = i + 1;
                var wafer = _materialCassette.GetWafer(i);

                // 슬롯 상태에 따른 기본 색상
                Color cellColor = GetSlotColor(wafer);

                // 선택된 슬롯이면 밝은 초록색으로 변경
                if (_selectedSlots.Contains(slotNumber))
                {
                    cellColor = Color.LimeGreen;
                }

                int y = drawRect.Bottom - (i + 1) * _cellSize;
                var rect = new Rectangle(drawRect.Left, y, width, _cellSize);

                // 슬롯 배경 그리기
                using (var brush = new SolidBrush(cellColor))
                {
                    g.FillRectangle(brush, rect);
                }

                // 테두리 그리기
                using (var pen = new Pen(Color.Black))
                {
                    g.DrawRectangle(pen, rect);
                }

                // 선택된 슬롯에 순서 번호 표시
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

            // 간단한 두 가지 상태만 처리
            switch (wafer.Presence)
            {
                case MaterialPresence.Exist:
                    return Color.Green; // 있음 - 초록
                case MaterialPresence.NotExist:
                default:
                    return Color.Gray; // 없음 - 회색
            }
        }

        private SlotDisplayState GetSlotDisplayState(MaterialWafer wafer)
        {
            if (wafer == null) return SlotDisplayState.Empty;

            return wafer.Presence == MaterialPresence.Exist ?
                   SlotDisplayState.Present : SlotDisplayState.Empty;
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
            using (var brush = new SolidBrush(Color.White))
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

        private void UpdateUI()
        {
            if (lblSelectedCountValue == null || lblNextOrderValue == null || listSelectedSlots == null)
                return;

            var selectedSlots = GetSelectedSlots();
            var selectedInOrder = GetSelectedSlotsInOrder();

            // 값이 실제로 변경될 때만 업데이트
            string countText = selectedSlots.Count.ToString();
            if (lblSelectedCountValue.Text != countText)
            {
                lblSelectedCountValue.Text = countText;
            }

            string nextOrderText = _nextSelectionNumber > _materialCassette.SlotCount ? "1" : _nextSelectionNumber.ToString();
            if (lblNextOrderValue.Text != nextOrderText)
            {
                lblNextOrderValue.Text = nextOrderText;
            }

            // 선택된 슬롯 목록 업데이트
            listSelectedSlots.Items.Clear();
            if (selectedInOrder.Count == 0)
            {
                listSelectedSlots.Items.Add("없음");
            }
            else
            {
                for (int i = 0; i < selectedInOrder.Count; i++)
                {
                    int slotNumber = selectedInOrder[i];
                    int order = GetSlotSelectionOrder(slotNumber);
                    listSelectedSlots.Items.Add($"{order}: Slot{slotNumber}");
                }
            }
        }

        #endregion

        #region 공개 메서드

        /// <summary>모든 선택 해제</summary>
        public void ClearSelection()
        {
            _selectedSlots.Clear();
            _selectionOrder.Clear();
            _nextSelectionNumber = 1;
            SafeInvalidate();
            UpdateUI();
        }

        /// <summary>선택된 슬롯 번호들 반환</summary>
        public List<int> GetSelectedSlots()
        {
            return _selectedSlots.ToList();
        }

        /// <summary>선택 순서대로 슬롯 번호들 반환</summary>
        public List<int> GetSelectedSlotsInOrder()
        {
            return _selectionOrder.OrderBy(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        /// <summary>특정 슬롯의 선택 순서 반환</summary>
        public int GetSlotSelectionOrder(int slotNumber)
        {
            return _selectionOrder.ContainsKey(slotNumber) ? _selectionOrder[slotNumber] : 0;
        }

        /// <summary>선택된 슬롯 개수 반환</summary>
        public int GetSelectedCount()
        {
            return _selectedSlots.Count;
        }

        /// <summary>테스트용 카세트 생성</summary>
        public void CreateTestCassette(int slotCount = 20)
        {
            var testCassette = new MaterialCassette(); // 매개변수 없는 생성자 사용

            // 슬롯 개수 설정 (MaterialCassette에 SlotCount 설정 메서드가 있다면)
            // testCassette.SetSlotCount(slotCount);

            // 테스트용 데이터 생성 - 교대로 있음/없음
            for (int i = 0; i < slotCount; i++)
            {
                var wafer = new MaterialWafer()
                {
                    Presence = (i % 2 == 0) ? MaterialPresence.Exist : MaterialPresence.NotExist
                };
                // testCassette.SetWafer(i, wafer);
            }

            SetMaterialCassette(testCassette);
        }

        #endregion
    }
}