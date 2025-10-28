using QMC.Common;
using QMC.Common.Vision;
using QMC.Common.Vision.Tools;
using QMC.Common.VisionPart;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common
{
    public enum SeletedROI
    {
        Train,
        Inspect,
        None,
    }
    public delegate void ROIButtonClickEventHandler();
    public delegate void TrainImageCapturedEventHandler(VisionImage image); // 신규: ROI Save 시 Train 이미지 캡쳐 이벤트
    public partial class MaintROIControl : UserControl
    {
        public event ROIButtonClickEventHandler ROIButtonClick;
        public event TrainImageCapturedEventHandler TrainImageCaptured; // 신규 이벤트
        private global::QMC.Common.VisionPart.VisionPart m_Owner; // generalized owner
        private SeletedROI m_selectROIMode;
        private Point m_TrainCenterposition;
        private Size m_TrainSize;
        private Point m_InspectCenterposition;
        private Size m_InspectSize;
        private int m_MoveToke;
        private int m_SizeToke;
        private Size m_FullSize;
        protected VisionImageViewer m_ImageViewer;
        protected RoiVisionTool m_SelectRoi;
        public MaintROIControl(global::QMC.Common.VisionPart.VisionPart visionPart)
        {
            InitializeComponent();
            m_Owner = visionPart;
            m_selectROIMode = SeletedROI.None;
            if (m_Owner != null && m_Owner.Camera != null)
            {
                m_FullSize = m_Owner.Camera.Resolution;
            }
            m_MoveToke = 10;
            m_SizeToke = 10;
            m_TrainCenterposition = new Point();
            m_TrainSize = new Size();
            m_InspectCenterposition = new Point();
            m_InspectSize = new Size();

            CalculateRoiParameter();
            CalculateCenterPosition();
            CalculateROISize();
            InitTrainParamGroupControl();

            baseButtonUp.BackColor = Color.Transparent;
            baseButtonDown.BackColor = Color.Transparent;
            baseButtonLeft.BackColor = Color.Transparent;
            baseButtonRight.BackColor = Color.Transparent;
            baseButtonCenter.BackColor = Color.Transparent;
            baseButtonXSizeUp.BackColor = Color.Transparent;
            baseButtonXSizeDown.BackColor = Color.Transparent;
            baseButtonYSizeUp.BackColor = Color.Transparent;
            baseButtonYSizeDown.BackColor = Color.Transparent;
            SetControlEnable(false);
            EnsureToggleAvailability();
        }
        public MaintROIControl() : this(null) { }

        // 유지: object 오버로드 (호출 호환성)
        public void SetOwner(object visionPart)
        {
            SetOwner(visionPart as global::QMC.Common.VisionPart.VisionPart);
        }

        public void SetOwner(global::QMC.Common.VisionPart.VisionPart visionPart)
        {
            if (visionPart == null)
            {
                baseToggleButtonTrain.Enabled = false;
                baseToggleButtonInspect.Enabled = false;
                SetControlEnable(false);
                return;
            }
            m_Owner = visionPart;
            if (m_Owner.Camera != null)
                m_FullSize = m_Owner.Camera.Resolution;
            CalculateCenterPosition();
            CalculateROISize();
            InitTrainParamGroupControl();
            InitInspectParamGroupControl();
            EnsureToggleAvailability();
        }

        // 외부(뷰어/카메라 선택)에서 이미지 크기 전달
        public void UpdateImageInfo(Size imageSize)
        {
            if (imageSize.Width <= 0 || imageSize.Height <= 0) return;
            m_FullSize = imageSize;

            // 처음 세팅 or 0 인 경우 기본값 부여
            if (m_TrainSize.IsEmpty || m_TrainSize.Width <= 0 || m_TrainSize.Height <= 0)
                m_TrainSize = imageSize;
            if (m_InspectSize.IsEmpty || m_InspectSize.Width <= 0 || m_InspectSize.Height <= 0)
                m_InspectSize = imageSize;
            if (m_TrainCenterposition.IsEmpty)
                m_TrainCenterposition = new Point(imageSize.Width / 2, imageSize.Height / 2);
            if (m_InspectCenterposition.IsEmpty)
                m_InspectCenterposition = new Point(imageSize.Width / 2, imageSize.Height / 2);

            InitTrainParamGroupControl();
            InitInspectParamGroupControl();
            EnsureToggleAvailability();
        }

        // ROI Tool 이 아직 초기화되지 않은 경우 기본 파라미터 적용 (호출 측에서 필요 시 호출)
        public void EnsureDefaultRoiTools()
        {
            if (m_Owner == null) return;
            try
            {
                var train = m_Owner.GetTrainRoi();
                if (train != null)
                {
                    if (train.Parameter.Size.Width <= 0 || train.Parameter.Size.Height <= 0)
                        train.Parameter.Size = m_TrainSize;
                    if (train.Parameter.CenterLocation.IsEmpty)
                        train.Parameter.CenterLocation = m_TrainCenterposition;
                }
            }
            catch { }
            try
            {
                var inspect = m_Owner.GetInspectRoi();
                if (inspect != null)
                {
                    if (inspect.Parameter.Size.Width <= 0 || inspect.Parameter.Size.Height <= 0)
                        inspect.Parameter.Size = m_InspectSize;
                    if (inspect.Parameter.CenterLocation.IsEmpty)
                        inspect.Parameter.CenterLocation = m_InspectCenterposition;
                }
            }
            catch { }
        }

        private void EnsureToggleAvailability()
        {
            if (m_Owner == null)
            {
                baseToggleButtonTrain.Enabled = false;
                baseToggleButtonInspect.Enabled = false;
                return;
            }
            bool canTrain = false;
            bool canInspect = false;
            try { canTrain = m_Owner.UseTrainRoi || m_Owner.GetTrainRoi() != null; } catch { }
            try { canInspect = m_Owner.UseInspectRoi || m_Owner.GetInspectRoi() != null; } catch { }
            if (!canTrain && !canInspect)
            {
                // 둘 다 정보 없음 -> 둘 다 허용 (사용자가 선택 후 ROI 생성할 수 있게)
                baseToggleButtonTrain.Enabled = true;
                baseToggleButtonInspect.Enabled = true;
            }
            else
            {
                baseToggleButtonTrain.Enabled = canTrain;
                baseToggleButtonInspect.Enabled = canInspect;
            }
        }

        private void CalculateRoiParameter() { }
        private void CalculateCenterPosition()
        {
            if (m_Owner == null) return;
            try
            {
                if (m_Owner.UseTrainRoi)
                {
                    var sp = m_Owner.GetTrainStartPoint();
                    var ep = m_Owner.GetTrainEndPoint();
                    m_TrainCenterposition = new Point(((ep.X - sp.X) / 2) + sp.X, ((ep.Y - sp.Y) / 2) + sp.Y);
                }
            }
            catch { }
            try
            {
                if (m_Owner.UseInspectRoi)
                {
                    var sp = m_Owner.GetInspectStartPoint();
                    var ep = m_Owner.GetInspectEndPoint();
                    m_InspectCenterposition = new Point(((ep.X - sp.X) / 2) + sp.X, ((ep.Y - sp.Y) / 2) + sp.Y);
                }
            }
            catch { }
        }

        private void CalculateROISize()
        {
            if (m_Owner == null) return;
            try
            {
                if (m_Owner.UseTrainRoi)
                {
                    var sp = m_Owner.GetTrainStartPoint();
                    var ep = m_Owner.GetTrainEndPoint();
                    m_TrainSize = new Size(ep.X - sp.X, ep.Y - sp.Y);
                }
            }
            catch { }
            try
            {
                if (m_Owner.UseInspectRoi)
                {
                    var sp = m_Owner.GetInspectStartPoint();
                    var ep = m_Owner.GetInspectEndPoint();
                    m_InspectSize = new Size(ep.X - sp.X, ep.Y - sp.Y);
                }
            }
            catch { }
        }

        private void CalculateTrainROIPosition(out Point startPosition, out Point endPosition)
        {
            startPosition = new Point((int)(m_TrainCenterposition.X - m_TrainSize.Width / 2), (int)(m_TrainCenterposition.Y - m_TrainSize.Height / 2));
            endPosition = new Point((int)(m_TrainCenterposition.X + m_TrainSize.Width / 2), (int)(m_TrainCenterposition.Y + m_TrainSize.Height / 2));
        }
        private void CalculateInspectROIPosition(out Point startPosition, out Point endPosition)
        {
            startPosition = new Point((int)(m_InspectCenterposition.X - m_InspectSize.Width / 2), (int)(m_InspectCenterposition.Y - m_InspectSize.Height / 2));
            endPosition = new Point((int)(m_InspectCenterposition.X + m_InspectSize.Width / 2), (int)(m_InspectCenterposition.Y + m_InspectSize.Height / 2));
        }

        private void UpdateROIInformation()
        {
            if (m_Owner == null) return;
            Point sp, ep;
            CalculateTrainROIPosition(out sp, out ep);
            try { m_Owner.SetTrainStartPoint(sp); m_Owner.SetTrainEndPoint(ep); } catch { }
            CalculateInspectROIPosition(out sp, out ep);
            try { m_Owner.SetInspectStartPoint(sp); m_Owner.SetInspectEndPoint(ep); } catch { }
        }

        private void InitTrainParamGroupControl()
        {
            Param param;
            param = new Param(); param.SetParam("Move Toke", Param.DisplayTypeKey.Text, m_MoveToke, Param.ValueTypeKey.Int, "ROI"); this.paramTextControlMoveToke.InitControl(param);
            param = new Param(); param.SetParam("Size Toke", Param.DisplayTypeKey.Text, m_SizeToke, Param.ValueTypeKey.Int, "ROI"); this.paramTextControlSizeToke.InitControl(param);
            param = new Param(); param.SetParam("Center Point", Param.DisplayTypeKey.Text, m_TrainCenterposition, Param.ValueTypeKey.Point, "ROI"); this.paramDualTextControlCenter.InitControl(param);
            param = new Param(); param.SetParam("ROI Size", Param.DisplayTypeKey.Text, m_TrainSize, Param.ValueTypeKey.Size, "ROI"); this.paramDualTextControlSize.InitControl(param);
        }
        private void InitInspectParamGroupControl()
        {
            Param param;
            param = new Param(); param.SetParam("Move Toke", Param.DisplayTypeKey.Text, m_MoveToke, Param.ValueTypeKey.Int, "ROI"); this.paramTextControlMoveToke.InitControl(param);
            param = new Param(); param.SetParam("Size Toke", Param.DisplayTypeKey.Text, m_SizeToke, Param.ValueTypeKey.Int, "ROI"); this.paramTextControlSizeToke.InitControl(param);
            param = new Param(); param.SetParam("Center Point", Param.DisplayTypeKey.Text, m_InspectCenterposition, Param.ValueTypeKey.Point, "ROI"); this.paramDualTextControlCenter.InitControl(param);
            param = new Param(); param.SetParam("ROI Size", Param.DisplayTypeKey.Text, m_InspectSize, Param.ValueTypeKey.Size, "ROI"); this.paramDualTextControlSize.InitControl(param);
        }

        private void SetParamGroupControlData()
        {
            Param p = this.paramTextControlMoveToke.GetParamData(); if (p != null) { int v = m_MoveToke; if (p.GetIntValue(ref v)) m_MoveToke = v; }
            p = this.paramTextControlSizeToke.GetParamData(); if (p != null) { int v = m_SizeToke; if (p.GetIntValue(ref v)) m_SizeToke = v; }
            if (m_selectROIMode == SeletedROI.Train)
            {
                p = this.paramDualTextControlCenter.GetParamData(); if (p != null) { Point pt = m_TrainCenterposition; if (p.GetPointValue(ref pt)) m_TrainCenterposition = pt; }
                p = this.paramDualTextControlSize.GetParamData(); if (p != null) { Size sz = m_TrainSize; if (p.GetSizeValue(ref sz)) m_TrainSize = sz; }
            }
            else if (m_selectROIMode == SeletedROI.Inspect)
            {
                p = this.paramDualTextControlCenter.GetParamData(); if (p != null) { Point pt = m_InspectCenterposition; if (p.GetPointValue(ref pt)) m_InspectCenterposition = pt; }
                p = this.paramDualTextControlSize.GetParamData(); if (p != null) { Size sz = m_InspectSize; if (p.GetSizeValue(ref sz)) m_InspectSize = sz; }
            }
        }

        private void baseToggleButtonTrain_Click(object sender, EventArgs e)
        {
            if (m_Owner == null) return;
            RoiVisionTool roi = null; try { roi = m_Owner.GetTrainRoi(); } catch { }
            bool isClicked = this.baseToggleButtonTrain.GetButtonStatus();
            if (isClicked)
            {
                m_selectROIMode = SeletedROI.None;
                baseToggleButtonTrain.UpdateToggleStatus(!isClicked);
                if (roi != null && m_ImageViewer != null) m_ImageViewer.NormalOverlays.Remove(roi.Parameter.Overlay);
                SetControlEnable(!isClicked);
            }
            else
            {
                m_selectROIMode = SeletedROI.Train;
                baseToggleButtonTrain.UpdateToggleStatus(!isClicked);
                baseToggleButtonInspect.UpdateToggleStatus(isClicked);
                if (roi != null && m_ImageViewer != null)
                {
                    if (m_SelectRoi != null) m_ImageViewer.NormalOverlays.Remove(m_SelectRoi.Parameter.Overlay);
                    roi.Parameter.CenterLocation = m_TrainCenterposition;
                    roi.Parameter.Size = m_TrainSize;
                    roi.Parameter.Overlay.Visible = true;
                    m_ImageViewer.NormalOverlays.Add(roi.Parameter.Overlay);
                    m_SelectRoi = roi;
                }
                SetControlEnable(!isClicked);
            }
            InitTrainParamGroupControl();
        }

        private void baseToggleButtonInspect_Click(object sender, EventArgs e)
        {
            if (m_Owner == null) return;
            RoiVisionTool roi = null; try { roi = m_Owner.GetInspectRoi(); } catch { }
            bool isClicked = this.baseToggleButtonInspect.GetButtonStatus();
            if (isClicked)
            {
                m_selectROIMode = SeletedROI.None;
                baseToggleButtonInspect.UpdateToggleStatus(!isClicked);
                if (roi != null && m_ImageViewer != null) m_ImageViewer.NormalOverlays.Remove(roi.Parameter.Overlay);
                SetControlEnable(!isClicked);
            }
            else
            {
                m_selectROIMode = SeletedROI.Inspect;
                baseToggleButtonInspect.UpdateToggleStatus(!isClicked);
                baseToggleButtonTrain.UpdateToggleStatus(isClicked);
                if (roi != null && m_ImageViewer != null)
                {
                    if (m_SelectRoi != null) m_ImageViewer.NormalOverlays.Remove(m_SelectRoi.Parameter.Overlay);
                    roi.Parameter.CenterLocation = m_InspectCenterposition;
                    roi.Parameter.Size = m_InspectSize;
                    roi.Parameter.Overlay.Visible = true;
                    m_ImageViewer.NormalOverlays.Add(roi.Parameter.Overlay);
                    m_SelectRoi = roi;
                }
                SetControlEnable(!isClicked);
            }
            InitInspectParamGroupControl();
        }

        private void baseButtonLocation_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            SetParamGroupControlData();
            if (m_SelectRoi == null) return;
            if (m_selectROIMode == SeletedROI.Train)
            {
                if (btn == baseButtonCenter) m_TrainCenterposition = new Point(m_FullSize.Width / 2, m_FullSize.Height / 2);
                else if (btn == baseButtonRight) m_TrainCenterposition.X += m_MoveToke;
                else if (btn == baseButtonLeft) m_TrainCenterposition.X -= m_MoveToke;
                else if (btn == baseButtonDown) m_TrainCenterposition.Y += m_MoveToke;
                else if (btn == baseButtonUp) m_TrainCenterposition.Y -= m_MoveToke;
                m_SelectRoi.Parameter.CenterLocation = m_TrainCenterposition;
                InitTrainParamGroupControl();
            }
            else if (m_selectROIMode == SeletedROI.Inspect)
            {
                if (btn == baseButtonCenter) m_InspectCenterposition = new Point(m_FullSize.Width / 2, m_FullSize.Height / 2);
                else if (btn == baseButtonRight) m_InspectCenterposition.X += m_MoveToke;
                else if (btn == baseButtonLeft) m_InspectCenterposition.X -= m_MoveToke;
                else if (btn == baseButtonDown) m_InspectCenterposition.Y += m_MoveToke;
                else if (btn == baseButtonUp) m_InspectCenterposition.Y -= m_MoveToke;
                m_SelectRoi.Parameter.CenterLocation = m_InspectCenterposition;
                InitInspectParamGroupControl();
            }
            ROIButtonClick?.Invoke();
        }

        private void baseButtonSize_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            SetParamGroupControlData();
            if (m_SelectRoi == null) return;
            if (m_selectROIMode == SeletedROI.Train)
            {
                if (btn == baseButtonXSizeUp) { m_TrainSize.Width += m_SizeToke; m_TrainCenterposition.X += m_SizeToke / 2; }
                else if (btn == baseButtonXSizeDown) { m_TrainSize.Width -= m_SizeToke; m_TrainCenterposition.X -= m_SizeToke / 2; }
                else if (btn == baseButtonYSizeUp) { m_TrainSize.Height += m_SizeToke; m_TrainCenterposition.Y += m_SizeToke / 2; }
                else if (btn == baseButtonYSizeDown) { m_TrainSize.Height -= m_SizeToke; m_TrainCenterposition.Y -= m_SizeToke / 2; }
                else if (btn == baseButtonFullSize) { m_TrainSize = m_FullSize; m_TrainCenterposition = new Point(m_FullSize.Width / 2, m_FullSize.Height / 2); }
                m_SelectRoi.Parameter.Size = m_TrainSize;
                InitTrainParamGroupControl();
            }
            else if (m_selectROIMode == SeletedROI.Inspect)
            {
                if (btn == baseButtonXSizeUp) { m_InspectSize.Width += m_SizeToke; m_InspectCenterposition.X += m_SizeToke / 2; }
                else if (btn == baseButtonXSizeDown) { m_InspectSize.Width -= m_SizeToke; m_InspectCenterposition.X -= m_SizeToke / 2; }
                else if (btn == baseButtonYSizeUp) { m_InspectSize.Height += m_SizeToke; m_InspectCenterposition.Y += m_SizeToke / 2; }
                else if (btn == baseButtonYSizeDown) { m_InspectSize.Height -= m_SizeToke; m_InspectCenterposition.Y -= m_SizeToke / 2; }
                else if (btn == baseButtonFullSize) { m_InspectSize = m_FullSize; m_InspectCenterposition = new Point(m_FullSize.Width / 2, m_FullSize.Height / 2); }
                m_SelectRoi.Parameter.Size = m_InspectSize;
                InitInspectParamGroupControl();
            }
            ROIButtonClick?.Invoke();
        }

        // 신규: 소스 이미지 확보 통합 메서드
        private VisionImage AcquireSourceImage()
        {
            VisionImage src = null;
            try
            {
                // 1) 카메라 Latest 우선 (Grab 부하 줄이기)
                var cam = m_Owner?.Camera;
                if (cam != null && cam.Opened && cam.LatestImage != null && cam.LatestImage.RawData != null)
                {
                    src = cam.LatestImage;
                }
                // 2) 필요 시 동기 Grab
                if (src == null && cam != null && cam.Opened)
                {
                    try { cam.GrabSync(out src); } catch (Exception ex) { Log.Write("ROI", "GrabSync 실패: " + ex.Message); }
                }
                // 3) VisionPart TestImage
                if (src == null)
                {
                    try
                    {
                        var mp = m_Owner as MultiPatternMatchingVisionPart;
                        if (mp != null && mp.TestImage != null && mp.TestImage.RawData != null)
                            src = mp.TestImage;
                    }
                    catch (Exception ex) { Log.Write("ROI", "TestImage 접근 실패: " + ex.Message); }
                }
                // 4) Viewer InputImage (PictureBox.Image 대신 RawData 존재 여부)
                if (src == null && m_ImageViewer != null && m_ImageViewer.InputImage != null && m_ImageViewer.InputImage.RawData != null)
                {
                    src = m_ImageViewer.InputImage;
                }
                // 5) Viewer.Image (비트맵 -> VisionImage 변환)
                if (src == null && m_ImageViewer != null && m_ImageViewer.Image != null)
                {
                    try { src = VisionImage.CreateInstance((Image)m_ImageViewer.Image.Clone()); } catch (Exception ex) { Log.Write("ROI", "Viewer.Image Clone 실패: " + ex.Message); }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ROI", "AcquireSourceImage 예외: " + ex.Message);
            }
            return src;
        }

        private Rectangle ClampRoi(Point start, Point end, VisionImage src)
        {
            if (src == null || src.Header == null) return Rectangle.Empty;
            if (end.X < start.X) { int t = start.X; start.X = end.X; end.X = t; }
            if (end.Y < start.Y) { int t = start.Y; start.Y = end.Y; end.Y = t; }
            int w = end.X - start.X;
            int h = end.Y - start.Y;
            if (w <= 0 || h <= 0) return Rectangle.Empty;
            int maxW = src.Header.Width;
            int maxH = src.Header.Height;
            if (start.X < 0) start.X = 0;
            if (start.Y < 0) start.Y = 0;
            if (start.X + w > maxW) w = maxW - start.X;
            if (start.Y + h > maxH) h = maxH - start.Y;
            if (w <= 0 || h <= 0) return Rectangle.Empty;
            return new Rectangle(start.X, start.Y, w, h);
        }

        private void baseButtonSave_Click(object sender, EventArgs e)
        {
            if (m_Owner == null) return;
            try
            {
                // 1) ROI 좌표 반영
                UpdateROIInformation();

                // 2) Train ROI 선택상태이거나 Train ROI 데이터가 있는 경우 Train 이미지 캡쳐 시도
                if (m_Owner.UseTrainRoi || m_selectROIMode == SeletedROI.Train)
                {
                    var start = m_Owner.GetTrainStartPoint();
                    var end = m_Owner.GetTrainEndPoint();
                    if (end.X > start.X && end.Y > start.Y)
                    {
                        VisionImage src = AcquireSourceImage();
                        if (src == null || src.RawData == null)
                        {
                            Log.Write("ROI", "소스 이미지 없음. Train 캡쳐 중단");
                            return;
                        }

                        // ROI Clamp
                        var rect = ClampRoi(start, end, src);
                        if (rect == Rectangle.Empty)
                        {
                            Log.Write("ROI", $"ROI Clamp 실패 start=({start.X},{start.Y}) end=({end.X},{end.Y})");
                            return;
                        }
                        VisionImage cut = null;
                        try
                        {
                            cut = src.CutVisionImage(rect);
                        }
                        catch (Exception ex)
                        {
                            Log.Write("ROI", "CutVisionImage 예외: " + ex.Message);
                        }
                        if (cut != null && cut.RawData != null && cut.Header.Width > 0)
                        {
                            cut.Tag = $"Train_{DateTime.Now:HHmmss}";
                            TrainImageCaptured?.Invoke(cut); // 부모 다이얼로그에서 리스트에 추가
                        }
                        else
                        {
                            Log.Write("ROI", "잘라낸 이미지가 유효하지 않음");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("ROI", "baseButtonSave_Click 예외: " + ex.Message);
            }
        }

        public void SetImageviwer(VisionImageViewer imageViewer) => m_ImageViewer = imageViewer;

        private void baseButtonRollBack_Click(object sender, EventArgs e)
        {
            if (m_Owner == null) return;
            RollBackTrainROI();
            RollBackInspectROI();
        }

        private void RollBackTrainROI()
        {
            if (m_Owner == null) return;
            try
            {
                var sp = m_Owner.GetTrainStartPoint();
                var ep = m_Owner.GetTrainEndPoint();
                m_TrainCenterposition = new Point(((ep.X - sp.X) / 2) + sp.X, ((ep.Y - sp.Y) / 2) + sp.Y);
                m_TrainSize = new Size(ep.X - sp.X, ep.Y - sp.Y);
                var roi = m_Owner.GetTrainRoi();
                if (roi != null)
                {
                    roi.Parameter.CenterLocation = m_TrainCenterposition;
                    roi.Parameter.Size = m_TrainSize;
                }
                if (m_selectROIMode == SeletedROI.Train) InitTrainParamGroupControl();
            }
            catch { }
        }

        private void RollBackInspectROI()
        {
            if (m_Owner == null) return;
            try
            {
                var sp = m_Owner.GetInspectStartPoint();
                var ep = m_Owner.GetInspectEndPoint();
                m_InspectCenterposition = new Point(((ep.X - sp.X) / 2) + sp.X, ((ep.Y - sp.Y) / 2) + sp.Y);
                m_InspectSize = new Size(ep.X - sp.X, ep.Y - sp.Y);
                var roi = m_Owner.GetInspectRoi();
                if (roi != null)
                {
                    roi.Parameter.CenterLocation = m_InspectCenterposition;
                    roi.Parameter.Size = m_InspectSize;
                }
                if (m_selectROIMode == SeletedROI.Inspect) InitInspectParamGroupControl();
            }
            catch { }
        }

        private void SetControlEnable(bool bEnable)
        {
            this.baseButtonCenter.Enabled = bEnable;
            this.baseButtonDown.Enabled = bEnable;
            this.baseButtonFullSize.Enabled = bEnable;
            this.baseButtonLeft.Enabled = bEnable;
            this.baseButtonRight.Enabled = bEnable;
            this.baseButtonRollBack.Enabled = bEnable;
            this.baseButtonSave.Enabled = bEnable;
            this.baseButtonUp.Enabled = bEnable;
            this.baseButtonXSizeDown.Enabled = bEnable;
            this.baseButtonXSizeUp.Enabled = bEnable;
            this.baseButtonYSizeDown.Enabled = bEnable;
            this.baseButtonYSizeUp.Enabled = bEnable;
            this.paramDualTextControlCenter.Enabled = bEnable;
            this.paramDualTextControlSize.Enabled = bEnable;
            this.paramTextControlMoveToke.Enabled = bEnable;
            this.paramTextControlSizeToke.Enabled = bEnable;
        }

        public void CommitCurrentRoi()
        {
            if (m_Owner == null) return;
            try
            {
                // 우선 현재 ROI Tool 파라미터에서 직접 좌표를 읽는다 (내부 캐시 불일치 방지)
                try
                {
                    var troi = m_Owner.GetTrainRoi();
                    if (troi != null && troi.Parameter != null)
                    {
                        var p = troi.Parameter;
                        // 내부 캐시 업데이트
                        m_TrainCenterposition = p.CenterLocation;
                        m_TrainSize = p.Size;
                        // VisionPart 저장
                        m_Owner.SetTrainStartPoint(p.StartLocation);
                        m_Owner.SetTrainEndPoint(p.EndLocation);
                    }
                }
                catch { }
                try
                {
                    var iroi = m_Owner.GetInspectRoi();
                    if (iroi != null && iroi.Parameter != null)
                    {
                        var p = iroi.Parameter;
                        m_InspectCenterposition = p.CenterLocation;
                        m_InspectSize = p.Size;
                        m_Owner.SetInspectStartPoint(p.StartLocation);
                        m_Owner.SetInspectEndPoint(p.EndLocation);
                    }
                }
                catch { }
            }
            catch { }
            // 보강: 내부 계산 로직도 수행 (예전 방식 필요 시)
            try { UpdateROIInformation(); } catch { }
        }

        public void ReloadRoiFromPart()
        {
            if (m_Owner == null) return;
            try
            {
                // Train
                var tsp = m_Owner.GetTrainStartPoint();
                var tep = m_Owner.GetTrainEndPoint();
                if (tep.X > tsp.X && tep.Y > tsp.Y)
                {
                    m_TrainSize = new Size(tep.X - tsp.X, tep.Y - tsp.Y);
                    m_TrainCenterposition = new Point(tsp.X + m_TrainSize.Width / 2, tsp.Y + m_TrainSize.Height / 2);
                    var troi = m_Owner.GetTrainRoi();
                    if (troi != null)
                    {
                        troi.Parameter.StartLocation = tsp;
                        troi.Parameter.EndLocation = tep;
                        troi.Parameter.Size = m_TrainSize; // ensures overlay update
                        troi.Parameter.CenterLocation = m_TrainCenterposition;
                    }
                }
            }
            catch { }
            try
            {
                // Inspect
                var isp = m_Owner.GetInspectStartPoint();
                var iep = m_Owner.GetInspectEndPoint();
                if (iep.X > isp.X && iep.Y > isp.Y)
                {
                    m_InspectSize = new Size(iep.X - isp.X, iep.Y - isp.Y);
                    m_InspectCenterposition = new Point(isp.X + m_InspectSize.Width / 2, isp.Y + m_InspectSize.Height / 2);
                    var iroi = m_Owner.GetInspectRoi();
                    if (iroi != null)
                    {
                        iroi.Parameter.StartLocation = isp;
                        iroi.Parameter.EndLocation = iep;
                        iroi.Parameter.Size = m_InspectSize;
                        iroi.Parameter.CenterLocation = m_InspectCenterposition;
                    }
                }
            }
            catch { }
            // Refresh parameter UI depending on current mode
            try
            {
                if (m_selectROIMode == SeletedROI.Train) InitTrainParamGroupControl();
                else if (m_selectROIMode == SeletedROI.Inspect) InitInspectParamGroupControl();
            }
            catch { }
        }
    }
}
