using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.CustomControl;
using QMC.LCP_280.Process.Component;

namespace QMC.Common
{
    partial class TopContentsIOStatusControl
    {
        private IContainer components = null;

        private DeleteInnerBorderTableLayoutPanel tableLayoutContentsIOStatusPanel;

        private IndividualMenuButton btnBuzzerInput;   // 왼-상: BUZZER 인풋(읽기전용)
        private IndividualMenuButton btnBuzzerSound;   // 왼-중: BUZZER 소리 토글(클릭)
        private IndividualMenuButton btnLampRed;       // 오른-상: RED
        private IndividualMenuButton btnLampYellow;    // 오른-중: YEL
        private IndividualMenuButton btnLampGreen;     // 오른-하: GRN

        private Timer timerBlink;                      // BUZZER 인풋 깜빡임

        /// <summary>디자이너 리소스 정리</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                if (timerBlink != null)
                {
                    timerBlink.Stop();
                    timerBlink.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutContentsIOStatusPanel = new QMC.Common.CustomControl.DeleteInnerBorderTableLayoutPanel();
            this.timerBlink = new System.Windows.Forms.Timer(this.components);
            this.btnBuzzerInput = new QMC.Common.IndividualMenuButton();
            this.btnBuzzerSound = new QMC.Common.IndividualMenuButton();
            this.btnLampRed = new QMC.Common.IndividualMenuButton();
            this.btnLampYellow = new QMC.Common.IndividualMenuButton();
            this.btnLampGreen = new QMC.Common.IndividualMenuButton();
            this.tableLayoutContentsIOStatusPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutContentsIOStatusPanel
            // 
            this.tableLayoutContentsIOStatusPanel.ColumnCount = 2;
            this.tableLayoutContentsIOStatusPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutContentsIOStatusPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutContentsIOStatusPanel.Controls.Add(this.btnBuzzerInput, 0, 0);
            this.tableLayoutContentsIOStatusPanel.Controls.Add(this.btnBuzzerSound, 0, 1);
            this.tableLayoutContentsIOStatusPanel.Controls.Add(this.btnLampRed, 1, 0);
            this.tableLayoutContentsIOStatusPanel.Controls.Add(this.btnLampYellow, 1, 1);
            this.tableLayoutContentsIOStatusPanel.Controls.Add(this.btnLampGreen, 1, 2);
            this.tableLayoutContentsIOStatusPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutContentsIOStatusPanel.InnerBorderColor = System.Drawing.Color.White;
            this.tableLayoutContentsIOStatusPanel.InnerBorderWidth = 2;
            this.tableLayoutContentsIOStatusPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutContentsIOStatusPanel.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutContentsIOStatusPanel.Name = "tableLayoutContentsIOStatusPanel";
            this.tableLayoutContentsIOStatusPanel.OuterBorderColor = System.Drawing.Color.Black;
            this.tableLayoutContentsIOStatusPanel.OuterBorderWidth = 1;
            this.tableLayoutContentsIOStatusPanel.RowCount = 3;
            this.tableLayoutContentsIOStatusPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutContentsIOStatusPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutContentsIOStatusPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutContentsIOStatusPanel.ShowInnerBorder = false;
            this.tableLayoutContentsIOStatusPanel.Size = new System.Drawing.Size(150, 102);
            this.tableLayoutContentsIOStatusPanel.TabIndex = 0;
            // 
            // timerBlink
            // 
            this.timerBlink.Interval = 500;
            this.timerBlink.Tick += new System.EventHandler(this.timerBlink_Tick);
            // 
            // btnBuzzerInput
            // 
            this.btnBuzzerInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnBuzzerInput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnBuzzerInput.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnBuzzerInput.CustomFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnBuzzerInput.CustomForeColor = System.Drawing.Color.Black;
            this.btnBuzzerInput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBuzzerInput.Enabled = false;
            this.btnBuzzerInput.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnBuzzerInput.ForeColor = System.Drawing.Color.Black;
            this.btnBuzzerInput.Image = global::QMC.Common.Properties.Resources.ico_speaker_gray_14;
            this.btnBuzzerInput.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBuzzerInput.ImageSize = new System.Drawing.Size(45, 45);
            this.btnBuzzerInput.Location = new System.Drawing.Point(1, 1);
            this.btnBuzzerInput.Margin = new System.Windows.Forms.Padding(1);
            this.btnBuzzerInput.Name = "btnBuzzerInput";
            this.btnBuzzerInput.Size = new System.Drawing.Size(73, 32);
            this.btnBuzzerInput.TabIndex = 0;
            this.btnBuzzerInput.TabStop = false;
            this.btnBuzzerInput.Text = "IDLE";
            this.btnBuzzerInput.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBuzzerInput.UseVisualStyleBackColor = false;
            // 
            // btnBuzzerSound
            // 
            this.btnBuzzerSound.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnBuzzerSound.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnBuzzerSound.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnBuzzerSound.CustomFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnBuzzerSound.CustomForeColor = System.Drawing.Color.Black;
            this.btnBuzzerSound.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnBuzzerSound.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnBuzzerSound.ForeColor = System.Drawing.Color.Black;
            this.btnBuzzerSound.Image = global::QMC.Common.Properties.Resources.ico_speaker_gray_14;
            this.btnBuzzerSound.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnBuzzerSound.ImageSize = new System.Drawing.Size(45, 45);
            this.btnBuzzerSound.Location = new System.Drawing.Point(1, 35);
            this.btnBuzzerSound.Margin = new System.Windows.Forms.Padding(1);
            this.btnBuzzerSound.Name = "btnBuzzerSound";
            this.btnBuzzerSound.Size = new System.Drawing.Size(73, 32);
            this.btnBuzzerSound.TabIndex = 1;
            this.btnBuzzerSound.TabStop = false;
            this.btnBuzzerSound.Text = "BUZZER";
            this.btnBuzzerSound.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnBuzzerSound.UseVisualStyleBackColor = false;
            this.btnBuzzerSound.Click += new System.EventHandler(this.btnBuzzerSound_Click);
            // 
            // btnLampRed
            // 
            this.btnLampRed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLampRed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLampRed.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLampRed.CustomFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnLampRed.CustomForeColor = System.Drawing.Color.Black;
            this.btnLampRed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLampRed.Enabled = false;
            this.btnLampRed.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnLampRed.ForeColor = System.Drawing.Color.Black;
            this.btnLampRed.Image = global::QMC.Common.Properties.Resources.ico_circle_off_14;
            this.btnLampRed.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLampRed.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLampRed.Location = new System.Drawing.Point(76, 1);
            this.btnLampRed.Margin = new System.Windows.Forms.Padding(1);
            this.btnLampRed.Name = "btnLampRed";
            this.btnLampRed.Size = new System.Drawing.Size(73, 32);
            this.btnLampRed.TabIndex = 2;
            this.btnLampRed.TabStop = false;
            this.btnLampRed.Text = "OFF";
            this.btnLampRed.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLampRed.UseVisualStyleBackColor = false;
            // 
            // btnLampYellow
            // 
            this.btnLampYellow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLampYellow.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLampYellow.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLampYellow.CustomFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnLampYellow.CustomForeColor = System.Drawing.Color.Black;
            this.btnLampYellow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLampYellow.Enabled = false;
            this.btnLampYellow.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnLampYellow.ForeColor = System.Drawing.Color.Black;
            this.btnLampYellow.Image = global::QMC.Common.Properties.Resources.ico_circle_off_14;
            this.btnLampYellow.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLampYellow.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLampYellow.Location = new System.Drawing.Point(76, 35);
            this.btnLampYellow.Margin = new System.Windows.Forms.Padding(1);
            this.btnLampYellow.Name = "btnLampYellow";
            this.btnLampYellow.Size = new System.Drawing.Size(73, 32);
            this.btnLampYellow.TabIndex = 3;
            this.btnLampYellow.TabStop = false;
            this.btnLampYellow.Text = "OFF";
            this.btnLampYellow.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLampYellow.UseVisualStyleBackColor = false;
            // 
            // btnLampGreen
            // 
            this.btnLampGreen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLampGreen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLampGreen.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLampGreen.CustomFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnLampGreen.CustomForeColor = System.Drawing.Color.Black;
            this.btnLampGreen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLampGreen.Enabled = false;
            this.btnLampGreen.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.btnLampGreen.ForeColor = System.Drawing.Color.Black;
            this.btnLampGreen.Image = global::QMC.Common.Properties.Resources.ico_circle_off_14;
            this.btnLampGreen.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLampGreen.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLampGreen.Location = new System.Drawing.Point(76, 69);
            this.btnLampGreen.Margin = new System.Windows.Forms.Padding(1);
            this.btnLampGreen.Name = "btnLampGreen";
            this.btnLampGreen.Size = new System.Drawing.Size(73, 32);
            this.btnLampGreen.TabIndex = 4;
            this.btnLampGreen.TabStop = false;
            this.btnLampGreen.Text = "OFF";
            this.btnLampGreen.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnLampGreen.UseVisualStyleBackColor = false;
            // 
            // IOStatusControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutContentsIOStatusPanel);
            this.Name = "IOStatusControl";
            this.Size = new System.Drawing.Size(150, 102);
            this.tableLayoutContentsIOStatusPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
