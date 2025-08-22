using QMC.Common;
using QMC.Common.CustomControl;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class CassetteLoadingElevatorUnit_Config
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private PropertyCollectionView PositionpropertyCollectionView;
        private IOPropertyCollectionView inputPropertyCollectionView;
        private IOPropertyCollectionView outputPropertyCollectionView;
        private ListBoxItemsView positionlistBoxItemsView;

        // Axis 목록 (추가)
        private ListBoxItemsView axisListBoxItemsView;

        // Position / Editor 버튼
        private IndividualMenuButton btnSave;
        private IndividualMenuButton btnCancel;
        private IndividualMenuButton btnMovePosition;
        private IndividualMenuButton btnZeroPosition;
        private IndividualMenuButton individualMenuButton1;

        private RadioButtonView rbTeachingMoveMode;
        private RadioButtonView rbCommandMoveMode;
        private CustomBorderLabel lblAxisPositionValue;
        private CustomBorderLabel lblAxisPositionCaption;
        private CustomBorderLabel lblAxisMovePositionCaption;
        
        private TextBox tbAxisMovePositionValue;

        // 원시 값 등 추가표시(선택) - 예시

        // 그룹박스들
        private GroupBox gbTeachingMove;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbCommandMove;


        // Move Axis 외곽 그룹박스 (추가)
        private GroupBox gbMoveAxis;
        private GroupBox gbSelectAxis;
        private GroupBox gbDestinationMoveMode;

        private System.ComponentModel.IContainer components = null;

        // Actual Position 주기 업데이트 타이머
        private Timer _axisPosTimer;

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
            this.gbTeachingMove = new System.Windows.Forms.GroupBox();
            this.btnMovePosition = new QMC.Common.IndividualMenuButton();
            this.rbTeachingMoveMode = new QMC.Common.RadioButtonView();
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionlistBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.PositionpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.inputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.gbJogMove = new System.Windows.Forms.GroupBox();
            this.individualMenuButton3 = new QMC.Common.IndividualMenuButton();
            this.individualMenuButton2 = new QMC.Common.IndividualMenuButton();
            this.tbAxisMoveStepSizeValue = new System.Windows.Forms.TextBox();
            this.lblAxisMoveStepSizeCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.rbJogMoveMode = new QMC.Common.RadioButtonView();
            this.gbCommandMove = new System.Windows.Forms.GroupBox();
            this.gbDestinationMoveMode = new System.Windows.Forms.GroupBox();
            this.individualMenuButton1 = new QMC.Common.IndividualMenuButton();
            this.lblAxisMovePositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.tbAxisMovePositionValue = new System.Windows.Forms.TextBox();
            this.rbCommandMoveMode = new QMC.Common.RadioButtonView();
            this.btnZeroPosition = new QMC.Common.IndividualMenuButton();
            this.gbSelectAxis = new System.Windows.Forms.GroupBox();
            this.lblAxisPositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lblAxisPositionValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.gbAxisPositions = new System.Windows.Forms.GroupBox();
            this.gbTeachingMove.SuspendLayout();
            this.gbPositionTeaching.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.gbMoveAxis.SuspendLayout();
            this.gbJogMove.SuspendLayout();
            this.gbCommandMove.SuspendLayout();
            this.gbDestinationMoveMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTeachingMove
            // 
            this.gbTeachingMove.BackColor = System.Drawing.Color.White;
            this.gbTeachingMove.Controls.Add(this.btnMovePosition);
            this.gbTeachingMove.Controls.Add(this.rbTeachingMoveMode);
            this.gbTeachingMove.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbTeachingMove.Location = new System.Drawing.Point(279, 209);
            this.gbTeachingMove.Name = "gbTeachingMove";
            this.gbTeachingMove.Size = new System.Drawing.Size(326, 138);
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
            this.btnMovePosition.Location = new System.Drawing.Point(200, 31);
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
            this.rbTeachingMoveMode.Location = new System.Drawing.Point(13, 28);
            this.rbTeachingMoveMode.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.rbTeachingMoveMode.Name = "rbTeachingMoveMode";
            this.rbTeachingMoveMode.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.rbTeachingMoveMode.SelectedIndex = -1;
            this.rbTeachingMoveMode.Size = new System.Drawing.Size(171, 98);
            this.rbTeachingMoveMode.TabIndex = 5;
            // 
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.BackColor = System.Drawing.Color.White;
            this.gbPositionTeaching.Controls.Add(this.positionlistBoxItemsView);
            this.gbPositionTeaching.Controls.Add(this.btnSave);
            this.gbPositionTeaching.Controls.Add(this.btnCancel);
            this.gbPositionTeaching.Controls.Add(this.gbTeachingMove);
            this.gbPositionTeaching.Controls.Add(this.PositionpropertyCollectionView);
            this.gbPositionTeaching.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbPositionTeaching.Location = new System.Drawing.Point(9, 12);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Size = new System.Drawing.Size(613, 361);
            this.gbPositionTeaching.TabIndex = 8;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Position Teaching";
            // 
            // positionlistBoxItemsView
            // 
            this.positionlistBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.positionlistBoxItemsView.BorderWidth = 2;
            this.positionlistBoxItemsView.GroupName = "Position Items";
            this.positionlistBoxItemsView.Location = new System.Drawing.Point(9, 34);
            this.positionlistBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.positionlistBoxItemsView.Name = "positionlistBoxItemsView";
            this.positionlistBoxItemsView.SelectedIndex = -1;
            this.positionlistBoxItemsView.Size = new System.Drawing.Size(257, 313);
            this.positionlistBoxItemsView.TabIndex = 2;
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
            this.btnSave.Location = new System.Drawing.Point(290, 143);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 40);
            this.btnSave.TabIndex = 3;
            this.btnSave.TabStop = false;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            this.btnCancel.Location = new System.Drawing.Point(496, 143);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 40);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // PositionpropertyCollectionView
            // 
            this.PositionpropertyCollectionView.GroupName = "Editor";
            this.PositionpropertyCollectionView.Location = new System.Drawing.Point(279, 34);
            this.PositionpropertyCollectionView.Margin = new System.Windows.Forms.Padding(4);
            this.PositionpropertyCollectionView.Name = "PositionpropertyCollectionView";
            this.PositionpropertyCollectionView.Size = new System.Drawing.Size(326, 168);
            this.PositionpropertyCollectionView.TabIndex = 0;
            this.PositionpropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.gbDigitalIO.Controls.Add(this.inputPropertyCollectionView);
            this.gbDigitalIO.Controls.Add(this.outputPropertyCollectionView);
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDigitalIO.Location = new System.Drawing.Point(9, 379);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(613, 358);
            this.gbDigitalIO.TabIndex = 9;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // inputPropertyCollectionView
            // 
            this.inputPropertyCollectionView.GroupName = "Input";
            this.inputPropertyCollectionView.Location = new System.Drawing.Point(9, 35);
            this.inputPropertyCollectionView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.inputPropertyCollectionView.Name = "inputPropertyCollectionView";
            this.inputPropertyCollectionView.Size = new System.Drawing.Size(295, 314);
            this.inputPropertyCollectionView.TabIndex = 1;
            // 
            // outputPropertyCollectionView
            // 
            this.outputPropertyCollectionView.GroupName = "Output";
            this.outputPropertyCollectionView.Location = new System.Drawing.Point(310, 35);
            this.outputPropertyCollectionView.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.outputPropertyCollectionView.Name = "outputPropertyCollectionView";
            this.outputPropertyCollectionView.Size = new System.Drawing.Size(295, 314);
            this.outputPropertyCollectionView.TabIndex = 1;
            // 
            // gbMoveAxis
            // 
            this.gbMoveAxis.BackColor = System.Drawing.Color.White;
            this.gbMoveAxis.Controls.Add(this.gbAxisPositions);
            this.gbMoveAxis.Controls.Add(this.gbJogMove);
            this.gbMoveAxis.Controls.Add(this.gbCommandMove);
            this.gbMoveAxis.Controls.Add(this.btnZeroPosition);
            this.gbMoveAxis.Controls.Add(this.gbSelectAxis);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionCaption);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionValue);
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(635, 12);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.gbMoveAxis.Size = new System.Drawing.Size(613, 724);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";
            // 
            // gbJogMove
            // 
            this.gbJogMove.BackColor = System.Drawing.Color.White;
            this.gbJogMove.Controls.Add(this.individualMenuButton3);
            this.gbJogMove.Controls.Add(this.individualMenuButton2);
            this.gbJogMove.Controls.Add(this.tbAxisMoveStepSizeValue);
            this.gbJogMove.Controls.Add(this.lblAxisMoveStepSizeCaption);
            this.gbJogMove.Controls.Add(this.rbJogMoveMode);
            this.gbJogMove.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbJogMove.Location = new System.Drawing.Point(11, 420);
            this.gbJogMove.Name = "gbJogMove";
            this.gbJogMove.Size = new System.Drawing.Size(288, 296);
            this.gbJogMove.TabIndex = 10;
            this.gbJogMove.TabStop = false;
            this.gbJogMove.Text = "Jog Move";
            // 
            // individualMenuButton3
            // 
            this.individualMenuButton3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton3.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton3.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton3.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton3.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton3.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton3.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton3.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton3.Location = new System.Drawing.Point(6, 201);
            this.individualMenuButton3.Name = "individualMenuButton3";
            this.individualMenuButton3.Size = new System.Drawing.Size(111, 50);
            this.individualMenuButton3.TabIndex = 12;
            this.individualMenuButton3.TabStop = false;
            this.individualMenuButton3.Text = "- Jog";
            this.individualMenuButton3.UseVisualStyleBackColor = false;
            // 
            // individualMenuButton2
            // 
            this.individualMenuButton2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton2.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton2.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton2.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton2.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton2.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton2.Location = new System.Drawing.Point(171, 201);
            this.individualMenuButton2.Name = "individualMenuButton2";
            this.individualMenuButton2.Size = new System.Drawing.Size(111, 50);
            this.individualMenuButton2.TabIndex = 11;
            this.individualMenuButton2.TabStop = false;
            this.individualMenuButton2.Text = "+ Jog";
            this.individualMenuButton2.UseVisualStyleBackColor = false;
            // 
            // tbAxisMoveStepSizeValue
            // 
            this.tbAxisMoveStepSizeValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbAxisMoveStepSizeValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.tbAxisMoveStepSizeValue.Location = new System.Drawing.Point(146, 120);
            this.tbAxisMoveStepSizeValue.Multiline = true;
            this.tbAxisMoveStepSizeValue.Name = "tbAxisMoveStepSizeValue";
            this.tbAxisMoveStepSizeValue.Size = new System.Drawing.Size(135, 35);
            this.tbAxisMoveStepSizeValue.TabIndex = 10;
            this.tbAxisMoveStepSizeValue.Text = "000.000";
            this.tbAxisMoveStepSizeValue.WordWrap = false;
            // 
            // lblAxisMoveStepSizeCaption
            // 
            this.lblAxisMoveStepSizeCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lblAxisMoveStepSizeCaption.BorderWidth = 1;
            this.lblAxisMoveStepSizeCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblAxisMoveStepSizeCaption.Location = new System.Drawing.Point(56, 120);
            this.lblAxisMoveStepSizeCaption.Name = "lblAxisMoveStepSizeCaption";
            this.lblAxisMoveStepSizeCaption.Size = new System.Drawing.Size(84, 35);
            this.lblAxisMoveStepSizeCaption.TabIndex = 9;
            this.lblAxisMoveStepSizeCaption.Text = "Step Size";
            this.lblAxisMoveStepSizeCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rbJogMoveMode
            // 
            this.rbJogMoveMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rbJogMoveMode.GroupName = "Move Mode";
            this.rbJogMoveMode.Location = new System.Drawing.Point(6, 25);
            this.rbJogMoveMode.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.rbJogMoveMode.Name = "rbJogMoveMode";
            this.rbJogMoveMode.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.rbJogMoveMode.SelectedIndex = -1;
            this.rbJogMoveMode.Size = new System.Drawing.Size(276, 87);
            this.rbJogMoveMode.TabIndex = 7;
            // 
            // gbCommandMove
            // 
            this.gbCommandMove.BackColor = System.Drawing.Color.White;
            this.gbCommandMove.Controls.Add(this.gbDestinationMoveMode);
            this.gbCommandMove.Controls.Add(this.rbCommandMoveMode);
            this.gbCommandMove.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbCommandMove.Location = new System.Drawing.Point(9, 223);
            this.gbCommandMove.Name = "gbCommandMove";
            this.gbCommandMove.Size = new System.Drawing.Size(290, 191);
            this.gbCommandMove.TabIndex = 8;
            this.gbCommandMove.TabStop = false;
            this.gbCommandMove.Text = "Command Move";
            // 
            // gbDestinationMoveMode
            // 
            this.gbDestinationMoveMode.BackColor = System.Drawing.Color.White;
            this.gbDestinationMoveMode.Controls.Add(this.individualMenuButton1);
            this.gbDestinationMoveMode.Controls.Add(this.lblAxisMovePositionCaption);
            this.gbDestinationMoveMode.Controls.Add(this.tbAxisMovePositionValue);
            this.gbDestinationMoveMode.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDestinationMoveMode.Location = new System.Drawing.Point(6, 115);
            this.gbDestinationMoveMode.Name = "gbDestinationMoveMode";
            this.gbDestinationMoveMode.Size = new System.Drawing.Size(278, 70);
            this.gbDestinationMoveMode.TabIndex = 9;
            this.gbDestinationMoveMode.TabStop = false;
            this.gbDestinationMoveMode.Text = "Destination Position";
            // 
            // individualMenuButton1
            // 
            this.individualMenuButton1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.individualMenuButton1.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.individualMenuButton1.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.CustomForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.individualMenuButton1.ForeColor = System.Drawing.Color.Black;
            this.individualMenuButton1.ImageSize = new System.Drawing.Size(45, 45);
            this.individualMenuButton1.Location = new System.Drawing.Point(195, 25);
            this.individualMenuButton1.Name = "individualMenuButton1";
            this.individualMenuButton1.Size = new System.Drawing.Size(78, 35);
            this.individualMenuButton1.TabIndex = 11;
            this.individualMenuButton1.TabStop = false;
            this.individualMenuButton1.Text = "Move";
            this.individualMenuButton1.UseVisualStyleBackColor = false;
            // 
            // lblAxisMovePositionCaption
            // 
            this.lblAxisMovePositionCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lblAxisMovePositionCaption.BorderWidth = 1;
            this.lblAxisMovePositionCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblAxisMovePositionCaption.Location = new System.Drawing.Point(4, 24);
            this.lblAxisMovePositionCaption.Name = "lblAxisMovePositionCaption";
            this.lblAxisMovePositionCaption.Size = new System.Drawing.Size(81, 35);
            this.lblAxisMovePositionCaption.TabIndex = 9;
            this.lblAxisMovePositionCaption.Text = "Move Pos.";
            this.lblAxisMovePositionCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbAxisMovePositionValue
            // 
            this.tbAxisMovePositionValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbAxisMovePositionValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.tbAxisMovePositionValue.Location = new System.Drawing.Point(90, 25);
            this.tbAxisMovePositionValue.Multiline = true;
            this.tbAxisMovePositionValue.Name = "tbAxisMovePositionValue";
            this.tbAxisMovePositionValue.Size = new System.Drawing.Size(99, 35);
            this.tbAxisMovePositionValue.TabIndex = 10;
            this.tbAxisMovePositionValue.Text = "000.000";
            this.tbAxisMovePositionValue.WordWrap = false;
            // 
            // rbCommandMoveMode
            // 
            this.rbCommandMoveMode.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rbCommandMoveMode.GroupName = "Move Mode";
            this.rbCommandMoveMode.Location = new System.Drawing.Point(6, 25);
            this.rbCommandMoveMode.Margin = new System.Windows.Forms.Padding(3, 8, 3, 8);
            this.rbCommandMoveMode.Name = "rbCommandMoveMode";
            this.rbCommandMoveMode.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.rbCommandMoveMode.SelectedIndex = -1;
            this.rbCommandMoveMode.Size = new System.Drawing.Size(278, 87);
            this.rbCommandMoveMode.TabIndex = 7;
            // 
            // btnZeroPosition
            // 
            this.btnZeroPosition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnZeroPosition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnZeroPosition.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnZeroPosition.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnZeroPosition.CustomForeColor = System.Drawing.Color.Black;
            this.btnZeroPosition.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnZeroPosition.ForeColor = System.Drawing.Color.Black;
            this.btnZeroPosition.ImageSize = new System.Drawing.Size(45, 45);
            this.btnZeroPosition.Location = new System.Drawing.Point(241, 179);
            this.btnZeroPosition.Name = "btnZeroPosition";
            this.btnZeroPosition.Size = new System.Drawing.Size(58, 35);
            this.btnZeroPosition.TabIndex = 8;
            this.btnZeroPosition.TabStop = false;
            this.btnZeroPosition.Text = "\"0";
            this.btnZeroPosition.UseVisualStyleBackColor = false;
            // 
            // gbSelectAxis
            // 
            this.gbSelectAxis.BackColor = System.Drawing.Color.White;
            this.gbSelectAxis.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gbSelectAxis.Location = new System.Drawing.Point(9, 25);
            this.gbSelectAxis.Name = "gbSelectAxis";
            this.gbSelectAxis.Size = new System.Drawing.Size(290, 150);
            this.gbSelectAxis.TabIndex = 0;
            this.gbSelectAxis.TabStop = false;
            this.gbSelectAxis.Text = "Select Axis";
            // 
            // lblAxisPositionCaption
            // 
            this.lblAxisPositionCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lblAxisPositionCaption.BorderWidth = 1;
            this.lblAxisPositionCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblAxisPositionCaption.Location = new System.Drawing.Point(9, 178);
            this.lblAxisPositionCaption.Name = "lblAxisPositionCaption";
            this.lblAxisPositionCaption.Size = new System.Drawing.Size(90, 35);
            this.lblAxisPositionCaption.TabIndex = 1;
            this.lblAxisPositionCaption.Text = "Position";
            this.lblAxisPositionCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblAxisPositionValue
            // 
            this.lblAxisPositionValue.BackColor = System.Drawing.Color.Black;
            this.lblAxisPositionValue.BorderColor = System.Drawing.Color.Black;
            this.lblAxisPositionValue.BorderWidth = 1;
            this.lblAxisPositionValue.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblAxisPositionValue.ForeColor = System.Drawing.Color.Lime;
            this.lblAxisPositionValue.Location = new System.Drawing.Point(103, 179);
            this.lblAxisPositionValue.Name = "lblAxisPositionValue";
            this.lblAxisPositionValue.Size = new System.Drawing.Size(136, 35);
            this.lblAxisPositionValue.TabIndex = 2;
            this.lblAxisPositionValue.Text = "000.000";
            this.lblAxisPositionValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // axisListBoxItemsView
            // 
            this.axisListBoxItemsView.BorderWidth = 2;
            this.axisListBoxItemsView.GroupName = "";
            this.axisListBoxItemsView.Location = new System.Drawing.Point(8, 18);
            this.axisListBoxItemsView.Name = "axisListBoxItemsView";
            this.axisListBoxItemsView.SelectedIndex = -1;
            this.axisListBoxItemsView.Size = new System.Drawing.Size(234, 124);
            this.axisListBoxItemsView.TabIndex = 0;
            this.axisListBoxItemsView.ItemSelected += new System.EventHandler<int>(this.OnAxisSelected);
            // 
            // gbAxisPositions
            // 
            this.gbAxisPositions.BackColor = System.Drawing.Color.White;
            this.gbAxisPositions.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbAxisPositions.Location = new System.Drawing.Point(314, 25);
            this.gbAxisPositions.Name = "gbAxisPositions";
            this.gbAxisPositions.Size = new System.Drawing.Size(290, 691);
            this.gbAxisPositions.TabIndex = 13;
            this.gbAxisPositions.TabStop = false;
            this.gbAxisPositions.Text = "Axis Positions";
            // 
            // CassetteLoadingElevatorUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 746);
            this.Controls.Add(this.gbMoveAxis);
            this.Controls.Add(this.gbDigitalIO);
            this.Controls.Add(this.gbPositionTeaching);
            this.Name = "CassetteLoadingElevatorUnit_Config";
            this.Text = "CassetteLoadingElevator Unit Configuration";
            this.gbTeachingMove.ResumeLayout(false);
            this.gbPositionTeaching.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.gbJogMove.ResumeLayout(false);
            this.gbJogMove.PerformLayout();
            this.gbCommandMove.ResumeLayout(false);
            this.gbDestinationMoveMode.ResumeLayout(false);
            this.gbDestinationMoveMode.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private void InitializeUI()
        {
            try
            {
                // 🚀 PropertyPosition을 사용하여 Position Item들을 listBoxItemsView에 설정
                SetAxisDefinitionsToAxisListBox();

                // 🚀 Position Item 선택 이벤트 연결
                SetupPositionItemSelectionEvent();

                InitializeRadioButtonView();
            }
            catch (Exception ex)
            {

            }
        }
    /// <summary>
    /// CassetteElevator + WaferTransferArm 의 AxisDefinition DisplayName 을 axisListBoxItemsView 에 설정
    /// </summary>
    private void SetAxisDefinitionsToAxisListBox()
        {
            try
            {
                // Equipment에서 CassetteLoadingElevator Unit 가져오기
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "CassetteLoadingElevator";

                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var cassetteUnit = unit as CassetteLoadingElevator;
                    if (cassetteUnit?.CassetteLoadingElevatorConfig?.PropertyPosition != null)
                    {
                        var propertyPosition = cassetteUnit.CassetteLoadingElevatorConfig.PropertyPosition;

                        // PropertyPosition에서 Position Title들을 추출하여 ListBox에 설정
                        var positionTitles = propertyPosition.GetPropertyTitles();

                        if (positionTitles.Length > 0)
                        {
                            // listBoxItemsView에 Position Title들 설정
                            positionlistBoxItemsView?.SetItems(positionTitles);

                            Console.WriteLine($"✅ PropertyPosition을 listBoxItemsView에 설정 완료: {positionTitles.Length}개 항목");
                            Console.WriteLine($"   설정된 항목들: {string.Join(", ", positionTitles)}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ PropertyPosition에 Position 항목이 없습니다.");
                            positionlistBoxItemsView?.SetItems();
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ CassetteElevator Config 또는 PropertyPosition을 찾을 수 없습니다.");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ '{UNIT_NAME}' Unit을 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ PropertyPosition 설정 중 오류: {ex.Message}");
            }
        }

        /// <summary>
        /// 🚀 Position Item 선택 이벤트 설정
        /// </summary>
        private void SetupPositionItemSelectionEvent()
        {
            if (positionlistBoxItemsView != null)
            {
                // 기존 이벤트 핸들러 제거 (중복 방지)
                positionlistBoxItemsView.ItemSelected -= OnPositionItemSelected;

                // 새 이벤트 핸들러 등록
                positionlistBoxItemsView.ItemSelected += OnPositionItemSelected;

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
                // Equipment에서 CassetteLoadingElevator Unit 가져오기
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "CassetteLoadingElevator";

                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var cassetteUnit = unit as CassetteLoadingElevator;
                    if (cassetteUnit?.CassetteLoadingElevatorConfig?.PropertyPosition != null)
                    {
                        var propertyPosition = cassetteUnit.CassetteLoadingElevatorConfig.PropertyPosition;
                        var positionTitles = propertyPosition.GetPropertyTitles();

                        if (selectedIndex >= 0 && selectedIndex < positionTitles.Length)
                        {
                            var selectedTitle = positionTitles[selectedIndex];
                            var selectedProperty = propertyPosition.GetPropertyByTitle(selectedTitle);

                            if (selectedProperty != null)
                            {
                                // 🚀 선택된 Position Property를 Editor(PropertyCollectionView)에 표시
                                var editorProperties = new PropertyCollection();

                                // Position (Abs, mm) 타이틀 추가
                                editorProperties.Add(new TitleOnlyProperty("Position (Abs, mm)"));

                                // 선택된 Position Property를 Editor용으로 복사
                                if (selectedProperty is DoubleProperty doubleProp)
                                {
                                    var editableProperty = new DoubleProperty(selectedTitle, doubleProp.Value);
                                    editorProperties.Add(editableProperty);
                                }
                                else
                                {
                                    editorProperties.Add(selectedProperty);
                                }

                                // PropertyCollectionView에 Editor 내용 설정
                                PositionpropertyCollectionView?.SetProperties(editorProperties);

                                Console.WriteLine($"📍 Position Item 선택: {selectedTitle}");
                                if (selectedProperty is DoubleProperty dp)
                                {
                                    Console.WriteLine($"   값: {dp.Value:F3} mm");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"⚠️ 선택된 Position Property를 찾을 수 없습니다: {selectedTitle}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ 잘못된 선택 인덱스: {selectedIndex}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ PropertyPosition을 찾을 수 없습니다.");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ '{UNIT_NAME}' Unit을 찾을 수 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position Item 선택 처리 중 오류: {ex.Message}");
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
           
        }  

        private void InitializeRadioButtonView()
        {
            try
            {
                rbTeachingMoveMode?.SetOptions(true, "Fine", "Coarse");
                rbCommandMoveMode?.SetOptions(false, "Absolute", "Relative");
                rbJogMoveMode?.SetOptions(false, "Continuous+", "Step");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RadioButtonView 오류: {ex.Message}");
            }
        }

        #region Save / Cancel


      #region Save / Cancel

        private void btnSave_Click(object sender, EventArgs e)
        {
            
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








        #endregion

        private GroupBox gbJogMove;
        private IndividualMenuButton individualMenuButton3;
        private IndividualMenuButton individualMenuButton2;
        private TextBox tbAxisMoveStepSizeValue;
        private CustomBorderLabel lblAxisMoveStepSizeCaption;
        private RadioButtonView rbJogMoveMode;
        private GroupBox gbAxisPositions;
    }
}