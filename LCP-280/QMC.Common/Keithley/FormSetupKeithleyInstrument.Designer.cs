namespace QMC.Common.Keithley
{
    partial class FormSetupKeithleyInstrument
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
            this.comboBoxInterface = new System.Windows.Forms.ComboBox();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.listBoxResource = new System.Windows.Forms.ListBox();
            this.buttonApply = new System.Windows.Forms.Button();
            this.labelResult = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxInterface
            // 
            this.comboBoxInterface.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.comboBoxInterface.FormattingEnabled = true;
            this.comboBoxInterface.Location = new System.Drawing.Point(12, 12);
            this.comboBoxInterface.Name = "comboBoxInterface";
            this.comboBoxInterface.Size = new System.Drawing.Size(294, 25);
            this.comboBoxInterface.TabIndex = 0;
            // 
            // buttonSearch
            // 
            this.buttonSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonSearch.Location = new System.Drawing.Point(312, 12);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(113, 25);
            this.buttonSearch.TabIndex = 1;
            this.buttonSearch.Text = "Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // listBoxResource
            // 
            this.listBoxResource.FormattingEnabled = true;
            this.listBoxResource.ItemHeight = 12;
            this.listBoxResource.Location = new System.Drawing.Point(12, 66);
            this.listBoxResource.Name = "listBoxResource";
            this.listBoxResource.Size = new System.Drawing.Size(413, 280);
            this.listBoxResource.TabIndex = 2;
            this.listBoxResource.SelectedIndexChanged += new System.EventHandler(this.listBoxResource_SelectedIndexChanged);
            // 
            // buttonApply
            // 
            this.buttonApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApply.Location = new System.Drawing.Point(276, 362);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new System.Drawing.Size(149, 39);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "Apply";
            this.buttonApply.UseVisualStyleBackColor = true;
            this.buttonApply.Click += new System.EventHandler(this.buttonApply_Click);
            // 
            // labelResult
            // 
            this.labelResult.Location = new System.Drawing.Point(12, 45);
            this.labelResult.Name = "labelResult";
            this.labelResult.Size = new System.Drawing.Size(413, 17);
            this.labelResult.TabIndex = 6;
            this.labelResult.Text = " - ";
            // 
            // FormSetupKeithleyInstrument
            // 
            this.ClientSize = new System.Drawing.Size(438, 413);
            this.Controls.Add(this.labelResult);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.listBoxResource);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.comboBoxInterface);
            this.Name = "FormSetupKeithleyInstrument";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxInterface;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.ListBox listBoxResource;
        private System.Windows.Forms.Button buttonApply;
        private System.Windows.Forms.Label labelResult;
    }
}