using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common;
using QMC.Common.CustomControl;

namespace QMC.LCP_280.Process.Unit
{
    partial class DigitalIO_Setup
    {
        // UI Controls (디자이너 전용 보관)
        private ListBoxItemsView dioModuleListBoxItemsView;
        private IndividualMenuButton btn_Save_Setup_Ouput;
        private IndividualMenuButton btn_Save_Setup_Input;
        private IOPropertyCollectionView inputIOPropertyCollectionView;
        private IOPropertyCollectionView outputIOPropertyCollectionView;
        private PropertyCollectionView inputpropertyCollectionView;
        private PropertyCollectionView outputpropertyCollectionView;

        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.btn_Save_Setup_Input = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Setup_Ouput = new QMC.Common.IndividualMenuButton();
            this.dioModuleListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.inputIOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputIOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.inputpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.outputpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.SuspendLayout();
            // 
            // btn_Save_Setup_Input
            // 
            this.btn_Save_Setup_Input.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Input.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Input.Location = new System.Drawing.Point(672, 687);
            this.btn_Save_Setup_Input.Name = "btn_Save_Setup_Input";
            this.btn_Save_Setup_Input.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Input.TabIndex = 5;
            this.btn_Save_Setup_Input.TabStop = false;
            this.btn_Save_Setup_Input.Text = "Save";
            this.btn_Save_Setup_Input.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Input.Click += new System.EventHandler(this.btn_Save_Setup_Input_Property_Click);
            // 
            // btn_Save_Setup_Ouput
            // 
            this.btn_Save_Setup_Ouput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Ouput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Ouput.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Ouput.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Ouput.Location = new System.Drawing.Point(1141, 687);
            this.btn_Save_Setup_Ouput.Name = "btn_Save_Setup_Ouput";
            this.btn_Save_Setup_Ouput.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Ouput.TabIndex = 4;
            this.btn_Save_Setup_Ouput.TabStop = false;
            this.btn_Save_Setup_Ouput.Text = "Save";
            this.btn_Save_Setup_Ouput.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Ouput.Click += new System.EventHandler(this.btn_Save_Setup_Output_Property_Click);
            // 
            // dioModuleListBoxItemsView
            // 
            this.dioModuleListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dioModuleListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.dioModuleListBoxItemsView.BorderWidth = 2;
            this.dioModuleListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.dioModuleListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.GroupName = "DIO Module";
            this.dioModuleListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.dioModuleListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.dioModuleListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.dioModuleListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.dioModuleListBoxItemsView.Name = "dioModuleListBoxItemsView";
            this.dioModuleListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.dioModuleListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.SelectedIndex = -1;
            this.dioModuleListBoxItemsView.Size = new System.Drawing.Size(305, 722);
            this.dioModuleListBoxItemsView.TabIndex = 2;
            // 
            // inputIOPropertyCollectionView
            // 
            this.inputIOPropertyCollectionView.GroupName = "Digital Input";
            this.inputIOPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.inputIOPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.inputIOPropertyCollectionView.Location = new System.Drawing.Point(323, 12);
            this.inputIOPropertyCollectionView.Name = "inputIOPropertyCollectionView";
            this.inputIOPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.inputIOPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.inputIOPropertyCollectionView.Size = new System.Drawing.Size(460, 522);
            this.inputIOPropertyCollectionView.TabIndex = 11;
            // 
            // outputIOPropertyCollectionView
            // 
            this.outputIOPropertyCollectionView.GroupName = "Digital Output";
            this.outputIOPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.outputIOPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.outputIOPropertyCollectionView.Location = new System.Drawing.Point(792, 12);
            this.outputIOPropertyCollectionView.Name = "outputIOPropertyCollectionView";
            this.outputIOPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputIOPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputIOPropertyCollectionView.Size = new System.Drawing.Size(460, 522);
            this.outputIOPropertyCollectionView.TabIndex = 12;
            // 
            // inputpropertyCollectionView
            // 
            this.inputpropertyCollectionView.GroupName = "Property";
            this.inputpropertyCollectionView.Location = new System.Drawing.Point(333, 540);
            this.inputpropertyCollectionView.Name = "inputpropertyCollectionView";
            this.inputpropertyCollectionView.Size = new System.Drawing.Size(323, 187);
            this.inputpropertyCollectionView.TabIndex = 13;
            // 
            // outputpropertyCollectionView
            // 
            this.outputpropertyCollectionView.GroupName = "Property";
            this.outputpropertyCollectionView.Location = new System.Drawing.Point(802, 540);
            this.outputpropertyCollectionView.Name = "outputpropertyCollectionView";
            this.outputpropertyCollectionView.Size = new System.Drawing.Size(323, 187);
            this.outputpropertyCollectionView.TabIndex = 14;
            // 
            // DigitalIO_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.outputpropertyCollectionView);
            this.Controls.Add(this.btn_Save_Setup_Ouput);
            this.Controls.Add(this.btn_Save_Setup_Input);
            this.Controls.Add(this.inputpropertyCollectionView);
            this.Controls.Add(this.dioModuleListBoxItemsView);
            this.Controls.Add(this.inputIOPropertyCollectionView);
            this.Controls.Add(this.outputIOPropertyCollectionView);
            this.Name = "DigitalIO_Setup";
            this.Text = "Motion Setup";
            this.ResumeLayout(false);
        }
        #endregion
    }
}
