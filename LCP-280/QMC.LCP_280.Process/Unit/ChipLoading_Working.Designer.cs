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
    partial class ChipLoading_Working
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
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.gbModuleUnit = new System.Windows.Forms.GroupBox();
            this.axisPositionsView = new QMC.Common.PropertyCollectionView();
            this.infomationView = new QMC.Common.PropertyCollectionView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.outputIoView = new QMC.Common.IOPropertyCollectionView();
            this.inputIoView = new QMC.Common.IOPropertyCollectionView();
            this.sequencePanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel.SuspendLayout();
            this.gbModuleUnit.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
            this.sequencePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel.Controls.Add(this.gbModuleUnit, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.infomationView, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.gbDigitalIO, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.sequencePanel, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(1264, 780);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // gbModuleUnit
            // 
            this.tableLayoutPanel.SetColumnSpan(this.gbModuleUnit, 2);
            this.gbModuleUnit.Controls.Add(this.axisPositionsView);
            this.gbModuleUnit.Location = new System.Drawing.Point(319, 3);
            this.gbModuleUnit.Name = "gbModuleUnit";
            this.gbModuleUnit.Size = new System.Drawing.Size(626, 384);
            this.gbModuleUnit.TabIndex = 6;
            this.gbModuleUnit.TabStop = false;
            this.gbModuleUnit.Text = "ModuleUnit";
            // 
            // axisPositionsView
            // 
            this.axisPositionsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axisPositionsView.GroupName = "Axis Positions";
            this.axisPositionsView.Location = new System.Drawing.Point(3, 17);
            this.axisPositionsView.Name = "axisPositionsView";
            this.axisPositionsView.Size = new System.Drawing.Size(620, 364);
            this.axisPositionsView.TabIndex = 2;
            // 
            // infomationView
            // 
            this.infomationView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.infomationView.GroupName = "Infomation";
            this.infomationView.Location = new System.Drawing.Point(3, 3);
            this.infomationView.Name = "infomationView";
            this.infomationView.Size = new System.Drawing.Size(310, 384);
            this.infomationView.TabIndex = 0;
            // 
            // gbDigitalIO
            // 
            this.tableLayoutPanel.SetColumnSpan(this.gbDigitalIO, 2);
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
            this.ioTableLayoutPanel.Controls.Add(this.outputIoView, 1, 0);
            this.ioTableLayoutPanel.Controls.Add(this.inputIoView, 0, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(3, 17);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(620, 364);
            this.ioTableLayoutPanel.TabIndex = 0;
            // 
            // outputIoView
            // 
            this.outputIoView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputIoView.GroupName = "Output";
            this.outputIoView.Location = new System.Drawing.Point(313, 3);
            this.outputIoView.Name = "outputIoView";
            this.outputIoView.Size = new System.Drawing.Size(304, 358);
            this.outputIoView.TabIndex = 4;
            // 
            // inputIoView
            // 
            this.inputIoView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputIoView.GroupName = "Input";
            this.inputIoView.Location = new System.Drawing.Point(3, 3);
            this.inputIoView.Name = "inputIoView";
            this.inputIoView.Size = new System.Drawing.Size(304, 358);
            this.inputIoView.TabIndex = 3;
            // 
            // sequencePanel
            // 
            this.sequencePanel.Controls.Add(null);
            this.sequencePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequencePanel.Location = new System.Drawing.Point(3, 393);
            this.sequencePanel.Name = "sequencePanel";
            this.sequencePanel.Size = new System.Drawing.Size(310, 384);
            this.sequencePanel.TabIndex = 8;
            // 
            // inputStageSequenceControl
            //
            // 
            // InputStageUnit_Working
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "InputStageUnit_Working";
            this.Text = "InputCassetteLifter Unit Configuration";
            this.tableLayoutPanel.ResumeLayout(false);
            this.gbModuleUnit.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.sequencePanel.ResumeLayout(false);
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

        private TableLayoutPanel tableLayoutPanel;
        private PropertyCollectionView infomationView;
        private ListBoxItemsView sequenceView;
        private PropertyCollectionView axisPositionsView;
        private IOPropertyCollectionView inputIoView;
        private IOPropertyCollectionView outputIoView;
        private GroupBox gbDigitalIO;
        private GroupBox gbModuleUnit;
        private TableLayoutPanel ioTableLayoutPanel;
        private Panel sequencePanel;
    }
}