using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Vision_Setup : Form
    {
        private readonly Equipment equipment = Equipment.Instance;

        // Vision 관련 필드
        private CameraSwitch _camSwitch;
        private List<string> _cameraNames;

        // Jog Popup
        private Form_AxisJogPopup _jogPopup = null;

        // Viewer popup 관리
        private Form _viewerPopupForm;
        private Control _viewerOriginalParent;
        private Rectangle _viewerOriginalBounds;
        private bool _viewerPoppedOut;
        private bool _restoringViewer;

        // Property 인덱스 (필요시 확장)
        private Dictionary<(string section, string title), PropertyBase> _configIndex;
        private Dictionary<(string section, string title), PropertyBase> _speedIndex;

        public Vision_Setup()
        {
            InitializeComponent();
            SuspendLayout();
            InitializeUI();

            if (visionImageViewer != null)
            {
                _viewerOriginalParent = visionImageViewer.Parent;
                _viewerOriginalBounds = new Rectangle(visionImageViewer.Location, visionImageViewer.Size);
                visionImageViewer.DoubleClick -= VisionImageViewer_DoubleClick;
                visionImageViewer.DoubleClick += VisionImageViewer_DoubleClick;
            }

            //_jogPopup = new Form_AxisJogPopup();
            ResumeLayout(true);
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                BinVisionList();
                WireAxisSelectionEvent();
                InitializeRadioButtonView();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        // ===== Camera List Binding =====
        private void BinVisionList()
        {
            try
            {
                _cameraNames = Equipment.Instance.Cameras.Keys.ToList();
                cameraListBoxItemsView.SetItems(_cameraNames.ToArray());

                _camSwitch = new CameraSwitch();
                foreach (var cam in Equipment.Instance.Cameras.Values)
                    _camSwitch.Cameras.Add(cam);

                visionImageViewer.CameraSwitch = _camSwitch;
                visionImageViewer.FrameRate = 30;

                if (_camSwitch.Cameras.Count > 0)
                {
                    _camSwitch.Change(0);
                    cameraListBoxItemsView.SelectedIndex = 0;
                    ResetViewerForCameraChange();
                    visionImageViewer.ResumeDisplay();
                }
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"BindVisionList error: {ex}");
                cameraListBoxItemsView?.SetItems();
            }
        }

        // ===== Event Wiring =====
        private void WireAxisSelectionEvent()
        {
            if (cameraListBoxItemsView == null) return;
            cameraListBoxItemsView.ItemSelected -= OnVisionItemSelected;
            cameraListBoxItemsView.ItemSelected += OnVisionItemSelected;
        }

        private void OnVisionItemSelected(object sender, int selectedIndex)
        {
            if (_camSwitch == null) return;
            if (selectedIndex < 0 || selectedIndex >= _camSwitch.Cameras.Count) return;
            try { visionImageViewer.CurrentCamera?.StopLive(); } catch { }
            visionImageViewer.SuspendDisplay();
            _camSwitch.Change(selectedIndex);
            ResetViewerForCameraChange();
            try { visionImageViewer.CurrentCamera?.StartLive(); }
            catch (Exception ex) { Log.Write(ex); }
            visionImageViewer.ResumeDisplay();
            visionImageViewer.StartUpdateTask();
        }

        private void ResetViewerForCameraChange()
        {
            var cam = visionImageViewer.CurrentCamera;
            if (cam == null) return;
            visionImageViewer.Scale.Wheel = 1.0;
            visionImageViewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
            visionImageViewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
            visionImageViewer.InitCrossLine();
            visionImageViewer.ShowCrossLine(visionImageViewer.VisibleCrossLine);
            cam.GrabSync(out var snap);
            if (snap != null) visionImageViewer.SetImageNDisplay(snap);
        }

        // ===== Jog Popup =====
        private void btn_JogPopup_Click(object sender, EventArgs e)
        {
            ShowOrRestoreJogPopup(this);
        }

        private void ShowOrRestoreJogPopup(IWin32Window owner)
        {
            //return;
            if (_jogPopup == null || _jogPopup.IsDisposed)
            {
                _jogPopup = new Form_AxisJogPopup();
                _jogPopup.StartPosition = FormStartPosition.CenterParent;
                //_jogPopup.ShowInTaskbar = false;

                // ✅ 별도 아이콘 나오게
                _jogPopup.ShowInTaskbar = true;
                _jogPopup.StartPosition = FormStartPosition.CenterScreen;

                // ✅ Owner 관계 제거 (메인창과 독립)
                _jogPopup.Owner = null;
                _jogPopup.Load += (s, e) =>
                {
                    // 메인폼과 다른 AppID 부여
                    TaskbarHelper.SetAppId(_jogPopup.Handle, "MyApp.JogPanel");
                };


                _jogPopup.FormClosed += (s, _) => { _jogPopup = null; };
                _jogPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing) 
                    { 
                        ev.Cancel = true; 
                        _jogPopup.Hide(); 
                    }
                };
            }
            if (!_jogPopup.Visible)
            {
                //_jogPopup.Show(owner);
                _jogPopup.Show();
            }
                
            if (_jogPopup.WindowState == FormWindowState.Minimized) 
                _jogPopup.WindowState = FormWindowState.Normal;

            _jogPopup.BringToFront();
            //_jogPopup.TopMost = true; 
            //_jogPopup.TopMost = false;
            _jogPopup.Activate();
        }

        // ===== Viewer Popup (Double-click) =====
        private void VisionImageViewer_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (visionImageViewer == null || _restoringViewer) return;
                if (!_viewerPoppedOut) PopOutViewer(); else RestoreViewer();
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", "Viewer popup toggle error: " + ex.Message);
            }
        }

        private void PopOutViewer()
        {
            if (visionImageViewer == null || _viewerPoppedOut) return;
            _viewerOriginalParent = visionImageViewer.Parent;
            _viewerOriginalBounds = new Rectangle(visionImageViewer.Location, visionImageViewer.Size);
            _viewerPopupForm = new Form
            {
                Text = "Camera View",
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(800, 600),
                ShowInTaskbar = false
            };
            _viewerPopupForm.FormClosed += (s, e) => { if (!_restoringViewer) RestoreViewer(); };
            visionImageViewer.SuspendLayout();
            _viewerOriginalParent.Controls.Remove(visionImageViewer);
            visionImageViewer.Dock = DockStyle.Fill;
            _viewerPopupForm.Controls.Add(visionImageViewer);
            visionImageViewer.ResumeLayout();
            _viewerPoppedOut = true;
            _viewerPopupForm.Show(this);
            _viewerPopupForm.BringToFront();
        }

        private void RestoreViewer()
        {
            if (visionImageViewer == null || !_viewerPoppedOut) return;
            _restoringViewer = true;
            try
            {
                if (_viewerPopupForm != null && !_viewerPopupForm.IsDisposed)
                {
                    try { _viewerPopupForm.Controls.Remove(visionImageViewer); } catch { }
                }
                if (_viewerOriginalParent != null && !_viewerOriginalParent.IsDisposed)
                {
                    visionImageViewer.SuspendLayout();
                    visionImageViewer.Dock = DockStyle.None;
                    visionImageViewer.Location = _viewerOriginalBounds.Location;
                    visionImageViewer.Size = _viewerOriginalBounds.Size;
                    _viewerOriginalParent.Controls.Add(visionImageViewer);
                    visionImageViewer.ResumeLayout();
                    visionImageViewer.BringToFront();
                }
                if (_viewerPopupForm != null)
                {
                    try { if (!_viewerPopupForm.IsDisposed) _viewerPopupForm.Close(); } catch { }
                    try { _viewerPopupForm.Dispose(); } catch { }
                }
            }
            finally
            {
                _viewerPopupForm = null;
                _viewerPoppedOut = false;
                _restoringViewer = false;
            }
        }

        // ===== Save Button (Stub) =====
        private void btn_Save_Setup_Cylinder_Click(object sender, EventArgs e)
        {
            try { /* TODO: Save implementation */ }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== Index Utilities =====
        private static Dictionary<(string section, string title), PropertyBase> BuildIndex(PropertyCollection pc)
        {
            var map = new Dictionary<(string section, string title), PropertyBase>(StringTupleComparer.OrdinalIgnoreCase);
            if (pc == null || pc.Count == 0) return map;
            string currentSection = string.Empty;
            foreach (var p in pc)
            {
                if (p == null) continue;
                if (p is TitleOnlyProperty)
                {
                    currentSection = GetName(p) ?? string.Empty;
                    continue;
                }
                var title = GetName(p);
                if (string.IsNullOrEmpty(title)) continue;
                var key = (currentSection, title);
                if (!map.ContainsKey(key)) map[key] = p;
            }
            return map;
        }

        private PropertyBase Find(string section, string title)
        {
            if (_configIndex != null && _configIndex.TryGetValue((section ?? string.Empty, title), out var p1)) return p1;
            return null;
        }
        private PropertyBase FindS(string section, string title)
        {
            if (_speedIndex != null && _speedIndex.TryGetValue((section ?? string.Empty, title), out var p1)) return p1;
            return null;
        }
        private double GetDouble(string section, string title, double fallback) => ReadDouble(Find(section, title), fallback);
        private double GetDoubleS(string section, string title, double fallback) => ReadDouble(FindS(section, title), fallback);
        private bool GetBool(string section, string title, bool fallback) => ReadBool(Find(section, title), fallback);
        private int GetInt(string section, string title, int fallback) => ReadInt(Find(section, title), fallback);
        private int GetIntS(string section, string title, int fallback) => ReadInt(FindS(section, title), fallback);

        private static double ReadDouble(PropertyBase p, double fallback)
        {
            if (p == null) return fallback;
            if (p is DoubleProperty dp) return dp.Value;
            if (p is BoolProperty bp) return bp.Value ? 1.0 : 0.0;
            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture); } catch { }
                }
            }
            return fallback;
        }
        private static bool ReadBool(PropertyBase p, bool fallback)
        {
            if (p == null) return fallback;
            if (p is BoolProperty bp) return bp.Value;
            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is bool b) return b;
                if (v is IConvertible)
                {
                    try { return Convert.ToDouble(v, System.Globalization.CultureInfo.InvariantCulture) != 0.0; } catch { }
                }
            }
            return fallback;
        }
        private static int ReadInt(PropertyBase p, int fallback)
        {
            if (p == null) return fallback;
            switch (p)
            {
                case IntProperty ip: return ip.Value;
                case LongProperty lp: try { checked { return (int)lp.Value; } } catch { return fallback; }
                case FloatProperty fp: return (int)Math.Round(fp.Value);
                case DoubleProperty dp: return (int)Math.Round(dp.Value);
                case BoolProperty bp: return bp.Value ? 1 : 0;
                case StringProperty sp:
                    if (int.TryParse(sp.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var i)) return i;
                    if (double.TryParse(sp.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d)) return (int)Math.Round(d);
                    return fallback;
            }
            var vProp = p.GetType().GetProperty("Value");
            if (vProp != null)
            {
                var v = vProp.GetValue(p);
                if (v is int i) return i;
                if (v is long l) { try { checked { return (int)l; } } catch { return fallback; } }
                if (v is float f) return (int)Math.Round(f);
                if (v is double d) return (int)Math.Round(d);
                if (v is bool b) return b ? 1 : 0;
                if (v is string s)
                {
                    if (int.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var i2)) return i2;
                    if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d2)) return (int)Math.Round(d2);
                }
                if (v is IConvertible)
                {
                    try { return Convert.ToInt32(v, System.Globalization.CultureInfo.InvariantCulture); } catch { }
                }
            }
            return fallback;
        }
        private static string GetName(PropertyBase p)
        {
            if (p == null) return null;
            var nameProp = p.GetType().GetProperty("Name");
            var titleProp = p.GetType().GetProperty("Title");
            return nameProp?.GetValue(p)?.ToString() ?? titleProp?.GetValue(p)?.ToString();
        }
        private sealed class StringTupleComparer : IEqualityComparer<(string section, string title)>
        {
            public static readonly StringTupleComparer OrdinalIgnoreCase = new StringTupleComparer();
            private readonly StringComparer _cmp = StringComparer.OrdinalIgnoreCase;
            public bool Equals((string section, string title) x, (string section, string title) y) => _cmp.Equals(x.section, y.section) && _cmp.Equals(x.title, y.title);
            public int GetHashCode((string section, string title) obj) => HashCode.Combine(_cmp.GetHashCode(obj.section ?? string.Empty), _cmp.GetHashCode(obj.title ?? string.Empty));
        }

        // ===== 기타 Stub =====
        private void InitializeRadioButtonView() { }

        // ===== Paint / Resize =====
        protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); }
        protected override void OnResize(EventArgs e) { base.OnResize(e); this.Invalidate(); }

    }
}
