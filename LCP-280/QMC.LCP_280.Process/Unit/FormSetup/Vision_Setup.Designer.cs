using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using QMC.Common; // ListBoxItemsView, IndividualMenuButton, PropertyCollectionView
using QMC.Common.CustomControl;
using QMC.Common.Vision;

namespace QMC.LCP_280.Process.Unit
{
    partial class Vision_Setup
    {
        private VisionImageViewer visionImageViewer;
        private ListBoxItemsView cameraListBoxItemsView;
        private ListBoxItemsView iluminatorListBoxItemsView;
        private ListBoxItemsView iluminatorChannelListBoxItemsView;
        private PropertyCollectionView cameraPropertyCollectionView;
        private PropertyCollectionView illuminatorPropertyCollectionView;
        private IndividualMenuButton btn_Save_Setup_Cylinder;
        private IndividualMenuButton individualMenuButton1;
        private IndividualMenuButton btn_Camera_Setup;
        private IndividualMenuButton btn_Illuninator_Setup;
        private IndividualMenuButton btn_Off_Illuminator;
        private IndividualMenuButton btn_On_Illuminator;
        private IndividualMenuButton btn_Save_Camera_Setup;
        private IndividualMenuButton btn_Save_Illuninator_Setup;
        private GroupBox gbIlluminatorControl;
        private IndividualMenuButton btn_JogPopup;

        private IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null) components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.cameraPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.btn_Save_Setup_Cylinder = new QMC.Common.IndividualMenuButton();
            this.visionImageViewer = new QMC.Common.Vision.VisionImageViewer();
            this.iluminatorListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.illuminatorPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.individualMenuButton1 = new QMC.Common.IndividualMenuButton();
            this.iluminatorChannelListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btn_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Illuninator_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Off_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_On_Illuminator = new QMC.Common.IndividualMenuButton();
            this.gbIlluminatorControl = new System.Windows.Forms.GroupBox();
            this.btn_Save_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Illuninator_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_JogPopup = new QMC.Common.IndividualMenuButton();
            this.groupBoxImageView = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.cameraPropertyCollectionView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).BeginInit();
            this.illuminatorPropertyCollectionView.SuspendLayout();
            this.gbIlluminatorControl.SuspendLayout();
            this.groupBoxImageView.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.SuspendLayout();
            // 
            // cameraListBoxItemsView
            // 
            this.cameraListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.BorderWidth = 2;
            this.cameraListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.cameraListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.GroupName = "Camera";
            this.cameraListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.cameraListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(177, 509);
            this.cameraListBoxItemsView.TabIndex = 2;
            this.cameraListBoxItemsView.Load += new System.EventHandler(this.cameraListBoxItemsView_Load);
            // 
            // cameraPropertyCollectionView
            // 
            this.cameraPropertyCollectionView.Controls.Add(this.btn_Save_Setup_Cylinder);
            this.cameraPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cameraPropertyCollectionView.FastBuild = true;
            this.cameraPropertyCollectionView.GroupName = "Property";
            this.cameraPropertyCollectionView.Location = new System.Drawing.Point(3, 524);
            this.cameraPropertyCollectionView.Name = "cameraPropertyCollectionView";
            this.cameraPropertyCollectionView.Size = new System.Drawing.Size(430, 165);
            this.cameraPropertyCollectionView.SuppressResizeInvalidation = true;
            this.cameraPropertyCollectionView.TabIndex = 13;
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
            this.btn_Save_Setup_Cylinder.Location = new System.Drawing.Point(330, 312);
            this.btn_Save_Setup_Cylinder.Name = "btn_Save_Setup_Cylinder";
            this.btn_Save_Setup_Cylinder.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Cylinder.TabIndex = 5;
            this.btn_Save_Setup_Cylinder.TabStop = false;
            this.btn_Save_Setup_Cylinder.Text = "Save";
            this.btn_Save_Setup_Cylinder.UseVisualStyleBackColor = false;
            // 
            // visionImageViewer
            // 
            this.visionImageViewer.BackColor = System.Drawing.Color.Black;
            this.visionImageViewer.Camera = null;
            this.visionImageViewer.CameraSwitch = null;
            this.visionImageViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.visionImageViewer.FrameRate = 1D;
            this.visionImageViewer.InputImage = null;
            this.visionImageViewer.IsViewCustomizedImage = false;
            this.visionImageViewer.Location = new System.Drawing.Point(3, 17);
            this.visionImageViewer.Name = "visionImageViewer";
            this.visionImageViewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.visionImageViewer.Simulated = false;
            this.visionImageViewer.Size = new System.Drawing.Size(424, 495);
            this.visionImageViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.visionImageViewer.TabIndex = 16;
            this.visionImageViewer.TabStop = false;
            this.visionImageViewer.UpdateDelayTime = 80;
            this.visionImageViewer.VisibleCrossLine = true;
            // 
            // iluminatorListBoxItemsView
            // 
            this.iluminatorListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.iluminatorListBoxItemsView.BorderWidth = 2;
            this.iluminatorListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iluminatorListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.iluminatorListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.GroupName = "Illuminator";
            this.iluminatorListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.iluminatorListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.iluminatorListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.iluminatorListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorListBoxItemsView.Name = "iluminatorListBoxItemsView";
            this.iluminatorListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.SelectedIndex = -1;
            this.iluminatorListBoxItemsView.Size = new System.Drawing.Size(177, 509);
            this.iluminatorListBoxItemsView.TabIndex = 17;
            // 
            // illuminatorPropertyCollectionView
            // 
            this.illuminatorPropertyCollectionView.Controls.Add(this.individualMenuButton1);
            this.illuminatorPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.illuminatorPropertyCollectionView.FastBuild = true;
            this.illuminatorPropertyCollectionView.GroupName = "Property";
            this.illuminatorPropertyCollectionView.Location = new System.Drawing.Point(3, 375);
            this.illuminatorPropertyCollectionView.Name = "illuminatorPropertyCollectionView";
            this.illuminatorPropertyCollectionView.Size = new System.Drawing.Size(432, 292);
            this.illuminatorPropertyCollectionView.SuppressResizeInvalidation = true;
            this.illuminatorPropertyCollectionView.TabIndex = 18;
            // 
            // individualMenuButton1
            // 
            this.individualMenuButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton1.Location = new System.Drawing.Point(330, 312);
            this.individualMenuButton1.Name = "individualMenuButton1";
            this.individualMenuButton1.Size = new System.Drawing.Size(100, 40);
            this.individualMenuButton1.TabIndex = 5;
            this.individualMenuButton1.TabStop = false;
            this.individualMenuButton1.Text = "Save";
            this.individualMenuButton1.UseVisualStyleBackColor = false;
            // 
            // iluminatorChannelListBoxItemsView
            // 
            this.iluminatorChannelListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.iluminatorChannelListBoxItemsView.BorderWidth = 2;
            this.iluminatorChannelListBoxItemsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iluminatorChannelListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.iluminatorChannelListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.GroupName = "Channel";
            this.iluminatorChannelListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.iluminatorChannelListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.iluminatorChannelListBoxItemsView.Location = new System.Drawing.Point(3, 6);
            this.iluminatorChannelListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorChannelListBoxItemsView.Name = "iluminatorChannelListBoxItemsView";
            this.iluminatorChannelListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorChannelListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.SelectedIndex = -1;
            this.iluminatorChannelListBoxItemsView.Size = new System.Drawing.Size(432, 308);
            this.iluminatorChannelListBoxItemsView.TabIndex = 19;
            // 
            // btn_Camera_Setup
            // 
            this.btn_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Camera_Setup.Location = new System.Drawing.Point(91, 3);
            this.btn_Camera_Setup.Name = "btn_Camera_Setup";
            this.btn_Camera_Setup.Size = new System.Drawing.Size(83, 40);
            this.btn_Camera_Setup.TabIndex = 20;
            this.btn_Camera_Setup.TabStop = false;
            this.btn_Camera_Setup.Text = "Set up";
            this.btn_Camera_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Illuninator_Setup
            // 
            this.btn_Illuninator_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Illuninator_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Illuninator_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Illuninator_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Illuninator_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Illuninator_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Illuninator_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Illuninator_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Illuninator_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Illuninator_Setup.Location = new System.Drawing.Point(3, 524);
            this.btn_Illuninator_Setup.Name = "btn_Illuninator_Setup";
            this.btn_Illuninator_Setup.Size = new System.Drawing.Size(177, 46);
            this.btn_Illuninator_Setup.TabIndex = 21;
            this.btn_Illuninator_Setup.TabStop = false;
            this.btn_Illuninator_Setup.Text = "Set up";
            this.btn_Illuninator_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Off_Illuminator
            // 
            this.btn_Off_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Off_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Off_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Off_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Off_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Off_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Off_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Off_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Off_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Off_Illuminator.Location = new System.Drawing.Point(216, 3);
            this.btn_Off_Illuminator.Name = "btn_Off_Illuminator";
            this.btn_Off_Illuminator.Size = new System.Drawing.Size(207, 43);
            this.btn_Off_Illuminator.TabIndex = 1;
            this.btn_Off_Illuminator.TabStop = false;
            this.btn_Off_Illuminator.Text = "Off";
            this.btn_Off_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_On_Illuminator
            // 
            this.btn_On_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_On_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_On_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_On_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_On_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_On_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_On_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_On_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_On_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_On_Illuminator.Location = new System.Drawing.Point(3, 3);
            this.btn_On_Illuminator.Name = "btn_On_Illuminator";
            this.btn_On_Illuminator.Size = new System.Drawing.Size(207, 43);
            this.btn_On_Illuminator.TabIndex = 0;
            this.btn_On_Illuminator.TabStop = false;
            this.btn_On_Illuminator.Text = "On";
            this.btn_On_Illuminator.UseVisualStyleBackColor = false;
            // 
            // gbIlluminatorControl
            // 
            this.gbIlluminatorControl.BackColor = System.Drawing.Color.White;
            this.gbIlluminatorControl.Controls.Add(this.tableLayoutPanel7);
            this.gbIlluminatorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbIlluminatorControl.Location = new System.Drawing.Point(3, 673);
            this.gbIlluminatorControl.Name = "gbIlluminatorControl";
            this.gbIlluminatorControl.Size = new System.Drawing.Size(432, 69);
            this.gbIlluminatorControl.TabIndex = 22;
            this.gbIlluminatorControl.TabStop = false;
            this.gbIlluminatorControl.Text = "Control";
            // 
            // btn_Save_Camera_Setup
            // 
            this.btn_Save_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Camera_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Camera_Setup.Location = new System.Drawing.Point(3, 695);
            this.btn_Save_Camera_Setup.Name = "btn_Save_Camera_Setup";
            this.btn_Save_Camera_Setup.Size = new System.Drawing.Size(430, 47);
            this.btn_Save_Camera_Setup.TabIndex = 23;
            this.btn_Save_Camera_Setup.TabStop = false;
            this.btn_Save_Camera_Setup.Text = "Save";
            this.btn_Save_Camera_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Save_Illuninator_Setup
            // 
            this.btn_Save_Illuninator_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Illuninator_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Illuninator_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Illuninator_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Illuninator_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Illuninator_Setup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save_Illuninator_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Illuninator_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Illuninator_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Illuninator_Setup.Location = new System.Drawing.Point(3, 323);
            this.btn_Save_Illuninator_Setup.Name = "btn_Save_Illuninator_Setup";
            this.btn_Save_Illuninator_Setup.Size = new System.Drawing.Size(432, 46);
            this.btn_Save_Illuninator_Setup.TabIndex = 24;
            this.btn_Save_Illuninator_Setup.TabStop = false;
            this.btn_Save_Illuninator_Setup.Text = "Save";
            this.btn_Save_Illuninator_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_JogPopup
            // 
            this.btn_JogPopup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_JogPopup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_JogPopup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_JogPopup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_JogPopup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_JogPopup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_JogPopup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_JogPopup.ForeColor = System.Drawing.Color.Black;
            this.btn_JogPopup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_JogPopup.Location = new System.Drawing.Point(3, 3);
            this.btn_JogPopup.Name = "btn_JogPopup";
            this.btn_JogPopup.Size = new System.Drawing.Size(82, 40);
            this.btn_JogPopup.TabIndex = 25;
            this.btn_JogPopup.TabStop = false;
            this.btn_JogPopup.Text = "Axis Jog";
            this.btn_JogPopup.UseVisualStyleBackColor = false;
            this.btn_JogPopup.Click += new System.EventHandler(this.btn_JogPopup_Click);
            // 
            // groupBoxImageView
            // 
            this.groupBoxImageView.Controls.Add(this.visionImageViewer);
            this.groupBoxImageView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxImageView.Location = new System.Drawing.Point(3, 3);
            this.groupBoxImageView.Name = "groupBoxImageView";
            this.groupBoxImageView.Size = new System.Drawing.Size(430, 515);
            this.groupBoxImageView.TabIndex = 26;
            this.groupBoxImageView.TabStop = false;
            this.groupBoxImageView.Text = "ImageView";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1264, 751);
            this.tableLayoutPanel1.TabIndex = 27;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.iluminatorChannelListBoxItemsView, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.gbIlluminatorControl, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.btn_Save_Illuninator_Setup, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.illuminatorPropertyCollectionView, 0, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(823, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 4;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 43F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(438, 745);
            this.tableLayoutPanel5.TabIndex = 4;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.iluminatorListBoxItemsView, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btn_Illuninator_Setup, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(634, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(183, 745);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.cameraListBoxItemsView, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel6, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 3;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(183, 745);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.groupBoxImageView, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.cameraPropertyCollectionView, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.btn_Save_Camera_Setup, 0, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(192, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 23F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 7F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(436, 745);
            this.tableLayoutPanel3.TabIndex = 1;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.btn_JogPopup, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.btn_Camera_Setup, 1, 0);
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 524);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(177, 46);
            this.tableLayoutPanel6.TabIndex = 3;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Controls.Add(this.btn_Off_Illuminator, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.btn_On_Illuminator, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 17);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(426, 49);
            this.tableLayoutPanel7.TabIndex = 2;
            // 
            // Vision_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 751);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Vision_Setup";
            this.Text = "Motion Setup";
            this.cameraPropertyCollectionView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).EndInit();
            this.illuminatorPropertyCollectionView.ResumeLayout(false);
            this.gbIlluminatorControl.ResumeLayout(false);
            this.groupBoxImageView.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private GroupBox groupBoxImageView;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel6;
        private TableLayoutPanel tableLayoutPanel7;
    }
}
