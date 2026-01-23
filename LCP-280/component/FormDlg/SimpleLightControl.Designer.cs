namespace QMC.LCP_280.Process.Unit.FormSetup
{
    partial class SimpleLightControl
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TableLayoutPanel mainPanel;
        private System.Windows.Forms.GroupBox groupBox_Controller;
        private System.Windows.Forms.GroupBox groupBox_Channel;
        private System.Windows.Forms.GroupBox groupBox_Control;
        private System.Windows.Forms.ComboBox comboBox_Illuminator;
        private System.Windows.Forms.ComboBox comboBox_Channel;
        private System.Windows.Forms.Button btn_Connect;
        private System.Windows.Forms.Button btn_Disconnect;
        private System.Windows.Forms.Button btn_On;
        private System.Windows.Forms.Button btn_Off;
        private System.Windows.Forms.TrackBar trackBar_LightIntensity;
        private System.Windows.Forms.Label label_Intensity;
        private System.Windows.Forms.Label label_Status;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.mainPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox_Controller = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBox_Illuminator = new System.Windows.Forms.ComboBox();
            this.label_Status = new System.Windows.Forms.Label();
            this.btn_Disconnect = new System.Windows.Forms.Button();
            this.btn_Connect = new System.Windows.Forms.Button();
            this.groupBox_Channel = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.comboBox_Channel = new System.Windows.Forms.ComboBox();
            this.label_Intensity = new System.Windows.Forms.Label();
            this.trackBar_LightIntensity = new System.Windows.Forms.TrackBar();
            this.groupBox_Control = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btn_Off = new System.Windows.Forms.Button();
            this.btn_On = new System.Windows.Forms.Button();
            this.label_Message = new System.Windows.Forms.Label();
            this.mainPanel.SuspendLayout();
            this.groupBox_Controller.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox_Channel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_LightIntensity)).BeginInit();
            this.groupBox_Control.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.ColumnCount = 1;
            this.mainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainPanel.Controls.Add(this.groupBox_Controller, 0, 0);
            this.mainPanel.Controls.Add(this.groupBox_Channel, 0, 1);
            this.mainPanel.Controls.Add(this.groupBox_Control, 0, 2);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.RowCount = 3;
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.mainPanel.Size = new System.Drawing.Size(668, 430);
            this.mainPanel.TabIndex = 0;
            // 
            // groupBox_Controller
            // 
            this.groupBox_Controller.Controls.Add(this.tableLayoutPanel1);
            this.groupBox_Controller.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_Controller.Location = new System.Drawing.Point(6, 5);
            this.groupBox_Controller.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox_Controller.Name = "groupBox_Controller";
            this.groupBox_Controller.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox_Controller.Size = new System.Drawing.Size(656, 119);
            this.groupBox_Controller.TabIndex = 0;
            this.groupBox_Controller.TabStop = false;
            this.groupBox_Controller.Text = "Controller Selection";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.comboBox_Illuminator, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_Status, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btn_Disconnect, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn_Connect, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 26);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(644, 88);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // comboBox_Illuminator
            // 
            this.comboBox_Illuminator.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_Illuminator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Illuminator.FormattingEnabled = true;
            this.comboBox_Illuminator.Location = new System.Drawing.Point(6, 5);
            this.comboBox_Illuminator.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_Illuminator.Name = "comboBox_Illuminator";
            this.comboBox_Illuminator.Size = new System.Drawing.Size(374, 26);
            this.comboBox_Illuminator.TabIndex = 0;
            // 
            // label_Status
            // 
            this.label_Status.AutoSize = true;
            this.label_Status.ForeColor = System.Drawing.Color.Red;
            this.label_Status.Location = new System.Drawing.Point(4, 49);
            this.label_Status.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.label_Status.Name = "label_Status";
            this.label_Status.Size = new System.Drawing.Size(130, 18);
            this.label_Status.TabIndex = 3;
            this.label_Status.Text = "Not Connected";
            // 
            // btn_Disconnect
            // 
            this.btn_Disconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Disconnect.Location = new System.Drawing.Point(520, 5);
            this.btn_Disconnect.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btn_Disconnect.Name = "btn_Disconnect";
            this.btn_Disconnect.Size = new System.Drawing.Size(118, 34);
            this.btn_Disconnect.TabIndex = 2;
            this.btn_Disconnect.Text = "Disconnect";
            this.btn_Disconnect.UseVisualStyleBackColor = true;
            // 
            // btn_Connect
            // 
            this.btn_Connect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Connect.Location = new System.Drawing.Point(392, 5);
            this.btn_Connect.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btn_Connect.Name = "btn_Connect";
            this.btn_Connect.Size = new System.Drawing.Size(116, 34);
            this.btn_Connect.TabIndex = 1;
            this.btn_Connect.Text = "Connect";
            this.btn_Connect.UseVisualStyleBackColor = true;
            // 
            // groupBox_Channel
            // 
            this.groupBox_Channel.Controls.Add(this.tableLayoutPanel2);
            this.groupBox_Channel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_Channel.Location = new System.Drawing.Point(6, 134);
            this.groupBox_Channel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox_Channel.Name = "groupBox_Channel";
            this.groupBox_Channel.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox_Channel.Size = new System.Drawing.Size(656, 183);
            this.groupBox_Channel.TabIndex = 1;
            this.groupBox_Channel.TabStop = false;
            this.groupBox_Channel.Text = "Channel & Intensity";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.trackBar_LightIntensity, 0, 1);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 26);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(644, 152);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.Controls.Add(this.comboBox_Channel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.label_Intensity, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(4, 5);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(636, 50);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // comboBox_Channel
            // 
            this.comboBox_Channel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_Channel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Channel.FormattingEnabled = true;
            this.comboBox_Channel.Location = new System.Drawing.Point(6, 5);
            this.comboBox_Channel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.comboBox_Channel.Name = "comboBox_Channel";
            this.comboBox_Channel.Size = new System.Drawing.Size(369, 26);
            this.comboBox_Channel.TabIndex = 0;
            // 
            // label_Intensity
            // 
            this.label_Intensity.AutoSize = true;
            this.label_Intensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Intensity.Location = new System.Drawing.Point(385, 5);
            this.label_Intensity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.label_Intensity.Name = "label_Intensity";
            this.label_Intensity.Size = new System.Drawing.Size(247, 40);
            this.label_Intensity.TabIndex = 2;
            this.label_Intensity.Text = "Intensity: 0";
            this.label_Intensity.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // trackBar_LightIntensity
            // 
            this.trackBar_LightIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackBar_LightIntensity.Enabled = false;
            this.trackBar_LightIntensity.Location = new System.Drawing.Point(6, 65);
            this.trackBar_LightIntensity.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.trackBar_LightIntensity.Maximum = 4095;
            this.trackBar_LightIntensity.Name = "trackBar_LightIntensity";
            this.trackBar_LightIntensity.Size = new System.Drawing.Size(632, 82);
            this.trackBar_LightIntensity.TabIndex = 1;
            this.trackBar_LightIntensity.TickFrequency = 256;
            // 
            // groupBox_Control
            // 
            this.groupBox_Control.Controls.Add(this.tableLayoutPanel4);
            this.groupBox_Control.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_Control.Location = new System.Drawing.Point(6, 327);
            this.groupBox_Control.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox_Control.Name = "groupBox_Control";
            this.groupBox_Control.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.groupBox_Control.Size = new System.Drawing.Size(656, 98);
            this.groupBox_Control.TabIndex = 2;
            this.groupBox_Control.TabStop = false;
            this.groupBox_Control.Text = "Light Control";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel4.Controls.Add(this.btn_Off, 2, 0);
            this.tableLayoutPanel4.Controls.Add(this.btn_On, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.label_Message, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(6, 26);
            this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(644, 67);
            this.tableLayoutPanel4.TabIndex = 2;
            // 
            // btn_Off
            // 
            this.btn_Off.BackColor = System.Drawing.Color.LightCoral;
            this.btn_Off.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Off.Enabled = false;
            this.btn_Off.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btn_Off.Location = new System.Drawing.Point(520, 5);
            this.btn_Off.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btn_Off.Name = "btn_Off";
            this.btn_Off.Size = new System.Drawing.Size(118, 57);
            this.btn_Off.TabIndex = 1;
            this.btn_Off.Text = "OFF";
            this.btn_Off.UseVisualStyleBackColor = false;
            // 
            // btn_On
            // 
            this.btn_On.BackColor = System.Drawing.Color.LightGreen;
            this.btn_On.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_On.Enabled = false;
            this.btn_On.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold);
            this.btn_On.Location = new System.Drawing.Point(392, 5);
            this.btn_On.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.btn_On.Name = "btn_On";
            this.btn_On.Size = new System.Drawing.Size(116, 57);
            this.btn_On.TabIndex = 0;
            this.btn_On.Text = "ON";
            this.btn_On.UseVisualStyleBackColor = false;
            // 
            // label_Message
            // 
            this.label_Message.AutoSize = true;
            this.label_Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_Message.Location = new System.Drawing.Point(4, 5);
            this.label_Message.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.label_Message.Name = "label_Message";
            this.label_Message.Size = new System.Drawing.Size(378, 57);
            this.label_Message.TabIndex = 2;
            this.label_Message.Text = "label1";
            this.label_Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SimpleLightControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainPanel);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "SimpleLightControl";
            this.Size = new System.Drawing.Size(668, 430);
            this.mainPanel.ResumeLayout(false);
            this.groupBox_Controller.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBox_Channel.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar_LightIntensity)).EndInit();
            this.groupBox_Control.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Label label_Message;
    }
}