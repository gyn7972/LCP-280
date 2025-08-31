using QMC.Common;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace QMC.LCP_280.Process.Unit
{
    partial class InputStageEjectorUnit_Working
    {
        private System.ComponentModel.IContainer components = null;

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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gbModuleUnit = new System.Windows.Forms.GroupBox();
            this.axisPositionsPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.infomationPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.outputIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.inputIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnManualAction = new QMC.Common.IndividualMenuButton();
            this.sequenceListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel1.SuspendLayout();
            this.gbModuleUnit.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.gbModuleUnit, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.infomationPropertyCollectionView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.gbDigitalIO, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 780);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // gbModuleUnit
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.gbModuleUnit, 2);
            this.gbModuleUnit.Controls.Add(this.axisPositionsPropertyCollectionView);
            this.gbModuleUnit.Location = new System.Drawing.Point(319, 3);
            this.gbModuleUnit.Name = "gbModuleUnit";
            this.gbModuleUnit.Size = new System.Drawing.Size(626, 384);
            this.gbModuleUnit.TabIndex = 6;
            this.gbModuleUnit.TabStop = false;
            this.gbModuleUnit.Text = "ModuleUnit";
            // 
            // axisPositionsPropertyCollectionView
            // 
            this.axisPositionsPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axisPositionsPropertyCollectionView.GroupName = "Axis Positions";
            this.axisPositionsPropertyCollectionView.Location = new System.Drawing.Point(3, 17);
            this.axisPositionsPropertyCollectionView.Name = "axisPositionsPropertyCollectionView";
            this.axisPositionsPropertyCollectionView.Size = new System.Drawing.Size(620, 364);
            this.axisPositionsPropertyCollectionView.TabIndex = 2;
            // 
            // infomationPropertyCollectionView
            // 
            this.infomationPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infomationPropertyCollectionView.GroupName = "Infomation";
            this.infomationPropertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.infomationPropertyCollectionView.Name = "infomationPropertyCollectionView";
            this.infomationPropertyCollectionView.Size = new System.Drawing.Size(310, 384);
            this.infomationPropertyCollectionView.TabIndex = 0;
            // 
            // gbDigitalIO
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.gbDigitalIO, 2);
            this.gbDigitalIO.Controls.Add(this.ioTableLayoutPanel);
            this.gbDigitalIO.Location = new System.Drawing.Point(319, 393);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(626, 384);
            this.gbDigitalIO.TabIndex = 5;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // ioTableLayoutPanel
            // 
            this.ioTableLayoutPanel.ColumnCount = 2;
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Controls.Add(this.outputIoPropertyCollectionView, 1, 0);
            this.ioTableLayoutPanel.Controls.Add(this.inputIoPropertyCollectionView, 0, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(3, 17);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(620, 364);
            this.ioTableLayoutPanel.TabIndex = 0;
            // 
            // outputIoPropertyCollectionView
            // 
            this.outputIoPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputIoPropertyCollectionView.GroupName = "Output";
            this.outputIoPropertyCollectionView.Location = new System.Drawing.Point(313, 3);
            this.outputIoPropertyCollectionView.Name = "outputIoPropertyCollectionView";
            this.outputIoPropertyCollectionView.Size = new System.Drawing.Size(304, 358);
            this.outputIoPropertyCollectionView.TabIndex = 4;
            // 
            // inputIoPropertyCollectionView
            // 
            this.inputIoPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputIoPropertyCollectionView.GroupName = "Input";
            this.inputIoPropertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.inputIoPropertyCollectionView.Name = "inputIoPropertyCollectionView";
            this.inputIoPropertyCollectionView.Size = new System.Drawing.Size(304, 358);
            this.inputIoPropertyCollectionView.TabIndex = 3;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnManualAction);
            this.panel1.Controls.Add(this.sequenceListBoxItemsView);
            this.panel1.Location = new System.Drawing.Point(3, 393);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(310, 384);
            this.panel1.TabIndex = 8;
            // 
            // btnManualAction
            // 
            this.btnManualAction.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnManualAction.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnManualAction.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnManualAction.CustomForeColor = System.Drawing.Color.Black;
            this.btnManualAction.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnManualAction.ForeColor = System.Drawing.Color.Black;
            this.btnManualAction.ImageSize = new System.Drawing.Size(45, 45);
            this.btnManualAction.Location = new System.Drawing.Point(203, 319);
            this.btnManualAction.Name = "btnManualAction";
            this.btnManualAction.Size = new System.Drawing.Size(104, 50);
            this.btnManualAction.TabIndex = 7;
            this.btnManualAction.TabStop = false;
            this.btnManualAction.Text = "Manual\r\nAction";
            this.btnManualAction.UseVisualStyleBackColor = false;
            // 
            // sequenceListBoxItemsView
            // 
            this.sequenceListBoxItemsView.BorderWidth = 2;
            this.sequenceListBoxItemsView.GroupName = "Sequence";
            this.sequenceListBoxItemsView.Location = new System.Drawing.Point(0, 3);
            this.sequenceListBoxItemsView.Name = "sequenceListBoxItemsView";
            this.sequenceListBoxItemsView.SelectedIndex = -1;
            this.sequenceListBoxItemsView.Size = new System.Drawing.Size(197, 378);
            this.sequenceListBoxItemsView.TabIndex = 1;
            // 
            // InputStageEjectorUnit_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "InputStageEjectorUnit_Working";
            this.Text = "Input Stage Ejector Unit Working";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.gbModuleUnit.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
        }
        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox gbModuleUnit;
        private QMC.Common.PropertyCollectionView axisPositionsPropertyCollectionView;
        private QMC.Common.PropertyCollectionView infomationPropertyCollectionView;
        private System.Windows.Forms.GroupBox gbDigitalIO;
        private System.Windows.Forms.TableLayoutPanel ioTableLayoutPanel;
        private QMC.Common.IOPropertyCollectionView inputIoPropertyCollectionView;
        private QMC.Common.IOPropertyCollectionView outputIoPropertyCollectionView;
        private System.Windows.Forms.Panel panel1;
        private QMC.Common.IndividualMenuButton btnManualAction;
        private QMC.Common.ListBoxItemsView sequenceListBoxItemsView;
    }
}
