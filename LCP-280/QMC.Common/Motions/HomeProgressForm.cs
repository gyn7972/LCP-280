using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.Common.Motions
{
    /// <summary>
    /// 공용 홈(또는 기타 장비 동작) 진행 표시 모달 폼
    /// - 진행률, 로그, 취소/강제정지 제공
    /// </summary>
    public class HomeProgressForm : Form
    {
        private ProgressBar _bar;
        private Label _lblTitle;
        private Label _lblStatus;
        private ListBox _list;
        private Button _btnCancel;
        private Button _btnForce;
        private Button _btnClose;

        private int _totalSteps;
        private bool _completed;
        private bool _canceled;
        private bool _aborted;

        public event Action CancelRequested;        // 1차 취소(토큰 Cancel)
        public event Action ForceStopRequested;    // 2차 강제 정지(EmgStopAll)

        public HomeProgressForm()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(620, 360);
            this.Text = "Operation Progress";
            this.MaximizeBox = false; this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            _lblTitle = new Label { Left = 12, Top = 10, AutoSize = true, Font = new Font(Font, FontStyle.Bold), Text = "Home" };
            _bar = new ProgressBar { Left = 12, Top = 38, Width = 460, Height = 20, Minimum = 0, Maximum = 1, Value = 0 };            
            _lblStatus = new Label { Left = 480, Top = 40, AutoSize = true, Text = "0/0" };

            _list = new ListBox { Left = 12, Top = 70, Width = 590, Height = 220, HorizontalScrollbar = true };            
            _btnCancel = new Button { Left = 12, Top = 300, Width = 90, Height = 28, Text = "Cancel" };
            _btnForce = new Button { Left = 108, Top = 300, Width = 90, Height = 28, Text = "Force Stop", Enabled = false };
            _btnClose = new Button { Left = 512, Top = 300, Width = 90, Height = 28, Text = "Close", Enabled = false };

            _btnCancel.Click += (s, e) =>
            {
                if (_completed) return;
                _canceled = true;
                _btnCancel.Enabled = false;
                _btnForce.Enabled = true;
                LogLine("Cancel requested...");
                var h = CancelRequested; if (h != null) h();
            };
            _btnForce.Click += (s, e) =>
            {
                if (_completed && !_aborted) return;
                _btnForce.Enabled = false;
                LogLine("Force stop requested...");
                var h = ForceStopRequested; if (h != null) h();
            };
            _btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { _lblTitle, _bar, _lblStatus, _list, _btnCancel, _btnForce, _btnClose });
        }

        private void LogLine(string text)
        {
            try
            {
                _list.Items.Add(DateTime.Now.ToString("HH:mm:ss ") + text);
                _list.TopIndex = _list.Items.Count - 1;
            }
            catch { }
        }

        public void InitializeProgress(string title, int totalSteps)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => InitializeProgress(title, totalSteps))); return; }
            _lblTitle.Text = title ?? "Operation";
            _totalSteps = totalSteps <= 0 ? 1 : totalSteps;
            _bar.Minimum = 0; _bar.Maximum = _totalSteps; _bar.Value = 0;
            _lblStatus.Text = $"0/{_totalSteps}";
            _completed = false; _canceled = false; _aborted = false;
            _btnCancel.Enabled = true; _btnForce.Enabled = false; _btnClose.Enabled = false;
            _list.Items.Clear();
            LogLine("Started");
        }

        public void SafeUpdate(OperationProgress p)
        {
            if (p == null) return;
            if (this.IsDisposed) return;
            if (InvokeRequired) { BeginInvoke(new Action(() => SafeUpdate(p))); return; }
            try
            {
                if (_bar.Maximum != p.TotalSteps) { _bar.Maximum = p.TotalSteps <= 0 ? 1 : p.TotalSteps; }
                if (p.IsStepCompleted)
                {
                    var val = Math.Min(p.StepIndex + 1, _bar.Maximum);
                    if (val >= 0 && val <= _bar.Maximum) _bar.Value = val;
                    _lblStatus.Text = $"{Math.Min(p.StepIndex + 1, _bar.Maximum)}/{_bar.Maximum}";
                    LogLine($"STEP {p.StepIndex + 1}/{_bar.Maximum} DONE | Fail:{p.StepFailCount} | {p.StepName}");
                }
                else if (!p.IsCompleted)
                {
                    LogLine($"STEP {p.StepIndex + 1} START - {p.StepName}");
                }

                if (p.IsCompleted)
                {
                    _completed = true; _aborted = p.IsAborted; _canceled = p.IsCanceled;
                    _btnCancel.Enabled = false; _btnForce.Enabled = false; _btnClose.Enabled = true;
                    string state = p.IsCanceled ? "Canceled" : p.IsAborted ? "Aborted" : "Completed";
                    LogLine(state + (string.IsNullOrEmpty(p.Message) ? string.Empty : (" - " + p.Message)));
                }
            }
            catch { }
        }
    }
}
