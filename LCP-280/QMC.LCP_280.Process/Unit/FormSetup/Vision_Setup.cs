using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.LightController;
using QMC.Common.Vision;
using QMC.LCP_280.Process.Component;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(2)]
    public partial class Vision_Setup : Form
    {
        private readonly Equipment equipment = Equipment.Instance;

        #region Camera
        private ConfigReflectionMapper _cameraConfigMapper;

        private HIKGigECamera _selectedCamera;
        private CameraSwitch _camSwitch;
        private List<string> _cameraNames;
        #endregion

        #region Light
        private ConfigReflectionMapper _illuminatorConfigMapper;
        private ConfigReflectionMapper _illuminatorChannelConfigMapper;

        private LeesOsLightController _selectedIlluminator;
        private List<string> _illuminatorNames;
        private List<string> _channelNames;
        private int _selectedChannelIndex = -1;
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

                BinIlluminatorList(); // 추가
                WireIlluminatorEvents(); // 추가

                // 추가: 저장 버튼 이벤트 연결
                WireSaveButtonEvents();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        #region Event Handlers
        // ===== Event Wiring =====
        private void WireAxisSelectionEvent()
        {
            if (cameraListBoxItemsView != null)
            {
                cameraListBoxItemsView.ItemSelected -= OnVisionItemSelected;
                cameraListBoxItemsView.ItemSelected += OnVisionItemSelected;
            }
        }

        private void WireSaveButtonEvents()
        {
            if (btn_Save_Camera_Setup != null)
            {
                btn_Save_Camera_Setup.Click -= btn_Save_Camera_Setup_Click;
                btn_Save_Camera_Setup.Click += btn_Save_Camera_Setup_Click;
            }
        }

        private void WireIlluminatorEvents()
        {
            // 조명 리스트 선택 이벤트
            if (iluminatorListBoxItemsView != null)
            {
                iluminatorListBoxItemsView.ItemSelected -= OnIlluminatorSelected;
                iluminatorListBoxItemsView.ItemSelected += OnIlluminatorSelected;
            }

            // 채널 리스트 선택 이벤트
            if (iluminatorChannelListBoxItemsView != null)
            {
                iluminatorChannelListBoxItemsView.ItemSelected -= OnIlluminatorChannelSelected;
                iluminatorChannelListBoxItemsView.ItemSelected += OnIlluminatorChannelSelected;
            }

            ////Setup
            //if (btn_Illuninator_Setup != null)
            //{
            //    btn_Illuninator_Setup.Click -= btn_Illuninator_Setup_Click;
            //    btn_Illuninator_Setup.Click += btn_Save_Illuninator_Setup_Click;
            //}

            //Save
            if (btn_Save_Illuninator_Setup != null)
            {
                btn_Save_Illuninator_Setup.Click -= btn_Save_Illuninator_Setup_Click;
                btn_Save_Illuninator_Setup.Click += btn_Save_Illuninator_Setup_Click;
            }

            // On/Off 버튼 이벤트
            if (btn_On_Illuminator != null)
            {
                btn_On_Illuminator.Click -= btn_On_Illuminator_Click;
                btn_On_Illuminator.Click += btn_On_Illuminator_Click;
            }

            if (btn_Off_Illuminator != null)
            {
                btn_Off_Illuminator.Click -= btn_Off_Illuminator_Click;
                btn_Off_Illuminator.Click += btn_Off_Illuminator_Click;
            }
        }

        private void btn_On_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    MessageBox.Show("채널을 선택하세요.");
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = true;

                // Property UI 갱신
                OnIlluminatorChannelSelected(null, _selectedChannelIndex);
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"btn_On_Illuminator_Click error: {ex}");
            }
        }

        private void btn_Off_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    MessageBox.Show("채널을 선택하세요.");
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = false;

                // Property UI 갱신
                OnIlluminatorChannelSelected(null, _selectedChannelIndex);
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"btn_Off_Illuminator_Click error: {ex}");
            }
        }

        private void btn_Save_Illuninator_Setup_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                {
                    MessageBox.Show("조명을 선택하세요.");
                    return;
                }

                // === 기존 코드에 포커스 해제만 추가 ===
                this.ActiveControl = null;
                Application.DoEvents();

                if (_selectedChannelIndex >= 0 && _selectedChannelIndex < _selectedIlluminator.Channels.Count)
                {
                    // 채널 설정 저장
                    if (_illuminatorChannelConfigMapper != null)
                    {
                        var pc = illuminatorPropertyCollectionView?.GetCurrentProperties();
                        if (pc != null)
                        {
                            _illuminatorChannelConfigMapper.ApplyToObject(pc);
                        }

                        var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                        var saveResult = channel.Config.Save();

                        MessageBox.Show($"Channel {_selectedChannelIndex + 1} 설정 저장 완료.");
                    }
                }
                else
                {
                    // 메인 컨트롤러 설정 저장
                    if (_illuminatorConfigMapper != null)
                    {
                        var pc = illuminatorPropertyCollectionView?.GetCurrentProperties();
                        if (pc != null)
                        {
                            _illuminatorConfigMapper.ApplyToObject(pc);
                        }

                        var saveResult = _selectedIlluminator.Config.Save();

                        MessageBox.Show($"'{_selectedIlluminator.Name}' 컨트롤러 설정 저장 완료.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 중 오류: {ex.Message}");
                Log.Write("Vision_Setup", $"Save illuminator config error: {ex}");
            }
        }
        #endregion

        #region Camera
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
                    //ResetViewerForCameraChange(0);
                    visionImageViewer.ResumeDisplay();
                }
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"BindVisionList error: {ex}");
                cameraListBoxItemsView?.SetItems();
            }
        }



        // 새 저장 메서드
        private void btn_Save_Camera_Setup_Click(object sender, EventArgs e)
        {
            try
            {
                // ... 기존 검증 코드 ...

                // === 저장 전후 값 비교 ===
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

        private void OnVisionItemSelected(object sender, int selectedIndex)
        {
            // 동기화 중이면 카메라 전환 없이 탭만 동기화
            if (_syncingSelection)
            {
                SyncPopupTab(selectedIndex);
                return;
            }

            if (_camSwitch == null) return;
            if (selectedIndex < 0 || selectedIndex >= _camSwitch.Cameras.Count) return;

            var cameraName = _cameraNames[selectedIndex];
            _selectedCamera = Equipment.Instance.Cameras[cameraName];

            try { _selectedCamera.StopLive(); } catch { }

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
                Log.Write("Vision_Setup", $"OnVisionItemSelected StartLive error: {ex}");
            }

            ResetViewerForCameraChange(selectedIndex);
            visionImageViewer.ResumeDisplay();
            visionImageViewer.StartUpdateTask();

            // ===== 여기에 추가: PropertyCollection 바인딩 =====
            LoadCameraProperties(selectedIndex);

            // 팝업 탭 동기화
            SyncPopupTab(selectedIndex);
        }

        // ===== Camera Properties Loading =====
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
                    // === PropertyCollectionView 완전 재설정 ===

                    // 1) 기존 매핑 해제
                    cameraPropertyCollectionView?.SetProperties(null);
                    _cameraConfigMapper = null;

                    // 2) UI 업데이트 대기
                    Application.DoEvents();

                    // 3) 새 매퍼 생성 및 설정
                    _cameraConfigMapper = new ConfigReflectionMapper(_selectedCamera.CameraConfig);
                    cameraPropertyCollectionView?.SetProperties(_cameraConfigMapper.PropertyCollection);

                    // 4) UI 완전 새로고침
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

        #region Light
        private void BinIlluminatorList()
        {
            try
            {
                _illuminatorNames = new List<string>();

                // Equipment에서 조명 컨트롤러들을 가져옴
                foreach (var lightKey in Equipment.Instance.LightControllers.Keys)
                {
                    _illuminatorNames.Add(lightKey);
                }

                if (_illuminatorNames.Count == 0)
                {
                    // 조명이 없으면 빈 상태로
                    iluminatorListBoxItemsView?.SetItems();
                    iluminatorChannelListBoxItemsView?.SetItems();
                    return;
                }

                iluminatorListBoxItemsView?.SetItems(_illuminatorNames.ToArray());

                // 첫 번째 조명 자동 선택
                if (_illuminatorNames.Count > 0)
                {
                    iluminatorListBoxItemsView.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"BinIlluminatorList error: {ex}");
            }
        }

        private void OnIlluminatorSelected(object sender, int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= _illuminatorNames.Count)
            {
                _selectedIlluminator = null;
                _channelNames = null;
                iluminatorChannelListBoxItemsView?.SetItems();
                illuminatorPropertyCollectionView?.SetProperties(null);
                _illuminatorConfigMapper = null;
                return;
            }

            var illuminatorName = _illuminatorNames[selectedIndex];
            _selectedIlluminator = Equipment.Instance.LightControllers[illuminatorName];

            if (_selectedIlluminator != null)
            {
                _channelNames = new List<string>();
                for (int i = 0; i < _selectedIlluminator.Channels.Count; i++)
                {
                    _channelNames.Add($"Channel {i + 1}");
                }

                iluminatorChannelListBoxItemsView?.SetItems(_channelNames.ToArray());

                // 기본적으로 채널 선택 해제하고 메인 컨트롤러 설정 표시
                iluminatorChannelListBoxItemsView.SelectedIndex = -1;
                _selectedChannelIndex = -1;

                // === 기존 매핑 해제 추가 ===
                illuminatorPropertyCollectionView?.SetProperties(null);
                _illuminatorChannelConfigMapper = null;

                if (_selectedIlluminator.Config != null)
                {
                    _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
                }
            }
        }

        private void OnIlluminatorChannelSelected(object sender, int selectedIndex)
        {
            _selectedChannelIndex = selectedIndex;

            if (_selectedIlluminator == null)
                return;

            if (selectedIndex < 0 || selectedIndex >= _selectedIlluminator.Channels.Count)
            {
                // 메인 컨트롤러 설정 표시
                if (_selectedIlluminator.Config != null)
                {
                    // === 기존 매핑 해제 추가 ===
                    illuminatorPropertyCollectionView?.SetProperties(null);
                    _illuminatorChannelConfigMapper = null;

                    _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
                }
                return;
            }

            // 채널 설정 표시
            var selectedChannel = _selectedIlluminator.Channels[selectedIndex];

            // === 기존 매핑 해제 추가 ===
            illuminatorPropertyCollectionView?.SetProperties(null);
            _illuminatorConfigMapper = null;

            _illuminatorChannelConfigMapper = new ConfigReflectionMapper(selectedChannel.Config);
            illuminatorPropertyCollectionView?.SetProperties(_illuminatorChannelConfigMapper.PropertyCollection);
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

        private void ResetViewerForCameraChange(int selectedIndex)
        {
            var cam = _camSwitch.Cameras[selectedIndex];
            if (cam == null) return;
            visionImageViewer.Scale.Wheel = 1.0;
            visionImageViewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
            visionImageViewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));
            visionImageViewer.InitCrossLine();
            visionImageViewer.ShowCrossLine(visionImageViewer.VisibleCrossLine);
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
                FormBorderStyle = FormBorderStyle.Sizable,          //FormBorderStyle.SizableToolWindow,
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

            // 탭 컨트롤 생성 및 카메라 이름으로 탭 구성
            _popupTabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // 카메라 이름들로 탭 페이지 생성
            var camNames = cameraListBoxItemsView?.GetItems() ?? _cameraNames?.ToArray() ?? Array.Empty<string>();
            if (camNames.Length == 0 && Equipment.Instance?.Cameras != null)
            {
                camNames = Equipment.Instance.Cameras.Keys.ToArray();
            }

            foreach (var name in camNames)
                _popupTabControl.TabPages.Add(new TabPage(string.IsNullOrWhiteSpace(name) ? "Camera" : name));

            // 현재 선택된 인덱스 반영
            int selIndex = 0;
            if (cameraListBoxItemsView != null && cameraListBoxItemsView.SelectedIndex >= 0)
                selIndex = cameraListBoxItemsView.SelectedIndex;
            else if (_camSwitch != null && _camSwitch.SelectCameraIndex >= 0)
                selIndex = _camSwitch.SelectCameraIndex;

            selIndex = Math.Max(0, Math.Min(selIndex, _popupTabControl.TabPages.Count - 1));
            _popupTabControl.SelectedIndex = selIndex;

            // 탭 선택 이벤트 연결
            _popupTabControl.SelectedIndexChanged -= PopupTabs_SelectedIndexChanged;
            _popupTabControl.SelectedIndexChanged += PopupTabs_SelectedIndexChanged;

            // 뷰어를 선택된 탭 페이지에 추가
            visionImageViewer.SuspendLayout();
            _viewerOriginalParent.Controls.Remove(visionImageViewer);
            var hostPage = _popupTabControl.TabPages[selIndex];
            visionImageViewer.Dock = DockStyle.Fill;
            hostPage.Controls.Add(visionImageViewer);
            visionImageViewer.ResumeLayout();

            // 팝업 폼에 탭 컨트롤 추가
            _viewerPopupForm.Controls.Add(_popupTabControl);
            _viewerPoppedOut = true;
            _viewerPopupForm.Show(this);
            _viewerPopupForm.BringToFront();

            // 리스트 선택과 동기화
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

            // 뷰어를 현재 탭 페이지로 이동
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

            // 동기화 중이 아닐 때만 카메라 전환 및 리스트 동기화
            if (!_syncingSelection)
            {
                // 카메라 전환
                if (_camSwitch != null && idx < _camSwitch.Cameras.Count)
                {
                    var cam = _camSwitch.Cameras[idx];
                    try { cam.StopLive(); } catch { }
                    
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

                // 리스트 선택 동기화
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
                // 탭에서 뷰어 분리
                if (_popupTabControl != null)
                {
                    try
                    {
                        var host = visionImageViewer.Parent;
                        if (host != null) host.Controls.Remove(visionImageViewer);
                    }
                    catch { }
                }

                // 원래 부모로 복귀
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

                // 팝업 폼/탭 정리
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

        private void cameraListBoxItemsView_Load(object sender, EventArgs e)
        {

        }
    }
}
