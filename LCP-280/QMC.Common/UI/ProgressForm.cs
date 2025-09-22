using QMC.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.Common.UI
{
    public partial class ProgressForm : Form
    {
        private Task<int> m_AsyncResult;
        private List<Task<int>> m_listAsyncResults;
        private object m_obj;

        public delegate void StopProcessEvent(object target);
        public event StopProcessEvent StopProcess;
        public ProgressForm() : this("", "", (Task<int>)null)
        {

        }

        public ProgressForm(string strTitle, string strMessage, List<Task<int>> list)
        {
            InitializeComponent();
            m_AsyncResult = null;
            m_listAsyncResults = list;
            labelTitle.Text = strTitle;
            labelContent.Text = strMessage;

            if (m_listAsyncResults != null)
            {
                timerCheckProcess.Interval = 100;
                timerCheckProcess.Tick += TimerCheckProcess_Tick;
                timerCheckProcess.Start();
            }
        }

        public ProgressForm(string strTitle, string strMessage, Task<int> param) 
            : this(strTitle, strMessage, param, null)
        {
        }

        public ProgressForm(string strTitle, string strMessage, Task<int> param, object target)
        {
            
            InitializeComponent();

            m_AsyncResult = param;
            labelTitle.Text = strTitle;
            labelContent.Text = strMessage;
            m_listAsyncResults = null;
            StartPosition = FormStartPosition.CenterParent;
            if (m_AsyncResult != null)
            {
                timerCheckProcess.Interval = 100;
                timerCheckProcess.Tick += TimerCheckProcess_Tick;
                timerCheckProcess.Start();
            }
            m_obj = target;

        }

        private void TimerCheckProcess_Tick(object sender, EventArgs e)
        {
            timerCheckProcess.Stop();
            {
                if (m_AsyncResult != null)
                {
                    if (m_AsyncResult.Status == TaskStatus.RanToCompletion)
                    {
                        Console.WriteLine("Progress End");
                        int nResult = 0;
                        nResult = m_AsyncResult.Result;
                        if (nResult == 0)
                        {
                            DialogResult = DialogResult.OK;
                        }
                        else if (nResult < 0)
                        {
                            DialogResult = DialogResult.Cancel;
                            //MessageBox.Show("오류");
                        }
                        else
                        {
                            DialogResult = DialogResult.Cancel;
                        }
                        return;
                    }
                }
                else if(m_listAsyncResults != null)
                {
                    bool bComplete = false;
                    foreach(Task<int> result in m_listAsyncResults)
                    {
                        if(result.IsCompleted)
                        {
                            bComplete &= true;
                        }
                        else
                        {
                            bComplete &= false;
                        }
                    }
                }              
                else
                {
                    DialogResult = DialogResult.OK;
                    return;
                }
            }
            timerCheckProcess.Start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if(StopProcess != null)
                StopProcess(m_obj);
            DialogResult = DialogResult.Cancel;
            //m_AsyncResult.Dispose();
        }
    }
}
