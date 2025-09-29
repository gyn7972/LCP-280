namespace QMC.Common.History
{
    partial class AlarmHistoryGridViewer
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmSource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AlarmCause = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
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
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(1140, 400);
            this.dataGridView1.TabIndex = 0;
            // 
            // Time
            // 
            this.Time.Frozen = true;
            this.Time.HeaderText = "Time";
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Time.Width = 150;
            // 
            // AlarmType
            // 
            this.AlarmType.Frozen = true;
            this.AlarmType.HeaderText = "Type";
            this.AlarmType.Name = "AlarmType";
            this.AlarmType.ReadOnly = true;
            this.AlarmType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.AlarmType.Width = 80;
            // 
            // AlarmCode
            // 
            this.AlarmCode.Frozen = true;
            this.AlarmCode.HeaderText = "Code";
            this.AlarmCode.Name = "AlarmCode";
            this.AlarmCode.ReadOnly = true;
            this.AlarmCode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.AlarmCode.Width = 80;
            // 
            // AlarmSource
            // 
            this.AlarmSource.Frozen = true;
            this.AlarmSource.HeaderText = "Source";
            this.AlarmSource.Name = "AlarmSource";
            this.AlarmSource.ReadOnly = true;
            this.AlarmSource.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.AlarmSource.Width = 200;
            // 
            // AlarmTitle
            // 
            this.AlarmTitle.Frozen = true;
            this.AlarmTitle.HeaderText = "Title";
            this.AlarmTitle.Name = "AlarmTitle";
            this.AlarmTitle.ReadOnly = true;
            this.AlarmTitle.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.AlarmTitle.Width = 300;
            // 
            // AlarmCause
            // 
            this.AlarmCause.Frozen = true;
            this.AlarmCause.HeaderText = "Cause";
            this.AlarmCause.Name = "AlarmCause";
            this.AlarmCause.ReadOnly = true;
            this.AlarmCause.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.AlarmCause.Width = 500;
            // 
            // AlarmHistoryGridViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Name = "AlarmHistoryGridViewer";
            this.Size = new System.Drawing.Size(1140, 400);
            this.Load += new System.EventHandler(this.AlarmHistoryGridViewer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
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
    }
}
