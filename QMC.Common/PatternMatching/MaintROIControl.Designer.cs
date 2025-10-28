namespace QMC.Common
{
    partial class MaintROIControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 디자이너 생성 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.expandableGroupBox1 = new QMC.Common.SimpleGroupBox();
            this.baseButtonRollBack = new System.Windows.Forms.Button();
            this.paramTextControlSizeToke = new QMC.Common.ParamTextControl();
            this.paramTextControlMoveToke = new QMC.Common.ParamTextControl();
            this.baseButtonSave = new System.Windows.Forms.Button();
            this.paramDualTextControlSize = new QMC.Common.ParamDualTextControl();
            this.paramDualTextControlCenter = new QMC.Common.ParamDualTextControl();
            this.baseButtonFullSize = new System.Windows.Forms.Button();
            this.baseButtonYSizeDown = new System.Windows.Forms.Button();
            this.baseButtonYSizeUp = new System.Windows.Forms.Button();
            this.baseButtonXSizeDown = new System.Windows.Forms.Button();
            this.baseButtonXSizeUp = new System.Windows.Forms.Button();
            this.baseButtonLeft = new System.Windows.Forms.Button();
            this.baseButtonDown = new System.Windows.Forms.Button();
            this.baseButtonRight = new System.Windows.Forms.Button();
            this.baseButtonCenter = new System.Windows.Forms.Button();
            this.baseButtonUp = new System.Windows.Forms.Button();
            this.baseToggleButtonInspect = new QMC.Common.SimpleToggleButton();
            this.baseToggleButtonTrain = new QMC.Common.SimpleToggleButton();
            this.expandableGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // expandableGroupBox1
            // 
            this.expandableGroupBox1.BackColor = System.Drawing.Color.Transparent;
            this.expandableGroupBox1.Controls.Add(this.baseButtonRollBack);
            this.expandableGroupBox1.Controls.Add(this.paramTextControlSizeToke);
            this.expandableGroupBox1.Controls.Add(this.paramTextControlMoveToke);
            this.expandableGroupBox1.Controls.Add(this.baseButtonSave);
            this.expandableGroupBox1.Controls.Add(this.paramDualTextControlSize);
            this.expandableGroupBox1.Controls.Add(this.paramDualTextControlCenter);
            this.expandableGroupBox1.Controls.Add(this.baseButtonFullSize);
            this.expandableGroupBox1.Controls.Add(this.baseButtonYSizeDown);
            this.expandableGroupBox1.Controls.Add(this.baseButtonYSizeUp);
            this.expandableGroupBox1.Controls.Add(this.baseButtonXSizeDown);
            this.expandableGroupBox1.Controls.Add(this.baseButtonXSizeUp);
            this.expandableGroupBox1.Controls.Add(this.baseButtonLeft);
            this.expandableGroupBox1.Controls.Add(this.baseButtonDown);
            this.expandableGroupBox1.Controls.Add(this.baseButtonRight);
            this.expandableGroupBox1.Controls.Add(this.baseButtonCenter);
            this.expandableGroupBox1.Controls.Add(this.baseButtonUp);
            this.expandableGroupBox1.Controls.Add(this.baseToggleButtonInspect);
            this.expandableGroupBox1.Controls.Add(this.baseToggleButtonTrain);
            this.expandableGroupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.expandableGroupBox1.ForeColor = System.Drawing.Color.Black;
            this.expandableGroupBox1.Location = new System.Drawing.Point(0, 0);
            this.expandableGroupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.expandableGroupBox1.Name = "expandableGroupBox1";
            this.expandableGroupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.expandableGroupBox1.Radious = 0;
            this.expandableGroupBox1.Size = new System.Drawing.Size(391, 519);
            this.expandableGroupBox1.TabIndex = 0;
            this.expandableGroupBox1.TabStop = false;
            this.expandableGroupBox1.Text = "ROI";
            this.expandableGroupBox1.TitleBackColor = System.Drawing.Color.SteelBlue;
            this.expandableGroupBox1.TitleFontSize = 9F;
            this.expandableGroupBox1.UseExpand = false;
            // 
            // baseButtonRollBack
            // 
            this.baseButtonRollBack.Location = new System.Drawing.Point(183, 471);
            this.baseButtonRollBack.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonRollBack.Name = "baseButtonRollBack";
            this.baseButtonRollBack.Size = new System.Drawing.Size(184, 39);
            this.baseButtonRollBack.TabIndex = 19;
            this.baseButtonRollBack.Text = "Roll Back";
            this.baseButtonRollBack.UseVisualStyleBackColor = true;
            this.baseButtonRollBack.Click += new System.EventHandler(this.baseButtonRollBack_Click);
            // 
            // paramTextControlSizeToke
            // 
            this.paramTextControlSizeToke.Location = new System.Drawing.Point(7, 314);
            this.paramTextControlSizeToke.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.paramTextControlSizeToke.Name = "paramTextControlSizeToke";
            this.paramTextControlSizeToke.Size = new System.Drawing.Size(360, 45);
            this.paramTextControlSizeToke.TabIndex = 18;
            this.paramTextControlSizeToke.TitleRatio = 50;
            // 
            // paramTextControlMoveToke
            // 
            this.paramTextControlMoveToke.Location = new System.Drawing.Point(9, 261);
            this.paramTextControlMoveToke.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.paramTextControlMoveToke.Name = "paramTextControlMoveToke";
            this.paramTextControlMoveToke.Size = new System.Drawing.Size(358, 45);
            this.paramTextControlMoveToke.TabIndex = 17;
            this.paramTextControlMoveToke.TitleRatio = 50;
            // 
            // baseButtonSave
            // 
            this.baseButtonSave.Location = new System.Drawing.Point(7, 471);
            this.baseButtonSave.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonSave.Name = "baseButtonSave";
            this.baseButtonSave.Size = new System.Drawing.Size(161, 39);
            this.baseButtonSave.TabIndex = 15;
            this.baseButtonSave.Text = "Save";
            this.baseButtonSave.UseVisualStyleBackColor = true;
            this.baseButtonSave.Click += new System.EventHandler(this.baseButtonSave_Click);
            // 
            // paramDualTextControlSize
            // 
            this.paramDualTextControlSize.Location = new System.Drawing.Point(7, 419);
            this.paramDualTextControlSize.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.paramDualTextControlSize.Name = "paramDualTextControlSize";
            this.paramDualTextControlSize.Size = new System.Drawing.Size(360, 45);
            this.paramDualTextControlSize.TabIndex = 14;
            this.paramDualTextControlSize.TitleRatio = 50;
            // 
            // paramDualTextControlCenter
            // 
            this.paramDualTextControlCenter.Location = new System.Drawing.Point(7, 366);
            this.paramDualTextControlCenter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.paramDualTextControlCenter.Name = "paramDualTextControlCenter";
            this.paramDualTextControlCenter.Size = new System.Drawing.Size(360, 45);
            this.paramDualTextControlCenter.TabIndex = 13;
            this.paramDualTextControlCenter.TitleRatio = 50;
            // 
            // baseButtonFullSize
            // 
            this.baseButtonFullSize.Location = new System.Drawing.Point(183, 215);
            this.baseButtonFullSize.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonFullSize.Name = "baseButtonFullSize";
            this.baseButtonFullSize.Size = new System.Drawing.Size(184, 39);
            this.baseButtonFullSize.TabIndex = 11;
            this.baseButtonFullSize.Text = "Full Size";
            this.baseButtonFullSize.UseVisualStyleBackColor = true;
            this.baseButtonFullSize.Click += new System.EventHandler(this.baseButtonSize_Click);
            // 
            // baseButtonYSizeDown
            // 
            this.baseButtonYSizeDown.Location = new System.Drawing.Point(281, 145);
            this.baseButtonYSizeDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonYSizeDown.Name = "baseButtonYSizeDown";
            this.baseButtonYSizeDown.Size = new System.Drawing.Size(86, 62);
            this.baseButtonYSizeDown.TabIndex = 10;
            this.baseButtonYSizeDown.Text = "Y-";
            this.baseButtonYSizeDown.UseVisualStyleBackColor = true;
            this.baseButtonYSizeDown.Click += new System.EventHandler(this.baseButtonSize_Click);
            // 
            // baseButtonYSizeUp
            // 
            this.baseButtonYSizeUp.Location = new System.Drawing.Point(183, 145);
            this.baseButtonYSizeUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonYSizeUp.Name = "baseButtonYSizeUp";
            this.baseButtonYSizeUp.Size = new System.Drawing.Size(86, 62);
            this.baseButtonYSizeUp.TabIndex = 9;
            this.baseButtonYSizeUp.Text = "Y+";
            this.baseButtonYSizeUp.UseVisualStyleBackColor = true;
            this.baseButtonYSizeUp.Click += new System.EventHandler(this.baseButtonSize_Click);
            // 
            // baseButtonXSizeDown
            // 
            this.baseButtonXSizeDown.Location = new System.Drawing.Point(281, 74);
            this.baseButtonXSizeDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonXSizeDown.Name = "baseButtonXSizeDown";
            this.baseButtonXSizeDown.Size = new System.Drawing.Size(86, 62);
            this.baseButtonXSizeDown.TabIndex = 8;
            this.baseButtonXSizeDown.Text = "X-";
            this.baseButtonXSizeDown.UseVisualStyleBackColor = true;
            this.baseButtonXSizeDown.Click += new System.EventHandler(this.baseButtonSize_Click);
            // 
            // baseButtonXSizeUp
            // 
            this.baseButtonXSizeUp.Location = new System.Drawing.Point(183, 71);
            this.baseButtonXSizeUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonXSizeUp.Name = "baseButtonXSizeUp";
            this.baseButtonXSizeUp.Size = new System.Drawing.Size(86, 62);
            this.baseButtonXSizeUp.TabIndex = 7;
            this.baseButtonXSizeUp.Text = "X+";
            this.baseButtonXSizeUp.UseVisualStyleBackColor = true;
            this.baseButtonXSizeUp.Click += new System.EventHandler(this.baseButtonSize_Click);
            // 
            // baseButtonLeft
            // 
            this.baseButtonLeft.Location = new System.Drawing.Point(8, 134);
            this.baseButtonLeft.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonLeft.Name = "baseButtonLeft";
            this.baseButtonLeft.Size = new System.Drawing.Size(46, 50);
            this.baseButtonLeft.TabIndex = 6;
            this.baseButtonLeft.Text = "◀";
            this.baseButtonLeft.UseVisualStyleBackColor = true;
            this.baseButtonLeft.Click += new System.EventHandler(this.baseButtonLocation_Click);
            // 
            // baseButtonDown
            // 
            this.baseButtonDown.Location = new System.Drawing.Point(64, 195);
            this.baseButtonDown.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonDown.Name = "baseButtonDown";
            this.baseButtonDown.Size = new System.Drawing.Size(46, 50);
            this.baseButtonDown.TabIndex = 5;
            this.baseButtonDown.Text = "▼";
            this.baseButtonDown.UseVisualStyleBackColor = true;
            this.baseButtonDown.Click += new System.EventHandler(this.baseButtonLocation_Click);
            // 
            // baseButtonRight
            // 
            this.baseButtonRight.Location = new System.Drawing.Point(120, 134);
            this.baseButtonRight.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonRight.Name = "baseButtonRight";
            this.baseButtonRight.Size = new System.Drawing.Size(46, 50);
            this.baseButtonRight.TabIndex = 4;
            this.baseButtonRight.Text = "▶";
            this.baseButtonRight.UseVisualStyleBackColor = true;
            this.baseButtonRight.Click += new System.EventHandler(this.baseButtonLocation_Click);
            // 
            // baseButtonCenter
            // 
            this.baseButtonCenter.Location = new System.Drawing.Point(64, 134);
            this.baseButtonCenter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonCenter.Name = "baseButtonCenter";
            this.baseButtonCenter.Size = new System.Drawing.Size(46, 50);
            this.baseButtonCenter.TabIndex = 3;
            this.baseButtonCenter.Text = "●";
            this.baseButtonCenter.UseVisualStyleBackColor = true;
            this.baseButtonCenter.Click += new System.EventHandler(this.baseButtonLocation_Click);
            // 
            // baseButtonUp
            // 
            this.baseButtonUp.Location = new System.Drawing.Point(64, 72);
            this.baseButtonUp.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseButtonUp.Name = "baseButtonUp";
            this.baseButtonUp.Size = new System.Drawing.Size(46, 50);
            this.baseButtonUp.TabIndex = 2;
            this.baseButtonUp.Text = "▲";
            this.baseButtonUp.UseVisualStyleBackColor = true;
            this.baseButtonUp.Click += new System.EventHandler(this.baseButtonLocation_Click);
            // 
            // baseToggleButtonInspect
            // 
            this.baseToggleButtonInspect.Location = new System.Drawing.Point(183, 25);
            this.baseToggleButtonInspect.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseToggleButtonInspect.Name = "baseToggleButtonInspect";
            this.baseToggleButtonInspect.Size = new System.Drawing.Size(184, 39);
            this.baseToggleButtonInspect.TabIndex = 1;
            this.baseToggleButtonInspect.Text = "Inspect";
            this.baseToggleButtonInspect.UseVisualStyleBackColor = true;
            this.baseToggleButtonInspect.Click += new System.EventHandler(this.baseToggleButtonInspect_Click);
            // 
            // baseToggleButtonTrain
            // 
            this.baseToggleButtonTrain.Location = new System.Drawing.Point(7, 25);
            this.baseToggleButtonTrain.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.baseToggleButtonTrain.Name = "baseToggleButtonTrain";
            this.baseToggleButtonTrain.Size = new System.Drawing.Size(161, 39);
            this.baseToggleButtonTrain.TabIndex = 0;
            this.baseToggleButtonTrain.Text = "Train";
            this.baseToggleButtonTrain.UseVisualStyleBackColor = true;
            this.baseToggleButtonTrain.Click += new System.EventHandler(this.baseToggleButtonTrain_Click);
            // 
            // MaintROIControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.expandableGroupBox1);
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "MaintROIControl";
            this.Size = new System.Drawing.Size(391, 531);
            this.expandableGroupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SimpleGroupBox expandableGroupBox1;
        private SimpleToggleButton baseToggleButtonInspect;
        private SimpleToggleButton baseToggleButtonTrain;
        private System.Windows.Forms.Button baseButtonLeft;
        private System.Windows.Forms.Button baseButtonDown;
        private System.Windows.Forms.Button baseButtonRight;
        private System.Windows.Forms.Button baseButtonCenter;
        private System.Windows.Forms.Button baseButtonUp;
        private System.Windows.Forms.Button baseButtonFullSize;
        private System.Windows.Forms.Button baseButtonYSizeDown;
        private System.Windows.Forms.Button baseButtonYSizeUp;
        private System.Windows.Forms.Button baseButtonXSizeDown;
        private System.Windows.Forms.Button baseButtonXSizeUp;
        private ParamTextControl paramTextControlSizeToke;
        private ParamTextControl paramTextControlMoveToke;
        private System.Windows.Forms.Button baseButtonSave;
        private ParamDualTextControl paramDualTextControlSize;
        private ParamDualTextControl paramDualTextControlCenter;
        private System.Windows.Forms.Button baseButtonRollBack;
    }
}
