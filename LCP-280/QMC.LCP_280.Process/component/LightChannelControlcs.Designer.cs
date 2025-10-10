using QMC.Common;

namespace QMC.LCP_280.Process.Unit
{
    partial class LightChannelControlcs
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
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.iluminatorChannelListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.trackBar_LightIntensity = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_LightIntensity)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.iluminatorChannelListBoxItemsView, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(423, 276);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 231);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(417, 42);
            this.label2.TabIndex = 28;
            this.label2.Text = "Value";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // iluminatorChannelListBoxItemsView
            // 
            this.iluminatorChannelListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.iluminatorChannelListBoxItemsView.BorderWidth = 2;
            this.iluminatorChannelListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iluminatorChannelListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.iluminatorChannelListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.GroupName = "Channel";
            this.iluminatorChannelListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.iluminatorChannelListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.iluminatorChannelListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.iluminatorChannelListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorChannelListBoxItemsView.Name = "iluminatorChannelListBoxItemsView";
            this.iluminatorChannelListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorChannelListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.SelectedIndex = -1;
            this.iluminatorChannelListBoxItemsView.Size = new System.Drawing.Size(417, 181);
            this.iluminatorChannelListBoxItemsView.TabIndex = 20;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Controls.Add(this.trackBar_LightIntensity, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBox1, 2, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 196);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(417, 29);
            this.tableLayoutPanel3.TabIndex = 27;
            // 
            // trackBar_LightIntensity
            // 
            this.trackBar_LightIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackBar_LightIntensity.Location = new System.Drawing.Point(3, 3);
            this.trackBar_LightIntensity.Maximum = 4095;
            this.trackBar_LightIntensity.Name = "trackBar_LightIntensity";
            this.trackBar_LightIntensity.Size = new System.Drawing.Size(285, 23);
            this.trackBar_LightIntensity.TabIndex = 1;
            this.trackBar_LightIntensity.TickFrequency = 256;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(294, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Value";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(356, 3);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(58, 21);
            this.textBox1.TabIndex = 3;
            // 
            // LightChannelControlcs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "LightChannelControlcs";
            this.Size = new System.Drawing.Size(423, 276);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_LightIntensity)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private ListBoxItemsView iluminatorChannelListBoxItemsView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TrackBar trackBar_LightIntensity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
    }
}
