using QMC.Common;
using QMC.Common.GEMSecs;
using QMC.LCP_280.Process.Component;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    [FormOrder(7)]
    public partial class Gem_Setup : Form
    {
        private readonly XLinkGemService _gem = new XLinkGemService();
        private XLinkGemServiceConfig _cfg;

        public Gem_Setup()
        {
            InitializeComponent();
            WireEvents();

            // 폼 종료 시 자원 정리 (COM/Thread)
            this.FormClosed += Gem_Setup_FormClosed;
        }


        private void GEM_Setup_Load(object sender, EventArgs e)
        {
            try
            {
                // 기본 로드
                LoadConfigFromDisk();
                ApplyConfigToUi();
                AppendLog("GEM UI Ready.");
            }
            catch (Exception ex)
            {
                AppendLog("Load error: " + ex.Message);
            }
        }

        private void Gem_Setup_FormClosed(object sender, FormClosedEventArgs e)
        {
            try { _gem.Stop(); } catch { }
            try { _gem.Dispose(); } catch { }
        }

        // FormConfig가 child 폼 크기 조절할 때 호출됨
        public void SetPanelSize(int width, int height)
        {
            try
            {
                this.Size = new System.Drawing.Size(width, height);
                this.ClientSize = new System.Drawing.Size(width, height);
            }
            catch { }
        }

        private void EnsureConfigLoaded()
        {
            if (_cfg != null) return;

            _cfg = XLinkGemServiceConfig.LoadOrCreate("XLinkGEM");
            AppendLog("Config loaded: " + _cfg.GetFilePath());
        }

        private void WireEvents()
        {
            btnLoadConfig.Click += (s, e) =>
            {
                Safe("LoadConfig", () =>
                {
                    _cfg = null;
                    EnsureConfigLoaded();
                    ApplyConfigToUi();
                });
            };

            btnSaveConfig.Click += (s, e) =>
            {
                Safe("SaveConfig", () =>
                {
                    EnsureConfigLoaded();
                    ApplyUiToConfig();
                    SaveConfigToDisk();
                });
            };

            btnApplyConfig.Click += (s, e) =>
            {
                Safe("ApplyConfig", () =>
                {
                    EnsureConfigLoaded();
                    ApplyUiToConfig();
                    ApplyServiceConfig();
                });
            };

            btnBrowseLogPath.Click += (s, e) => BrowseFolder(txtLogPath);
            btnBrowseSecsDir.Click += (s, e) => BrowseFolder(txtSecsDir);

            btnCreate.Click += (s, e) => Safe("Create", () => _gem.Create());

            btnStart.Click += (s, e) =>
            {
                Safe("Start", () =>
                {
                    EnsureConfigLoaded();
                    ApplyUiToConfig(); // UI값 반영 후 시작
                    _gem.StartWithConfig(_cfg, setOnlineRemote: false);
                });
            };

            btnStop.Click += (s, e) => Safe("Stop", () => _gem.Stop());

            btnOffline.Click += (s, e) => Safe("Offline", () => _gem.SetOffline());
            btnOnlineLocal.Click += (s, e) => Safe("OnlineLocal", () => _gem.SetOnlineLocal());
            btnOnlineRemote.Click += (s, e) => Safe("OnlineRemote", () => _gem.SetOnlineRemote());

            btnInitDefinitions.Click += (s, e) =>
            {
                Safe("InitDefinitions", () =>
                {
                    EnsureConfigLoaded();

                    var dir = (txtSecsDir.Text ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                        throw new DirectoryNotFoundException("SecsDirectory not found: " + dir);

                    if (!_gem.IsStarted)
                        AppendLog("주의: GEM이 Start 되지 않았습니다. Start 후 InitDefinitions 권장.");

                    _gem.InitializeGemDefinitions(dir, _cfg.TimeFormatDigits, setCustomSecsMessageAll: true, startSecsPolling: true);
                });
            };

            btnSendCeid.Click += (s, e) =>
            {
                Safe("Send CEID", () =>
                {
                    int ceid = (int)nudCeid.Value;
                    bool ok = _gem.EventReport(ceid);
                    AppendLog($"EventReport({ceid}) => {ok}");
                });
            };

            btnSetSvid.Click += (s, e) =>
            {
                Safe("Set SVID", () =>
                {
                    int svid = (int)nudSvid.Value;
                    string val = txtSvidValue.Text ?? "";
                    bool ok = _gem.SetVariableValue(svid, val);
                    AppendLog($"SetVariableValue({svid}, '{val}') => {ok}");
                });
            };

            chkDiagnostics.CheckedChanged += (s, e) =>
            {
                _gem.DiagnosticsEnabled = chkDiagnostics.Checked;
                AppendLog("DiagnosticsEnabled=" + _gem.DiagnosticsEnabled);
            };

            // GEM 이벤트 -> 로그 출력
            _gem.CommunicationStateChanged += (s, e) => UiLog($"COMM: code={e.Code}, msg={e.Message}");
            _gem.ErrorEventReceived += (s, e) => UiLog($"ERROR: code={e.ErrorCode}, msg={e.ErrorText}");
            _gem.ControlStateChanged += (s, e) => UiLog($"CONTROL: {e.ControlState}");
            _gem.TerminalMessageReceived += (s, e) => UiLog($"TERMINAL({e.MsgId}): {string.Join(" | ", e.Lines ?? Array.Empty<string>())}");
            _gem.RemoteCommandReceived += (s, e) =>
            {
                UiLog($"RCMD: msgId={e.MsgId}, cmd='{e.Command}', paramCount={e.ParamCount}");
                try
                {
                    var ps = _gem.ReadRemoteCommandParams();
                    foreach (var p in ps) UiLog($"  PARAM[{p.Index}] {p.Name}='{p.Value}'");
                }
                catch (Exception ex)
                {
                    UiLog("  PARAM read fail: " + ex.Message);
                }
            };
            _gem.EquipmentConstantsChanged += (s, e) =>
            {
                var sb = new StringBuilder();
                sb.Append($"ECID_CHANGE msgId={e.MsgId}: ");
                foreach (var c in e.Changes)
                    sb.Append($"[{c.Ecid}='{c.Value}'] ");
                UiLog(sb.ToString());
            };
            _gem.SecsMessageReceived += (s, e) => UiLog($"SECS: Dev={e.DevId} S{e.Stream}F{e.Function} W={e.WBit} Sys={e.SysByte}");
        }

        private void LoadConfigFromDisk()
        {
            _cfg = XLinkGemServiceConfig.LoadOrCreate("XLinkGEM");
            AppendLog("Config loaded: " + _cfg.GetFilePath());
        }

        private void SaveConfigToDisk()
        {
            _cfg.Reset();
            _cfg.Validate();
            _cfg.Save();
            AppendLog("Config saved: " + _cfg.GetFilePath());
        }

        private void ApplyServiceConfig()
        {
            _gem.SetConfig(_cfg);
            _gem.Create();
            _gem.ApplyConfig();
            AppendLog("ApplyConfig OK");
        }

        private void ApplyConfigToUi()
        {
            if (_cfg == null) 
                return;

            chkEnable.Checked = _cfg.Enable;

            // 모드 선택값이 비었으면 기본 0(Active)
            cmbMode.SelectedIndex = (_cfg.Mode == XLinkGemServiceConfig.HsmsMode.Active) ? 0 : 1;
            if (cmbMode.SelectedIndex < 0) 
                cmbMode.SelectedIndex = 0;

            txtIp.Text = _cfg.Ip ?? "";
            nudPort.Value = Clamp(nudPort, _cfg.Port);
            nudDevId.Value = Clamp(nudDevId, _cfg.DevId);

            txtModelName.Text = _cfg.ModelName ?? "";
            txtSoftRev.Text = _cfg.SoftRev ?? "";

            nudT3.Value = Clamp(nudT3, _cfg.T3);
            nudT5.Value = Clamp(nudT5, _cfg.T5);
            nudT6.Value = Clamp(nudT6, _cfg.T6);
            nudT7.Value = Clamp(nudT7, _cfg.T7);
            nudT8.Value = Clamp(nudT8, _cfg.T8);
            nudLinkTest.Value = Clamp(nudLinkTest, _cfg.LinkTestInterval);
            nudEstablish.Value = Clamp(nudEstablish, _cfg.EstablishTimeout);

            // 0/1/2
            var tf = _cfg.TimeFormatDigits;
            if (tf < 0 || tf > 2) tf = 1;
            cmbTimeFormat.SelectedIndex = tf;

            chkLogEnabled.Checked = _cfg.LogEnabled;
            txtLogPath.Text = _cfg.LogPath ?? "";
            txtLogPrefix.Text = _cfg.LogPrefix ?? "";
            nudLogKeepDays.Value = Clamp(nudLogKeepDays, _cfg.LogKeepDays);

            // 기본값 편의
            if (string.IsNullOrWhiteSpace(txtSecsDir.Text))
                txtSecsDir.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Secs", "Config");

            chkDiagnostics.Checked = _gem.DiagnosticsEnabled;
        }

        private void ApplyUiToConfig()
        {
            if (_cfg == null) _cfg = new XLinkGemServiceConfig();

            _cfg.Enable = chkEnable.Checked;

            // 모드 선택값이 비었으면 기본 0(Active)
            cmbMode.SelectedIndex = (_cfg.Mode == XLinkGemServiceConfig.HsmsMode.Active) ? 0 : 1;
            if (cmbMode.SelectedIndex < 0) cmbMode.SelectedIndex = 0;

            _cfg.Mode = (cmbMode.SelectedIndex == 1) ? XLinkGemServiceConfig.HsmsMode.Passive : XLinkGemServiceConfig.HsmsMode.Active;
            
            _cfg.Ip = (txtIp.Text ?? "").Trim();
            _cfg.Port = (short)nudPort.Value;
            _cfg.DevId = (short)nudDevId.Value;

            _cfg.ModelName = txtModelName.Text ?? "";
            _cfg.SoftRev = txtSoftRev.Text ?? "";

            _cfg.T3 = (short)nudT3.Value;
            _cfg.T5 = (short)nudT5.Value;
            _cfg.T6 = (short)nudT6.Value;
            _cfg.T7 = (short)nudT7.Value;
            _cfg.T8 = (short)nudT8.Value;
            _cfg.LinkTestInterval = (short)nudLinkTest.Value;
            _cfg.EstablishTimeout = (short)nudEstablish.Value;

            _cfg.TimeFormatDigits = (short)Math.Max(0, Math.Min(2, cmbTimeFormat.SelectedIndex));

            _cfg.LogEnabled = chkLogEnabled.Checked;
            _cfg.LogPath = txtLogPath.Text ?? "";
            _cfg.LogPrefix = txtLogPrefix.Text ?? "GEM";
            _cfg.LogKeepDays = (short)nudLogKeepDays.Value;

            _cfg.Reset(); // 경로 자동 보정/기본값
        }

        private void BrowseFolder(TextBox target)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = (target.Text ?? "").Trim();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    target.Text = dlg.SelectedPath;
            }
        }

        private void Safe(string title, Action action)
        {
            try
            {
                action();
                AppendLog($"{title}: OK");
            }
            catch (Exception ex)
            {
                AppendLog($"{title}: FAIL - {ex.Message}");
            }
        }

        private void UiLog(string msg)
        {
            if (this.IsDisposed) return;
            if (this.InvokeRequired)
            {
                try { this.BeginInvoke(new Action(() => AppendLog(msg))); } catch { }
                return;
            }
            AppendLog(msg);
        }

        private void AppendLog(string msg)
        {
            var line = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  " + msg;
            txtLog.AppendText(line + Environment.NewLine);
        }

        private static decimal Clamp(NumericUpDown nud, int value)
        {
            var v = (decimal)value;
            if (v < nud.Minimum) v = nud.Minimum;
            if (v > nud.Maximum) v = nud.Maximum;
            return v;
        } 

        private static decimal Clamp(NumericUpDown nud, short value) 
            => Clamp(nud, (int)value);

        private void btnGEMDlg_Click(object sender, EventArgs e)
        {
            //test
            var dlg = new FormGemClient();
            dlg.ShowDialog();

            //if (_secsMsgDlg == null || _secsMsgDlg.IsDisposed)
            //    _secsMsgDlg = new Gem_SecsMsgDialog(_gem);

            //_secsMsgDlg.Show();
            //_secsMsgDlg.BringToFront();
        }
    }
}
