using QMC.Common;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    #region Define
    public delegate void MultiParameterValueChangeEventHandle();
    public delegate void MultiParameterButtonClickEventHandle();
    public delegate void MultiParameterImageChangedEventHandlerHandler(int nIndex, VisionImage image);
    #endregion

    public partial class MultiPatternMatchingParameterControl : UserControl
    {
        #region Define
        public event MultiParameterValueChangeEventHandle MultiParameterValueChangeEvent;
        public event MultiParameterValueChangeEventHandle MultiParameterButtonClickEvent;
        public event MultiParameterImageChangedEventHandlerHandler MultiImageChangeEvent;
        public event EventHandler TrainImageListChanged; // 새 이벤트: 리스트 구조 변경 알림
        #endregion

        #region Property
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public MultiPatternMatchingParameters Parameters { get; set; }
        public VisionImage TrainImage { get; set; }
        public VisionImage LearnImage { get; set; }
        public double Tolerance { get; set; }
        public bool DuplicateChecked { get; set; }
        public int MaxInstnce { get; set; }
        public double MinScore { get; set; }
        public bool UseMaskImage { get; set; }

        public int SelectedIndex { get; set; }
        #endregion

        #region Field
        private RectangleD m_StartRectAngle;
        private RectangleD m_EndRectAngle;
        private RectangleD m_RectAngle;
        private bool m_IsChecked;
        private readonly bool _designMode;
        #endregion

        #region Constructor
        public MultiPatternMatchingParameterControl(MultiPatternMatchingParameters parameters)
        {
            // 디자인 타임 감지
            _designMode = IsActuallyInDesignMode();

            // 파라미터 준비 (가벼움)
            Parameters = parameters ?? new MultiPatternMatchingParameters();

            // 디자이너가 먼저 컨트롤 트리를 만들어야 하므로 먼저 InitializeComponent
            InitializeComponent();

            // 디자인 타임이면 여기서 종료 (나머지 런타임 로직 비활성)
            if (_designMode)
            {
                try { BackColor = Color.Transparent; } catch { }
                return;
            }

            this.m_IsChecked = false;
            this.UseMaskImage = false;
            m_StartRectAngle = new RectangleD();
            m_EndRectAngle = new RectangleD();
            m_RectAngle = new RectangleD();
            SelectedIndex = 0;

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            if (this.pictureBoxMultiTraimImage != null)
            {
                this.pictureBoxMultiTraimImage.BackColor = Color.White;
                this.pictureBoxMultiTraimImage.ImageChanged += ChangeTrainImage;
            }
            UpdateStyles();

            UpdateParameters();
        }

        public MultiPatternMatchingParameterControl() : this(new MultiPatternMatchingParameters())
        {
        }

        private bool IsActuallyInDesignMode()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
            try { return Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase); }
            catch { return false; }
        }
        #endregion

        #region Method
        private void ChangeTrainImage()
        {
            if (_designMode) return;
            int curIndex = this.baseListBoxTrainList.SelectedIndex;
            if (MultiImageChangeEvent != null && curIndex >= 0)
            {
                MultiImageChangeEvent(curIndex, this.pictureBoxMultiTraimImage.GetImage());
            }
        }

        public void UpdateParameters()
        {
            if (_designMode) 
                return;
            if (Parameters == null) 
                return;

            this.UseMaskImage = Parameters.UseMaskImage;
            this.checkBoxDupCheck.Checked = Parameters.DuplicateChecked;
            this.TextBoxTolerance.Text = Parameters.MaxTolerance.ToString();
            this.TextMaxInstance.Text = Parameters.MaxInstance.ToString();
            this.TextMinScore.Text = Parameters.MinScore.ToString();
            this.ToggleButtonUseMaskImage.UpdateToggleStatus(Parameters.UseMaskImage);

            try
            {
                if (Parameters.TrainImages != null && Parameters.TrainImages.Count != 0)
                {
                    UpdateTrainList();
                }
                if (Parameters.TrainImages != null && SelectedIndex >= 0 && SelectedIndex < Parameters.TrainImages.Count)
                {
                    var vimg = Parameters.TrainImages[SelectedIndex];
                    if (vimg != null)
                        this.pictureBoxMultiTraimImage.SetImage(vimg.GetImage());
                    else
                        this.pictureBoxMultiTraimImage.SetImage((Image)null);
                }
            }
            catch { /* swallow at runtime */ }
        }

        public void UpdateParameters(MultiPatternMatchingParameters parameter)
        {
            Parameters = parameter ?? new MultiPatternMatchingParameters();
            UpdateParameters();
        }

        private void UpdateTrainList()
        {
            if (_designMode) return;
            BindingSource bs = new BindingSource();
            bs.DataSource = Parameters?.TrainImages ?? new List<VisionImage>();
            baseListBoxTrainList.DisplayMember = "Tag";
            baseListBoxTrainList.DataSource = bs;
        }

        public void SetTrainImage(VisionImage image)
        {
            if (_designMode) return;
            if (image == null) { this.pictureBoxMultiTraimImage.SetImage((Image)null); return; }
            this.pictureBoxMultiTraimImage.SetImage(image.GetImage());
        }

        public void SetTrainList()
        {
            if (_designMode) return;
            if (this.Parameters?.TrainImages != null)
            {
                UpdateTrainList();
            }
        }

        public void SwapList<T>(List<T> list, int from, int to)
        {
            if (from < 0 || to < 0) return;
            try
            {
                if (from < list.Count && to < list.Count)
                {
                    T tmp = list[from];
                    list[from] = list[to];
                    list[to] = tmp;
                }
            }
            catch { }
        }

        private void OnTrainImageListChanged()
        {
            if (_designMode) return;
            try
            {
                if (Parameters?.TrainImages != null)
                {
                    for (int i = 0; i < Parameters.TrainImages.Count; i++)
                    {
                        if (Parameters.TrainImages[i] != null &&
                            (Parameters.TrainImages[i].Tag == null || string.IsNullOrWhiteSpace(Parameters.TrainImages[i].Tag.ToString())))
                            Parameters.TrainImages[i].Tag = i.ToString();
                    }
                }
            }
            catch { }
            try { TrainImageListChanged?.Invoke(this, EventArgs.Empty); } catch { }
        }
        #endregion

        #region EventHandler
        private void checkBoxDupCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (_designMode) return;
            m_IsChecked = this.checkBoxDupCheck.Checked;
            if (Parameters != null) Parameters.DuplicateChecked = m_IsChecked;
        }

        private void ChangeParametersTolerance(object sender, EventArgs e)
        {
            if (_designMode) return;
            if (Parameters != null)
            {
                try
                {
                    Parameters.MaxTolerance = double.Parse(TextBoxTolerance.Text);
                    Parameters.MinTolerance = double.Parse(TextBoxTolerance.Text) * -1;
                }
                catch { }
            }
        }

        private void ChangeParametersMaxInstnce(object sender, EventArgs e)
        {
            if (_designMode) return;
            if (Parameters != null)
            {
                try
                {
                    Parameters.MaxInstance = int.Parse(TextMaxInstance.Text);
                }
                catch { }
            }
        }

        private void ChangeParametersMinScore(object sender, EventArgs e)
        {
            if (_designMode) return;
            if (Parameters != null)
            {
                double dValue = 0.0;
                double.TryParse(TextMinScore.Text, out dValue);
                Parameters.MinScore = dValue;
            }
        }

        private void baseToggleButton_Click(object sender, EventArgs e)
        {
            if (_designMode) return;

            bool bOn = ToggleButtonUseMaskImage.GetButtonStatus();
            if (bOn == false)
            {
                ToggleButtonUseMaskImage.UpdateToggleStatus(true);

                this.pictureBoxMultiTraimImage.MouseDown += pictureBoxMultiTrainImage_MouseDown;
                this.pictureBoxMultiTraimImage.MouseUp += pictureBoxMultiTrainImage_MouseUp;
                this.pictureBoxMultiTraimImage.MouseMove += pictureBoxMultiTrainImage_MouseMove;
                this.pictureBoxMultiTraimImage.Paint += pictureBoxMultiTrainImage_Paint;
            }
            else // bOn == true
            {
                ToggleButtonUseMaskImage.UpdateToggleStatus(false);

                // 잘못된 += null 제거, 정확히 해제
                this.pictureBoxMultiTraimImage.MouseDown -= pictureBoxMultiTrainImage_MouseDown;
                this.pictureBoxMultiTraimImage.MouseUp -= pictureBoxMultiTrainImage_MouseUp;
                this.pictureBoxMultiTraimImage.MouseMove -= pictureBoxMultiTrainImage_MouseMove;
                this.pictureBoxMultiTraimImage.Paint -= pictureBoxMultiTrainImage_Paint;
            }
            if (Parameters != null)
                Parameters.UseMaskImage = ToggleButtonUseMaskImage.GetButtonStatus();
        }

        private void pictureBoxMultiTrainImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (_designMode) return;
            m_StartRectAngle = new RectangleD(e.X, e.Y, 0, 0);
        }

        private void pictureBoxMultiTrainImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (_designMode) return;
            m_EndRectAngle = new RectangleD(e.X, e.Y, 0, 0);
        }

        private void pictureBoxMultiTrainImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_designMode) return;
            if (e.Button == MouseButtons.Left)
            {
                // 간단한 드래그 박스 계산
                double x1 = Math.Min(m_StartRectAngle.X, e.X);
                double y1 = Math.Min(m_StartRectAngle.Y, e.Y);
                double x2 = Math.Max(m_StartRectAngle.X, e.X);
                double y2 = Math.Max(m_StartRectAngle.Y, e.Y);
                m_RectAngle = new RectangleD(x1, y1, x2 - x1, y2 - y1);
                this.Refresh();
            }
            if (Parameters != null)
                Parameters.MaskRegion = m_RectAngle;
        }

        private void pictureBoxMultiTrainImage_Paint(object sender, PaintEventArgs e)
        {
            if (_designMode) return;
            using (Pen pen = new Pen(Color.Red, 2))
            using (Brush brush = new SolidBrush(Color.FromArgb(64, Color.Red)))
            {
                e.Graphics.DrawRectangle(pen, m_RectAngle);
                e.Graphics.FillRectangle(brush, m_RectAngle);
            }
        }

        private void baseButtonAdd_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            VisionImage newimage = new VisionImage();
            newimage.Tag = (this.Parameters?.TrainImages?.Count ?? 0).ToString();
            if (this.Parameters != null)
            {
                if (this.Parameters.TrainImages == null) this.Parameters.TrainImages = new System.Collections.Generic.List<VisionImage>();
                this.Parameters.TrainImages.Add(newimage);
            }
            UpdateTrainList();
            OnTrainImageListChanged();
        }

        private void baseButtonRemove_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            int sel = baseListBoxTrainList.SelectedIndex;
            if (sel >= 0 && this.Parameters != null && this.Parameters.TrainImages != null && sel < this.Parameters.TrainImages.Count)
            {
                this.Parameters.TrainImages.RemoveAt(sel);
                UpdateTrainList();
                OnTrainImageListChanged();
            }
        }

        private void baseButtonClear_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            if (this.Parameters?.TrainImages != null)
                this.Parameters.TrainImages.Clear();
            UpdateTrainList();
            OnTrainImageListChanged();
        }

        private void baseButtonUp_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            int sel = baseListBoxTrainList.SelectedIndex;

            if (sel > 0 && this.Parameters?.TrainImages != null)
            {
                SwapList<VisionImage>(this.Parameters.TrainImages, sel, sel - 1);
                UpdateTrainList();
                baseListBoxTrainList.SetSelected(sel - 1, true);
                OnTrainImageListChanged();
            }
        }

        private void baseButtonDown_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            int sel = baseListBoxTrainList.SelectedIndex;

            if (this.Parameters?.TrainImages != null && sel >= 0 && sel < this.Parameters.TrainImages.Count - 1)
            {
                SwapList<VisionImage>(this.Parameters.TrainImages, sel, sel + 1);
                UpdateTrainList();
                baseListBoxTrainList.SetSelected(sel + 1, true);
                OnTrainImageListChanged();
            }
        }

        private void baseListBoxTrainList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_designMode) return;
            int curIndex = baseListBoxTrainList.SelectedIndex;

            try
            {
                if (curIndex > -1 && Parameters?.TrainImages != null)
                {
                    this.SelectedIndex = curIndex;
                    var img = Parameters.TrainImages[SelectedIndex];
                    if (img != null)
                        this.pictureBoxMultiTraimImage.SetImage(img.GetImage());
                    else
                        this.pictureBoxMultiTraimImage.SetImage((Image)null);
                }
                OnTrainImageListChanged();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        #endregion
    }
}
