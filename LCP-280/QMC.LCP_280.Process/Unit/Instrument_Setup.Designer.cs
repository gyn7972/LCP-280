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
            this.gbCommunication = new System.Windows.Forms.GroupBox();
            this.gbCommTerminal = new System.Windows.Forms.GroupBox();
            this.btn_SendTest = new QMC.Common.IndividualMenuButton();
            this.tbSendText = new System.Windows.Forms.TextBox();
            this.tbLog = new System.Windows.Forms.RichTextBox();
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
            this.configPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.gbControl = new System.Windows.Forms.GroupBox();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.gbCommunication.SuspendLayout();
            this.gbCommTerminal.SuspendLayout();
            this.gbInformation.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(4, 5);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1257, 745);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel1);
            this.tabPage1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tabPage1.Location = new System.Drawing.Point(4, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1249, 714);
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
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1243, 708);
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
            this.lbivSelectItem.Size = new System.Drawing.Size(293, 698);
            this.lbivSelectItem.TabIndex = 0;
            this.lbivSelectItem.ItemSelected += new System.EventHandler<int>(this.lbivSelectItem_ItemSelected);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.gbCommunication, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.gbInformation, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(304, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.74392F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.25608F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(489, 702);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // gbCommunication
            // 
            this.gbCommunication.Controls.Add(this.gbCommTerminal);
            this.gbCommunication.Controls.Add(this.btn_Initialize);
            this.gbCommunication.Controls.Add(this.btn_Setup);
            this.gbCommunication.Controls.Add(this.btn_Disconnect);
            this.gbCommunication.Controls.Add(this.btn_Connect);
            this.gbCommunication.Controls.Add(this.lbStatusCaption);
            this.gbCommunication.Controls.Add(this.lbStatusValue);
            this.gbCommunication.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCommunication.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbCommunication.Location = new System.Drawing.Point(3, 148);
            this.gbCommunication.Name = "gbCommunication";
            this.gbCommunication.Padding = new System.Windows.Forms.Padding(7);
            this.gbCommunication.Size = new System.Drawing.Size(483, 551);
            this.gbCommunication.TabIndex = 1;
            this.gbCommunication.TabStop = false;
            this.gbCommunication.Text = "Communication";
            // 
            // gbCommTerminal
            // 
            this.gbCommTerminal.Controls.Add(this.btn_SendTest);
            this.gbCommTerminal.Controls.Add(this.tbSendText);
            this.gbCommTerminal.Controls.Add(this.tbLog);
            this.gbCommTerminal.Location = new System.Drawing.Point(10, 124);
            this.gbCommTerminal.Name = "gbCommTerminal";
            this.gbCommTerminal.Padding = new System.Windows.Forms.Padding(7);
            this.gbCommTerminal.Size = new System.Drawing.Size(463, 415);
            this.gbCommTerminal.TabIndex = 25;
            this.gbCommTerminal.TabStop = false;
            this.gbCommTerminal.Text = "Comm. Terminal";
            // 
            // btn_SendTest
            // 
            this.btn_SendTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_SendTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_SendTest.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_SendTest.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_SendTest.CustomForeColor = System.Drawing.Color.Black;
            this.btn_SendTest.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_SendTest.ForeColor = System.Drawing.Color.Black;
            this.btn_SendTest.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_SendTest.Location = new System.Drawing.Point(363, 380);
            this.btn_SendTest.Name = "btn_SendTest";
            this.btn_SendTest.Size = new System.Drawing.Size(90, 25);
            this.btn_SendTest.TabIndex = 22;
            this.btn_SendTest.TabStop = false;
            this.btn_SendTest.Text = "Send";
            this.btn_SendTest.UseVisualStyleBackColor = false;
            this.btn_SendTest.Click += new System.EventHandler(this.btn_SendTest_Click);
            // 
            // tbSendText
            // 
            this.tbSendText.Location = new System.Drawing.Point(10, 380);
            this.tbSendText.Name = "tbSendText";
            this.tbSendText.Size = new System.Drawing.Size(347, 25);
            this.tbSendText.TabIndex = 1;
            // 
            // tbLog
            // 
            this.tbLog.Location = new System.Drawing.Point(10, 28);
            this.tbLog.Name = "tbLog";
            this.tbLog.Size = new System.Drawing.Size(443, 346);
            this.tbLog.TabIndex = 0;
            this.tbLog.Text = "";
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
            this.btn_Setup.Click += new System.EventHandler(this.btn_Setup_Click);
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
            this.btn_Disconnect.Click += new System.EventHandler(this.btn_Disconnect_Click);
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
            this.gbInformation.Size = new System.Drawing.Size(483, 139);
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
            this.tableLayoutPanel3.Controls.Add(this.configPropertyCollectionView, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.gbControl, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.btnSave, 0, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(799, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 324F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.94118F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 87.05882F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(441, 702);
            this.tableLayoutPanel3.TabIndex = 2;
            // 
            // configPropertyCollectionView
            // 
            this.configPropertyCollectionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configPropertyCollectionView.GroupName = "Property";
            this.configPropertyCollectionView.Location = new System.Drawing.Point(3, 3);
            this.configPropertyCollectionView.Name = "configPropertyCollectionView";
            this.configPropertyCollectionView.Size = new System.Drawing.Size(435, 318);
            this.configPropertyCollectionView.TabIndex = 5;
            // 
            // gbControl
            // 
            this.gbControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbControl.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbControl.Location = new System.Drawing.Point(3, 375);
            this.gbControl.Name = "gbControl";
            this.gbControl.Padding = new System.Windows.Forms.Padding(7);
            this.gbControl.Size = new System.Drawing.Size(435, 324);
            this.gbControl.TabIndex = 2;
            this.gbControl.TabStop = false;
            this.gbControl.Text = "Control";
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
            this.btnSave.Location = new System.Drawing.Point(348, 327);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 42);
            this.btnSave.TabIndex = 25;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            this.gbCommunication.ResumeLayout(false);
            this.gbCommTerminal.ResumeLayout(false);
            this.gbCommTerminal.PerformLayout();
            this.gbInformation.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.ListBoxItemsView lbivSelectItem;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox gbCommunication;
        private System.Windows.Forms.GroupBox gbInformation;
        private Common.CustomControl.CustomBorderLabel lbModelCaption;
        private Common.CustomControl.CustomBorderLabel lbModelValue;
        private Common.CustomControl.CustomBorderLabel lbSerialNumberCaption;
        private Common.CustomControl.CustomBorderLabel lbSerialNumberValue;
        private Common.CustomControl.CustomBorderLabel lbStatusCaption;
        private Common.CustomControl.CustomBorderLabel lbStatusValue;
        private Common.IndividualMenuButton btn_Connect;
        private Common.IndividualMenuButton btn_Initialize;
        private Common.IndividualMenuButton btn_Setup;
        private Common.IndividualMenuButton btn_Disconnect;
        private System.Windows.Forms.GroupBox gbCommTerminal;
        private Common.IndividualMenuButton btn_SendTest;
        private System.Windows.Forms.TextBox tbSendText;
        private System.Windows.Forms.RichTextBox tbLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.GroupBox gbControl;
        private Common.PropertyCollectionView configPropertyCollectionView;
        private Common.IndividualMenuButton btnSave;
    }
}