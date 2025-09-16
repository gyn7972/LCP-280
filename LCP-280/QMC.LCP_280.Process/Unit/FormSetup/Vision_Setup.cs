using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;

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
        private AxisPostionPopup _axisPosPopup = null;

        // Viewer popup 관리
        private Form _viewerPopupForm;
        private Control _viewerOriginalParent;
        private Rectangle _viewerOriginalBounds;
        private bool _viewerPoppedOut;
        private bool _restoringViewer;

        // 팝업 탭 및 동기화
        private TabControl _popupTabControl;
        private bool _syncingSelection;

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

            ResumeLayout(true);
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                WireAxisSelectionEvent();
                BinVisionList();
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
                    cameraListBoxItemsView.SelectedIndex = 0;
                    OnVisionItemSelected(cameraListBoxItemsView, 0);
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

            SwitchCameraTo(selectedIndex);

            if (_popupTabControl != null && _popupTabControl.IsHandleCreated)
            {
                try
                {
                    _syncingSelection = true;
                    if (selectedIndex >= 0 && selectedIndex < _popupTabControl.TabPages.Count)
                        _popupTabControl.SelectedIndex = selectedIndex;
                }
                finally { _syncingSelection = false; }
            }
        }

        private void SwitchCameraTo(int index)
        {
            try { visionImageViewer.CurrentCamera?.StopLive(); } catch { }

            visionImageViewer.SuspendDisplay();

            _camSwitch.Change(index);

            try
            {
                var cam = visionImageViewer.CurrentCamera;
                if (cam != null)
                {
                    visionImageViewer.Simulated = false;

                    if (!cam.Opened)
                    {
                        var rcRe = cam.Reconnect();
                        Log.Write("Vision_Setup", $"Camera.Reconnect '{cam.Name}' rc={rcRe}");
                      }

                    cam.SuspendedImageDisplay = false;

                    try { if (visionImageViewer.FrameRate > 0) cam.SetFrameRate(visionImageViewer.FrameRate); } catch { }

                    var rcLive = cam.StartLive();
                    Log.Write("Vision_Setup", $"Camera.StartLive '{cam.Name}' rc={rcLive}");
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"SwitchCameraTo StartLive error: {ex}");
            }

            // 스케일/크로스라인/스냅 초기화(스냅은 라이브 실패 대비 썸네일용)
            ResetViewerForCameraChange();

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
            ShowOrRestoreAxisPosPopup(this);
        }

        private void ShowOrRestoreJogPopup(IWin32Window owner)
        {
            if (_jogPopup == null || _jogPopup.IsDisposed)
            {
                _jogPopup = new Form_AxisJogPopup();
                _jogPopup.StartPosition = FormStartPosition.CenterParent;

                _jogPopup.ShowInTaskbar = true;
                _jogPopup.StartPosition = FormStartPosition.CenterScreen;

                _jogPopup.Owner = null;
                _jogPopup.Load += (s, e) =>
                {
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
                _jogPopup.Show();
            }

            if (_jogPopup.WindowState == FormWindowState.Minimized)
                _jogPopup.WindowState = FormWindowState.Normal;

            _jogPopup.BringToFront();
            _jogPopup.TopMost = true;
            _jogPopup.Activate();
        }

        private void ShowOrRestoreAxisPosPopup(IWin32Window owner)
        {
            if (_axisPosPopup == null || _axisPosPopup.IsDisposed)
            {
                _axisPosPopup = new AxisPostionPopup();
                _axisPosPopup.StartPosition = FormStartPosition.CenterParent;

                _axisPosPopup.ShowInTaskbar = true;
                _axisPosPopup.StartPosition = FormStartPosition.CenterScreen;

                _axisPosPopup.Owner = null;
                _axisPosPopup.Load += (s, e) =>
                {
                    TaskbarHelper.SetAppId(_axisPosPopup.Handle, "MyApp.AxisPosition");
                };

                _axisPosPopup.FormClosed += (s, _) => { _axisPosPopup = null; };
                _axisPosPopup.FormClosing += (s, ev) =>
                {
                    if (ev.CloseReason == CloseReason.UserClosing)
                    {
                        ev.Cancel = true;
                        _axisPosPopup.Hide();
                    }
                };
            }
            if (!_axisPosPopup.Visible)
            {
                _axisPosPopup.Show();
            }

            if (_axisPosPopup.WindowState == FormWindowState.Minimized)
                _axisPosPopup.WindowState = FormWindowState.Normal;

            _axisPosPopup.BringToFront();

            _axisPosPopup.TopMost = true;

            _axisPosPopup.Activate();
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
                ClientSize = new Size(1000, 700),
                ShowInTaskbar = false
            };
            _viewerPopupForm.FormClosed += (s, e) => { if (!_restoringViewer) RestoreViewer(); };

            _popupTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };
            var camNames = cameraListBoxItemsView?.GetItems() ?? _cameraNames?.ToArray() ?? Array.Empty<string>();
            if (camNames.Length == 0 && Equipment.Instance?.Cameras != null)
            {
                camNames = Equipment.Instance.Cameras.Keys.ToArray();
            }
            foreach (var name in camNames)
                _popupTabControl.TabPages.Add(new TabPage(string.IsNullOrWhiteSpace(name) ? "Camera" : name));

            // 현재 선택 인덱스 반영
            int selIndex = 0;
            if (cameraListBoxItemsView != null && cameraListBoxItemsView.SelectedIndex >= 0)
                selIndex = cameraListBoxItemsView.SelectedIndex;
            else if (_camSwitch != null && _camSwitch.SelectCameraIndex >= 0)
                selIndex = _camSwitch.SelectCameraIndex;
            selIndex = Math.Max(0, Math.Min(selIndex, _popupTabControl.TabPages.Count - 1));
            _popupTabControl.SelectedIndex = selIndex;

            _popupTabControl.SelectedIndexChanged -= PopupTabs_SelectedIndexChanged;
            _popupTabControl.SelectedIndexChanged += PopupTabs_SelectedIndexChanged;

            visionImageViewer.SuspendLayout();
            _viewerOriginalParent.Controls.Remove(visionImageViewer);
            var hostPage = _popupTabControl.TabPages[selIndex];
            visionImageViewer.Dock = DockStyle.Fill;
            hostPage.Controls.Add(visionImageViewer);
            visionImageViewer.ResumeLayout();

            _viewerPopupForm.Controls.Add(_popupTabControl);
            _viewerPoppedOut = true;
            _viewerPopupForm.Show(this);
            _viewerPopupForm.BringToFront();

            try
            {
                _syncingSelection = true;
                if (cameraListBoxItemsView != null)
                    cameraListBoxItemsView.SelectedIndex = selIndex;
            }
            finally { _syncingSelection = false; }
        }

        private void PopupTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_popupTabControl == null) return;
            int idx = _popupTabControl.SelectedIndex;
            if (idx < 0) return;

            if (_syncingSelection)
            {
                MoveViewerIntoTab(idx);
                return;
            }

            if (_camSwitch != null && idx < _camSwitch.Cameras.Count)
                SwitchCameraTo(idx);

            MoveViewerIntoTab(idx);

            if (cameraListBoxItemsView != null)
            {
                try
                {
                    _syncingSelection = true;
                    cameraListBoxItemsView.SelectedIndex = idx;
                }
                finally { _syncingSelection = false; }
            }
        }

        private void MoveViewerIntoTab(int idx)
        {
            try
            {
                var page = _popupTabControl.TabPages[idx];
                if (visionImageViewer.Parent != page)
                {
                    if (visionImageViewer.Parent != null)
                        visionImageViewer.Parent.Controls.Remove(visionImageViewer);

                    visionImageViewer.Dock = DockStyle.Fill;
                    page.Controls.Add(visionImageViewer);
                }
            }
            catch { }
        }

        private void RestoreViewer()
        {
            if (visionImageViewer == null || !_viewerPoppedOut) return;
            _restoringViewer = true;
            try
            {
                if (_popupTabControl != null)
                {
                    try
                    {
                        var host = visionImageViewer.Parent;
                        if (host != null) host.Controls.Remove(visionImageViewer);
                    }
                    catch { }
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

                if (_popupTabControl != null)
                {
                    try { _popupTabControl.SelectedIndexChanged -= PopupTabs_SelectedIndexChanged; } catch { }
                    try { _popupTabControl.Dispose(); } catch { }
                    _popupTabControl = null;
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
