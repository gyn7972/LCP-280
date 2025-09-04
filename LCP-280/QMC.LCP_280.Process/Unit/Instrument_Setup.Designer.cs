namespace QMC.LCP_280.Process.Unit
{
    partial class Instrument_Setup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lbivSelectItem = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.gbCommTerminal = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tbLog = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_SendTest = new QMC.Common.IndividualMenuButton();
            this.tbSendText = new System.Windows.Forms.TextBox();
            this.gbCommunication = new System.Windows.Forms.GroupBox();
            this.btn_Initialize = new QMC.Common.IndividualMenuButton();
            this.btn_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Disconnect = new QMC.Common.IndividualMenuButton();
            this.btn_Connect = new QMC.Common.IndividualMenuButton();
            this.lbStatusCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbStatusValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.gbInformation = new System.Windows.Forms.GroupBox();
            this.lbSerialNumberCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSerialNumberValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbModelCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbModelValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.pcvConfig = new QMC.Common.PropertyCollectionView();
            this.gbChannel = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.lbivSelectChannel = new QMC.Common.ListBoxItemsView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnChannelTest = new QMC.Common.IndividualMenuButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.gbCommTerminal.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.gbCommunication.SuspendLayout();
            this.gbInformation.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.gbChannel.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1264, 752);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1256, 721);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Sourcemeter";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.54777F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.45223F));
            this.tableLayoutPanel1.Controls.Add(this.lbivSelectItem, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1250, 715);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lbivSelectItem
            // 
            this.lbivSelectItem.BorderWidth = 2;
            this.lbivSelectItem.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelectItem.GroupName = "Select Item";
            this.lbivSelectItem.Location = new System.Drawing.Point(4, 5);
            this.lbivSelectItem.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbivSelectItem.Name = "lbivSelectItem";
            this.lbivSelectItem.SelectedIndex = -1;
            this.lbivSelectItem.Size = new System.Drawing.Size(293, 705);
            this.lbivSelectItem.TabIndex = 0;
            this.lbivSelectItem.ItemSelected += new System.EventHandler<int>(this.lbivSelectItem_ItemSelected);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.gbCommTerminal, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.gbCommunication, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.gbInformation, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(304, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 121F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 129F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 195F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(492, 709);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // gbCommTerminal
            // 
            this.gbCommTerminal.Controls.Add(this.tableLayoutPanel6);
            this.gbCommTerminal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCommTerminal.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbCommTerminal.Location = new System.Drawing.Point(3, 253);
            this.gbCommTerminal.Name = "gbCommTerminal";
            this.gbCommTerminal.Size = new System.Drawing.Size(486, 453);
            this.gbCommTerminal.TabIndex = 29;
            this.gbCommTerminal.TabStop = false;
            this.gbCommTerminal.Text = "Comm. Terminal";
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 1;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Controls.Add(this.tbLog, 0, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 0, 1);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 21);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 2;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 91.46919F));
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 8.530806F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(480, 429);
            this.tableLayoutPanel6.TabIndex = 23;
            // 
            // tbLog
            // 
            this.tbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLog.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tbLog.Location = new System.Drawing.Point(3, 3);
            this.tbLog.Name = "tbLog";
            this.tbLog.Size = new System.Drawing.Size(474, 386);
            this.tbLog.TabIndex = 1;
            this.tbLog.Text = "";
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 2;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.12264F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.87736F));
            this.tableLayoutPanel7.Controls.Add(this.btn_SendTest, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.tbSendText, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 395);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(474, 31);
            this.tableLayoutPanel7.TabIndex = 2;
            // 
            // btn_SendTest
            // 
            this.btn_SendTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_SendTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_SendTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_SendTest.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_SendTest.CustomForeColor = System.Drawing.Color.Black;
            this.btn_SendTest.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_SendTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SendTest.ForeColor = System.Drawing.Color.Black;
            this.btn_SendTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_SendTest.Location = new System.Drawing.Point(368, 3);
            this.btn_SendTest.Name = "btn_SendTest";
            this.btn_SendTest.Size = new System.Drawing.Size(103, 25);
            this.btn_SendTest.TabIndex = 24;
            this.btn_SendTest.TabStop = false;
            this.btn_SendTest.Text = "Send";
            this.btn_SendTest.UseVisualStyleBackColor = false;
            this.btn_SendTest.Click += new System.EventHandler(this.btn_SendTest_Click);
            // 
            // tbSendText
            // 
            this.tbSendText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSendText.Location = new System.Drawing.Point(3, 3);
            this.tbSendText.Name = "tbSendText";
            this.tbSendText.Size = new System.Drawing.Size(359, 25);
            this.tbSendText.TabIndex = 2;
            // 
            // gbCommunication
            // 
            this.gbCommunication.Controls.Add(this.btn_Initialize);
            this.gbCommunication.Controls.Add(this.btn_Setup);
            this.gbCommunication.Controls.Add(this.btn_Disconnect);
            this.gbCommunication.Controls.Add(this.btn_Connect);
            this.gbCommunication.Controls.Add(this.lbStatusCaption);
            this.gbCommunication.Controls.Add(this.lbStatusValue);
            this.gbCommunication.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCommunication.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbCommunication.Location = new System.Drawing.Point(3, 124);
            this.gbCommunication.Name = "gbCommunication";
            this.gbCommunication.Padding = new System.Windows.Forms.Padding(7);
            this.gbCommunication.Size = new System.Drawing.Size(486, 123);
            this.gbCommunication.TabIndex = 3;
            this.gbCommunication.TabStop = false;
            this.gbCommunication.Text = "Communication";
            // 
            // btn_Initialize
            // 
            this.btn_Initialize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Initialize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Initialize.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Initialize.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Initialize.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Initialize.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Initialize.ForeColor = System.Drawing.Color.Black;
            this.btn_Initialize.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Initialize.Location = new System.Drawing.Point(383, 70);
            this.btn_Initialize.Name = "btn_Initialize";
            this.btn_Initialize.Size = new System.Drawing.Size(90, 39);
            this.btn_Initialize.TabIndex = 24;
            this.btn_Initialize.TabStop = false;
            this.btn_Initialize.Text = "Initialize";
            this.btn_Initialize.UseVisualStyleBackColor = false;
            this.btn_Initialize.Click += new System.EventHandler(this.btn_Initialize_Click);
            // 
            // btn_Setup
            // 
            this.btn_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Setup.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Setup.Location = new System.Drawing.Point(198, 70);
            this.btn_Setup.Name = "btn_Setup";
            this.btn_Setup.Size = new System.Drawing.Size(90, 39);
            this.btn_Setup.TabIndex = 23;
            this.btn_Setup.TabStop = false;
            this.btn_Setup.Text = "Setup";
            this.btn_Setup.UseVisualStyleBackColor = false;
            this.btn_Setup.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btn_Setup_MouseClick);
            // 
            // btn_Disconnect
            // 
            this.btn_Disconnect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Disconnect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Disconnect.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Disconnect.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Disconnect.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Disconnect.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Disconnect.ForeColor = System.Drawing.Color.Black;
            this.btn_Disconnect.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Disconnect.Location = new System.Drawing.Point(105, 70);
            this.btn_Disconnect.Name = "btn_Disconnect";
            this.btn_Disconnect.Size = new System.Drawing.Size(90, 39);
            this.btn_Disconnect.TabIndex = 22;
            this.btn_Disconnect.TabStop = false;
            this.btn_Disconnect.Text = "Disconnect";
            this.btn_Disconnect.UseVisualStyleBackColor = false;
            this.btn_Disconnect.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btn_Disconnect_Click);
            // 
            // btn_Connect
            // 
            this.btn_Connect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Connect.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Connect.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Connect.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Connect.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Connect.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_Connect.ForeColor = System.Drawing.Color.Black;
            this.btn_Connect.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Connect.Location = new System.Drawing.Point(12, 70);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(90, 39);
            this.btn_Connect.TabIndex = 21;
            this.btn_Connect.TabStop = false;
            this.btn_Connect.Text = "Connect";
            this.btn_Connect.UseVisualStyleBackColor = false;
            this.btn_Connect.Click += new System.EventHandler(this.btn_Connect_Click);
            // 
            // lbStatusCaption
            // 
            this.lbStatusCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbStatusCaption.BorderWidth = 1;
            this.lbStatusCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbStatusCaption.Location = new System.Drawing.Point(13, 26);
            this.lbStatusCaption.Name = "lbStatusCaption";
            this.lbStatusCaption.Size = new System.Drawing.Size(90, 35);
            this.lbStatusCaption.TabIndex = 19;
            this.lbStatusCaption.Text = "Status";
            this.lbStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStatusValue
            // 
            this.lbStatusValue.BackColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderWidth = 1;
            this.lbStatusValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbStatusValue.ForeColor = System.Drawing.Color.Lime;
            this.lbStatusValue.Location = new System.Drawing.Point(106, 25);
            this.lbStatusValue.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lbStatusValue.Name = "lbStatusValue";
            this.lbStatusValue.Size = new System.Drawing.Size(367, 35);
            this.lbStatusValue.TabIndex = 20;
            this.lbStatusValue.Text = " - ";
            this.lbStatusValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gbInformation
            // 
            this.gbInformation.Controls.Add(this.lbSerialNumberCaption);
            this.gbInformation.Controls.Add(this.lbSerialNumberValue);
            this.gbInformation.Controls.Add(this.lbModelCaption);
            this.gbInformation.Controls.Add(this.lbModelValue);
            this.gbInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbInformation.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbInformation.Location = new System.Drawing.Point(3, 3);
            this.gbInformation.Name = "gbInformation";
            this.gbInformation.Padding = new System.Windows.Forms.Padding(7, 7, 10, 10);
            this.gbInformation.Size = new System.Drawing.Size(486, 115);
            this.gbInformation.TabIndex = 0;
            this.gbInformation.TabStop = false;
            this.gbInformation.Text = "Information";
            // 
            // lbSerialNumberCaption
            // 
            this.lbSerialNumberCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbSerialNumberCaption.BorderWidth = 1;
            this.lbSerialNumberCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbSerialNumberCaption.Location = new System.Drawing.Point(13, 66);
            this.lbSerialNumberCaption.Name = "lbSerialNumberCaption";
            this.lbSerialNumberCaption.Size = new System.Drawing.Size(90, 35);
            this.lbSerialNumberCaption.TabIndex = 19;
            this.lbSerialNumberCaption.Text = "Serial No.";
            this.lbSerialNumberCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbSerialNumberValue
            // 
            this.lbSerialNumberValue.BackColor = System.Drawing.Color.Black;
            this.lbSerialNumberValue.BorderColor = System.Drawing.Color.Black;
            this.lbSerialNumberValue.BorderWidth = 1;
            this.lbSerialNumberValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSerialNumberValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSerialNumberValue.Location = new System.Drawing.Point(106, 65);
            this.lbSerialNumberValue.Margin = new System.Windows.Forms.Padding(0);
            this.lbSerialNumberValue.Name = "lbSerialNumberValue";
            this.lbSerialNumberValue.Size = new System.Drawing.Size(367, 35);
            this.lbSerialNumberValue.TabIndex = 20;
            this.lbSerialNumberValue.Text = " - ";
            this.lbSerialNumberValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbModelCaption
            // 
            this.lbModelCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbModelCaption.BorderWidth = 1;
            this.lbModelCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbModelCaption.Location = new System.Drawing.Point(13, 28);
            this.lbModelCaption.Name = "lbModelCaption";
            this.lbModelCaption.Size = new System.Drawing.Size(90, 35);
            this.lbModelCaption.TabIndex = 17;
            this.lbModelCaption.Text = "Model";
            this.lbModelCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbModelValue
            // 
            this.lbModelValue.BackColor = System.Drawing.Color.Black;
            this.lbModelValue.BorderColor = System.Drawing.Color.Black;
            this.lbModelValue.BorderWidth = 1;
            this.lbModelValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbModelValue.ForeColor = System.Drawing.Color.Lime;
            this.lbModelValue.Location = new System.Drawing.Point(106, 27);
            this.lbModelValue.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.lbModelValue.Name = "lbModelValue";
            this.lbModelValue.Size = new System.Drawing.Size(367, 35);
            this.lbModelValue.TabIndex = 18;
            this.lbModelValue.Text = " - ";
            this.lbModelValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.gbChannel, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(802, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 53.7037F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.2963F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(445, 709);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.btnSave, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.pcvConfig, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.48787F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.51213F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(439, 374);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSave.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSave.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.CustomForeColor = System.Drawing.Color.Black;
            this.btnSave.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSave.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSave.Location = new System.Drawing.Point(346, 337);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 34);
            this.btnSave.TabIndex = 27;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // pcvConfig
            // 
            this.pcvConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcvConfig.GroupName = "Property";
            this.pcvConfig.Location = new System.Drawing.Point(0, 0);
            this.pcvConfig.Margin = new System.Windows.Forms.Padding(0);
            this.pcvConfig.Name = "pcvConfig";
            this.pcvConfig.Size = new System.Drawing.Size(439, 334);
            this.pcvConfig.TabIndex = 7;
            // 
            // gbChannel
            // 
            this.gbChannel.Controls.Add(this.tableLayoutPanel5);
            this.gbChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbChannel.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbChannel.Location = new System.Drawing.Point(3, 383);
            this.gbChannel.Name = "gbChannel";
            this.gbChannel.Size = new System.Drawing.Size(439, 323);
            this.gbChannel.TabIndex = 1;
            this.gbChannel.TabStop = false;
            this.gbChannel.Text = "Channel";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.lbivSelectChannel, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 21);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 86.77966F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 13.22034F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(433, 299);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // lbivSelectChannel
            // 
            this.lbivSelectChannel.BorderWidth = 2;
            this.lbivSelectChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelectChannel.GroupName = "Select Item";
            this.lbivSelectChannel.Location = new System.Drawing.Point(4, 7);
            this.lbivSelectChannel.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lbivSelectChannel.Name = "lbivSelectChannel";
            this.lbivSelectChannel.SelectedIndex = -1;
            this.lbivSelectChannel.Size = new System.Drawing.Size(425, 245);
            this.lbivSelectChannel.TabIndex = 1;
            this.lbivSelectChannel.ItemSelected += new System.EventHandler<int>(this.lbivSelectChannel_ItemSelected);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnChannelTest);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 262);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(427, 34);
            this.panel1.TabIndex = 2;
            // 
            // btnChannelTest
            // 
            this.btnChannelTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnChannelTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnChannelTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnChannelTest.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnChannelTest.CustomForeColor = System.Drawing.Color.Black;
            this.btnChannelTest.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnChannelTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnChannelTest.ForeColor = System.Drawing.Color.Black;
            this.btnChannelTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btnChannelTest.Location = new System.Drawing.Point(337, 0);
            this.btnChannelTest.Name = "btnChannelTest";
            this.btnChannelTest.Size = new System.Drawing.Size(90, 34);
            this.btnChannelTest.TabIndex = 28;
            this.btnChannelTest.TabStop = false;
            this.btnChannelTest.Text = "Test";
            this.btnChannelTest.UseVisualStyleBackColor = false;
            this.btnChannelTest.Click += new System.EventHandler(this.btnChannelTest_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 27);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1249, 714);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Spectrometer";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // Instrument_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Instrument_Setup";
            this.Text = "Instrument_Setup";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Instrument_Setup_FormClosed);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.gbCommTerminal.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.gbCommunication.ResumeLayout(false);
            this.gbInformation.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.gbChannel.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.ListBoxItemsView lbivSelectItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox gbInformation;
        private Common.CustomControl.CustomBorderLabel lbModelCaption;
        private Common.CustomControl.CustomBorderLabel lbModelValue;
        private Common.CustomControl.CustomBorderLabel lbSerialNumberCaption;
        private Common.CustomControl.CustomBorderLabel lbSerialNumberValue;
        private System.Windows.Forms.GroupBox gbCommunication;
        private Common.IndividualMenuButton btn_Initialize;
        private Common.IndividualMenuButton btn_Setup;
        private Common.IndividualMenuButton btn_Disconnect;
        private Common.IndividualMenuButton btn_Connect;
        private Common.CustomControl.CustomBorderLabel lbStatusCaption;
        private Common.CustomControl.CustomBorderLabel lbStatusValue;
        private System.Windows.Forms.GroupBox gbCommTerminal;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.RichTextBox tbLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Common.IndividualMenuButton btn_SendTest;
        private System.Windows.Forms.TextBox tbSendText;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private Common.IndividualMenuButton btnSave;
        private Common.PropertyCollectionView pcvConfig;
        private System.Windows.Forms.GroupBox gbChannel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private Common.ListBoxItemsView lbivSelectChannel;
        private System.Windows.Forms.Panel panel1;
        private Common.IndividualMenuButton btnChannelTest;
    }
}