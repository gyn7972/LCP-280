using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.VisionPart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common
{
    public delegate void ClickMultiTrainButtonEventHandler(PatternMatchingParamControl.ButtonType type);
    public delegate void ChangedMultiTrainImageEventHandler(int nIndex, VisionImage image);
    public delegate void ChangeMultiTrainImageListEventHandler(MultiPatternMatchingParameters parameters);

    public partial class PatternMatchingParamControl : UserControl
    {
        public enum ButtonType { Train, Add, Remove, Clear, Up, Down }

        public int SelectedRow { set; get; }
        public string ImageTag { set; get; }

        #region Field
        public ClickMultiTrainButtonEventHandler MultiTrainButtonClick;
        public ChangedMultiTrainImageEventHandler MultiImageChanged;
        public event ChangeMultiTrainImageListEventHandler MultiImageChangeImageList;
        private Camera m_Camera;
        public MultiPatternMatchingVisionPart m_Owner;
        private MultiPatternMatchingParameters m_MultiPatternMatchingParameters;
        private MaintROIControl m_RoiControl; // (May be null – ROI not supported for multi tool directly)
        private PatternMatchingResultControl m_ResultControl;
        private VisionImageViewer m_ImageViewer;
        #endregion

        private static readonly Size CompactPreferredSize = new Size(470, 280);
        private readonly bool _designMode;

        public PatternMatchingParamControl()
        {
            _designMode = IsActuallyInDesignMode();
            InitializeComponent();
            this.MinimumSize = new Size(CompactPreferredSize.Width, CompactPreferredSize.Height);
            if (_designMode)
            {
                BackColor = Color.Transparent;
                return;
            }
            RuntimeInitialize(null, false, false);
        }

        public PatternMatchingParamControl(MultiPatternMatchingVisionPart visionpart, bool useROIControl, bool isMaint)
        {
            _designMode = IsActuallyInDesignMode();
            InitializeComponent();
            this.MinimumSize = new Size(CompactPreferredSize.Width, CompactPreferredSize.Height);
            if (_designMode)
            {
                BackColor = Color.Transparent;
                return;
            }
            RuntimeInitialize(visionpart, useROIControl, isMaint);
        }

        // 외부에서 VisionPart 주입 위한 메서드 추가
        public void SetOwner(MultiPatternMatchingVisionPart owner)
        {
            if (_designMode) return;
            m_Owner = owner;
            if (m_Owner != null)
            {
                try { m_MultiPatternMatchingParameters = m_Owner.GetPatternMatchingParameters(); } catch { }
            }
            BindResultControl();
        }

        private void RuntimeInitialize(MultiPatternMatchingVisionPart visionpart, bool useROIControl, bool isMaint)
        {
            m_Owner = visionpart;
            if (visionpart != null)
            {
                try { m_MultiPatternMatchingParameters = visionpart.GetPatternMatchingParameters(); }
                catch { }
            }
            if (m_MultiPatternMatchingParameters == null)
                m_MultiPatternMatchingParameters = new MultiPatternMatchingParameters();

            if (m_Owner != null)
                m_Camera = m_Owner.Camera;

            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();

            pictureBoxTrainImage.BackColor = Color.Black;
            pictureBoxTrainImage.ImageChanged += TrainImageChanged;
            baseListBoxTrainList.BackColor = Color.DimGray;
            baseListBoxTrainList.ForeColor = Color.White;

            try
            {
                SetPatternMatchingParam(m_MultiPatternMatchingParameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PatternMatchingParamControl SetPatternMatchingParam error: " + ex.Message);
            }

            InitROIControl(useROIControl);
            InitSearchControl(isMaint);
            BindResultControl();
        }

        private bool IsActuallyInDesignMode()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return true;
            try { return Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase); }
            catch { return false; }
        }

        public override Size GetPreferredSize(Size proposedSize) => CompactPreferredSize;

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (_designMode) return;
            if (Width > CompactPreferredSize.Width * 1.5 || Height > CompactPreferredSize.Height * 1.5)
                Size = CompactPreferredSize;
        }

        private void InitParamGroupControl(ParamGroup group) { }

        private void InitSearchControl(bool isMaint)
        {
            if (_designMode) return;
            if (isMaint)
            {
                m_ResultControl = new PatternMatchingResultControl();
                if (m_RoiControl != null)
                    m_ResultControl.Location = new Point(m_RoiControl.Location.X, m_RoiControl.Location.Y + m_RoiControl.Height + 5);
                else
                    m_ResultControl.Location = new Point(5, pictureBoxTrainImage.Bottom + 10);
                m_ResultControl.SearchButtonClick += ResultControl_SearchButtonClick;
                baseGroupBoxPatternMatching.Controls.Add(m_ResultControl);
                BindResultControl();
            }
        }

        // ResultControl 바인딩(소유자/파라미터/뷰어)
        private void BindResultControl()
        {
            if (_designMode) return;
            if (m_ResultControl != null && m_MultiPatternMatchingParameters != null)
            {
                m_ResultControl.Bind(m_Owner, m_MultiPatternMatchingParameters, m_ImageViewer);
            }
        }

        public void SetImageViewer(VisionImageViewer imageViewer)
        {
            m_ImageViewer = imageViewer;
            if (m_RoiControl != null)
                m_RoiControl.SetImageviwer(imageViewer);
            BindResultControl();
        }

        private void InitROIControl(bool useROIControl)
        {
            if (_designMode || !useROIControl) return;
        }

        private void ResultControl_SearchButtonClick()
        {
            if (_designMode) return;
            PatternMatchingResult result = null;
            if (m_Owner != null)
            {
                if (m_ImageViewer != null)
                    m_ImageViewer.ResultOverlays.Clear();
                Point start = new Point(0, 0);
                Point end = new Point(0, 0);
                try
                {
                    m_Owner.OnSearch(start, end, m_MultiPatternMatchingParameters, m_Owner.GetIlluminationDataSet());
                    result = m_Owner.GetResult();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Search exception: " + ex.Message);
                }
                if (result != null)
                {
                    if (result.Values.Count > 0 && m_ResultControl != null)
                        m_ResultControl.SetResultData(result.Values[0].X, result.Values[0].Y, result.Values[0].R);
                    if (m_ImageViewer != null)
                    {
                        foreach (var overlay in result.ResultOverlays)
                        {
                            m_ImageViewer.ResultOverlays.Add(overlay);
                            overlay.Visible = true;
                        }
                    }
                }
            }
            if (m_Owner?.Camera != null)
                m_Owner.Camera.StartLive();
        }

        private void TrainImageChanged()
        {
            if (_designMode) return;
            int curIndex = baseListBoxTrainList.SelectedIndex;
            if (MultiImageChanged != null && curIndex >= 0)
                MultiImageChanged(curIndex, pictureBoxTrainImage.GetImage());
        }

        public void SetButtonState(bool bOn)
        {
            baseToggleButtonAvg.Enabled = bOn;
            baseToggleButtonAvg.Visible = bOn;
            baseButtonTrain.Enabled = bOn;
            baseButtonTrain.Visible = bOn;
            if (!bOn)
                baseListBoxTrainList.Height = pictureBoxTrainImage.Height;
        }

        private void baseButtonAdd_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            VisionImage newimage = new VisionImage { Tag = m_MultiPatternMatchingParameters.TrainImages.Count.ToString() };
            m_MultiPatternMatchingParameters.TrainImages.Add(newimage);
            UpdateTrainList();
        }

        private void baseButtonRemove_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            int sel = baseListBoxTrainList.SelectedIndex;
            if (sel >= 0 && sel < m_MultiPatternMatchingParameters.TrainImages.Count)
            {
                m_MultiPatternMatchingParameters.TrainImages.RemoveAt(sel);
                UpdateTrainList();
                MultiImageChangeImageList?.Invoke(m_MultiPatternMatchingParameters);
            }
        }

        private void baseButtonClear_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            m_MultiPatternMatchingParameters.TrainImages.Clear();
            UpdateTrainList();
            MultiImageChangeImageList?.Invoke(m_MultiPatternMatchingParameters);
        }

        private void baseButtonUp_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            int sel = baseListBoxTrainList.SelectedIndex;
            if (sel > 0)
            {
                SwapList(m_MultiPatternMatchingParameters.TrainImages, sel, sel - 1);
                UpdateTrainList();
                baseListBoxTrainList.SetSelected(sel - 1, true);
                MultiImageChangeImageList?.Invoke(m_MultiPatternMatchingParameters);
            }
        }

        private void baseButtonDown_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            int sel = baseListBoxTrainList.SelectedIndex;
            if (sel < m_MultiPatternMatchingParameters.TrainImages.Count - 1)
            {
                SwapList(m_MultiPatternMatchingParameters.TrainImages, sel, sel + 1);
                UpdateTrainList();
                baseListBoxTrainList.SetSelected(sel + 1, true);
                MultiImageChangeImageList?.Invoke(m_MultiPatternMatchingParameters);
            }
        }

        private void baseToggleButtonAvg_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            bool isOn = !baseToggleButtonAvg.GetButtonStatus();
            baseToggleButtonAvg.UpdateToggleStatus(isOn);
            if (m_Camera != null)
                m_Camera.IsAvgOn = isOn;
        }

        private void baseButtonTrain_Click(object sender, EventArgs e)
        {
            if (_designMode) return;
            if (SelectedRow < 0 || SelectedRow >= m_MultiPatternMatchingParameters.TrainImages.Count) return;
            ImageTag = string.Empty;
            if (m_MultiPatternMatchingParameters.TrainImages[SelectedRow].Tag == null)
                m_MultiPatternMatchingParameters.TrainImages[SelectedRow].Tag = 0;
            ImageTag = m_MultiPatternMatchingParameters.TrainImages[SelectedRow].Tag.ToString();
            MultiTrainButtonClick?.Invoke(ButtonType.Train);
            try { m_Owner?.OnTrain(new Point(0, 0), new Point(0, 0), m_MultiPatternMatchingParameters, m_Owner?.GetIlluminationDataSet(), SelectedRow); }
            catch (Exception ex) { Debug.WriteLine("Train exception: " + ex.Message); }
            m_MultiPatternMatchingParameters.TrainImages[SelectedRow].Tag = ImageTag;
            UpdateTrainList();
        }

        private void UpdateTrainList()
        {
            if (_designMode) return;
            BindingSource bs = new BindingSource();
            if (m_MultiPatternMatchingParameters.TrainImages.Count > 0)
            {
                for (int i = 0; i < m_MultiPatternMatchingParameters.TrainImages.Count; i++)
                    if (m_MultiPatternMatchingParameters.TrainImages[i].Tag == null)
                        m_MultiPatternMatchingParameters.TrainImages[i].Tag = i.ToString();
            }
            bs.DataSource = m_MultiPatternMatchingParameters.TrainImages;
            baseListBoxTrainList.DisplayMember = "Tag";
            baseListBoxTrainList.DataSource = bs;
        }

        public void SwapList<T>(List<T> list, int from, int to)
        {
            if (from < 0 || to < 0) return;
            if (from >= list.Count || to >= list.Count) return;
            T tmp = list[from];
            list[from] = list[to];
            list[to] = tmp;
        }

        private void baseListBoxTrainList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_designMode) return;
            int curIndex = baseListBoxTrainList.SelectedIndex;
            if (curIndex > -1)
            {
                SelectedRow = curIndex;
                var vimg = m_MultiPatternMatchingParameters.TrainImages[SelectedRow];
                if (vimg != null)
                    pictureBoxTrainImage.SetImage(vimg);
            }
        }

        private void pictureBoxTrainImage_ImageChanged()
        {
            if (_designMode) return;
            int curIndex = baseListBoxTrainList.SelectedIndex;
            if (MultiImageChanged != null && curIndex >= 0)
                MultiImageChanged(curIndex, pictureBoxTrainImage.GetImage());
            MultiPatternMatchingParameters parameter = m_Owner?.GetPatternMatchingParameters();
            if (parameter != null && curIndex < parameter.TrainImages.Count && curIndex >= 0)
                parameter.TrainImages[curIndex] = pictureBoxTrainImage.GetImage();
        }

        public void SetTrainImage(VisionImage image)
        {
            if (_designMode) return;
            pictureBoxTrainImage.SetImage(image);
        }

        public void SetTrainList()
        {
            if (_designMode) return;
            if (m_MultiPatternMatchingParameters.TrainImages != null)
                UpdateTrainList();
        }

        public VisionImage GetTrainImage() => pictureBoxTrainImage.GetImage();

        public int SetPatternMatchingParam(MultiPatternMatchingParameters patternMatchingParam)
        {
            int ret = 0;
            if (patternMatchingParam != null)
            {
                m_MultiPatternMatchingParameters = patternMatchingParam;
                InitParamGroupControl(patternMatchingParam.GetParamGroup());
                if (!_designMode && patternMatchingParam.TrainImages.Count != 0)
                {
                    for (int i = 0; i < patternMatchingParam.TrainImages.Count; i++)
                        patternMatchingParam.TrainImages[i].Tag = i.ToString();
                }
                if (!_designMode) UpdateTrainList();
                if (!_designMode && baseListBoxTrainList.Items.Count > 0)
                    baseListBoxTrainList.SelectedIndex = 0;
                BindResultControl();
            }
            else
            {
                ret = -1;
            }
            return ret;
        }

        public MultiPatternMatchingParameters GetPatternMatchingParameters() => m_MultiPatternMatchingParameters;
    }
}
