namespace QMC.LCP_280.Process.Unit.FormSetup
{
    partial class LightControl
    {
        /// <summary> 
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 구성 요소 디자이너에서 생성한 코드

        /// <summary> 
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.iluminatorListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.gbIlluminatorControl = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_All_On_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_All_Off_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Connect_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Disconnect_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_On_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Off_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Illuninator_Setup = new QMC.Common.IndividualMenuButton();
            this.illuminatorPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            //this.individualMenuButton1 = new QMC.Common.IndividualMenuButton();
            this.iluminatorChannelListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label_Intensity = new System.Windows.Forms.Label();
            this.trackBar_LightIntensity = new System.Windows.Forms.TrackBar();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.gbIlluminatorControl.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.illuminatorPropertyCollectionView.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_LightIntensity)).BeginInit();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(887, 1118);
            this.tableLayoutPanel1.TabIndex = 0;
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
            this.iluminatorListBoxItemsView.Location = new System.Drawing.Point(4, 9);
            this.iluminatorListBoxItemsView.Margin = new System.Windows.Forms.Padding(4, 9, 4, 9);
            this.iluminatorListBoxItemsView.Name = "iluminatorListBoxItemsView";
            this.iluminatorListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorListBoxItemsView.SelectedIndex = -1;
            this.iluminatorListBoxItemsView.Size = new System.Drawing.Size(272, 364);
            this.iluminatorListBoxItemsView.TabIndex = 18;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Controls.Add(this.gbIlluminatorControl, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.btn_Save_Illuninator_Setup, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.illuminatorPropertyCollectionView, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 4);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 5F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(879, 1110);
            this.tableLayoutPanel2.TabIndex = 19;
            // 
            // gbIlluminatorControl
            // 
            this.gbIlluminatorControl.BackColor = System.Drawing.Color.White;
            this.gbIlluminatorControl.Controls.Add(this.tableLayoutPanel7);
            this.gbIlluminatorControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbIlluminatorControl.Location = new System.Drawing.Point(4, 890);
            this.gbIlluminatorControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbIlluminatorControl.Name = "gbIlluminatorControl";
            this.gbIlluminatorControl.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.gbIlluminatorControl.Size = new System.Drawing.Size(871, 216);
            this.gbIlluminatorControl.TabIndex = 26;
            this.gbIlluminatorControl.TabStop = false;
            this.gbIlluminatorControl.Text = "Control";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.00001F));
            this.tableLayoutPanel7.Controls.Add(this.btn_All_On_Illuminator, 0, 1);
            this.tableLayoutPanel7.Controls.Add(this.btn_All_Off_Illuminator, 1, 1);
            this.tableLayoutPanel7.Controls.Add(this.btn_Connect_Illuminator, 0, 0);
            this.tableLayoutPanel7.Controls.Add(this.btn_Disconnect_Illuminator, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.btn_On_Illuminator, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.btn_Off_Illuminator, 1, 2);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(4, 25);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 3;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(863, 187);
            this.tableLayoutPanel7.TabIndex = 2;
            // 
            // btn_All_On_Illuminator
            // 
            this.btn_All_On_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_On_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_All_On_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_On_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_On_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_All_On_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_All_On_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_On_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_All_On_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_All_On_Illuminator.Location = new System.Drawing.Point(4, 66);
            this.btn_All_On_Illuminator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_All_On_Illuminator.Name = "btn_All_On_Illuminator";
            this.btn_All_On_Illuminator.Size = new System.Drawing.Size(423, 54);
            this.btn_All_On_Illuminator.TabIndex = 12;
            this.btn_All_On_Illuminator.TabStop = false;
            this.btn_All_On_Illuminator.Text = "All On";
            this.btn_All_On_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_All_Off_Illuminator
            // 
            this.btn_All_Off_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_Off_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_All_Off_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_All_Off_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_Off_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_All_Off_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_All_Off_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_All_Off_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_All_Off_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_All_Off_Illuminator.Location = new System.Drawing.Point(435, 66);
            this.btn_All_Off_Illuminator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_All_Off_Illuminator.Name = "btn_All_Off_Illuminator";
            this.btn_All_Off_Illuminator.Size = new System.Drawing.Size(424, 54);
            this.btn_All_Off_Illuminator.TabIndex = 11;
            this.btn_All_Off_Illuminator.TabStop = false;
            this.btn_All_Off_Illuminator.Text = "All Off";
            this.btn_All_Off_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_Connect_Illuminator
            // 
            this.btn_Connect_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Connect_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Connect_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Connect_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Connect_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Connect_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Connect_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Connect_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Connect_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Connect_Illuminator.Location = new System.Drawing.Point(4, 4);
            this.btn_Connect_Illuminator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Connect_Illuminator.Name = "btn_Connect_Illuminator";
            this.btn_Connect_Illuminator.Size = new System.Drawing.Size(423, 54);
            this.btn_Connect_Illuminator.TabIndex = 10;
            this.btn_Connect_Illuminator.TabStop = false;
            this.btn_Connect_Illuminator.Text = "Connect";
            this.btn_Connect_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_Disconnect_Illuminator
            // 
            this.btn_Disconnect_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Disconnect_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Disconnect_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Disconnect_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Disconnect_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Disconnect_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Disconnect_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Disconnect_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Disconnect_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Disconnect_Illuminator.Location = new System.Drawing.Point(435, 4);
            this.btn_Disconnect_Illuminator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Disconnect_Illuminator.Name = "btn_Disconnect_Illuminator";
            this.btn_Disconnect_Illuminator.Size = new System.Drawing.Size(424, 54);
            this.btn_Disconnect_Illuminator.TabIndex = 9;
            this.btn_Disconnect_Illuminator.TabStop = false;
            this.btn_Disconnect_Illuminator.Text = "Disconnect";
            this.btn_Disconnect_Illuminator.UseVisualStyleBackColor = false;
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
            this.btn_On_Illuminator.Location = new System.Drawing.Point(4, 128);
            this.btn_On_Illuminator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_On_Illuminator.Name = "btn_On_Illuminator";
            this.btn_On_Illuminator.Size = new System.Drawing.Size(423, 55);
            this.btn_On_Illuminator.TabIndex = 6;
            this.btn_On_Illuminator.TabStop = false;
            this.btn_On_Illuminator.Text = "On";
            this.btn_On_Illuminator.UseVisualStyleBackColor = false;
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
            this.btn_Off_Illuminator.Location = new System.Drawing.Point(435, 128);
            this.btn_Off_Illuminator.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Off_Illuminator.Name = "btn_Off_Illuminator";
            this.btn_Off_Illuminator.Size = new System.Drawing.Size(424, 55);
            this.btn_Off_Illuminator.TabIndex = 1;
            this.btn_Off_Illuminator.TabStop = false;
            this.btn_Off_Illuminator.Text = "Off";
            this.btn_Off_Illuminator.UseVisualStyleBackColor = false;
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
            this.btn_Save_Illuninator_Setup.Location = new System.Drawing.Point(4, 835);
            this.btn_Save_Illuninator_Setup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Save_Illuninator_Setup.Name = "btn_Save_Illuninator_Setup";
            this.btn_Save_Illuninator_Setup.Size = new System.Drawing.Size(871, 47);
            this.btn_Save_Illuninator_Setup.TabIndex = 25;
            this.btn_Save_Illuninator_Setup.TabStop = false;
            this.btn_Save_Illuninator_Setup.Text = "Save";
            this.btn_Save_Illuninator_Setup.UseVisualStyleBackColor = false;
            // 
            // illuminatorPropertyCollectionView
            // 
            //this.illuminatorPropertyCollectionView.Controls.Add(this.individualMenuButton1);
            this.illuminatorPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.illuminatorPropertyCollectionView.FastBuild = true;
            this.illuminatorPropertyCollectionView.GroupName = "Property";
            this.illuminatorPropertyCollectionView.Location = new System.Drawing.Point(6, 449);
            this.illuminatorPropertyCollectionView.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.illuminatorPropertyCollectionView.Name = "illuminatorPropertyCollectionView";
            this.illuminatorPropertyCollectionView.Size = new System.Drawing.Size(867, 376);
            this.illuminatorPropertyCollectionView.SuppressResizeInvalidation = true;
            this.illuminatorPropertyCollectionView.TabIndex = 21;
            this.illuminatorPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold);
            this.illuminatorPropertyCollectionView.TextBoxFontSize = 10F;
            // 
            // individualMenuButton1
            // 
            //this.individualMenuButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            //this.individualMenuButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            //this.individualMenuButton1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            //this.individualMenuButton1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            //this.individualMenuButton1.CustomForeColor = System.Drawing.Color.Black;
            //this.individualMenuButton1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            //this.individualMenuButton1.ForeColor = System.Drawing.Color.Black;
            //this.individualMenuButton1.ImageSize = new System.Drawing.Size(45, 45);
            //this.individualMenuButton1.Location = new System.Drawing.Point(330, 312);
            //this.individualMenuButton1.Name = "individualMenuButton1";
            //this.individualMenuButton1.Size = new System.Drawing.Size(100, 40);
            //this.individualMenuButton1.TabIndex = 5;
            //this.individualMenuButton1.TabStop = false;
            //this.individualMenuButton1.Text = "Save";
            //this.individualMenuButton1.UseVisualStyleBackColor = false;
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
            this.iluminatorChannelListBoxItemsView.Location = new System.Drawing.Point(284, 9);
            this.iluminatorChannelListBoxItemsView.Margin = new System.Windows.Forms.Padding(4, 9, 4, 9);
            this.iluminatorChannelListBoxItemsView.Name = "iluminatorChannelListBoxItemsView";
            this.iluminatorChannelListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.iluminatorChannelListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.iluminatorChannelListBoxItemsView.SelectedIndex = -1;
            this.iluminatorChannelListBoxItemsView.Size = new System.Drawing.Size(585, 364);
            this.iluminatorChannelListBoxItemsView.TabIndex = 20;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 114F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label_Intensity, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.trackBar_LightIntensity, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 392);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(871, 47);
            this.tableLayoutPanel3.TabIndex = 27;
            // 
            // label_Intensity
            // 
            this.label_Intensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Intensity.Location = new System.Drawing.Point(4, 4);
            this.label_Intensity.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label_Intensity.Name = "label_Intensity";
            this.label_Intensity.Size = new System.Drawing.Size(106, 39);
            this.label_Intensity.TabIndex = 0;
            // 
            // trackBar_LightIntensity
            // 
            this.trackBar_LightIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackBar_LightIntensity.Location = new System.Drawing.Point(118, 4);
            this.trackBar_LightIntensity.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.trackBar_LightIntensity.Maximum = 4095;
            this.trackBar_LightIntensity.Name = "trackBar_LightIntensity";
            this.trackBar_LightIntensity.Size = new System.Drawing.Size(749, 39);
            this.trackBar_LightIntensity.TabIndex = 1;
            this.trackBar_LightIntensity.TickFrequency = 256;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.tableLayoutPanel4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(873, 382);
            this.panel1.TabIndex = 28;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.07331F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.92669F));
            this.tableLayoutPanel4.Controls.Add(this.iluminatorListBoxItemsView, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.iluminatorChannelListBoxItemsView, 1, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(873, 382);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // LightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "LightControl";
            this.Size = new System.Drawing.Size(887, 1118);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.gbIlluminatorControl.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.illuminatorPropertyCollectionView.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_LightIntensity)).EndInit();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.ListBoxItemsView iluminatorListBoxItemsView;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private Common.ListBoxItemsView iluminatorChannelListBoxItemsView;
        private Common.PropertyCollectionView illuminatorPropertyCollectionView;
        //private Common.IndividualMenuButton individualMenuButton1;
        private Common.IndividualMenuButton btn_Save_Illuninator_Setup;
        private System.Windows.Forms.GroupBox gbIlluminatorControl;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Common.IndividualMenuButton btn_All_On_Illuminator;
        private Common.IndividualMenuButton btn_All_Off_Illuminator;
        private Common.IndividualMenuButton btn_Connect_Illuminator;
        private Common.IndividualMenuButton btn_Disconnect_Illuminator;
        private Common.IndividualMenuButton btn_On_Illuminator;
        private Common.IndividualMenuButton btn_Off_Illuminator;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label_Intensity;
        private System.Windows.Forms.TrackBar trackBar_LightIntensity;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
    }
}
