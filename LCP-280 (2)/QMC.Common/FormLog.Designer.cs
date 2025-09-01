using System;

namespace QMC.Common
{
    partial class FormLog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            
            // 🔧 고정 크기 대신 최소 크기만 설정하고, 실제 크기는 SetPanelSize에서 동적으로 설정
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Size = new System.Drawing.Size(800, 450); // 기본 크기 (SetPanelSize 호출 전까지 임시)
            this.ClientSize = new System.Drawing.Size(800, 450);
            
            this.Text = "FormLog";
            this.BackColor = System.Drawing.Color.White;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            
            // 🔧 크기 변경 이벤트 추가 (디버깅용)
            this.Resize += FormLog_Resize;
        }
        
        /// <summary>
        /// 🔧 폼 크기 변경 이벤트 (디버깅용)
        /// </summary>
        private void FormLog_Resize(object sender, System.EventArgs e)
        {
            Console.WriteLine($"📏 FormLog 크기 변경됨: Size={this.Size}, ClientSize={this.ClientSize}");
            
            // TabControl이 존재하고 Dock이 Fill이 아닌 경우 크기 동기화
            if (logTabControl != null && logTabControl.Dock != System.Windows.Forms.DockStyle.Fill)
            {
                logTabControl.Size = this.ClientSize;
                Console.WriteLine($"   TabControl 크기 동기화: {logTabControl.Size}");
            }
            
            // 테두리 다시 그리기
            this.Invalidate();
        }

        #endregion
        
        #region Border Theme Properties

        /// <summary>
        /// 디자이너에서 테두리 설정을 위한 프로퍼티들
        /// </summary>
        [System.ComponentModel.Category("Border")]
        [System.ComponentModel.Description("폼 테두리 색상")]
        [System.ComponentModel.DefaultValue(typeof(System.Drawing.Color), "Black")]
        public System.Drawing.Color FormBorderColor
        {
            get => _formBorderColor;
            set
            {
                _formBorderColor = value;
                this.Invalidate();
            }
        }
        
        [System.ComponentModel.Category("Border")]
        [System.ComponentModel.Description("폼 테두리 두께 (픽셀)")]
        [System.ComponentModel.DefaultValue(2)]
        public int FormBorderWidth
        {
            get => _formBorderWidth;
            set
            {
                _formBorderWidth = System.Math.Max(1, value);
                this.Invalidate();
            }
        }
        
        private System.Drawing.Color _formBorderColor = System.Drawing.Color.Black;
        private int _formBorderWidth = 2;

        #endregion
    }
}