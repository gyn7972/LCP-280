using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.Motions;
using QMC.LCP_280.Process.Unit;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class Cylinder_Setup
    {
        private IContainer components = null;

        private ListBoxItemsView selectItemListBoxItemsView;
        private PropertyCollectionView cylinderPropertyCollectionView;
        private IndividualMenuButton btn_Save_Setup_Cylinder;
        private PropertyCollectionView inputStatepropertyCollectionView;
        private GroupBox gbCylinderControl;
        private IndividualMenuButton btn_Backward_Move;
        private IndividualMenuButton btn_Forward_Move;
        private GroupBox gbCylinderState;
        private CustomBorderLabel lbStatusCaption;
        private CustomBorderLabel lbStatusValue;

        private void InitializeComponent()
        {
            this.gbCylinderControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_Forward_Move = new QMC.Common.IndividualMenuButton();
            this.btn_Backward_Move = new QMC.Common.IndividualMenuButton();
            this.gbCylinderState = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.lbStatusValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbStatusCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.inputStatepropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.selectItemListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btn_Save_Setup_Cylinder = new QMC.Common.IndividualMenuButton();
            this.cylinderPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.gbCylinderControl.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.gbCylinderState.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbCylinderControl
            // 
            this.gbCylinderControl.BackColor = System.Drawing.Color.White;
            this.gbCylinderControl.Controls.Add(this.tableLayoutPanel4);
            this.gbCylinderControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCylinderControl.Location = new System.Drawing.Point(4, 1009);
            this.gbCylinderControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbCylinderControl.Name = "gbCylinderControl";
            this.gbCylinderControl.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbCylinderControl.Size = new System.Drawing.Size(1312, 105);
            this.gbCylinderControl.TabIndex = 15;
            this.gbCylinderControl.TabStop = false;
            this.gbCylinderControl.Text = "Control";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.btn_Forward_Move, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btn_Backward_Move, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(4, 25);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1304, 76);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // btn_Forward_Move
            // 
            this.btn_Forward_Move.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Forward_Move.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Forward_Move.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Forward_Move.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Forward_Move.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Forward_Move.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Forward_Move.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Forward_Move.ForeColor = System.Drawing.Color.Black;
            this.btn_Forward_Move.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Forward_Move.Location = new System.Drawing.Point(4, 4);
            this.btn_Forward_Move.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Forward_Move.Name = "btn_Forward_Move";
            this.btn_Forward_Move.Size = new System.Drawing.Size(644, 68);
            this.btn_Forward_Move.TabIndex = 0;
            this.btn_Forward_Move.TabStop = false;
            this.btn_Forward_Move.Text = "Forward Move";
            this.btn_Forward_Move.UseVisualStyleBackColor = false;
            this.btn_Forward_Move.Click += new System.EventHandler(this.btn_Forward_Move_Click);
            // 
            // btn_Backward_Move
            // 
            this.btn_Backward_Move.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Backward_Move.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Backward_Move.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Backward_Move.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Backward_Move.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Backward_Move.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Backward_Move.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Backward_Move.ForeColor = System.Drawing.Color.Black;
            this.btn_Backward_Move.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Backward_Move.Location = new System.Drawing.Point(656, 4);
            this.btn_Backward_Move.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Backward_Move.Name = "btn_Backward_Move";
            this.btn_Backward_Move.Size = new System.Drawing.Size(644, 68);
            this.btn_Backward_Move.TabIndex = 1;
            this.btn_Backward_Move.TabStop = false;
            this.btn_Backward_Move.Text = "Backward Move";
            this.btn_Backward_Move.UseVisualStyleBackColor = false;
            this.btn_Backward_Move.Click += new System.EventHandler(this.btn_Backward_Move_Click);
            // 
            // gbCylinderState
            // 
            this.gbCylinderState.BackColor = System.Drawing.Color.White;
            this.gbCylinderState.Controls.Add(this.tableLayoutPanel5);
            this.gbCylinderState.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCylinderState.Location = new System.Drawing.Point(4, 4);
            this.gbCylinderState.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbCylinderState.Name = "gbCylinderState";
            this.gbCylinderState.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbCylinderState.Size = new System.Drawing.Size(1312, 439);
            this.gbCylinderState.TabIndex = 16;
            this.gbCylinderState.TabStop = false;
            this.gbCylinderState.Text = "State";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel6, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.inputStatepropertyCollectionView, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(4, 25);
            this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1304, 410);
            this.tableLayoutPanel5.TabIndex = 17;
            this.tableLayoutPanel5.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel5_Paint);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel6.Controls.Add(this.lbStatusValue, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.lbStatusCaption, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1296, 74);
            this.tableLayoutPanel6.TabIndex = 0;
            // 
            // lbStatusValue
            // 
            this.lbStatusValue.BackColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderWidth = 1;
            this.lbStatusValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStatusValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbStatusValue.ForeColor = System.Drawing.Color.Lime;
            this.lbStatusValue.Location = new System.Drawing.Point(392, 0);
            this.lbStatusValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbStatusValue.Name = "lbStatusValue";
            this.lbStatusValue.Size = new System.Drawing.Size(900, 74);
            this.lbStatusValue.TabIndex = 16;
            this.lbStatusValue.Text = "Forward";
            this.lbStatusValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbStatusCaption
            // 
            this.lbStatusCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbStatusCaption.BorderWidth = 1;
            this.lbStatusCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStatusCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbStatusCaption.Location = new System.Drawing.Point(4, 0);
            this.lbStatusCaption.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbStatusCaption.Name = "lbStatusCaption";
            this.lbStatusCaption.Size = new System.Drawing.Size(380, 74);
            this.lbStatusCaption.TabIndex = 15;
            this.lbStatusCaption.Text = "Status";
            this.lbStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // inputStatepropertyCollectionView
            // 
            this.inputStatepropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputStatepropertyCollectionView.FastBuild = true;
            this.inputStatepropertyCollectionView.GroupName = "Input State";
            this.inputStatepropertyCollectionView.Location = new System.Drawing.Point(6, 88);
            this.inputStatepropertyCollectionView.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.inputStatepropertyCollectionView.Name = "inputStatepropertyCollectionView";
            this.inputStatepropertyCollectionView.Size = new System.Drawing.Size(1292, 316);
            this.inputStatepropertyCollectionView.SuppressResizeInvalidation = true;
            this.inputStatepropertyCollectionView.TabIndex = 14;
            this.inputStatepropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 10F);
            this.inputStatepropertyCollectionView.TextBoxFontSize = 10F;
            // 
            // selectItemListBoxItemsView
            // 
            this.selectItemListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.selectItemListBoxItemsView.BorderWidth = 2;
            this.selectItemListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectItemListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.selectItemListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.GroupName = "Select Item";
            this.selectItemListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.selectItemListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.selectItemListBoxItemsView.Location = new System.Drawing.Point(4, 9);
            this.selectItemListBoxItemsView.Margin = new System.Windows.Forms.Padding(4, 9, 4, 9);
            this.selectItemListBoxItemsView.Name = "selectItemListBoxItemsView";
            this.selectItemListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.selectItemListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.SelectedIndex = -1;
            this.selectItemListBoxItemsView.Size = new System.Drawing.Size(560, 1108);
            this.selectItemListBoxItemsView.TabIndex = 2;
            // 
            // btn_Save_Setup_Cylinder
            // 
            this.btn_Save_Setup_Cylinder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Cylinder.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Setup_Cylinder.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Cylinder.Location = new System.Drawing.Point(659, 4);
            this.btn_Save_Setup_Cylinder.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Save_Setup_Cylinder.Name = "btn_Save_Setup_Cylinder";
            this.btn_Save_Setup_Cylinder.Size = new System.Drawing.Size(649, 62);
            this.btn_Save_Setup_Cylinder.TabIndex = 5;
            this.btn_Save_Setup_Cylinder.TabStop = false;
            this.btn_Save_Setup_Cylinder.Text = "Save";
            this.btn_Save_Setup_Cylinder.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Cylinder.Click += new System.EventHandler(this.btn_Save_Setup_Cylinder_Click);
            // 
            // cylinderPropertyCollectionView
            // 
            this.cylinderPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cylinderPropertyCollectionView.FastBuild = true;
            this.cylinderPropertyCollectionView.GroupName = "Property";
            this.cylinderPropertyCollectionView.Location = new System.Drawing.Point(6, 453);
            this.cylinderPropertyCollectionView.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.cylinderPropertyCollectionView.Name = "cylinderPropertyCollectionView";
            this.cylinderPropertyCollectionView.Size = new System.Drawing.Size(1308, 468);
            this.cylinderPropertyCollectionView.SuppressResizeInvalidation = true;
            this.cylinderPropertyCollectionView.TabIndex = 13;
            this.cylinderPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 10F);
            this.cylinderPropertyCollectionView.TextBoxFontSize = 10F;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.selectItemListBoxItemsView, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1896, 1126);
            this.tableLayoutPanel1.TabIndex = 17;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.gbCylinderState, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.gbCylinderControl, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.cylinderPropertyCollectionView, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(572, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 43F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1320, 1118);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel3.Controls.Add(this.btn_Save_Setup_Cylinder, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 931);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1312, 70);
            this.tableLayoutPanel3.TabIndex = 17;
            // 
            // Cylinder_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1896, 1126);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Cylinder_Setup";
            this.gbCylinderControl.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.gbCylinderState.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
    }
}
