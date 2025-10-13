using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    /// <summary>
    /// 대화상자 (Ok)
    /// </summary>
    public partial class MessageBoxDieStatusSelect : Form
    {
        public int RotateStatus { get; set; } = 0;

        public enum MessageStatus
        {
            error,
            warning,
            notification
        }

        /// <summary>
        /// 제목
        /// </summary>
        public string Title
        {
            get { return this.lblTitle.Text; }
            set { this.lblTitle.Text = value; }
        }
        /// <summary>
        /// 본문
        /// </summary>
        public string Message
        {
            get { return this.txtMessage.Text; }
            set { this.txtMessage.Text = value; }
        }
        /// <summary>
        /// 기본값 Color.Control
        /// </summary>
        public Color TitleBgColor
        {
            get { return this.panel1.BackColor; }
            set { this.panel1.BackColor = value; }
        }
        /// <summary>
        /// 기본값 Color.ControlText
        /// </summary>
        public Color TitleFgColor
        {
            get { return this.lblTitle.ForeColor; }
            set { this.lblTitle.ForeColor = value; }
        }

        int closedSecs;
        int secs;

        private bool isMouseDown;
        private Point mouseDownLocation;

        /// <summary>
        /// 생성자
        /// </summary>
        public MessageBoxDieStatusSelect()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            //this.TopMost = true;

            lblTitle.MouseDown += (o, e) => { if (e.Button == MouseButtons.Left) { isMouseDown = true; mouseDownLocation = e.Location; } };
            lblTitle.MouseMove += (o, e) => { if (isMouseDown) Location = new Point(Location.X + (e.X - mouseDownLocation.X), Location.Y + (e.Y - mouseDownLocation.Y)); };
            lblTitle.MouseUp += (o, e) => { if (e.Button == MouseButtons.Left) { isMouseDown = false; mouseDownLocation = e.Location; } };
        }

        /// <summary>
        /// Drop Shadow (그림자) 효과
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        /// <summary>
        /// 대화상자 출력 (Modal)
        /// </summary>
        /// <returns></returns>
        public new DialogResult ShowDialog()
        {
            return base.ShowDialog();
        }

        /// <summary>
        /// 대화상자 출력 (Modal)
        /// </summary>
        /// <param name="title">제목</param>
        /// <param name="message">본문</param>
        /// <param name="closedSecs">자동 닫기 시간 (초)</param>
        /// <returns></returns>
        public DialogResult ShowDialog(string title, string message, int closedSecs = 0)
        {
            this.Title = title;
            this.Message = message;
            this.closedSecs = closedSecs;

            //if (this.closedSecs > 0)
            //{
            //    this.btn_Ok.Text = $"&Ok ({closedSecs})";
            //    this.secs = 0;
            //    var timer = new Timer();
            //    timer.Tick += Timer_Tick;
            //    timer.Interval = 1000;
            //    timer.Enabled = true;
            //}
            //else
            //{
            //    this.btn_Ok.Text = $"&Ok";
            //}

            return base.ShowDialog();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.secs++;
            this.btn_Ok.Text = $"&Ok ({this.closedSecs - this.secs})";
            if (this.secs >= this.closedSecs)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btn_Skip_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_Empty_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Empty;
            this.Close();
        }

        private void btn_Loading_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Loading;
            this.Close();
        }

        private void btn_Loaded_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Loaded;
            this.Close();
        }

        private void btn_Aligning_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Aligning;
            this.Close();
        }

        private void btn_Aligned_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Aligned;
            this.Close();
        }

        private void btn_Probing_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Probing;
            this.Close();
        }

        private void btn_Probed_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Probed;
            this.Close();
        }

        private void btn_Unloading_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Unloading;
            this.Close();
        }

        private void btn_Outputting_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Outputting;
            this.Close();
        }

        private void btn_Completed_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Completed;
            this.Close();
        }

        private void btn_Error_Click(object sender, EventArgs e)
        {
            RotateStatus = (int)Rotary.RotarySocketState.Error;
            this.Close();
        }
    }
}
