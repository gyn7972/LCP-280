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

        public MapMatchDecisionDialog(double score, double threshold, string mapFilePath)
        {
            _score = score;
            _threshold = threshold;
            _mapFile = mapFilePath ?? string.Empty;

            InitializeComponent();
            ApplyRuntimeTexts();
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
            _btnStop.Location = new Point(right - _btnStop.Width, top);
            _btnContinue.Location = new Point(_btnStop.Left - _btnContinue.Width - 10, top); // 버튼 간격 10
        }
    }
}