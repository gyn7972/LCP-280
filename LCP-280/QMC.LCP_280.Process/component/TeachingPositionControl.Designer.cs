namespace QMC.LCP_280.Process.Component
{
    partial class TeachingPositionControl
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

        #region Designer generated code
        private void InitializeComponent()
        {
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.mainTable = new System.Windows.Forms.TableLayoutPanel();
            this.positionItemView = new QMC.Common.ListBoxItemsView();
            this.rightPanel = new System.Windows.Forms.TableLayoutPanel();
            this.editorGroupPanel = new System.Windows.Forms.Panel();
            this.positionEditorView = new QMC.Common.PropertyCollectionView();
            this.flowButtonsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbTeachingMove = new System.Windows.Forms.GroupBox();
            this.moveTable = new System.Windows.Forms.TableLayoutPanel();
            this.gbMoveMode = new System.Windows.Forms.GroupBox();
            this.rdoCoarse = new System.Windows.Forms.RadioButton();
            this.rdoFine = new System.Windows.Forms.RadioButton();
            this.btnMovePosition = new QMC.Common.IndividualMenuButton();
            this.gbPositionTeaching.SuspendLayout();
            this.mainTable.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.editorGroupPanel.SuspendLayout();
            this.flowButtonsPanel.SuspendLayout();
            this.gbTeachingMove.SuspendLayout();
            this.moveTable.SuspendLayout();
            this.gbMoveMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.BackColor = System.Drawing.Color.White;
            this.gbPositionTeaching.Controls.Add(this.mainTable);
            this.gbPositionTeaching.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbPositionTeaching.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbPositionTeaching.Location = new System.Drawing.Point(0, 0);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Size = new System.Drawing.Size(552, 353);
            this.gbPositionTeaching.TabIndex = 0;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Position Teaching";
            // 
            // mainTable
            // 
            this.mainTable.ColumnCount = 2;
            this.mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 250F));
            this.mainTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTable.Controls.Add(this.positionItemView, 0, 0);
            this.mainTable.Controls.Add(this.rightPanel, 1, 0);
            this.mainTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTable.Location = new System.Drawing.Point(3, 21);
            this.mainTable.Name = "mainTable";
            this.mainTable.RowCount = 1;
            this.mainTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTable.Size = new System.Drawing.Size(546, 329);
            this.mainTable.TabIndex = 0;
            // 
            // positionItemView
            // 
            this.positionItemView.BorderColor = System.Drawing.Color.Black;
            this.positionItemView.BorderWidth = 1;
            this.positionItemView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionItemView.GroupBackColor = System.Drawing.Color.White;
            this.positionItemView.GroupForeColor = System.Drawing.Color.Black;
            this.positionItemView.GroupName = "Position Item";
            this.positionItemView.ItemBackColor = System.Drawing.Color.Black;
            this.positionItemView.ItemForeColor = System.Drawing.Color.Lime;
            this.positionItemView.ListBackColor = System.Drawing.Color.Black;
            this.positionItemView.ListForeColor = System.Drawing.Color.Lime;
            this.positionItemView.Location = new System.Drawing.Point(3, 3);
            this.positionItemView.Name = "positionItemView";
            this.positionItemView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.positionItemView.SelectedForeColor = System.Drawing.Color.Black;
            this.positionItemView.SelectedIndex = -1;
            this.positionItemView.Size = new System.Drawing.Size(244, 323);
            this.positionItemView.TabIndex = 0;
            // 
            // rightPanel
            // 
            this.rightPanel.ColumnCount = 1;
            this.rightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rightPanel.Controls.Add(this.editorGroupPanel, 0, 0);
            this.rightPanel.Controls.Add(this.flowButtonsPanel, 0, 1);
            this.rightPanel.Controls.Add(this.gbTeachingMove, 0, 2);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(253, 3);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.RowCount = 3;
            this.rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.rightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.rightPanel.Size = new System.Drawing.Size(290, 323);
            this.rightPanel.TabIndex = 1;
            // 
            // editorGroupPanel
            // 
            this.editorGroupPanel.Controls.Add(this.positionEditorView);
            this.editorGroupPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorGroupPanel.Location = new System.Drawing.Point(3, 3);
            this.editorGroupPanel.Name = "editorGroupPanel";
            this.editorGroupPanel.Size = new System.Drawing.Size(284, 144);
            this.editorGroupPanel.TabIndex = 0;
            // 
            // positionEditorView
            // 
            this.positionEditorView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionEditorView.GroupName = "Editor";
            this.positionEditorView.Location = new System.Drawing.Point(0, 0);
            this.positionEditorView.Name = "positionEditorView";
            this.positionEditorView.Size = new System.Drawing.Size(284, 144);
            this.positionEditorView.TabIndex = 0;
            // 
            // flowButtonsPanel
            // 
            this.flowButtonsPanel.Controls.Add(this.btnSave);
            this.flowButtonsPanel.Controls.Add(this.btnCancel);
            this.flowButtonsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowButtonsPanel.Location = new System.Drawing.Point(3, 153);
            this.flowButtonsPanel.Name = "flowButtonsPanel";
            this.flowButtonsPanel.Padding = new System.Windows.Forms.Padding(2, 8, 2, 2);
            this.flowButtonsPanel.Size = new System.Drawing.Size(284, 44);
            this.flowButtonsPanel.TabIndex = 1;
            // 
            // btnSave
            // 
            this.btnSave.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnSave.Location = new System.Drawing.Point(5, 11);
            this.btnSave.Margin = new System.Windows.Forms.Padding(3, 3, 30, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.btnCancel.Location = new System.Drawing.Point(138, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // gbTeachingMove
            // 
            this.gbTeachingMove.Controls.Add(this.moveTable);
            this.gbTeachingMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTeachingMove.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbTeachingMove.Location = new System.Drawing.Point(3, 203);
            this.gbTeachingMove.Name = "gbTeachingMove";
            this.gbTeachingMove.Size = new System.Drawing.Size(284, 117);
            this.gbTeachingMove.TabIndex = 2;
            this.gbTeachingMove.TabStop = false;
            this.gbTeachingMove.Text = "Teaching Move";
            // 
            // moveTable
            // 
            this.moveTable.ColumnCount = 2;
            this.moveTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.moveTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.moveTable.Controls.Add(this.gbMoveMode, 0, 0);
            this.moveTable.Controls.Add(this.btnMovePosition, 1, 0);
            this.moveTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.moveTable.Location = new System.Drawing.Point(3, 21);
            this.moveTable.Name = "moveTable";
            this.moveTable.RowCount = 1;
            this.moveTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.moveTable.Size = new System.Drawing.Size(278, 93);
            this.moveTable.TabIndex = 0;
            // 
            // gbMoveMode
            // 
            this.gbMoveMode.Controls.Add(this.rdoCoarse);
            this.gbMoveMode.Controls.Add(this.rdoFine);
            this.gbMoveMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMoveMode.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gbMoveMode.Location = new System.Drawing.Point(3, 3);
            this.gbMoveMode.Name = "gbMoveMode";
            this.gbMoveMode.Size = new System.Drawing.Size(160, 87);
            this.gbMoveMode.TabIndex = 0;
            this.gbMoveMode.TabStop = false;
            this.gbMoveMode.Text = "Move Mode";
            // 
            // rdoCoarse
            // 
            this.rdoCoarse.AutoSize = true;
            this.rdoCoarse.Location = new System.Drawing.Point(59, 22);
            this.rdoCoarse.Name = "rdoCoarse";
            this.rdoCoarse.Size = new System.Drawing.Size(61, 19);
            this.rdoCoarse.TabIndex = 1;
            this.rdoCoarse.Text = "Coarse";
            this.rdoCoarse.UseVisualStyleBackColor = true;
            // 
            // rdoFine
            // 
            this.rdoFine.AutoSize = true;
            this.rdoFine.Checked = true;
            this.rdoFine.Location = new System.Drawing.Point(6, 22);
            this.rdoFine.Name = "rdoFine";
            this.rdoFine.Size = new System.Drawing.Size(47, 19);
            this.rdoFine.TabIndex = 0;
            this.rdoFine.TabStop = true;
            this.rdoFine.Text = "Fine";
            this.rdoFine.UseVisualStyleBackColor = true;
            // 
            // btnMovePosition
            // 
            this.btnMovePosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMovePosition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMovePosition.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMovePosition.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMovePosition.CustomForeColor = System.Drawing.Color.Black;
            this.btnMovePosition.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnMovePosition.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMovePosition.ForeColor = System.Drawing.Color.Black;
            this.btnMovePosition.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMovePosition.Location = new System.Drawing.Point(169, 3);
            this.btnMovePosition.Name = "btnMovePosition";
            this.btnMovePosition.Size = new System.Drawing.Size(106, 87);
            this.btnMovePosition.TabIndex = 1;
            this.btnMovePosition.TabStop = false;
            this.btnMovePosition.Text = "Move\r\nPosition";
            this.btnMovePosition.UseVisualStyleBackColor = false;
            // 
            // TeachingPositionControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.gbPositionTeaching);
            this.Name = "TeachingPositionControl";
            this.Size = new System.Drawing.Size(552, 353);
            this.gbPositionTeaching.ResumeLayout(false);
            this.mainTable.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.editorGroupPanel.ResumeLayout(false);
            this.flowButtonsPanel.ResumeLayout(false);
            this.gbTeachingMove.ResumeLayout(false);
            this.moveTable.ResumeLayout(false);
            this.gbMoveMode.ResumeLayout(false);
            this.gbMoveMode.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.GroupBox gbPositionTeaching;
        private System.Windows.Forms.TableLayoutPanel mainTable;
        private QMC.Common.ListBoxItemsView positionItemView;
        private System.Windows.Forms.TableLayoutPanel rightPanel;
        private System.Windows.Forms.Panel editorGroupPanel;
        private QMC.Common.PropertyCollectionView positionEditorView;
        private System.Windows.Forms.FlowLayoutPanel flowButtonsPanel;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox gbTeachingMove;
        private System.Windows.Forms.TableLayoutPanel moveTable;
        private System.Windows.Forms.GroupBox gbMoveMode;
        private System.Windows.Forms.RadioButton rdoCoarse;
        private System.Windows.Forms.RadioButton rdoFine;
        private QMC.Common.IndividualMenuButton btnMovePosition;
    }
}