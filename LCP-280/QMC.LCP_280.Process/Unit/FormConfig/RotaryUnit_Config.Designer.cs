using QMC.Common;
using QMC.Common.CustomControl;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class RotaryUnit_Config
    {
        private IOPropertyCollectionView inputView;
        private IOPropertyCollectionView outputView;
        private ListBoxItemsView axisListBoxItemsView;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbMoveAxis;
        private JogControl jogControl;
        private TableLayoutPanel mainTableLayoutPanel;
        private TableLayoutPanel ioTableLayoutPanel;
        private Panel positionItemPanel;
        private QMC.LCP_280.Process.Unit.FormConfig.PositionTeachingControl positionTeachingControl;
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
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionTeachingControl = new QMC.LCP_280.Process.Unit.FormConfig.PositionTeachingControl();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inputView = new QMC.Common.IOPropertyCollectionView();
            this.outputView = new QMC.Common.IOPropertyCollectionView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.unitConfigControl = new QMC.LCP_280.Process.Component.UnitConfig();
            this.positionItemPanel = new System.Windows.Forms.Panel();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
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
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbDigitalIO, 2);
            this.gbDigitalIO.Controls.Add(this.ioTableLayoutPanel);
            this.gbDigitalIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbDigitalIO.Location = new System.Drawing.Point(3, 393);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(626, 384);
            this.gbDigitalIO.TabIndex = 9;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // ioTableLayoutPanel
            // 
            this.ioTableLayoutPanel.ColumnCount = 2;
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Controls.Add(this.inputView, 0, 0);
            this.ioTableLayoutPanel.Controls.Add(this.outputView, 1, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(3, 21);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(620, 360);
            this.ioTableLayoutPanel.TabIndex = 2;
            // 
            // inputView
            // 
            this.inputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputView.FastBuild = true;
            this.inputView.FastInitialPaint = true;
            this.inputView.GroupName = "Input";
            this.inputView.ListBackColor = System.Drawing.Color.Black;
            this.inputView.ListForeColor = System.Drawing.Color.Lime;
            this.inputView.Location = new System.Drawing.Point(4, 6);
            this.inputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.inputView.Name = "inputView";
            this.inputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.inputView.SelectedForeColor = System.Drawing.Color.Black;
            this.inputView.Size = new System.Drawing.Size(302, 348);
            this.inputView.SuppressResizeInvalidation = true;
            this.inputView.TabIndex = 1;
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.FastBuild = true;
            this.outputView.FastInitialPaint = true;
            this.outputView.GroupName = "Output";
            this.outputView.ListBackColor = System.Drawing.Color.Black;
            this.outputView.ListForeColor = System.Drawing.Color.Lime;
            this.outputView.Location = new System.Drawing.Point(314, 6);
            this.outputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputView.Name = "outputView";
            this.outputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputView.Size = new System.Drawing.Size(302, 348);
            this.outputView.SuppressResizeInvalidation = true;
            this.outputView.TabIndex = 1;
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
            // RotaryUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "RotaryUnit_Config";
            this.Text = "Rotary Unit Configuration";
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private Component.UnitConfig unitConfigControl;
    }
}