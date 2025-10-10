using System.Windows.Forms;

namespace QMC.LCP_280.Process
{
    partial class Monitoring_Main
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private GroupBox groupBoxdieInputControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieInputControl dieInputControl1;

        private GroupBox groupBoxdieIndexSelectControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieIndexSelectControl dieIndexSelectControl1;

        private GroupBox groupBoxdieOutputControl;
        private QMC.LCP_280.Process.Unit.FormMain.DieOutputControl dieOutputControl1;

        private GroupBox groupBoxMeasurementControl;
        private QMC.LCP_280.Process.Unit.FormMain.MeasurementControl measurementControl1;

        private GroupBox groupBoxInputWaferCarrierControl;
        private QMC.LCP_280.Process.Unit.FormMain.InputWaferCarrierControl inputWaferCarrierControl1;

        private GroupBox groupBoxOutputWaferCarrierControl;
        private QMC.LCP_280.Process.Unit.FormMain.OutputWaferCarrierControl outputWaferCarrierControl1;

        private TableLayoutPanel tableLayoutPanel1;

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
            this.groupBoxdieInputControl = new System.Windows.Forms.GroupBox();
            this.dieInputControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieInputControl();
            this.groupBoxdieIndexSelectControl = new System.Windows.Forms.GroupBox();
            this.dieIndexSelectControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieIndexSelectControl();
            this.groupBoxdieOutputControl = new System.Windows.Forms.GroupBox();
            this.dieOutputControl1 = new QMC.LCP_280.Process.Unit.FormMain.DieOutputControl();
            this.groupBoxInputWaferCarrierControl = new System.Windows.Forms.GroupBox();
            this.inputWaferCarrierControl1 = new QMC.LCP_280.Process.Unit.FormMain.InputWaferCarrierControl();
            this.groupBoxMeasurementControl = new System.Windows.Forms.GroupBox();
            this.measurementControl1 = new QMC.LCP_280.Process.Unit.FormMain.MeasurementControl();
            this.groupBoxOutputWaferCarrierControl = new System.Windows.Forms.GroupBox();
            this.outputWaferCarrierControl1 = new QMC.LCP_280.Process.Unit.FormMain.OutputWaferCarrierControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox_SequenceAuto = new System.Windows.Forms.GroupBox();
            this.sequenceAutoControl = new QMC.LCP_280.Process.Unit.FormMain.SequenceAutoControl();
            this.groupBoxdieInputControl.SuspendLayout();
            this.groupBoxdieIndexSelectControl.SuspendLayout();
            this.groupBoxdieOutputControl.SuspendLayout();
            this.groupBoxInputWaferCarrierControl.SuspendLayout();
            this.groupBoxMeasurementControl.SuspendLayout();
            this.groupBoxOutputWaferCarrierControl.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBox_SequenceAuto.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxdieInputControl
            // 
            this.groupBoxdieInputControl.Controls.Add(this.dieInputControl1);
            this.groupBoxdieInputControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieInputControl.Location = new System.Drawing.Point(2, 2);
            this.groupBoxdieInputControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxdieInputControl.Name = "groupBoxdieInputControl";
            this.groupBoxdieInputControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxdieInputControl.Size = new System.Drawing.Size(470, 465);
            this.groupBoxdieInputControl.TabIndex = 0;
            this.groupBoxdieInputControl.TabStop = false;
            this.groupBoxdieInputControl.Text = "Die Input Tr";
            // 
            // dieInputControl1
            // 
            this.dieInputControl1.BackColor = System.Drawing.Color.White;
            this.dieInputControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieInputControl1.Location = new System.Drawing.Point(2, 20);
            this.dieInputControl1.Margin = new System.Windows.Forms.Padding(2);
            this.dieInputControl1.Name = "dieInputControl1";
            this.dieInputControl1.Size = new System.Drawing.Size(466, 443);
            this.dieInputControl1.TabIndex = 0;
            // 
            // groupBoxdieIndexSelectControl
            // 
            this.groupBoxdieIndexSelectControl.Controls.Add(this.dieIndexSelectControl1);
            this.groupBoxdieIndexSelectControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieIndexSelectControl.Location = new System.Drawing.Point(476, 2);
            this.groupBoxdieIndexSelectControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxdieIndexSelectControl.Name = "groupBoxdieIndexSelectControl";
            this.groupBoxdieIndexSelectControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxdieIndexSelectControl.Size = new System.Drawing.Size(470, 465);
            this.groupBoxdieIndexSelectControl.TabIndex = 1;
            this.groupBoxdieIndexSelectControl.TabStop = false;
            this.groupBoxdieIndexSelectControl.Text = "Index";
            // 
            // dieIndexSelectControl1
            // 
            this.dieIndexSelectControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.dieIndexSelectControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieIndexSelectControl1.Location = new System.Drawing.Point(2, 20);
            this.dieIndexSelectControl1.Margin = new System.Windows.Forms.Padding(2);
            this.dieIndexSelectControl1.Name = "dieIndexSelectControl1";
            this.dieIndexSelectControl1.Size = new System.Drawing.Size(466, 443);
            this.dieIndexSelectControl1.TabIndex = 1;
            // 
            // groupBoxdieOutputControl
            // 
            this.groupBoxdieOutputControl.Controls.Add(this.dieOutputControl1);
            this.groupBoxdieOutputControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxdieOutputControl.Location = new System.Drawing.Point(950, 2);
            this.groupBoxdieOutputControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxdieOutputControl.Name = "groupBoxdieOutputControl";
            this.groupBoxdieOutputControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxdieOutputControl.Size = new System.Drawing.Size(470, 465);
            this.groupBoxdieOutputControl.TabIndex = 2;
            this.groupBoxdieOutputControl.TabStop = false;
            this.groupBoxdieOutputControl.Text = "Die Output Tr";
            // 
            // dieOutputControl1
            // 
            this.dieOutputControl1.BackColor = System.Drawing.Color.White;
            this.dieOutputControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dieOutputControl1.Location = new System.Drawing.Point(2, 20);
            this.dieOutputControl1.Margin = new System.Windows.Forms.Padding(2);
            this.dieOutputControl1.Name = "dieOutputControl1";
            this.dieOutputControl1.Size = new System.Drawing.Size(466, 443);
            this.dieOutputControl1.TabIndex = 2;
            // 
            // groupBoxInputWaferCarrierControl
            // 
            this.groupBoxInputWaferCarrierControl.Controls.Add(this.inputWaferCarrierControl1);
            this.groupBoxInputWaferCarrierControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxInputWaferCarrierControl.Location = new System.Drawing.Point(2, 471);
            this.groupBoxInputWaferCarrierControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxInputWaferCarrierControl.Name = "groupBoxInputWaferCarrierControl";
            this.groupBoxInputWaferCarrierControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxInputWaferCarrierControl.Size = new System.Drawing.Size(470, 466);
            this.groupBoxInputWaferCarrierControl.TabIndex = 3;
            this.groupBoxInputWaferCarrierControl.TabStop = false;
            this.groupBoxInputWaferCarrierControl.Text = "Input Wafer Carrier";
            // 
            // inputWaferCarrierControl1
            // 
            this.inputWaferCarrierControl1.BackColor = System.Drawing.Color.White;
            this.inputWaferCarrierControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputWaferCarrierControl1.Location = new System.Drawing.Point(2, 20);
            this.inputWaferCarrierControl1.Margin = new System.Windows.Forms.Padding(2);
            this.inputWaferCarrierControl1.Name = "inputWaferCarrierControl1";
            this.inputWaferCarrierControl1.Size = new System.Drawing.Size(466, 444);
            this.inputWaferCarrierControl1.TabIndex = 4;
            // 
            // groupBoxMeasurementControl
            // 
            this.groupBoxMeasurementControl.Controls.Add(this.measurementControl1);
            this.groupBoxMeasurementControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxMeasurementControl.Location = new System.Drawing.Point(476, 471);
            this.groupBoxMeasurementControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxMeasurementControl.Name = "groupBoxMeasurementControl";
            this.groupBoxMeasurementControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxMeasurementControl.Size = new System.Drawing.Size(470, 466);
            this.groupBoxMeasurementControl.TabIndex = 4;
            this.groupBoxMeasurementControl.TabStop = false;
            this.groupBoxMeasurementControl.Text = "Measurement";
            // 
            // measurementControl1
            // 
            this.measurementControl1.BackColor = System.Drawing.Color.White;
            this.measurementControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.measurementControl1.Location = new System.Drawing.Point(2, 20);
            this.measurementControl1.Margin = new System.Windows.Forms.Padding(2);
            this.measurementControl1.Name = "measurementControl1";
            this.measurementControl1.Size = new System.Drawing.Size(466, 444);
            this.measurementControl1.TabIndex = 4;
            // 
            // groupBoxOutputWaferCarrierControl
            // 
            this.groupBoxOutputWaferCarrierControl.Controls.Add(this.outputWaferCarrierControl1);
            this.groupBoxOutputWaferCarrierControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxOutputWaferCarrierControl.Location = new System.Drawing.Point(950, 471);
            this.groupBoxOutputWaferCarrierControl.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxOutputWaferCarrierControl.Name = "groupBoxOutputWaferCarrierControl";
            this.groupBoxOutputWaferCarrierControl.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxOutputWaferCarrierControl.Size = new System.Drawing.Size(470, 466);
            this.groupBoxOutputWaferCarrierControl.TabIndex = 3;
            this.groupBoxOutputWaferCarrierControl.TabStop = false;
            this.groupBoxOutputWaferCarrierControl.Text = "Output Wafer Carrier";
            // 
            // outputWaferCarrierControl1
            // 
            this.outputWaferCarrierControl1.BackColor = System.Drawing.Color.White;
            this.outputWaferCarrierControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputWaferCarrierControl1.Location = new System.Drawing.Point(2, 20);
            this.outputWaferCarrierControl1.Margin = new System.Windows.Forms.Padding(2);
            this.outputWaferCarrierControl1.Name = "outputWaferCarrierControl1";
            this.outputWaferCarrierControl1.Size = new System.Drawing.Size(466, 444);
            this.outputWaferCarrierControl1.TabIndex = 4;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 157F));
            this.tableLayoutPanel1.Controls.Add(this.groupBoxOutputWaferCarrierControl, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxMeasurementControl, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxInputWaferCarrierControl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxdieIndexSelectControl, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxdieOutputControl, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBoxdieInputControl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.groupBox_SequenceAuto, 3, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1580, 939);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // groupBox_SequenceAuto
            // 
            this.groupBox_SequenceAuto.Controls.Add(this.sequenceAutoControl);
            this.groupBox_SequenceAuto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox_SequenceAuto.Font = new System.Drawing.Font("맑은 고딕", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.groupBox_SequenceAuto.Location = new System.Drawing.Point(1424, 471);
            this.groupBox_SequenceAuto.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceAuto.Name = "groupBox_SequenceAuto";
            this.groupBox_SequenceAuto.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_SequenceAuto.Size = new System.Drawing.Size(154, 466);
            this.groupBox_SequenceAuto.TabIndex = 20;
            this.groupBox_SequenceAuto.TabStop = false;
            this.groupBox_SequenceAuto.Text = "Sequence Auto";
            // 
            // sequenceAutoControl
            // 
            this.sequenceAutoControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sequenceAutoControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sequenceAutoControl.Location = new System.Drawing.Point(2, 25);
            this.sequenceAutoControl.Margin = new System.Windows.Forms.Padding(2);
            this.sequenceAutoControl.Name = "sequenceAutoControl";
            this.sequenceAutoControl.Size = new System.Drawing.Size(150, 439);
            this.sequenceAutoControl.TabIndex = 1;
            // 
            // Monitoring_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1580, 939);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Monitoring_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Monitoring";
            this.Load += new System.EventHandler(this.Monitoring_Main_Load);
            this.groupBoxdieInputControl.ResumeLayout(false);
            this.groupBoxdieIndexSelectControl.ResumeLayout(false);
            this.groupBoxdieOutputControl.ResumeLayout(false);
            this.groupBoxInputWaferCarrierControl.ResumeLayout(false);
            this.groupBoxMeasurementControl.ResumeLayout(false);
            this.groupBoxOutputWaferCarrierControl.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.groupBox_SequenceAuto.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox groupBox_SequenceAuto;
        private Unit.FormMain.SequenceAutoControl sequenceAutoControl;
    }
}