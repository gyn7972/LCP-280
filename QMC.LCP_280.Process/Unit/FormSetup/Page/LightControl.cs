using QMC.Common;
using QMC.Common.LightController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    public partial class LightControl : UserControl
    {
        #region Light
        private ConfigReflectionMapper _illuminatorConfigMapper;
        private ConfigReflectionMapper _illuminatorChannelConfigMapper;

        private LeesOsLightController _selectedIlluminator;
        private List<string> _illuminatorNames;
        private List<string> _channelNames;
        private int _selectedChannelIndex = -1;
        #endregion

        public LightControl()
        {
            InitializeComponent();
            InitializeUI();
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                //조명
                BinIlluminatorList();
                WireIlluminatorEvents();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

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
        #endregion

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

            // Save 버튼
            if (btn_Save_Illuninator_Setup != null)
            {
                btn_Save_Illuninator_Setup.Click -= btn_Save_Illuninator_Setup_Click;
                btn_Save_Illuninator_Setup.Click += btn_Save_Illuninator_Setup_Click;
            }

            if (btn_Connect_Illuminator != null)
            {
                btn_Connect_Illuminator.Click -= btn_Connect_Illuminator_Click;
                btn_Connect_Illuminator.Click += btn_Connect_Illuminator_Click;
            }

            if (btn_Disconnect_Illuminator != null)
            {
                btn_Disconnect_Illuminator.Click -= btn_Disconnect_Illuminator_Click;
                btn_Disconnect_Illuminator.Click += btn_Disconnect_Illuminator_Click;
            }

            if (btn_All_On_Illuminator != null)
            {
                btn_All_On_Illuminator.Click -= btn_All_On_Illuminator_Click;
                btn_All_On_Illuminator.Click += btn_All_On_Illuminator_Click;
            }

            if (btn_All_Off_Illuminator != null)
            {
                btn_All_Off_Illuminator.Click -= btn_All_Off_Illuminator_Click;
                btn_All_Off_Illuminator.Click += btn_All_Off_Illuminator_Click;
            }

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

            // === TrackBar 이벤트 추가 ===
            if (trackBar_LightIntensity != null)
            {
                trackBar_LightIntensity.Scroll -= trackBar_LightIntensity_Scroll;
                trackBar_LightIntensity.Scroll += trackBar_LightIntensity_Scroll;

                trackBar_LightIntensity.MouseUp -= trackBar_LightIntensity_MouseUp;
                trackBar_LightIntensity.MouseUp += trackBar_LightIntensity_MouseUp;
            }
        }

        #region TrackBar Events

        // Scroll 이벤트 - 실시간 값 표시용
        private void trackBar_LightIntensity_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (label_Intensity != null)
                {
                    label_Intensity.Text = $"Intensity: {trackBar_LightIntensity.Value}";
                }

                // === 연결 여부와 관계없이 Config Volume 값 변경 ===
                if (_selectedIlluminator != null && _selectedChannelIndex >= 0)
                {
                    var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                    channel.Config.Volume = trackBar_LightIntensity.Value;

                    _illuminatorConfigMapper = new ConfigReflectionMapper(channel.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);

                    // PropertyCollectionView 갱신
                    illuminatorPropertyCollectionView?.View_RefreshProperties();
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"trackBar_LightIntensity_Scroll error: {ex}");
            }
        }

        // MouseUp 이벤트 - 조명 밝기 실제 적용
        private void trackBar_LightIntensity_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    ShowStatusMessage("채널을 선택하세요.", false);
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                int intensity = trackBar_LightIntensity.Value;
                  
                // === Volume 값 설정 및 Property 갱신 ===
                channel.Config.Volume = intensity;
                illuminatorPropertyCollectionView?.View_RefreshProperties();

                // === 연결된 경우에만 실제 조명 제어 ===
                if (_selectedIlluminator.IsConnected)
                {
                    _selectedIlluminator.SetVolumeCommandString(_selectedChannelIndex + 1, intensity);
                    ShowStatusMessage($"Channel {_selectedChannelIndex + 1} 밝기 적용: {intensity}", true);
                    Log.Write("Vision_Setup", $"Channel {_selectedChannelIndex + 1} intensity set to {intensity}");
                }
                else
                {
                    // 연결 안된 경우 설정값만 변경되었음을 알림
                    ShowStatusMessage($"Channel {_selectedChannelIndex + 1} 밝기 설정: {intensity} (조명 미연결)", true);
                    Log.Write("Vision_Setup", $"Channel {_selectedChannelIndex + 1} intensity configured to {intensity} (not connected)");
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"밝기 조절 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"trackBar_LightIntensity_MouseUp error: {ex}");
            }
        }


        #endregion

        private void btn_Save_Illuninator_Setup_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                {
                    MessageBox.Show("조명을 선택하세요.");
                    return;
                }

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

                        if (_selectedIlluminator.IsConnected)
                        {
                            _selectedIlluminator.SetVolumeCommandString(channel.ChannelNo, channel.Config.Volume);
                        }

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

        #region Light Controller Improvements

        private void btn_On_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    ShowStatusMessage("채널을 선택하세요.", false);
                    return;
                }

                if (!EnsureIlluminatorConnection())
                {
                    ShowStatusMessage("조명 컨트롤러 연결에 실패했습니다.", false);
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = true;

                bool success = _selectedIlluminator.SetChannelsOn(_selectedChannelIndex + 1);

                OnIlluminatorChannelSelected(null, _selectedChannelIndex);
                UpdateIlluminatorButtonColors();

                ShowStatusMessage($"Channel {_selectedChannelIndex + 1} ON", true);
                Log.Write("Vision_Setup", $"Channel {_selectedChannelIndex + 1} turned ON successfully");
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"조명 켜기 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"btn_On_Illuminator_Click error: {ex}");
            }
        }

        private void btn_Off_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    ShowStatusMessage("채널을 선택하세요.", false);
                    return;
                }

                if (!EnsureIlluminatorConnection())
                {
                    ShowStatusMessage("조명 컨트롤러 연결에 실패했습니다.", false);
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = false;

                _selectedIlluminator.SetChannelsOff(_selectedChannelIndex + 1);

                OnIlluminatorChannelSelected(null, _selectedChannelIndex);
                UpdateIlluminatorButtonColors();

                ShowStatusMessage($"Channel {_selectedChannelIndex + 1} OFF", true);
                Log.Write("Vision_Setup", $"Channel {_selectedChannelIndex + 1} turned OFF successfully");
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"조명 끄기 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"btn_Off_Illuminator_Click error: {ex}");
            }
        }

        private bool EnsureIlluminatorConnection()
        {
            try
            {
                if (_selectedIlluminator == null)
                    return false;

                if (_selectedIlluminator.IsConnected)
                    return true;

                if (!_selectedIlluminator.Config.Validate())
                {
                    ShowStatusMessage("포트 설정을 확인하세요.", false);
                    return false;
                }

                int result = _selectedIlluminator.Connect();
                if (result == 0)
                {
                    ShowStatusMessage($"자동 연결됨: {_selectedIlluminator.Config.PortName}", true);
                    UpdateConnectionStatus();
                    return true;
                }
                else
                {
                    ShowStatusMessage($"연결 실패 (코드: {result})", false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"EnsureIlluminatorConnection error: {ex}");
                return false;
            }
        }

        private void UpdateConnectionStatus()
        {
            try
            {
                if (_selectedIlluminator != null)
                {
                    bool isConnected = _selectedIlluminator.IsConnected;

                    if (btn_On_Illuminator != null)
                    {
                        btn_On_Illuminator.Enabled = true;
                        btn_On_Illuminator.BackColor = isConnected ?
                            Color.LightGreen : Color.LightGray;
                    }

                    if (btn_Off_Illuminator != null)
                    {
                        btn_Off_Illuminator.Enabled = true;
                        btn_Off_Illuminator.BackColor = isConnected ?
                            Color.LightCoral : Color.LightGray;
                    }

                    if (gbIlluminatorControl != null)
                    {
                        gbIlluminatorControl.Text = isConnected ?
                            $"Control - 연결됨 ({_selectedIlluminator.Config.PortName})" :
                            "Control - 연결 안됨";
                        gbIlluminatorControl.ForeColor = isConnected ?
                            Color.DarkGreen : Color.DarkRed;
                    }

                    // === TrackBar 상태 업데이트 추가 ===
                    //if (trackBar_LightIntensity != null)
                    //{
                    //    trackBar_LightIntensity.Enabled = isConnected && _selectedChannelIndex >= 0;
                    //}
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"UpdateConnectionStatus error: {ex}");
            }
        }

        private void UpdateIlluminatorButtonColors()
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                    return;

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                bool isOn = channel.Config.On;

                if (btn_On_Illuminator != null)
                {
                    btn_On_Illuminator.BackColor = isOn ?
                        Color.Green : Color.LightGreen;
                    btn_On_Illuminator.ForeColor = isOn ?
                        Color.White : Color.Black;
                }

                if (btn_Off_Illuminator != null)
                {
                    btn_Off_Illuminator.BackColor = !isOn ?
                        Color.Red : Color.LightCoral;
                    btn_Off_Illuminator.ForeColor = !isOn ?
                        Color.White : Color.Black;
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"UpdateIlluminatorButtonColors error: {ex}");
            }
        }

        private void ShowStatusMessage(string message, bool isSuccess)
        {
            try
            {
                if (gbIlluminatorControl != null)
                {
                    string baseTitle =
                        (_selectedIlluminator != null && _selectedIlluminator.IsConnected)
                        ? $"Control - 연결됨 ({_selectedIlluminator.Config?.PortName})"
                        : "Control - 연결 안됨";

                    gbIlluminatorControl.Text = $"{baseTitle}  |  {message}";

                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 3000;
                    timer.Tick += (s, e) =>
                    {
                        string restoreTitle =
                            (_selectedIlluminator != null && _selectedIlluminator.IsConnected)
                            ? $"Control - 연결됨 ({_selectedIlluminator.Config?.PortName})"
                            : "Control - 연결 안됨";

                        gbIlluminatorControl.Text = restoreTitle;

                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();
                }

                Log.Write("Vision_Setup", $"Status: {message}");
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"ShowStatusMessage error: {ex}");
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
                UpdateConnectionStatus();
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

                iluminatorChannelListBoxItemsView.SelectedIndex = -1;
                _selectedChannelIndex = -1;

                illuminatorPropertyCollectionView?.SetProperties(null);
                _illuminatorChannelConfigMapper = null;

                if (_selectedIlluminator.Config != null)
                {
                    _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
                }

                UpdateConnectionStatus();

                ShowStatusMessage($"선택됨: {_selectedIlluminator.Model} ({_selectedIlluminator.Channels.Count}채널)", true);
            }
        }

        private void OnIlluminatorChannelSelected(object sender, int selectedIndex)
        {
            _selectedChannelIndex = selectedIndex;

            if (_selectedIlluminator == null)
                return;

            if (selectedIndex < 0 || selectedIndex >= _selectedIlluminator.Channels.Count)
            {
                if (_selectedIlluminator.Config != null)
                {
                    illuminatorPropertyCollectionView?.SetProperties(null);
                    _illuminatorChannelConfigMapper = null;

                    _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
                }

                if (btn_On_Illuminator != null) btn_On_Illuminator.Enabled = false;
                if (btn_Off_Illuminator != null) btn_Off_Illuminator.Enabled = false;

                if (trackBar_LightIntensity != null)
                {
                    trackBar_LightIntensity.Enabled = false;
                    trackBar_LightIntensity.Value = 0;
                }
                if (label_Intensity != null)
                {
                    label_Intensity.Text = "Intensity";
                }
                return;
            }

            // === 채널 선택 시 저장된 Config를 다시 로드하여 복원 ===
            var selectedChannel = _selectedIlluminator.Channels[selectedIndex];

            // Config 파일에서 저장된 값을 다시 로드 (이전 수정사항 취소)
            selectedChannel.Config.Load();

            illuminatorPropertyCollectionView?.SetProperties(null);
            _illuminatorConfigMapper = null;

            _illuminatorChannelConfigMapper = new ConfigReflectionMapper(selectedChannel.Config);
            illuminatorPropertyCollectionView?.SetProperties(_illuminatorChannelConfigMapper.PropertyCollection);

            if (btn_On_Illuminator != null) btn_On_Illuminator.Enabled = true;
            if (btn_Off_Illuminator != null) btn_Off_Illuminator.Enabled = true;

            // === TrackBar 값을 저장된 Config 값으로 설정 ===
            if (trackBar_LightIntensity != null)
            {
                trackBar_LightIntensity.Enabled = true;
                trackBar_LightIntensity.Value = Math.Min(Math.Max(selectedChannel.Config.Volume, 0), 4095);
            }
            if (label_Intensity != null)
            {
                label_Intensity.Text = $"Intensity: {selectedChannel.Config.Volume}";
            }

            UpdateIlluminatorButtonColors();
            UpdateConnectionStatus();
        }

        private void btn_All_On_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                {
                    ShowStatusMessage("조명 컨트롤러를 선택하세요.", false);
                    return;
                }

                if (!EnsureIlluminatorConnection())
                {
                    ShowStatusMessage("연결에 실패했습니다.", false);
                    return;
                }

                _selectedIlluminator.SetAllChannelsOn();

                foreach (var channel in _selectedIlluminator.Channels)
                {
                    channel.Config.On = true;
                }

                if (_selectedChannelIndex >= 0)
                {
                    OnIlluminatorChannelSelected(null, _selectedChannelIndex);
                    UpdateIlluminatorButtonColors();
                }

                ShowStatusMessage("모든 채널 켜짐", true);
                Log.Write("Vision_Setup", "All channels turned ON");
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"전체 켜기 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"btn_All_On_Illuminator_Click error: {ex}");
            }
        }

        private void btn_All_Off_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                {
                    ShowStatusMessage("조명 컨트롤러를 선택하세요.", false);
                    return;
                }

                if (!EnsureIlluminatorConnection())
                {
                    ShowStatusMessage("연결에 실패했습니다.", false);
                    return;
                }

                _selectedIlluminator.SetAllChannelsOff();

                foreach (var channel in _selectedIlluminator.Channels)
                {
                    channel.Config.On = false;
                }

                if (_selectedChannelIndex >= 0)
                {
                    OnIlluminatorChannelSelected(null, _selectedChannelIndex);
                    UpdateIlluminatorButtonColors();
                }

                ShowStatusMessage("모든 채널 꺼짐", true);
                Log.Write("Vision_Setup", "All channels turned OFF");
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"전체 끄기 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"btn_All_Off_Illuminator_Click error: {ex}");
            }
        }

        private void btn_Connect_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                {
                    ShowStatusMessage("조명 컨트롤러를 선택하세요.", false);
                    return;
                }

                if (_selectedIlluminator.IsConnected)
                {
                    ShowStatusMessage("이미 연결되어 있습니다.", true);
                    return;
                }

                int result = _selectedIlluminator.Connect();
                if (result == 0)
                {
                    ShowStatusMessage("연결되었습니다.", true);
                    UpdateConnectionStatus();
                }
                else
                {
                    ShowStatusMessage($"연결 실패 (코드: {result})", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"연결 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"btn_Connect_Illuminator_Click error: {ex}");
            }
        }

        private void btn_Disconnect_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                    return;

                _selectedIlluminator.Disconnect();
                ShowStatusMessage("연결이 해제되었습니다.", true);
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                ShowStatusMessage($"해제 오류: {ex.Message}", false);
                Log.Write("Vision_Setup", $"btn_Disconnect_Illuminator_Click error: {ex}");
            }
        }

        #endregion
    }
}