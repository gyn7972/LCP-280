using QMC.Common;
using QMC.Common.Component;
using QMC.Common.Unit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormConfig
{
    public partial class PositionTeachingControl : UserControl
    {
        private BaseUnit _unit;
        private BaseConfig _config;

        // 부모 Form에 알릴 이벤트들
        public event EventHandler<PositionSelectedEventArgs> PositionSelected;
        public event EventHandler<SavePositionEventArgs> SaveRequested;
        public event EventHandler<MovePositionEventArgs> MoveRequested;
        public event EventHandler<CurrentPosEventArgs> CurrentPosRequested;

        public PositionTeachingControl()
        {
            InitializeComponent();
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
            rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
        }

        public void SetUnitData(BaseUnit unit, BaseConfig config)
        {
            _unit = unit;
            _config = config;

            InitializeUI();
            InitializeRadioButtonView();
        }

        public void RefreshPositionList()
        {
            BindingList_PositionTeaching();
        }

        private void BindingList_PositionTeaching()
        {
            try
            {
                if (_config?.TeachingPositions != null && _config.TeachingPositions.Count > 0)
                {
                    string[] names = _config.TeachingPositions
                        .Select(t => t.Name)
                        .ToArray();

                    positionItemView?.SetItems(names);
                }
                else
                {
                    positionItemView?.SetItems();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("BindingList_PositionTeaching 오류: " + ex.Message);
            }
        }

        #region Event Registration

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

                // 부모에게 선택 변경 알림
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

                // 부모에게 저장 요청
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

                // 부모에게 현재 위치 읽기 요청
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
                int selIndex = GetSelectedIndex();
                if (selIndex < 0)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                bool isFine = GetSelectedMoveModeIsFine();

                // 부모에게 이동 요청
                MoveRequested?.Invoke(this, new MovePositionEventArgs
                {
                    Index = selIndex,
                    IsFine = isFine
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"btnMovePosition_Click error: {ex.Message}");
            }
        }

        #endregion

        #region Helper Methods
        private void ShowTeachingPositionInEditor(int selectedIndex)
        {
            if (_config?.TeachingPositions == null) return;
            if (selectedIndex < 0 || selectedIndex >= _config.TeachingPositions.Count) return;

            var tp = _config.TeachingPositions[selectedIndex];

            var pc = new PropertyCollection();
            pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
            pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

            foreach (var axis in tp.AxisPositions)
            {
                pc.Add(new DoubleProperty(axis.Key + " Position (mm)", axis.Value));
            }

            foreach (var kv in tp.ExtraInfo)
            {
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
                    if (val is int)
                    {
                        selIndex = (int)val;
                    }
                }
            }
            catch
            {
                selIndex = -1;
            }
            return selIndex;
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
                        if (v is int)
                        {
                            isFine = ((int)v) == 0;
                        }
                    }
                }
            }
            catch
            {
                isFine = true;
            }
            return isFine;
        }

        #endregion
    }

    #region EventArgs Classes

    public class PositionSelectedEventArgs : EventArgs
    {
        public int Index { get; set; }
    }

    public class SavePositionEventArgs : EventArgs
    {
        public int Index { get; set; }
        public PropertyCollection Properties { get; set; }
    }

    public class MovePositionEventArgs : EventArgs
    {
        public int Index { get; set; }
        public bool IsFine { get; set; }
    }

    public class CurrentPosEventArgs : EventArgs
    {
        public int Index { get; set; }
    }

    #endregion
}