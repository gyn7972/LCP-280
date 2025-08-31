using QMC.Common;
using QMC.Common.CustomControl;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class InputCassetteLifterUnit_Working
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>


        private System.ComponentModel.IContainer components = null;

        // Actual Position 주기 업데이트 타이머
        private Timer _axisPosTimer;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.propertyCollectionView2 = new QMC.Common.PropertyCollectionView();
            this.propertyCollectionView1 = new QMC.Common.PropertyCollectionView();
            this.listBoxItemsView1 = new QMC.Common.ListBoxItemsView();
            this.ioPropertyCollectionView1 = new QMC.Common.IOPropertyCollectionView();
            this.ioPropertyCollectionView2 = new QMC.Common.IOPropertyCollectionView();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.propertyCollectionView2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.propertyCollectionView1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.listBoxItemsView1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.ioPropertyCollectionView1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.ioPropertyCollectionView2, 2, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 780);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // propertyCollectionView2
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.propertyCollectionView2, 2);
            this.propertyCollectionView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyCollectionView2.GroupName = "Property Group";
            this.propertyCollectionView2.Location = new System.Drawing.Point(319, 3);
            this.propertyCollectionView2.Name = "propertyCollectionView2";
            this.propertyCollectionView2.Size = new System.Drawing.Size(626, 384);
            this.propertyCollectionView2.TabIndex = 2;
            // 
            // propertyCollectionView1
            // 
            this.propertyCollectionView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyCollectionView1.GroupName = "Property Group";
            this.propertyCollectionView1.Location = new System.Drawing.Point(3, 3);
            this.propertyCollectionView1.Name = "propertyCollectionView1";
            this.propertyCollectionView1.Size = new System.Drawing.Size(310, 384);
            this.propertyCollectionView1.TabIndex = 0;
            // 
            // listBoxItemsView1
            // 
            this.listBoxItemsView1.BorderWidth = 2;
            this.listBoxItemsView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxItemsView1.GroupName = "Group Title";
            this.listBoxItemsView1.Location = new System.Drawing.Point(3, 393);
            this.listBoxItemsView1.Name = "listBoxItemsView1";
            this.listBoxItemsView1.SelectedIndex = -1;
            this.listBoxItemsView1.Size = new System.Drawing.Size(310, 384);
            this.listBoxItemsView1.TabIndex = 1;
            // 
            // ioPropertyCollectionView1
            // 
            this.ioPropertyCollectionView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioPropertyCollectionView1.GroupName = "IO Property Group";
            this.ioPropertyCollectionView1.Location = new System.Drawing.Point(319, 393);
            this.ioPropertyCollectionView1.Name = "ioPropertyCollectionView1";
            this.ioPropertyCollectionView1.Size = new System.Drawing.Size(310, 384);
            this.ioPropertyCollectionView1.TabIndex = 3;
            // 
            // ioPropertyCollectionView2
            // 
            this.ioPropertyCollectionView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioPropertyCollectionView2.GroupName = "IO Property Group";
            this.ioPropertyCollectionView2.Location = new System.Drawing.Point(635, 393);
            this.ioPropertyCollectionView2.Name = "ioPropertyCollectionView2";
            this.ioPropertyCollectionView2.Size = new System.Drawing.Size(310, 384);
            this.ioPropertyCollectionView2.TabIndex = 4;
            // 
            // InputCassetteLifterUnit_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "InputCassetteLifterUnit_Working";
            this.Text = "InputCassetteLifter Unit Configuration";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private void InitializeUI()
        {
            try
            {
            }
            catch (Exception ex)
            {

            }
        }
        /// <summary>
        /// CassetteElevator + WaferTransferArm 의 AxisDefinition DisplayName 을 axisListBoxItemsView 에 설정
        /// </summary>

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        private TableLayoutPanel tableLayoutPanel1;
        private PropertyCollectionView propertyCollectionView1;
        private ListBoxItemsView listBoxItemsView1;
        private PropertyCollectionView propertyCollectionView2;
        private IOPropertyCollectionView ioPropertyCollectionView1;
        private IOPropertyCollectionView ioPropertyCollectionView2;
    }
}