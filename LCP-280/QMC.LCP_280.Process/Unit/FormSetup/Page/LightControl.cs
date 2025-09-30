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
                BinIlluminatorList(); // 추가
                WireIlluminatorEvents(); // 추가
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

        //private void OnIlluminatorSelected(object sender, int selectedIndex)
        //{
        //    if (selectedIndex < 0 || selectedIndex >= _illuminatorNames.Count)
        //    {
        //        _selectedIlluminator = null;
        //        _channelNames = null;
        //        iluminatorChannelListBoxItemsView?.SetItems();
        //        illuminatorPropertyCollectionView?.SetProperties(null);
        //        _illuminatorConfigMapper = null;
        //        return;
        //    }

        //    var illuminatorName = _illuminatorNames[selectedIndex];
        //    _selectedIlluminator = Equipment.Instance.LightControllers[illuminatorName];

        //    if (_selectedIlluminator != null)
        //    {
        //        _channelNames = new List<string>();
        //        for (int i = 0; i < _selectedIlluminator.Channels.Count; i++)
        //        {
        //            _channelNames.Add($"Channel {i + 1}");
        //        }

        //        iluminatorChannelListBoxItemsView?.SetItems(_channelNames.ToArray());

        //        // 기본적으로 채널 선택 해제하고 메인 컨트롤러 설정 표시
        //        iluminatorChannelListBoxItemsView.SelectedIndex = -1;
        //        _selectedChannelIndex = -1;

        //        // === 기존 매핑 해제 추가 ===
        //        illuminatorPropertyCollectionView?.SetProperties(null);
        //        _illuminatorChannelConfigMapper = null;

        //        if (_selectedIlluminator.Config != null)
        //        {
        //            _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
        //            illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
        //        }
        //    }
        //}

        //private void OnIlluminatorChannelSelected(object sender, int selectedIndex)
        //{
        //    _selectedChannelIndex = selectedIndex;

        //    if (_selectedIlluminator == null)
        //        return;

        //    if (selectedIndex < 0 || selectedIndex >= _selectedIlluminator.Channels.Count)
        //    {
        //        // 메인 컨트롤러 설정 표시
        //        if (_selectedIlluminator.Config != null)
        //        {
        //            // === 기존 매핑 해제 추가 ===
        //            illuminatorPropertyCollectionView?.SetProperties(null);
        //            _illuminatorChannelConfigMapper = null;

        //            _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
        //            illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
        //        }
        //        return;
        //    }

        //    // 채널 설정 표시
        //    var selectedChannel = _selectedIlluminator.Channels[selectedIndex];

        //    // === 기존 매핑 해제 추가 ===
        //    illuminatorPropertyCollectionView?.SetProperties(null);
        //    _illuminatorConfigMapper = null;

        //    _illuminatorChannelConfigMapper = new ConfigReflectionMapper(selectedChannel.Config);
        //    illuminatorPropertyCollectionView?.SetProperties(_illuminatorChannelConfigMapper.PropertyCollection);
        //}
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

            // 기존 ON/OFF 버튼만 연결
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

                        _selectedIlluminator.SetVolumeCommandString(channel.ChannelNo, channel.Config.Volume);

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

        // 기존 이벤트 핸들러 개선
        private void btn_On_Illuminator_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    ShowStatusMessage("채널을 선택하세요.", false);
                    return;
                }

                // Config 검증 및 자동 연결
                if (!EnsureIlluminatorConnection())
                {
                    ShowStatusMessage("조명 컨트롤러 연결에 실패했습니다.", false);
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = true;

                bool success = _selectedIlluminator.SetChannelsOn(_selectedChannelIndex + 1); // 최대 5회 재시도

                // UI 업데이트
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

                // Config 검증 및 자동 연결
                if (!EnsureIlluminatorConnection())
                {
                    ShowStatusMessage("조명 컨트롤러 연결에 실패했습니다.", false);
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = false;

                _selectedIlluminator.SetChannelsOff(_selectedChannelIndex + 1);

                // UI 업데이트
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

        // 연결 확인 및 자동 연결 메서드
        private bool EnsureIlluminatorConnection()
        {
            try
            {
                if (_selectedIlluminator == null)
                    return false;

                // 이미 연결되어 있으면 OK
                if (_selectedIlluminator.IsConnected)
                    return true;

                // Config 검증
                if (!_selectedIlluminator.Config.Validate())
                {
                    ShowStatusMessage("포트 설정을 확인하세요.", false);
                    return false;
                }

                // 자동 연결 시도
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

        // 연결 상태 업데이트
        private void UpdateConnectionStatus()
        {
            try
            {
                if (_selectedIlluminator != null)
                {
                    bool isConnected = _selectedIlluminator.IsConnected;

                    // 버튼 상태 업데이트
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

                    // GroupBox 타이틀에 연결 상태 표시
                    if (gbIlluminatorControl != null)
                    {
                        gbIlluminatorControl.Text = isConnected ?
                            $"Control - 연결됨 ({_selectedIlluminator.Config.PortName})" :
                            "Control - 연결 안됨";
                        gbIlluminatorControl.ForeColor = isConnected ?
                            Color.DarkGreen : Color.DarkRed;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"UpdateConnectionStatus error: {ex}");
            }
        }

        // 채널 상태에 따른 버튼 색상 업데이트
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

        // 상태 메시지 표시 (기존 UI 활용)
        private void ShowStatusMessage(string message, bool isSuccess)
        {
            try
            {
                if (gbIlluminatorControl != null)
                {
                    // 현재 연결 상태 기준 기본 제목 생성
                    string baseTitle =
                        (_selectedIlluminator != null && _selectedIlluminator.IsConnected)
                        ? $"Control - 연결됨 ({_selectedIlluminator.Config?.PortName})"
                        : "Control - 연결 안됨";

                    // 메시지를 제목에 덧붙여 표시
                    gbIlluminatorControl.Text = $"{baseTitle}  |  {message}";

                    // 3초 후 원래 제목으로 복구 (텍스트만)
                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 3000;
                    timer.Tick += (s, e) =>
                    {
                        // 복구 시점에 연결 상태가 바뀌었을 수 있으니 다시 계산
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

                // 로그 기록
                Log.Write("Vision_Setup", $"Status: {message}");
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"ShowStatusMessage error: {ex}");
            }
        }

        // 조명 컨트롤러 선택 시 연결 상태 확인
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

                // 기본적으로 채널 선택 해제하고 메인 컨트롤러 설정 표시
                iluminatorChannelListBoxItemsView.SelectedIndex = -1;
                _selectedChannelIndex = -1;

                // 기존 매핑 해제
                illuminatorPropertyCollectionView?.SetProperties(null);
                _illuminatorChannelConfigMapper = null;

                if (_selectedIlluminator.Config != null)
                {
                    _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
                }

                // 연결 상태 업데이트
                UpdateConnectionStatus();

                // 모델 정보 표시
                ShowStatusMessage($"선택됨: {_selectedIlluminator.Model} ({_selectedIlluminator.Channels.Count}채널)", true);
            }
        }

        // 채널 선택 시 버튼 색상 업데이트
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
                    illuminatorPropertyCollectionView?.SetProperties(null);
                    _illuminatorChannelConfigMapper = null;

                    _illuminatorConfigMapper = new ConfigReflectionMapper(_selectedIlluminator.Config);
                    illuminatorPropertyCollectionView?.SetProperties(_illuminatorConfigMapper.PropertyCollection);
                }

                // 버튼 비활성화
                if (btn_On_Illuminator != null) btn_On_Illuminator.Enabled = false;
                if (btn_Off_Illuminator != null) btn_Off_Illuminator.Enabled = false;
                return;
            }

            // 채널 설정 표시
            var selectedChannel = _selectedIlluminator.Channels[selectedIndex];

            illuminatorPropertyCollectionView?.SetProperties(null);
            _illuminatorConfigMapper = null;

            _illuminatorChannelConfigMapper = new ConfigReflectionMapper(selectedChannel.Config);
            illuminatorPropertyCollectionView?.SetProperties(_illuminatorChannelConfigMapper.PropertyCollection);

            // 버튼 활성화 및 색상 업데이트
            if (btn_On_Illuminator != null) btn_On_Illuminator.Enabled = true;
            if (btn_Off_Illuminator != null) btn_Off_Illuminator.Enabled = true;

            UpdateIlluminatorButtonColors();
            UpdateConnectionStatus();
        }

        // 전체 채널 제어를 위한 추가 버튼 이벤트 (필요시 Designer에서 버튼 추가)
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

                // 모든 채널 상태 업데이트
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

                // 모든 채널 상태 업데이트
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

        // 연결/해제 버튼 이벤트 (필요시 Designer에서 버튼 추가)
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
