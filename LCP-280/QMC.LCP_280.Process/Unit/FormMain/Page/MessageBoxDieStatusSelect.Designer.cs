namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class MessageBoxDieStatusSelect
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

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxDieStatusSelect));
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_Error = new System.Windows.Forms.Button();
            this.btn_Completed = new System.Windows.Forms.Button();
            this.btn_Outputting = new System.Windows.Forms.Button();
            this.btn_Unloading = new System.Windows.Forms.Button();
            this.btn_Probed = new System.Windows.Forms.Button();
            this.btn_Probing = new System.Windows.Forms.Button();
            this.btn_Aligned = new System.Windows.Forms.Button();
            this.btn_Aligning = new System.Windows.Forms.Button();
            this.btn_Loaded = new System.Windows.Forms.Button();
            this.btn_Loading = new System.Windows.Forms.Button();
            this.btn_Empty = new System.Windows.Forms.Button();
            this.btn_Ok = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DarkOrange;
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(527, 23);
            this.panel1.TabIndex = 5;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.lblTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblTitle.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblTitle.Location = new System.Drawing.Point(0, 0);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(3);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(527, 23);
            this.lblTitle.TabIndex = 4;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.ErrorImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.ErrorImage")));
            this.pictureBox1.Image = global::QMC.LCP_280.Process.Properties.Resources.megaphone_80px;
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(93, 99);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.txtMessage.Location = new System.Drawing.Point(108, 3);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(416, 105);
            this.txtMessage.TabIndex = 6;
            this.txtMessage.Text = "This is a message";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(533, 294);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.txtMessage, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 32);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(527, 111);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 105F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(99, 105);
            this.tableLayoutPanel4.TabIndex = 7;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 6;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel3.Controls.Add(this.btn_Completed, 4, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Outputting, 3, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Unloading, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Probed, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Probing, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Aligned, 4, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Aligning, 3, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Loaded, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Loading, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Empty, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Error, 5, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Ok, 5, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 149);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(527, 142);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // btn_Error
            // 
            this.btn_Error.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Error.Location = new System.Drawing.Point(438, 3);
            this.btn_Error.Name = "btn_Error";
            this.btn_Error.Size = new System.Drawing.Size(86, 41);
            this.btn_Error.TabIndex = 13;
            this.btn_Error.Text = "&Error";
            this.btn_Error.UseVisualStyleBackColor = true;
            this.btn_Error.Click += new System.EventHandler(this.btn_Error_Click);
            // 
            // btn_Completed
            // 
            this.btn_Completed.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Completed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Completed.Location = new System.Drawing.Point(351, 50);
            this.btn_Completed.Name = "btn_Completed";
            this.btn_Completed.Size = new System.Drawing.Size(81, 41);
            this.btn_Completed.TabIndex = 12;
            this.btn_Completed.Text = "&Completed";
            this.btn_Completed.UseVisualStyleBackColor = true;
            this.btn_Completed.Click += new System.EventHandler(this.btn_Completed_Click);
            // 
            // btn_Outputting
            // 
            this.btn_Outputting.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Outputting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Outputting.Location = new System.Drawing.Point(264, 50);
            this.btn_Outputting.Name = "btn_Outputting";
            this.btn_Outputting.Size = new System.Drawing.Size(81, 41);
            this.btn_Outputting.TabIndex = 11;
            this.btn_Outputting.Text = "&Outputting";
            this.btn_Outputting.UseVisualStyleBackColor = true;
            this.btn_Outputting.Click += new System.EventHandler(this.btn_Outputting_Click);
            // 
            // btn_Unloading
            // 
            this.btn_Unloading.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Unloading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Unloading.Location = new System.Drawing.Point(177, 50);
            this.btn_Unloading.Name = "btn_Unloading";
            this.btn_Unloading.Size = new System.Drawing.Size(81, 41);
            this.btn_Unloading.TabIndex = 10;
            this.btn_Unloading.Text = "&Unloading";
            this.btn_Unloading.UseVisualStyleBackColor = true;
            this.btn_Unloading.Click += new System.EventHandler(this.btn_Unloading_Click);
            // 
            // btn_Probed
            // 
            this.btn_Probed.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Probed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Probed.Location = new System.Drawing.Point(90, 50);
            this.btn_Probed.Name = "btn_Probed";
            this.btn_Probed.Size = new System.Drawing.Size(81, 41);
            this.btn_Probed.TabIndex = 9;
            this.btn_Probed.Text = "&Probed";
            this.btn_Probed.UseVisualStyleBackColor = true;
            this.btn_Probed.Click += new System.EventHandler(this.btn_Probed_Click);
            // 
            // btn_Probing
            // 
            this.btn_Probing.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Probing.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Probing.Location = new System.Drawing.Point(3, 50);
            this.btn_Probing.Name = "btn_Probing";
            this.btn_Probing.Size = new System.Drawing.Size(81, 41);
            this.btn_Probing.TabIndex = 8;
            this.btn_Probing.Text = "&Probing";
            this.btn_Probing.UseVisualStyleBackColor = true;
            this.btn_Probing.Click += new System.EventHandler(this.btn_Probing_Click);
            // 
            // btn_Aligned
            // 
            this.btn_Aligned.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Aligned.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Aligned.Location = new System.Drawing.Point(351, 3);
            this.btn_Aligned.Name = "btn_Aligned";
            this.btn_Aligned.Size = new System.Drawing.Size(81, 41);
            this.btn_Aligned.TabIndex = 7;
            this.btn_Aligned.Text = "&Aligned";
            this.btn_Aligned.UseVisualStyleBackColor = true;
            this.btn_Aligned.Click += new System.EventHandler(this.btn_Aligned_Click);
            // 
            // btn_Aligning
            // 
            this.btn_Aligning.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Aligning.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Aligning.Location = new System.Drawing.Point(264, 3);
            this.btn_Aligning.Name = "btn_Aligning";
            this.btn_Aligning.Size = new System.Drawing.Size(81, 41);
            this.btn_Aligning.TabIndex = 6;
            this.btn_Aligning.Text = "&Aligning";
            this.btn_Aligning.UseVisualStyleBackColor = true;
            this.btn_Aligning.Click += new System.EventHandler(this.btn_Aligning_Click);
            // 
            // btn_Loaded
            // 
            this.btn_Loaded.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Loaded.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Loaded.Location = new System.Drawing.Point(177, 3);
            this.btn_Loaded.Name = "btn_Loaded";
            this.btn_Loaded.Size = new System.Drawing.Size(81, 41);
            this.btn_Loaded.TabIndex = 5;
            this.btn_Loaded.Text = "&Loaded";
            this.btn_Loaded.UseVisualStyleBackColor = true;
            this.btn_Loaded.Click += new System.EventHandler(this.btn_Loaded_Click);
            // 
            // btn_Loading
            // 
            this.btn_Loading.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Loading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Loading.Location = new System.Drawing.Point(90, 3);
            this.btn_Loading.Name = "btn_Loading";
            this.btn_Loading.Size = new System.Drawing.Size(81, 41);
            this.btn_Loading.TabIndex = 4;
            this.btn_Loading.Text = "&Loading";
            this.btn_Loading.UseVisualStyleBackColor = true;
            this.btn_Loading.Click += new System.EventHandler(this.btn_Loading_Click);
            // 
            // btn_Empty
            // 
            this.btn_Empty.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Empty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Empty.Location = new System.Drawing.Point(3, 3);
            this.btn_Empty.Name = "btn_Empty";
            this.btn_Empty.Size = new System.Drawing.Size(81, 41);
            this.btn_Empty.TabIndex = 3;
            this.btn_Empty.Text = "&Empty";
            this.btn_Empty.UseVisualStyleBackColor = true;
            this.btn_Empty.Click += new System.EventHandler(this.btn_Empty_Click);
            // 
            // btn_Ok
            // 
            this.btn_Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_Ok.Location = new System.Drawing.Point(438, 97);
            this.btn_Ok.Name = "btn_Ok";
            this.btn_Ok.Size = new System.Drawing.Size(86, 42);
            this.btn_Ok.TabIndex = 14;
            this.btn_Ok.Text = "&Ok";
            this.btn_Ok.UseVisualStyleBackColor = true;
            this.btn_Ok.Click += new System.EventHandler(this.btn_Skip_Click);
            // 
            // MessageBoxDieStatusSelect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 294);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MessageBoxDieStatusSelect";
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button btn_Aligned;
        private System.Windows.Forms.Button btn_Aligning;
        private System.Windows.Forms.Button btn_Loaded;
        private System.Windows.Forms.Button btn_Loading;
        private System.Windows.Forms.Button btn_Empty;
        private System.Windows.Forms.Button btn_Error;
        private System.Windows.Forms.Button btn_Completed;
        private System.Windows.Forms.Button btn_Outputting;
        private System.Windows.Forms.Button btn_Unloading;
        private System.Windows.Forms.Button btn_Probed;
        private System.Windows.Forms.Button btn_Probing;
        private System.Windows.Forms.Button btn_Ok;
    }
}
