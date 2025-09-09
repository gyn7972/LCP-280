namespace QMC.LCP_280.Process.Unit.FormSetup.Page
{
    partial class SourcemeterPage
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
            this.lbivSelectSourcemeter = new QMC.Common.ListBoxItemsView();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.gbCommTerminal = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tbLog = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_SendTest = new QMC.Common.IndividualMenuButton();
            this.tbSendText = new System.Windows.Forms.TextBox();
            this.gbCommunication = new System.Windows.Forms.GroupBox();
            this.btn_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Initialize = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel11 = new System.Windows.Forms.TableLayoutPanel();
            this.lbStatusCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbStatusValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.gbInformation = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel10 = new System.Windows.Forms.TableLayoutPanel();
            this.lbModelCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSerialNumberCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lbSerialNumberValue = new QMC.Common.CustomControl.CustomBorderLabel();
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
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.gbCommTerminal.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            this.gbCommunication.SuspendLayout();
            this.tableLayoutPanel11.SuspendLayout();
            this.gbInformation.SuspendLayout();
            this.tableLayoutPanel10.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.gbChannel.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 301F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52.54777F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 47.45223F));
            this.tableLayoutPanel1.Controls.Add(this.lbivSelectSourcemeter, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // lbivSelectSourcemeter
            // 
            this.lbivSelectSourcemeter.BorderColor = System.Drawing.Color.White;
            this.lbivSelectSourcemeter.BorderWidth = 2;
            this.lbivSelectSourcemeter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelectSourcemeter.GroupBackColor = System.Drawing.Color.White;
            this.lbivSelectSourcemeter.GroupForeColor = System.Drawing.Color.Black;
            this.lbivSelectSourcemeter.GroupName = "Select Item";
            this.lbivSelectSourcemeter.ItemBackColor = System.Drawing.Color.Black;
            this.lbivSelectSourcemeter.ItemForeColor = System.Drawing.Color.Lime;
            this.lbivSelectSourcemeter.ListBackColor = System.Drawing.Color.Black;
            this.lbivSelectSourcemeter.ListForeColor = System.Drawing.Color.Lime;
            this.lbivSelectSourcemeter.Location = new System.Drawing.Point(4, 5);
            this.lbivSelectSourcemeter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lbivSelectSourcemeter.Name = "lbivSelectSourcemeter";
            this.lbivSelectSourcemeter.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.lbivSelectSourcemeter.SelectedForeColor = System.Drawing.Color.Black;
            this.lbivSelectSourcemeter.SelectedIndex = -1;
            this.lbivSelectSourcemeter.Size = new System.Drawing.Size(293, 690);
            this.lbivSelectSourcemeter.TabIndex = 0;
            this.lbivSelectSourcemeter.ItemSelected += new System.EventHandler<int>(this.lbivSelectSourcemeter_ItemSelected);
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
            this.tableLayoutPanel2.Size = new System.Drawing.Size(492, 694);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // gbCommTerminal
            // 
            this.gbCommTerminal.Controls.Add(this.tableLayoutPanel6);
            this.gbCommTerminal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCommTerminal.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbCommTerminal.Location = new System.Drawing.Point(3, 253);
            this.gbCommTerminal.Name = "gbCommTerminal";
            this.gbCommTerminal.Size = new System.Drawing.Size(486, 438);
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
            this.tableLayoutPanel6.Size = new System.Drawing.Size(480, 414);
            this.tableLayoutPanel6.TabIndex = 23;
            // 
            // tbLog
            // 
            this.tbLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbLog.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tbLog.Location = new System.Drawing.Point(3, 3);
            this.tbLog.Name = "tbLog";
            this.tbLog.Size = new System.Drawing.Size(474, 372);
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
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 381);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(474, 30);
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
            this.btn_SendTest.Size = new System.Drawing.Size(103, 24);
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
            this.gbCommunication.Controls.Add(this.btn_Setup);
            this.gbCommunication.Controls.Add(this.btn_Initialize);
            this.gbCommunication.Controls.Add(this.tableLayoutPanel11);
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
            this.btn_Setup.Location = new System.Drawing.Point(6, 72);
            this.btn_Setup.Name = "btn_Setup";
            this.btn_Setup.Size = new System.Drawing.Size(90, 39);
            this.btn_Setup.TabIndex = 23;
            this.btn_Setup.TabStop = false;
            this.btn_Setup.Text = "Setup";
            this.btn_Setup.UseVisualStyleBackColor = false;
            this.btn_Setup.Click += new System.EventHandler(this.btn_Setup_Click);
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
            this.btn_Initialize.Location = new System.Drawing.Point(390, 72);
            this.btn_Initialize.Name = "btn_Initialize";
            this.btn_Initialize.Size = new System.Drawing.Size(90, 39);
            this.btn_Initialize.TabIndex = 24;
            this.btn_Initialize.TabStop = false;
            this.btn_Initialize.Text = "Initialize";
            this.btn_Initialize.UseVisualStyleBackColor = false;
            this.btn_Initialize.Click += new System.EventHandler(this.btn_Initialize_Click);
            // 
            // tableLayoutPanel11
            // 
            this.tableLayoutPanel11.ColumnCount = 2;
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 21.72996F));
            this.tableLayoutPanel11.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 78.27004F));
            this.tableLayoutPanel11.Controls.Add(this.lbStatusCaption, 0, 0);
            this.tableLayoutPanel11.Controls.Add(this.lbStatusValue, 1, 0);
            this.tableLayoutPanel11.Location = new System.Drawing.Point(6, 25);
            this.tableLayoutPanel11.Name = "tableLayoutPanel11";
            this.tableLayoutPanel11.RowCount = 1;
            this.tableLayoutPanel11.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel11.Size = new System.Drawing.Size(474, 42);
            this.tableLayoutPanel11.TabIndex = 25;
            // 
            // lbStatusCaption
            // 
            this.lbStatusCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbStatusCaption.BorderWidth = 1;
            this.lbStatusCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStatusCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbStatusCaption.Location = new System.Drawing.Point(0, 3);
            this.lbStatusCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.lbStatusCaption.Name = "lbStatusCaption";
            this.lbStatusCaption.Size = new System.Drawing.Size(103, 36);
            this.lbStatusCaption.TabIndex = 19;
            this.lbStatusCaption.Text = "Status";
            this.lbStatusCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbStatusValue
            // 
            this.lbStatusValue.BackColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderColor = System.Drawing.Color.Black;
            this.lbStatusValue.BorderWidth = 1;
            this.lbStatusValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbStatusValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbStatusValue.ForeColor = System.Drawing.Color.Lime;
            this.lbStatusValue.Location = new System.Drawing.Point(106, 3);
            this.lbStatusValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbStatusValue.Name = "lbStatusValue";
            this.lbStatusValue.Size = new System.Drawing.Size(365, 36);
            this.lbStatusValue.TabIndex = 20;
            this.lbStatusValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // gbInformation
            // 
            this.gbInformation.Controls.Add(this.tableLayoutPanel10);
            this.gbInformation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbInformation.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbInformation.Location = new System.Drawing.Point(3, 3);
            this.gbInformation.Name = "gbInformation";
            this.gbInformation.Padding = new System.Windows.Forms.Padding(7);
            this.gbInformation.Size = new System.Drawing.Size(486, 115);
            this.gbInformation.TabIndex = 0;
            this.gbInformation.TabStop = false;
            this.gbInformation.Text = "Information";
            // 
            // tableLayoutPanel10
            // 
            this.tableLayoutPanel10.ColumnCount = 2;
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.12766F));
            this.tableLayoutPanel10.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.87234F));
            this.tableLayoutPanel10.Controls.Add(this.lbModelCaption, 0, 0);
            this.tableLayoutPanel10.Controls.Add(this.lbSerialNumberCaption, 0, 1);
            this.tableLayoutPanel10.Controls.Add(this.lbSerialNumberValue, 1, 1);
            this.tableLayoutPanel10.Controls.Add(this.lbModelValue, 1, 0);
            this.tableLayoutPanel10.Location = new System.Drawing.Point(6, 24);
            this.tableLayoutPanel10.Name = "tableLayoutPanel10";
            this.tableLayoutPanel10.RowCount = 2;
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel10.Size = new System.Drawing.Size(474, 77);
            this.tableLayoutPanel10.TabIndex = 21;
            // 
            // lbModelCaption
            // 
            this.lbModelCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbModelCaption.BorderWidth = 1;
            this.lbModelCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbModelCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbModelCaption.Location = new System.Drawing.Point(0, 3);
            this.lbModelCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.lbModelCaption.Name = "lbModelCaption";
            this.lbModelCaption.Size = new System.Drawing.Size(104, 32);
            this.lbModelCaption.TabIndex = 17;
            this.lbModelCaption.Text = "Model";
            this.lbModelCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbSerialNumberCaption
            // 
            this.lbSerialNumberCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lbSerialNumberCaption.BorderWidth = 1;
            this.lbSerialNumberCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSerialNumberCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lbSerialNumberCaption.Location = new System.Drawing.Point(0, 41);
            this.lbSerialNumberCaption.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.lbSerialNumberCaption.Name = "lbSerialNumberCaption";
            this.lbSerialNumberCaption.Size = new System.Drawing.Size(104, 33);
            this.lbSerialNumberCaption.TabIndex = 19;
            this.lbSerialNumberCaption.Text = "Serial No.";
            this.lbSerialNumberCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbSerialNumberValue
            // 
            this.lbSerialNumberValue.BackColor = System.Drawing.Color.Black;
            this.lbSerialNumberValue.BorderColor = System.Drawing.Color.Black;
            this.lbSerialNumberValue.BorderWidth = 1;
            this.lbSerialNumberValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbSerialNumberValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbSerialNumberValue.ForeColor = System.Drawing.Color.Lime;
            this.lbSerialNumberValue.Location = new System.Drawing.Point(107, 41);
            this.lbSerialNumberValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbSerialNumberValue.Name = "lbSerialNumberValue";
            this.lbSerialNumberValue.Size = new System.Drawing.Size(364, 33);
            this.lbSerialNumberValue.TabIndex = 20;
            this.lbSerialNumberValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lbModelValue
            // 
            this.lbModelValue.BackColor = System.Drawing.Color.Black;
            this.lbModelValue.BorderColor = System.Drawing.Color.Black;
            this.lbModelValue.BorderWidth = 1;
            this.lbModelValue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbModelValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lbModelValue.ForeColor = System.Drawing.Color.Lime;
            this.lbModelValue.Location = new System.Drawing.Point(107, 3);
            this.lbModelValue.Margin = new System.Windows.Forms.Padding(3);
            this.lbModelValue.Name = "lbModelValue";
            this.lbModelValue.Size = new System.Drawing.Size(364, 32);
            this.lbModelValue.TabIndex = 18;
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
            this.tableLayoutPanel3.Size = new System.Drawing.Size(445, 694);
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
            this.tableLayoutPanel4.Size = new System.Drawing.Size(439, 366);
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
            this.btnSave.Location = new System.Drawing.Point(346, 330);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 33);
            this.btnSave.TabIndex = 27;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // pcvConfig
            // 
            this.pcvConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pcvConfig.FastBuild = true;
            this.pcvConfig.GroupName = "Property";
            this.pcvConfig.Location = new System.Drawing.Point(0, 0);
            this.pcvConfig.Margin = new System.Windows.Forms.Padding(0);
            this.pcvConfig.Name = "pcvConfig";
            this.pcvConfig.Size = new System.Drawing.Size(439, 327);
            this.pcvConfig.SuppressResizeInvalidation = true;
            this.pcvConfig.TabIndex = 7;
            // 
            // gbChannel
            // 
            this.gbChannel.Controls.Add(this.tableLayoutPanel5);
            this.gbChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbChannel.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbChannel.Location = new System.Drawing.Point(3, 375);
            this.gbChannel.Name = "gbChannel";
            this.gbChannel.Size = new System.Drawing.Size(439, 316);
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
            this.tableLayoutPanel5.Size = new System.Drawing.Size(433, 292);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // lbivSelectChannel
            // 
            this.lbivSelectChannel.BorderColor = System.Drawing.Color.White;
            this.lbivSelectChannel.BorderWidth = 2;
            this.lbivSelectChannel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbivSelectChannel.GroupBackColor = System.Drawing.Color.White;
            this.lbivSelectChannel.GroupForeColor = System.Drawing.Color.Black;
            this.lbivSelectChannel.GroupName = "Select Item";
            this.lbivSelectChannel.ItemBackColor = System.Drawing.Color.Black;
            this.lbivSelectChannel.ItemForeColor = System.Drawing.Color.Lime;
            this.lbivSelectChannel.ListBackColor = System.Drawing.Color.Black;
            this.lbivSelectChannel.ListForeColor = System.Drawing.Color.Lime;
            this.lbivSelectChannel.Location = new System.Drawing.Point(4, 7);
            this.lbivSelectChannel.Margin = new System.Windows.Forms.Padding(4, 7, 4, 7);
            this.lbivSelectChannel.Name = "lbivSelectChannel";
            this.lbivSelectChannel.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.lbivSelectChannel.SelectedForeColor = System.Drawing.Color.Black;
            this.lbivSelectChannel.SelectedIndex = -1;
            this.lbivSelectChannel.Size = new System.Drawing.Size(425, 239);
            this.lbivSelectChannel.TabIndex = 1;
            this.lbivSelectChannel.ItemSelected += new System.EventHandler<int>(this.lbivSelectChannel_ItemSelected);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnChannelTest);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 256);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(427, 33);
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
            this.btnChannelTest.Size = new System.Drawing.Size(90, 33);
            this.btnChannelTest.TabIndex = 28;
            this.btnChannelTest.TabStop = false;
            this.btnChannelTest.Text = "Test";
            this.btnChannelTest.UseVisualStyleBackColor = false;
            this.btnChannelTest.Click += new System.EventHandler(this.btnChannelTest_Click);
            // 
            // SourcemeterPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourcemeterPage";
            this.Size = new System.Drawing.Size(1250, 700);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.gbCommTerminal.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.gbCommunication.ResumeLayout(false);
            this.tableLayoutPanel11.ResumeLayout(false);
            this.gbInformation.ResumeLayout(false);
            this.tableLayoutPanel10.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.gbChannel.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Common.ListBoxItemsView lbivSelectSourcemeter;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox gbCommTerminal;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.RichTextBox tbLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private Common.IndividualMenuButton btn_SendTest;
        private System.Windows.Forms.TextBox tbSendText;
        private System.Windows.Forms.GroupBox gbCommunication;
        private Common.IndividualMenuButton btn_Setup;
        private Common.IndividualMenuButton btn_Initialize;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel11;
        private Common.CustomControl.CustomBorderLabel lbStatusCaption;
        private Common.CustomControl.CustomBorderLabel lbStatusValue;
        private System.Windows.Forms.GroupBox gbInformation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel10;
        private Common.CustomControl.CustomBorderLabel lbModelCaption;
        private Common.CustomControl.CustomBorderLabel lbSerialNumberCaption;
        private Common.CustomControl.CustomBorderLabel lbSerialNumberValue;
        private Common.CustomControl.CustomBorderLabel lbModelValue;
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
