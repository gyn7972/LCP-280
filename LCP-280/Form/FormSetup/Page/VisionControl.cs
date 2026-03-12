using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PropertyCollection = QMC.Common.PropertyCollection;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    public partial class VisionControl : UserControl
    {
        #region Camera
        private ConfigReflectionMapper _cameraConfigMapper;

        private HIKGigECamera _selectedCamera;
        private CameraSwitch _camSwitch;
        private List<string> _cameraNames;
        #endregion

        // Jog Popup
        private Form_AxisJogPopup _jogPopup = null;
        private AxisPostionPopup _axisPosPopup = null;

        // Viewer popup 관리
        private Form _viewerPopupForm;
        private Control _viewerOriginalParent;
        private Rectangle _viewerOriginalBounds;
        private bool _viewerPoppedOut;
        private bool _restoringViewer;

        // 팝업 탭 및 동기화 추가
        private TabControl _popupTabControl;
        private bool _syncingSelection;

        // Property 인덱스 (필요시 확장)
        private Dictionary<(string section, string title), PropertyBase> _configIndex;
        private Dictionary<(string section, string title), PropertyBase> _speedIndex;

        public VisionControl()
        {
            InitializeComponent();
            SuspendLayout();

            InitializeUI();

            if (visionImageViewer != null)
            {
                _viewerOriginalParent = visionImageViewer.Parent;
                _viewerOriginalBounds = new Rectangle(visionImageViewer.Location, visionImageViewer.Size);
            }

            ResumeLayout(true);
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                BinVisionList();
                WriteEvents_Barcder();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        #region Camera Binding
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
                    // 기존 주석 해제: 초기 카메라에서도 크로스라인 재초기화
                    ResetViewerForCameraChange(0);
                    visionImageViewer.ResumeDisplay();
                }
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"BindVisionList error: {ex}");
                cameraListBoxItemsView?.SetItems();
            }
        }
        #endregion

        #region Event Handlers
        // ===== Event Wiring =====
        private void WriteEvents_Barcder()
        {
            if (cameraListBoxItemsView != null)
            {
                cameraListBoxItemsView.ItemSelected -= OnVisionItemSelected;
                cameraListBoxItemsView.ItemSelected += OnVisionItemSelected;
            }

            if (visionImageViewer != null)
            {
                visionImageViewer.DoubleClick -= VisionImageViewer_DoubleClick;
                visionImageViewer.DoubleClick += VisionImageViewer_DoubleClick;
            }

            if (btn_Save_Camera_Setup != null)
            {
                btn_Save_Camera_Setup.Click -= btn_Save_Camera_Setup_Click;
                btn_Save_Camera_Setup.Click += btn_Save_Camera_Setup_Click;
            }

            if (btn_JogPopup != null)
            {
                btn_JogPopup.Click -= btn_JogPopup_Click;
                btn_JogPopup.Click += btn_JogPopup_Click;
            }
        }
        #endregion

        #region Button Click Handlers
        private void OnVisionItemSelected(object sender, int selectedIndex)
        {
            if (_syncingSelection)
            {
                SyncPopupTab(selectedIndex);
                return;
            }

            if (_camSwitch == null) 
                return;

            if (selectedIndex < 0 || selectedIndex >= _camSwitch.Cameras.Count) 
                return;

            var cameraName = _cameraNames[selectedIndex];
            var newCamera = Equipment.Instance.Cameras[cameraName];
            // [수정] 방어 코드 추가: 현재 이미 선택된 카메라와 동일하면 로직 수행 중단
            if (_selectedCamera == newCamera)
            {
                return;
            }
            // 카메라 변경 로직 시작
            _selectedCamera = newCamera;
            try 
            { 
                _selectedCamera.StopLive(); 
            } 
            catch { }

            // 기존 뷰 크로스라인/오버레이 완전 정리
            PrepareViewerForCameraSwitch();

            visionImageViewer.SuspendDisplay();
            _camSwitch.Change(selectedIndex);

            try
            {
                if (_selectedCamera != null)
                {
                    visionImageViewer.Simulated = false;
                    _selectedCamera.SuspendedImageDisplay = false;
                    var rcLive = _selectedCamera.StartLive();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }

            // 새 카메라 기준 크로스라인 재구성
            ResetViewerForCameraChange(selectedIndex);
            visionImageViewer.ResumeDisplay();
            visionImageViewer.StartUpdateTask();

            LoadCameraProperties(selectedIndex);
            SyncPopupTab(selectedIndex);
        }

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

        private void btn_Save_Camera_Setup_Click(object sender, EventArgs e)
        {
            try
            {
                var configBefore = LogConfigProperties("저장 전", _selectedCamera.CameraConfig);

                var pc = cameraPropertyCollectionView?.GetCurrentProperties();
                var uiBefore = LogPropertyCollection("UI에서 가져온 값", pc);

                if (pc != null)
                {
                    _cameraConfigMapper.ApplyToObject(pc);
                }

                var configAfter = LogConfigProperties("ApplyToObject 후", _selectedCamera.CameraConfig);
                var saveResult = _selectedCamera.CameraConfig.Save();

                MessageBox.Show($"저장 결과: {saveResult}\n저장 전후 로그를 확인하세요.");
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"Save error: {ex}");
            }
        }
        #endregion

        #region Camera
        private string LogConfigProperties(string phase, object config)
        {
            var props = config.GetType().GetProperties().Take(5);
            var values = string.Join(", ", props.Select(p => $"{p.Name}={p.GetValue(config)}"));
            Log.Write("Vision_Setup", $"=== {phase} === {values}");
            return values;
        }

        private string LogPropertyCollection(string phase, object pc)
        {
            if (pc == null) return "null";

            var pcType = pc.GetType();
            var countProp = pcType.GetProperty("Count");
            var count = countProp?.GetValue(pc) ?? 0;

            Log.Write("Vision_Setup", $"=== {phase} === Count: {count}");
            return count.ToString();
        }

        private void LoadCameraProperties(int selectedIndex)
        {
            try
            {
                if (selectedIndex < 0 || selectedIndex >= _cameraNames.Count)
                {
                    cameraPropertyCollectionView?.SetProperties(null);
                    _cameraConfigMapper = null;
                    return;
                }

                if (_selectedCamera?.CameraConfig != null)
                {
                    cameraPropertyCollectionView?.SetProperties(null);
                    _cameraConfigMapper = null;

                    Application.DoEvents();

                    _cameraConfigMapper = new ConfigReflectionMapper(_selectedCamera.CameraConfig);
                    cameraPropertyCollectionView?.SetProperties(_cameraConfigMapper.PropertyCollection);
                    cameraPropertyCollectionView?.Refresh();

                    Log.Write("Vision_Setup", $"Camera '{_selectedCamera.Name}' properties loaded successfully");
                }
                else
                {
                    cameraPropertyCollectionView?.SetProperties(null);
                    _cameraConfigMapper = null;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"LoadCameraProperties error: {ex}");
                cameraPropertyCollectionView?.SetProperties(null);
                _cameraConfigMapper = null;
            }
        }
        #endregion

        private void SyncPopupTab(int selectedIndex)
        {
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

        // 기존 교체: 크로스라인 재초기화 + 이전 잔상/오버레이 제거
        private void ResetViewerForCameraChange(int selectedIndex)
        {
            var cam = _camSwitch.Cameras[selectedIndex];
            if (cam == null) 
                return;

            try
            {
                visionImageViewer.Scale.Wheel = 1.0;
                visionImageViewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
                visionImageViewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));

                // 이전 뷰 관련 오버레이 / 크로스라인 제거
                visionImageViewer.ResultOverlays?.Clear();
                visionImageViewer.NormalOverlays?.Clear();

                visionImageViewer.InitCrossLine(); // 내부 버퍼 재생성
                visionImageViewer.ShowCrossLine(visionImageViewer.VisibleCrossLine); // 상태 유지하여 표시
                visionImageViewer.Invalidate();
            }
            catch { }
        }

        // 카메라 전환 직전 호출: 현재 뷰 흔적 제거 (Init 후 표시하지 않음)
        private void PrepareViewerForCameraSwitch()
        {
            if (visionImageViewer == null) return;
            try
            {
                visionImageViewer.ResultOverlays?.Clear();
                visionImageViewer.NormalOverlays?.Clear();
                // 초기화만 하고 표시 여부는 다음 ResetViewerForCameraChange 에서 다시 결정
                visionImageViewer.InitCrossLine();
            }
            catch { }
        }

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

        private void PopOutViewer()
        {
            if (visionImageViewer == null || _viewerPoppedOut) return;

            _viewerOriginalParent = visionImageViewer.Parent;
            _viewerOriginalBounds = new Rectangle(visionImageViewer.Location, visionImageViewer.Size);

            _viewerPopupForm = new Form
            {
                Text = "Camera View",
                FormBorderStyle = FormBorderStyle.Sizable,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(1000, 700),
                ShowInTaskbar = true,
                Owner = null
            };
            _viewerPopupForm.Load += (s, e) =>
            {
                TaskbarHelper.SetAppId(_viewerPopupForm.Handle, "MyApp.Vision");
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

            // 팝업 이동 후 크로스라인 재초기화 (부모 변경에 따른 내부 좌표계 안정화)
            ResetViewerForCameraChange(selIndex);
        }

        private void PopupTabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_popupTabControl == null) return;
            int idx = _popupTabControl.SelectedIndex;
            if (idx < 0) return;

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

            if (!_syncingSelection)
            {
                if (_camSwitch != null && idx < _camSwitch.Cameras.Count)
                {
                    var cam = _camSwitch.Cameras[idx];
                    try { cam.StopLive(); } catch { }

                    PrepareViewerForCameraSwitch();

                    visionImageViewer.SuspendDisplay();
                    _camSwitch.Change(idx);

                    try
                    {
                        if (cam != null)
                        {
                            visionImageViewer.Simulated = false;
                            cam.SuspendedImageDisplay = false;
                            var rcLive = cam.StartLive();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Write("Vision_Setup", $"PopupTab camera switch error: {ex}");
                    }

                    ResetViewerForCameraChange(idx);
                    visionImageViewer.ResumeDisplay();
                    visionImageViewer.StartUpdateTask();
                }

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

            // 원래 자리 복귀 후 크로스라인 다시 초기화
            if (_camSwitch != null && _camSwitch.SelectCameraIndex >= 0)
                ResetViewerForCameraChange(_camSwitch.SelectCameraIndex);
        }

        private void btn_Save_Setup_Cylinder_Click(object sender, EventArgs e)
        {
            try { }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); }
        protected override void OnResize(EventArgs e) { base.OnResize(e); this.Invalidate(); }

        private void cameraListBoxItemsView_Load(object sender, EventArgs e)
        {
        }
    }
}
