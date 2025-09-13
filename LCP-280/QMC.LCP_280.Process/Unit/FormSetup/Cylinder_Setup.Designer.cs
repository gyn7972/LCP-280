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
            this.gbCylinderState = new System.Windows.Forms.GroupBox();
            this.btn_Backward_Move = new QMC.Common.IndividualMenuButton();
            this.btn_Forward_Move = new QMC.Common.IndividualMenuButton();
            this.selectItemListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btn_Save_Setup_Cylinder = new QMC.Common.IndividualMenuButton();
            this.cylinderPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.lbStatusCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbStatusValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.inputStatepropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.gbCylinderControl.SuspendLayout();
            this.gbCylinderState.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbCylinderControl
            // 
            this.gbCylinderControl.BackColor = System.Drawing.Color.White;
            this.gbCylinderControl.Controls.Add(this.btn_Backward_Move);
            this.gbCylinderControl.Controls.Add(this.btn_Forward_Move);
            this.gbCylinderControl.Location = new System.Drawing.Point(333, 626);
            this.gbCylinderControl.Name = "gbCylinderControl";
            this.gbCylinderControl.Size = new System.Drawing.Size(440, 108);
            this.gbCylinderControl.TabIndex = 15;
            this.gbCylinderControl.TabStop = false;
            this.gbCylinderControl.Text = "Control";
            // 
            // gbCylinderState
            // 
            this.gbCylinderState.BackColor = System.Drawing.Color.White;
            this.gbCylinderState.Controls.Add(this.lbStatusCaption);
            this.gbCylinderState.Controls.Add(this.lbStatusValue);
            this.gbCylinderState.Controls.Add(this.inputStatepropertyCollectionView);
            this.gbCylinderState.Location = new System.Drawing.Point(333, 12);
            this.gbCylinderState.Name = "gbCylinderState";
            this.gbCylinderState.Size = new System.Drawing.Size(440, 215);
            this.gbCylinderState.TabIndex = 16;
            this.gbCylinderState.TabStop = false;
            this.gbCylinderState.Text = "State";
            // 
            // btn_Backward_Move
            // 
            this.btn_Backward_Move.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Backward_Move.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Backward_Move.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Backward_Move.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Backward_Move.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Backward_Move.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Backward_Move.ForeColor = System.Drawing.Color.Black;
            this.btn_Backward_Move.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Backward_Move.Location = new System.Drawing.Point(258, 40);
            this.btn_Backward_Move.Name = "btn_Backward_Move";
            this.btn_Backward_Move.Size = new System.Drawing.Size(172, 48);
            this.btn_Backward_Move.TabIndex = 1;
            this.btn_Backward_Move.TabStop = false;
            this.btn_Backward_Move.Text = "Backward Move";
            this.btn_Backward_Move.UseVisualStyleBackColor = false;
            this.btn_Backward_Move.Click += new System.EventHandler(this.btn_Backward_Move_Click);
            // 
            // btn_Forward_Move
            // 
            this.btn_Forward_Move.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Forward_Move.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Forward_Move.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Forward_Move.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Forward_Move.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Forward_Move.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Forward_Move.ForeColor = System.Drawing.Color.Black;
            this.btn_Forward_Move.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Forward_Move.Location = new System.Drawing.Point(11, 40);
            this.btn_Forward_Move.Name = "btn_Forward_Move";
            this.btn_Forward_Move.Size = new System.Drawing.Size(172, 48);
            this.btn_Forward_Move.TabIndex = 0;
            this.btn_Forward_Move.TabStop = false;
            this.btn_Forward_Move.Text = "Forward Move";
            this.btn_Forward_Move.UseVisualStyleBackColor = false;
            this.btn_Forward_Move.Click += new System.EventHandler(this.btn_Forward_Move_Click);
            // 
            // selectItemListBoxItemsView
            // 
            this.selectItemListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectItemListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.selectItemListBoxItemsView.BorderWidth = 2;
            this.selectItemListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.selectItemListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.GroupName = "Select Item";
            this.selectItemListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.selectItemListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.selectItemListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.selectItemListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.selectItemListBoxItemsView.Name = "selectItemListBoxItemsView";
            this.selectItemListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.selectItemListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.selectItemListBoxItemsView.SelectedIndex = -1;
            this.selectItemListBoxItemsView.Size = new System.Drawing.Size(305, 722);
            this.selectItemListBoxItemsView.TabIndex = 2;
            // 
            // btn_Save_Setup_Cylinder
            // 
            this.btn_Save_Setup_Cylinder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Cylinder.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Cylinder.Location = new System.Drawing.Point(663, 570);
            this.btn_Save_Setup_Cylinder.Name = "btn_Save_Setup_Cylinder";
            this.btn_Save_Setup_Cylinder.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Cylinder.TabIndex = 5;
            this.btn_Save_Setup_Cylinder.TabStop = false;
            this.btn_Save_Setup_Cylinder.Text = "Save";
            this.btn_Save_Setup_Cylinder.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Cylinder.Click += new System.EventHandler(this.btn_Save_Setup_Cylinder_Click);
            // 
            // cylinderPropertyCollectionView
            // 
            this.cylinderPropertyCollectionView.FastBuild = true;
            this.cylinderPropertyCollectionView.GroupName = "Property";
            this.cylinderPropertyCollectionView.Location = new System.Drawing.Point(333, 233);
            this.cylinderPropertyCollectionView.Name = "cylinderPropertyCollectionView";
            this.cylinderPropertyCollectionView.Size = new System.Drawing.Size(440, 331);
            this.cylinderPropertyCollectionView.SuppressResizeInvalidation = true;
            this.cylinderPropertyCollectionView.TabIndex = 13;
            // 
            // lbStatusCaption
            // 
            this.lbStatusCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbStatusCaption.BorderWidth = 1;
            this.lbStatusCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbStatusCaption.Location = new System.Drawing.Point(8, 23);
            this.lbStatusCaption.Name = "lbStatusCaption";
            this.lbStatusCaption.Size = new System.Drawing.Size(90, 35);
            this.lbStatusCaption.TabIndex = 15;
            this.lbStatusCaption.Text = "Status";
            this.lbStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStatusValue
            // 
            this.lbStatusValue.BackColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderWidth = 1;
            this.lbStatusValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbStatusValue.ForeColor = System.Drawing.Color.Lime;
            this.lbStatusValue.Location = new System.Drawing.Point(104, 23);
            this.lbStatusValue.Name = "lbStatusValue";
            this.lbStatusValue.Size = new System.Drawing.Size(327, 35);
            this.lbStatusValue.TabIndex = 16;
            this.lbStatusValue.Text = "Forward";
            this.lbStatusValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // inputStatepropertyCollectionView
            // 
            this.inputStatepropertyCollectionView.FastBuild = true;
            this.inputStatepropertyCollectionView.GroupName = "Input State";
            this.inputStatepropertyCollectionView.Location = new System.Drawing.Point(9, 68);
            this.inputStatepropertyCollectionView.Name = "inputStatepropertyCollectionView";
            this.inputStatepropertyCollectionView.Size = new System.Drawing.Size(422, 141);
            this.inputStatepropertyCollectionView.SuppressResizeInvalidation = true;
            this.inputStatepropertyCollectionView.TabIndex = 14;
            // 
            // Cylinder_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.gbCylinderControl);
            this.Controls.Add(this.selectItemListBoxItemsView);
            this.Controls.Add(this.btn_Save_Setup_Cylinder);
            this.Controls.Add(this.cylinderPropertyCollectionView);
            this.Controls.Add(this.gbCylinderState);
            this.Name = "Cylinder_Setup";
            this.Text = "Motion Setup";
            this.gbCylinderControl.ResumeLayout(false);
            this.gbCylinderState.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
