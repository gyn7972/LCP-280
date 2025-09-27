using QMC.Common;
using QMC.Common.Sequence;
using QMC.Common.UI;
using QMC.Common.Unit;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace QMC.LCP_280.Process.Component
{
    public partial class ManualSequenceControl : UserControl
    {
        private BaseUnit m_ParentUnit;
        int SelectedIndex = -1;
        public BaseUnit ParentUnit 
        {
            get
            {
                return   m_ParentUnit;
            }
            set
            {
                m_ParentUnit = value;
                UpdateSeqList();
            }
        }

        private void UpdateSeqList()
        {
            if (m_ParentUnit == null) return;
            this._lstSteps.Items.Clear();
            SelectedIndex = -1; 
            foreach (var v in m_ParentUnit.SequencePlayers)
            {
                int Index = this._lstSteps.Items.Add(v.Method.Name);
                if(m_ParentUnit.CurrentFunc != null)
                {
                    if (m_ParentUnit.CurrentFunc.Method.Name == v.Method.Name)
                    {
                        SelectedIndex = Index;
                    }
                }
                
            }
            this._lstSteps.SelectedIndex = SelectedIndex;
        }

        public ManualSequenceControl()
        {
            InitializeComponent();
        }

        private void _btnNext_Click(object sender, EventArgs e)
        {

            if (m_ParentUnit == null) return;
            this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
            this._lstSteps.SelectedIndex = this.SelectedIndex;
            if (this._lstSteps.SelectedIndex < 0)
            {
                this._lstSteps.SelectedIndex = 0;
            }
            if (this._lstSteps.SelectedIndex < m_ParentUnit.SequencePlayers.Count)
            {
                var func = m_ParentUnit.SequencePlayers[this._lstSteps.SelectedIndex];
                if (func != null)
                {
                    
                    Task<int> t = m_ParentUnit.RunManualFunction(func);

                    UpdateSeqList();
                    ProgressForm form = new ProgressForm("Manual Running", func.Method.Name, t, m_ParentUnit);

                    if (t != null)
                    {
                        try
                        {
                            form.ShowDialog();
                            if(form.DialogResult == DialogResult.Cancel)
                            {
                                m_ParentUnit.CancelSequence();
                            }
                            if (t.Status == TaskStatus.RanToCompletion && t.Result == 0)
                            {
                                this.SelectedIndex++;
                                this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
                                this._lstSteps.SelectedIndex = this.SelectedIndex;
                            }
                            else if (t.IsFaulted)
                            {
                                // 예외 메시지 표시
                                MessageBox.Show(t.Exception?.GetBaseException().Message, "Manual Run Error");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex);
                        }
                    }
                }

                //form.ShowDialog();
                //if(t.Result == 0)
                //{
                //    this.SelectedIndex++;
                //    this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
                //    this._lstSteps.SelectedIndex = this.SelectedIndex;
                //}
            }
        }


        private void _btnRun_Click(object sender, EventArgs e)
        {
            if (m_ParentUnit == null) return;
            if (this._lstSteps.SelectedIndex < 0)
            {
                this._lstSteps.SelectedIndex = 0;
            }
            if (this._lstSteps.SelectedIndex < m_ParentUnit.SequencePlayers.Count)
            {
                var func = m_ParentUnit.SequencePlayers[this._lstSteps.SelectedIndex];
                
                Task<int> t = m_ParentUnit.RunManualFunction(func);
                SelectedIndex = this._lstSteps.SelectedIndex;
                UpdateSeqList();
                ProgressForm form = new ProgressForm("Manual Running", func.Method.Name, t, m_ParentUnit);
                form.ShowDialog();
                if(form.DialogResult == DialogResult.Cancel)
                {   
                    m_ParentUnit.CancelSequence();
                }

            }
        }


        private async void _btnPlay_Click(object sender, EventArgs e)
        {
            if (m_ParentUnit == null) 
                return;

            var ask = new MessageBoxYesNo();
            if (ask.ShowDialog("확인", "시컨스를 진행하시겠습니까?") != DialogResult.Yes)
                return;

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스가 초기화되지 않았습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var unitName = m_ParentUnit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    MessageBox.Show("UnitName 이 비어있습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 이미 실행 중인지 간단 체크 (RunStatus 사용 가능 시)
                if (m_ParentUnit.RunUnitStatus == BaseUnit.UnitStatus.Running)
                {
                    MessageBox.Show($"Unit '{unitName}' 는 이미 실행 중입니다.", "정보",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var btn = sender as Button;
                bool restore = false;
                if (btn != null && btn.Enabled)
                {
                    btn.Enabled = false;
                    restore = true;
                }
                Cursor prev = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                bool ok = await eq.StartUnitAsync(unitName);
                if (!ok)
                {
                    MessageBox.Show($"Unit '{unitName}' 시작 실패.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    // 필요 시 목록/상태 갱신
                    UpdateSeqList();
                }

                Cursor.Current = prev;
                if (restore) 
                    btn.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private async void _btnStop_Click(object sender, EventArgs e)
        {
            if (m_ParentUnit == null) 
                return;

            try
            {
                var eq = Equipment.Instance;
                if (eq == null)
                {
                    MessageBox.Show("Equipment 인스턴스가 없습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var unitName = m_ParentUnit.UnitName;
                if (string.IsNullOrEmpty(unitName))
                {
                    MessageBox.Show("UnitName 이 비어 있습니다.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 이미 정지 상태이면 반환
                if (m_ParentUnit.RunUnitStatus == BaseUnit.UnitStatus.Stopping ||
                    m_ParentUnit.RunUnitStatus == BaseUnit.UnitStatus.Stopped ||
                    m_ParentUnit.RunUnitStatus == BaseUnit.UnitStatus.CycleStop)
                {
                    MessageBox.Show($"Unit '{unitName}' 는 이미 정지 상태입니다.", "정보",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var btn = sender as Button;
                bool restore = false;
                if (btn != null && btn.Enabled)
                {
                    btn.Enabled = false;
                    restore = true;
                }
                var prevCursor = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;

                bool ok = await eq.StopUnitAsync(unitName);
                if (!ok)
                {
                    MessageBox.Show($"Unit '{unitName}' 정지 실패.", "오류",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    UpdateSeqList();
                }

                Cursor.Current = prevCursor;
                if (restore) btn.Enabled = true;
            }
            catch (Exception ex)
            {
                Log.Write(ex);
                MessageBox.Show(ex.Message, "정지 처리 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
