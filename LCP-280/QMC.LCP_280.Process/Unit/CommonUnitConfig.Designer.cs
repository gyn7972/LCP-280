namespace QMC.LCP_280.Process.Unit
{
    partial class CommonUnitConfig
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
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.axisPositionsView = new QMC.Common.ListBoxItemsView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inputView = new QMC.Common.IOPropertyCollectionView();
            this.outputView = new QMC.Common.IOPropertyCollectionView();
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.gbTeachingMove = new System.Windows.Forms.GroupBox();
            this.btnMovePosition = new QMC.Common.IndividualMenuButton();
            this.rbTeachingMoveMode = new QMC.Common.RadioButtonView();
            this.editorPanel = new System.Windows.Forms.Panel();
            this.button_Test = new System.Windows.Forms.Button();
            this.positionEditorView = new QMC.Common.PropertyCollectionView();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.positionItemView = new QMC.Common.ListBoxItemsView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.mainTableLayoutPanel.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
            this.gbPositionTeaching.SuspendLayout();
            this.positionTableLayoutPanel.SuspendLayout();
            this.gbTeachingMove.SuspendLayout();
            this.editorPanel.SuspendLayout();
            this.gbMoveAxis.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 4;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.mainTableLayoutPanel.Controls.Add(this.axisPositionsView, 3, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbDigitalIO, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.gbPositionTeaching, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.gbMoveAxis, 2, 0);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 2;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(1264, 780);
            this.mainTableLayoutPanel.TabIndex = 13;
            // 
            // axisPositionsView
            // 
            this.axisPositionsView.BorderWidth = 2;
            this.axisPositionsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axisPositionsView.GroupName = "Axis Positions";
            this.axisPositionsView.Location = new System.Drawing.Point(951, 3);
            this.axisPositionsView.Name = "axisPositionsView";
            this.mainTableLayoutPanel.SetRowSpan(this.axisPositionsView, 2);
            this.axisPositionsView.SelectedIndex = -1;
            this.axisPositionsView.Size = new System.Drawing.Size(310, 774);
            this.axisPositionsView.TabIndex = 11;
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbDigitalIO, 2);
            this.gbDigitalIO.Controls.Add(this.ioTableLayoutPanel);
            this.gbDigitalIO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDigitalIO.Location = new System.Drawing.Point(3, 393);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(626, 384);
            this.gbDigitalIO.TabIndex = 9;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // ioTableLayoutPanel
            // 
            this.ioTableLayoutPanel.ColumnCount = 2;
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ioTableLayoutPanel.Controls.Add(this.inputView, 0, 0);
            this.ioTableLayoutPanel.Controls.Add(this.outputView, 1, 0);
            this.ioTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ioTableLayoutPanel.Location = new System.Drawing.Point(3, 21);
            this.ioTableLayoutPanel.Name = "ioTableLayoutPanel";
            this.ioTableLayoutPanel.RowCount = 1;
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ioTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 360F));
            this.ioTableLayoutPanel.Size = new System.Drawing.Size(620, 360);
            this.ioTableLayoutPanel.TabIndex = 2;
            // 
            // inputView
            // 
            this.inputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inputView.GroupName = "Input";
            this.inputView.Location = new System.Drawing.Point(4, 6);
            this.inputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.inputView.Name = "inputView";
            this.inputView.Size = new System.Drawing.Size(302, 348);
            this.inputView.TabIndex = 1;
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.GroupName = "Output";
            this.outputView.Location = new System.Drawing.Point(314, 6);
            this.outputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputView.Name = "outputView";
            this.outputView.Size = new System.Drawing.Size(302, 348);
            this.outputView.TabIndex = 1;
            // 
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.BackColor = System.Drawing.Color.White;
            this.mainTableLayoutPanel.SetColumnSpan(this.gbPositionTeaching, 2);
            this.gbPositionTeaching.Controls.Add(this.positionTableLayoutPanel);
            this.gbPositionTeaching.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbPositionTeaching.Location = new System.Drawing.Point(3, 3);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Size = new System.Drawing.Size(626, 384);
            this.gbPositionTeaching.TabIndex = 8;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Position Teaching";
            // 
            // positionTableLayoutPanel
            // 
            this.positionTableLayoutPanel.ColumnCount = 2;
            this.positionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.positionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.positionTableLayoutPanel.Controls.Add(this.gbTeachingMove, 1, 1);
            this.positionTableLayoutPanel.Controls.Add(this.editorPanel, 1, 0);
            this.positionTableLayoutPanel.Controls.Add(this.positionItemView, 0, 0);
            this.positionTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionTableLayoutPanel.Location = new System.Drawing.Point(3, 21);
            this.positionTableLayoutPanel.Name = "positionTableLayoutPanel";
            this.positionTableLayoutPanel.RowCount = 2;
            this.positionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.positionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.positionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.positionTableLayoutPanel.Size = new System.Drawing.Size(620, 360);
            this.positionTableLayoutPanel.TabIndex = 13;
            // 
            // gbTeachingMove
            // 
            this.gbTeachingMove.BackColor = System.Drawing.Color.White;
            this.gbTeachingMove.Controls.Add(this.btnMovePosition);
            this.gbTeachingMove.Controls.Add(this.rbTeachingMoveMode);
            this.gbTeachingMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbTeachingMove.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbTeachingMove.Location = new System.Drawing.Point(251, 219);
            this.gbTeachingMove.Name = "gbTeachingMove";
            this.gbTeachingMove.Size = new System.Drawing.Size(366, 138);
            this.gbTeachingMove.TabIndex = 7;
            this.gbTeachingMove.TabStop = false;
            this.gbTeachingMove.Text = "Teaching Move";
            // 
            // btnMovePosition
            // 
            this.btnMovePosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMovePosition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnMovePosition.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnMovePosition.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMovePosition.CustomForeColor = System.Drawing.Color.Black;
            this.btnMovePosition.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnMovePosition.ForeColor = System.Drawing.Color.Black;
            this.btnMovePosition.ImageSize = new System.Drawing.Size(45, 45);
            this.btnMovePosition.Location = new System.Drawing.Point(242, 34);
            this.btnMovePosition.Name = "btnMovePosition";
            this.btnMovePosition.Size = new System.Drawing.Size(117, 95);
            this.btnMovePosition.TabIndex = 6;
            this.btnMovePosition.TabStop = false;
            this.btnMovePosition.Text = "Move\r\nPosition";
            this.btnMovePosition.UseVisualStyleBackColor = false;
            // 
            // rbTeachingMoveMode
            // 
            this.rbTeachingMoveMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rbTeachingMoveMode.GroupName = "Move Mode";
            this.rbTeachingMoveMode.Location = new System.Drawing.Point(6, 25);
            this.rbTeachingMoveMode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.rbTeachingMoveMode.Name = "rbTeachingMoveMode";
            this.rbTeachingMoveMode.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.rbTeachingMoveMode.SelectedIndex = -1;
            this.rbTeachingMoveMode.Size = new System.Drawing.Size(230, 105);
            this.rbTeachingMoveMode.TabIndex = 5;
            // 
            // editorPanel
            // 
            this.editorPanel.Controls.Add(this.button_Test);
            this.editorPanel.Controls.Add(this.positionEditorView);
            this.editorPanel.Controls.Add(this.btnCancel);
            this.editorPanel.Controls.Add(this.btnSave);
            this.editorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorPanel.Location = new System.Drawing.Point(251, 3);
            this.editorPanel.Name = "editorPanel";
            this.editorPanel.Size = new System.Drawing.Size(366, 210);
            this.editorPanel.TabIndex = 8;
            // 
            // button_Test
            // 
            this.button_Test.Location = new System.Drawing.Point(147, 173);
            this.button_Test.Name = "button_Test";
            this.button_Test.Size = new System.Drawing.Size(75, 23);
            this.button_Test.TabIndex = 12;
            this.button_Test.Text = "Test";
            this.button_Test.UseVisualStyleBackColor = true;
            // 
            // positionEditorView
            // 
            this.positionEditorView.GroupName = "Editor";
            this.positionEditorView.Location = new System.Drawing.Point(4, 4);
            this.positionEditorView.Margin = new System.Windows.Forms.Padding(4);
            this.positionEditorView.Name = "positionEditorView";
            this.positionEditorView.Size = new System.Drawing.Size(358, 145);
            this.positionEditorView.TabIndex = 0;
            this.positionEditorView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnCancel.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnCancel.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.CustomForeColor = System.Drawing.Color.Black;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.Black;
            this.btnCancel.ImageSize = new System.Drawing.Size(45, 45);
            this.btnCancel.Location = new System.Drawing.Point(263, 156);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 45);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSave.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnSave.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnSave.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.CustomForeColor = System.Drawing.Color.Black;
            this.btnSave.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.Black;
            this.btnSave.ImageSize = new System.Drawing.Size(45, 45);
            this.btnSave.Location = new System.Drawing.Point(4, 156);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 45);
            this.btnSave.TabIndex = 3;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // positionItemView
            // 
            this.positionItemView.BorderWidth = 2;
            this.positionItemView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionItemView.GroupName = "Position Item";
            this.positionItemView.Location = new System.Drawing.Point(3, 8);
            this.positionItemView.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.positionItemView.Name = "positionItemView";
            this.positionTableLayoutPanel.SetRowSpan(this.positionItemView, 2);
            this.positionItemView.SelectedIndex = -1;
            this.positionItemView.Size = new System.Drawing.Size(242, 344);
            this.positionItemView.TabIndex = 2;
            // 
            // gbMoveAxis
            // 
            this.gbMoveAxis.BackColor = System.Drawing.Color.White;
            this.gbMoveAxis.Controls.Add(this.jogControl);
            this.gbMoveAxis.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(635, 3);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.mainTableLayoutPanel.SetRowSpan(this.gbMoveAxis, 2);
            this.gbMoveAxis.Size = new System.Drawing.Size(310, 774);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";
            // 
            // jogControl
            // 
            this.jogControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jogControl.Location = new System.Drawing.Point(3, 21);
            this.jogControl.Margin = new System.Windows.Forms.Padding(0);
            this.jogControl.Name = "jogControl";
            this.jogControl.Size = new System.Drawing.Size(304, 750);
            this.jogControl.TabIndex = 0;
            // 
            // CommonUnitConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "CommonUnitConfig";
            this.Size = new System.Drawing.Size(1264, 780);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.gbPositionTeaching.ResumeLayout(false);
            this.positionTableLayoutPanel.ResumeLayout(false);
            this.gbTeachingMove.ResumeLayout(false);
            this.editorPanel.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private Common.ListBoxItemsView axisPositionsView;
        private System.Windows.Forms.GroupBox gbDigitalIO;
        private System.Windows.Forms.TableLayoutPanel ioTableLayoutPanel;
        private Common.IOPropertyCollectionView inputView;
        private Common.IOPropertyCollectionView outputView;
        private System.Windows.Forms.GroupBox gbPositionTeaching;
        private System.Windows.Forms.TableLayoutPanel positionTableLayoutPanel;
        private System.Windows.Forms.GroupBox gbTeachingMove;
        private Common.IndividualMenuButton btnMovePosition;
        private Common.RadioButtonView rbTeachingMoveMode;
        private System.Windows.Forms.Panel editorPanel;
        private System.Windows.Forms.Button button_Test;
        private Common.PropertyCollectionView positionEditorView;
        private Common.IndividualMenuButton btnCancel;
        private Common.IndividualMenuButton btnSave;
        private Common.ListBoxItemsView positionItemView;
        private System.Windows.Forms.GroupBox gbMoveAxis;
        private JogControl jogControl;
    }
}
