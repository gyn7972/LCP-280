using QMC.Common;
using QMC.Common.CustomControl;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class GageRnRUnit_Config
    {
        private ListBoxItemsView axisListBoxItemsView;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbMoveAxis;
        private JogControl jogControl;
        private IContainer components = null;
        private TableLayoutPanel mainTableLayoutPanel;
        private Panel positionItemPanel;
        private QMC.LCP_280.Process.Unit.FormConfig.PositionTeachingControl positionTeachingControl;
        private QMC.LCP_280.Process.Unit.FormConfig.DigitalIOControl digitalIOControl;

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
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.unitConfigControl = new QMC.LCP_280.Process.Component.UnitConfig();
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionTeachingControl = new QMC.LCP_280.Process.Unit.FormConfig.PositionTeachingControl();
            this.digitalIOControl = new QMC.LCP_280.Process.Unit.FormConfig.DigitalIOControl();
            this.positionItemPanel = new System.Windows.Forms.Panel();
            this.gbMoveAxis.SuspendLayout();
            this.mainTableLayoutPanel.SuspendLayout();
            this.gbPositionTeaching.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.Controls.Add(this.digitalIOControl);
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
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
            this.digitalIOControl.Location = new System.Drawing.Point(2, 16);
            this.digitalIOControl.Margin = new System.Windows.Forms.Padding(2);
            this.digitalIOControl.Name = "digitalIOControl";
            this.digitalIOControl.Size = new System.Drawing.Size(624, 368);
            this.digitalIOControl.TabIndex = 0;
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
            this.mainTableLayoutPanel.ColumnCount = 3;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.mainTableLayoutPanel.Controls.Add(this.unitConfigControl, 2, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbMoveAxis, 1, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbDigitalIO, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.gbPositionTeaching, 0, 0);
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
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.Controls.Add(this.positionTeachingControl);
            this.gbPositionTeaching.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPositionTeaching.Location = new System.Drawing.Point(2, 2);
            this.gbPositionTeaching.Margin = new System.Windows.Forms.Padding(2);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Padding = new System.Windows.Forms.Padding(2);
            this.gbPositionTeaching.Size = new System.Drawing.Size(628, 386);
            this.gbPositionTeaching.TabIndex = 0;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Position Teaching";
            // 
            // positionTeachingControl
            // 
            this.positionTeachingControl.BackColor = System.Drawing.Color.White;
            this.positionTeachingControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionTeachingControl.Location = new System.Drawing.Point(2, 16);
            this.positionTeachingControl.Margin = new System.Windows.Forms.Padding(2);
            this.positionTeachingControl.Name = "positionTeachingControl";
            this.positionTeachingControl.Size = new System.Drawing.Size(624, 368);
            this.positionTeachingControl.TabIndex = 0;
            // 
            // positionItemPanel
            // 
            this.positionItemPanel.Location = new System.Drawing.Point(0, 0);
            this.positionItemPanel.Name = "positionItemPanel";
            this.positionItemPanel.Size = new System.Drawing.Size(200, 100);
            this.positionItemPanel.TabIndex = 0;
            // 
            // GageRnRUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "GageRnRUnit_Config";
            this.Text = "GageRnR Unit Configuration";
            this.gbMoveAxis.ResumeLayout(false);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.gbPositionTeaching.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private Component.UnitConfig unitConfigControl;
    }
}