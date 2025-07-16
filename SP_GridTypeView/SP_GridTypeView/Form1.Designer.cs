namespace SP_GridTypeView
{
    partial class Form1
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

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.propertyCollectionView1 = new SP_GridTypeView.PropertyCollectionView();
            this.propertyCollectionView2 = new SP_GridTypeView.PropertyCollectionView();
            this.SuspendLayout();
            // 
            // propertyCollectionView1
            // 
            this.propertyCollectionView1.Location = new System.Drawing.Point(690, 171);
            this.propertyCollectionView1.Name = "propertyCollectionView1";
            this.propertyCollectionView1.Size = new System.Drawing.Size(337, 245);
            this.propertyCollectionView1.TabIndex = 0;
            this.propertyCollectionView1.TextBoxFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.propertyCollectionView1.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // propertyCollectionView2
            // 
            this.propertyCollectionView2.Location = new System.Drawing.Point(164, 115);
            this.propertyCollectionView2.Name = "propertyCollectionView2";
            this.propertyCollectionView2.Size = new System.Drawing.Size(404, 193);
            this.propertyCollectionView2.TabIndex = 1;
            this.propertyCollectionView2.TextBoxFont = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.propertyCollectionView2.TextBoxFontSize = 12F;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1162, 531);
            this.Controls.Add(this.propertyCollectionView2);
            this.Controls.Add(this.propertyCollectionView1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PropertyCollectionView propertyCollectionView1;
        private PropertyCollectionView propertyCollectionView2;
    }
}

