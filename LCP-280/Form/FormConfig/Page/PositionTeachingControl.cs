using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Unit; // [ADD] IndexChipProbeControllerRecipe
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace QMC.LCP_280.Process.Unit.FormConfig
{
    public interface IHasTeachingPositions
    {
        List<TeachingPosition> TeachingPositions { get; }
    }

    public partial class PositionTeachingControl : UserControl
    {
        private BaseUnit _unit;

        // (기존 호환) 기존 유닛들은 config.TeachingPositions를 사용
        private BaseConfig _config;

        // (공용) 신규 유닛은 recipe.TeachingPositions가 단일 소스
        private IHasTeachingPositions _teachingSource;

        // 부모 Form에 알릴 이벤트들
        public event EventHandler<PositionSelectedEventArgs> PositionSelected;
        public event EventHandler<SavePositionEventArgs> SaveRequested;
        public event EventHandler<MovePositionEventArgs> MoveRequested;
        public event EventHandler<CurrentPosEventArgs> CurrentPosRequested;

        public PositionTeachingControl()
        {
            InitializeComponent();
        }

        // 기존 호환 API
        public void SetUnitData(BaseUnit unit, BaseConfig config)
        {
            SetUnitData(unit, config, teachingSource: null);
        }

        // [CHG] 공용 SetUnitData: 어떤 Recipe든 TeachingPositions만 제공하면 사용 가능
        public void SetUnitData(BaseUnit unit, BaseConfig config, IHasTeachingPositions teachingSource)
        {
            _unit = unit;
            _config = config;
            _teachingSource = teachingSource;

            InitializeUI();
            InitializeRadioButtonView();
        }

        private bool EnsureAxisReadyOrShowMessage(string actionName)
        {
            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스를 찾을 수 없습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return eq.EnsureAxisReadyForAutoOrMove(actionName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "초기화 필요",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        private void InitializeUI()
        {
            try
            {
                BindingList_PositionTeaching();
                WriteEvents_PositionTeaching();
            }
            catch (Exception ex)
            {
                Log.Write("PositionTeachingControl", $"InitializeUI error: {ex}");
            }
        }

        private void InitializeRadioButtonView()
        {
            rbTeachingMoveMode?.SetOptions(false, "Fine", "Coarse");
        }

        public void RefreshPositionList()
        {
            BindingList_PositionTeaching();
        }

        // [FIX] 단일 접근점: recipe 우선, 없으면 config fallback
        private IList<TeachingPosition> GetTeachingList()
        {
            if (_teachingSource != null && _teachingSource.TeachingPositions != null)
                return _teachingSource.TeachingPositions;

            if (_config?.TeachingPositions != null)
                return _config.TeachingPositions;

            return new List<TeachingPosition>();
        }

        private void BindingList_PositionTeaching()
        {
            try
            {
                int prevSelIndex = GetSelectedIndex();

                var list = GetTeachingList();
                if (list != null && list.Count > 0)
                {
                    string[] names = list.Select(t => t?.Name ?? string.Empty).ToArray();
                    positionItemView?.SetItems(names);

                    if (prevSelIndex >= 0 && prevSelIndex < names.Length)
                    {
                        SetSelectedIndex(prevSelIndex);
                        RefreshAxisButtonsForIndex(prevSelIndex);
                        ShowTeachingPositionInEditor(prevSelIndex);
                    }
                    else
                    {
                        BuildAxisButtons(Enumerable.Empty<string>());
                    }
                }
                else
                {
                    positionItemView?.SetItems();
                    BuildAxisButtons(Enumerable.Empty<string>());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BindingList_PositionTeaching 오류: " + ex.Message);
                positionItemView?.SetItems();
                BuildAxisButtons(Enumerable.Empty<string>());
            }
        }

        private void WriteEvents_PositionTeaching()
        {
            if (positionItemView != null)
            {
                positionItemView.ItemSelected -= OnPositionItemSelected;
                positionItemView.ItemSelected += OnPositionItemSelected;
            }

            if (btnSave != null)
            {
                btnSave.Click -= btnSave_Click;
                btnSave.Click += btnSave_Click;
            }

            if (btnCancel != null)
            {
                btnCancel.Click -= btnCancel_Click;
                btnCancel.Click += btnCancel_Click;
            }

            if (btnMovePosition != null)
            {
                btnMovePosition.Click -= btnMovePosition_Click;
                btnMovePosition.Click += btnMovePosition_Click;
            }
        }

        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {
                ShowTeachingPositionInEditor(selectedIndex);
                RefreshAxisButtonsForIndex(selectedIndex);

                PositionSelected?.Invoke(this, new PositionSelectedEventArgs { Index = selectedIndex });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnPositionItemSelected error: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                int selIndex = GetSelectedIndex();
                if (selIndex < 0)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                positionEditorView?.Apply();
                var props = positionEditorView?.GetCurrentProperties();
                if (props == null || props.Count == 0)
                {
                    MessageBox.Show("편집할 데이터가 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                SaveRequested?.Invoke(this, new SavePositionEventArgs
                {
                    Index = selIndex,
                    Properties = props
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"저장 처리 중 오류: {ex.Message}",
                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                int selIndex = GetSelectedIndex();
                if (selIndex < 0)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                CurrentPosRequested?.Invoke(this, new CurrentPosEventArgs { Index = selIndex });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"btnCancel_Click error: {ex.Message}");
            }
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                if (!EnsureAxisReadyOrShowMessage("Teaching.MovePosition"))
                    return;

                var mb = new MessageBoxOk();
                // 1. 현재 상태가 AutoRunning이면 동작 차단
                if (Equipment.Instance.EqState == EquipmentState.AutoRunning ||
                    Equipment.Instance.EqState == EquipmentState.Starting)
                {
                    mb.ShowDialog("Warring", "The equipment is currently running in automatic mode.Please stop it and try again.");
                    return;
                }

                int selIndex = GetSelectedIndex();
                if (selIndex < 0)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var mbYesNo = new MessageBoxYesNo();
                mbYesNo.Title = "이동 확인";
                mbYesNo.Message = "선택된 Teaching Position으로 이동하시겠습니까?";
                var dr = mbYesNo.ShowDialog();
                if (dr != DialogResult.Yes)
                    return;

                bool isFine = GetSelectedMoveModeIsFine();
                MoveRequested?.Invoke(this, new MovePositionEventArgs
                {
                    Index = selIndex,
                    IsFine = isFine
                });
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private void ShowTeachingPositionInEditor(int selectedIndex)
        {
            var list = GetTeachingList();
            if (list == null) return;
            if (selectedIndex < 0 || selectedIndex >= list.Count) return;

            var tp = list[selectedIndex];
            if (tp == null) return;

            var pc = new PropertyCollection();
            pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
            pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

            if (tp.AxisPositions != null)
            {
                foreach (var axis in tp.AxisPositions)
                    pc.Add(new DoubleProperty(axis.Key + " Position (mm)", axis.Value));
            }

            if (tp.ExtraInfo != null)
            {
                foreach (var kv in tp.ExtraInfo)
                    pc.Add(new StringProperty("Extra: " + kv.Key, kv.Value?.ToString() ?? string.Empty));
            }

            positionEditorView?.SetProperties(pc);
        }

        public void UpdateEditorProperties(PropertyCollection properties)
        {
            positionEditorView?.SetProperties(properties);
        }

        private int GetSelectedIndex()
        {
            int selIndex = -1;
            try
            {
                var pi = positionItemView?.GetType().GetProperty("SelectedIndex");
                if (pi != null)
                {
                    object val = pi.GetValue(positionItemView, null);
                    if (val is int) selIndex = (int)val;
                }
            }
            catch { selIndex = -1; }
            return selIndex;
        }

        private void SetSelectedIndex(int index)
        {
            try
            {
                if (positionItemView == null) return;
                var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                if (pi != null && pi.CanWrite) pi.SetValue(positionItemView, index, null);
            }
            catch { }
        }

        private bool GetSelectedMoveModeIsFine()
        {
            bool isFine = true;
            try
            {
                if (rbTeachingMoveMode != null)
                {
                    var siProp = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                    if (siProp != null)
                    {
                        object v = siProp.GetValue(rbTeachingMoveMode, null);
                        if (v is int) isFine = ((int)v) == 0;
                    }
                }
            }
            catch { isFine = true; }
            return isFine;
        }

        private void RefreshAxisButtonsForIndex(int selectedIndex)
        {
            try
            {
                var list = GetTeachingList();
                if (list == null || selectedIndex < 0 || selectedIndex >= list.Count)
                {
                    BuildAxisButtons(Enumerable.Empty<string>());
                    return;
                }

                var tp = list[selectedIndex];
                var axisNames = (tp?.AxisPositions != null) ? tp.AxisPositions.Keys.ToArray() : new string[0];
                BuildAxisButtons(axisNames);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("RefreshAxisButtonsForIndex error: " + ex.Message);
            }
        }

        private void BuildAxisButtons(IEnumerable<string> axisNames)
        {
            if (axisButtonPanel == null)
                return;

            axisButtonPanel.SuspendLayout();
            try
            {
                axisButtonPanel.Controls.Clear();
                if (axisNames == null) return;

                foreach (var name in axisNames)
                {
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var btn = new QMC.Common.IndividualMenuButton();
                    btn.Name = "btnAxis_" + name;
                    btn.Text = name;
                    if (btnMovePosition != null)
                    {
                        btn.Size = btnMovePosition.Size;
                        btn.CustomBackColor = btnMovePosition.CustomBackColor;
                        btn.CustomForeColor = btnMovePosition.CustomForeColor;
                        btn.CustomFont = btnMovePosition.CustomFont;
                        btn.Font = btnMovePosition.Font;
                        btn.ImageSize = btnMovePosition.ImageSize;
                    }
                    btn.Margin = new Padding(0);
                    btn.Tag = name;
                    btn.Click += OnAxisButtonClick;
                    axisButtonPanel.Controls.Add(btn);
                }
            }
            finally
            {
                axisButtonPanel.ResumeLayout();
            }
        }

        private async void OnAxisButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (!EnsureAxisReadyOrShowMessage("Teaching.MoveAxis")) return;

                int selIndex = GetSelectedIndex();
                if (selIndex < 0)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var btn = sender as Control;
                var axisName = btn != null ? (btn.Tag as string ?? btn.Text) : null;
                if (string.IsNullOrEmpty(axisName)) return;

                bool isFine = GetSelectedMoveModeIsFine();

                var list = GetTeachingList();
                if (list == null || selIndex < 0 || selIndex >= list.Count)
                {
                    MessageBox.Show("선택된 Teaching Position 정보가 올바르지 않습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = list[selIndex];
                if (tp?.AxisPositions == null || !tp.AxisPositions.ContainsKey(axisName))
                {
                    MessageBox.Show($"선택된 Teaching Position에 축 '{axisName}' 정보가 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                double dTargetPos = tp.AxisPositions[axisName];

                var mb = new MessageBoxYesNo();
                mb.Title = "이동 확인";
                mb.Message = "선택된 Teaching Position으로 이동하시겠습니까?";
                var dr = mb.ShowDialog();
                if (dr != DialogResult.Yes)
                    return;

                var task = _unit?.MoveAxisPositionOneAsync(axisName, dTargetPos, isFine);
                if (task == null)
                {
                    MessageBox.Show($"축 '{axisName}' 을(를) 찾을 수 없어 이동을 시작하지 못했습니다.",
                        "이동 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var oldCursor = Cursor.Current;
                if (btn != null) btn.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                int rc;
                try
                {
                    rc = await task;
                }
                finally
                {
                    Cursor.Current = oldCursor;
                    if (btn != null) btn.Enabled = true;
                }

                mb.Title = "이동 확인";
                mb.Message = (rc == 0) ? "이동 완료." : "이동 실패.";
                mb.ShowDialog();
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }

    public class PositionSelectedEventArgs : EventArgs { public int Index { get; set; } }
    public class SavePositionEventArgs : EventArgs { public int Index { get; set; } public PropertyCollection Properties { get; set; } }
    public class MovePositionEventArgs : EventArgs { public int Index { get; set; } public bool IsFine { get; set; } }
    public class CurrentPosEventArgs : EventArgs { public int Index { get; set; } }
    public class MoveAxisEventArgs : EventArgs { public int Index { get; set; } public string AxisName { get; set; } public bool IsFine { get; set; } }
}