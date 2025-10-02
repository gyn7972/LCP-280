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

namespace QMC.LCP_280.Process.Unit
{
    public partial class LightChannelControlcs : UserControl
    {
        private ConfigReflectionMapper _illuminatorChannelConfigMapper;

        private LeesOsLightController _selectedIlluminator;
        private string _illuminatorName;
        private List<string> _channelNames;
        private int _selectedChannelIndex = -1;

        public LightChannelControlcs()
        {
            InitializeComponent();
            InitializeUI();
        }

        // ===== Initialize =====
        private void InitializeUI()
        {
            try
            {
                WireIlluminatorEvents();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        public void BinIlluminatorChannelList(LeesOsLightController lightController, string illuminatorName)
        {
            try
            {
                _illuminatorName = illuminatorName;

                //_selectedIlluminator = Equipment.Instance.LightControllers[_illuminatorName];
                _selectedIlluminator = lightController;

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

                    _illuminatorChannelConfigMapper = null;

                    UpdateConnectionStatus();

                    ShowStatusMessage($"선택됨: {_selectedIlluminator.Model} ({_selectedIlluminator.Channels.Count}채널)", true);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Vision_Setup", $"BinIlluminatorList error: {ex}");
            }
        }

        public void WireIlluminatorEvents()
        {

        }

        private void ShowStatusMessage(string message, bool isSuccess)
        {
            try
            {
                if (label2 != null)
                {
                    string baseTitle =
                        (_selectedIlluminator != null && _selectedIlluminator.IsConnected)
                        ? $"Control - 연결됨 ({_selectedIlluminator.Config?.PortName})"
                        : "Control - 연결 안됨";

                    label2.Text = $"{baseTitle}  |  {message}";

                    var timer = new System.Windows.Forms.Timer();
                    timer.Interval = 3000;
                    timer.Tick += (s, e) =>
                    {
                        string restoreTitle =
                            (_selectedIlluminator != null && _selectedIlluminator.IsConnected)
                            ? $"Control - 연결됨 ({_selectedIlluminator.Config?.PortName})"
                            : "Control - 연결 안됨";

                        label2.Text = restoreTitle;

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


        private void UpdateConnectionStatus()
        {
            try
            {
                if (_selectedIlluminator != null)
                {
                    bool isConnected = _selectedIlluminator.IsConnected;

                    if (label2 != null)
                    {
                        label2.Text = isConnected ?
                            $"Control - 연결됨 ({_selectedIlluminator.Config.PortName})" :
                            "Control - 연결 안됨";
                        label2.ForeColor = isConnected ?
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
    }
}
