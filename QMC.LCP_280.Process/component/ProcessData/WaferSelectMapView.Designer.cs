namespace QMC.LCP_280.Process.Component
{
    partial class WaferSelectMapView
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
                _toolTip?.Dispose();
                _hoverTimer?.Dispose();
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pWaferImage = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_NextOrderValue = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_NextOrder = new System.Windows.Forms.Label();
            this.list_SelectedSlots = new System.Windows.Forms.ListBox();
            this.btn_ResetAll = new System.Windows.Forms.Button();
            this.btn_All = new System.Windows.Forms.Button();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.lbl_SelectedCountValue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.tableLayoutPanel1);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox.Name = "groupBox";
            this.groupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox.Size = new System.Drawing.Size(450, 450);
            this.groupBox.TabIndex = 1;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Wafer Map";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Controls.Add(this.pWaferImage, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(4, 25);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(442, 421);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pWaferImage
            // 
            this.pWaferImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pWaferImage.Location = new System.Drawing.Point(4, 4);
            this.pWaferImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pWaferImage.Name = "pWaferImage";
            this.pWaferImage.Size = new System.Drawing.Size(301, 413);
            this.pWaferImage.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.btn_ResetAll, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.btn_All, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(313, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(125, 413);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.lbl_NextOrderValue, 1, 0);
            this.tableLayoutPanel5.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel5.Font = new System.Drawing.Font("굴림", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tableLayoutPanel5.Location = new System.Drawing.Point(4, 167);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(116, 32);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // lbl_NextOrderValue
            // 
            this.lbl_NextOrderValue.AutoSize = true;
            this.lbl_NextOrderValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_NextOrderValue.Location = new System.Drawing.Point(62, 4);
            this.lbl_NextOrderValue.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lbl_NextOrderValue.Name = "lbl_NextOrderValue";
            this.lbl_NextOrderValue.Size = new System.Drawing.Size(50, 24);
            this.lbl_NextOrderValue.TabIndex = 7;
            this.lbl_NextOrderValue.Text = "0";
            this.lbl_NextOrderValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("굴림", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(4, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 24);
            this.label2.TabIndex = 6;
            this.label2.Text = "Index:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.lbl_NextOrder, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.list_SelectedSlots, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 208);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(117, 201);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // lbl_NextOrder
            // 
            this.lbl_NextOrder.AutoSize = true;
            this.lbl_NextOrder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_NextOrder.Font = new System.Drawing.Font("굴림", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_NextOrder.Location = new System.Drawing.Point(4, 4);
            this.lbl_NextOrder.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lbl_NextOrder.Name = "lbl_NextOrder";
            this.lbl_NextOrder.Size = new System.Drawing.Size(109, 22);
            this.lbl_NextOrder.TabIndex = 5;
            this.lbl_NextOrder.Text = "List:";
            this.lbl_NextOrder.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // list_SelectedSlots
            // 
            this.list_SelectedSlots.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list_SelectedSlots.Font = new System.Drawing.Font("굴림", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.list_SelectedSlots.FormattingEnabled = true;
            this.list_SelectedSlots.ItemHeight = 14;
            this.list_SelectedSlots.Location = new System.Drawing.Point(4, 34);
            this.list_SelectedSlots.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.list_SelectedSlots.Name = "list_SelectedSlots";
            this.list_SelectedSlots.Size = new System.Drawing.Size(109, 163);
            this.list_SelectedSlots.TabIndex = 4;
            // 
            // btn_ResetAll
            // 
            this.btn_ResetAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_ResetAll.Font = new System.Drawing.Font("굴림", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_ResetAll.Location = new System.Drawing.Point(4, 65);
            this.btn_ResetAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_ResetAll.Name = "btn_ResetAll";
            this.btn_ResetAll.Size = new System.Drawing.Size(117, 53);
            this.btn_ResetAll.TabIndex = 1;
            this.btn_ResetAll.Text = "All Init";
            this.btn_ResetAll.UseVisualStyleBackColor = true;
            this.btn_ResetAll.Click += new System.EventHandler(this.btn_ResetAll_Click);
            // 
            // btn_All
            // 
            this.btn_All.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_All.Font = new System.Drawing.Font("굴림", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_All.Location = new System.Drawing.Point(4, 4);
            this.btn_All.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_All.Name = "btn_All";
            this.btn_All.Size = new System.Drawing.Size(117, 53);
            this.btn_All.TabIndex = 0;
            this.btn_All.Text = "All Select";
            this.btn_All.UseVisualStyleBackColor = true;
            this.btn_All.Click += new System.EventHandler(this.btn_All_Click);
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.lbl_SelectedCountValue, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 126);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(116, 32);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // lbl_SelectedCountValue
            // 
            this.lbl_SelectedCountValue.AutoSize = true;
            this.lbl_SelectedCountValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbl_SelectedCountValue.Font = new System.Drawing.Font("굴림", 6F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lbl_SelectedCountValue.Location = new System.Drawing.Point(62, 4);
            this.lbl_SelectedCountValue.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lbl_SelectedCountValue.Name = "lbl_SelectedCountValue";
            this.lbl_SelectedCountValue.Size = new System.Drawing.Size(50, 24);
            this.lbl_SelectedCountValue.TabIndex = 7;
            this.lbl_SelectedCountValue.Text = "0";
            this.lbl_SelectedCountValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("굴림", 6.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 24);
            this.label1.TabIndex = 6;
            this.label1.Text = "Select:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WaferSelectMapView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.groupBox);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "WaferSelectMapView";
            this.Size = new System.Drawing.Size(450, 450);
            this.groupBox.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel pWaferImage;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.ListBox list_SelectedSlots;
        private System.Windows.Forms.Button btn_ResetAll;
        private System.Windows.Forms.Button btn_All;
        private System.Windows.Forms.Label lbl_NextOrder;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbl_NextOrderValue;
        private System.Windows.Forms.Label lbl_SelectedCountValue;
    }
}
