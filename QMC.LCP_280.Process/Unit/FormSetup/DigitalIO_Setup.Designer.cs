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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Save_Setup_Input
            // 
            this.btn_Save_Setup_Input.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Input.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Setup_Input.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Input.Location = new System.Drawing.Point(3, 695);
            this.btn_Save_Setup_Input.Name = "btn_Save_Setup_Input";
            this.btn_Save_Setup_Input.Size = new System.Drawing.Size(409, 47);
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
            this.btn_Save_Setup_Ouput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Setup_Ouput.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Ouput.Location = new System.Drawing.Point(3, 695);
            this.btn_Save_Setup_Ouput.Name = "btn_Save_Setup_Ouput";
            this.btn_Save_Setup_Ouput.Size = new System.Drawing.Size(410, 47);
            this.btn_Save_Setup_Ouput.TabIndex = 4;
            this.btn_Save_Setup_Ouput.TabStop = false;
            this.btn_Save_Setup_Ouput.Text = "Save";
            this.btn_Save_Setup_Ouput.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Ouput.Click += new System.EventHandler(this.btn_Save_Setup_Output_Property_Click);
            // 
            // dioModuleListBoxItemsView
            // 
            this.dioModuleListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.dioModuleListBoxItemsView.BorderWidth = 2;
            this.dioModuleListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dioModuleListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.dioModuleListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.GroupName = "DIO Module";
            this.dioModuleListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.dioModuleListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.dioModuleListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.dioModuleListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.dioModuleListBoxItemsView.Name = "dioModuleListBoxItemsView";
            this.dioModuleListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.dioModuleListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.dioModuleListBoxItemsView.SelectedIndex = -1;
            this.dioModuleListBoxItemsView.Size = new System.Drawing.Size(415, 739);
            this.dioModuleListBoxItemsView.TabIndex = 2;
            this.dioModuleListBoxItemsView.Load += new System.EventHandler(this.dioModuleListBoxItemsView_Load);
            // 
            // inputIOPropertyCollectionView
            // 
            this.inputIOPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputIOPropertyCollectionView.FastBuild = true;
            this.inputIOPropertyCollectionView.FastInitialPaint = true;
            this.inputIOPropertyCollectionView.GroupName = "Digital Input";
            this.inputIOPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.inputIOPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.inputIOPropertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.inputIOPropertyCollectionView.Name = "inputIOPropertyCollectionView";
            this.inputIOPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.inputIOPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.inputIOPropertyCollectionView.Size = new System.Drawing.Size(409, 515);
            this.inputIOPropertyCollectionView.SuppressResizeInvalidation = true;
            this.inputIOPropertyCollectionView.TabIndex = 11;
            // 
            // outputIOPropertyCollectionView
            // 
            this.outputIOPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputIOPropertyCollectionView.FastBuild = true;
            this.outputIOPropertyCollectionView.FastInitialPaint = true;
            this.outputIOPropertyCollectionView.GroupName = "Digital Output";
            this.outputIOPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.outputIOPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.outputIOPropertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.outputIOPropertyCollectionView.Name = "outputIOPropertyCollectionView";
            this.outputIOPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputIOPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputIOPropertyCollectionView.Size = new System.Drawing.Size(410, 515);
            this.outputIOPropertyCollectionView.SuppressResizeInvalidation = true;
            this.outputIOPropertyCollectionView.TabIndex = 12;
            // 
            // inputpropertyCollectionView
            // 
            this.inputpropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputpropertyCollectionView.FastBuild = true;
            this.inputpropertyCollectionView.GroupName = "Property";
            this.inputpropertyCollectionView.Location = new System.Drawing.Point(3, 524);
            this.inputpropertyCollectionView.Name = "inputpropertyCollectionView";
            this.inputpropertyCollectionView.Size = new System.Drawing.Size(409, 165);
            this.inputpropertyCollectionView.SuppressResizeInvalidation = true;
            this.inputpropertyCollectionView.TabIndex = 13;
            // 
            // outputpropertyCollectionView
            // 
            this.outputpropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputpropertyCollectionView.FastBuild = true;
            this.outputpropertyCollectionView.GroupName = "Property";
            this.outputpropertyCollectionView.Location = new System.Drawing.Point(3, 524);
            this.outputpropertyCollectionView.Name = "outputpropertyCollectionView";
            this.outputpropertyCollectionView.Size = new System.Drawing.Size(410, 165);
            this.outputpropertyCollectionView.SuppressResizeInvalidation = true;
            this.outputpropertyCollectionView.TabIndex = 14;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.dioModuleListBoxItemsView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 15;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.btn_Save_Setup_Ouput, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.outputpropertyCollectionView, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.outputIOPropertyCollectionView, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(845, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(416, 745);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.btn_Save_Setup_Input, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.inputpropertyCollectionView, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.inputIOPropertyCollectionView, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(424, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(415, 745);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // DigitalIO_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DigitalIO_Setup";
            this.Text = "Motion Setup";
            this.Load += new System.EventHandler(this.DigitalIO_Setup_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel3;
    }
}
