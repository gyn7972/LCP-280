using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.Common.VisionPart;
using QMC.Common.Vision.Tools;
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
    public delegate void SearchButtonClickEventHandler();
    public partial class PatternMatchingResultControl : UserControl
    {
        public event SearchButtonClickEventHandler SearchButtonClick;

        private MultiPatternMatchingVisionPart _owner;
        private MultiPatternMatchingParameters _parameters;
        private VisionImageViewer _viewer;

        public PatternMatchingResultControl()
        {
            InitializeComponent();
            SetResultData(0,0,0);
        }

        public void Bind(MultiPatternMatchingVisionPart owner, MultiPatternMatchingParameters parameters, VisionImageViewer viewer = null)
        {
            _owner = owner;
            _parameters = parameters;
            if (viewer != null) _viewer = viewer;
        }

        public void SetBindingOwner(MultiPatternMatchingVisionPart owner) { _owner = owner; }
        public void SetBindingParameters(MultiPatternMatchingParameters parameters) { _parameters = parameters; }
        public void SetBindingViewer(VisionImageViewer viewer) { _viewer = viewer; }

        public void SetResultData(double dX, double dY, double dT)
        {
            {
                Param param = new Param();
                param.SetParam("Result X", Param.DisplayTypeKey.Text, dX, Param.ValueTypeKey.Double, "Search Result");
                this.paramTextControlResultX.InitControl(param);
                this.paramTextControlResultX.SetReadOnlyTextbox(true);
            }
            {
                Param param = new Param();
                param.SetParam("Result Y", Param.DisplayTypeKey.Text, dY, Param.ValueTypeKey.Double, "Search Result");
                this.paramTextControlResultY.InitControl(param);
                this.paramTextControlResultY.SetReadOnlyTextbox(true);
            }
            {
                Param param = new Param();
                param.SetParam("Result T", Param.DisplayTypeKey.Text, dT, Param.ValueTypeKey.Double, "Search Result");
                this.paramTextControlResultT.InitControl(param);
                this.paramTextControlResultT.SetReadOnlyTextbox(true);
            }
        }

        private void baseButtonSearch_Click(object sender, EventArgs e)
        {
            RunInternalSearchIfBindable();
            if (SearchButtonClick != null)
                SearchButtonClick();
        }

        private void RunInternalSearchIfBindable()
        {
            if (_owner == null || _parameters == null || _viewer == null)
                AutoBindIfPossible();

            if (_owner == null || _parameters == null)
                return;

            try
            {
                EnsureTestImage();
                Point start = new Point(0, 0);
                Point end = new Point(0, 0);
                try
                {
                    if (_owner.UseInspectRoi)
                    {
                        var ispStart = _owner.GetInspectStartPoint();
                        var ispEnd = _owner.GetInspectEndPoint();
                        if (ispEnd.X > ispStart.X && ispEnd.Y > ispStart.Y)
                        {
                            start = ispStart;
                            end = ispEnd;
                        }
                    }
                }
                catch { }

                _owner.OnSearch(start, end, _parameters, _owner.GetIlluminationDataSet());
                var result = _owner.GetResult();
                if (result == null) return;

                if (result.Values.Count > 0)
                    SetResultData(result.Values[0].X, result.Values[0].Y, result.Values[0].R);

                if (_viewer != null)
                {
                    try
                    {
                        _viewer.ResultOverlays.Clear();
                        foreach (var ov in result.ResultOverlays)
                        {
                            ov.Visible = true;
                            _viewer.ResultOverlays.Add(ov);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log.Write("PatternMatchingResultControl", "Internal search failed: " + ex.Message);
            }
            finally
            {
                try
                {
                    if (_owner?.Camera != null && _owner.Camera.IsLiveOn == false)
                        _owner.Camera.StartLive();
                }
                catch { }
            }
        }

        private void AutoBindIfPossible()
        {
            try
            {
                Control root = this; // 자신 포함해서 탐색 시작
                while (root != null)
                {
                    // 1) root 자신 검사
                    if ((_owner == null || _parameters == null) && root is PatternMatchingParamControl selfParam)
                    {
                        if (_owner == null) _owner = selfParam.m_Owner;
                        if (_parameters == null) _parameters = selfParam.GetPatternMatchingParameters();
                    }
                    if (_viewer == null && root is VisionImageViewer selfViewer)
                    {
                        _viewer = selfViewer;
                    }

                    // 2) 자식 컨트롤 검사
                    if ((_owner == null || _parameters == null) && root.Controls != null)
                    {
                        var paramCtrl = root.Controls.OfType<PatternMatchingParamControl>().FirstOrDefault();
                        if (paramCtrl != null)
                        {
                            if (_owner == null) _owner = paramCtrl.m_Owner;
                            if (_parameters == null) _parameters = paramCtrl.GetPatternMatchingParameters();
                        }
                    }
                    if (_viewer == null && root.Controls != null)
                    {
                        var viewer = root.Controls.OfType<VisionImageViewer>().FirstOrDefault();
                        if (viewer != null) _viewer = viewer;
                    }

                    if (_owner != null && _parameters != null && _viewer != null)
                        return;

                    root = root.Parent;
                }
            }
            catch { }
        }

        private void EnsureTestImage()
        {
            if (_owner == null) return;
            if (_owner.TestImage != null && _owner.TestImage.GetImage() != null) return;

            if (_viewer != null)
            {
                try
                {
                    if (_viewer.InputImage != null && _viewer.InputImage.GetImage() != null)
                    {
                        _owner.TestImage = _viewer.InputImage;
                        return;
                    }
                }
                catch { }

                try
                {
                    var cam = _viewer.Camera;
                    if (cam != null)
                    {
                        VisionImage vi = null;
                        if (cam.LatestImage != null && cam.LatestImage.GetImage() != null)
                        {
                            vi = cam.LatestImage;
                        }
                        else if (cam.Opened)
                        {
                            cam.GrabSync(out vi);
                        }
                        if (vi != null && vi.GetImage() != null)
                        {
                            _owner.TestImage = vi;
                            return;
                        }
                    }
                }
                catch { }
            }
        }
    }
}
