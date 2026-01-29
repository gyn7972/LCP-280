using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.PKGTester;
using QMC.LCP_280.Process.Unit.FormWork.Repro;

namespace QMC.LCP_280.Process.Unit.FormWork
{
    public partial class IndexCalibrationDialog : Form
    {
        private readonly ManualSeqReproTestRunner _runner;
        private readonly List<DataGridView> _measureBlocks = new List<DataGridView>();
        private bool _running;
        private int _targetCycleCount = 1;

        // SPEC 범위 캐시
        private double _specVfMin = -0.03, _specVfMax = 0.03;
        private double _specWMin = -5, _specWMax = 5;
        private double _specWdwpMin = -0.1, _specWdwpMax = 0.1;
        private double _specTovMin = -0.03, _specTovMax = 0.03;

        private bool _stopRequestedByUser = false;

        public IndexCalibrationDialog(ManualSeqReproTestRunner runner)
        {
            InitializeComponent();
            _runner = runner ?? throw new ArgumentNullException(nameof(runner));

            // [추가] 시스템 최소화 버튼 활성화
            this.ControlBox = true;           // 우상단 시스템 박스 표시
            this.MinimizeBox = true;          // 최소화 버튼 활성화
            this.MaximizeBox = false;         // 필요 시 최대화 비활성화(다이얼로그 성격이면 일반적으로 false)
            this.ShowInTaskbar = true;        // 작업 표시줄에 표시(최소화 시 필요)
         // this.FormBorderStyle = FormBorderStyle.Sizable; // 최소화에는 필요없으나, 다이얼로그 기본은 FixedDialog일 수 있음
                                              // FixedDialog여도 최소화 버튼은 표시/동작함(제조사 커스텀 테마에 따라 다를 수 있음)


            InitGrids();
            InitSpecRows();
            InitCopyRows();

            btnStart.Click += (s, e) => OnStartClicked();
            btnStop.Click += (s, e) => OnStopClicked();
            btnApply.Click += (s, e) => OnApplyClicked();
            btnSave.Click += (s, e) => OnSaveClicked();
            txtCount.TextChanged += (s, e) =>
            {
                if (_running) 
                    return; // [추가] 실행 중에는 재구성 금지 (데이터 보존)
                RebuildLeftBlocks();
            };

            _runner.SocketAdvanced += OnSocketAdvanced;
            _runner.MeasurementCompleted += OnMeasurementCompleted; // 추가: 측정 완료 구독
                                                                    // 핵심: Runner 신호로 Start/Stop 제어 동기화
            _runner.RunningChanged += on =>
            {
                if (IsDisposed) 
                    return;

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() =>
                    {
                        _running = on;
                        UpdateButtons();
                        txtCount.Enabled = !on;
                        if (!on) _stopRequestedByUser = false;
                    }));
                    return;
                }

                _running = on;
                UpdateButtons();
                txtCount.Enabled = !on;

                if (on == false)
                {
                    _stopRequestedByUser = false;
                }
            };

            UpdateButtons();
            RebuildLeftBlocks();
        }

        private void UpdateButtons()
        {
            // [FIX] Dialog의 _running 변수보다 Runner의 실제 상태를 우선
            bool running = _runner != null && _runner.IsRunning;

            btnStart.Enabled = !_running;
            btnStop.Enabled = _running;
            btnApply.Enabled = true;
            btnSave.Enabled = true;
        }

        // 좌측 블록 동적 구성 (Count에 맞춰 생성/제거)
        private void RebuildLeftBlocks(bool preserveData = false)
        {
            int count = ParseCountOrDefault();
            _targetCycleCount = count;

            // 백업: 기존 값 보존용
            List<string[,]> backups = null;
            if (preserveData && _measureBlocks.Count > 0)
            {
                backups = new List<string[,]>(_measureBlocks.Count);
                foreach (var old in _measureBlocks)
                {
                    var buf = new string[old.Rows.Count, old.Columns.Count];
                    for (int r = 0; r < old.Rows.Count; r++)
                        for (int c = 0; c < old.Columns.Count; c++)
                            buf[r, c] = old.Rows[r].Cells[c].Value?.ToString();
                    backups.Add(buf);
                }
            }

            // 레이아웃 일시 중지로 성능/플리커 개선
            flLeftBlocks.SuspendLayout();
            try
            {
                // 기존 제거
                foreach (var dgv in _measureBlocks)
                {
                    flLeftBlocks.Controls.Remove(dgv);
                    dgv.Dispose();
                }
                _measureBlocks.Clear();

                // 생성 폭 계산 (패딩 고려)
                int targetWidth = Math.Max(0, flLeftBlocks.ClientSize.Width - flLeftBlocks.Padding.Horizontal);

                for (int i = 0; i < count; i++)
                {
                    var dgv = CreateMeasureGrid(true);
                    EnableDoubleBuffered(dgv);
                    dgv.Margin = new Padding(0, 4, 0, 4);
                    dgv.Height = 115;
                    dgv.Width = targetWidth;
                    dgv.MinimumSize = new Size(600, 100);
                    InitMeasureRows(dgv);

                    _measureBlocks.Add(dgv);
                    flLeftBlocks.Controls.Add(dgv);
                    dgv.BringToFront();
                }

                // [변경] 기존 데이터 보존
                if (preserveData && backups != null)
                {
                    int copyCount = Math.Min(backups.Count, _measureBlocks.Count);
                    for (int i = 0; i < copyCount; i++)
                    {
                        var src = backups[i];
                        var dst = _measureBlocks[i];
                        int rows = Math.Min(dst.Rows.Count, src.GetLength(0));
                        int cols = Math.Min(dst.Columns.Count, src.GetLength(1));
                        for (int r = 0; r < rows; r++)
                            for (int c = 0; c < cols; c++)
                                dst.Rows[r].Cells[c].Value = src[r, c];
                    }
                }

                // [변경] AVG/OFFSET/COPY 재계산만 수행, 초기화 제거
                RecomputeAvgFromLeftBlocks();
                RecomputeOffsetFromAvg();
                UpdateCopyAndSpecColors();

            }
            finally
            {
                flLeftBlocks.ResumeLayout(performLayout: true);
            }
        }

        // FlowLayoutPanel 크기 변경 시 좌측 그리드 폭을 재조정
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            flLeftBlocks.SizeChanged += (_, __) =>
            {
                int targetWidth = Math.Max(0, flLeftBlocks.ClientSize.Width - flLeftBlocks.Padding.Horizontal);
                foreach (var dgv in _measureBlocks)
                {
                    if (!dgv.IsDisposed)
                        dgv.Width = targetWidth;
                }
            };
        }

        private void OnMinimizeClicked()
        {
            this.WindowState = FormWindowState.Minimized;
        }

        // DataGridView DoubleBuffered 활성화 (플리커 줄임)
        private static void EnableDoubleBuffered(DataGridView dgv)
        {
            try
            {
                typeof(DataGridView).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    ?.SetValue(dgv, true, null);
            }
            catch { /* 무시 (버전에 따라 실패 가능) */ }
        }

        private int ParseCountOrDefault()
        {
            if (int.TryParse(txtCount.Text.Trim(), out var cnt) && cnt > 0 && cnt <= 10) return cnt;
            return 5;
        }

        // 공통 그리드 속성/헤더 구성
        private void InitGrids()
        {
            // 오른쪽 그리드 구성
            SetupMeasureGrid(dgvAvg, false);
            SetupMeasureGrid(dgvOffset, false);
            InitAvgOffsetRows(dgvAvg);
            InitAvgOffsetRows(dgvOffset);

            // SPEC
            dgvSpec.AllowUserToAddRows = false;
            dgvSpec.AllowUserToDeleteRows = false;
            dgvSpec.RowHeadersVisible = false;
            dgvSpec.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvSpec.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSpec.Columns.Clear();
            dgvSpec.Columns.Add("Item", "Item");
            dgvSpec.Columns.Add("VF", "VF");
            dgvSpec.Columns.Add("Watt", "Watt");
            dgvSpec.Columns.Add("WDWP", "WD/WP");
            dgvSpec.Columns.Add("TOV", "TOV");

            // COPY
            dgvCopy.AllowUserToAddRows = false;
            dgvCopy.AllowUserToDeleteRows = false;
            dgvCopy.RowHeadersVisible = false;
            dgvCopy.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCopy.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvCopy.Columns.Clear();
            dgvCopy.Columns.Add("Index", "Index");
            dgvCopy.Columns.Add("VF", "VF");
            dgvCopy.Columns.Add("Watt", "Watt");
            dgvCopy.Columns.Add("WDWP", "WD/WP");
            dgvCopy.Columns.Add("TOV", "TOV");
        }

        private DataGridView CreateMeasureGrid(bool includeNoItem)
        {
            var dgv = new DataGridView();
            SetupMeasureGrid(dgv, includeNoItem);
            return dgv;
        }

        private void SetupMeasureGrid(DataGridView dgv, bool includeNoItem)
        {
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.Columns.Clear();
            var headers = includeNoItem
                ? new[] { "NO", "Item", "Index1", "Index2", "Index3", "Index4", "Index5", "Index6", "Index7", "Index8" }
                : new[] { "Item", "Index1", "Index2", "Index3", "Index4", "Index5", "Index6", "Index7", "Index8" };
            foreach (var h in headers) dgv.Columns.Add(h, h);
        }

        private void InitMeasureRows(DataGridView dgv)
        {
            dgv.Rows.Clear();
            dgv.Rows.Add("1", "VF", "", "", "", "", "", "", "", "");
            dgv.Rows.Add("2", "Watt", "", "", "", "", "", "", "", "");
            dgv.Rows.Add("3", "WD/WP", "", "", "", "", "", "", "", "");
            dgv.Rows.Add("4", "TOV", "", "", "", "", "", "", "", "");
        }

        private void InitAvgOffsetRows(DataGridView dgv)
        {
            dgv.Rows.Clear();
            dgv.Rows.Add("VF", "", "", "", "", "", "", "", "");
            dgv.Rows.Add("Watt", "", "", "", "", "", "", "", "");
            dgv.Rows.Add("WD/WP", "", "", "", "", "", "", "", "");
            dgv.Rows.Add("TOV", "", "", "", "", "", "", "", "");
        }

        private void InitSpecRows()
        {
            dgvSpec.Rows.Clear();
            dgvSpec.Rows.Add("Min", _specVfMin.ToString(CultureInfo.InvariantCulture),
                                   _specWMin.ToString(CultureInfo.InvariantCulture),
                                   _specWdwpMin.ToString(CultureInfo.InvariantCulture),
                                   _specTovMin.ToString(CultureInfo.InvariantCulture));
            dgvSpec.Rows.Add("Max", _specVfMax.ToString(CultureInfo.InvariantCulture),
                                   _specWMax.ToString(CultureInfo.InvariantCulture),
                                   _specWdwpMax.ToString(CultureInfo.InvariantCulture),
                                   _specTovMax.ToString(CultureInfo.InvariantCulture));
        }

        private void InitCopyRows()
        {
            dgvCopy.Rows.Clear();
            for (int i = 1; i <= 8; i++) dgvCopy.Rows.Add(i, "", "", "", "");
        }

        private void ClearAvgOffsetCopy()
        {
            InitAvgOffsetRows(dgvAvg);
            InitAvgOffsetRows(dgvOffset);
            InitCopyRows();
        }

        private int _currentBlockIndex = 0;
        private int _measureCountInCurrentBlock = 0; // 현재 블록 내 측정 완료 건수(0~8)

        // Start/Stop/Apply/Save
        private void OnStartClicked()
        {
            // [UX] 이미 실행 중이면 무시 + 안내
            if (_runner != null && _runner.IsRunning)
            {
                MessageBox.Show(
                    "이미 Index Calibration 시퀀스가 동작 중입니다.\n중지하려면 Stop을 눌러주세요.",
                    "실행 중",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                UpdateButtons(); // 혹시 버튼 상태가 어긋나 있으면 즉시 동기화
                return;
            }

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("INFO.", $"Index Cal을 시작하시겠습니까?") != DialogResult.Yes)
            {
                return;
            }


            // 1) UI 입력 반영
            _targetCycleCount = ParseCountOrDefault();
            _runner.RepeatCycleCount = _targetCycleCount;
            _runner.MeasureDelayMs = 0;
            _runner.AlignBeforeLoad = false;

            // 시작 시 블록/카운터 초기화
            _currentBlockIndex = 0;
            _measureCountInCurrentBlock = 0;

            // 2) UI/데이터 초기화
            ClearAvgOffsetCopy();
            foreach (var dgv in _measureBlocks)
                InitMeasureRows(dgv);

            // 3) 러너 내부 상태/유닛 초기화
            try { _runner.ResetForNewRun(); } catch { /* 러너에 메서드 없으면 무시 */ }

            // 4) 시작
            try
            {
                _runner.Start();

                // RunningChanged 이벤트로 버튼 상태가 갱신되지만, 즉시 갱신도 수행
                _running = true;
                UpdateButtons();

                // 실행 중 Count 변경 방지
                txtCount.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Start 실패: " + ex.Message);
                _running = false;
                UpdateButtons();
                txtCount.Enabled = true;
            }
        }

        //private void OnStartClicked()
        //{
        //    // 1) UI 입력 반영
        //    _targetCycleCount = ParseCountOrDefault();
        //    _runner.RepeatCycleCount = _targetCycleCount;
        //    _runner.MeasureDelayMs = 0;
        //    _runner.AlignBeforeLoad = false;

        //    // 시작 시 블록/카운터 초기화
        //    _currentBlockIndex = 0;
        //    _measureCountInCurrentBlock = 0;

        //    // 2) UI/데이터 초기화
        //    ClearAvgOffsetCopy();
        //    foreach (var dgv in _measureBlocks)
        //        InitMeasureRows(dgv);

        //    // 3) 러너 내부 상태/유닛 초기화
        //    try { _runner.ResetForNewRun(); } catch { /* 러너에 메서드 없으면 무시 */ }

        //    // 4) 시작
        //    try
        //    {
        //        _runner.Start();
        //        // RunningChanged 이벤트로 버튼 상태가 갱신되지만, 즉시 갱신도 수행
        //        _running = true;
        //        UpdateButtons();
        //        // 실행 중 Count 변경 방지
        //        txtCount.Enabled = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Start 실패: " + ex.Message);
        //        _running = false;
        //        UpdateButtons();
        //        txtCount.Enabled = true;
        //    }
        //}

        private void OnStopClicked()
        {
            try
            {
                // 사용자 Stop 표시 → 종료 시 재시작 메시지 차단
                _stopRequestedByUser = true;
                _runner.Stop();
            }
            catch { }

            // [FIX] 여기서 _running/버튼/Count를 강제로 바꾸지 않음.
            // 실제 종료 시점은 Runner.RunningChanged(false)에서 일괄 동기화.
            // 실제 버튼/입력 상태는 RunningChanged(false)에서 최종 동기화
            //_running = false;
            //UpdateButtons();
            //txtCount.Enabled = true;
        }

        private void OnApplyClicked()
        {
            try
            {
                // OFFSET → 레시피 오프셋에 반영 (예: Index별 VF/Watt/WDWP/TOV 오프셋 저장)
                // 실제 저장 대상 API는 장비 레시피 구조에 맞춰 교체 필요. 예시로 파일로 기록.
                var recipeDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "IndexCal");
                Directory.CreateDirectory(recipeDir);
                var file = Path.Combine(recipeDir, "RecipeOffset_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
                using (var w = new StreamWriter(file, false, Encoding.UTF8))
                {
                    w.WriteLine("Item,Index2,Index3,Index4,Index5,Index6,Index7,Index8");
                    for (int r = 0; r < 4; r++)
                    {
                        var item = dgvOffset.Rows[r].Cells[0].Value?.ToString() ?? "";
                        var vals = Enumerable.Range(2, 7)
                            .Select(c => dgvOffset.Rows[r].Cells[c - 1].Value?.ToString() ?? "")
                            .ToArray(); // c-1: OFFSET의 헤더는 Item,Index1..Index8이므로 Index2..8은 셀 2..8
                        w.WriteLine(string.Join(",", new[] { item }.Concat(vals)));
                    }
                }
                MessageBox.Show("레시피 오프셋 적용 및 임시 저장 완료.");
            }
            catch (Exception ex) { MessageBox.Show("적용 실패: " + ex.Message); }
        }

        private void OnSaveClicked()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var dir = Path.Combine(baseDir, "IndexCal");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, "IndexCal_UI_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

                using (var w = new StreamWriter(file, false, Encoding.UTF8))
                {
                    w.WriteLine("# Index Calibration UI Export");
                    foreach (var (dgv, name) in EnumerateAllTables())
                    {
                        w.WriteLine("# " + name);
                        WriteGridCsv(w, dgv);
                        w.WriteLine();
                    }
                }
                MessageBox.Show("저장 완료: " + file);
            }
            catch (Exception ex) { MessageBox.Show("저장 실패: " + ex.Message); }
        }

        private IEnumerable<(DataGridView dgv, string name)> EnumerateAllTables()
        {
            for (int i = 0; i < _measureBlocks.Count; i++)
                yield return (_measureBlocks[i], $"Block{i + 1}");
            yield return (dgvAvg, "AVG");
            yield return (dgvOffset, "OFFSET");
            yield return (dgvSpec, "SPEC");
            yield return (dgvCopy, "COPY");
        }

        private static void WriteGridCsv(StreamWriter w, DataGridView dgv)
        {
            var headers = new List<string>();
            foreach (DataGridViewColumn col in dgv.Columns) headers.Add(col.HeaderText);
            w.WriteLine(string.Join(",", headers.Select(Sanitize)));

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                var cells = new List<string>();
                foreach (DataGridViewCell cell in row.Cells)
                    cells.Add(Sanitize(cell.Value?.ToString() ?? ""));
                w.WriteLine(string.Join(",", cells));
            }
        }

        private static string Sanitize(string s) => string.IsNullOrEmpty(s) ? "" : s.Replace(",", " ").Trim();

        // 러너 이벤트에 맞춰 값 반영
        private void OnSocketAdvanced(int socketOneBased)
        {
            // 결과 쓰기는 MeasurementCompleted에서만 수행
            RecomputeAvgFromLeftBlocks();
            RecomputeOffsetFromAvg();
            UpdateCopyAndSpecColors();

            //int sock = socketOneBased - 1; // 0-based
            //// 현재 진행 중 사이클의 블록 인덱스 추정: 소켓 8개가 끝나면 다음 블록
            //int cycleIndex = EstimateCurrentCycleIndex();
            //var activeBlock = (_measureBlocks.Count > cycleIndex) ? _measureBlocks[cycleIndex] : _measureBlocks.LastOrDefault();
            //if (activeBlock == null) return;

            //// 최신 결과 가져오기: 러너에서 직접 전달되지 않으므로 필요 시 주입/이벤트 확장
            //var result = GetLatestResult();

            //// [중요] 결과가 없으면 기존 데이터를 절대 덮어쓰지 않음
            //if (result != null)
            //{
            //    double? vf = TryGetMeasure(result, new[] { "VF3", "VF", "Vf", "ForwardVoltage" });
            //    double? watt = TryGetMeasure(result, new[] { "WATT", "Watt", "Power", "Pwr" });
            //    double? wdwp = TryGetMeasure(result, new[] { "WD", "WD/WP", "WD_WP", "WdWp", "WDWP" });
            //    double? tov = TryGetMeasure(result, new[] { "VF1", "TOV", "OverVoltage", "TestOV" });

            //    // 기존 값 유지: v==null이면 SetCell이 셀을 건드리지 않음
            //    SetCell(activeBlock, 0, sock + 2, vf);
            //    SetCell(activeBlock, 1, sock + 2, watt);
            //    SetCell(activeBlock, 2, sock + 2, wdwp);
            //    SetCell(activeBlock, 3, sock + 2, tov);
            //}

            //// 각 소켓 평균(모든 블록 기준) → AVG
            //RecomputeAvgFromLeftBlocks();
            //// AVG 기반 OFFSET 재계산
            //RecomputeOffsetFromAvg();
            //// COPY 업데이트 + SPEC 색상 표시
            //UpdateCopyAndSpecColors();
        }

        private int EstimateCurrentCycleIndex()
        {
            // 러너로부터 사이클 번호 이벤트가 없다면,
            // 현재 블록 내 누적 측정 개수를 기준(8개가 채워지면 다음 블록)
            return Math.Max(0, Math.Min(_currentBlockIndex, _measureBlocks.Count - 1));
        }

        private PKGTesterResult GetLatestResult() => null; // 실제 연계 필요시 이벤트/주입

        private static void SetCell(DataGridView dgv, int row, int col, double? v)
        {
            if (row < 0 || col < 0 || row >= dgv.Rows.Count || col >= dgv.Columns.Count) return;
            if (!v.HasValue) return; // [추가] 값이 없으면 셀을 변경하지 않음 (기존 값을 보존)
            dgv.Rows[row].Cells[col].Value = Format(v);
        }

        // AVG: 왼쪽 모든 블록의 Index 열에서 같은 소켓(열) 기준 평균 계산
        private void RecomputeAvgFromLeftBlocks()
        {
            for (int itemRow = 0; itemRow < 4; itemRow++)
            {
                for (int idxCol = 0; idxCol < 8; idxCol++)
                {
                    var values = new List<double>();
                    foreach (var block in _measureBlocks)
                    {
                        var s = block.Rows[itemRow].Cells[idxCol + 2].Value?.ToString();
                        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                            values.Add(d);
                    }
                    var avg = values.Count > 0 ? values.Average() : double.NaN;
                    dgvAvg.Rows[itemRow].Cells[idxCol + 1].Value = double.IsNaN(avg) ? "" : avg.ToString("0.####", CultureInfo.InvariantCulture);
                }
            }
        }

        // OFFSET: AVG Index1 기준으로 Index2~Index8 = IndexN - Index1
        private void RecomputeOffsetFromAvg()
        {
            for (int itemRow = 0; itemRow < 4; itemRow++)
            {
                double refVal = ParseCell(dgvAvg.Rows[itemRow].Cells[1]); // Index1 (AVG 테이블은 Item,Index1..Index8)
                for (int n = 2; n <= 8; n++)
                {
                    double valN = ParseCell(dgvAvg.Rows[itemRow].Cells[n]);
                    double diff = double.IsNaN(refVal) || double.IsNaN(valN) ? double.NaN : (valN - refVal);
                    dgvOffset.Rows[itemRow].Cells[n].Value = double.IsNaN(diff) ? "" : diff.ToString("0.####", CultureInfo.InvariantCulture);
                }
                // Index1(참조)는 0으로 표시
                dgvOffset.Rows[itemRow].Cells[1].Value = "0";
            }
        }

        // COPY: AVG를 복사하여 표시, SPEC 범위에 따라 색상 표시
        private void UpdateCopyAndSpecColors()
        {
            // AVG를 COPY 테이블 구조로 반영: 행 = Index(1..8), 열 = 항목(VF/Watt/WD/WP/TOV)
            for (int idx = 1; idx <= 8; idx++)
            {
                dgvCopy.Rows[idx - 1].Cells[0].Value = idx.ToString();
                dgvCopy.Rows[idx - 1].Cells[1].Value = dgvAvg.Rows[0].Cells[idx].Value; // VF
                dgvCopy.Rows[idx - 1].Cells[2].Value = dgvAvg.Rows[1].Cells[idx].Value; // Watt
                dgvCopy.Rows[idx - 1].Cells[3].Value = dgvAvg.Rows[2].Cells[idx].Value; // WD/WP
                dgvCopy.Rows[idx - 1].Cells[4].Value = dgvAvg.Rows[3].Cells[idx].Value; // TOV
            }

            // 색상 초기화
            foreach (DataGridViewRow row in dgvCopy.Rows)
                foreach (DataGridViewCell cell in row.Cells)
                    cell.Style.BackColor = Color.White;

            // SPEC 범위 체크: 각 항목별 Min/Max 범위로 표시
            for (int idx = 1; idx <= 8; idx++)
            {
                Color bad = Color.LightPink;
                // VF
                MarkIfOutOfSpec(dgvCopy.Rows[idx - 1].Cells[1], _specVfMin, _specVfMax, bad);
                // Watt
                MarkIfOutOfSpec(dgvCopy.Rows[idx - 1].Cells[2], _specWMin, _specWMax, bad);
                // WD/WP
                MarkIfOutOfSpec(dgvCopy.Rows[idx - 1].Cells[3], _specWdwpMin, _specWdwpMax, bad);
                // TOV
                MarkIfOutOfSpec(dgvCopy.Rows[idx - 1].Cells[4], _specTovMin, _specTovMax, bad);
            }
        }

        private static void MarkIfOutOfSpec(DataGridViewCell cell, double min, double max, Color badColor)
        {
            double v = ParseCell(cell);
            if (double.IsNaN(v)) return;
            if (v < min || v > max) cell.Style.BackColor = badColor;
        }

        private static double ParseCell(DataGridViewCell cell)
        {
            var s = cell?.Value?.ToString();
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d;
            return double.NaN;
        }

        private static string Format(double? v)
        {
            if (!v.HasValue) return "";
            var d = v.Value;
            if (double.IsNaN(d) || double.IsInfinity(d)) return "";
            return d.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static double? TryGetMeasure(PKGTesterResult result, string[] keys)
        {
            try
            {
                if (result == null) return null;

                var prop = result.GetType().GetProperty("Items", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var dict = prop != null ? prop.GetValue(result, null) as System.Collections.Generic.IDictionary<string, TestItemResult> : null;
                if (dict != null && dict.Count > 0)
                {
                    foreach (var k in keys)
                    {
                        TestItemResult item;
                        if (dict.TryGetValue(k, out item) && item != null) return item.Value;
                        var kv = dict.FirstOrDefault(p => p.Key != null && p.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
                        if (kv.Value != null) return kv.Value.Value;
                    }
                }

                var field = result.GetType().GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dict2 = field != null ? field.GetValue(result) as System.Collections.Generic.IDictionary<string, TestItemResult> : null;
                if (dict2 != null && dict2.Count > 0)
                {
                    foreach (var k in keys)
                    {
                        TestItemResult item;
                        if (dict2.TryGetValue(k, out item) && item != null) return item.Value;
                        var kv = dict2.FirstOrDefault(p => p.Key != null && p.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
                        if (kv.Value != null) return kv.Value.Value;
                    }
                }

                foreach (var k in keys)
                {
                    var pi = result.GetType().GetProperty(k, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (pi != null)
                    {
                        var v = pi.GetValue(result, null);
                        if (v != null && double.TryParse(v.ToString(), out var d)) return d;
                    }
                }
            }
            catch { }
            return null;
        }

        // 측정 완료 이벤트 핸들러: 좌측 활성 블록 갱신 + AVG/OFFSET/COPY 재계산
        private void OnMeasurementCompleted(int socketZeroBased, PKGTesterResult result)
        {
            try
            {
                // UI 스레드 보장
                if (InvokeRequired)
                {
                    BeginInvoke(new Action<int, PKGTesterResult>(OnMeasurementCompleted), socketZeroBased, result);
                    return;
                }

                // 활성 블록 결정
                int cycleIndex = EstimateCurrentCycleIndex();
                var activeBlock = (_measureBlocks.Count > cycleIndex) ? _measureBlocks[cycleIndex] : _measureBlocks.LastOrDefault();
                if (activeBlock == null) return;

                int col = socketZeroBased + 2; // Index1..Index8 → 열 2..9
                if (col < 2 || col >= activeBlock.ColumnCount) return;

                // 측정값 파싱
                double? vf = TryGetMeasure(result, new[] { "VF3", "VF", "Vf", "ForwardVoltage" });
                double? watt = TryGetMeasure(result, new[] { "WATT", "Watt", "Power", "Pwr" });
                double? wdwp = TryGetMeasure(result, new[] { "WD", "WP", "WD/WP", "WD_WP", "WdWp", "WDWP" });
                double? tov = TryGetMeasure(result, new[] { "VF1", "VF5", "TOV", "OverVoltage", "TestOV" });

                // 좌측 그리드에 반영
                SetCell(activeBlock, 0, col, vf);
                SetCell(activeBlock, 1, col, watt);
                SetCell(activeBlock, 2, col, wdwp);
                SetCell(activeBlock, 3, col, tov);

                // 블록 내 누적 측정 수 증가(한 소켓 완주마다 1 증가)
                _measureCountInCurrentBlock = Math.Min(8, _measureCountInCurrentBlock + 1);
                // 8개 채우면 다음 블록으로 전환
                if (_measureCountInCurrentBlock >= 8)
                {
                    if (_currentBlockIndex < _measureBlocks.Count - 1)
                    {
                        _currentBlockIndex++;
                        _measureCountInCurrentBlock = 0;
                    }
                    else
                    {
                        // 마지막 블록에서는 계속 누적(원한다면 롤오버로 0으로 되돌릴 수 있음)
                        _measureCountInCurrentBlock = 8;
                    }
                }

                // 우측 테이블들 재계산
                RecomputeAvgFromLeftBlocks();
                RecomputeOffsetFromAvg();
                UpdateCopyAndSpecColors();
            }
            catch { /* UI 업데이트는 최대한 실패 무시 */ }
        }



        private void btnOffsetSave_Click(object sender, EventArgs e)
        {
            try
            {
                var ask = new MessageBoxYesNo();
                if (ask.ShowDialog("INFO.", $"Offset을 저장하시겠습니까?") != DialogResult.Yes)
                {
                    return;
                }

                var eq = Equipment.Instance;
                var recipe = eq?.EquipmentRecipe?.CurrentRecipe;
                if (recipe == null)
                {
                    MessageBox.Show("현재 레시피가 없습니다.");
                    return;
                }

                var filePath = recipe.TestConditionSetFile;
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    MessageBox.Show("TestConditionSet 파일 경로가 유효하지 않습니다.\n" + filePath);
                    return;
                }

                // 1) TestConditionSet 로드
                var set = new QMC.Common.PKGTester.TestConditionSet();
                if (set.LoadFromFile(filePath) != 0)
                {
                    MessageBox.Show("TestConditionSet 로드 실패:\n" + filePath);
                    return;
                }

                // 2) dgvOffset -> item 오프셋 반영
                //    행: 0 VF, 1 Watt, 2 WD/WP, 3 TOV
                ApplyOffsetRow(set, rowIndex: 0, itemAliases: new[] { "VF3", "VF" }, preferType: null);
                ApplyOffsetRow(set, rowIndex: 1, itemAliases: new[] { "WATT", "Watt", "Power", "Pwr" }, preferType: null);
                ApplyOffsetRow(set, rowIndex: 2, itemAliases: new[] { "WD", "WP", "WD/WP", "WD_WP", "WdWp", "WDWP" }, preferType: null);
                ApplyOffsetRow(set, rowIndex: 3, itemAliases: new[] { "VF1", "VF5", "TOV", "OverVoltage", "TestOV" }, preferType: null);

                // 3) 저장
                if (set.SaveToFile(filePath) != 0)
                {
                    MessageBox.Show("TestConditionSet 저장 실패:\n" + filePath);
                    return;
                }

                // 4) 런타임 즉시 반영(권장)
                try
                {
                    eq.Tester?.LoadTestConditionSet(filePath);
                }
                catch { /* 런타임 반영 실패는 저장 성공과 별개로 취급 */ }

                // 5) 레시피도 Save(경로 자체는 바뀌지 않지만, 기존 패턴에 맞춰 동기화)
                try
                {
                    recipe.Save();
                }
                catch { }

                MessageBox.Show("Offset이 레시피(TestConditionSet)에 적용되고 저장되었습니다.\n" + Path.GetFileName(filePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Offset 저장 실패: " + ex.Message);
            }
        }

        private void ApplyOffsetRow(QMC.Common.PKGTester.TestConditionSet set, int rowIndex, string[] itemAliases, QMC.Common.PKGTester.TestItemType? preferType)
        {
            if (set == null) return;
            if (rowIndex < 0 || rowIndex >= dgvOffset.Rows.Count) return;

            // 대상 아이템 찾기(이름 우선, 없으면 Type로 fallback 가능하도록 확장 여지)
            var item = FindConditionItem(set, itemAliases, preferType);
            if (item == null)
                return;

            // Index1..8 = columns 1..8 (0은 "Item")
            // 내부 배열은 0..7
            for (int idx = 1; idx <= 8; idx++)
            {
                double v = ParseCell(dgvOffset.Rows[rowIndex].Cells[idx]);
                if (double.IsNaN(v))
                    v = 0; // 비어있으면 0으로 처리(보수적으로)

                int arr = idx - 1;
                item.Offset[arr] = v;
                item.UseOffset[arr] = true;
            }
        }

        private static QMC.Common.PKGTester.TestConditionItem FindConditionItem(
            QMC.Common.PKGTester.TestConditionSet set,
            string[] aliases,
            QMC.Common.PKGTester.TestItemType? preferType)
        {
            if (set == null) return null;

            // 1) alias 이름 매칭(대소문자 무시)
            if (aliases != null)
            {
                foreach (var a in aliases)
                {
                    var found = set.Items.FirstOrDefault(x => x != null && x.Name != null && x.Name.Equals(a, StringComparison.OrdinalIgnoreCase));
                    if (found != null)
                        return found;
                }

                // 2) alias 포함(contains)도 허용
                foreach (var a in aliases)
                {
                    var found = set.Items.FirstOrDefault(x => x != null && x.Name != null && x.Name.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (found != null)
                        return found;
                }
            }

            // 3) Type 기반 fallback(가능할 때만)
            if (preferType.HasValue)
                return set.Items.FirstOrDefault(x => x != null && x.Type.Equals(preferType.Value));

            return null;
        }

    }
}