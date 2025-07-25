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
            this.propertyCollectionView = new SP_GridTypeView.PropertyCollectionView();
            this.ioPropertyCollectionView = new SP_GridTypeView.IOPropertyCollectionView();
            this.visionImageview = new SP_GridTypeView.VisionImageView();
            this.listBoxItemsView = new SP_GridTypeView.ListBoxItemsView();
            this.SuspendLayout();
            // 
            // propertyCollectionView1
            // 
            this.propertyCollectionView.Location = new System.Drawing.Point(690, 171);
            this.propertyCollectionView.Name = "propertyCollectionView1";
            this.propertyCollectionView.Size = new System.Drawing.Size(337, 245);
            this.propertyCollectionView.TabIndex = 0;
            this.propertyCollectionView.TextBoxFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.propertyCollectionView.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ioPropertyCollectionView1
            // 
            this.ioPropertyCollectionView.Location = new System.Drawing.Point(400, 500);
            this.ioPropertyCollectionView.Name = "ioPropertyCollectionView1";
            this.ioPropertyCollectionView.Size = new System.Drawing.Size(337, 245);
            this.ioPropertyCollectionView.TabIndex = 1;
            this.ioPropertyCollectionView.TextBoxFont = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ioPropertyCollectionView.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // visionImageview
            // 
            this.visionImageview.CrossLineWidth = 1;
            this.visionImageview.Location = new System.Drawing.Point(12, 12);
            this.visionImageview.Name = "visionImageview";
            this.visionImageview.ShowLiveGrabButtons = true;
            this.visionImageview.Size = new System.Drawing.Size(200, 200);
            this.visionImageview.TabBorderColor = System.Drawing.Color.Black;
            this.visionImageview.TabBorderWidth = 2;
            this.visionImageview.TabFont = new System.Drawing.Font("맑은 고딕", 9F);
            this.visionImageview.TabHeight = 28;
            this.visionImageview.TabIndex = 2;
            // 
            // listBoxItemsView
            // 
            this.listBoxItemsView.BorderWidth = 2;
            this.listBoxItemsView.Location = new System.Drawing.Point(12, 520);
            this.listBoxItemsView.Name = "listBoxItemsView";
            this.listBoxItemsView.SelectedIndex = -1;
            this.listBoxItemsView.Size = new System.Drawing.Size(200, 200);
            this.listBoxItemsView.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1162, 531);
            this.Controls.Add(this.visionImageview);
            this.Controls.Add(this.propertyCollectionView);
            this.Controls.Add(this.ioPropertyCollectionView);
            this.Controls.Add(this.listBoxItemsView);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private PropertyCollectionView propertyCollectionView;
        private IOPropertyCollectionView ioPropertyCollectionView;
        private VisionImageView visionImageview;
        private ListBoxItemsView listBoxItemsView;
    }
}

