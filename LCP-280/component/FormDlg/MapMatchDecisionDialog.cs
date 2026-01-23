using System;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Component
{
    public partial class MapMatchDecisionDialog : Form
    {
        private readonly double _score;
        private readonly double _threshold;
        private readonly string _mapFile;
        // [ADD] 수동 맵매치 결과 전달용
        public FormMapMatchManual.ManualTransformSettings ManualSettings { get; private set; }

        public MapMatchDecisionDialog(double score, double threshold, string mapFilePath)
        {
            _score = score;
            _threshold = threshold;
            _mapFile = mapFilePath ?? string.Empty;

            InitializeComponent();
            ApplyRuntimeTexts();

            // [ADD] 런타임에 수동버튼 추가(디자이너 수정 없이)
            TryAddManualMapMatchButton();
        }

        private void ApplyRuntimeTexts()
        {
            var scoreText = $"Score = {_score:F2} (기준 = {_threshold:F2})";
            var fileText = string.IsNullOrWhiteSpace(_mapFile) ? "(맵파일 경로 없음)" : _mapFile;
            var compareText = _score >= _threshold ? "이상" : "미만";

            if (_lblDetails != null)
            {
                _lblDetails.Text =
                    $"다운로드 맵: {fileText}\r\n\r\n" +
                    $"{scoreText}\r\n" +
                    $"매칭 점수가 기준값 {compareText}입니다.\r\n" +
                    $"시퀀스를 계속 진행하시겠습니까?";
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            try
            {
                if (_score < _threshold)
                {
                    if (_btnStop != null) _btnStop.Font = new Font(_btnStop.Font, FontStyle.Bold);
                }
                else
                {
                    if (_btnContinue != null) _btnContinue.Font = new Font(_btnContinue.Font, FontStyle.Bold);
                }
            }
            catch { /* ignore */ }
        }

        // 폼 로드 시 버튼 초기 위치 재배치
        private void MapMatchDecisionDialog_Load(object sender, EventArgs e)
        {
            RepositionButtons();
        }

        // 패널 리사이즈 시 오른쪽 정렬 유지
        private void _panelButtons_Resize(object sender, EventArgs e)
        {
            RepositionButtons();
        }

        // 공통 재배치 로직
        private void RepositionButtons()
        {
            if (_panelButtons == null || _btnStop == null || _btnContinue == null) return;

            int right = _panelButtons.ClientSize.Width - 12; // 오른쪽 여백 12
            int top = 12;

            // Stop(오른쪽)
            _btnStop.Location = new Point(right - _btnStop.Width, top);

            // Continue(Stop 왼쪽)
            _btnContinue.Location = new Point(_btnStop.Left - _btnContinue.Width - 10, top);

            // Manual(Continue 왼쪽) - 있으면 같이 정렬
            var manualBtn = _panelButtons.Controls["btnManualMapMatch"] as Button;
            if (manualBtn != null)
            {
                manualBtn.Location = new Point(_btnContinue.Left - manualBtn.Width - 10, top);
            }
        }

        // =========================
        // [ADD] Manual MapMatch Button
        // =========================
        private void TryAddManualMapMatchButton()
        {
            try
            {
                if (_panelButtons == null) return;
                if (_panelButtons.Controls["btnManualMapMatch"] != null) return;

                var btn = new Button
                {
                    Name = "btnManualMapMatch",
                    Text = "수동 맵매치",
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btn.Click += BtnManualMapMatch_Click;

                // 점수가 기준 이상이면 굳이 수동을 띄울 필요 없으니 비활성(원하면 항상 활성로 바꿔도 됨)
                btn.Enabled = (_score < _threshold);

                _panelButtons.Controls.Add(btn);
                btn.BringToFront();

                RepositionButtons();
            }
            catch { /* ignore */ }
        }

        private void BtnManualMapMatch_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fm = new FormMapMatchManual())
                {
                    // 카메라/맵파일 연동
                    try { fm.BindEquipmentInStageCamera(); } catch { }
                    try { fm.SetDownloadedMapFile(_mapFile); } catch { }

                    FormMapMatchManual.ManualTransformSettings applied = null;
                    fm.ManualMatchApplied += (s, arg) => { applied = arg?.Settings; };

                    var dr = fm.ShowDialog(this);
                    if (dr == DialogResult.OK && applied != null)
                    {
                        ManualSettings = applied;

                        // 수동 값 적용하겠다는 의미로 Continue 처리
                        DialogResult = DialogResult.Yes;
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("수동 맵매치 실행 실패: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}