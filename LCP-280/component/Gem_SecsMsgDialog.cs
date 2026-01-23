using QMC.Common.GEMSecs;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    public partial class Gem_SecsMsgDialog : Form
    {
        private readonly XLinkGemService _gem;

        public Gem_SecsMsgDialog(XLinkGemService gem)
        {
            if (gem == null) throw new ArgumentNullException(nameof(gem));

            InitializeComponent();

            _gem = gem;

            WireEvents();
            WireGemEvents();
        }

        public void TerminalMsgPrint(string message)
        {
            if (IsDisposed) return;

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(() => TerminalMsgPrint(message))); } catch { }
                return;
            }

            var line = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + "  " + (message ?? "");
            lstMsg.Items.Add(line);

            if (lstMsg.Items.Count > 0)
                lstMsg.TopIndex = lstMsg.Items.Count - 1;

            if (lstMsg.Items.Count > 1000)
                lstMsg.Items.Clear();
        }

        private void WireEvents()
        {
            btnRemote.Click += (s, e) =>
            {
                try
                {
                    _gem.SetOnlineRemote();
                    TerminalMsgPrint("CONTROL => ONLINE_REMOTE");
                }
                catch (Exception ex)
                {
                    TerminalMsgPrint("REMOTE FAIL: " + ex.Message);
                }
            };

            btnLocal.Click += (s, e) =>
            {
                try
                {
                    _gem.SetOnlineLocal();
                    TerminalMsgPrint("CONTROL => ONLINE_LOCAL");
                }
                catch (Exception ex)
                {
                    TerminalMsgPrint("LOCAL FAIL: " + ex.Message);
                }
            };

            btnOffline.Click += (s, e) =>
            {
                try
                {
                    _gem.SetOffline();
                    TerminalMsgPrint("CONTROL => OFFLINE");
                }
                catch (Exception ex)
                {
                    TerminalMsgPrint("OFFLINE FAIL: " + ex.Message);
                }
            };

            btnClear.Click += (s, e) => lstMsg.Items.Clear();

            btnFtpUpload.Click += (s, e) =>
            {
                // 기존 MFC의 FTP Task/OptionFlag가 C#에 실제로 무엇으로 구현돼있는지
                // 이 파일 컨텍스트에서 확정이 불가하므로 "훅"만 제공
                // => 필요 시 EquipmentGemBridge/FTP uploader 호출로 교체
                TerminalMsgPrint("FTP Upload: Trigger requested (TODO: bridge to uploader).");
            };

            FormClosed += (s, e) =>
            {
                try
                {
                    _gem.CommunicationStateChanged -= Gem_CommunicationStateChanged;
                    _gem.ErrorEventReceived -= Gem_ErrorEventReceived;
                    _gem.ControlStateChanged -= Gem_ControlStateChanged;
                    _gem.TerminalMessageReceived -= Gem_TerminalMessageReceived;
                    _gem.SecsMessageReceived -= Gem_SecsMessageReceived;
                }
                catch { }
            };
        }

        private void WireGemEvents()
        {
            _gem.CommunicationStateChanged += Gem_CommunicationStateChanged;
            _gem.ErrorEventReceived += Gem_ErrorEventReceived;
            _gem.ControlStateChanged += Gem_ControlStateChanged;
            _gem.TerminalMessageReceived += Gem_TerminalMessageReceived;
            _gem.SecsMessageReceived += Gem_SecsMessageReceived;
        }

        private void Gem_CommunicationStateChanged(object sender, dynamic e)
            => TerminalMsgPrint($"COMM: code={e.Code}, msg={e.Message}");

        private void Gem_ErrorEventReceived(object sender, dynamic e)
            => TerminalMsgPrint($"ERROR: code={e.ErrorCode}, msg={e.ErrorText}");

        private void Gem_ControlStateChanged(object sender, dynamic e)
            => TerminalMsgPrint($"CONTROL: {e.ControlState}");

        private void Gem_TerminalMessageReceived(object sender, dynamic e)
            => TerminalMsgPrint($"TERMINAL({e.MsgId}): {string.Join(" | ", e.Lines ?? Array.Empty<string>())}");

        private void Gem_SecsMessageReceived(object sender, dynamic e)
            => TerminalMsgPrint($"SECS: Dev={e.DevId} S{e.Stream}F{e.Function} W={e.WBit} Sys={e.SysByte}");
    }
}