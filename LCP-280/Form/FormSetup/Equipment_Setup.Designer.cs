using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class Equipment_Setup
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private QMC.Common.PropertyCollectionView EquipmentPropertyCollectionView;
        private QMC.Common.IndividualMenuButton btn_Save_Setup_Equipment;
        private QMC.Common.IndividualMenuButton btnLanguage;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.EquipmentPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.btnLanguage = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Setup_Equipment = new QMC.Common.IndividualMenuButton();
            this.brnMapMatch = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.EquipmentPropertyCollectionView, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 88F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1896, 1126);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // EquipmentPropertyCollectionView
            // 
            this.EquipmentPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EquipmentPropertyCollectionView.FastBuild = true;
            this.EquipmentPropertyCollectionView.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.EquipmentPropertyCollectionView.GroupName = "EquipmentConfig";
            this.EquipmentPropertyCollectionView.Location = new System.Drawing.Point(8, 8);
            this.EquipmentPropertyCollectionView.Margin = new System.Windows.Forms.Padding(8);
            this.EquipmentPropertyCollectionView.Name = "EquipmentPropertyCollectionView";
            this.EquipmentPropertyCollectionView.Size = new System.Drawing.Size(1880, 974);
            this.EquipmentPropertyCollectionView.SuppressResizeInvalidation = true;
            this.EquipmentPropertyCollectionView.TabIndex = 0;
            this.EquipmentPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 10F);
            this.EquipmentPropertyCollectionView.TextBoxFontSize = 10F;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 7;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 14.28571F));
            this.tableLayoutPanel3.Controls.Add(this.brnMapMatch, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.btnLanguage, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.btn_Save_Setup_Equipment, 6, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(8, 998);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(8);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1880, 120);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // btnLanguage
            // 
            this.btnLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLanguage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnLanguage.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnLanguage.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLanguage.CustomForeColor = System.Drawing.Color.Black;
            this.btnLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnLanguage.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnLanguage.ForeColor = System.Drawing.Color.Black;
            this.btnLanguage.ImageSize = new System.Drawing.Size(45, 45);
            this.btnLanguage.Location = new System.Drawing.Point(4, 4);
            this.btnLanguage.Margin = new System.Windows.Forms.Padding(4);
            this.btnLanguage.Name = "btnLanguage";
            this.btnLanguage.Size = new System.Drawing.Size(260, 112);
            this.btnLanguage.TabIndex = 0;
            this.btnLanguage.TabStop = false;
            this.btnLanguage.Text = "Language";
            this.btnLanguage.UseVisualStyleBackColor = false;
            this.btnLanguage.Click += new System.EventHandler(this.btnLanguage_Click);
            // 
            // btn_Save_Setup_Equipment
            // 
            this.btn_Save_Setup_Equipment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Equipment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Equipment.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Equipment.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Equipment.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Equipment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Setup_Equipment.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Equipment.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Equipment.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Equipment.Location = new System.Drawing.Point(1612, 4);
            this.btn_Save_Setup_Equipment.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Save_Setup_Equipment.Name = "btn_Save_Setup_Equipment";
            this.btn_Save_Setup_Equipment.Size = new System.Drawing.Size(264, 112);
            this.btn_Save_Setup_Equipment.TabIndex = 1;
            this.btn_Save_Setup_Equipment.TabStop = false;
            this.btn_Save_Setup_Equipment.Text = "Save";
            this.btn_Save_Setup_Equipment.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Equipment.Click += new System.EventHandler(this.btn_Save_Setup_Equipment_Click);
            // 
            // brnMapMatch
            // 
            this.brnMapMatch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.brnMapMatch.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.brnMapMatch.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.brnMapMatch.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.brnMapMatch.CustomForeColor = System.Drawing.Color.Black;
            this.brnMapMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.brnMapMatch.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.brnMapMatch.ForeColor = System.Drawing.Color.Black;
            this.brnMapMatch.ImageSize = new System.Drawing.Size(45, 45);
            this.brnMapMatch.Location = new System.Drawing.Point(272, 4);
            this.brnMapMatch.Margin = new System.Windows.Forms.Padding(4);
            this.brnMapMatch.Name = "brnMapMatch";
            this.brnMapMatch.Size = new System.Drawing.Size(260, 112);
            this.brnMapMatch.TabIndex = 2;
            this.brnMapMatch.TabStop = false;
            this.brnMapMatch.Text = "MapMatchTest";
            this.brnMapMatch.UseVisualStyleBackColor = false;
            this.brnMapMatch.Click += new System.EventHandler(this.brnMapMatch_Click);
            // 
            // Equipment_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1896, 1126);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Name = "Equipment_Setup";
            this.Text = "Equipment Setup";
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private Common.IndividualMenuButton brnMapMatch;
    }
}


//using QMC.Common;
//using System.Windows.Forms;

//namespace QMC.LCP_280.Process.Unit
//{
//    partial class Equipment_Setup
//    {
//        /// <summary>
//        /// Required designer variable.
//        /// </summary>
//        private System.ComponentModel.IContainer components = null;
//        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
//        private Common.PropertyCollectionView EquipmentPropertyCollectionView;
//        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
//        private Common.IndividualMenuButton btn_Save_Setup_Equipment;
//        private Common.IndividualMenuButton btnLanguage;
//        private PropertyCollectionView propertyCollectionView1;
//        private Button btnSave;
//        private Button btnReload;
//        private FlowLayoutPanel flowPanelTop;

//        /// <summary>
//        /// Clean up any resources being used.
//        /// </summary>
//        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
//        protected override void Dispose(bool disposing)
//        {
//            if (disposing && (components != null))
//            {
//                components.Dispose();
//            }
//            base.Dispose(disposing);
//        }

//        #region Windows Form Designer generated code

//        /// <summary>
//        /// Required method for Designer support - do not modify
//        /// the contents of this method with the code editor.
//        /// </summary>
//        private void InitializeComponent()
//        {
//            this.components = new System.ComponentModel.Container();
//            this.propertyCollectionView1 = new QMC.Common.PropertyCollectionView();
//            this.btnSave = new System.Windows.Forms.Button();
//            this.btnReload = new System.Windows.Forms.Button();
//            this.flowPanelTop = new System.Windows.Forms.FlowLayoutPanel();
//            this.flowPanelTop.SuspendLayout();

//            // 
//            // flowPanelTop
//            // 
//            this.flowPanelTop.Dock = System.Windows.Forms.DockStyle.Top;
//            this.flowPanelTop.Height = 36;
//            this.flowPanelTop.Padding = new Padding(4);
//            this.flowPanelTop.FlowDirection = FlowDirection.LeftToRight;
//            this.flowPanelTop.WrapContents = false;
//            // 
//            // btnSave
//            // 
//            this.btnSave.Text = "Save";
//            this.btnSave.AutoSize = true;
//            this.btnSave.Margin = new Padding(4);
//            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
//            // 
//            // btnReload
//            // 
//            this.btnReload.Text = "Reload";
//            this.btnReload.AutoSize = true;
//            this.btnReload.Margin = new Padding(4);
//            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
//            // 
//            // propertyCollectionView1
//            // 
//            this.propertyCollectionView1.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.propertyCollectionView1.Location = new System.Drawing.Point(0, 36);
//            this.propertyCollectionView1.Name = "propertyCollectionView1";
//            this.propertyCollectionView1.TabIndex = 0;
//            // 
//            // flowPanelTop add controls
//            // 
//            this.flowPanelTop.Controls.Add(this.btnSave);
//            this.flowPanelTop.Controls.Add(this.btnReload);
//            // 
//            // EquipmentPropertyCollectionView
//            // 
//            this.Controls.Add(this.propertyCollectionView1);
//            this.Controls.Add(this.flowPanelTop);
//            this.Name = "EquipmentPropertyCollectionView";
//            this.Size = new System.Drawing.Size(600, 400);
//            this.flowPanelTop.ResumeLayout(false);
//            this.flowPanelTop.PerformLayout();
//            this.ResumeLayout(false);


//            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
//            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
//            this.btn_Save_Setup_Equipment = new QMC.Common.IndividualMenuButton();
//            this.EquipmentPropertyCollectionView = new QMC.Common.PropertyCollectionView();
//            this.btnLanguage = new QMC.Common.IndividualMenuButton();
//            this.tableLayoutPanel2.SuspendLayout();
//            this.tableLayoutPanel3.SuspendLayout();
//            this.SuspendLayout();
//            // 
//            // tableLayoutPanel2
//            // 
//            this.tableLayoutPanel2.ColumnCount = 1;
//            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
//            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 2);
//            this.tableLayoutPanel2.Controls.Add(this.EquipmentPropertyCollectionView, 0, 0);
//            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
//            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4);
//            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
//            this.tableLayoutPanel2.RowCount = 4;
//            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 74.86678F));
//            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.081705F));
//            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
//            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
//            this.tableLayoutPanel2.Size = new System.Drawing.Size(1896, 1126);
//            this.tableLayoutPanel2.TabIndex = 1;
//            // 
//            // tableLayoutPanel3
//            // 
//            this.tableLayoutPanel3.ColumnCount = 2;
//            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
//            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001F));
//            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
//            this.tableLayoutPanel3.Controls.Add(this.btnLanguage, 0, 0);
//            this.tableLayoutPanel3.Controls.Add(this.btn_Save_Setup_Equipment, 1, 0);
//            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 938);
//            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4);
//            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
//            this.tableLayoutPanel3.RowCount = 1;
//            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
//            this.tableLayoutPanel3.Size = new System.Drawing.Size(1888, 70);
//            this.tableLayoutPanel3.TabIndex = 17;
//            // 
//            // btn_Save_Setup_Equipment
//            // 
//            this.btn_Save_Setup_Equipment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
//            this.btn_Save_Setup_Equipment.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
//            this.btn_Save_Setup_Equipment.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
//            this.btn_Save_Setup_Equipment.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
//            this.btn_Save_Setup_Equipment.CustomForeColor = System.Drawing.Color.Black;
//            this.btn_Save_Setup_Equipment.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.btn_Save_Setup_Equipment.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
//            this.btn_Save_Setup_Equipment.ForeColor = System.Drawing.Color.Black;
//            this.btn_Save_Setup_Equipment.ImageSize = new System.Drawing.Size(45, 45);
//            this.btn_Save_Setup_Equipment.Location = new System.Drawing.Point(947, 4);
//            this.btn_Save_Setup_Equipment.Margin = new System.Windows.Forms.Padding(4);
//            this.btn_Save_Setup_Equipment.Name = "btn_Save_Setup_Equipment";
//            this.btn_Save_Setup_Equipment.Size = new System.Drawing.Size(937, 62);
//            this.btn_Save_Setup_Equipment.TabIndex = 5;
//            this.btn_Save_Setup_Equipment.TabStop = false;
//            this.btn_Save_Setup_Equipment.Text = "Save";
//            this.btn_Save_Setup_Equipment.UseVisualStyleBackColor = false;
//            // 
//            // EquipmentPropertyCollectionView
//            // 
//            this.EquipmentPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.EquipmentPropertyCollectionView.FastBuild = true;
//            this.EquipmentPropertyCollectionView.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
//            this.EquipmentPropertyCollectionView.GroupName = "Property";
//            this.EquipmentPropertyCollectionView.Location = new System.Drawing.Point(6, 6);
//            this.EquipmentPropertyCollectionView.Margin = new System.Windows.Forms.Padding(6);
//            this.EquipmentPropertyCollectionView.Name = "EquipmentPropertyCollectionView";
//            this.EquipmentPropertyCollectionView.Size = new System.Drawing.Size(1884, 831);
//            this.EquipmentPropertyCollectionView.SuppressResizeInvalidation = true;
//            this.EquipmentPropertyCollectionView.TabIndex = 13;
//            this.EquipmentPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 10F);
//            this.EquipmentPropertyCollectionView.TextBoxFontSize = 10F;
//            // 
//            // btnLanguage
//            // 
//            this.btnLanguage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
//            this.btnLanguage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
//            this.btnLanguage.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
//            this.btnLanguage.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
//            this.btnLanguage.CustomForeColor = System.Drawing.Color.Black;
//            this.btnLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
//            this.btnLanguage.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
//            this.btnLanguage.ForeColor = System.Drawing.Color.Black;
//            this.btnLanguage.ImageSize = new System.Drawing.Size(45, 45);
//            this.btnLanguage.Location = new System.Drawing.Point(4, 4);
//            this.btnLanguage.Margin = new System.Windows.Forms.Padding(4);
//            this.btnLanguage.Name = "btnLanguage";
//            this.btnLanguage.Size = new System.Drawing.Size(935, 62);
//            this.btnLanguage.TabIndex = 6;
//            this.btnLanguage.TabStop = false;
//            this.btnLanguage.Text = "Language";
//            this.btnLanguage.UseVisualStyleBackColor = false;
//            this.btnLanguage.Click += new System.EventHandler(this.btnLanguage_Click);
//            // 
//            // Equipment_Setup
//            // 
//            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
//            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
//            this.ClientSize = new System.Drawing.Size(1896, 1126);
//            this.Controls.Add(this.tableLayoutPanel2);
//            this.Name = "Equipment_Setup";
//            this.Text = "Equipment_Setup";
//            this.tableLayoutPanel2.ResumeLayout(false);
//            this.tableLayoutPanel3.ResumeLayout(false);
//            this.ResumeLayout(false);

//        }

//        #endregion


//    }
//}