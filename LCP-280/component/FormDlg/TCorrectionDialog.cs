using QMC.Common;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.ThetaCorrection;
using QMC.Common.VisionPart;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace QMC.LCP_280.Process.Component
{
    public partial class TCorrectionDialog : Form
    {
        // 외부 의존성
        private readonly OutputStage _outputStage;
        private readonly PatternMatchingRunner _patternRunner;

        // 실행 제어
        private CancellationTokenSource _cts;
        private volatile bool _running;

        // 결과 모델
        private readonly List<ScanRecord> _records = new List<ScanRecord>();

        // 4점 마크 좌표(스테이지 기준)
        private readonly List<MarkPoint> _marks = new List<MarkPoint>(capacity: 4);
        LinkTypeXYTStageCorrection linkTypeXYTStageCorrection = new LinkTypeXYTStageCorrection();
        public TCorrectionDialog(OutputStage outputStage, PatternMatchingRunner patternRunner)
        {
            InitializeComponent();
            _outputStage = outputStage ?? throw new ArgumentNullException(nameof(outputStage));
            _patternRunner = patternRunner; // null 가능: 카메라 snapshot만 사용

            // 초기 마크 테이블 준비(4행)
            for (int i = 0; i < 4; i++)
            {
                dgvMarks.Rows.Add(i + 1, 0, 0);
            }

            // 초기 스캔 테이블 클리어
            dgvScan.Rows.Clear();
        }

        // Start: -4 ~ +5도, txtAngleStep 간격으로 스캔 + 마크 서치
        private async void OnStartClicked()
        {
            if (_running) return;

            if (!TryReadMarks(out var marks))
            {
                MessageBox.Show("마크 4점 좌표(PosX/PosY)를 모두 입력하세요.", "T-보정", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtAngleStep.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double step))
            {
                MessageBox.Show("angle 스텝 값을 확인하세요. 예: 0.1", "T-보정", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            step = Math.Max(0.01, Math.Min(5.0, step)); // 안전범위 제한

            // 범위는 UI에서 고정 값 사용(예:-4~+4) 또는 필요 시 입력 컨트롤 추가
            double rangeDeg = 4.0;
            // OutputStage에 직접 요청
            try
            {
                _running = true;
                _cts = new CancellationTokenSource();
                btnStart.Enabled = false;
                btnStop.Enabled = true;

                // marks를 (double X,double Y) 튜플 리스트로 변환
                var markTuples = marks
                    .OrderBy(m => m.Index)
                    .Select(m => (m.X, m.Y))
                    .ToList();

                // 장비 구동은 UI 블로킹 방지 위해 Task로 실행
                int rc = await Task.Run(() => _outputStage.StartTCorrection(markTuples, rangeDeg, step, fineSpeed: true, ct: _cts.Token));
                if (rc == 0)
                {
                    // 저장 경로 결정 (예: 작업 폴더 Logs/TCorrection_타임스탬프.csv)
                    var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                "Logs",
                                                $"TCorrection_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                    _outputStage.SaveTCorrectionCsv(savePath, markTuples, rangeDeg, step);
                }
                if (rc == -2)
                {
                    MessageBox.Show("보정 실행이 사용자의 정지 요청으로 중단되었습니다.", "T-보정", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("보정 실행 완료", "T-보정", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Write("TCorrection", $"Run failed: {ex.Message}");
                MessageBox.Show(ex.Message, "T-보정", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _running = false;
                _cts?.Dispose();
                _cts = null;

                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }

        private void OnStopClicked()
        {
            _cts?.Cancel();
        }

        // 보정 계산: 4점 마크로 T 보정(최소자승) 및 X/Y/T 오프셋 산출
        private void OnCalcClicked()
        {
            linkTypeXYTStageCorrection = new LinkTypeXYTStageCorrection();
            var v = _records.OrderBy(t => t.StageT).ThenBy(t=>t.MarkIndex);
            
            foreach (var rec in v)
            {
                var buffer = linkTypeXYTStageCorrection.CorrectionPoints.
                    Where(p => p.CommandTheta == rec.StageT);
                XyCoordinate xyCoordinate = new XyCoordinate() { X = rec.StageX + rec.ImageX, Y = rec.StageY + rec.ImageY };
                if (buffer.Count() == 0)
                {
                    var point = new List<XyCoordinate>();
                    point.Add(xyCoordinate);
                    for(int iter = 0; iter < 3; iter ++)
                    {
                        point.Add(new XyCoordinate() { X = 0, Y = 0 });
                    }
                    linkTypeXYTStageCorrection.AddCorrectionPoint(point, rec.StageT);
                }
                else
                {
                    var point = buffer.First().PointDs;
                    if(rec.MarkIndex<= point.Count)
                    {
                        point[rec.MarkIndex -1] = xyCoordinate;
                    }
                    else
                    {
                        point.Add(xyCoordinate);

                    }

                }
            }
            linkTypeXYTStageCorrection.SetZeroCommandTheta(0.3);
            this._outputStage.linkTypeXYTStageCorrection = linkTypeXYTStageCorrection;
        }

        // 현재 선택된 탭의 그리드를 반환 (Matrix: dgvScan, Mark1~4: dgvMark1..4)
        private DataGridView GetSelectedGrid()
        {
            if (tabMarks == null) return dgvScan;
            var tab = tabMarks.SelectedTab;
            if (tab == null) return dgvScan;

            // Matrix 탭은 Designer에서 dgvScan이 Dock=Fill로 배치
            if (tab == tabMatrix) return dgvScan;

            // Mark 탭들: 탭 내 DataGridView를 직접 찾음
            var grid = tab.Controls.OfType<DataGridView>().FirstOrDefault();
            return grid ?? dgvScan;
        }

        // 현재 그리드의 행을 ScanRecord 리스트로 변환
        private List<ScanRecord> ReadGridToRecords(DataGridView grid)
        {
            var list = new List<ScanRecord>();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow) continue;
                int idx;
                double angle, sx, sy;
                double? calX = null, calY = null, calT = null;

                int.TryParse(row.Cells["colIndex"]?.Value?.ToString(), out idx);
                double.TryParse(row.Cells["colAngle"]?.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out angle);
                double.TryParse(row.Cells["colStageX"]?.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out sx);
                double.TryParse(row.Cells["colStageY"]?.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out sy);

                double tmp;
                if (double.TryParse(row.Cells["colCalX"]?.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)) calX = tmp;
                if (double.TryParse(row.Cells["colCalY"]?.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)) calY = tmp;
                if (double.TryParse(row.Cells["colCalT"]?.Value?.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out tmp)) calT = tmp;

                list.Add(new ScanRecord
                {
                    Index = idx,
                    AngleDeg = angle,
                    StageX = sx,
                    StageY = sy,
                    ImageX = 0, // 그리드에 없으므로 기본값
                    ImageY = 0,
                    Score = 0,
                    CalX = calX,
                    CalY = calY,
                    CalT = calT
                });
            }
            return list;
        }

        // ScanRecord 리스트를 그리드에 채움 (현재 선택된 탭만 대상)
        private void PopulateGridFromRecords(DataGridView grid, IEnumerable<ScanRecord> list)
        {
            grid.Rows.Clear();
            foreach (var r in list)
            {
                grid.Rows.Add(
                    r.MarkIndex,
                    r.Index,
                    r.AngleDeg.ToString("F3", CultureInfo.InvariantCulture),
                    r.ImageX.ToString("F3", CultureInfo.InvariantCulture),
                    r.ImageY.ToString("F3", CultureInfo.InvariantCulture),
                    r.ImageT.ToString("F3", CultureInfo.InvariantCulture),
                    r.StageX.ToString("F3", CultureInfo.InvariantCulture),
                    r.StageY.ToString("F3", CultureInfo.InvariantCulture),
                    r.StageT.ToString("F3", CultureInfo.InvariantCulture),
                    ToCell(r.CalX),
                    ToCell(r.CalY),
                    ToCell(r.CalT));
            }
        }


        // 핵심 스캔 루프
        private void RunScan(List<MarkPoint> marks, double stepDeg, CancellationToken ct)
        {
            var cam = _outputStage.OutStageCamera;
            if (cam == null)
                throw new InvalidOperationException("OutStageCamera가 바인딩되지 않았습니다.");

            // 현재 스테이지 중심 Teaching 좌표
            var (centerX, centerY, centerT) = _outputStage.Config.GetPositionWithOffset(OutputStageConfig.TeachingPositionName.CenterPoint.ToString());

            // 각도 범위
            double minDeg = -4.0;
            double maxDeg = +5.0;

            int idx = 0;
            for (double d = maxDeg; d >= minDeg; d -= stepDeg) // 화면 예시처럼 4.0 -> -4.0 방향
            {
                ct.ThrowIfCancellationRequested();
                idx++;

                double targetT = centerT + d;
                // T축 이동
                int rcT = _outputStage.MoveAxisPositionOne(_outputStage.AxisT, targetT, isFine: false);
                if (rcT != 0) throw new InvalidOperationException($"T축 이동 실패 (deg={d:F3})");

                // 안정 대기
                Thread.Sleep(50);

                // 촬영
                cam.ExposeEnd(); // 제공 시그니처 기준 호출
                var image = cam.LatestImage; // VisionImage

                // 마크 서치 (패턴 러너가 있으면 사용, 없으면 간단 중심 근사)
                double imgX = 0, imgY = 0, score = 0;
                if (_patternRunner != null && image != null)
                {
                    //var sw = System.Diagnostics.Stopwatch.StartNew();
                    //var result = _patternRunner.All(image); // 시그니처에 맞춰 호출
                    //bool success = result != null && result.Count > 0;
                    //score = success ? result[0].Score : 0;
                    //if (success)
                    //{
                    //    imgX = result[0].Center.X;
                    //    imgY = result[0].Center.Y;
                    //}
                    //_patternRunner.Update(sw.Elapsed.TotalMilliseconds, success, success ? result.Count : 0, result);
                }
                else
                {
                    //// 대체: 이미지 중앙을 마크로 가정(임시)
                    //if (image != null)
                    //{
                    //    var center = new System.Drawing.Point(image.Width / 2, image.Height / 2);
                    //    imgX = center.X;
                    //    imgY = center.Y;
                    //    score = 0.0;
                    //}
                }

                // 이미지 좌표 -> 스테이지 좌표 근사 (픽셀->mm 변환이 있으면 적용)
                // 여기서는 현재 스테이지 중심 좌표를 그대로 사용(데모)
                double stageX = centerX;
                double stageY = centerY;

                var rec = new ScanRecord
                {
                    Index = idx,
                    AngleDeg = d,
                    ImageX = imgX,
                    ImageY = imgY,
                    StageX = stageX,
                    StageY = stageY,
                    Score = score
                };

                _records.Add(rec);
                AddRow(rec);
            }
        }

        private void AddRow(ScanRecord r)
        {
            if (dgvScan.InvokeRequired)
            {
                dgvScan.Invoke(new Action<ScanRecord>(AddRow), r);
                return;
            }
            dgvScan.Rows.Add(
                r.MarkIndex,
                r.Index,
                r.AngleDeg.ToString("F3", CultureInfo.InvariantCulture),
                r.ImageX.ToString("F3", CultureInfo.InvariantCulture),
                r.ImageY.ToString("F3", CultureInfo.InvariantCulture),
                r.ImageT.ToString("F3", CultureInfo.InvariantCulture),
                r.StageX.ToString("F3", CultureInfo.InvariantCulture),
                r.StageY.ToString("F3", CultureInfo.InvariantCulture),
                r.StageT.ToString("F3", CultureInfo.InvariantCulture),
                ToCell(r.CalX),
                ToCell(r.CalY),
                ToCell(r.CalT));
        }

        private void UpdateRow(ScanRecord r)
        {
            if (dgvScan.InvokeRequired)
            {
                dgvScan.Invoke(new Action<ScanRecord>(UpdateRow), r);
                return;
            }
            foreach (DataGridViewRow row in dgvScan.Rows)
            {
                if (row.Cells["colIndex"].Value is object v &&
                    int.TryParse(v.ToString(), out int idx) &&
                    idx == r.Index)
                {
                    row.Cells["colCalX"].Value = ToCell(r.CalX);
                    row.Cells["colCalY"].Value = ToCell(r.CalY);
                    row.Cells["colCalT"].Value = ToCell(r.CalT);
                    break;
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e) => OnStartClicked();
        private void btnStop_Click(object sender, EventArgs e) => OnStopClicked();
        private void btnSave_Click(object sender, EventArgs e) => OnSaveClicked();
        private void btnLoad_Click(object sender, EventArgs e) => OnLoadClicked();
        private void btnCalc_Click(object sender, EventArgs e) => OnCalcClicked();
        private void btnClose_Click(object sender, EventArgs e) => this.Close();


        private static string ToCell(double? v) => v.HasValue ? v.Value.ToString("F3") : "";

        private bool TryReadMarks(out List<MarkPoint> points)
        {
            points = new List<MarkPoint>();
            if (dgvMarks.Rows.Count < 4) return false;

            for (int i = 0; i < 4; i++)
            {
                var row = dgvMarks.Rows[i];
                if (!TryReadCell(row.Cells["colMX"].Value, out double x)) return false;
                if (!TryReadCell(row.Cells["colMY"].Value, out double y)) return false;
                points.Add(new MarkPoint { Index = i + 1, X = x, Y = y });
            }
            return true;
        }

        private static bool TryReadCell(object cellVal, out double d)
        {
            if (cellVal == null) { d = 0; return false; }
            return double.TryParse(cellVal.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out d);
        }

        private static void SaveCsv(string path, IEnumerable<ScanRecord> list)
        {
            using (var w = new StreamWriter(path))
            {
                w.WriteLine("Index,Angle,ImageX,ImageY,StageX,StageY,Score,CalX,CalY,CalT");
                foreach (var r in list)
                {
                    w.WriteLine($"{r.Index},{r.AngleDeg:F3},{r.ImageX:F3},{r.ImageY:F3},{r.StageX:F3},{r.StageY:F3},{r.Score:F3},{(r.CalX?.ToString("F3") ?? "")},{(r.CalY?.ToString("F3") ?? "")},{(r.CalT?.ToString("F3") ?? "")}");
                }
            }
        }

        private static List<ScanRecord> LoadCsv(string path)
        {
            var list = new List<ScanRecord>();
            foreach (var line in File.ReadAllLines(path).Skip(1))
            {
                var sp = line.Split(',');
                if (sp.Length < 6) continue;
                var r = new ScanRecord
                {
                    Index = int.Parse(sp[0]),
                    AngleDeg = double.Parse(sp[1], CultureInfo.InvariantCulture),
                    ImageX = double.Parse(sp[2], CultureInfo.InvariantCulture),
                    ImageY = double.Parse(sp[3], CultureInfo.InvariantCulture),
                    StageX = double.Parse(sp[4], CultureInfo.InvariantCulture),
                    StageY = double.Parse(sp[5], CultureInfo.InvariantCulture),
                    Score = sp.Length > 6 ? double.Parse(sp[6], CultureInfo.InvariantCulture) : 0,
                    CalX = ParseNullable(sp, 7),
                    CalY = ParseNullable(sp, 8),
                    CalT = ParseNullable(sp, 9)
                };
                list.Add(r);
            }
            return list;

            double? ParseNullable(string[] arr, int idx)
            {
                if (idx >= arr.Length) return null;
                var s = arr[idx];
                if (string.IsNullOrWhiteSpace(s)) return null;
                if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) return d;
                return null;
            }
        }

        // 2D Rigid (회전+이동) 추정
        private static bool TrySolveRigid2D(double[][] S, double[][] D, out double rotDeg, out double tx, out double ty)
        {
            rotDeg = 0; tx = 0; ty = 0;
            if (S == null || D == null || S.Length != D.Length || S.Length < 2) return false;

            // 중심 정렬
            double cxS = S.Average(p => p[0]);
            double cyS = S.Average(p => p[1]);
            double cxD = D.Average(p => p[0]);
            double cyD = D.Average(p => p[1]);

            var Sc = S.Select(p => new[] { p[0] - cxS, p[1] - cyS }).ToArray();
            var Dc = D.Select(p => new[] { p[0] - cxD, p[1] - cyD }).ToArray();

            // 2x2 공분산
            double sxx = 0, sxy = 0, syx = 0, syy = 0;
            for (int i = 0; i < Sc.Length; i++)
            {
                sxx += Sc[i][0] * Dc[i][0];
                sxy += Sc[i][0] * Dc[i][1];
                syx += Sc[i][1] * Dc[i][0];
                syy += Sc[i][1] * Dc[i][1];
            }

            // 회전 추정 (atan2)
            double theta = Math.Atan2(sxy - syx, sxx + syy);
            rotDeg = theta * 180.0 / Math.PI;

            // 이동 추정
            tx = cxD - (cxS * Math.Cos(theta) - cyS * Math.Sin(theta));
            ty = cyD - (cxS * Math.Sin(theta) + cyS * Math.Cos(theta));

            return true;
        }


        // 최근 저장/로드한 T-보정 메타 파일 경로
        private string _tCorrectionMetaFilePath;

        // 메타 구조체
        private sealed class TCorrectionMeta
        {
            public double RangeDeg;
            public double StepDeg;
            public readonly Dictionary<int, (double X, double Y)> Marks = new Dictionary<int, (double X, double Y)>(4);
        }

        // ===== CSV + 메타 저장 =====
        // 포맷:
        // #TCorrectionMeta
        // RangeDeg=4.000
        // StepDeg=0.100
        // Mark1=123.456,234.567
        // Mark2=...
        // Mark3=...
        // Mark4=...
        // ---DATA---
        // Index,Angle,ImageX,ImageY,StageX,StageY,Score,CalX,CalY,CalT
        // ...
        // 메타+데이터 저장 (정상 구문)
        private static void SaveTCorrectionFile(string path,
                                                IEnumerable<ScanRecord> scanRecords,
                                                IList<MarkPoint> markPoints,
                                                double rangeDeg,
                                                double stepDeg)
        {
            using (var w = new StreamWriter(path, false))
            {
                w.WriteLine("#TCorrectionMeta");
                w.WriteLine(string.Format(CultureInfo.InvariantCulture, "RangeDeg={0:F3}", rangeDeg));
                w.WriteLine(string.Format(CultureInfo.InvariantCulture, "StepDeg={0:F3}", stepDeg));
                for (int i = 0; i < markPoints.Count; i++)
                {
                    var mp = markPoints[i];
                    w.WriteLine(string.Format(CultureInfo.InvariantCulture, "Mark{0}={1:F6},{2:F6}", mp.Index, mp.X, mp.Y));
                }

                w.WriteLine("---DATA---");
                w.WriteLine("Index,Angle,ImageX,ImageY,StageX,StageY,Score,CalX,CalY,CalT");
                foreach (var r in scanRecords)
                {
                    string calX = r.CalX.HasValue ? r.CalX.Value.ToString("F3", CultureInfo.InvariantCulture) : "";
                    string calY = r.CalY.HasValue ? r.CalY.Value.ToString("F3", CultureInfo.InvariantCulture) : "";
                    string calT = r.CalT.HasValue ? r.CalT.Value.ToString("F3", CultureInfo.InvariantCulture) : "";

                    w.WriteLine(string.Format(CultureInfo.InvariantCulture,
                        "{0},{1:F3},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7},{8},{9}",
                        r.Index, r.AngleDeg, r.ImageX, r.ImageY, r.StageX, r.StageY, r.Score, calX, calY, calT));
                }
            }
        }

        // 메타+데이터 로드 (정상 구문)
        private static bool LoadTCorrectionFile(string path,
                                                out List<ScanRecord> records,
                                                out TCorrectionMeta meta,
                                                out string error)
        {
            records = new List<ScanRecord>();
            meta = null;
            error = null;

            try
            {
                var lines = File.ReadAllLines(path);
                int dataStart = -1;
                var tmpMeta = new TCorrectionMeta();

                // 메타 파싱
                for (int i = 0; i < lines.Length; i++)
                {
                    var ln = lines[i].Trim();
                    if (ln == "---DATA---")
                    {
                        dataStart = i;
                        break;
                    }
                    if (ln.Length == 0 || ln.StartsWith("#")) continue;

                    if (ln.StartsWith("RangeDeg=", StringComparison.OrdinalIgnoreCase))
                    {
                        double v;
                        if (double.TryParse(ln.Substring("RangeDeg=".Length), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            tmpMeta.RangeDeg = v;
                    }
                    else if (ln.StartsWith("StepDeg=", StringComparison.OrdinalIgnoreCase))
                    {
                        double v;
                        if (double.TryParse(ln.Substring("StepDeg=".Length), NumberStyles.Float, CultureInfo.InvariantCulture, out v))
                            tmpMeta.StepDeg = v;
                    }
                    else if (ln.StartsWith("Mark", StringComparison.OrdinalIgnoreCase))
                    {
                        int eqIdx = ln.IndexOf('=');
                        if (eqIdx > 4)
                        {
                            string name = ln.Substring(0, eqIdx);    // e.g. "Mark1"
                            string val = ln.Substring(eqIdx + 1);    // "X,Y"
                            var parts = val.Split(',');
                            int markIdx;
                            double mx, my;
                            if (parts.Length == 2 &&
                                int.TryParse(name.Substring(4), out markIdx) &&
                                double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out mx) &&
                                double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out my))
                            {
                                tmpMeta.Marks[markIdx] = (mx, my);
                            }
                        }
                    }
                }

                if (dataStart < 0)
                {
                    error = "메타 구분자(---DATA---)를 찾을 수 없습니다.";
                    return false;
                }

                // 데이터 헤더 다음 줄부터 파싱
                for (int i = dataStart + 1; i < lines.Length; i++)
                {
                    var ln = lines[i].Trim();
                    if (ln.Length == 0) continue;
                    if (ln.StartsWith("#")) continue;

                    var sp = ln.Split(',');
                    if (sp.Length < 6) continue;

                    int idx = ParseInt(sp, 0);
                    double angle = ParseDouble(sp, 1);
                    double imgX = ParseDouble(sp, 2);
                    double imgY = ParseDouble(sp, 3);
                    double sx = ParseDouble(sp, 4);
                    double sy = ParseDouble(sp, 5);
                    double score = sp.Length > 6 ? ParseDouble(sp, 6) : 0;
                    double? calX = ParseNullable(sp, 7);
                    double? calY = ParseNullable(sp, 8);
                    double? calT = ParseNullable(sp, 9);

                    records.Add(new ScanRecord
                    {
                        Index = idx,
                        AngleDeg = angle,
                        ImageX = imgX,
                        ImageY = imgY,
                        StageX = sx,
                        StageY = sy,
                        Score = score,
                        CalX = calX,
                        CalY = calY,
                        CalT = calT
                    });
                }

                meta = tmpMeta;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            // 로컬 파서들
            int ParseInt(string[] arr, int idx)
            {
                if (idx >= arr.Length) return 0;
                int v;
                return int.TryParse(arr[idx], NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : 0;
            }
            double ParseDouble(string[] arr, int idx)
            {
                if (idx >= arr.Length) return 0;
                double v;
                return double.TryParse(arr[idx], NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : 0;
            }
            double? ParseNullable(string[] arr, int idx)
            {
                if (idx >= arr.Length) return null;
                var s = arr[idx];
                if (string.IsNullOrWhiteSpace(s)) return null;
                double v;
                return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? (double?)v : null;
            }
        }

        // 저장 클릭: 현재 선택된 탭 데이터 + 마크 1~4 위치 + 스캔 각도 범위/스텝 메타 포함 저장
        private void OnSaveClicked()
        {
            using (var dlg = new SaveFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Save T-보정 결과",
                FileName = $"TCorrection_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var grid = GetSelectedGrid();
                    var list = ReadGridToRecords(grid);

                    // 마크 좌표 읽기
                    if (!TryReadMarks(out var markPoints))
                    {
                        MessageBox.Show("마크 좌표가 올바르지 않습니다.", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 각도 범위/스텝: 현재 그리드에 있는 AngleDeg 최소/최대 → rangeDeg, txtAngleStep
                    double stepDeg;
                    if (!double.TryParse(txtAngleStep.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out stepDeg))
                        stepDeg = 0.1;

                    double minA = list.Count > 0 ? list.Min(r => r.AngleDeg) : -4.0;
                    double maxA = list.Count > 0 ? list.Max(r => r.AngleDeg) : +4.0;
                    // rangeDeg: 절대값 최대 (대칭 가정)
                    double rangeDeg = Math.Max(Math.Abs(minA), Math.Abs(maxA));

                    SaveTCorrectionFile(dlg.FileName, list, markPoints, rangeDeg, stepDeg);
                    _tCorrectionMetaFilePath = dlg.FileName;
                    MessageBox.Show("저장 완료 (메타+데이터)", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Save 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 로드 클릭: 파일에서 메타 + 데이터 읽어 현재 탭에 반영
        private void OnLoadClicked() 
        {
            using (var dlg = new OpenFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Load T-보정 결과 (OutStage)"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                LoadFromOutStageCsv(dlg.FileName);
            }

            //using (var dlg = new OpenFileDialog()
            //{
            //    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            //    Title = "Load T-보정 결과"
            //})
            //{
            //    if (dlg.ShowDialog(this) != DialogResult.OK) return;
            //    try
            //    {
            //        if (!LoadTCorrectionFile(dlg.FileName, out var list, out var meta, out var err))
            //        {
            //            if (!string.IsNullOrEmpty(err))
            //                MessageBox.Show(err, "Load 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            return;
            //        }

            //        var grid = GetSelectedGrid();
            //        PopulateGridFromRecords(grid, list);
            //        _records.Clear();
            //        _records.AddRange(list);
            //        _loadedMeta = meta;
            //        _tCorrectionMetaFilePath = dlg.FileName;

            //        // 마크 테이블에 메타 반영
            //        if (meta != null && meta.Marks.Count == 4 && dgvMarks.Rows.Count >= 4)
            //        {
            //            for (int i = 0; i < 4; i++)
            //            {
            //                int markIdx = i + 1;
            //                if (meta.Marks.TryGetValue(markIdx, out var pos))
            //                {
            //                    dgvMarks.Rows[i].Cells["colMX"].Value = pos.X.ToString("F6", CultureInfo.InvariantCulture);
            //                    dgvMarks.Rows[i].Cells["colMY"].Value = pos.Y.ToString("F6", CultureInfo.InvariantCulture);
            //                }
            //            }
            //            txtAngleStep.Text = meta.StepDeg.ToString("F3", CultureInfo.InvariantCulture);
            //        }

            //        MessageBox.Show("로드 완료 (메타+데이터)", "Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message, "Load 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}
        }

        // OutStage CSV 로드 → 그리드 채우기
        private void LoadFromOutStageCsv(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("파일 경로가 비어 있습니다.", "T-보정 Load", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var recs = _outputStage.LoadTCorrectionCsv(path,
                                                       out var marks,
                                                       out var rangeDeg,
                                                       out var stepDeg);
            if (recs == null)
            {
                MessageBox.Show("로드 실패 또는 포맷 오류", "T-보정 Load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _records.Clear();
            dgvScan.Rows.Clear();

            int runningIndex = 0;
            foreach (var r in recs)
            {
                runningIndex++;
                var scan = new ScanRecord
                {
                    MarkIndex = r.MarkIndex,
                    Index = runningIndex,
                    AngleDeg = r.AngleDeg,
                    ImageX = r.FoundOffsetX,
                    ImageY = r.FoundOffsetY,
                    ImageT = r.FoundAngle,
                    StageX = r.StageX,
                    StageY = r.StageY,
                    StageT = r.StageT,
                    Score = r.AlignSuccess ? 1.0 : 0.0
                };
                _records.Add(scan);
                AddRow(scan);
            }

            // 마크 좌표 반영
            if (marks != null && marks.Count >= 4 && dgvMarks.Rows.Count >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    dgvMarks.Rows[i].Cells["colMX"].Value = marks[i].X.ToString("F6", CultureInfo.InvariantCulture);
                    dgvMarks.Rows[i].Cells["colMY"].Value = marks[i].Y.ToString("F6", CultureInfo.InvariantCulture);
                }
            }
            if (stepDeg > 0)
                txtAngleStep.Text = stepDeg.ToString("F3", CultureInfo.InvariantCulture);

            _tCorrectionMetaFilePath = path;
            MessageBox.Show("OutStage CSV 로드 완료", "T-보정 Load", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // 버튼 이벤트 예: 기존 OnLoadClicked 대신 OutStage 포맷을 불러오고 싶을 때 사용
        private void btnLoadFromOutStage_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Title = "Load OutStage T-보정 CSV"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                LoadFromOutStageCsv(dlg.FileName);
            }
        }

        private sealed class ScanRecord
        {
            public int MarkIndex { get; set; }
            public int Index { get; set; }
            public double AngleDeg { get; set; }
            public double ImageX { get; set; }
            public double ImageY { get; set; }
            public double ImageT { get; set; }
            public double StageX { get; set; }
            public double StageY { get; set; }
            public double StageT { get; set; }
            public double Score { get; set; }
            public double? CalX { get; set; }
            public double? CalY { get; set; }
            public double? CalT { get; set; }
        }

        private sealed class MarkPoint
        {
            public int Index { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public bool IsValid => !(double.IsNaN(X) || double.IsNaN(Y));
        }
    }
}