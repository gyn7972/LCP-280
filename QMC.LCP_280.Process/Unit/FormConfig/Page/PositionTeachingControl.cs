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
        // 각 축 이동 요청 이벤트(선택된 Teaching Position 내 특정 축만 이동)
        public event EventHandler<MoveAxisEventArgs> MoveAxisRequested;

        public PositionTeachingControl()
        {
            InitializeComponent();
        }



        

        public void SetUnitData(BaseUnit unit, BaseConfig config)
        {
            _unit = unit;
            _config = config;

            InitializeUI();
            InitializeRadioButtonView();
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

        private void BindingList_PositionTeaching()
        {
            try
            {
                // 현재 선택 인덱스 저장
                int prevSelIndex = GetSelectedIndex();

                if (_config?.TeachingPositions != null && _config.TeachingPositions.Count > 0)
                {
                    string[] names = _config.TeachingPositions
                        .Select(t => t.Name)
                        .ToArray();

                    positionItemView?.SetItems(names);

                    // 선택 인덱스 복원 (범위 체크)
                    if (prevSelIndex >= 0 && prevSelIndex < names.Length)
                    {
                        SetSelectedIndex(prevSelIndex);
                        RefreshAxisButtonsForIndex(prevSelIndex);
                        ShowTeachingPositionInEditor(prevSelIndex);
                    }
                    else
                    {
                        // 기존 선택이 없거나 범위를 벗어난 경우 축 버튼 초기화
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
                RefreshAxisButtonsForIndex(selectedIndex);

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

                var mb = new MessageBoxYesNo();
                mb.Title = "이동 확인";
                mb.Message = "선택된 Teaching Position으로 이동하시겠습니까?";
                var dr = mb.ShowDialog();
                if (dr != DialogResult.Yes)
                    return;

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
                Log.Write(ex);
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

        // 선택 인덱스 설정(리플렉션)
        private void SetSelectedIndex(int index)
        {
            try
            {
                if (positionItemView == null) return;
                var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                if (pi != null && pi.CanWrite)
                {
                    pi.SetValue(positionItemView, index, null);
                }
            }
            catch
            {
                // 무시: 설정 실패 시 포커스 복원 불가
            }
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

        private void RefreshAxisButtonsForIndex(int selectedIndex)
        {
            try
            {
                if (_config?.TeachingPositions == null || selectedIndex < 0 || selectedIndex >= _config.TeachingPositions.Count)
                {
                    BuildAxisButtons(Enumerable.Empty<string>());
                    return;
                }

                var tp = _config.TeachingPositions[selectedIndex];
                var axisNames = (tp.AxisPositions != null) ? tp.AxisPositions.Keys.ToArray() : new string[0];
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
                    // 동일 크기 적용
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
                    btn.Tag = name; // 축 이름 저장
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

                double dTargetPos = 0.0;
                if (_config?.TeachingPositions != null &&
                    selIndex >= 0 && selIndex < _config.TeachingPositions.Count)
                {
                    var tp = _config.TeachingPositions[selIndex];
                    if (tp.AxisPositions != null && tp.AxisPositions.ContainsKey(axisName))
                    {
                        dTargetPos = tp.AxisPositions[axisName];
                    }
                    else
                    {
                        MessageBox.Show($"선택된 Teaching Position에 축 '{axisName}' 정보가 없습니다.",
                            "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("선택된 Teaching Position 정보가 올바르지 않습니다.",
                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var mb = new MessageBoxYesNo();
                mb.Title = "이동 확인";
                mb.Message = "선택된 Teaching Position으로 이동하시겠습니까?";
                var dr = mb.ShowDialog();
                if (dr != DialogResult.Yes)
                    return;

                // 이동 실행 및 결과 처리
                var task = _unit?.MoveAxisPositionOneAsync(axisName, dTargetPos, isFine);
                if (task == null)
                {
                    MessageBox.Show($"축 '{axisName}' 을(를) 찾을 수 없어 이동을 시작하지 못했습니다.",
                        "이동 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // UI 피드백
                var oldCursor = Cursor.Current;
                if (btn != null) btn.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                int rc;
                try
                {
                    rc = await task; // UI 스레드 콘텍스트로 복귀
                }
                finally
                {
                    Cursor.Current = oldCursor;
                    if (btn != null) btn.Enabled = true;
                }

                var mbResult = new MessageBoxOk();
                if (rc == 0)
                {
                    mb.Title = "이동 확인";
                    mb.Message = "이동 완료.";
                    mb.ShowDialog();
                    //MessageBox.Show($"[{axisName}] {dTargetPos} 위치로 이동 완료.",
                    //    "이동 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    mb.Title = "이동 확인";
                    mb.Message = "이동 실패.";
                    mb.ShowDialog();
                    //MessageBox.Show($"[{axisName}] {dTargetPos} 위치로 이동 실패 (rc={rc}).",
                    //    "이동 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                //Debug.WriteLine("OnAxisButtonClick error: " + ex.Message);
                //MessageBox.Show("축 이동 중 오류가 발생했습니다.\n" + ex.Message,
                //    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

    public class MoveAxisEventArgs : EventArgs
    {
        public int Index { get; set; }
        public string AxisName { get; set; }
        public bool IsFine { get; set; }
    }

    #endregion
}


//using QMC.Common;
//using QMC.Common.Component;
//using QMC.Common.Unit;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Windows.Forms;

//namespace QMC.LCP_280.Process.Unit.FormConfig
//{
//    public partial class PositionTeachingControl : UserControl
//    {
//        private BaseUnit _unit;
//        private BaseConfig _config;

//        // 부모 Form에 알릴 이벤트들
//        public event EventHandler<PositionSelectedEventArgs> PositionSelected;
//        public event EventHandler<SavePositionEventArgs> SaveRequested;
//        public event EventHandler<MovePositionEventArgs> MoveRequested;
//        public event EventHandler<CurrentPosEventArgs> CurrentPosRequested;
//        // 각 축 이동 요청 이벤트(선택된 Teaching Position 내 특정 축만 이동)
//        public event EventHandler<MoveAxisEventArgs> MoveAxisRequested;

//        public PositionTeachingControl()
//        {
//            InitializeComponent();
//        }

//        public void SetUnitData(BaseUnit unit, BaseConfig config)
//        {
//            _unit = unit;
//            _config = config;

//            InitializeUI();
//            InitializeRadioButtonView();
//        }

//        private void InitializeUI()
//        {
//            try
//            {
//                BindingList_PositionTeaching();
//                WriteEvents_PositionTeaching();
//            }
//            catch (Exception ex)
//            {
//                Log.Write("PositionTeachingControl", $"InitializeUI error: {ex}");
//            }
//        }

//        private void InitializeRadioButtonView()
//        {
//            rbTeachingMoveMode?.SetOptions(false, "Fine", "Coarse");
//        }

//        public void RefreshPositionList()
//        {
//            BindingList_PositionTeaching();
//        }

//        private void BindingList_PositionTeaching()
//        {
//            try
//            {
//                if (_config?.TeachingPositions != null && _config.TeachingPositions.Count > 0)
//                {
//                    string[] names = _config.TeachingPositions
//                        .Select(t => t.Name)
//                        .ToArray();

//                    positionItemView?.SetItems(names);

//                    // 선택된 항목 기준으로 축 버튼도 갱신
//                    int selIndex = GetSelectedIndex();
//                    if (selIndex >= 0)
//                        RefreshAxisButtonsForIndex(selIndex);
//                    else
//                        BuildAxisButtons(Enumerable.Empty<string>());
//                }
//                else
//                {
//                    positionItemView?.SetItems();
//                    BuildAxisButtons(Enumerable.Empty<string>());
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("BindingList_PositionTeaching 오류: " + ex.Message);
//            }
//        }

//        #region Event Registration

//        private void WriteEvents_PositionTeaching()
//        {
//            if (positionItemView != null)
//            {
//                positionItemView.ItemSelected -= OnPositionItemSelected;
//                positionItemView.ItemSelected += OnPositionItemSelected;
//            }

//            if (btnSave != null)
//            {
//                btnSave.Click -= btnSave_Click;
//                btnSave.Click += btnSave_Click;
//            }

//            if (btnCancel != null)
//            {
//                btnCancel.Click -= btnCancel_Click;
//                btnCancel.Click += btnCancel_Click;
//            }

//            if (btnMovePosition != null)
//            {
//                btnMovePosition.Click -= btnMovePosition_Click;
//                btnMovePosition.Click += btnMovePosition_Click;
//            }
//        }

//        private void OnPositionItemSelected(object sender, int selectedIndex)
//        {
//            try
//            {
//                ShowTeachingPositionInEditor(selectedIndex);
//                RefreshAxisButtonsForIndex(selectedIndex);

//                // 부모에게 선택 변경 알림
//                PositionSelected?.Invoke(this, new PositionSelectedEventArgs { Index = selectedIndex });
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("OnPositionItemSelected error: " + ex.Message);
//            }
//        }

//        private void btnSave_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                int selIndex = GetSelectedIndex();
//                if (selIndex < 0)
//                {
//                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
//                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    return;
//                }

//                positionEditorView?.Apply();
//                var props = positionEditorView?.GetCurrentProperties();
//                if (props == null || props.Count == 0)
//                {
//                    MessageBox.Show("편집할 데이터가 없습니다.",
//                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    return;
//                }

//                // 부모에게 저장 요청
//                SaveRequested?.Invoke(this, new SavePositionEventArgs
//                {
//                    Index = selIndex,
//                    Properties = props
//                });
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"저장 처리 중 오류: {ex.Message}",
//                    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        private void btnCancel_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                int selIndex = GetSelectedIndex();
//                if (selIndex < 0)
//                {
//                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
//                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    return;
//                }

//                // 부모에게 현재 위치 읽기 요청
//                CurrentPosRequested?.Invoke(this, new CurrentPosEventArgs { Index = selIndex });
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"btnCancel_Click error: {ex.Message}");
//            }
//        }

//        private void btnMovePosition_Click(object sender, EventArgs e)
//        {
//            try
//            {
//                int selIndex = GetSelectedIndex();
//                if (selIndex < 0)
//                {
//                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
//                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    return;
//                }

//                var mb = new MessageBoxYesNo();
//                mb.Title = "이동 확인";
//                mb.Message = "선택된 Teaching Position으로 이동하시겠습니까?";
//                var dr = mb.ShowDialog();
//                if (dr != DialogResult.Yes)
//                    return;

//                bool isFine = GetSelectedMoveModeIsFine();
//                // 부모에게 이동 요청
//                MoveRequested?.Invoke(this, new MovePositionEventArgs
//                {
//                    Index = selIndex,
//                    IsFine = isFine
//                });
//            }
//            catch (Exception ex)
//            {
//                Log.Write(ex);
//            }
//        }

//        #endregion

//        #region Helper Methods
//        private void ShowTeachingPositionInEditor(int selectedIndex)
//        {
//            if (_config?.TeachingPositions == null) return;
//            if (selectedIndex < 0 || selectedIndex >= _config.TeachingPositions.Count) return;

//            var tp = _config.TeachingPositions[selectedIndex];

//            var pc = new PropertyCollection();
//            pc.Add(new TitleOnlyProperty("Teaching Position: " + tp.Name + " (mm, Abs. Pos)"));
//            pc.Add(new StringProperty("Description", tp.Description ?? string.Empty));

//            foreach (var axis in tp.AxisPositions)
//            {
//                pc.Add(new DoubleProperty(axis.Key + " Position (mm)", axis.Value));
//            }

//            foreach (var kv in tp.ExtraInfo)
//            {
//                pc.Add(new StringProperty("Extra: " + kv.Key, kv.Value?.ToString() ?? string.Empty));
//            }

//            positionEditorView?.SetProperties(pc);
//        }

//        public void UpdateEditorProperties(PropertyCollection properties)
//        {
//            positionEditorView?.SetProperties(properties);
//        }

//        private int GetSelectedIndex()
//        {
//            int selIndex = -1;
//            try
//            {
//                var pi = positionItemView?.GetType().GetProperty("SelectedIndex");
//                if (pi != null)
//                {
//                    object val = pi.GetValue(positionItemView, null);
//                    if (val is int)
//                    {
//                        selIndex = (int)val;
//                    }
//                }
//            }
//            catch
//            {
//                selIndex = -1;
//            }
//            return selIndex;
//        }

//        private bool GetSelectedMoveModeIsFine()
//        {
//            bool isFine = true;
//            try
//            {
//                if (rbTeachingMoveMode != null)
//                {
//                    var siProp = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
//                    if (siProp != null)
//                    {
//                        object v = siProp.GetValue(rbTeachingMoveMode, null);
//                        if (v is int)
//                        {
//                            isFine = ((int)v) == 0;
//                        }
//                    }
//                }
//            }
//            catch
//            {
//                isFine = true;
//            }
//            return isFine;
//        }

//        private void RefreshAxisButtonsForIndex(int selectedIndex)
//        {
//            try
//            {
//                if (_config?.TeachingPositions == null || selectedIndex < 0 || selectedIndex >= _config.TeachingPositions.Count)
//                {
//                    BuildAxisButtons(Enumerable.Empty<string>());
//                    return;
//                }

//                var tp = _config.TeachingPositions[selectedIndex];
//                var axisNames = (tp.AxisPositions != null) ? tp.AxisPositions.Keys.ToArray() : new string[0];
//                BuildAxisButtons(axisNames);
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine("RefreshAxisButtonsForIndex error: " + ex.Message);
//            }
//        }

//        private void BuildAxisButtons(IEnumerable<string> axisNames)
//        {
//            if (axisButtonPanel == null) 
//                return;

//            axisButtonPanel.SuspendLayout();
//            try
//            {
//                axisButtonPanel.Controls.Clear();
//                if (axisNames == null) return;

//                foreach (var name in axisNames)
//                {
//                    if (string.IsNullOrWhiteSpace(name)) continue;

//                    var btn = new QMC.Common.IndividualMenuButton();
//                    btn.Name = "btnAxis_" + name;
//                    btn.Text = name;
//                    // 동일 크기 적용
//                    if (btnMovePosition != null)
//                    {
//                        btn.Size = btnMovePosition.Size;
//                        btn.CustomBackColor = btnMovePosition.CustomBackColor;
//                        btn.CustomForeColor = btnMovePosition.CustomForeColor;
//                        btn.CustomFont = btnMovePosition.CustomFont;
//                        btn.Font = btnMovePosition.Font;
//                        btn.ImageSize = btnMovePosition.ImageSize;
//                    }
//                    btn.Margin = new Padding(0);
//                    btn.Tag = name; // 축 이름 저장
//                    btn.Click += OnAxisButtonClick;
//                    axisButtonPanel.Controls.Add(btn);
//                }
//            }
//            finally
//            {
//                axisButtonPanel.ResumeLayout();
//            }
//        }

//        private async void OnAxisButtonClick(object sender, EventArgs e)
//        {
//            try
//            {
//                int selIndex = GetSelectedIndex();
//                if (selIndex < 0)
//                {
//                    MessageBox.Show("선택된 Teaching Position이 없습니다.",
//                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    return;
//                }

//                var btn = sender as Control;
//                var axisName = btn != null ? (btn.Tag as string ?? btn.Text) : null;
//                if (string.IsNullOrEmpty(axisName)) return;

//                bool isFine = GetSelectedMoveModeIsFine();

//                double dTargetPos = 0.0;
//                if (_config?.TeachingPositions != null &&
//                    selIndex >= 0 && selIndex < _config.TeachingPositions.Count)
//                {
//                    var tp = _config.TeachingPositions[selIndex];
//                    if (tp.AxisPositions != null && tp.AxisPositions.ContainsKey(axisName))
//                    {
//                        dTargetPos = tp.AxisPositions[axisName];
//                    }
//                    else
//                    {
//                        MessageBox.Show($"선택된 Teaching Position에 축 '{axisName}' 정보가 없습니다.",
//                            "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                        return;
//                    }
//                }
//                else
//                {
//                    MessageBox.Show("선택된 Teaching Position 정보가 올바르지 않습니다.",
//                        "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                    return;
//                }

//                var mb = new MessageBoxYesNo();
//                mb.Title = "이동 확인";
//                mb.Message = "선택된 Teaching Position으로 이동하시겠습니까?";
//                var dr = mb.ShowDialog();
//                if (dr != DialogResult.Yes)
//                    return;

//                // 이동 실행 및 결과 처리
//                var task = _unit?.MoveAxisPositionOneAsync(axisName, dTargetPos, isFine);
//                if (task == null)
//                {
//                    MessageBox.Show($"축 '{axisName}' 을(를) 찾을 수 없어 이동을 시작하지 못했습니다.",
//                        "이동 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                    return;
//                }

//                // UI 피드백
//                var oldCursor = Cursor.Current;
//                if (btn != null) btn.Enabled = false;
//                Cursor.Current = Cursors.WaitCursor;

//                int rc;
//                try
//                {
//                    rc = await task; // UI 스레드 콘텍스트로 복귀
//                }
//                finally
//                {
//                    Cursor.Current = oldCursor;
//                    if (btn != null) btn.Enabled = true;
//                }

//                var mbResult = new MessageBoxOk();
//                if (rc == 0)
//                {
//                    mb.Title = "이동 확인";
//                    mb.Message = "이동 완료.";
//                    mb.ShowDialog();
//                    //MessageBox.Show($"[{axisName}] {dTargetPos} 위치로 이동 완료.",
//                    //    "이동 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                }
//                else
//                {
//                    mb.Title = "이동 확인";
//                    mb.Message = "이동 실패.";
//                    mb.ShowDialog();
//                    //MessageBox.Show($"[{axisName}] {dTargetPos} 위치로 이동 실패 (rc={rc}).",
//                    //    "이동 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.Write(ex);
//                //Debug.WriteLine("OnAxisButtonClick error: " + ex.Message);
//                //MessageBox.Show("축 이동 중 오류가 발생했습니다.\n" + ex.Message,
//                //    "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
//            }
//        }

//        #endregion
//    }

//    #region EventArgs Classes

//    public class PositionSelectedEventArgs : EventArgs
//    {
//        public int Index { get; set; }
//    }

//    public class SavePositionEventArgs : EventArgs
//    {
//        public int Index { get; set; }
//        public PropertyCollection Properties { get; set; }
//    }

//    public class MovePositionEventArgs : EventArgs
//    {
//        public int Index { get; set; }
//        public bool IsFine { get; set; }
//    }

//    public class CurrentPosEventArgs : EventArgs
//    {
//        public int Index { get; set; }
//    }

//    public class MoveAxisEventArgs : EventArgs
//    {
//        public int Index { get; set; }
//        public string AxisName { get; set; }
//        public bool IsFine { get; set; }
//    }

//    #endregion
//}