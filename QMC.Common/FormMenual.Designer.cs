using System;

namespace QMC.Common
{
    partial class FormMenual
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
            this.SuspendLayout();
            // 
            // FormManual
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1200, 675);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(889, 572);
            this.Name = "FormManual";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "FormManual";
            this.ResumeLayout(false);

        }
        
        /// <summary>
        /// 🔧 폼 크기 변경 이벤트 (디버깅용)
        /// </summary>
        private void FormConfig_Resize(object sender, System.EventArgs e)
        {
            Console.WriteLine($"📏 FormConfig 크기 변경됨: Size={this.Size}, ClientSize={this.ClientSize}");
            
            // TabControl이 존재하고 Dock이 Fill이 아닌 경우 크기 동기화
            if (ManualTabControl != null && ManualTabControl.Dock != System.Windows.Forms.DockStyle.Fill)
            {
                ManualTabControl.Size = this.ClientSize;
                Console.WriteLine($"   TabControl 크기 동기화: {ManualTabControl.Size}");
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