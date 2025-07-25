using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

namespace SP_GridTypeView
{
    public partial class VisionImageView : UserControl
    {
        private TabControl tabControl;
        private List<PictureBox> pictureBoxes = new List<PictureBox>();
        private List<float> zoomFactors = new List<float>();
        private List<IndividualMenuButton> liveButtons = new List<IndividualMenuButton>();
        private List<IndividualMenuButton> grabButtons = new List<IndividualMenuButton>();
        private List<Point> imageOffsets = new List<Point>();
        private List<Point> dragStartPoints = new List<Point>();
        private List<Point> dragStartOffsets = new List<Point>();
        private bool isDragging = false;
        private int draggingIndex = -1;

        // 탭 높이 속성 추가
        private int _tabHeight = 28;
        public int TabHeight
        {
            get => _tabHeight;
            set
            {
                _tabHeight = value;
                tabControl.ItemSize = new Size(tabControl.ItemSize.Width, _tabHeight);
                tabControl.SizeMode = TabSizeMode.Fixed;
                tabControl.Invalidate();
            }
        }

        // 탭 테두리 색상 속성 추가
        private Color _tabBorderColor = Color.Black;
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.Description("탭 테두리 색상")]
        public Color TabBorderColor
        {
            get => _tabBorderColor;
            set
            {
                _tabBorderColor = value;
                tabControl.Invalidate();
            }
        }

        // 탭 테두리 두께 속성 추가
        private int _tabBorderWidth = 2;
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.Description("탭 테두리 두께")]
        public int TabBorderWidth
        {
            get => _tabBorderWidth;
            set
            {
                _tabBorderWidth = Math.Max(1, value); // 최소 1픽셀
                tabControl.Invalidate();
            }
        }

        // 탭 폰트 속성 추가
        private Font _tabFont = new Font("맑은 고딕", 9, FontStyle.Regular);
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.Description("탭 글씨체 및 크기")]
        public Font TabFont
        {
            get => _tabFont;
            set
            {
                _tabFont = value;
                tabControl.Invalidate();
            }
        }

        // 십자 점선 두께 속성 추가
        private int _crossLineWidth = 1;
        [System.ComponentModel.Category("Appearance")]
        [System.ComponentModel.Description("가운데 녹색 십자 점선 두께")]
        public int CrossLineWidth
        {
            get => _crossLineWidth;
            set
            {
                _crossLineWidth = Math.Max(1, value);
                foreach (var pb in pictureBoxes)
                    pb.Invalidate();
            }
        }

        private bool _showLiveGrabButtons = true;
        public bool ShowLiveGrabButtons
        {
            get => _showLiveGrabButtons;
            set
            {
                if (_showLiveGrabButtons != value)
                {
                    _showLiveGrabButtons = value;
                    UpdateLiveGrabButtonsVisibility();
                }
            }
        }

        public VisionImageView()
        {
            InitializeComponent();
            InitializeUserControl();
        }

        private void InitializeUserControl()
        {
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(120, _tabHeight),
                SizeMode = TabSizeMode.Fixed,
            };
            tabControl.DrawItem += TabControl_DrawItem;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            Controls.Add(tabControl);
        }

        // 탭 변경 시 버튼 선택 상태 변경
        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = tabControl.SelectedIndex;
            if (idx >= 0 && idx < zoomFactors.Count && idx < pictureBoxes.Count)
            {
                zoomFactors[idx] = 1.0f;
                imageOffsets[idx] = Point.Empty; // ← 추가: 오프셋도 초기화
                pictureBoxes[idx].Invalidate();
            }
            // 버튼 선택 상태 변경
            for (int i = 0; i < liveButtons.Count; i++)
            {
                liveButtons[i].SetButtonState(i == idx);
                grabButtons[i].SetButtonState(i == idx);
            }

            // ChangeView 함수 호출
            ChangeView(idx);
        }

        // 탭 인덱스를 받아 메시지창으로 보여주는 함수
        private void ChangeView(int tabIndex)
        {
            MessageBox.Show($"ChangeView 호출됨: 탭 인덱스 = {tabIndex}", "ChangeView", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 탭 색상 커스텀 드로우
        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabControl.TabPages[e.Index];
            Rectangle tabRect = tabControl.GetTabRect(e.Index);

            // 선택된 탭은 하얀색, 아닌 탭은 회색
            Color backColor = (e.Index == tabControl.SelectedIndex) ? Color.White : Color.Gainsboro;

            using (Brush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabRect);
            }

            // 테두리 그리기 (사용자 지정 색상과 두께)
            using (Pen borderPen = new Pen(_tabBorderColor, _tabBorderWidth))
            {
                Rectangle borderRect = tabRect;
                if (_tabBorderWidth > 1)
                {
                    borderRect.Inflate(-_tabBorderWidth / 2, -_tabBorderWidth / 2);
                }
                e.Graphics.DrawRectangle(borderPen, borderRect);
            }

            // 텍스트 그리기 (사용자 지정 폰트)
            TextRenderer.DrawText(
                e.Graphics,
                page.Text,
                _tabFont,
                tabRect,
                Color.Black,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter
            );
        }

        /// <summary>
        /// 탭 이름 배열을 받아 탭과 PictureBox를 동적으로 생성합니다.
        /// </summary>
        public void SetImageViewName(params object[] tabNames)
        {
            tabControl.TabPages.Clear();
            pictureBoxes.Clear();
            zoomFactors.Clear();
            liveButtons.Clear();
            grabButtons.Clear();
            imageOffsets.Clear();
            dragStartPoints.Clear();
            dragStartOffsets.Clear();

            for (int i = 0; i < tabNames.Length; i++)
            {
                var name = tabNames[i];
                var tabPage = new TabPage((string)name)
                {
                    Padding = new Padding(0)
                };

                var panel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black
                };

                var buttonPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 32,
                    BackColor = Color.FromArgb(214, 220, 229),
                    Visible = _showLiveGrabButtons  
                };

                // Live 버튼
                var liveButton = new IndividualMenuButton
                {
                    Text = "Live",
                    Location = new Point(4, 4),
                    Size = new Size(55, 24),
                    Tag = i,
                    Visible = _showLiveGrabButtons
                };
                liveButton.Click += (s, e) =>
                {
                    int tabIndex = (int)((IndividualMenuButton)s).Tag;
                    OnLiveButtonClick(tabIndex);
                };
                liveButtons.Add(liveButton);

                // Grab 버튼
                var grabButton = new IndividualMenuButton
                {
                    Text = "Grab",
                    Location = new Point(65, 4),
                    Size = new Size(55, 24),
                    Tag = i,
                    Visible = _showLiveGrabButtons
                };
                grabButton.Click += (s, e) =>
                {
                    int tabIndex = (int)((IndividualMenuButton)s).Tag;
                    OnGrabButtonClick(tabIndex);
                };
                grabButtons.Add(grabButton);

                buttonPanel.Controls.Add(liveButton);
                buttonPanel.Controls.Add(grabButton);

                var pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.Black,
                    BorderStyle = BorderStyle.None,
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                zoomFactors.Add(1.0f);
                imageOffsets.Add(Point.Empty);
                dragStartPoints.Add(Point.Empty);
                dragStartOffsets.Add(Point.Empty);

                // 마우스 휠 줌 이벤트
                pictureBox.MouseWheel += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf((PictureBox)s);
                    if (idx < 0) return;

                    float currentZoom = zoomFactors[idx];
                    float newZoom = e.Delta > 0
                        ? Math.Min(currentZoom * 1.1f, 10f)
                        : Math.Max(currentZoom / 1.1f, 1.0f);

                    ZoomAtPoint(idx, newZoom, (PictureBox)s);
                };

                // 더블 클릭 시 줌 1배로 초기화
                pictureBox.DoubleClick += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf((PictureBox)s);
                    if (idx >= 0)
                    {
                        // 줌 배율이 1.0으로 초기화될 때 오프셋도 초기화 (더블클릭, 메뉴 등)
                        zoomFactors[idx] = 1.0f;
                        imageOffsets[idx] = Point.Empty;
                        pictureBox.Invalidate();
                    }
                };

                // 이미지 확대/축소를 Paint에서 처리 (기존 코드 유지)
                pictureBox.Paint += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf((PictureBox)s);
                    if (idx < 0) return;
                    var pb = (PictureBox)s;
                    var img = pb.Image;
                    float zoom = zoomFactors[idx];
                    Point offset = imageOffsets[idx];

                    e.Graphics.Clear(pb.BackColor);

                    if (img != null)
                    {
                        int drawWidth = (int)(img.Width * zoom);
                        int drawHeight = (int)(img.Height * zoom);
                        int x = (pb.Width - drawWidth) / 2 + offset.X;
                        int y = (pb.Height - drawHeight) / 2 + offset.Y;

                        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        e.Graphics.DrawImage(img, new Rectangle(x, y, drawWidth, drawHeight));

                        // 녹색 점선 십자 실선 (PictureBox 크기 기준)
                        int centerX = pb.ClientRectangle.Left + pb.ClientRectangle.Width / 2;
                        int centerY = pb.ClientRectangle.Top + pb.ClientRectangle.Height / 2;
                        using (var crossPen = new Pen(Color.Lime, _crossLineWidth))
                        {
                            crossPen.DashStyle = DashStyle.Dash;
                            e.Graphics.DrawLine(crossPen, pb.ClientRectangle.Left, centerY, pb.ClientRectangle.Right - 1, centerY);
                            e.Graphics.DrawLine(crossPen, centerX, pb.ClientRectangle.Top, centerX, pb.ClientRectangle.Bottom - 1);
                        }
                    }

                    var rect = pb.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    using (var pen = new Pen(Color.Black, 2))
                    {
                        e.Graphics.DrawRectangle(pen, rect);
                    }

                    // --- 네비게이션 미니맵 표시 (오른쪽 상단) ---
                    if (img != null && zoom > 1.0f)
                    {
                        // 네비게이션 미니맵 크기
                        int navWidth = 80;
                        int navHeight = 80;

                        // 미니맵 위치 (오른쪽 상단)
                        int navX = pb.ClientRectangle.Right - navWidth - 8;
                        int navY = pb.ClientRectangle.Top + 8;

                        // 이미지 비율에 맞게 미니맵 크기 조정
                        float imgAspect = (float)img.Width / img.Height;
                        float navAspect = (float)navWidth / navHeight;
                        int drawNavW = navWidth, drawNavH = navHeight;
                        if (imgAspect > navAspect)
                        {
                            drawNavH = (int)(navWidth / imgAspect);
                        }
                        else
                        {
                            drawNavW = (int)(navHeight * imgAspect);
                        }

                        Rectangle navRect = new Rectangle(navX, navY, drawNavW, drawNavH);
                        e.Graphics.DrawImage(img, navRect);

                        // 실제 이미지가 PictureBox에 그려지는 위치와 크기
                        int drawWidth = (int)(img.Width * zoom);
                        int drawHeight = (int)(img.Height * zoom);
                        int drawX = (pb.Width - drawWidth) / 2 + offset.X;
                        int drawY = (pb.Height - drawHeight) / 2 + offset.Y;

                        // 현재 보여지는 영역을 이미지 좌표계로 환산
                        float viewImgX = Math.Max(0, -((float)drawX) / zoom);
                        float viewImgY = Math.Max(0, -((float)drawY) / zoom);
                        float viewImgW = Math.Min(img.Width - viewImgX, pb.Width / zoom);
                        float viewImgH = Math.Min(img.Height - viewImgY, pb.Height / zoom);

                        // 이미지 좌표계 -> 미니맵 좌표계로 변환
                        float scaleX = (float)drawNavW / img.Width;
                        float scaleY = (float)drawNavH / img.Height;

                        RectangleF viewRect = new RectangleF(
                            navRect.X + viewImgX * scaleX,
                            navRect.Y + viewImgY * scaleY,
                            viewImgW * scaleX,
                            viewImgH * scaleY
                        );

                        // 현재 뷰 영역 표시 (빨간색 반투명)
                        using (Brush brush = new SolidBrush(Color.FromArgb(80, Color.Red)))
                            e.Graphics.FillRectangle(brush, viewRect);
                        using (Pen pen = new Pen(Color.Red, 2))
                            e.Graphics.DrawRectangle(pen, Rectangle.Round(viewRect));
                    }
                };

                // ContextMenuStrip 생성 및 연결
                var contextMenu = new ContextMenuStrip();
                var saveMenuItem = new ToolStripMenuItem("저장");
                var zoom2xMenuItem = new ToolStripMenuItem("2배");
                var zoom4xMenuItem = new ToolStripMenuItem("4배");
                var zoom8xMenuItem = new ToolStripMenuItem("8배");
                contextMenu.Items.Add(saveMenuItem);
                contextMenu.Items.Add(new ToolStripSeparator());
                contextMenu.Items.Add(zoom2xMenuItem);
                contextMenu.Items.Add(zoom4xMenuItem);
                contextMenu.Items.Add(zoom8xMenuItem);
                pictureBox.ContextMenuStrip = contextMenu;

                // 저장 메뉴 클릭 이벤트
                saveMenuItem.Click += (s, e) =>
                {
                    if (pictureBox.Image != null)
                    {
                        using (var sfd = new SaveFileDialog())
                        {
                            sfd.Filter = "PNG 파일 (*.png)|*.png|JPEG 파일 (*.jpg)|*.jpg|Bitmap 파일 (*.bmp)|*.bmp";
                            sfd.FileName = "image";
                            if (sfd.ShowDialog() == DialogResult.OK)
                            {
                                var ext = System.IO.Path.GetExtension(sfd.FileName).ToLower();
                                var format = System.Drawing.Imaging.ImageFormat.Png;
                                if (ext == ".jpg") format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                else if (ext == ".bmp") format = System.Drawing.Imaging.ImageFormat.Bmp;
                                pictureBox.Image.Save(sfd.FileName, format);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("저장할 이미지가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                };

                // 줌 메뉴 클릭 이벤트
                zoom2xMenuItem.Click += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf(pictureBox);
                    if (idx >= 0)
                    {
                        ZoomAtPoint(idx, 2.0f, pictureBox);
                    }
                };
                zoom4xMenuItem.Click += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf(pictureBox);
                    if (idx >= 0)
                    {
                        ZoomAtPoint(idx, 4.0f, pictureBox);
                    }
                };
                zoom8xMenuItem.Click += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf(pictureBox);
                    if (idx >= 0)
                    {
                        ZoomAtPoint(idx, 8.0f, pictureBox);
                    }
                };

                // 포커스가 있어야 MouseWheel 이벤트가 동작하므로 클릭 시 포커스 설정
                pictureBox.MouseEnter += (s, e) => { pictureBox.Focus(); };

                // 마우스 드래그 이벤트 추가
                pictureBox.MouseDown += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf((PictureBox)s);
                    if (e.Button == MouseButtons.Left && zoomFactors[idx] > 1.0f)
                    {
                        isDragging = true;
                        draggingIndex = idx;
                        dragStartPoints[idx] = e.Location;
                        dragStartOffsets[idx] = imageOffsets[idx]; // 드래그 시작 시점의 오프셋 저장
                        pictureBox.Cursor = Cursors.Hand;
                    }
                };
                pictureBox.MouseMove += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf((PictureBox)s);
                    if (isDragging && draggingIndex == idx && e.Button == MouseButtons.Left)
                    {
                        var pb = pictureBoxes[idx];
                        var img = pb.Image;
                        if (img == null) return;

                        float zoom = zoomFactors[idx];

                        // 드래그 시작 시점의 오프셋 + 마우스 이동량
                        int dx = e.Location.X - dragStartPoints[idx].X;
                        int dy = e.Location.Y - dragStartPoints[idx].Y;
                        var newOffset = new Point(dragStartOffsets[idx].X + dx, dragStartOffsets[idx].Y + dy);

                        int drawWidth = (int)(img.Width * zoom);
                        int drawHeight = (int)(img.Height * zoom);

                        // Zoom 모드 중앙정렬 기준 clamp
                        int maxOffsetX = (drawWidth - pb.Width) / 2;
                        int minOffsetX = -maxOffsetX;
                        int maxOffsetY = (drawHeight - pb.Height) / 2;
                        int minOffsetY = -maxOffsetY;

                        if (drawWidth <= pb.Width)
                            newOffset.X = 0;
                        else
                            newOffset.X = Math.Max(minOffsetX, Math.Min(newOffset.X, maxOffsetX));

                        if (drawHeight <= pb.Height)
                            newOffset.Y = 0;
                        else
                            newOffset.Y = Math.Max(minOffsetY, Math.Min(newOffset.Y, maxOffsetY));

                        imageOffsets[idx] = newOffset;
                        pictureBoxes[idx].Invalidate();
                    }
                };
                pictureBox.MouseUp += (s, e) =>
                {
                    int idx = pictureBoxes.IndexOf((PictureBox)s);
                    if (e.Button == MouseButtons.Left && isDragging && draggingIndex == idx)
                    {
                        isDragging = false;
                        draggingIndex = -1;
                        pictureBox.Cursor = Cursors.Default;
                    }
                };

                panel.Controls.Add(pictureBox);
                panel.Controls.Add(buttonPanel);

                tabPage.Controls.Add(panel);
                tabControl.TabPages.Add(tabPage);
                pictureBoxes.Add(pictureBox);
            }

            // 최초 선택된 탭의 버튼만 활성화
            TabControl_SelectedIndexChanged(tabControl, EventArgs.Empty);
        }

        /// <summary>
        /// 특정 탭의 PictureBox에 이미지를 설정합니다.
        /// </summary>
        public void SetImage(int tabIndex, Image image)
        {
            if (tabIndex >= 0 && tabIndex < pictureBoxes.Count)
            {
                pictureBoxes[tabIndex].Image = image;
                pictureBoxes[tabIndex].Invalidate();
            }
        }

        // 버튼 표시/숨김 처리
        private void UpdateLiveGrabButtonsVisibility()
        {
            foreach (var btn in liveButtons)
                btn.Visible = _showLiveGrabButtons;
            foreach (var btn in grabButtons)
                btn.Visible = _showLiveGrabButtons;
        }

        // Live 버튼 클릭 이벤트 핸들러
        private void OnLiveButtonClick(int tabIndex)
        {
            // 원하는 동작 구현
            MessageBox.Show($"Live 버튼 클릭: 탭 인덱스 {tabIndex}");
        }

        // Grab 버튼 클릭 이벤트 핸들러
        private void OnGrabButtonClick(int tabIndex)
        {
            // 원하는 동작 구현
            MessageBox.Show($"Grab 버튼 클릭: 탭 인덱스 {tabIndex}");
        }

        private void ZoomAtPoint(int idx, float newZoom, PictureBox pictureBox)
        {
            var img = pictureBox.Image;
            if (img == null) return;

            float oldZoom = zoomFactors[idx];
            if (Math.Abs(oldZoom - newZoom) < 0.0001f) return;

            // 1배율 이하로 줄어들면 무조건 1배율+중앙정렬
            if (newZoom <= 1.0f)
            {
                zoomFactors[idx] = 1.0f;
                imageOffsets[idx] = Point.Empty;
                pictureBox.Invalidate();
                return;
            }

            // 줌인(배율 증가)은 이미지 중앙 기준 확대
            Point centerPt = new Point(pictureBox.Width / 2, pictureBox.Height / 2);

            int oldDrawWidth = (int)(img.Width * oldZoom);
            int oldDrawHeight = (int)(img.Height * oldZoom);
            int oldX = (pictureBox.Width - oldDrawWidth) / 2 + imageOffsets[idx].X;
            int oldY = (pictureBox.Height - oldDrawHeight) / 2 + imageOffsets[idx].Y;
            float imgX = (centerPt.X - oldX) / oldZoom;
            float imgY = (centerPt.Y - oldY) / oldZoom;

            int newDrawWidth = (int)(img.Width * newZoom);
            int newDrawHeight = (int)(img.Height * newZoom);
            int newX = (pictureBox.Width - newDrawWidth) / 2;
            int newY = (pictureBox.Height - newDrawHeight) / 2;

            int newOffsetX = (int)(centerPt.X - newX - imgX * newZoom);
            int newOffsetY = (int)(centerPt.Y - newY - imgY * newZoom);

            zoomFactors[idx] = newZoom;
            imageOffsets[idx] = new Point(newOffsetX, newOffsetY);
            pictureBox.Invalidate();
        }
    }
}
