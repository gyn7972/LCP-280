using System.Drawing;

namespace QMC.LCP_280.Process.Unit.FormMain
{
    partial class SequenceManualControl
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
            if (disposing)
            {
                // 이벤트 해제
                foreach (var button in _buttonActions.Keys)
                {
                    button.Click -= OnSequenceButtonClick;
                }

                if (components != null)
                {
                    components.Dispose();
                }
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
            this.tableLayoutPanel8 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_Start_OutputWafer = new QMC.Common.IndividualMenuButton();
            this.btn_Start_ChipUnloading = new QMC.Common.IndividualMenuButton();
            this.btn_Start_Process = new QMC.Common.IndividualMenuButton();
            this.btn_Start_ChipLoading = new QMC.Common.IndividualMenuButton();
            this.btn_Ready_OutputWafer = new QMC.Common.IndividualMenuButton();
            this.btn_Ready_ChipUnloading = new QMC.Common.IndividualMenuButton();
            this.btn_Ready_Process = new QMC.Common.IndividualMenuButton();
            this.btn_Ready_ChipLoading = new QMC.Common.IndividualMenuButton();
            this.btn_Start_InputWafer = new QMC.Common.IndividualMenuButton();
            this.btn_Ready_InputWafer = new QMC.Common.IndividualMenuButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btn_Reset_InputWafer = new QMC.Common.IndividualMenuButton();
            this.btn_Reset_ChipLoading = new QMC.Common.IndividualMenuButton();
            this.btn_Reset_Process = new QMC.Common.IndividualMenuButton();
            this.btn_Reset_ChipUnloading = new QMC.Common.IndividualMenuButton();
            this.btn_Reset_OutputWafer = new QMC.Common.IndividualMenuButton();
            this.tableLayoutPanel8.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel8
            // 
            this.tableLayoutPanel8.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel8.ColumnCount = 5;
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.Controls.Add(this.btn_Reset_OutputWafer, 4, 3);
            this.tableLayoutPanel8.Controls.Add(this.btn_Reset_ChipUnloading, 3, 3);
            this.tableLayoutPanel8.Controls.Add(this.btn_Reset_Process, 2, 3);
            this.tableLayoutPanel8.Controls.Add(this.btn_Reset_ChipLoading, 1, 3);
            this.tableLayoutPanel8.Controls.Add(this.btn_Reset_InputWafer, 0, 3);
            this.tableLayoutPanel8.Controls.Add(this.btn_Start_InputWafer, 0, 2);
            this.tableLayoutPanel8.Controls.Add(this.btn_Ready_InputWafer, 0, 1);
            this.tableLayoutPanel8.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Start_ChipLoading, 1, 2);
            this.tableLayoutPanel8.Controls.Add(this.btn_Start_Process, 2, 2);
            this.tableLayoutPanel8.Controls.Add(this.btn_Start_ChipUnloading, 3, 2);
            this.tableLayoutPanel8.Controls.Add(this.btn_Start_OutputWafer, 4, 2);
            this.tableLayoutPanel8.Controls.Add(this.label2, 1, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Ready_ChipLoading, 1, 1);
            this.tableLayoutPanel8.Controls.Add(this.label3, 2, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Ready_Process, 2, 1);
            this.tableLayoutPanel8.Controls.Add(this.label4, 3, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Ready_ChipUnloading, 3, 1);
            this.tableLayoutPanel8.Controls.Add(this.label5, 4, 0);
            this.tableLayoutPanel8.Controls.Add(this.btn_Ready_OutputWafer, 4, 1);
            this.tableLayoutPanel8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel8.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel8.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel8.Name = "tableLayoutPanel8";
            this.tableLayoutPanel8.RowCount = 5;
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel8.Size = new System.Drawing.Size(886, 483);
            this.tableLayoutPanel8.TabIndex = 3;
            // 
            // btn_Start_OutputWafer
            // 
            this.btn_Start_OutputWafer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_OutputWafer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Start_OutputWafer.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_OutputWafer.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_OutputWafer.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Start_OutputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Start_OutputWafer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_OutputWafer.ForeColor = System.Drawing.Color.Black;
            this.btn_Start_OutputWafer.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Start_OutputWafer.Location = new System.Drawing.Point(712, 196);
            this.btn_Start_OutputWafer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Start_OutputWafer.Name = "btn_Start_OutputWafer";
            this.btn_Start_OutputWafer.Size = new System.Drawing.Size(170, 88);
            this.btn_Start_OutputWafer.TabIndex = 30;
            this.btn_Start_OutputWafer.TabStop = false;
            this.btn_Start_OutputWafer.Text = "Start";
            this.btn_Start_OutputWafer.UseVisualStyleBackColor = false;
            // 
            // btn_Start_ChipUnloading
            // 
            this.btn_Start_ChipUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_ChipUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Start_ChipUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_ChipUnloading.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_ChipUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Start_ChipUnloading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Start_ChipUnloading.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_ChipUnloading.ForeColor = System.Drawing.Color.Black;
            this.btn_Start_ChipUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Start_ChipUnloading.Location = new System.Drawing.Point(535, 196);
            this.btn_Start_ChipUnloading.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Start_ChipUnloading.Name = "btn_Start_ChipUnloading";
            this.btn_Start_ChipUnloading.Size = new System.Drawing.Size(169, 88);
            this.btn_Start_ChipUnloading.TabIndex = 29;
            this.btn_Start_ChipUnloading.TabStop = false;
            this.btn_Start_ChipUnloading.Text = "Start";
            this.btn_Start_ChipUnloading.UseVisualStyleBackColor = false;
            // 
            // btn_Start_Process
            // 
            this.btn_Start_Process.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_Process.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Start_Process.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_Process.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_Process.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Start_Process.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Start_Process.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_Process.ForeColor = System.Drawing.Color.Black;
            this.btn_Start_Process.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Start_Process.Location = new System.Drawing.Point(358, 196);
            this.btn_Start_Process.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Start_Process.Name = "btn_Start_Process";
            this.btn_Start_Process.Size = new System.Drawing.Size(169, 88);
            this.btn_Start_Process.TabIndex = 28;
            this.btn_Start_Process.TabStop = false;
            this.btn_Start_Process.Text = "Start";
            this.btn_Start_Process.UseVisualStyleBackColor = false;
            // 
            // btn_Start_ChipLoading
            // 
            this.btn_Start_ChipLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_ChipLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Start_ChipLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_ChipLoading.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_ChipLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Start_ChipLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Start_ChipLoading.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_ChipLoading.ForeColor = System.Drawing.Color.Black;
            this.btn_Start_ChipLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Start_ChipLoading.Location = new System.Drawing.Point(181, 196);
            this.btn_Start_ChipLoading.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Start_ChipLoading.Name = "btn_Start_ChipLoading";
            this.btn_Start_ChipLoading.Size = new System.Drawing.Size(169, 88);
            this.btn_Start_ChipLoading.TabIndex = 27;
            this.btn_Start_ChipLoading.TabStop = false;
            this.btn_Start_ChipLoading.Text = "Start";
            this.btn_Start_ChipLoading.UseVisualStyleBackColor = false;
            // 
            // btn_Ready_OutputWafer
            // 
            this.btn_Ready_OutputWafer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_OutputWafer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Ready_OutputWafer.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_OutputWafer.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_OutputWafer.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Ready_OutputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Ready_OutputWafer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_OutputWafer.ForeColor = System.Drawing.Color.Black;
            this.btn_Ready_OutputWafer.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Ready_OutputWafer.Location = new System.Drawing.Point(712, 100);
            this.btn_Ready_OutputWafer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Ready_OutputWafer.Name = "btn_Ready_OutputWafer";
            this.btn_Ready_OutputWafer.Size = new System.Drawing.Size(170, 88);
            this.btn_Ready_OutputWafer.TabIndex = 26;
            this.btn_Ready_OutputWafer.TabStop = false;
            this.btn_Ready_OutputWafer.Text = "Ready";
            this.btn_Ready_OutputWafer.UseVisualStyleBackColor = false;
            // 
            // btn_Ready_ChipUnloading
            // 
            this.btn_Ready_ChipUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_ChipUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Ready_ChipUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_ChipUnloading.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_ChipUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Ready_ChipUnloading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Ready_ChipUnloading.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_ChipUnloading.ForeColor = System.Drawing.Color.Black;
            this.btn_Ready_ChipUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Ready_ChipUnloading.Location = new System.Drawing.Point(535, 100);
            this.btn_Ready_ChipUnloading.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Ready_ChipUnloading.Name = "btn_Ready_ChipUnloading";
            this.btn_Ready_ChipUnloading.Size = new System.Drawing.Size(169, 88);
            this.btn_Ready_ChipUnloading.TabIndex = 25;
            this.btn_Ready_ChipUnloading.TabStop = false;
            this.btn_Ready_ChipUnloading.Text = "Ready";
            this.btn_Ready_ChipUnloading.UseVisualStyleBackColor = false;
            // 
            // btn_Ready_Process
            // 
            this.btn_Ready_Process.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_Process.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Ready_Process.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_Process.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_Process.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Ready_Process.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Ready_Process.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_Process.ForeColor = System.Drawing.Color.Black;
            this.btn_Ready_Process.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Ready_Process.Location = new System.Drawing.Point(358, 100);
            this.btn_Ready_Process.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Ready_Process.Name = "btn_Ready_Process";
            this.btn_Ready_Process.Size = new System.Drawing.Size(169, 88);
            this.btn_Ready_Process.TabIndex = 24;
            this.btn_Ready_Process.TabStop = false;
            this.btn_Ready_Process.Text = "Ready";
            this.btn_Ready_Process.UseVisualStyleBackColor = false;
            // 
            // btn_Ready_ChipLoading
            // 
            this.btn_Ready_ChipLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_ChipLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Ready_ChipLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_ChipLoading.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_ChipLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Ready_ChipLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Ready_ChipLoading.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_ChipLoading.ForeColor = System.Drawing.Color.Black;
            this.btn_Ready_ChipLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Ready_ChipLoading.Location = new System.Drawing.Point(181, 100);
            this.btn_Ready_ChipLoading.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Ready_ChipLoading.Name = "btn_Ready_ChipLoading";
            this.btn_Ready_ChipLoading.Size = new System.Drawing.Size(169, 88);
            this.btn_Ready_ChipLoading.TabIndex = 23;
            this.btn_Ready_ChipLoading.TabStop = false;
            this.btn_Ready_ChipLoading.Text = "Ready";
            this.btn_Ready_ChipLoading.UseVisualStyleBackColor = false;
            // 
            // btn_Start_InputWafer
            // 
            this.btn_Start_InputWafer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_InputWafer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Start_InputWafer.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Start_InputWafer.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_InputWafer.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Start_InputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Start_InputWafer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Start_InputWafer.ForeColor = System.Drawing.Color.Black;
            this.btn_Start_InputWafer.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Start_InputWafer.Location = new System.Drawing.Point(4, 196);
            this.btn_Start_InputWafer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Start_InputWafer.Name = "btn_Start_InputWafer";
            this.btn_Start_InputWafer.Size = new System.Drawing.Size(169, 88);
            this.btn_Start_InputWafer.TabIndex = 17;
            this.btn_Start_InputWafer.TabStop = false;
            this.btn_Start_InputWafer.Text = "Start";
            this.btn_Start_InputWafer.UseVisualStyleBackColor = false;
            // 
            // btn_Ready_InputWafer
            // 
            this.btn_Ready_InputWafer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_InputWafer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Ready_InputWafer.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Ready_InputWafer.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_InputWafer.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Ready_InputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Ready_InputWafer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Ready_InputWafer.ForeColor = System.Drawing.Color.Black;
            this.btn_Ready_InputWafer.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Ready_InputWafer.Location = new System.Drawing.Point(4, 100);
            this.btn_Ready_InputWafer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_Ready_InputWafer.Name = "btn_Ready_InputWafer";
            this.btn_Ready_InputWafer.Size = new System.Drawing.Size(169, 88);
            this.btn_Ready_InputWafer.TabIndex = 12;
            this.btn_Ready_InputWafer.TabStop = false;
            this.btn_Ready_InputWafer.Text = "Ready";
            this.btn_Ready_InputWafer.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 88);
            this.label1.TabIndex = 18;
            this.label1.Text = "Wafer";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(181, 4);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(169, 88);
            this.label2.TabIndex = 19;
            this.label2.Text = "Load Arm";
            this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label3.Location = new System.Drawing.Point(358, 4);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(169, 88);
            this.label3.TabIndex = 20;
            this.label3.Text = "Index";
            this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label4.Location = new System.Drawing.Point(535, 4);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(169, 88);
            this.label4.TabIndex = 21;
            this.label4.Text = "Unload Arm";
            this.label4.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label5.Location = new System.Drawing.Point(712, 4);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(170, 88);
            this.label5.TabIndex = 22;
            this.label5.Text = "Bin";
            this.label5.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btn_Reset_InputWafer
            // 
            this.btn_Reset_InputWafer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_InputWafer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Reset_InputWafer.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_InputWafer.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_InputWafer.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Reset_InputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Reset_InputWafer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_InputWafer.ForeColor = System.Drawing.Color.Black;
            this.btn_Reset_InputWafer.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Reset_InputWafer.Location = new System.Drawing.Point(4, 292);
            this.btn_Reset_InputWafer.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Reset_InputWafer.Name = "btn_Reset_InputWafer";
            this.btn_Reset_InputWafer.Size = new System.Drawing.Size(169, 88);
            this.btn_Reset_InputWafer.TabIndex = 31;
            this.btn_Reset_InputWafer.TabStop = false;
            this.btn_Reset_InputWafer.Text = "Reset";
            this.btn_Reset_InputWafer.UseVisualStyleBackColor = false;
            // 
            // btn_Reset_ChipLoading
            // 
            this.btn_Reset_ChipLoading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_ChipLoading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Reset_ChipLoading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_ChipLoading.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_ChipLoading.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Reset_ChipLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Reset_ChipLoading.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_ChipLoading.ForeColor = System.Drawing.Color.Black;
            this.btn_Reset_ChipLoading.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Reset_ChipLoading.Location = new System.Drawing.Point(181, 292);
            this.btn_Reset_ChipLoading.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Reset_ChipLoading.Name = "btn_Reset_ChipLoading";
            this.btn_Reset_ChipLoading.Size = new System.Drawing.Size(169, 88);
            this.btn_Reset_ChipLoading.TabIndex = 32;
            this.btn_Reset_ChipLoading.TabStop = false;
            this.btn_Reset_ChipLoading.Text = "Reset";
            this.btn_Reset_ChipLoading.UseVisualStyleBackColor = false;
            // 
            // btn_Reset_Process
            // 
            this.btn_Reset_Process.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_Process.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Reset_Process.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_Process.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_Process.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Reset_Process.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Reset_Process.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_Process.ForeColor = System.Drawing.Color.Black;
            this.btn_Reset_Process.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Reset_Process.Location = new System.Drawing.Point(358, 292);
            this.btn_Reset_Process.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Reset_Process.Name = "btn_Reset_Process";
            this.btn_Reset_Process.Size = new System.Drawing.Size(169, 88);
            this.btn_Reset_Process.TabIndex = 33;
            this.btn_Reset_Process.TabStop = false;
            this.btn_Reset_Process.Text = "Reset";
            this.btn_Reset_Process.UseVisualStyleBackColor = false;
            // 
            // btn_Reset_ChipUnloading
            // 
            this.btn_Reset_ChipUnloading.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_ChipUnloading.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Reset_ChipUnloading.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_ChipUnloading.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_ChipUnloading.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Reset_ChipUnloading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Reset_ChipUnloading.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_ChipUnloading.ForeColor = System.Drawing.Color.Black;
            this.btn_Reset_ChipUnloading.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Reset_ChipUnloading.Location = new System.Drawing.Point(535, 292);
            this.btn_Reset_ChipUnloading.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Reset_ChipUnloading.Name = "btn_Reset_ChipUnloading";
            this.btn_Reset_ChipUnloading.Size = new System.Drawing.Size(169, 88);
            this.btn_Reset_ChipUnloading.TabIndex = 34;
            this.btn_Reset_ChipUnloading.TabStop = false;
            this.btn_Reset_ChipUnloading.Text = "Reset";
            this.btn_Reset_ChipUnloading.UseVisualStyleBackColor = false;
            // 
            // btn_Reset_OutputWafer
            // 
            this.btn_Reset_OutputWafer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_OutputWafer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Reset_OutputWafer.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Reset_OutputWafer.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_OutputWafer.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Reset_OutputWafer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Reset_OutputWafer.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Reset_OutputWafer.ForeColor = System.Drawing.Color.Black;
            this.btn_Reset_OutputWafer.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Reset_OutputWafer.Location = new System.Drawing.Point(712, 292);
            this.btn_Reset_OutputWafer.Margin = new System.Windows.Forms.Padding(4);
            this.btn_Reset_OutputWafer.Name = "btn_Reset_OutputWafer";
            this.btn_Reset_OutputWafer.Size = new System.Drawing.Size(170, 88);
            this.btn_Reset_OutputWafer.TabIndex = 35;
            this.btn_Reset_OutputWafer.TabStop = false;
            this.btn_Reset_OutputWafer.Text = "Reset";
            this.btn_Reset_OutputWafer.UseVisualStyleBackColor = false;
            // 
            // SequenceManualControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel8);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SequenceManualControl";
            this.Size = new System.Drawing.Size(886, 483);
            this.tableLayoutPanel8.ResumeLayout(false);
            this.tableLayoutPanel8.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel8;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private Common.IndividualMenuButton btn_Start_OutputWafer;
        private Common.IndividualMenuButton btn_Start_ChipUnloading;
        private Common.IndividualMenuButton btn_Start_Process;
        private Common.IndividualMenuButton btn_Start_ChipLoading;
        private Common.IndividualMenuButton btn_Ready_OutputWafer;
        private Common.IndividualMenuButton btn_Ready_ChipUnloading;
        private Common.IndividualMenuButton btn_Ready_Process;
        private Common.IndividualMenuButton btn_Ready_ChipLoading;
        private Common.IndividualMenuButton btn_Start_InputWafer;
        private Common.IndividualMenuButton btn_Ready_InputWafer;
        private System.Windows.Forms.Label label1;
        private Common.IndividualMenuButton btn_Reset_OutputWafer;
        private Common.IndividualMenuButton btn_Reset_ChipUnloading;
        private Common.IndividualMenuButton btn_Reset_Process;
        private Common.IndividualMenuButton btn_Reset_ChipLoading;
        private Common.IndividualMenuButton btn_Reset_InputWafer;
    }
}
