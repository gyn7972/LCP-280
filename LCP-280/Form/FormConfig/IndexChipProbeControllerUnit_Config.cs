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

namespace QMC.LCP_280.Process.Unit
{
    public partial class IndexChipProbeControllerUnit_Config : Form
    {
        private const string _UNIT_NAME = "IndexChipProbeController";

        private Equipment _Equipment => Equipment.Instance;
        private IndexChipProbeController _unit;
        private IndexChipProbeControllerConfig _cfg;
        private IndexChipProbeControllerRecipe TeachingRecipe => _cfg?.TeachingRecipe; // [ADD]

        private HardInputDef[] _hardInputs;
        private HardOutputDef[] _hardOutputs;

        private readonly Size _designerSize;
        private bool _sizeMismatchWarned;

        private readonly List<IoRef> _ioInputs = new List<IoRef>();
        private readonly List<IoRef> _ioOutputs = new List<IoRef>();

        private volatile bool _pendingTeachingRefresh;

        public IndexChipProbeControllerUnit_Config()
        {
            InitializeComponent();

            InitializeUnit();

            SuspendLayout();
            _designerSize = Size;
            InitializeUI();
            ResumeLayout(true);

            // [ADD] Recipe 변경 이벤트 구독 (Teaching은 Recipe를 따라가야 함)
            this.Shown += IndexChipProbeControllerUnit_Config_Shown;
            this.FormClosed += IndexChipProbeControllerUnit_Config_FormClosed;

            // [ADD] 보이게 되는 순간 누락된 갱신 처리
            this.VisibleChanged += (s, e) =>
            {
                if (Visible && _pendingTeachingRefresh)
                {
                    _pendingTeachingRefresh = false;
                    try { ReloadTeachingFromRecipeAndRefreshUi(); } catch { }
                }
            };
            this.Activated += (s, e) =>
            {
                if (_pendingTeachingRefresh)
                {
                    _pendingTeachingRefresh = false;
                    try { ReloadTeachingFromRecipeAndRefreshUi(); } catch { }
                }
            };
        }

        private void IndexChipProbeControllerUnit_Config_Shown(object sender, EventArgs e)
        {
            EquipmentRecipe.CurrentRecipeChanged -= EquipmentRecipe_CurrentRecipeChanged;
            EquipmentRecipe.CurrentRecipeChanged += EquipmentRecipe_CurrentRecipeChanged;

            // 현재 표시 시점에서도 한 번 동기화
            try
            {
                ReloadTeachingFromRecipeAndRefreshUi();
            }
            catch { }
        }

        private void IndexChipProbeControllerUnit_Config_FormClosed(object sender, FormClosedEventArgs e)
        {
            EquipmentRecipe.CurrentRecipeChanged -= EquipmentRecipe_CurrentRecipeChanged;
        }
        private void EquipmentRecipe_CurrentRecipeChanged(object sender, EquipmentRecipe.MeasurementRecipeChangedEventArgs e)
        {
            try
            {
                // 폼 핸들 없으면 표시 시점에 처리하도록 플래그만 세팅
                if (!IsHandleCreated)
                {
                    _pendingTeachingRefresh = true;
                    return;
                }

                if (InvokeRequired)
                {
                    BeginInvoke(new Action(() => EquipmentRecipe_CurrentRecipeChanged(sender, e)));
                    return;
                }

                // [CHG] Visible 체크로 이벤트를 버리지 말고, 안보이면 나중에 처리
                if (!Visible)
                {
                    _pendingTeachingRefresh = true;
                    return;
                }

                _cfg?.InvalidateTeachingRecipeCache();
                ReloadTeachingFromRecipeAndRefreshUi();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
        private void ReloadTeachingFromRecipeAndRefreshUi()
        {
            if (_cfg == null)
                return;

            var recipe = TeachingRecipe;
            if (recipe == null)
                return;

            // 레시피 전환 시 레시피를 직접 로드/바인딩 (파일 없으면 기본 생성 포함)
            recipe.LoadAndBindAxes(Equipment.Instance.AxisManager);

            // [FIX] 핵심: PositionTeachingControl 내부가 예전 recipe reference를 들고있을 수 있으므로 재주입
            positionTeachingControl?.SetUnitData(_unit, _cfg, recipe);
            positionTeachingControl?.RefreshPositionList();

            // [ADD] Config 값도 UI에 다시 반영 (레시피 전환/티칭 로드 후 UI 갱신 누락 방지)
            try
            {
                if (unitConfigControl != null)
                {
                    unitConfigControl.BindConfig(null);
                    unitConfigControl.BindConfig(_cfg);
                }
            }
            catch { }

            // 확인용 로그(필요 시)
            try
            {
                var mr = Equipment.Instance?.EquipmentRecipe?.CurrentRecipe;
                //Log.Write(_UNIT_NAME, nameof(ReloadTeachingFromRecipeAndRefreshUi),
                //    $"MR.Name='{mr?.Name}', MR.TeachingRecipeName='{mr?.IndexChipProbeControllerTeachingRecipeName}', " +
                //    $"TeachingRecipe.Name='{recipe.Name}', TP.Count={(recipe.TeachingPositions?.Count ?? 0)}");
                Log.Write(_UNIT_NAME, nameof(ReloadTeachingFromRecipeAndRefreshUi),
                    $"MR.Name='{mr?.Name}', MR.TeachingRecipeName='{mr?.Name}', " +
                    $"TeachingRecipe.Name='{recipe.Name}', TP.Count={(recipe.TeachingPositions?.Count ?? 0)}");
            }
            catch { }
        }

        private void InitializeUnit()
        {
            try
            {
                if (_Equipment.Units.TryGetValue(_UNIT_NAME, out var unit))
                {
                    _unit = unit as IndexChipProbeController;
                    _cfg = _unit?.Config;
                }

                Log.Write(_UNIT_NAME, nameof(InitializeUnit),
                        $"CfgLoaded? IndexOfProbe={_cfg?.IndexOfProbe}, ViewMode={_cfg?.ViewMode}, GripperMode={_cfg?.GripperMode}");


                if (_unit == null)
                {
                    MessageBox.Show(
                        _UNIT_NAME + " Unit을 찾을 수 없습니다.\nEquipment에 Unit이 등록되어 있는지 확인하세요.",
                        "오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Unit 초기화 중 오류 발생: " + ex.Message,
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        #region UI 초기화

        private void InitializeUI()
        {
            try
            {
                SetupPositionTeachingControl();
                SetupDigitalIOControl();

                PopulateAllAxesInJogControl();
                InitializeUnitConfigPanel();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        private void SetupPositionTeachingControl()
        {
            if (positionTeachingControl == null) return;

            positionTeachingControl.SetUnitData(_unit, _cfg, TeachingRecipe);

            positionTeachingControl.PositionSelected += OnPositionTeachingSelected;
            positionTeachingControl.SaveRequested += OnPositionTeachingSaveRequested;
            positionTeachingControl.MoveRequested += OnPositionTeachingMoveRequested;
            positionTeachingControl.CurrentPosRequested += OnPositionTeachingCurrentPosRequested;
        }

        private void SetupDigitalIOControl()
        {
            if (digitalIOControl == null) return;

            _hardInputs = _cfg?.HardInputs ?? Array.Empty<HardInputDef>();
            _hardOutputs = _cfg?.HardOutputs ?? Array.Empty<HardOutputDef>();

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

                // [CHG] 기본 Teaching 생성도 Recipe 기준
                var recipe = TeachingRecipe;
                if (recipe != null && (recipe.TeachingPositions == null || recipe.TeachingPositions.Count == 0))
                {
                    try { recipe.InitializeDefaultTeachingPositions(save: true); } catch { }
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
                var eq = Equipment.Instance;
                var mr = eq?.EquipmentRecipe?.CurrentRecipe;

                var recipe = TeachingRecipe;
                if (recipe == null)
                    return;

                Log.Write(_UNIT_NAME, nameof(OnPositionTeachingSaveRequested),
                        $"MR.Name='{mr?.Name}', MR.TeachingRecipeName='{mr?.Name}', " +
                        $"TeachingRecipe.Name='{recipe.Name}', TP.Count={(recipe.TeachingPositions?.Count ?? 0)}");

                if (recipe.TeachingPositions == null || e.Index < 0 || e.Index >= recipe.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var target = recipe.TeachingPositions[e.Index];

                var newAxisPositions = new Dictionary<string, double>(target.AxisPositions ?? new Dictionary<string, double>());
                string newDescription = target.Description;
                var newExtra = new Dictionary<string, object>(target.ExtraInfo ?? new Dictionary<string, object>());

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

                target.Description = newDescription;
                target.AxisPositions = newAxisPositions;
                target.ExtraInfo = newExtra;

                // [CHG] 저장은 Recipe에 직접(축 필터 포함)
                recipe.UpsertFiltered(new TeachingPosition(
                    target.Name,
                    new Dictionary<string, double>(target.AxisPositions),
                    target.Description)
                {
                    ExtraInfo = new Dictionary<string, object>(target.ExtraInfo)
                }, save: true);

                ReloadTeachingFromRecipeAndRefreshUi();
                positionTeachingControl?.RefreshPositionList();

                MessageBox.Show("변경된 Teaching Position이 저장되었습니다.",
                    "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 처리 중 오류: {ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnPositionTeachingMoveRequested(object sender, MovePositionEventArgs e)
        {
            try
            {
                if (_unit == null)
                    return;

                int nRet = await Task.Run(() => _unit.MoveToTeachingPositionBySelectionIndex(e.Index, e.IsFine));
                if (nRet != 0)
                {
                    new MessageBoxOk().ShowDialog("Error.", "Teaching 위치 이동 실패");
                    return;
                }

                new MessageBoxOk().ShowDialog("Infor.", "이동 완료");
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                new MessageBoxOk().ShowDialog("Error.", "예외 발생");
            }
        }

        private void OnPositionTeachingCurrentPosRequested(object sender, CurrentPosEventArgs e)
        {
            try
            {
                var recipe = TeachingRecipe;
                if (recipe?.TeachingPositions == null || e.Index < 0 || e.Index >= recipe.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = recipe.TeachingPositions[e.Index];
                var updatedPositions = new Dictionary<string, double>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double fallback = kv.Value;
                    MotionAxis axis = null;

                    if (tp.Axes != null)
                        tp.Axes.TryGetValue(axisKey, out axis);

                    if (axis == null && _unit?.Axes != null && _unit.Axes.TryGetValue(axisKey, out var direct))
                        axis = direct;

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
                    pc.Add(new DoubleProperty(ap.Key + " Position (mm)", ap.Value));

                if (tp.ExtraInfo != null)
                {
                    foreach (var extra in tp.ExtraInfo)
                        pc.Add(new StringProperty("Extra: " + extra.Key, extra.Value?.ToString() ?? string.Empty));
                }

                positionTeachingControl.UpdateEditorProperties(pc);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"현재 위치 읽기 중 오류: {ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void unitConfigControl_Load(object sender, EventArgs e)
        {


        }
    }
}