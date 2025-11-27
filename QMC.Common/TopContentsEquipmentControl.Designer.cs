using QMC.Common.CustomControl;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.Common
{
    partial class TopContentsEquipmentControl
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
            this.tableLayoutContentsEquipmentPanel = new QMC.Common.CustomControl.DeleteLogoBorderTableLayoutPanel();
            this.pictureBoxLogo = new System.Windows.Forms.PictureBox();
            this._machineName = new QMC.Common.CustomControl.CustomBorderLabel();
            this._dateLabel = new QMC.Common.CustomControl.CustomBorderLabel();
            this._timeLabel = new QMC.Common.CustomControl.CustomBorderLabel();
            this._buildVerLabel = new QMC.Common.CustomControl.CustomBorderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutContentsEquipmentPanel
            // 
            this.tableLayoutContentsEquipmentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutContentsEquipmentPanel.BackColor = System.Drawing.Color.White;
            this.tableLayoutContentsEquipmentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutContentsEquipmentPanel.Padding = new System.Windows.Forms.Padding(0);
            this.tableLayoutContentsEquipmentPanel.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;

            this.tableLayoutContentsEquipmentPanel.ColumnCount = 2;
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Clear();
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutContentsEquipmentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));

            this.tableLayoutContentsEquipmentPanel.RowCount = 3;
            this.tableLayoutContentsEquipmentPanel.RowStyles.Clear();
            this.tableLayoutContentsEquipmentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tableLayoutContentsEquipmentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            this.tableLayoutContentsEquipmentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.3333F));
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxLogo.BackColor = System.Drawing.Color.White;
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxLogo.TabStop = false;
            this.pictureBoxLogo.Margin = new System.Windows.Forms.Padding(3);
            // 리소스가 존재하면 이미지 설정(없어도 디자이너 동작에는 영향 없음)
            this.pictureBoxLogo.Image = global::QMC.Common.Properties.Resources.Logo;
            // 
            // _machineName
            // 
            this._machineName.Text = "Machine Name";
            this._machineName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._machineName.Dock = System.Windows.Forms.DockStyle.Fill;
            this._machineName.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._machineName.AutoSize = false;
            this._machineName.BorderColor = System.Drawing.Color.White;
            this._machineName.TabStop = false;
            this._machineName.Margin = new System.Windows.Forms.Padding(3);
            // 
            // _dateLabel
            // 
            this._dateLabel.Text = "";
            this._dateLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._dateLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._dateLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._dateLabel.AutoSize = false;
            this._dateLabel.BorderColor = System.Drawing.Color.White;
            this._dateLabel.TabStop = false;
            this._dateLabel.Margin = new System.Windows.Forms.Padding(3);
            // 
            // _timeLabel
            // 
            this._timeLabel.Text = "";
            this._timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._timeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._timeLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._timeLabel.AutoSize = false;
            this._timeLabel.BorderColor = System.Drawing.Color.White;
            this._timeLabel.TabStop = false;
            this._timeLabel.Margin = new System.Windows.Forms.Padding(3);
            // 
            // _buildVerLabel
            // 
            this._buildVerLabel.Text = "Build Ver.";
            this._buildVerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this._buildVerLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._buildVerLabel.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._buildVerLabel.AutoSize = false;
            this._buildVerLabel.BorderColor = System.Drawing.Color.White;
            this._buildVerLabel.TabStop = false;
            this._buildVerLabel.Margin = new System.Windows.Forms.Padding(3);
            // 
            // add controls and layout
            // 
            this.tableLayoutContentsEquipmentPanel.Controls.Add(this.pictureBoxLogo, 0, 0);
            this.tableLayoutContentsEquipmentPanel.SetRowSpan(this.pictureBoxLogo, 2);

            this.tableLayoutContentsEquipmentPanel.Controls.Add(this._machineName, 0, 2);
            this.tableLayoutContentsEquipmentPanel.Controls.Add(this._dateLabel, 1, 0);
            this.tableLayoutContentsEquipmentPanel.Controls.Add(this._timeLabel, 1, 1);
            this.tableLayoutContentsEquipmentPanel.Controls.Add(this._buildVerLabel, 1, 2);

            // 
            // TopContentsEquipmentControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutContentsEquipmentPanel);
            this.Name = "TopContentsEquipmentControl";
            this.Size = new System.Drawing.Size(600, 140);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private DeleteLogoBorderTableLayoutPanel tableLayoutContentsEquipmentPanel;
        private System.Windows.Forms.PictureBox pictureBoxLogo;
    }
}