using QMC.Common;
using QMC.Common.Unit;
using QMC.LCP_280.Process; // Equipment Namespace
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace QMC.LCP_280.Process.Component.FormDlg
{
    public partial class TaktMonitorAllDialog : Form
    {
        // 보여줄 시간 범위 (최근 N초)
        private const double VIEW_WINDOW_SECONDS = 10.0;
        private string _selectedUnitName = "All";

        // ==========================================
        // [로깅을 위한 백그라운드 처리 변수 추가]
        // ==========================================
        private BlockingCollection<string> _logQueue = new BlockingCollection<string>();
        private CancellationTokenSource _logCts = new CancellationTokenSource();
        private Dictionary<string, DateTime> _lastLoggedTimeMap = new Dictionary<string, DateTime>();
        private string _logDirectory;

        // 1. 순서 및 표시할 Task 정의 (실제 Unit 이름 -> Task 이름 리스트)
        // 여기에 정의된 Unit과 Task만 화면에 표시됩니다.
        // 리스트의 순서대로 차트 위에서부터 그려집니다.
        private readonly Dictionary<string, List<string>> _predefinedOrder = new Dictionary<string, List<string>>()
        {
            { "InputDieTransfer",
                new List<string>
                {
                    "One Cycle",
                    "PickUp_Wait",
                    "SetVacuum_On",
                    "PositionToolTPickup",
                    "PrepareNextDie",
                    "Wafer Pick Die",
                    "RaiseEjectorForPick",
                    "PickDownDie",
                    "SyncPickUpDie",
                    "SyncPickDieRetreat",
                    "AfterPick_EjectorVacuumOff",
                    "AfterPick_VacuumCheck_Commit",
                    "AfterPick_VacuumFail_Recover",
                    "PlaceDie_ToolT",
                    "DiePlace_Wait",
                    "DiePlace_NextDie_Req",
                    "StartPrepareNextDieTask",
                    "DiePlace_Vacuum_Check",
                    "WaitPrepareNextDieTask_Miss",
                    "PlaceDownDie",
                    "PlaceUp",
                    "PlaceUp_AfterProcess"
                }
            },

            { "Rotary",
                new List<string>
                {
                    "Rotate",
                    "MoveRotate",
                    "WaitDoneRotate",
                    "Place Die",
                    "M-Align",
                    "Plobe Inspection",
                    "UnloadAlign",
                    "TrashCan",
                    "Pick Die",
                }
            },
            { "OutputDieTransfer",
                new List<string>
                {
                    "One Cycle",
                    "PickDie_ToolT",
                    "PickDownDie",
                    "PickUpDie",
                    "PlaceDie_ToolT",
                    "PlaceUp",
                    "Bin Place Die",
                }
            },
            {"IndexChipProbeController",
                new List<string>
                {
                    "One Cycle",
                    "SyncProbeZUpAndBottomProbeZReady",
                    "GripperXClamp",
                    "BottomProbeZUp",
                    "UpperWaitTime",
                    "Measure",
                    "ProbeCardZSafety",
                    "GripperXReady",
                    "ProbeZSafety",
                }
            },
            {"IndexLoadAligner",
                new List<string>
                {
                    "One Cycle",
                    "AlignTReady",
                    "AlignZUp",
                    "AlignTForward",
                    "WaitTime1Step",
                    "AlignTBackward",
                    "WaitTime2Step",
                    "AlignTReady2",
                    "AlignXY_Vision",
                    "SafetyZ",
                }
            },
        };

        // 2. Unit 이름 변경 (실제 이름 -> 표시할 이름)
        private readonly Dictionary<string, string> _unitAliases = new Dictionary<string, string>()
        {
            { "InputDieTransfer", "Input Arm" },   // 예: InputDieTransfer -> Input Arm으로 표시
            { "Rotary", "Index Table" },           // 예: Rotary -> Index Table로 표시
            { "OutputDieTransfer", "Output Arm" }  // 예: OutputDieTransfer -> Output Arm으로 표시
        };
        // 1-1. "All" 콤보박스 선택 시 표시할 특정 Unit과 Task 정의 (직접 원하는 것만 세팅)
        private readonly Dictionary<string, List<string>> _allViewOrder = new Dictionary<string, List<string>>()
        {
            { "InputDieTransfer",
                new List<string>
                {
                    "Wafer Pick Die",
                }
            },
            { "Rotary",
                new List<string>
                {
                    "Rotate",
                    "Place Die",
                    "M-Align",
                    "Plobe Inspection",
                    "UnloadAlign",
                    "TrashCan",
                    "Pick Die",
                }
            },
            { "OutputDieTransfer",
                new List<string>
                {
                    "Bin Place Die",
                }
            }
        };

        // 콤보박스 아이템용 클래스
        private class UnitComboItem
        {
            public string RealName { get; set; }
            public string DisplayName { get; set; }
            public override string ToString() => DisplayName;
        }

        public TaktMonitorAllDialog()
        {
            InitializeComponent();

            // 로그 폴더 준비
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log", "GraphTaktLogs");
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // [추가] 백그라운드 로깅 Task 시작
            Task.Run(() => BackgroundLogWriter(_logCts.Token));

            // 타이머 설정
            if (timerRefresh == null)
            {
                timerRefresh = new System.Windows.Forms.Timer();
                timerRefresh.Tick += timerRefresh_Tick;
            }
            timerRefresh.Interval = 100;
            timerRefresh.Start();

            InitializeChartStyle();
            LoadUnitList();
            RefreshChartData();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (timerRefresh != null) timerRefresh.Stop();

            // [추가] 백그라운드 로깅 큐 종료 및 대기
            _logQueue.CompleteAdding();
            _logCts.Cancel();

            base.OnFormClosed(e);
        }

        private void InitializeChartStyle()
        {
            chartMain.Series.Clear();
            chartMain.Legends.Clear();

            var area = chartMain.ChartAreas.Count > 0 ? chartMain.ChartAreas[0] : chartMain.ChartAreas.Add("MainArea");

            // [추가] 차트 안쪽 그림 영역(InnerPlotPosition)과 바깥 여백 조정
            // Label이 모두 보일 수 있도록 차트 왼쪽 여백 늘리기
            // Position의 Width, Height, X, Y를 자동에서 수동으로 조절
            area.Position.Auto = true; // 전체 뼈대는 Auto
            area.InnerPlotPosition.Auto = false; // 차트가 그려지는 안쪽 영역은 수동
            area.InnerPlotPosition.X = 18;       // 좌측 여백(라벨 공간)을 18% 정도로 크게 확보
            area.InnerPlotPosition.Y = 10;
            area.InnerPlotPosition.Width = 80;
            area.InnerPlotPosition.Height = 85;

            // === [Y축 (가로)] : 시간 ===
            area.AxisY.LabelStyle.Format = "ss.fff";
            area.AxisY.IntervalType = DateTimeIntervalType.Milliseconds;
            area.AxisY.Title = "Time Timeline (sec.ms)";

            // 1ms 단위 격자
            area.AxisY.MajorGrid.Enabled = true;
            area.AxisY.MajorGrid.Interval = 100; // 0.1초 굵은선
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(80, Color.Gray);
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            area.AxisY.MinorGrid.Enabled = true;
            area.AxisY.MinorGrid.Interval = 1;   // 1ms 얇은선
            area.AxisY.MinorGrid.LineColor = Color.FromArgb(40, Color.LightGray);
            area.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dot;

            area.AxisY.ScrollBar.Enabled = true;
            area.AxisY.ScrollBar.IsPositionedInside = true;
            area.AxisY.ScaleView.Zoomable = true;
            area.AxisY.ScaleView.SmallScrollMinSize = 0.001;

            area.CursorY.IsUserEnabled = true;
            area.CursorY.IsUserSelectionEnabled = true;

            // === [X축 (세로)] : 항목 ===
            area.AxisX.Title = "";
            area.AxisX.LabelStyle.Font = new Font("Malgun Gothic", 9, FontStyle.Bold);

            // [수정] 라벨 글자가 축소되거나 "..."으로 잘리는 것을 방지
            area.AxisX.LabelAutoFitStyle = LabelAutoFitStyles.None;
            area.AxisX.LabelStyle.TruncatedLabels = false; // "..."으로 그리기 방지

            // 위에서부터 0번 인덱스가 그려지도록 반전
            area.AxisX.IsReversed = true;

            area.AxisX.MajorGrid.Enabled = true;
            area.AxisX.MajorGrid.Interval = 1;
            area.AxisX.MajorGrid.LineColor = Color.Black;
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Solid;

            area.AxisX.IsInterlaced = true;
            area.AxisX.InterlacedColor = Color.FromArgb(15, Color.SteelBlue);

            area.AxisX.ScrollBar.Enabled = true;
            area.AxisX.ScaleView.Zoomable = true;

        }

        private void LoadUnitList()
        {
            cmbUnitSelector.Items.Clear();

            // "All" 추가
            cmbUnitSelector.Items.Add(new UnitComboItem { RealName = "All", DisplayName = "All Units" });

            var eq = Equipment.Instance;
            if (eq != null && eq.Units != null)
            {
                // _predefinedOrder에 정의된 유닛만 콤보박스에 추가
                foreach (var realName in _predefinedOrder.Keys)
                {
                    if (eq.Units.ContainsKey(realName))
                    {
                        string display = _unitAliases.ContainsKey(realName) ? _unitAliases[realName] : realName;
                        cmbUnitSelector.Items.Add(new UnitComboItem { RealName = realName, DisplayName = display });
                    }
                }
            }

            if (cmbUnitSelector.Items.Count > 0)
                cmbUnitSelector.SelectedIndex = 0;
        }

        #region Event Handlers

        private void cmbUnitSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbUnitSelector.SelectedItem is UnitComboItem item)
            {
                _selectedUnitName = item.RealName;
            }
            RefreshChartData();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshChartData();
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (chkAutoRefresh.Checked)
            {
                RefreshChartData();
            }
        }

        #endregion

        private void RefreshChartData()
        {
            if (chartMain == null || chartMain.IsDisposed) return;
            var eq = Equipment.Instance;
            if (eq == null) return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(RefreshChartData));
                return;
            }

            // 1. 차트 시리즈 준비
            string seriesName = "GanttSeries";
            Series series = chartMain.Series.FindByName(seriesName);
            if (series == null)
            {
                series = new Series(seriesName);
                series.ChartType = SeriesChartType.RangeBar;
                series.YValueType = ChartValueType.DateTime;
                series["DrawSideBySide"] = "false";
                series["PointWidth"] = "0.6";
                series["BarLabelStyle"] = "Center";
                series.SmartLabelStyle.Enabled = true;
                series.SmartLabelStyle.AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No;
                chartMain.Series.Add(series);
            }

            // 2. 표시할 Unit 목록 선정
            List<string> targetUnitNames = new List<string>();

            var currentOrderDict = (_selectedUnitName == "All") ? _allViewOrder : _predefinedOrder;

            if (_selectedUnitName == "All")
            {
                targetUnitNames.AddRange(_allViewOrder.Keys);
            }
            else
            {
                if (_predefinedOrder.ContainsKey(_selectedUnitName)) targetUnitNames.Add(_selectedUnitName);
            }

            // 3. Y축 라벨 순서 미리 생성
            List<string> orderedAxisKeys = new List<string>();
            Dictionary<string, int> axisIndexMap = new Dictionary<string, int>();
            int rowIndex = 1;
            foreach (var uName in targetUnitNames)
            {
                if (!currentOrderDict.ContainsKey(uName)) continue;
                var tasks = currentOrderDict[uName];
                foreach (var tName in tasks)
                {
                    string key = $"{uName}::{tName}";
                    if (!axisIndexMap.ContainsKey(key)) // 중복 키 방지
                    {
                        orderedAxisKeys.Add(key);
                        axisIndexMap[key] = rowIndex++;
                    }
                }
            }

            // --- 4. 데이터 수집 및 기준 시간 계산 (스레드 충돌 방지 적용) ---
            DateTime maxEndTime = DateTime.MinValue;
            foreach (var uName in targetUnitNames)
            {
                if (!(eq.GetUnit(uName) is BaseUnit unit) || unit.TaktTimers == null) continue;

                foreach (var taskName in currentOrderDict[uName])
                {
                    if (unit.TaktTimers.TryGetValue(taskName, out CycleTimer timer) && timer.CycleTimes != null)
                    {
                        try
                        {
                            // 동기화 문제(Collection was modified) 방지를 위해 ToArray() 등으로 복사본 사용
                            var cyclesSnapshot = timer.CycleTimes.ToArray();
                            if (cyclesSnapshot.Length > 0)
                            {
                                var lastCycle = cyclesSnapshot[cyclesSnapshot.Length - 1];
                                if (lastCycle.End > maxEndTime)
                                    maxEndTime = lastCycle.End;
                            }
                        }
                        catch (Exception) { /* 읽기 도중 발생한 충돌 무시 (다음 틱에 갱신) */ }
                    }
                }
            }

            DateTime now = DateTime.Now;
            if (maxEndTime != DateTime.MinValue && (now - maxEndTime).TotalSeconds > 2.0)
            {
                now = maxEndTime.AddSeconds(1.0);
            }

            DateTime minTime = now.AddSeconds(-VIEW_WINDOW_SECONDS);
            series.Points.Clear();

            var area = chartMain.ChartAreas[0];
            area.AxisX.CustomLabels.Clear();

            List<TaskPoint> pointsToAdd = new List<TaskPoint>();

            foreach (var uName in targetUnitNames)
            {
                if (!(eq.GetUnit(uName) is BaseUnit unit) || unit.TaktTimers == null)
                    continue;

                if (!_predefinedOrder.ContainsKey(uName))
                    continue;

                var definedTasks = _predefinedOrder[uName];
                foreach (var taskName in definedTasks)
                {
                    if (!unit.TaktTimers.TryGetValue(taskName, out CycleTimer timer) || timer.CycleTimes == null)
                        continue;

                    string key = $"{uName}::{taskName}";
                    string displayUnit = _unitAliases.ContainsKey(uName) ? _unitAliases[uName] : uName;

                    try
                    {
                        var cycles = timer.CycleTimes.ToArray().Where(c => c.End > minTime).ToList();

                        foreach (var c in cycles)
                        {
                            if (c.Start == DateTime.MinValue || c.End == DateTime.MinValue) continue;
                            if (c.End < c.Start) continue;

                            pointsToAdd.Add(new TaskPoint
                            {
                                RealUnitName = uName,
                                TaskName = taskName,
                                Key = key,
                                StartTime = c.Start,
                                EndTime = c.End,
                                DurationMs = c.Interval.TotalMilliseconds
                            });

                            // ==========================================
                            // [추가] 새로운 사이클인 경우에만 큐에 로깅 데이터 삽입
                            // ==========================================
                            if (!_lastLoggedTimeMap.ContainsKey(key) || c.End > _lastLoggedTimeMap[key])
                            {
                                _lastLoggedTimeMap[key] = c.End;

                                string logLine = string.Format("{0},{1},{2},{3:0},{4},{5}",
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    displayUnit,
                                    taskName,
                                    c.Interval.TotalMilliseconds,
                                    c.Start.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                    c.End.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                //string logLine = string.Format("{0},{1},{2},{3:0.##},{4},{5}",
                                //    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                //    displayUnit,
                                //    taskName,
                                //    c.Interval.TotalMilliseconds,
                                //    c.Start.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                                //    c.End.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                // 파일 IO 작업 없이 큐에만 빠르게 밀어넣고 빠져나옴 (장비 부하 없음)
                                _logQueue.Add(logLine);
                            }
                        }
                    }
                    catch (Exception) { /* 충돌 보호 */ }
                }
            }

            // 5. 차트에 그리기
            foreach (var key in orderedAxisKeys)
            {
                string[] parts = key.Split(new string[] { "::" }, StringSplitOptions.None);
                if (parts.Length < 2) continue;

                string realUnit = parts[0];
                string task = parts[1];
                string displayUnit = _unitAliases.ContainsKey(realUnit) ? _unitAliases[realUnit] : realUnit;
                int idx = axisIndexMap[key];
                string labelText = $"[{displayUnit}]\n{task}";
                area.AxisX.CustomLabels.Add(idx - 0.5, idx + 0.5, labelText);
            }

            if (orderedAxisKeys.Count > 0)
            {
                area.AxisX.Minimum = 0.5;
                area.AxisX.Maximum = orderedAxisKeys.Count + 0.5;
            }

            Dictionary<string, bool> smallLabelToggle = new Dictionary<string, bool>();
            foreach (var p in pointsToAdd)
            {
                if (!axisIndexMap.ContainsKey(p.Key))
                    continue;

                int xIndex = axisIndexMap[p.Key];
                DataPoint dp = new DataPoint();
                dp.XValue = xIndex;
                dp.YValues = new double[] { p.StartTime.ToOADate(), p.EndTime.ToOADate() };
                dp.Color = GetColorForUnit(p.RealUnitName);
                dp.BorderColor = Color.Black;
                dp.BorderWidth = 1;

                string displayUnit = _unitAliases.ContainsKey(p.RealUnitName) ? _unitAliases[p.RealUnitName] : p.RealUnitName;

                dp.ToolTip = $"Unit: {displayUnit}\nTask: {p.TaskName}\nDur: {p.DurationMs:F0}ms";
                //dp.ToolTip = $"Unit: {displayUnit}\nTask: {p.TaskName}\nDur: {p.DurationMs:F1}ms";

                if (p.DurationMs >= 30.0)
                {
                    dp.Label = $"{p.DurationMs:F0}";
                    //dp.Label = $"{p.DurationMs:F1}";
                }
                else
                {
                    if (!smallLabelToggle.ContainsKey(p.Key))
                        smallLabelToggle[p.Key] = true;

                    if (smallLabelToggle[p.Key])
                    {
                        dp.Label = $"{p.DurationMs:F0}";
                        //dp.Label = $"{p.DurationMs:F1}";
                    }

                    smallLabelToggle[p.Key] = !smallLabelToggle[p.Key];
                }

                dp.Font = new Font("Arial", 8, FontStyle.Regular);
                series.Points.Add(dp);
            }

            // =========================================================
            // Total Takt Time 라벨 갱신 로직 (스레드 충돌 방지 추가)
            // =========================================================
            string totalUnitName = "OutputDieTransfer";
            string totalTaskName = "One Cycle";

            if (eq.GetUnit(totalUnitName) is BaseUnit totalUnit && totalUnit.TaktTimers != null)
            {
                if (totalUnit.TaktTimers.TryGetValue(totalTaskName, out CycleTimer totalTimer))
                {
                    try
                    {
                        // 스레드 충돌 방지를 위해 배열 복사 후 확인
                        var cyclesSnapshot = totalTimer.CycleTimes.ToArray();
                        if (cyclesSnapshot.Length > 0)
                        {
                            var latest = cyclesSnapshot[cyclesSnapshot.Length - 1]; // 배열의 마지막 값(=최신 값)

                            labelTotalTackTime.Text = $"Total : {latest.Interval.TotalMilliseconds:0} ms";
                            //labelTotalTackTime.Text = $"Total : {latest.Interval.TotalMilliseconds:0.0} ms";
                        }
                        else
                        {
                            labelTotalTackTime.Text = "Total : - ms";
                        }
                    }
                    catch
                    {
                        // 충돌 시 이전 값 유지
                    }
                }
            }

            // 6. 뷰 윈도우 이동 - 기준 시점부터 최근 뷰초만큼 표시
            area.AxisY.Minimum = minTime.ToOADate();
            area.AxisY.Maximum = now.ToOADate();
        }

        // 유닛별 고정 색상
        private Dictionary<string, Color> _unitColors = new Dictionary<string, Color>();
        private Color GetColorForUnit(string unitName)
        {
            if (!_unitColors.ContainsKey(unitName))
            {
                int hash = unitName.GetHashCode();
                Random r = new Random(hash);
                // 파스텔 톤 색상
                _unitColors[unitName] = Color.FromArgb(r.Next(160, 240), r.Next(160, 240), r.Next(160, 240));
            }
            return _unitColors[unitName];
        }

        private class TaskPoint
        {
            public string RealUnitName { get; set; }
            public string TaskName { get; set; }
            public string Key { get; set; } // "Unit::Task"
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public double DurationMs { get; set; }
        }

        // ==========================================
        // [백그라운드 파일 쓰기 함수 추가]
        // ==========================================
        private void BackgroundLogWriter(CancellationToken token)
        {
            string currentFileDate = "";
            StreamWriter sw = null;

            try
            {
                foreach (var logLine in _logQueue.GetConsumingEnumerable())
                {
                    string today = DateTime.Now.ToString("yyyyMMdd");
                    if (currentFileDate != today || sw == null)
                    {
                        sw?.Dispose();
                        currentFileDate = today;
                        string filePath = Path.Combine(_logDirectory, $"GraphTaktData_{today}.csv");

                        bool writeHeader = !File.Exists(filePath);
                        sw = new StreamWriter(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read));

                        if (writeHeader)
                        {
                            sw.WriteLine("RecordTime,UnitName,TaskName,Duration(ms),StartTime,EndTime");
                        }
                    }

                    sw.WriteLine(logLine);
                    sw.Flush(); // 데이터 유실 방지
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                // 로깅 에러 무시 (장비 동작에 영향 없도록)
            }
            finally
            {
                sw?.Dispose();
            }
        }








    }
}