using QMC.Common;
using QMC.Common.Component;
using QMC.Common.CustomControl;
using QMC.Common.DIO;
using QMC.Common.IO;
using QMC.Common.IOUtil;
using QMC.Common.Motions;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Component; // Added for TeachingPosition
using QMC.LCP_280.Process.Unit.FormConfig;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static QMC.Common.MessageBoxOk;

namespace QMC.LCP_280.Process.Unit
{
    public partial class GageRnRUnit_Config : Form
    {
        private const string _UNIT_NAME = "GageRnR";

        private Equipment _Equipment => Equipment.Instance;
        private GageRnR _unit;
        private GageRnRConfig _cfg;

        private HardInputDef[] _hardInputs;
        private HardOutputDef[] _hardOutputs;

        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        private Timer _axisPosTimer; // reserved
        private Timer _ioTimer;      // reserved

        public GageRnRUnit_Config()
        {
            InitializeComponent();

            InitializeUnit();

            SuspendLayout();
            _designerSize = Size;
            InitializeUI();
            ResumeLayout(true);
        }

        private void InitializeUnit()
        {
            try
            {
                if (_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    _unit = unit as GageRnR;
                    _cfg = _unit?.Config;
                    _hardInputs = _hardInputs ?? Array.Empty<HardInputDef>();
                    _hardOutputs = _hardOutputs ?? Array.Empty<HardOutputDef>();
                }

                if (_unit == null)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Error!", _UNIT_NAME + " Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.");
                }
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", "Unit 초기화 중 오류 발생: " + ex.Message);
            }
        }

        #region UI 초기화

        private void InitializeUI()
        {
            try
            {
                SetupPositionTeachingControl(); //PositionTeachingControl에 데이터 전달
                SetupDigitalIOControl();

                PopulateAllAxesInJogControl();
                InitializeUnitConfigPanel(); // <-- 추가
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        private void SetupPositionTeachingControl()
        {
            if (positionTeachingControl == null) return;

            // 데이터 전달
            positionTeachingControl.SetUnitData(_unit, _cfg);

            // 이벤트 연결
            positionTeachingControl.PositionSelected += OnPositionTeachingSelected;
            positionTeachingControl.SaveRequested += OnPositionTeachingSaveRequested;
            positionTeachingControl.MoveRequested += OnPositionTeachingMoveRequested;
            positionTeachingControl.CurrentPosRequested += OnPositionTeachingCurrentPosRequested;
        }

        private void SetupDigitalIOControl()
        {
            if(digitalIOControl == null) return;

            digitalIOControl.SetUnitData(_unit, _cfg, _hardInputs, _hardOutputs);
        }

        /// <summary>
        /// 우측 UnitConfig UserControl 초기화
        /// </summary>
        private void InitializeUnitConfigPanel()
        {
            try
            {
                if (unitConfigControl == null) return;
                if (_cfg == null)
                {
                    unitConfigControl.BindConfig(null);
                    return;
                }
                unitConfigControl.BindConfig(_cfg);

                // 기본 선택 TeachingPosition 없을 때 첫 항목 표시
                if (_cfg.TeachingPositions != null && _cfg.TeachingPositions.Count == 0)
                {
                    try { _cfg.InitializeDefaultTeachingPositions(); } catch { }
                    positionTeachingControl.RefreshPositionList();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUnitConfigPanel error: " + ex.Message);
            }
        }

        #endregion

        #region PositionTeaching Event Handlers

        private void OnPositionTeachingSelected(object sender, PositionSelectedEventArgs e)
        {
            // 필요시 추가 처리
            Debug.WriteLine($"Position selected: {e.Index}");
        }

        private void OnPositionTeachingSaveRequested(object sender, SavePositionEventArgs e)
        {
            try
            {
                if (_unit == null || e.Index < 0 || e.Index >= _unit.TeachingPositions.Count)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "선택된 Teaching Position이 없습니다.");

                    return;
                }

                var target = _unit.TeachingPositions[e.Index];
                var newAxisPositions = new Dictionary<string, double>(target.AxisPositions ?? new Dictionary<string, double>());
                string newDescription = target.Description;
                var newExtra = new Dictionary<string, object>(target.ExtraInfo ?? new Dictionary<string, object>());

                // Property 파싱
                foreach (var p in e.Properties)
                {
                    if (p is StringProperty sp && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        newDescription = sp.Value ?? string.Empty;
                    }
                    else if (p is DoubleProperty dp && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        int pos = p.Title.IndexOf(" Position (mm)", StringComparison.OrdinalIgnoreCase);
                        string axisKey = p.Title.Substring(0, pos).Trim();
                        newAxisPositions[axisKey] = dp.Value;
                    }
                    else if (p is StringProperty sp2 && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        string extraKey = p.Title.Substring("Extra:".Length).Trim();
                        newExtra[extraKey] = sp2.Value;
                    }
                }

                // 저장
                target.Description = newDescription;
                target.AxisPositions = newAxisPositions;
                target.ExtraInfo = newExtra;

                _unit.Config.SetTeachingPosition(new TeachingPosition(
                    target.Name,
                    new Dictionary<string, double>(target.AxisPositions),
                    target.Description)
                {
                    ExtraInfo = new Dictionary<string, object>(target.ExtraInfo)
                });

                _unit.Config.LoadAndBindAxes(Equipment.Instance.AxisManager);

                var snapshot = _unit.Config.TeachingPositions != null
                     ? new List<TeachingPosition>(_unit.Config.TeachingPositions)
                     : new List<TeachingPosition>();

                _unit.Config.TeachingPositions = snapshot.ToList();
                if (_unit.TeachingPositions != null)
                {
                    _unit.TeachingPositions.Clear();
                    foreach (var tp in snapshot)
                    {
                        _unit.TeachingPositions.Add(tp);
                    }
                }

                // UI 갱신
                positionTeachingControl.RefreshPositionList();

                var mb1 = new MessageBoxOk();
                mb1.ShowDialog("Notification!", "변경된 Teaching Position이 저장되었습니다.");
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", $"저장 처리 중 오류: {ex.Message}");
            }
        }

        private async void OnPositionTeachingMoveRequested(object sender, MovePositionEventArgs e)
        {
            try
            {
                if (_unit == null) return;

                var task = _unit.MoveTeachingPositionOnceAsync(e.Index, e.IsFine);

                using (var pf = new ProgressForm(_UNIT_NAME, "Teaching Position 이동 중...", task))
                {
                    var dr = pf.ShowDialog(this);
                    if (dr == DialogResult.Cancel)
                    {
                        _unit.StopTeachingPositionOnce(e.Index);
                        return;
                    }
                }

                var result = await task;
                var mb = new MessageBoxOk();
                if (result == 0)
                {
                    mb.ShowDialog("Information.", "Teaching Position 이동 완료");
                }
                else
                {
                    mb.ShowDialog("Error.", "일부 축 이동 실패 또는 타임아웃");
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void OnPositionTeachingCurrentPosRequested(object sender, CurrentPosEventArgs e)
        {
            try
            {
                if (_cfg?.TeachingPositions == null || e.Index < 0 || e.Index >= _cfg.TeachingPositions.Count)
                {
                    var mb = new MessageBoxOk();
                    mb.ShowDialog("Notification!", "선택된 Teaching Position이 없습니다.");
                    return;
                }

                var tp = _cfg.TeachingPositions[e.Index];
                var updatedPositions = new Dictionary<string, double>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;
                    MotionAxis axis = null;

                    if (tp.Axes != null)
                    {
                        tp.Axes.TryGetValue(axisKey, out axis);
                    }

                    if (axis == null && _unit?.Axes != null && _unit.Axes.TryGetValue(axisKey, out var direct))
                    {
                        axis = direct;
                    }

                    if (axis == null && _unit?.Axes != null)
                    {
                        foreach (var pair in _unit.Axes)
                        {
                            MotionAxis a = pair.Value;
                            if (a != null && string.Equals(a.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = a;
                                break;
                            }
                        }
                    }

                    double pos = fallback;
                    if (axis != null)
                    {
                        try { pos = axis.GetPosition(); }
                        catch { pos = fallback; }
                    }
                    updatedPositions[axisKey] = pos;
                }

                var pc = new PropertyCollection();
                pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
                pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

                foreach (var ap in updatedPositions)
                {
                    pc.Add(new DoubleProperty(ap.Key + " Position (mm)", ap.Value));
                }

                foreach (var extra in tp.ExtraInfo)
                {
                    pc.Add(new StringProperty("Extra: " + extra.Key, extra.Value?.ToString() ?? string.Empty));
                }

                positionTeachingControl.UpdateEditorProperties(pc);
            }
            catch (Exception ex)
            {
                var mb = new MessageBoxOk();
                mb.ShowDialog("Error!", $"현재 위치 읽기 중 오류: {ex.Message}");
            }
        }

        #endregion


        #region JogControl

        private void PopulateAllAxesInJogControl()
        {
            try
            {
                if (jogControl == null) return;

                // InitializeUnit()에서 채운 _unit을 그대로 사용
                if (_unit?.Axes == null || _unit.Axes.Count == 0)
                {
                    jogControl.SetTeachingAxisList(null);
                    return;
                }
                string[] axisNames = _unit.Axes.Values
                    .Where(a => a != null)
                    .Select(a => a.Name ?? a.Setup?.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToArray();
                jogControl.SetTeachingAxisList(axisNames);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PopulateAllAxesInJogControl error: " + ex.Message);
            }
        }

        #endregion

        #region Panel Size

        public void SetPanelSize(int width, int height)
        {
            if (!_sizeMismatchWarned && (width != _designerSize.Width || height != _designerSize.Height))
            {
                _sizeMismatchWarned = true;
                Debug.WriteLine(
                    "[SizeMismatch] Form=" + GetType().Name +
                    " Designer=" + _designerSize.Width + "x" + _designerSize.Height +
                    " Requested=" + width + "x" + height
                );
            }
            try
            {
                SuspendLayout();
                Size = new Size(width, height);
                Invalidate();
                Update();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        #endregion

        #region Axis 선택 / 위치 업데이트 (확장 포인트)

        private void OnAxisSelected(object sender, int index)
        {
            // 필요 시 구현
        }

        private void UpdateAxisActualPosition()
        {
            // Timer 활용 예정 지점
        }

        #endregion

        #region Paint / Resize

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int centerX = ClientSize.Width / 2;
            using (var blackPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(blackPen, centerX, 0, centerX, ClientSize.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        #endregion
    }
}