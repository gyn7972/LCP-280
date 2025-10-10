using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.DIO; // 추가
using QMC.Common.IO;  // DIOUnit, DIOModuleSetup
using QMC.Common.Motions;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class IndexUnloadAlignerUnit_Config
    {
        private ListBoxItemsView axisListBoxItemsView;  // Axis 목록 UI 컨트롤
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbMoveAxis;
        private JogControl jogControl;
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel mainTableLayoutPanel;
        private Panel positionItemPanel;
        private QMC.LCP_280.Process.Unit.FormConfig.PositionTeachingControl positionTeachingControl;
        private QMC.LCP_280.Process.Unit.FormConfig.DigitalIOControl digitalIOControl;

        /// <summary>
        ///  Clean up any resources being used.
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

        private void InitializeComponent()
        {
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionTeachingControl = new QMC.LCP_280.Process.Unit.FormConfig.PositionTeachingControl();
            this.digitalIOControl = new QMC.LCP_280.Process.Unit.FormConfig.DigitalIOControl();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.unitConfigControl = new QMC.LCP_280.Process.Component.UnitConfig();
            this.positionItemPanel = new System.Windows.Forms.Panel();
            this.gbPositionTeaching.SuspendLayout();
            this.gbMoveAxis.SuspendLayout();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbPositionTeaching, 2);
            this.gbPositionTeaching.Controls.Add(this.positionTeachingControl);
            this.gbPositionTeaching.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbPositionTeaching.Location = new System.Drawing.Point(3, 3);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Size = new System.Drawing.Size(626, 384);
            this.gbPositionTeaching.TabIndex = 8;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Position Teaching";
            // 
            // positionTeachingControl
            // 
            this.positionTeachingControl.BackColor = System.Drawing.Color.White;
            this.positionTeachingControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionTeachingControl.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.positionTeachingControl.Location = new System.Drawing.Point(3, 21);
            this.positionTeachingControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.positionTeachingControl.Name = "positionTeachingControl";
            this.positionTeachingControl.Size = new System.Drawing.Size(620, 360);
            this.positionTeachingControl.TabIndex = 8;
            this.positionTeachingControl.TabStop = false;
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.Controls.Add(this.digitalIOControl);
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbDigitalIO, 2);
            this.gbDigitalIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbDigitalIO.Location = new System.Drawing.Point(3, 393);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(626, 384);
            this.gbDigitalIO.TabIndex = 9;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // digitalIOControl
            // 
            this.digitalIOControl.BackColor = System.Drawing.Color.White;
            this.digitalIOControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.digitalIOControl.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.digitalIOControl.Location = new System.Drawing.Point(3, 21);
            this.digitalIOControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.digitalIOControl.Name = "digitalIOControl";
            this.digitalIOControl.Size = new System.Drawing.Size(620, 360);
            this.digitalIOControl.TabIndex = 8;
            this.digitalIOControl.TabStop = false;
            // 
            // gbMoveAxis
            // 
            this.gbMoveAxis.BackColor = System.Drawing.Color.White;
            this.gbMoveAxis.Controls.Add(this.jogControl);
            this.gbMoveAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(635, 3);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.mainTableLayoutPanel.SetRowSpan(this.gbMoveAxis, 2);
            this.gbMoveAxis.Size = new System.Drawing.Size(310, 774);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";
            // 
            // jogControl
            // 
            this.jogControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jogControl.Location = new System.Drawing.Point(3, 21);
            this.jogControl.Margin = new System.Windows.Forms.Padding(0);
            this.jogControl.Name = "jogControl";
            this.jogControl.Size = new System.Drawing.Size(304, 750);
            this.jogControl.TabIndex = 0;
            // 
            // axisListBoxItemsView
            // 
            this.axisListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.axisListBoxItemsView.BorderWidth = 2;
            this.axisListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.axisListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.GroupName = "";
            this.axisListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.axisListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.axisListBoxItemsView.Location = new System.Drawing.Point(8, 18);
            this.axisListBoxItemsView.Name = "axisListBoxItemsView";
            this.axisListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.axisListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.SelectedIndex = -1;
            this.axisListBoxItemsView.Size = new System.Drawing.Size(234, 124);
            this.axisListBoxItemsView.TabIndex = 0;
            this.axisListBoxItemsView.ItemSelected += new System.EventHandler<int>(this.OnAxisSelected);
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 4;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.Controls.Add(this.unitConfigControl, 3, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbDigitalIO, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.gbPositionTeaching, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbMoveAxis, 2, 0);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(1264, 780);
            this.mainTableLayoutPanel.TabIndex = 12;
            // 
            // unitConfigControl
            // 
            this.unitConfigControl.AutoReloadOnActivate = true;
            this.unitConfigControl.BackColor = System.Drawing.Color.White;
            this.unitConfigControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.unitConfigControl.Location = new System.Drawing.Point(948, 0);
            this.unitConfigControl.Margin = new System.Windows.Forms.Padding(0);
            this.unitConfigControl.Name = "unitConfigControl";
            this.mainTableLayoutPanel.SetRowSpan(this.unitConfigControl, 2);
            this.unitConfigControl.Size = new System.Drawing.Size(316, 780);
            this.unitConfigControl.TabIndex = 12;
            // 
            // positionItemPanel
            // 
            this.positionItemPanel.Location = new System.Drawing.Point(0, 0);
            this.positionItemPanel.Name = "positionItemPanel";
            this.positionItemPanel.Size = new System.Drawing.Size(200, 100);
            this.positionItemPanel.TabIndex = 0;
            // 
            // IndexUnloadAlignerUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "IndexUnloadAlignerUnit_Config";
            this.Text = "IndexUnloadAligner Unit Configuration";
            this.gbPositionTeaching.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private UnitConfig unitConfigControl;
    }
}