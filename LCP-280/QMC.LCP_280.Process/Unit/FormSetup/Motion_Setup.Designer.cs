using QMC.Common;
using QMC.Common.CustomControl;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Motion_Setup Designer (GUI only)
    /// </summary>
    partial class Motion_Setup
    {
        private System.ComponentModel.IContainer components = null;

        // UI Controls
        private ListBoxItemsView selectAxisListBoxItemsView;
        private IOPropertyCollectionView motorStateIoPropertyCollectionView;
        private IOPropertyCollectionView motorIoPropertyCollectionView;
        private PropertyCollectionView positionVelocityPropertyCollectionView;
        private PropertyCollectionView speedListBoxItemsView;
        private PropertyCollectionView configurationListBoxItemsView;
        private GroupBox gbAxisProperty;
        private GroupBox gbAxisPositions;
        private IndividualMenuButton btn_Save_Setup_Motion_Configuration;
        private IndividualMenuButton btn_Save_Setup_Motion_Speed;
        private IndividualMenuButton btnHome;
        private IndividualMenuButton btnServoOn;
        private IndividualMenuButton btnServoOff;
        private IndividualMenuButton btnHomeAll;

        /// <summary>
        /// Dispose
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.gbAxisProperty = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_Save_Setup_Motion_Speed = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Setup_Motion_Configuration = new QMC.Common.IndividualMenuButton();
            this.configurationListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.speedListBoxItemsView = new QMC.Common.PropertyCollectionView();
            this.gbAxisPositions = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.motorStateIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.motorIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.positionVelocityPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btnServoOn = new QMC.Common.IndividualMenuButton();
            this.btnHomeAll = new QMC.Common.IndividualMenuButton();
            this.btnServoOff = new QMC.Common.IndividualMenuButton();
            this.btnHome = new QMC.Common.IndividualMenuButton();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.gbAxisProperty.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.gbAxisPositions.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAxisProperty
            // 
            this.gbAxisProperty.BackColor = System.Drawing.Color.White;
            this.gbAxisProperty.Controls.Add(this.tableLayoutPanel1);
            this.gbAxisProperty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAxisProperty.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbAxisProperty.Location = new System.Drawing.Point(571, 3);
            this.gbAxisProperty.Name = "gbAxisProperty";
            this.gbAxisProperty.Size = new System.Drawing.Size(690, 745);
            this.gbAxisProperty.TabIndex = 10;
            this.gbAxisProperty.TabStop = false;
            this.gbAxisProperty.Text = "Axis Property";
            this.gbAxisProperty.Enter += new System.EventHandler(this.gbAxisProperty_Enter);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.btn_Save_Setup_Motion_Speed, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.btn_Save_Setup_Motion_Configuration, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.configurationListBoxItemsView, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.speedListBoxItemsView, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 21);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 93F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(684, 721);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // btn_Save_Setup_Motion_Speed
            // 
            this.btn_Save_Setup_Motion_Speed.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Speed.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Motion_Speed.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Speed.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Speed.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Speed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Setup_Motion_Speed.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Speed.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Speed.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Motion_Speed.Location = new System.Drawing.Point(345, 636);
            this.btn_Save_Setup_Motion_Speed.Name = "btn_Save_Setup_Motion_Speed";
            this.btn_Save_Setup_Motion_Speed.Size = new System.Drawing.Size(336, 41);
            this.btn_Save_Setup_Motion_Speed.TabIndex = 5;
            this.btn_Save_Setup_Motion_Speed.TabStop = false;
            this.btn_Save_Setup_Motion_Speed.Text = "Save";
            this.btn_Save_Setup_Motion_Speed.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Motion_Speed.Click += new System.EventHandler(this.btn_Save_Setup_Motion_Speed_Click);
            // 
            // btn_Save_Setup_Motion_Configuration
            // 
            this.btn_Save_Setup_Motion_Configuration.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Configuration.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Motion_Configuration.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Motion_Configuration.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Configuration.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Configuration.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Setup_Motion_Configuration.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Motion_Configuration.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Motion_Configuration.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Motion_Configuration.Location = new System.Drawing.Point(3, 636);
            this.btn_Save_Setup_Motion_Configuration.Name = "btn_Save_Setup_Motion_Configuration";
            this.btn_Save_Setup_Motion_Configuration.Size = new System.Drawing.Size(336, 41);
            this.btn_Save_Setup_Motion_Configuration.TabIndex = 4;
            this.btn_Save_Setup_Motion_Configuration.TabStop = false;
            this.btn_Save_Setup_Motion_Configuration.Text = "Save";
            this.btn_Save_Setup_Motion_Configuration.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Motion_Configuration.Click += new System.EventHandler(this.btn_Save_Setup_Motion_Configuration_Click);
            // 
            // configurationListBoxItemsView
            // 
            this.configurationListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationListBoxItemsView.FastBuild = true;
            this.configurationListBoxItemsView.GroupName = "Configuration";
            this.configurationListBoxItemsView.Location = new System.Drawing.Point(3, 4);
            this.configurationListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.configurationListBoxItemsView.Name = "configurationListBoxItemsView";
            this.configurationListBoxItemsView.Size = new System.Drawing.Size(336, 625);
            this.configurationListBoxItemsView.SuppressResizeInvalidation = true;
            this.configurationListBoxItemsView.TabIndex = 0;
            // 
            // speedListBoxItemsView
            // 
            this.speedListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.speedListBoxItemsView.FastBuild = true;
            this.speedListBoxItemsView.GroupName = "Speed";
            this.speedListBoxItemsView.Location = new System.Drawing.Point(345, 4);
            this.speedListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.speedListBoxItemsView.Name = "speedListBoxItemsView";
            this.speedListBoxItemsView.Size = new System.Drawing.Size(336, 625);
            this.speedListBoxItemsView.SuppressResizeInvalidation = true;
            this.speedListBoxItemsView.TabIndex = 1;
            // 
            // gbAxisPositions
            // 
            this.gbAxisPositions.BackColor = System.Drawing.Color.White;
            this.gbAxisPositions.Controls.Add(this.tableLayoutPanel2);
            this.gbAxisPositions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbAxisPositions.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbAxisPositions.Location = new System.Drawing.Point(255, 3);
            this.gbAxisPositions.Name = "gbAxisPositions";
            this.gbAxisPositions.Size = new System.Drawing.Size(310, 745);
            this.gbAxisPositions.TabIndex = 13;
            this.gbAxisPositions.TabStop = false;
            this.gbAxisPositions.Text = "Axis Status";
            this.gbAxisPositions.Enter += new System.EventHandler(this.gbAxisPositions_Enter);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.motorStateIoPropertyCollectionView, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.motorIoPropertyCollectionView, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.positionVelocityPropertyCollectionView, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel4, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 21);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(304, 721);
            this.tableLayoutPanel2.TabIndex = 9;
            // 
            // motorStateIoPropertyCollectionView
            // 
            this.motorStateIoPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.motorStateIoPropertyCollectionView.FastBuild = true;
            this.motorStateIoPropertyCollectionView.FastInitialPaint = true;
            this.motorStateIoPropertyCollectionView.GroupName = "Motor State";
            this.motorStateIoPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.motorStateIoPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.motorStateIoPropertyCollectionView.Location = new System.Drawing.Point(3, 414);
            this.motorStateIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.motorStateIoPropertyCollectionView.Name = "motorStateIoPropertyCollectionView";
            this.motorStateIoPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.motorStateIoPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.motorStateIoPropertyCollectionView.Size = new System.Drawing.Size(298, 192);
            this.motorStateIoPropertyCollectionView.SuppressResizeInvalidation = true;
            this.motorStateIoPropertyCollectionView.TabIndex = 2;
            // 
            // motorIoPropertyCollectionView
            // 
            this.motorIoPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.motorIoPropertyCollectionView.FastBuild = true;
            this.motorIoPropertyCollectionView.FastInitialPaint = true;
            this.motorIoPropertyCollectionView.GroupName = "Motor I/O";
            this.motorIoPropertyCollectionView.ListBackColor = System.Drawing.Color.Black;
            this.motorIoPropertyCollectionView.ListForeColor = System.Drawing.Color.Lime;
            this.motorIoPropertyCollectionView.Location = new System.Drawing.Point(3, 208);
            this.motorIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.motorIoPropertyCollectionView.Name = "motorIoPropertyCollectionView";
            this.motorIoPropertyCollectionView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.motorIoPropertyCollectionView.SelectedForeColor = System.Drawing.Color.Black;
            this.motorIoPropertyCollectionView.Size = new System.Drawing.Size(298, 196);
            this.motorIoPropertyCollectionView.SuppressResizeInvalidation = true;
            this.motorIoPropertyCollectionView.TabIndex = 1;
            this.motorIoPropertyCollectionView.ItemClicked += new System.EventHandler<string>(this.OnMotorIOItemClicked);
            // 
            // positionVelocityPropertyCollectionView
            // 
            this.positionVelocityPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionVelocityPropertyCollectionView.FastBuild = true;
            this.positionVelocityPropertyCollectionView.GroupName = "Position & Velocity";
            this.positionVelocityPropertyCollectionView.Location = new System.Drawing.Point(3, 4);
            this.positionVelocityPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.positionVelocityPropertyCollectionView.Name = "positionVelocityPropertyCollectionView";
            this.positionVelocityPropertyCollectionView.Size = new System.Drawing.Size(298, 196);
            this.positionVelocityPropertyCollectionView.SuppressResizeInvalidation = true;
            this.positionVelocityPropertyCollectionView.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel4.Controls.Add(this.btnServoOn, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnHomeAll, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.btnServoOff, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.btnHome, 2, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 615);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(298, 62);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // btnServoOn
            // 
            this.btnServoOn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnServoOn.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOn.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOn.CustomForeColor = System.Drawing.Color.Black;
            this.btnServoOn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnServoOn.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOn.ForeColor = System.Drawing.Color.Black;
            this.btnServoOn.ImageSize = new System.Drawing.Size(45, 45);
            this.btnServoOn.Location = new System.Drawing.Point(3, 3);
            this.btnServoOn.Name = "btnServoOn";
            this.btnServoOn.Size = new System.Drawing.Size(93, 25);
            this.btnServoOn.TabIndex = 7;
            this.btnServoOn.TabStop = false;
            this.btnServoOn.Text = "Servo On";
            this.btnServoOn.UseVisualStyleBackColor = false;
            this.btnServoOn.Click += new System.EventHandler(this.btnServoOn_Click);
            // 
            // btnHomeAll
            // 
            this.btnHomeAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHomeAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnHomeAll.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHomeAll.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHomeAll.CustomForeColor = System.Drawing.Color.Black;
            this.btnHomeAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnHomeAll.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHomeAll.ForeColor = System.Drawing.Color.Black;
            this.btnHomeAll.ImageSize = new System.Drawing.Size(45, 45);
            this.btnHomeAll.Location = new System.Drawing.Point(3, 34);
            this.btnHomeAll.Name = "btnHomeAll";
            this.btnHomeAll.Size = new System.Drawing.Size(93, 25);
            this.btnHomeAll.TabIndex = 8;
            this.btnHomeAll.TabStop = false;
            this.btnHomeAll.Text = "Home All";
            this.btnHomeAll.UseVisualStyleBackColor = false;
            this.btnHomeAll.Click += new System.EventHandler(this.btnHomeAll_Click);
            // 
            // btnServoOff
            // 
            this.btnServoOff.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOff.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnServoOff.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnServoOff.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOff.CustomForeColor = System.Drawing.Color.Black;
            this.btnServoOff.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnServoOff.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnServoOff.ForeColor = System.Drawing.Color.Black;
            this.btnServoOff.ImageSize = new System.Drawing.Size(45, 45);
            this.btnServoOff.Location = new System.Drawing.Point(102, 3);
            this.btnServoOff.Name = "btnServoOff";
            this.btnServoOff.Size = new System.Drawing.Size(93, 25);
            this.btnServoOff.TabIndex = 6;
            this.btnServoOff.TabStop = false;
            this.btnServoOff.Text = "Servo Off";
            this.btnServoOff.UseVisualStyleBackColor = false;
            this.btnServoOff.Click += new System.EventHandler(this.btnServoOff_Click);
            // 
            // btnHome
            // 
            this.btnHome.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHome.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnHome.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnHome.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHome.CustomForeColor = System.Drawing.Color.Black;
            this.btnHome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnHome.Enabled = false;
            this.btnHome.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnHome.ForeColor = System.Drawing.Color.Black;
            this.btnHome.ImageSize = new System.Drawing.Size(45, 45);
            this.btnHome.Location = new System.Drawing.Point(201, 3);
            this.btnHome.Name = "btnHome";
            this.btnHome.Size = new System.Drawing.Size(94, 25);
            this.btnHome.TabIndex = 5;
            this.btnHome.TabStop = false;
            this.btnHome.Text = "Home Test";
            this.btnHome.UseVisualStyleBackColor = false;
            this.btnHome.Click += new System.EventHandler(this.btnHome_Click);
            // 
            // selectAxisListBoxItemsView
            // 
            this.selectAxisListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.selectAxisListBoxItemsView.Font = new System.Drawing.Font("굴림", 11F);
            this.selectAxisListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.selectAxisListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.GroupName = "Select Axis";
            this.selectAxisListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.selectAxisListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.selectAxisListBoxItemsView.Location = new System.Drawing.Point(3, 3);
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";
            this.selectAxisListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.selectAxisListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.selectAxisListBoxItemsView.SelectedIndex = -1;
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(246, 745);
            this.selectAxisListBoxItemsView.TabIndex = 2;
            this.selectAxisListBoxItemsView.ItemSelected += new System.EventHandler<int>(this.OnAxisSelected);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.tableLayoutPanel3.Controls.Add(this.gbAxisProperty, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.selectAxisListBoxItemsView, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.gbAxisPositions, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel3.TabIndex = 9;
            // 
            // Motion_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel3);
            this.Name = "Motion_Setup";
            this.Text = "Motion Setup";
            this.Load += new System.EventHandler(this.Motion_Setup_Load);
            this.gbAxisProperty.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.gbAxisPositions.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel4;
    }
}
