namespace QMC.LCP_280.Process.Component
{
    partial class WaferMapView
    {
        /// <summary>
        /// 디자이너에서 필요한 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 해제할지 여부</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 컨트롤 초기화

        private void InitializeComponent()
        {
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = new System.Drawing.Point(0, 0);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(140, 230);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Wafer Map";
            // (Paint 이벤트는 코드 비하인드에서 연결)
            // 
            // WaferMapView
            // 
            this.Controls.Add(this.groupBox);
            this.Name = "WaferMapView";
            this.Size = new System.Drawing.Size(140, 230);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
    }
}