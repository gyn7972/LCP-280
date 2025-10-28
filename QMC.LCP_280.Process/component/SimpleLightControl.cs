using QMC.Common;
using QMC.Common.LightController;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormSetup
{
    public partial class SimpleLightControl : UserControl
    {
        private LeesOsLightController _selectedIlluminator;
        private List<string> _illuminatorNames;
        private List<string> _channelNames;
        private int _selectedChannelIndex = -1;

        // 외부에서 접근 가능한 속성들
        public LeesOsLightController SelectedIlluminator => _selectedIlluminator;
        public int SelectedChannelIndex => _selectedChannelIndex;
        public int CurrentIntensity => trackBar_LightIntensity.Value;

        public SimpleLightControl()
        {
            InitializeComponent();
            InitializeUI();
        }

        private void InitializeUI()
        {
            try
            {
                BindIlluminatorList();
                WireEvents();
            }
            catch (Exception ex)
            {
                Log.Write("SimpleLightControl", $"InitializeUI error: {ex}");
            }
        }

        private void BindIlluminatorList()
        {
            try
            {
                _illuminatorNames = new List<string>();

                foreach (var lightKey in Equipment.Instance.LightControllers.Keys)
                {
                    _illuminatorNames.Add(lightKey);
                }

                if (_illuminatorNames.Count == 0)
                {
                    comboBox_Illuminator.DataSource = null;
                    comboBox_Channel.DataSource = null;
                    return;
                }

                comboBox_Illuminator.DataSource = _illuminatorNames;

                if (_illuminatorNames.Count > 0)
                {
                    comboBox_Illuminator.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Log.Write("SimpleLightControl", $"BindIlluminatorList error: {ex}");
            }
        }

        private void WireEvents()
        {
            // ComboBox 선택 이벤트
            comboBox_Illuminator.SelectedIndexChanged += OnIlluminatorSelected;
            comboBox_Channel.SelectedIndexChanged += OnChannelSelected;

            // 버튼 이벤트
            btn_Connect.Click += btn_Connect_Click;
            btn_Disconnect.Click += btn_Disconnect_Click;
            btn_On.Click += btn_On_Click;
            btn_Off.Click += btn_Off_Click;

            // TrackBar 이벤트
            trackBar_LightIntensity.Scroll += trackBar_LightIntensity_Scroll;
            trackBar_LightIntensity.MouseUp += trackBar_LightIntensity_MouseUp;
        }

        private void OnIlluminatorSelected(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_Illuminator.SelectedIndex < 0)
                {
                    _selectedIlluminator = null;
                    comboBox_Channel.DataSource = null;
                    UpdateConnectionStatus();
                    return;
                }

                var illuminatorName = _illuminatorNames[comboBox_Illuminator.SelectedIndex];
                _selectedIlluminator = Equipment.Instance.LightControllers[illuminatorName];

                if (_selectedIlluminator != null)
                {
                    // 채널 리스트 생성
                    _channelNames = new List<string>();
                    for (int i = 0; i < _selectedIlluminator.Channels.Count; i++)
                    {
                        _channelNames.Add($"Channel {i + 1}");
                    }

                    comboBox_Channel.DataSource = _channelNames;
                    comboBox_Channel.SelectedIndex = -1;
                    _selectedChannelIndex = -1;

                    UpdateConnectionStatus();
                }
            }
            catch (Exception ex)
            {
                Log.Write("SimpleLightControl", $"OnIlluminatorSelected error: {ex}");
            }
        }

        private void OnChannelSelected(object sender, EventArgs e)
        {
            try
            {
                _selectedChannelIndex = comboBox_Channel.SelectedIndex;

                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    trackBar_LightIntensity.Enabled = false;
                    trackBar_LightIntensity.Value = 0;
                    label_Intensity.Text = "Intensity: 0";
                    btn_On.Enabled = false;
                    btn_Off.Enabled = false;
                    return;
                }

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];

                // TrackBar 설정
                trackBar_LightIntensity.Enabled = true;
                trackBar_LightIntensity.Value = Math.Min(Math.Max(channel.Config.Volume, 0), 4095);
                label_Intensity.Text = $"Intensity: {channel.Config.Volume}";

                // 버튼 활성화
                btn_On.Enabled = true;
                btn_Off.Enabled = true;

                UpdateButtonColors();
            }
            catch (Exception ex)
            {
                Log.Write("SimpleLightControl", $"OnChannelSelected error: {ex}");
            }
        }

        private void trackBar_LightIntensity_Scroll(object sender, EventArgs e)
        {
            label_Intensity.Text = $"Intensity: {trackBar_LightIntensity.Value}";
        }

        private void trackBar_LightIntensity_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                    return;

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                int intensity = trackBar_LightIntensity.Value;

                channel.Config.Volume = intensity;

                if (_selectedIlluminator.IsConnected)
                {
                    _selectedIlluminator.SetVolumeCommandString(_selectedChannelIndex + 1, intensity);
                    ShowStatus($"Ch{_selectedChannelIndex + 1} Intensity: {intensity}", true);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", false);
                Log.Write("SimpleLightControl", $"trackBar_LightIntensity_MouseUp error: {ex}");
            }
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                {
                    ShowStatus("Select controller", false);
                    return;
                }

                if (_selectedIlluminator.IsConnected)
                {
                    ShowStatus("Already connected", true);
                    return;
                }

                int result = _selectedIlluminator.Connect();
                if (result == 0)
                {
                    ShowStatus($"Connected: {_selectedIlluminator.Config.PortName}", true);
                    UpdateConnectionStatus();
                }
                else
                {
                    ShowStatus($"Connection failed (Code: {result})", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", false);
                Log.Write("SimpleLightControl", $"btn_Connect_Click error: {ex}");
            }
        }

        private void btn_Disconnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null)
                    return;

                _selectedIlluminator.Disconnect();
                ShowStatus("Disconnected", true);
                UpdateConnectionStatus();
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", false);
                Log.Write("SimpleLightControl", $"btn_Disconnect_Click error: {ex}");
            }
        }

        private void btn_On_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    ShowStatus("Select channel", false);
                    return;
                }

                if (!EnsureConnection())
                    return;

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = true;

                _selectedIlluminator.SetChannelsOn(_selectedChannelIndex + 1);
                UpdateButtonColors();
                ShowStatus($"Ch{_selectedChannelIndex + 1} ON", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", false);
                Log.Write("SimpleLightControl", $"btn_On_Click error: {ex}");
            }
        }

        private void btn_Off_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                {
                    ShowStatus("Select channel", false);
                    return;
                }

                if (!EnsureConnection())
                    return;

                var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
                channel.Config.On = false;

                _selectedIlluminator.SetChannelsOff(_selectedChannelIndex + 1);
                UpdateButtonColors();
                ShowStatus($"Ch{_selectedChannelIndex + 1} OFF", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Error: {ex.Message}", false);
                Log.Write("SimpleLightControl", $"btn_Off_Click error: {ex}");
            }
        }

        private bool EnsureConnection()
        {
            if (_selectedIlluminator == null)
                return false;

            if (_selectedIlluminator.IsConnected)
                return true;

            int result = _selectedIlluminator.Connect();
            if (result == 0)
            {
                UpdateConnectionStatus();
                return true;
            }

            ShowStatus("Connection required", false);
            return false;
        }

        private void UpdateConnectionStatus()
        {
            if (_selectedIlluminator != null)
            {
                bool connected = _selectedIlluminator.IsConnected;

                label_Status.Text = connected ?
                    $"Connected ({_selectedIlluminator.Config.PortName})" :
                    "Not Connected";

                label_Status.ForeColor = connected ? Color.Green : Color.Red;

                btn_Connect.Enabled = !connected;
                btn_Disconnect.Enabled = connected;
            }
            else
            {
                label_Status.Text = "No Controller Selected";
                label_Status.ForeColor = Color.Gray;
                btn_Connect.Enabled = false;
                btn_Disconnect.Enabled = false;
            }
        }

        private void UpdateButtonColors()
        {
            if (_selectedIlluminator == null || _selectedChannelIndex < 0)
                return;

            var channel = _selectedIlluminator.Channels[_selectedChannelIndex];
            bool isOn = channel.Config.On;

            btn_On.BackColor = isOn ? Color.Green : Color.LightGreen;
            btn_On.ForeColor = isOn ? Color.White : Color.Black;

            btn_Off.BackColor = !isOn ? Color.Red : Color.LightCoral;
            btn_Off.ForeColor = !isOn ? Color.White : Color.Black;
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            label_Message.Text = message;
            label_Message.ForeColor = isSuccess ? Color.Blue : Color.Red;

            // 3초 후 메시지 지우기
            var timer = new Timer();
            timer.Interval = 3000;
            timer.Tick += (s, e) =>
            {
                label_Message.Text = "";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        // 외부에서 호출 가능한 메서드들
        public void SetIntensity(int channelIndex, int intensity)
        {
            if (_selectedIlluminator == null || channelIndex < 0 || channelIndex >= _selectedIlluminator.Channels.Count)
                return;

            comboBox_Channel.SelectedIndex = channelIndex;
            trackBar_LightIntensity.Value = Math.Min(Math.Max(intensity, 0), 4095);

            if (_selectedIlluminator.IsConnected)
            {
                _selectedIlluminator.SetVolumeCommandString(channelIndex + 1, intensity);
            }
        }

        public void TurnOnChannel(int channelIndex)
        {
            if (_selectedIlluminator == null || channelIndex < 0)
                return;

            comboBox_Channel.SelectedIndex = channelIndex;
            btn_On_Click(null, null);
        }

        public void TurnOffChannel(int channelIndex)
        {
            if (_selectedIlluminator == null || channelIndex < 0)
                return;

            comboBox_Channel.SelectedIndex = channelIndex;
            btn_Off_Click(null, null);
        }
    }
}