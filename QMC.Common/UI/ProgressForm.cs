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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
                timerCheckProcess.Tick -= TimerCheckProcess_Tick;
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
                timerCheckProcess.Tick -= TimerCheckProcess_Tick;
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
                            bComplete = true;
                        }
                        else
                        {
                            bComplete = false;
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

        public Func<Tuple<int, string>> CustomStatusProvider { get; set; }
        // 기존 컨트롤 재사용을 위한 원본 메시지 저장용(초기화 이후 1회만 설정)
        private string _baseContentMessage;
        private string _lastStatusText;
        private int _lastPercent;

        private void CacheBaseMessageOnce()
        {
            if (_baseContentMessage == null)
                _baseContentMessage = labelContent != null ? labelContent.Text : "";
        }

        // 기존 timerCheckProcess 와 별도로 진행률 갱신이 필요하면 디자이너에서 연결된 timerUpdate 활용
        // (progressBar / lblStatus 컨트롤이 없으므로 labelContent 를 재사용)
        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            if (CustomStatusProvider == null)
                return;

            CacheBaseMessageOnce();

            Tuple<int, string> tup = null;
            try
            {
                tup = CustomStatusProvider();
            }
            catch
            {
                // 공급자 실행 실패는 무시
            }

            if (tup == null)
                return;

            int percent = tup.Item1;
            string status = tup.Item2 ?? "";

            // 이전 값과 동일하면 불필요한 UI 갱신 최소화
            if (percent == _lastPercent && status == _lastStatusText)
                return;

            _lastPercent = percent;
            _lastStatusText = status;

            // 기존 labelContent 재사용
            // (원래 메시지 + 진행률 + 상태)
            if (labelContent != null)
            {
                // 퍼센트 범위 안전화
                if (percent < 0) percent = 0;
                if (percent > 100) percent = 100;

                labelContent.Text =
                    $"{_baseContentMessage}\r\n" +
                    $"Progress: {percent}%\r\n" +
                    $"{status}";
            }

            // 제목에도 퍼센트 반영(선택)
            if (labelTitle != null)
            {
                // 기존 제목에 퍼센트만 덧붙이기 (중복 방지 위해 괄호 영역만 대체)
                string t = labelTitle.Text;
                int idx = t.LastIndexOf(" (");
                if (idx >= 0)
                    t = t.Substring(0, idx);
                labelTitle.Text = $"{t} ({percent}%)";
            }
        }
    }
}
