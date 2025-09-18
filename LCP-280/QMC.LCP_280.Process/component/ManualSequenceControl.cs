using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using QMC.Common.Sequence;
using QMC.LCP_280.Process.Unit;
using System.Collections.Generic;
using System.Linq;
using QMC.Common.Unit;
using QMC.Common.UI;
using System.Threading.Tasks;

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
            this.SelectedIndex++;
            this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
            this._lstSteps.SelectedIndex = this.SelectedIndex;
            if (this._lstSteps.SelectedIndex < 0)
            {
                this._lstSteps.SelectedIndex = 0;
            }
            if (this._lstSteps.SelectedIndex < m_ParentUnit.SequencePlayers.Count)
            {
                var func = m_ParentUnit.SequencePlayers[this._lstSteps.SelectedIndex];
                Task<int> t = m_ParentUnit.RunManualFunction(func);
                
                UpdateSeqList();
                ProgressForm form = new ProgressForm("Manual Running", func.Method.Name, t, m_ParentUnit);
                form.ShowDialog();
                if(t.Result == 0)
                {
                    this.SelectedIndex++;
                    this.SelectedIndex = (this.SelectedIndex % this._lstSteps.Items.Count);
                    this._lstSteps.SelectedIndex = this.SelectedIndex;
                }
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
            }
        }
    }
}
