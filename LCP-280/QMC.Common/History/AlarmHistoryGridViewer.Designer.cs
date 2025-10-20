namespace QMC.Common.History
{
    partial class AlarmHistoryGridViewer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmCause = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblWarning = new System.Windows.Forms.Label();
            this.lblError = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.cmbRecentDates = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnClearSearch = new System.Windows.Forms.Button();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbPageSize = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkEnableDateFilter = new System.Windows.Forms.CheckBox();
            this.dtpDateFilter = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFilterWarning = new System.Windows.Forms.Button();
            this.btnFilterError = new System.Windows.Forms.Button();
            this.btnFilterAll = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.lblCurrentPage = new System.Windows.Forms.Label();
            this.btnLastPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnPrevPage = new System.Windows.Forms.Button();
            this.btnFirstPage = new System.Windows.Forms.Button();
            this.lblPagination = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panelTop.SuspendLayout();
            this.panelFilter.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Time,
            this.AlarmType,
            this.AlarmCode,
            this.AlarmSource,
            this.AlarmTitle,
            this.AlarmCause});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 100);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(1140, 270);
            this.dataGridView1.TabIndex = 0;
            // 
            // Time
            // 
            this.Time.HeaderText = "Time";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.Width = 150;
            // 
            // AlarmType
            // 
            this.AlarmType.HeaderText = "Type";
            this.AlarmType.Name = "AlarmType";
            this.AlarmType.ReadOnly = true;
            this.AlarmType.Width = 80;
            // 
            // AlarmCode
            // 
            this.AlarmCode.HeaderText = "Code";
            this.AlarmCode.Name = "AlarmCode";
            this.AlarmCode.ReadOnly = true;
            this.AlarmCode.Width = 70;
            // 
            // AlarmSource
            // 
            this.AlarmSource.HeaderText = "Source";
            this.AlarmSource.Name = "AlarmSource";
            this.AlarmSource.ReadOnly = true;
            this.AlarmSource.Width = 180;
            // 
            // AlarmTitle
            // 
            this.AlarmTitle.HeaderText = "Title";
            this.AlarmTitle.Name = "AlarmTitle";
            this.AlarmTitle.ReadOnly = true;
            this.AlarmTitle.Width = 150;
            // 
            // AlarmCause
            // 
            this.AlarmCause.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.AlarmCause.HeaderText = "Cause";
            this.AlarmCause.Name = "AlarmCause";
            this.AlarmCause.ReadOnly = true;
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.lblWarning);
            this.panelTop.Controls.Add(this.lblError);
            this.panelTop.Controls.Add(this.lblTotal);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1140, 30);
            this.panelTop.TabIndex = 1;
            // 
            // lblWarning
            // 
            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new System.Drawing.Point(180, 8);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(64, 12);
            this.lblWarning.TabIndex = 2;
            this.lblWarning.Text = "Warning: 0";
            // 
            // lblError
            // 
            this.lblError.AutoSize = true;
            this.lblError.Location = new System.Drawing.Point(100, 8);
            this.lblError.Name = "lblError";
            this.lblError.Size = new System.Drawing.Size(46, 12);
            this.lblError.TabIndex = 1;
            this.lblError.Text = "Error: 0";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(10, 8);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(47, 12);
            this.lblTotal.TabIndex = 0;
            this.lblTotal.Text = "Total: 0";
            // 
            // panelFilter
            // 
            this.panelFilter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFilter.Controls.Add(this.cmbRecentDates);
            this.panelFilter.Controls.Add(this.label5);
            this.panelFilter.Controls.Add(this.btnClearSearch);
            this.panelFilter.Controls.Add(this.txtSearch);
            this.panelFilter.Controls.Add(this.label3);
            this.panelFilter.Controls.Add(this.cmbPageSize);
            this.panelFilter.Controls.Add(this.label4);
            this.panelFilter.Controls.Add(this.chkEnableDateFilter);
            this.panelFilter.Controls.Add(this.dtpDateFilter);
            this.panelFilter.Controls.Add(this.label2);
            this.panelFilter.Controls.Add(this.btnFilterWarning);
            this.panelFilter.Controls.Add(this.btnFilterError);
            this.panelFilter.Controls.Add(this.btnFilterAll);
            this.panelFilter.Controls.Add(this.label1);
            this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilter.Location = new System.Drawing.Point(0, 30);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Size = new System.Drawing.Size(1140, 70);
            this.panelFilter.TabIndex = 2;
            // 
            // cmbRecentDates
            // 
            this.cmbRecentDates.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRecentDates.FormattingEnabled = true;
            this.cmbRecentDates.Location = new System.Drawing.Point(372, 8);
            this.cmbRecentDates.Name = "cmbRecentDates";
            this.cmbRecentDates.Size = new System.Drawing.Size(200, 20);
            this.cmbRecentDates.TabIndex = 12;
            this.cmbRecentDates.SelectedIndexChanged += new System.EventHandler(this.cmbRecentDates_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(282, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 12);
            this.label5.TabIndex = 13;
            this.label5.Text = "Quick Date:";
            // 
            // btnClearSearch
            // 
            this.btnClearSearch.Location = new System.Drawing.Point(518, 37);
            this.btnClearSearch.Name = "btnClearSearch";
            this.btnClearSearch.Size = new System.Drawing.Size(55, 23);
            this.btnClearSearch.TabIndex = 11;
            this.btnClearSearch.Text = "Clear";
            this.btnClearSearch.UseVisualStyleBackColor = true;
            this.btnClearSearch.Click += new System.EventHandler(this.btnClearSearch_Click);
            // 
            // txtSearch
            // 
            this.txtSearch.Location = new System.Drawing.Point(60, 38);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(452, 21);
            this.txtSearch.TabIndex = 10;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(2, 42);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "Search:";
            // 
            // cmbPageSize
            // 
            this.cmbPageSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPageSize.FormattingEnabled = true;
            this.cmbPageSize.Items.AddRange(new object[] {
            "10",
            "20",
            "50",
            "100"});
            this.cmbPageSize.Location = new System.Drawing.Point(638, 38);
            this.cmbPageSize.Name = "cmbPageSize";
            this.cmbPageSize.Size = new System.Drawing.Size(60, 20);
            this.cmbPageSize.TabIndex = 8;
            this.cmbPageSize.SelectedIndexChanged += new System.EventHandler(this.cmbPageSize_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(588, 43);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "Rows:";
            // 
            // chkEnableDateFilter
            // 
            this.chkEnableDateFilter.AutoSize = true;
            this.chkEnableDateFilter.Location = new System.Drawing.Point(774, 12);
            this.chkEnableDateFilter.Name = "chkEnableDateFilter";
            this.chkEnableDateFilter.Size = new System.Drawing.Size(15, 14);
            this.chkEnableDateFilter.TabIndex = 6;
            this.chkEnableDateFilter.UseVisualStyleBackColor = true;
            this.chkEnableDateFilter.CheckedChanged += new System.EventHandler(this.chkEnableDateFilter_CheckedChanged);
            // 
            // dtpDateFilter
            // 
            this.dtpDateFilter.Enabled = false;
            this.dtpDateFilter.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpDateFilter.Location = new System.Drawing.Point(638, 8);
            this.dtpDateFilter.Name = "dtpDateFilter";
            this.dtpDateFilter.Size = new System.Drawing.Size(125, 21);
            this.dtpDateFilter.TabIndex = 5;
            this.dtpDateFilter.ValueChanged += new System.EventHandler(this.dtpDateFilter_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(594, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "Date:";
            // 
            // btnFilterWarning
            // 
            this.btnFilterWarning.Location = new System.Drawing.Point(200, 7);
            this.btnFilterWarning.Name = "btnFilterWarning";
            this.btnFilterWarning.Size = new System.Drawing.Size(70, 23);
            this.btnFilterWarning.TabIndex = 3;
            this.btnFilterWarning.Text = "Warning";
            this.btnFilterWarning.UseVisualStyleBackColor = true;
            this.btnFilterWarning.Click += new System.EventHandler(this.btnFilterWarning_Click);
            // 
            // btnFilterError
            // 
            this.btnFilterError.Location = new System.Drawing.Point(130, 7);
            this.btnFilterError.Name = "btnFilterError";
            this.btnFilterError.Size = new System.Drawing.Size(70, 23);
            this.btnFilterError.TabIndex = 2;
            this.btnFilterError.Text = "Error";
            this.btnFilterError.UseVisualStyleBackColor = true;
            this.btnFilterError.Click += new System.EventHandler(this.btnFilterError_Click);
            // 
            // btnFilterAll
            // 
            this.btnFilterAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnFilterAll.ForeColor = System.Drawing.Color.White;
            this.btnFilterAll.Location = new System.Drawing.Point(60, 7);
            this.btnFilterAll.Name = "btnFilterAll";
            this.btnFilterAll.Size = new System.Drawing.Size(70, 23);
            this.btnFilterAll.TabIndex = 1;
            this.btnFilterAll.Text = "All";
            this.btnFilterAll.UseVisualStyleBackColor = false;
            this.btnFilterAll.Click += new System.EventHandler(this.btnFilterAll_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(15, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Type:";
            // 
            // panelBottom
            // 
            this.panelBottom.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelBottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelBottom.Controls.Add(this.lblCurrentPage);
            this.panelBottom.Controls.Add(this.btnLastPage);
            this.panelBottom.Controls.Add(this.btnNextPage);
            this.panelBottom.Controls.Add(this.btnPrevPage);
            this.panelBottom.Controls.Add(this.btnFirstPage);
            this.panelBottom.Controls.Add(this.lblPagination);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 370);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1140, 30);
            this.panelBottom.TabIndex = 3;
            // 
            // lblCurrentPage
            // 
            this.lblCurrentPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCurrentPage.Location = new System.Drawing.Point(850, 8);
            this.lblCurrentPage.Name = "lblCurrentPage";
            this.lblCurrentPage.Size = new System.Drawing.Size(100, 12);
            this.lblCurrentPage.TabIndex = 5;
            this.lblCurrentPage.Text = "Page 1 / 1";
            this.lblCurrentPage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnLastPage
            // 
            this.btnLastPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLastPage.Location = new System.Drawing.Point(1090, 4);
            this.btnLastPage.Name = "btnLastPage";
            this.btnLastPage.Size = new System.Drawing.Size(40, 23);
            this.btnLastPage.TabIndex = 4;
            this.btnLastPage.Text = ">|";
            this.btnLastPage.UseVisualStyleBackColor = true;
            this.btnLastPage.Click += new System.EventHandler(this.btnLastPage_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNextPage.Location = new System.Drawing.Point(1050, 4);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(40, 23);
            this.btnNextPage.TabIndex = 3;
            this.btnNextPage.Text = ">";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // btnPrevPage
            // 
            this.btnPrevPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPrevPage.Location = new System.Drawing.Point(1010, 4);
            this.btnPrevPage.Name = "btnPrevPage";
            this.btnPrevPage.Size = new System.Drawing.Size(40, 23);
            this.btnPrevPage.TabIndex = 2;
            this.btnPrevPage.Text = "<";
            this.btnPrevPage.UseVisualStyleBackColor = true;
            this.btnPrevPage.Click += new System.EventHandler(this.btnPrevPage_Click);
            // 
            // btnFirstPage
            // 
            this.btnFirstPage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFirstPage.Location = new System.Drawing.Point(970, 4);
            this.btnFirstPage.Name = "btnFirstPage";
            this.btnFirstPage.Size = new System.Drawing.Size(40, 23);
            this.btnFirstPage.TabIndex = 1;
            this.btnFirstPage.Text = "|<";
            this.btnFirstPage.UseVisualStyleBackColor = true;
            this.btnFirstPage.Click += new System.EventHandler(this.btnFirstPage_Click);
            // 
            // lblPagination
            // 
            this.lblPagination.AutoSize = true;
            this.lblPagination.Location = new System.Drawing.Point(10, 8);
            this.lblPagination.Name = "lblPagination";
            this.lblPagination.Size = new System.Drawing.Size(43, 12);
            this.lblPagination.TabIndex = 0;
            this.lblPagination.Text = "0-0 / 0";
            // 
            // AlarmHistoryGridViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelFilter);
            this.Controls.Add(this.panelTop);
            this.Name = "AlarmHistoryGridViewer";
            this.Size = new System.Drawing.Size(1140, 400);
            this.Load += new System.EventHandler(this.AlarmHistoryGridViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn AlarmType;
        private System.Windows.Forms.DataGridViewTextBoxColumn AlarmCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn AlarmSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn AlarmTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn AlarmCause;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.Label lblError;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.Button btnFilterWarning;
        private System.Windows.Forms.Button btnFilterError;
        private System.Windows.Forms.Button btnFilterAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label lblPagination;
        private System.Windows.Forms.Button btnLastPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnPrevPage;
        private System.Windows.Forms.Button btnFirstPage;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DateTimePicker dtpDateFilter;
        private System.Windows.Forms.CheckBox chkEnableDateFilter;
        private System.Windows.Forms.ComboBox cmbPageSize;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblCurrentPage;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnClearSearch;
        private System.Windows.Forms.ComboBox cmbRecentDates;
        private System.Windows.Forms.Label label5;
    }
}