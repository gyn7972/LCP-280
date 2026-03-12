using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static QMC.Common.Material;

namespace QMC.LCP_280.Process.Component
{
    public partial class WaferMapView : UserControl
    {
        private MaterialCassette _materialCassette;
        private int _cellSize = 20;

        #region 편집 기능 추가 (OutputMagazine용)

        // 편집 모드
        private bool _isEditable = false;
        private int _hoveredSlotIndex = -1;

        // 툴팁
        private ToolTip _toolTip;

        // 렌더링 타이머 (깜빡임 방지)
        private Timer _renderTimer;
        private bool _needsRedraw = false;

        /// <summary>
        /// 편집 모드 활성화 여부
        /// OutputMagazine에서 사용자가 슬롯 상태를 직접 설정할 때 사용
        /// </summary>
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value;
                Cursor = _isEditable ? Cursors.Hand : Cursors.Default;
                SafeInvalidate();
            }
        }

        /// <summary>
        /// 슬롯 상태 변경 이벤트
        /// </summary>
        public event EventHandler<SlotChangedEventArgs> SlotChanged;

        /// <summary>
        /// 슬롯 변경 이벤트 인자
        /// </summary>
        public class SlotChangedEventArgs : EventArgs
        {
            public int SlotIndex { get; set; }
            public MaterialPresence NewPresence { get; set; }
            public MaterialWafer Wafer { get; set; }
        }

        #endregion

        #region 슬롯 위치 이동 기능 추가

        /// <summary>
        /// 슬롯 위치 이동 요청 이벤트
        /// </summary>
        public event EventHandler<SlotMoveRequestEventArgs> SlotMoveRequested;

        /// <summary>
        /// 슬롯 이동 요청 이벤트 인자
        /// </summary>
        public class SlotMoveRequestEventArgs : EventArgs
        {
            /// <summary>
            /// 슬롯 인덱스 (0-based)
            /// </summary>
            public int SlotIndex { get; set; }

            /// <summary>
            /// 슬롯 번호 (1-based, 표시용)
            /// </summary>
            public int SlotNumber => SlotIndex + 1;

            /// <summary>
            /// 대상 Z 위치 (mm)
            /// </summary>
            public double TargetZPosition { get; set; }

            /// <summary>
            /// 대상 Y 위치 (mm) - 선택적
            /// </summary>
            public double? TargetYPosition { get; set; }

            /// <summary>
            /// 해당 슬롯의 Wafer 정보
            /// </summary>
            public MaterialWafer Wafer { get; set; }

            /// <summary>
            /// 이동 후 콜백 (선택적)
            /// </summary>
            public Action<bool> OnMoveComplete { get; set; }
        }

        /// <summary>
        /// 슬롯별 Z 위치 매핑 데이터
        /// Key: SlotIndex (0-based), Value: Z Position (mm)
        /// </summary>
        private Dictionary<int, double> _slotZPositions = new Dictionary<int, double>();

        /// <summary>
        /// 슬롯별 Y 위치 매핑 데이터 (선택적)
        /// </summary>
        private Dictionary<int, double> _slotYPositions = new Dictionary<int, double>();

        /// <summary>
        /// 슬롯 위치 이동 기능 활성화 여부
        /// </summary>
        public bool EnableSlotMovement { get; set; } = false;

        /// <summary>
        /// Output Magazine 여부 (색상 구분용)
        /// </summary>
        public bool IsOutputMagazine { get; set; } = false;

        /// <summary>
        /// 슬롯별 Z 위치 설정
        /// </summary>
        public void SetSlotZPosition(int slotIndex, double zPosition)
        {
            _slotZPositions[slotIndex] = zPosition;
        }

        /// <summary>
        /// 슬롯별 Y 위치 설정
        /// </summary>
        public void SetSlotYPosition(int slotIndex, double yPosition)
        {
            _slotYPositions[slotIndex] = yPosition;
        }

        /// <summary>
        /// 슬롯 위치 데이터 초기화 (Mapping 결과 반영)
        /// </summary>
        public void SetSlotPositions(Dictionary<int, double> zPositions, Dictionary<int, double> yPositions = null)
        {
            _slotZPositions = zPositions ?? new Dictionary<int, double>();
            _slotYPositions = yPositions ?? new Dictionary<int, double>();
            SafeInvalidate();
        }

        /// <summary>
        /// Cassette 데이터에서 슬롯 위치 자동 추출
        /// </summary>
        public void ExtractSlotPositionsFromCassette()
        {
            _slotZPositions.Clear();
            _slotYPositions.Clear();

            if (_materialCassette?.Slots == null) return;

            for (int i = 0; i < _materialCassette.Slots.Count; i++)
            {
                var wafer = _materialCassette.Slots[i];
                if (wafer != null && wafer.DetectedZPosition > 0.001)
                {
                    _slotZPositions[i] = wafer.DetectedZPosition;
                }
            }
        }

        /// <summary>
        /// 슬롯의 Z 위치 가져오기
        /// </summary>
        public double? GetSlotZPosition(int slotIndex)
        {
            if (_slotZPositions.TryGetValue(slotIndex, out double pos))
                return pos;

            // Cassette에서 직접 가져오기
            var wafer = _materialCassette?.GetWafer(slotIndex);
            if (wafer != null && wafer.DetectedZPosition > 0.001)
                return wafer.DetectedZPosition;

            return null;
        }

        /// <summary>
        /// 슬롯의 Y 위치 가져오기
        /// </summary>
        public double? GetSlotYPosition(int slotIndex)
        {
            if (_slotYPositions.TryGetValue(slotIndex, out double pos))
                return pos;
            return null;
        }

        /// <summary>
        /// 위치가 설정된 슬롯 개수
        /// </summary>
        public int GetMappedSlotCount()
        {
            return _slotZPositions.Count;
        }

        /// <summary>
        /// 모든 슬롯 위치 정보 가져오기
        /// </summary>
        public Dictionary<int, double> GetAllSlotZPositions()
        {
            return new Dictionary<int, double>(_slotZPositions);
        }

        #endregion

        public WaferMapView()
        {
            InitializeComponent();

            // ========== 더블 버퍼링 강화 (깜빡임 방지) ==========
            SetupDoubleBuffering();

            // 툴팁 초기화
            _toolTip = new ToolTip
            {
                AutoPopDelay = 3000,
                InitialDelay = 200,
                ReshowDelay = 100
            };

            // 렌더링 타이머 (30 FPS)
            _renderTimer = new Timer { Interval = 33 };
            _renderTimer.Tick += RenderTimer_Tick;
            _renderTimer.Start();

            if (groupBox != null)
            {
                // GroupBox 더블 버퍼링 적용
                EnableControlDoubleBuffering(groupBox);

                groupBox.Paint -= GroupBox_Paint;
                groupBox.Paint += GroupBox_Paint;

                // 편집 기능용 마우스 이벤트
                groupBox.MouseClick -= GroupBox_MouseClick;
                groupBox.MouseClick += GroupBox_MouseClick;
                groupBox.MouseMove -= GroupBox_MouseMove;
                groupBox.MouseMove += GroupBox_MouseMove;
                groupBox.MouseLeave -= GroupBox_MouseLeave;
                groupBox.MouseLeave += GroupBox_MouseLeave;
            }
        }

        #region 더블 버퍼링 설정 (깜빡임 방지)

        /// <summary>
        /// 더블 버퍼링 설정
        /// </summary>
        private void SetupDoubleBuffering()
        {
            this.DoubleBuffered = true;
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.Opaque,
                true);
            this.UpdateStyles();
        }

        /// <summary>
        /// 컨트롤에 더블 버퍼링 적용 (리플렉션)
        /// </summary>
        private void EnableControlDoubleBuffering(Control control)
        {
            if (control == null) return;

            try
            {
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, control, new object[] { true });
            }
            catch { }
        }

        /// <summary>
        /// 렌더링 타이머 Tick
        /// </summary>
        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            if (!_needsRedraw) return;
            if (!Visible || IsDisposed || Disposing) return;

            _needsRedraw = false;

            if (groupBox != null && groupBox.IsHandleCreated && !groupBox.IsDisposed)
            {
                groupBox.Invalidate();
            }
        }

        #endregion

        #region 편집 기능 - Mouse Events

        private void GroupBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (_materialCassette == null) return;

            int slotIndex = GetSlotIndexFromY(e.Y);
            if (slotIndex < 0 || slotIndex >= _materialCassette.SlotCount) return;

            // 좌클릭: 상태 토글 (편집 모드일 때만)
            if (e.Button == MouseButtons.Left && _isEditable)
            {
                ToggleSlotPresence(slotIndex);
            }
            // 우클릭: 컨텍스트 메뉴 (슬롯 이동 기능)
            else if (e.Button == MouseButtons.Right)
            {
                ShowSlotContextMenu(slotIndex, e.Location);
            }
        }

        /// <summary>
        /// 슬롯별 컨텍스트 메뉴 표시
        /// </summary>
        private void ShowSlotContextMenu(int slotIndex, Point location)
        {
            var contextMenu = new ContextMenuStrip();
            var wafer = _materialCassette?.GetWafer(slotIndex);

            // 슬롯 정보 헤더
            string statusText = wafer?.Presence == MaterialPresence.Exist ? "제품 있음" : "비어있음";
            var headerItem = new ToolStripMenuItem($"Slot {slotIndex + 1} - {statusText}")
            {
                Enabled = false,
                Font = new Font(contextMenu.Font, FontStyle.Bold)
            };
            contextMenu.Items.Add(headerItem);
            contextMenu.Items.Add(new ToolStripSeparator());

            // Z 위치 정보 표시
            double? zPos = GetSlotZPosition(slotIndex);
            if (zPos.HasValue)
            {
                var posInfoItem = new ToolStripMenuItem($"Z Position: {zPos.Value:F3} mm")
                {
                    Enabled = false
                };
                contextMenu.Items.Add(posInfoItem);
            }
            else
            {
                var noPosItem = new ToolStripMenuItem("위치 정보 없음 (Mapping 필요)")
                {
                    Enabled = false,
                    ForeColor = Color.Gray
                };
                contextMenu.Items.Add(noPosItem);
            }

            contextMenu.Items.Add(new ToolStripSeparator());

            // ========== 이동 메뉴 ==========
            if (EnableSlotMovement && zPos.HasValue)
            {
                // 해당 슬롯으로 이동
                var moveToSlotItem = new ToolStripMenuItem($"이 슬롯으로 이동 (Z: {zPos.Value:F3} mm)");
                moveToSlotItem.Click += (s, ev) => RequestMoveToSlot(slotIndex);
                contextMenu.Items.Add(moveToSlotItem);

                // 픽업 위치로 이동 (Offset 적용)
                var moveToPickupItem = new ToolStripMenuItem("픽업 위치로 이동 (+Offset)");
                moveToPickupItem.Click += (s, ev) => RequestMoveToSlot(slotIndex, applyOffset: true);
                contextMenu.Items.Add(moveToPickupItem);

                contextMenu.Items.Add(new ToolStripSeparator());
            }

            // ========== 편집 메뉴 (편집 모드일 때) ==========
            if (_isEditable)
            {
                var toggleItem = new ToolStripMenuItem(
                    wafer?.Presence == MaterialPresence.Exist ? "비우기" : "채우기");
                toggleItem.Click += (s, ev) => ToggleSlotPresence(slotIndex);
                contextMenu.Items.Add(toggleItem);
            }

            // 슬롯 상세 정보
            var detailItem = new ToolStripMenuItem("상세 정보...");
            detailItem.Click += (s, ev) => ShowSlotDetailDialog(slotIndex);
            contextMenu.Items.Add(detailItem);

            // 메뉴 표시
            contextMenu.Show(groupBox, location);
        }

        /// <summary>
        /// 슬롯 위치로 이동 요청
        /// </summary>
        private void RequestMoveToSlot(int slotIndex, bool applyOffset = false)
        {
            double? zPos = GetSlotZPosition(slotIndex);
            if (!zPos.HasValue)
            {
                MessageBox.Show(
                    $"Slot {slotIndex + 1}의 위치 정보가 없습니다.\nMapping을 먼저 수행하세요.",
                    "위치 정보 없음",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var wafer = _materialCassette?.GetWafer(slotIndex);

            var args = new SlotMoveRequestEventArgs
            {
                SlotIndex = slotIndex,
                TargetZPosition = zPos.Value,
                TargetYPosition = GetSlotYPosition(slotIndex),
                Wafer = wafer
            };

            // 이벤트 발생
            SlotMoveRequested?.Invoke(this, args);
        }

        /// <summary>
        /// 슬롯 상세 정보 다이얼로그
        /// </summary>
        private void ShowSlotDetailDialog(int slotIndex)
        {
            var wafer = _materialCassette?.GetWafer(slotIndex);
            double? zPos = GetSlotZPosition(slotIndex);

            string info = $"===== Slot {slotIndex + 1} 상세 정보 =====\n\n";
            info += $"Presence: {wafer?.Presence ?? MaterialPresence.Unknown}\n";
            info += $"Process State: {wafer?.ProcessSatate ?? MaterialProcessSatate.Unknown}\n";
            info += $"Carrier ID: {wafer?.CarrierId ?? "N/A"}\n";
            info += $"Wafer ID: {wafer?.WaferId ?? "N/A"}\n";
            info += $"Barcode: {wafer?.WaferId ?? "N/A"}\n";  //info += $"Barcode: {wafer?.BarcodeId ?? "N/A"}\n";
            info += $"Name: {wafer?.Name ?? "N/A"}\n";
            info += $"\n===== 위치 정보 =====\n";
            info += $"Z Position: {(zPos.HasValue ? $"{zPos.Value:F3} mm" : "N/A")}\n";
            info += $"Detected Z: {(wafer?.DetectedZPosition > 0.001 ? $"{wafer.DetectedZPosition:F3} mm" : "N/A")}\n";

            if (wafer?.ArrivedTime != null && wafer.ArrivedTime != DateTime.MinValue)
            {
                info += $"\n도착 시간: {wafer.ArrivedTime:yyyy-MM-dd HH:mm:ss}\n";
            }

            MessageBox.Show(info, $"Slot {slotIndex + 1} 정보", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GroupBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_materialCassette == null) return;

            int newHovered = GetSlotIndexFromY(e.Y);
            if (newHovered != _hoveredSlotIndex)
            {
                _hoveredSlotIndex = newHovered;

                // 편집 모드 또는 이동 기능 활성화 시 hover 효과
                if (_isEditable || EnableSlotMovement)
                {
                    SafeInvalidate();
                }

                // 툴팁 표시
                if (_hoveredSlotIndex >= 0 && _hoveredSlotIndex < _materialCassette.SlotCount)
                {
                    var wafer = _materialCassette.GetWafer(_hoveredSlotIndex);
                    string status = wafer?.Presence.ToString() ?? "Unknown";
                    string processState = wafer?.ProcessSatate.ToString() ?? "Unknown";

                    string tipText = $"Slot {_hoveredSlotIndex + 1}\nPresence: {status}\nState: {processState}";

                    // Z 위치 정보 표시
                    double? zPos = GetSlotZPosition(_hoveredSlotIndex);
                    if (zPos.HasValue)
                    {
                        tipText += $"\nZ: {zPos.Value:F3} mm";
                    }

                    if (_isEditable)
                    {
                        tipText += "\n(좌클릭: 토글, 우클릭: 메뉴)";
                    }
                    else if (EnableSlotMovement)
                    {
                        tipText += "\n(우클릭: 이동 메뉴)";
                    }

                    _toolTip.SetToolTip(groupBox, tipText);
                }
                else
                {
                    _toolTip.SetToolTip(groupBox, "");
                }
            }
        }

        private void GroupBox_MouseLeave(object sender, EventArgs e)
        {
            if (_hoveredSlotIndex != -1)
            {
                _hoveredSlotIndex = -1;
                if (_isEditable || EnableSlotMovement)
                {
                    SafeInvalidate();
                }
            }
            _toolTip.SetToolTip(groupBox, "");
        }

        /// <summary>
        /// Y 좌표에서 슬롯 인덱스 계산
        /// </summary>
        private int GetSlotIndexFromY(int y)
        {
            if (_materialCassette == null || _cellSize <= 0) return -1;

            int topMargin = 14;
            var bounds = groupBox?.ClientRectangle ?? ClientRectangle;
            var drawRect = new Rectangle(bounds.X + 2, bounds.Y + topMargin, bounds.Width - 4, bounds.Height - topMargin - 2);

            // 슬롯은 아래에서 위로 그려짐 (i=0이 맨 아래)
            int relativeY = drawRect.Bottom - y;
            if (relativeY < 0) return -1;

            int slotIndex = relativeY / _cellSize;
            if (slotIndex >= _materialCassette.SlotCount) return -1;

            return slotIndex;
        }

        /// <summary>
        /// 슬롯 상태 토글 (NotExist ↔ Exist)
        /// </summary>
        private void ToggleSlotPresence(int slotIndex)
        {
            if (_materialCassette == null) return;
            if (slotIndex < 0 || slotIndex >= _materialCassette.SlotCount) return;

            // Slots 리스트 확인/초기화
            EnsureSlotInitialized(slotIndex);

            var wafer = _materialCassette.GetWafer(slotIndex);
            if (wafer == null) return;

            // 상태 토글
            if (wafer.Presence == MaterialPresence.Exist)
            {
                wafer.Presence = MaterialPresence.NotExist;
                wafer.ProcessSatate = MaterialProcessSatate.Unknown;
            }
            else
            {
                wafer.Presence = MaterialPresence.Exist;
                wafer.ProcessSatate = MaterialProcessSatate.Ready;
                wafer.ArrivedTime = DateTime.Now;
            }

            _materialCassette.SetWafer(slotIndex, wafer);

            // 이벤트 발생
            SlotChanged?.Invoke(this, new SlotChangedEventArgs
            {
                SlotIndex = slotIndex,
                NewPresence = wafer.Presence,
                Wafer = wafer
            });

            SafeInvalidate();
        }

        /// <summary>
        /// 슬롯이 초기화되어 있는지 확인하고 없으면 생성
        /// </summary>
        private void EnsureSlotInitialized(int index)
        {
            if (_materialCassette == null) return;
            if (index < 0 || index >= _materialCassette.SlotCount) return;

            // Slots 리스트가 없으면 생성
            if (_materialCassette.Slots == null)
            {
                _materialCassette.Slots = new System.Collections.Generic.List<MaterialWafer>();
                for (int i = 0; i < _materialCassette.SlotCount; i++)
                {
                    _materialCassette.Slots.Add(null);
                }
            }

            // 해당 슬롯이 null이면 생성
            var wafer = _materialCassette.GetWafer(index);
            if (wafer == null)
            {
                wafer = new MaterialWafer
                {
                    Presence = MaterialPresence.NotExist,
                    ProcessSatate = MaterialProcessSatate.Unknown,
                    CarrierId = _materialCassette.CarrierId,
                    SlotIndex = index,
                    Name = $"Slot_{index:D2}"
                };
                _materialCassette.SetWafer(index, wafer);
            }
        }

        #endregion

        #region 공개 API (외부 제어)

        /// <summary>
        /// 모든 슬롯을 Exist 상태로 설정
        /// </summary>
        public void SetAllSlotsExist()
        {
            if (_materialCassette == null) return;

            for (int i = 0; i < _materialCassette.SlotCount; i++)
            {
                EnsureSlotInitialized(i);
                var wafer = _materialCassette.GetWafer(i);
                wafer.Presence = MaterialPresence.Exist;
                wafer.ProcessSatate = MaterialProcessSatate.Ready;
            }

            SafeInvalidate();
        }

        /// <summary>
        /// 모든 슬롯을 NotExist 상태로 설정
        /// </summary>
        public void SetAllSlotsEmpty()
        {
            if (_materialCassette == null) return;

            for (int i = 0; i < _materialCassette.SlotCount; i++)
            {
                EnsureSlotInitialized(i);
                var wafer = _materialCassette.GetWafer(i);
                wafer.Presence = MaterialPresence.NotExist;
                wafer.ProcessSatate = MaterialProcessSatate.Unknown;
            }

            SafeInvalidate();
        }

        /// <summary>
        /// 특정 슬롯의 Presence 설정 (0-based index)
        /// </summary>
        public void SetSlotPresence(int slotIndex, bool exist)
        {
            if (_materialCassette == null) return;
            if (slotIndex < 0 || slotIndex >= _materialCassette.SlotCount) return;

            EnsureSlotInitialized(slotIndex);
            var wafer = _materialCassette.GetWafer(slotIndex);
            wafer.Presence = exist ? MaterialPresence.Exist : MaterialPresence.NotExist;
            wafer.ProcessSatate = exist ? MaterialProcessSatate.Ready : MaterialProcessSatate.Unknown;

            SafeInvalidate();
        }

        /// <summary>
        /// Exist 상태인 슬롯 개수 반환
        /// </summary>
        public int GetExistSlotCount()
        {
            if (_materialCassette == null || _materialCassette.Slots == null) return 0;

            int count = 0;
            for (int i = 0; i < _materialCassette.SlotCount; i++)
            {
                var wafer = _materialCassette.GetWafer(i);
                if (wafer?.Presence == MaterialPresence.Exist)
                    count++;
            }
            return count;
        }

        #endregion

        /// <summary>
        /// MaterialCassette 데이터를 설정합니다.
        /// </summary>
        /// <param name="materialCassette">MaterialCassette 객체</param>
        public void SetMaterialCassette(MaterialCassette materialCassette)
        {
            if (materialCassette == null)
                throw new ArgumentNullException(nameof(materialCassette));

            _materialCassette = materialCassette;

            // Cassette에서 위치 정보 자동 추출
            ExtractSlotPositionsFromCassette();

            AdjustCellSize();
            SafeInvalidate();
        }

        public void NotifyCassetteChanged()
        {
            // 외부에서 내부 슬롯 상태가 바뀐 후 호출
            ExtractSlotPositionsFromCassette();
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

            ExtractSlotPositionsFromCassette();
            AdjustCellSize();

            // 즉시 갱신 (타이머 우회)
            if (groupBox != null && groupBox.IsHandleCreated && !groupBox.IsDisposed)
            {
                groupBox.Invalidate();
                groupBox.Update();
            }
        }

        /// <summary>
        /// 셀 크기를 조정합니다.
        /// </summary>
        private void AdjustCellSize()
        {
            if (_materialCassette == null || _materialCassette.SlotCount <= 0) return;
            int h = (groupBox?.ClientSize.Height ?? ClientSize.Height);
            if (h <= 0) return;
            _cellSize = Math.Max(1, h / _materialCassette.SlotCount);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AdjustCellSize();
            _needsRedraw = true;
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                RefreshMapImmediate();
            }
        }

        private void SafeInvalidate()
        {
            // 플래그만 설정하고 타이머가 처리하게 함 (깜빡임 방지)
            _needsRedraw = true;
        }

        // 그룹박스 위에 직접 그리기
        private void GroupBox_Paint(object sender, PaintEventArgs e)
        {
            DrawMap(e.Graphics, ((Control)sender).ClientRectangle);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // groupBox가 있으면 거기서만 그림 (중복 그리기 방지)
            base.OnPaint(e);
        }

        private void DrawMap(Graphics g, Rectangle bounds)
        {
            // 그래픽 품질 설정 (깜빡임 방지)
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Low;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;

            // groupBox 텍스트 영역 보정 (단순 상단 마진 14 픽셀 가정)
            int topMargin = 14; // 폰트 높이에 따라 조정 가능
            var drawRect = new Rectangle(bounds.X + 2, bounds.Y + topMargin, bounds.Width - 4, bounds.Height - topMargin - 2);
            if (drawRect.Width <= 0 || drawRect.Height <= 0)
            {
                return;
            }

            // 배경 한번에 그리기
            using (var bgBrush = new SolidBrush(Color.Black))
            {
                g.FillRectangle(bgBrush, bounds);
            }

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
                var wafer = _materialCassette.GetWafer(i);
                Color cellColor = Color.Gray;

                if (wafer != null)
                {
                    switch (wafer.Presence)
                    {
                        case MaterialPresence.Exist: cellColor = Color.LimeGreen; break;
                        case MaterialPresence.NotExist: cellColor = Color.Gray; break;
                        case MaterialPresence.Unknown: cellColor = Color.Yellow; break;
                    }
                    switch (wafer.ProcessSatate)
                    {
                        case MaterialProcessSatate.Ready: cellColor = Color.Blue; break;
                        case MaterialProcessSatate.Processing: cellColor = Color.Orange; break;
                        case MaterialProcessSatate.Completed: cellColor = Color.Green; break;
                        //case MaterialProcessSatate.Stored: cellColor = Color.Blue; break;
                        case MaterialProcessSatate.Unknown: break;
                    }
                }

                int y = drawRect.Bottom - (i + 1) * _cellSize;
                var rect = new Rectangle(drawRect.Left, y, width, _cellSize);

                // 배경 색상
                using (var brush = new SolidBrush(cellColor))
                {
                    g.FillRectangle(brush, rect);
                }

                // 위치 정보가 있는 슬롯 표시 (작은 마커)
                if (_slotZPositions.ContainsKey(i))
                {
                    using (var markerBrush = new SolidBrush(Color.FromArgb(200, Color.White)))
                    {
                        int markerSize = Math.Max(4, _cellSize / 4);
                        g.FillEllipse(markerBrush,
                            rect.Right - markerSize - 2,
                            rect.Top + (rect.Height - markerSize) / 2,
                            markerSize, markerSize);
                    }
                }

                // 편집 모드 또는 이동 모드에서 hover 효과
                if ((_isEditable || EnableSlotMovement) && i == _hoveredSlotIndex)
                {
                    using (var hoverBrush = new SolidBrush(Color.FromArgb(80, Color.White)))
                    {
                        g.FillRectangle(hoverBrush, rect);
                    }
                }

                // 테두리
                using (var pen = new Pen(Color.Black))
                {
                    g.DrawRectangle(pen, rect);
                }

                // 슬롯 번호 표시 (편집 모드 또는 이동 모드일 때)
                if (_isEditable || EnableSlotMovement)
                {
                    DrawSlotNumber(g, rect, i + 1);
                }
            }

            // 편집 모드 표시
            if (_isEditable)
            {
                DrawEditModeIndicator(g, drawRect);
            }

            // 이동 모드 표시
            if (EnableSlotMovement && !_isEditable)
            {
                DrawMoveModeIndicator(g, drawRect);
            }
        }

        /// <summary>
        /// 슬롯 번호 그리기 (외곽선 포함)
        /// </summary>
        private void DrawSlotNumber(Graphics g, Rectangle rect, int slotNumber)
        {
            if (_cellSize < 12) return; // 너무 작으면 생략

            using (var font = new Font("Arial", Math.Max(6, _cellSize * 0.35f), FontStyle.Bold))
            using (var path = new GraphicsPath())
            using (var outlinePen = new Pen(Color.White, 2f))
            using (var fillBrush = new SolidBrush(Color.Black))
            {
                string text = slotNumber.ToString();
                float x = rect.Left + 2;
                float y = rect.Top + (rect.Height - font.Height) / 2;

                path.AddString(text, font.FontFamily, (int)font.Style, font.Size,
                               new PointF(x, y), StringFormat.GenericDefault);

                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(outlinePen, path);
                g.FillPath(fillBrush, path);
            }
        }

        /// <summary>
        /// 편집 모드 표시
        /// </summary>
        private void DrawEditModeIndicator(Graphics g, Rectangle drawRect)
        {
            string editText = "[EDIT]";
            using (var font = new Font("Arial", 7, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Yellow))
            {
                var size = g.MeasureString(editText, font);
                float x = drawRect.Right - size.Width - 2;
                float y = drawRect.Top + 2;
                g.DrawString(editText, font, brush, x, y);
            }
        }

        /// <summary>
        /// 이동 모드 표시
        /// </summary>
        private void DrawMoveModeIndicator(Graphics g, Rectangle drawRect)
        {
            string moveText = "[MOVE]";
            using (var font = new Font("Arial", 7, FontStyle.Bold))
            using (var brush = new SolidBrush(Color.Cyan))
            {
                var size = g.MeasureString(moveText, font);
                float x = drawRect.Right - size.Width - 2;
                float y = drawRect.Top + 2;
                g.DrawString(moveText, font, brush, x, y);
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
    }
}