using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.DIO; // 추가
using QMC.Common.IO;  // DIOUnit, DIOModuleSetup
using QMC.Common.Motions;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Component;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class IndexLoadAlignerUnit_Config
    {
        private IOPropertyCollectionView inputView;
        private IOPropertyCollectionView outputView;

        // Axis 목록 (추가)
        private ListBoxItemsView axisListBoxItemsView;

        private IndividualMenuButton btnSave;
        private IndividualMenuButton btnCancel;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbMoveAxis;

        private JogControl jogControl;

        private System.ComponentModel.IContainer components = null;

        // Actual Position 주기 업데이트 타이머
        private Timer _axisPosTimer;

        // === Digital IO 표시용 내부 구조 추가 (기존 코드 유지) ===
        private struct _IoRef { public string Module; public string Disp; public PropertyState Prop; }
        private readonly List<_IoRef> _ioInputs = new List<_IoRef>();
        // 출력 사용 안함
        private readonly List<_IoRef> _ioOutputs = new List<_IoRef>();
        // 타이머 제거 (실시간 스캔 이벤트 사용)
        private Timer _ioTimer; // 남겨두되 사용 안함

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
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.gbTeachingMove = new System.Windows.Forms.GroupBox();
            this.btnMovePosition = new QMC.Common.IndividualMenuButton();
            this.rbTeachingMoveMode = new QMC.Common.RadioButtonView();
            this.editorPanel = new System.Windows.Forms.Panel();
            this.positionEditorView = new QMC.Common.PropertyCollectionView();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.positionItemView = new QMC.Common.ListBoxItemsView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.ioTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inputView = new QMC.Common.IOPropertyCollectionView();
            this.outputView = new QMC.Common.IOPropertyCollectionView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.axisPositionsView = new QMC.Common.ListBoxItemsView();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.positionItemPanel = new System.Windows.Forms.Panel();
            this.gbPositionTeaching.SuspendLayout();
            this.positionTableLayoutPanel.SuspendLayout();
            this.gbTeachingMove.SuspendLayout();
            this.editorPanel.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.ioTableLayoutPanel.SuspendLayout();
            this.gbMoveAxis.SuspendLayout();
            this.mainTableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
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
            this.btnMovePosition.Click += new System.EventHandler(this.btnMovePosition_Click);
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
            this.editorPanel.Controls.Add(this.positionEditorView);
            this.editorPanel.Controls.Add(this.btnCancel);
            this.editorPanel.Controls.Add(this.btnSave);
            this.editorPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorPanel.Location = new System.Drawing.Point(251, 3);
            this.editorPanel.Name = "editorPanel";
            this.editorPanel.Size = new System.Drawing.Size(366, 210);
            this.editorPanel.TabIndex = 8;
            // 
            // positionEditorView
            // 
            this.positionEditorView.FastBuild = true;
            this.positionEditorView.GroupName = "Editor";
            this.positionEditorView.Location = new System.Drawing.Point(4, 4);
            this.positionEditorView.Margin = new System.Windows.Forms.Padding(4);
            this.positionEditorView.Name = "positionEditorView";
            this.positionEditorView.Size = new System.Drawing.Size(358, 145);
            this.positionEditorView.SuppressResizeInvalidation = true;
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
            this.btnCancel.Text = "CurrentPos";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCurrentPos_Click);
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
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // positionItemView
            // 
            this.positionItemView.BorderColor = System.Drawing.Color.White;
            this.positionItemView.BorderWidth = 2;
            this.positionItemView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.positionItemView.GroupBackColor = System.Drawing.Color.White;
            this.positionItemView.GroupForeColor = System.Drawing.Color.Black;
            this.positionItemView.GroupName = "Position Item";
            this.positionItemView.ItemBackColor = System.Drawing.Color.Black;
            this.positionItemView.ItemForeColor = System.Drawing.Color.Lime;
            this.positionItemView.ListBackColor = System.Drawing.Color.Black;
            this.positionItemView.ListForeColor = System.Drawing.Color.Lime;
            this.positionItemView.Location = new System.Drawing.Point(3, 8);
            this.positionItemView.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.positionItemView.Name = "positionItemView";
            this.positionTableLayoutPanel.SetRowSpan(this.positionItemView, 2);
            this.positionItemView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.positionItemView.SelectedForeColor = System.Drawing.Color.Black;
            this.positionItemView.SelectedIndex = -1;
            this.positionItemView.Size = new System.Drawing.Size(242, 344);
            this.positionItemView.TabIndex = 2;
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
            this.inputView.FastBuild = true;
            this.inputView.FastInitialPaint = true;
            this.inputView.GroupName = "Input";
            this.inputView.ListBackColor = System.Drawing.Color.Black;
            this.inputView.ListForeColor = System.Drawing.Color.Lime;
            this.inputView.Location = new System.Drawing.Point(4, 6);
            this.inputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.inputView.Name = "inputView";
            this.inputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.inputView.SelectedForeColor = System.Drawing.Color.Black;
            this.inputView.Size = new System.Drawing.Size(302, 348);
            this.inputView.SuppressResizeInvalidation = true;
            this.inputView.TabIndex = 1;
            // 
            // outputView
            // 
            this.outputView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputView.FastBuild = true;
            this.outputView.FastInitialPaint = true;
            this.outputView.GroupName = "Output";
            this.outputView.ListBackColor = System.Drawing.Color.Black;
            this.outputView.ListForeColor = System.Drawing.Color.Lime;
            this.outputView.Location = new System.Drawing.Point(314, 6);
            this.outputView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputView.Name = "outputView";
            this.outputView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.outputView.SelectedForeColor = System.Drawing.Color.Black;
            this.outputView.Size = new System.Drawing.Size(302, 348);
            this.outputView.SuppressResizeInvalidation = true;
            this.outputView.TabIndex = 1;
            // ★ 출력 항목 클릭 이벤트 연결 (토글)
            this.outputView.ItemClicked -= new System.EventHandler<string>(this.OnOutputItemClicked);
            this.outputView.ItemClicked += new System.EventHandler<string>(this.OnOutputItemClicked);
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
            // axisPositionsView
            // 
            this.axisPositionsView.BorderColor = System.Drawing.Color.White;
            this.axisPositionsView.BorderWidth = 2;
            this.axisPositionsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axisPositionsView.GroupBackColor = System.Drawing.Color.White;
            this.axisPositionsView.GroupForeColor = System.Drawing.Color.Black;
            this.axisPositionsView.GroupName = "Axis Positions";
            this.axisPositionsView.ItemBackColor = System.Drawing.Color.Black;
            this.axisPositionsView.ItemForeColor = System.Drawing.Color.Lime;
            this.axisPositionsView.ListBackColor = System.Drawing.Color.Black;
            this.axisPositionsView.ListForeColor = System.Drawing.Color.Lime;
            this.axisPositionsView.Location = new System.Drawing.Point(951, 3);
            this.axisPositionsView.Name = "axisPositionsView";
            this.mainTableLayoutPanel.SetRowSpan(this.axisPositionsView, 2);
            this.axisPositionsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.axisPositionsView.SelectedForeColor = System.Drawing.Color.Black;
            this.axisPositionsView.SelectedIndex = -1;
            this.axisPositionsView.Size = new System.Drawing.Size(310, 774);
            this.axisPositionsView.TabIndex = 11;
            // 
            // axisListBoxItemsView
            // 
            this.axisListBoxItemsView.BorderColor = System.Drawing.Color.White;
            this.axisListBoxItemsView.BorderWidth = 2;
            this.axisListBoxItemsView.GroupBackColor = System.Drawing.Color.White;
            this.axisListBoxItemsView.GroupForeColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.GroupName = "";
            this.axisListBoxItemsView.ItemBackColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.ItemForeColor = System.Drawing.Color.Lime;
            this.axisListBoxItemsView.ListBackColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.ListForeColor = System.Drawing.Color.Lime;
            this.axisListBoxItemsView.Location = new System.Drawing.Point(8, 18);
            this.axisListBoxItemsView.Name = "axisListBoxItemsView";
            this.axisListBoxItemsView.SelectedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            this.axisListBoxItemsView.SelectedForeColor = System.Drawing.Color.Black;
            this.axisListBoxItemsView.SelectedIndex = -1;
            this.axisListBoxItemsView.Size = new System.Drawing.Size(234, 124);
            this.axisListBoxItemsView.TabIndex = 0;
            this.axisListBoxItemsView.ItemSelected += new System.EventHandler<int>(this.OnAxisSelected);
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
            this.mainTableLayoutPanel.TabIndex = 12;
            // 
            // positionItemPanel
            // 
            this.positionItemPanel.Location = new System.Drawing.Point(0, 0);
            this.positionItemPanel.Name = "positionItemPanel";
            this.positionItemPanel.Size = new System.Drawing.Size(200, 100);
            this.positionItemPanel.TabIndex = 0;
            // 
            // IndexLoadAlignerUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 780);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Name = "IndexLoadAlignerUnit_Config";
            this.Text = "IndexLoadAligner Unit Configuration";
            this.gbPositionTeaching.ResumeLayout(false);
            this.positionTableLayoutPanel.ResumeLayout(false);
            this.gbTeachingMove.ResumeLayout(false);
            this.editorPanel.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.ioTableLayoutPanel.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        /// <summary>
        /// JogControl 에 해당 Unit 의 모든 축을 표시 (Position 선택과 무관)
        /// </summary>
        private void PopulateAllAxesInJogControl()
        {
            try
            {
                if (jogControl == null) return;
                const string UNIT_NAME = "IndexLoadAligner";
                var eq = Equipment.Instance;
                if (eq?.Units == null) return;
                if (!eq.Units.TryGetValue(UNIT_NAME, out var unit)) return;
                var ejector = unit as IndexLoadAligner;
                if (ejector?.Axes == null || ejector.Axes.Count == 0)
                {
                    jogControl.SetTeachingAxisList(null); // 비움
                    return;
                }
                var axisNames = ejector.Axes.Values
                    .Where(a => a != null)
                    .Select(a => a.Name ?? a.Setup?.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToArray();
                jogControl.SetTeachingAxisList(axisNames);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PopulateAllAxesInJogControl error: " + ex.Message);
            }
        }

        private void InitializeUI()
        {
            try
            {
                // 🚀 PropertyPosition을 사용하여 Position Item들을 listBoxItemsView에 설정
                SetAxisDefinitionsToAxisListBox();

                // 🚀 Position Item 선택 이벤트 연결
                SetupPositionItemSelectionEvent();

                InitializeRadioButtonView();
                InitializeDigitalIO();            // ★ Digital IO 초기화 추가
                PopulateAllAxesInJogControl(); // ★ 모든 축 표시
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        // ===== Digital IO 초기화 (IndexLoadAligner Unit 관련 IO 자동 필터) =====
        private void InitializeDigitalIO()
        {
            try
            {
                if (inputView == null)
                    return;

                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                var unitIO = eq?.UnitIO;
                if (scan == null || unitIO == null)
                {
                    inputView.SetProperties(new PropertyCollection());
                    outputView.SetProperties(new PropertyCollection());
                    return;
                }

                _ioInputs.Clear();
                _ioOutputs.Clear();

                HardInputDef[] hardInputs;
                HardOutputDef[] hardOutputs;

                const string UNIT_NAME = "IndexLoadAligner";
                if (eq?.Units != null &&
                    eq.Units.TryGetValue(UNIT_NAME, out var unit) &&
                    unit is IndexLoadAligner aligner &&
                    aligner.IndexLoadAlignerConfig != null)
                {
                    var cfg = aligner.IndexLoadAlignerConfig;

                    // Config에 HardInputs가 선언되어 있지 않으면 처리하지 않음
                    var cfgType = cfg.GetType();
                    var piIn = cfgType.GetProperty("HardInputs");
                    if (piIn == null)
                    {
                        inputView.SetProperties(new PropertyCollection());
                        outputView.SetProperties(new PropertyCollection());
                        return;
                    }

                    hardInputs = piIn.GetValue(cfg) as HardInputDef[] ?? Array.Empty<HardInputDef>();

                    // HardInputs가 비어 있으면 처리하지 않음
                    if (hardInputs.Length == 0)
                    {
                        inputView.SetProperties(new PropertyCollection());
                    }

                    // ★ Output도 동일 정책 적용: 프로퍼티 없거나 비어 있으면 출력만 건너뜀
                    var piOut = cfgType.GetProperty("HardOutputs");
                    if (piOut == null)
                    {
                        hardOutputs = Array.Empty<HardOutputDef>();
                        outputView.SetProperties(new PropertyCollection());
                    }
                    else
                    {
                        hardOutputs = piOut.GetValue(cfg) as HardOutputDef[] ?? Array.Empty<HardOutputDef>();
                        if (hardOutputs.Length == 0)
                        {
                            outputView.SetProperties(new PropertyCollection());
                        }
                    }
                }
                else
                {
                    hardInputs = Array.Empty<HardInputDef>();
                    hardOutputs = Array.Empty<HardOutputDef>();
                }

                // 모듈명 매핑
                Func<string, Tuple<string, string>> resolveIn = disp =>
                {
                    if (unitIO?.Modules == null) return new Tuple<string, string>(null, disp);
                    foreach (var m in unitIO.Modules)
                    {
                        if (m?.Inputs == null) continue;
                        foreach (var ch in m.Inputs)
                        {
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                                return new Tuple<string, string>(m.ModuleName, ch.DisplayNo);
                        }
                    }
                    return new Tuple<string, string>(null, disp);
                };
                Func<string, Tuple<string, string>> resolveOut = disp =>
                {
                    if (unitIO?.Modules == null) return new Tuple<string, string>(null, disp);
                    foreach (var m in unitIO.Modules)
                    {
                        if (m?.Outputs == null) continue;
                        foreach (var ch in m.Outputs)
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                                return new Tuple<string, string>(m.ModuleName, ch.DisplayNo);
                    }
                    return new Tuple<string, string>(null, disp);
                };

                if (hardInputs != null && hardInputs.Length > 0)
                {

                    var pcIn = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
                    pcIn.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var item in hardInputs)
                    {
                        var map = resolveIn(item.Disp);
                        bool cur = false;
                        if (map.Item1 != null) scan.TryGetInput(map.Item1, map.Item2, out cur);
                        string nameCell = $"{item.Disp} {item.Name}";
                        var ps = new PropertyState(item.No.ToString(), nameCell, cur);
                        pcIn.Add(ps);
                        _ioInputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
                    }
                    inputView.SetProperties(pcIn);
                }
                else
                {
                    inputView.SetProperties(new PropertyCollection());
                }

                if (hardOutputs != null && hardOutputs.Length > 0)
                {
                    var pcOut = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
                    pcOut.Add(new TitleOnlyProperty("No", "Name", "State"));
                    foreach (var item in hardOutputs)
                    {
                        var map = resolveOut(item.Disp);
                        bool cur = false; // 필요 시 현재 출력 상태 조회 API로 대체
                        string nameCell = $"{item.Disp} {item.Name}";
                        var ps = new PropertyState(item.No.ToString(), nameCell, cur);
                        pcOut.Add(ps);
                        _ioOutputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
                    }
                    outputView.SetProperties(pcOut);
                }
                else
                {
                    outputView.SetProperties(new PropertyCollection());
                }

                // 이벤트 중복 등록 방지 후 등록
                scan.InputChanged -= OnDioInputChanged;
                scan.InputChanged += OnDioInputChanged;
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeDigitalIO error: " + ex.Message);
            }
        }

        private void OnDioInputChanged(string module, string disp, bool value)
        {
            try
            {
                for (int i = 0; i < _ioInputs.Count; i++)
                {
                    if (_ioInputs[i].Module == module && string.Equals(_ioInputs[i].Disp, disp, StringComparison.OrdinalIgnoreCase))
                    {
                        _ioInputs[i].Prop.State = value; // 모델 업데이트
                        // 색상 갱신
                        inputView.SetStateByKey(disp, value);
                        break;
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// CassetteElevator + WaferTransferArm 의 AxisDefinition DisplayName 을 axisListBoxItemsView 에 설정
        /// </summary>
        private void SetAxisDefinitionsToAxisListBox()
        {
            try
            {
                // Equipment에서 IndexLoadAligner Unit 가져오기
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "IndexLoadAligner";

                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var aligner = unit as IndexLoadAligner;
                    // TeachingPositions 멤버를 직접 사용하여 Position 이름 리스트 추출
                    if (aligner?.TeachingPositions != null && aligner.TeachingPositions.Count > 0)
                    {
                        var positionNames = aligner.TeachingPositions.Select(tp => tp.Name).ToArray();
                        positionItemView?.SetItems(positionNames);
                        Console.WriteLine($"✅ TeachingPositions를 listBoxItemsView에 설정 완료: {positionNames.Length}개 항목");
                        Console.WriteLine($"   설정된 항목들: {string.Join(", ", positionNames)}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ TeachingPositions에 Position 항목이 없습니다.");
                        positionItemView?.SetItems();
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ '{UNIT_NAME}' Unit을 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ TeachingPositions 설정 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 Position Item 선택 이벤트 설정
        /// </summary>
        private void SetupPositionItemSelectionEvent()
        {
            if (positionItemView != null)
            {
                // 기존 이벤트 핸들러 제거 (중복 방지)
                positionItemView.ItemSelected -= OnPositionItemSelected;

                // 새 이벤트 핸들러 등록
                positionItemView.ItemSelected += OnPositionItemSelected;

                Console.WriteLine("✅ Position Item 선택 이벤트 설정 완료");
            }
        }
        /// <summary>
        /// 🚀 Position Item 선택 이벤트 처리
        /// </summary>
        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {
                ShowTeachingPositionInPropertyCollectionView(selectedIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position Item 선택 처리 중 오류: {ex.Message}");
            }
        }

        private void ShowTeachingPositionInPropertyCollectionView(int selectedIndex)
        {
            // Equipment에서 IndexLoadAligner Unit 가져오기
            var equipment = Equipment.Instance;
            const string UNIT_NAME = "IndexLoadAligner";
            if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
            {
                var aligner = unit as IndexLoadAligner;
                var config = aligner?.IndexLoadAlignerConfig;
                if (config?.TeachingPositions != null && selectedIndex >= 0 && selectedIndex < config.TeachingPositions.Count)
                {
                    var tp = config.TeachingPositions[selectedIndex];
                    var editorProperties = new PropertyCollection();
                    editorProperties.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name} (mm, Abs. Pos)"));
                    editorProperties.Add(new StringProperty("Description", tp.Description ?? ""));
                    // 축별 위치값 표시
                    foreach (var axis in tp.AxisPositions)
                    {
                        editorProperties.Add(new DoubleProperty($"{axis.Key} Position (mm)", axis.Value));
                    }
                    // 추가 정보 표시
                    foreach (var kv in tp.ExtraInfo)
                    {
                        editorProperties.Add(new StringProperty($"Extra: {kv.Key}", kv.Value?.ToString() ?? ""));
                    }
                    positionEditorView?.SetProperties(editorProperties);
                }
            }
        }

        private void InitializeTeachingPositionList()
        {
            // Equipment에서 IndexLoadAligner Unit 가져오기
            var equipment = Equipment.Instance;
            const string UNIT_NAME = "IndexLoadAligner";
            if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
            {
                var aligner = unit as IndexLoadAligner;
                var config = aligner?.IndexLoadAlignerConfig;
                if (config?.TeachingPositions != null)
                {
                    var positionNames = config.TeachingPositions.Select(tp => tp.Name).ToArray();
                    positionItemView.SetItems(positionNames);
                }
            }
        }

        private void OnAxisSelected(object sender, int index)
        {

        }

        private void UpdateAxisActualPosition()
        {

        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            try
            {
                const string UNIT_NAME = "IndexLoadAligner";
                var equipment = Equipment.Instance;
                if (!equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var aligner = unit as IndexLoadAligner;
                if (aligner == null)
                {
                    MessageBox.Show("Unit 형식 오류", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 선택된 Teaching Position 인덱스
                int selIndex = -1;
                try
                {
                    var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                    if (pi != null)
                    {
                        object val = pi.GetValue(positionItemView, null);
                        if (val is int) selIndex = (int)val;
                    }
                }
                catch { selIndex = -1; }

                if (selIndex < 0 || selIndex >= aligner.IndexLoadAlignerConfig.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = aligner.IndexLoadAlignerConfig.TeachingPositions[selIndex];

                // Fine / Coarse 판단 (RadioButtonView SelectedIndex: 0=Fine, 1=Coarse)
                bool isFine = true;
                if (rbTeachingMoveMode != null)
                {
                    try
                    {
                        var siProp = rbTeachingMoveMode.GetType().GetProperty("SelectedIndex");
                        if (siProp != null)
                        {
                            object v = siProp.GetValue(rbTeachingMoveMode, null);
                            if (v is int) isFine = ((int)v) == 0; // 0 → Fine
                        }
                    }
                    catch { isFine = true; }
                }

                // 축 이동 파라미터 수집 및 동시 이동
                // 기본값 (Config 값 없거나 0일 때 폴백)
                double defaultFineVel = 5.0;
                double defaultCoarseVel = 20.0;
                double defaultAcc = 10.0;
                double defaultDec = 10.0;
                double defaultJerk = 50.0;

                var moveResults = new List<Tuple<string, int>>();

                foreach (var kv in tp.AxisPositions)
                {
                    string axisKey = kv.Key;
                    double targetPos = kv.Value;

                    // 축 찾기: TeachingPosition.Axes 사전 우선 → 없으면 Unit.Axes에서 키 또는 Name 으로 재검색
                    MotionAxis axis = null;
                    if (tp.Axes != null && tp.Axes.TryGetValue(axisKey, out axis)) { }
                    if (axis == null && aligner.Axes.TryGetValue(axisKey, out var directAxis)) axis = directAxis;
                    if (axis == null)
                    {
                        // Name 매칭 시도
                        foreach (var aPair in aligner.Axes)
                        {
                            if (aPair.Value != null && string.Equals(aPair.Value.Name, axisKey, StringComparison.OrdinalIgnoreCase))
                            {
                                axis = aPair.Value; break;
                            }
                        }
                    }
                    if (axis == null) continue; // 해당 축 없음 → 스킵

                    // 속도/가감속/jerk 결정
                    double vel = isFine ? (axis.Config != null && axis.Config.JogFineVelocity > 0 ? axis.Config.JogFineVelocity : defaultFineVel)
                                        : (axis.Config != null && axis.Config.JogCoarseVelocity > 0 ? axis.Config.JogCoarseVelocity : defaultCoarseVel);
                    double acc = axis.Config != null && axis.Config.JogAcc > 0 ? axis.Config.JogAcc : defaultAcc;
                    double dec = axis.Config != null && axis.Config.JogDec > 0 ? axis.Config.JogDec : defaultDec;
                    double jerk = axis.Config != null ? (axis.Config.AccJerkPercent + axis.Config.DecJerkPercent) / 2.0 : defaultJerk;

                    // 이동 명령 전송 (비동기 실행; 완료는 WaitMoveDone 사용)
                    int rc = axis.MoveAbs(targetPos, vel, acc, dec, jerk);
                    moveResults.Add(new Tuple<string, int>(axisKey, rc));
                }

                // 이동 완료 대기 (모든 축 대상으로 최대 공통 Timeout 사용: 각 axis.Setup.MoveTimeoutMs)
                int waitErrors = 0;
                foreach (var kv in tp.AxisPositions)
                {
                    MotionAxis axis = null;
                    if (tp.Axes != null && tp.Axes.TryGetValue(kv.Key, out axis)) { }
                    if (axis == null && aligner.Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
                    if (axis == null) continue;

                    int rc = axis.WaitMoveDone(-1); // axis.Setup.MoveTimeoutMs 사용
                    if (rc != 0) waitErrors++;
                }

                // 결과 요약
                bool anyMoveFail = moveResults.Exists(t => t.Item2 != 0) || waitErrors > 0;
                if (!anyMoveFail)
                    MessageBox.Show("Teaching Position 이동 완료", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("일부 축 이동 실패 또는 타임아웃", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Move 처리 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeRadioButtonView()
        {
            try
            {
                rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RadioButtonView 오류: {ex.Message}");
            }
        }

        #region Save / Cancel

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                const string UNIT_NAME = "IndexLoadAligner";
                var equipment = Equipment.Instance;
                if (!equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var aligner = unit as IndexLoadAligner;
                if (aligner == null)
                {
                    MessageBox.Show("Unit 형식이 올바르지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 현재 선택된 Teaching Position 인덱스
                int selIndex = -1;
                try
                {
                    // ListBoxItemsView에 SelectedIndex 프로퍼티가 있다고 가정
                    var pi = positionItemView.GetType().GetProperty("SelectedIndex");
                    if (pi != null)
                    {
                        object val = pi.GetValue(positionItemView, null);
                        if (val is int) selIndex = (int)val;
                    }
                }
                catch { selIndex = -1; }

                if (selIndex < 0 || selIndex >= aligner.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 에디터(PropertyCollectionView)에 입력된 값 적용(안전 차원)
                positionEditorView?.Apply();

                var props = positionEditorView?.GetCurrentProperties();
                if (props == null || props.Count == 0)
                {
                    MessageBox.Show("편집할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var target = aligner.TeachingPositions[selIndex];

                // 기존 AxisPositions 복사 후 수정
                var newAxisPositions = new Dictionary<string, double>(target.AxisPositions != null ? target.AxisPositions : new Dictionary<string, double>());
                string newDescription = target.Description;
                Dictionary<string, object> newExtra = target.ExtraInfo != null ? new Dictionary<string, object>(target.ExtraInfo) : new Dictionary<string, object>();

                foreach (var p in props)
                {
                    // Description
                    if (p is StringProperty && string.Equals(p.Title, "Description", StringComparison.OrdinalIgnoreCase))
                    {
                        var sp = (StringProperty)p;
                        newDescription = sp.Value ?? string.Empty;
                        continue;
                    }
                    // Axis Position (DoubleProperty) → Title 패턴: "{AxisKey} Position (mm)"
                    if (p is DoubleProperty && p.Title.EndsWith(" Position (mm)", StringComparison.OrdinalIgnoreCase))
                    {
                        var dp = (DoubleProperty)p;
                        var axisKey = p.Title.Substring(0, p.Title.IndexOf(" Position (mm)")).Trim();
                        newAxisPositions[axisKey] = dp.Value;
                        continue;
                    }
                    // Extra: prefix "Extra: " (StringProperty)
                    if (p is StringProperty && p.Title.StartsWith("Extra:", StringComparison.OrdinalIgnoreCase))
                    {
                        var sp = (StringProperty)p;
                        var extraKey = p.Title.Substring("Extra:".Length).Trim();
                        newExtra[extraKey] = sp.Value;
                        continue;
                    }
                }

                // 수정 내용 TeachingPosition 객체에 반영
                target.Description = newDescription;
                target.AxisPositions = newAxisPositions; // 참조 교체(저장용 딥카피 목적)
                target.ExtraInfo = newExtra;

                // Config에도 반영 (SetTeachingPosition은 Saveconfig 호출 포함)
                aligner.IndexLoadAlignerConfig.SetTeachingPosition(new TeachingPosition(target.Name, new Dictionary<string, double>(target.AxisPositions), target.Description) { ExtraInfo = new Dictionary<string, object>(target.ExtraInfo) });

                // 저장 후 재로드 & 재바인딩 (선택적으로 최신 반영)
                aligner.IndexLoadAlignerConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
                aligner.TeachingPositions.Clear();
                foreach (var tp in aligner.IndexLoadAlignerConfig.TeachingPositions)
                    aligner.TeachingPositions.Add(tp);

                // 리스트 갱신
                SetAxisDefinitionsToAxisListBox();

                MessageBox.Show("변경된 Teaching Position이 저장되었습니다.", "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 처리 중 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }


        #endregion  #region Paint / Resize override (기존)

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            int centerX = this.ClientSize.Width / 2;
            using (Pen blackPen = new Pen(Color.Black, 2))
            {
                e.Graphics.DrawLine(blackPen, centerX, 0, centerX, this.ClientSize.Height);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        private static string NormalizeXYKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return raw;
            raw = raw.Trim().ToUpperInvariant();
            var m = Regex.Match(raw, @"^(X|Y)0*(\d+)$");
            if (m.Success)
            {
                // X / Y + 숫자 (선행 0 제거)
                var letter = m.Groups[1].Value;
                var digits = m.Groups[2].Value;
                if (string.IsNullOrEmpty(digits)) digits = "0";
                return letter + digits; // 예: Y026 -> Y26
            }
            return raw;
        }

        private void OnOutputItemClicked(object sender, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key)) return;

                // 설비 / 스캐너 참조
                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                if (scan == null) return;

                // 패딩 제거된 비교 키 (IOPropertyCollectionView 가 0 패딩 붙여도 매칭 가능하도록)
                var cmpKey = NormalizeXYKey(key);

                // 출력 목록에서 Display 번호(key)로 모듈 찾기 (직접 일치 또는 정규화 일치)
                string module = null;
                string originalDisp = null; // 실제 scan 호출에 사용할 DisplayNo (원본 저장값)
                for (int i = 0; i < _ioOutputs.Count; i++)
                {
                    var storedDisp = _ioOutputs[i].Disp; // Config에서 가져온 원본 (예: Y26)
                    if (string.Equals(storedDisp, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(NormalizeXYKey(storedDisp), cmpKey, StringComparison.OrdinalIgnoreCase))
                    {
                        module = _ioOutputs[i].Module;
                        originalDisp = storedDisp; // WriteOutput / TryGetOutput 시 사용
                        break;
                    }
                }
                if (string.IsNullOrEmpty(module) || string.IsNullOrEmpty(originalDisp)) return; // 매핑 실패

                // 현재 캐시 상태 읽기 (원본 Display 사용)
                bool before = false;
                scan.TryGetOutput(module, originalDisp, out before);

                // 사용자 확인
                var dr = MessageBox.Show($"[{module}:{originalDisp}] 현재 상태 = {before}\r\n변경하시겠습니까?", "Output Toggle", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr != DialogResult.Yes) return;

                // 토글 쓰기 (Reverse 처리는 DioScanService 내부에서 처리)
                int rc = scan.WriteOutput(module, originalDisp, !before);
                if (rc != 0)
                {
                    MessageBox.Show($"WriteOutput 실패 (rc={rc})", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 캐시 동기화
                scan.RefreshOnce();
                bool after = before;
                scan.TryGetOutput(module, originalDisp, out after);

                // UI 키는 클릭된 key 그대로 갱신 시도 (패딩 포함), 실패 시 원본로 재시도
                try
                {
                    if (outputView != null)
                    {
                        outputView.SetStateByKey(key, after);      // 패딩 형태
                        if (!string.Equals(key, originalDisp, StringComparison.OrdinalIgnoreCase))
                            outputView.SetStateByKey(originalDisp, after); // 원본 형태도 반영
                        // 정규화된 키(Y26)만 저장되어 있을 가능성 → NormalizeXYKey(key) 재시도
                        var norm = NormalizeXYKey(key);
                        if (!string.Equals(norm, key, StringComparison.OrdinalIgnoreCase) && !string.Equals(norm, originalDisp, StringComparison.OrdinalIgnoreCase))
                            outputView.SetStateByKey(norm, after);
                    }
                }
                catch { }

                MessageBox.Show($"{originalDisp}: {before} -> {after}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Output 토글 처리 중 오류: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private ListBoxItemsView axisPositionsView;
        private TableLayoutPanel mainTableLayoutPanel;
        private TableLayoutPanel ioTableLayoutPanel;
        private Panel positionItemPanel;
        private ListBoxItemsView positionItemView;
        private TableLayoutPanel positionTableLayoutPanel;
        private PropertyCollectionView positionEditorView;
        private GroupBox gbTeachingMove;
        private IndividualMenuButton btnMovePosition;
        private RadioButtonView rbTeachingMoveMode;
        private Panel editorPanel;
    }
}