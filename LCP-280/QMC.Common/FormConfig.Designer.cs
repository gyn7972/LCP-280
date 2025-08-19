using System;

namespace QMC.Common
{
    partial class FormConfig
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
            
            this.Text = "FormConfig";
            this.BackColor = System.Drawing.Color.White;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            
            // 🔧 크기 변경 이벤트 추가 (디버깅용)
            this.Resize += FormConfig_Resize;
        }
        
        /// <summary>
        /// 🔧 폼 크기 변경 이벤트 (디버깅용)
        /// </summary>
        private void FormConfig_Resize(object sender, System.EventArgs e)
        {
            Console.WriteLine($"📏 FormConfig 크기 변경됨: Size={this.Size}, ClientSize={this.ClientSize}");
            
            // TabControl이 존재하고 Dock이 Fill이 아닌 경우 크기 동기화
            if (configTabControl != null && configTabControl.Dock != System.Windows.Forms.DockStyle.Fill)
            {
                configTabControl.Size = this.ClientSize;
                Console.WriteLine($"   TabControl 크기 동기화: {configTabControl.Size}");
            }
        }

        #endregion
    }
}