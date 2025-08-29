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
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    partial class InputCassetteLifterUnit_Config
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private PropertyCollectionView positionPropertyCollectionView;
        private IOPropertyCollectionView inputPropertyCollectionView;
        private IOPropertyCollectionView outputPropertyCollectionView;
        private ListBoxItemsView positionListBoxItemsView;

        // Axis 목록 (추가)
        private ListBoxItemsView axisListBoxItemsView;

        private IndividualMenuButton btnSave;
        private IndividualMenuButton btnCancel;
        private IndividualMenuButton btnMovePosition;
        private IndividualMenuButton btnZeroPosition;
        private IndividualMenuButton btnDestinationMove;
        private IndividualMenuButton btnNegativeJog;
        private IndividualMenuButton btnPositiveJog;

        private RadioButtonView rbTeachingMoveMode;
        private RadioButtonView rbCommandMoveMode;
        private RadioButtonView rbJogMoveMode;

        private CustomBorderLabel lblAxisPositionValue;
        private CustomBorderLabel lblAxisPositionCaption;
        private CustomBorderLabel lblAxisMovePositionCaption;
        private CustomBorderLabel lblAxisMoveStepSizeCaption;
        
        private TextBox tbAxisMovePositionValue;
        private TextBox tbAxisMoveStepSizeValue;

        private GroupBox gbTeachingMove;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbCommandMove;
        private GroupBox gbJogMove;
        private GroupBox gbMoveAxis;
        private GroupBox gbSelectAxis;
        private GroupBox gbDestinationMoveMode;

        private System.ComponentModel.IContainer components = null;

        // Actual Position 주기 업데이트 타이머
        private Timer _axisPosTimer;

        // === Digital IO 표시용 내부 구조 추가 (기존 코드 유지) ===
        private struct _IoRef { public string Module; public string Disp; public PropertyState Prop; }
        private readonly List<_IoRef> _ioInputs = new List<_IoRef>();
        // 출력 사용 안함
        //private readonly List<_IoRef> _ioOutputs = new List<_IoRef>();
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
            this.gbTeachingMove = new System.Windows.Forms.GroupBox();
            this.btnMovePosition = new QMC.Common.IndividualMenuButton();
            this.rbTeachingMoveMode = new QMC.Common.RadioButtonView();
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.positionPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.inputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.gbJogMove = new System.Windows.Forms.GroupBox();
            this.btnNegativeJog = new QMC.Common.IndividualMenuButton();
            this.btnPositiveJog = new QMC.Common.IndividualMenuButton();
            this.tbAxisMoveStepSizeValue = new System.Windows.Forms.TextBox();
            this.lblAxisMoveStepSizeCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.rbJogMoveMode = new QMC.Common.RadioButtonView();
            this.gbCommandMove = new System.Windows.Forms.GroupBox();
            this.gbDestinationMoveMode = new System.Windows.Forms.GroupBox();
            this.btnDestinationMove = new QMC.Common.IndividualMenuButton();
            this.lblAxisMovePositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.tbAxisMovePositionValue = new System.Windows.Forms.TextBox();
            this.rbCommandMoveMode = new QMC.Common.RadioButtonView();
            this.btnZeroPosition = new QMC.Common.IndividualMenuButton();
            this.gbSelectAxis = new System.Windows.Forms.GroupBox();
            this.lblAxisPositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lblAxisPositionValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.AxispositonListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.button_Test = new System.Windows.Forms.Button();
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
            this.gbPositionTeaching.Controls.Add(this.button_Test);
            this.gbPositionTeaching.Controls.Add(this.positionListBoxItemsView);
            this.gbPositionTeaching.Controls.Add(this.btnSave);
            this.gbPositionTeaching.Controls.Add(this.btnCancel);
            this.gbPositionTeaching.Controls.Add(this.gbTeachingMove);
            this.gbPositionTeaching.Controls.Add(this.positionPropertyCollectionView);
            this.gbPositionTeaching.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbPositionTeaching.Location = new System.Drawing.Point(9, 12);
            this.gbPositionTeaching.Name = "gbPositionTeaching";
            this.gbPositionTeaching.Size = new System.Drawing.Size(613, 361);
            this.gbPositionTeaching.TabIndex = 8;
            this.gbPositionTeaching.TabStop = false;
            this.gbPositionTeaching.Text = "Position Teaching";
            // 
            // positionListBoxItemsView
            // 
            this.positionListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.positionListBoxItemsView.BorderWidth = 2;
            this.positionListBoxItemsView.GroupName = "Position Items";
            this.positionListBoxItemsView.Location = new System.Drawing.Point(9, 34);
            this.positionListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.positionListBoxItemsView.Name = "positionListBoxItemsView";
            this.positionListBoxItemsView.SelectedIndex = -1;
            this.positionListBoxItemsView.Size = new System.Drawing.Size(257, 313);
            this.positionListBoxItemsView.TabIndex = 2;
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
            // positionPropertyCollectionView
            // 
            this.positionPropertyCollectionView.GroupName = "Editor";
            this.positionPropertyCollectionView.Location = new System.Drawing.Point(279, 34);
            this.positionPropertyCollectionView.Margin = new System.Windows.Forms.Padding(4);
            this.positionPropertyCollectionView.Name = "positionPropertyCollectionView";
            this.positionPropertyCollectionView.Size = new System.Drawing.Size(326, 168);
            this.positionPropertyCollectionView.TabIndex = 0;
            this.positionPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // gbDigitalIO
            // 
            this.gbDigitalIO.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.gbDigitalIO.Controls.Add(this.inputPropertyCollectionView);
            this.gbDigitalIO.Controls.Add(this.outputPropertyCollectionView);
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDigitalIO.Location = new System.Drawing.Point(9, 382);
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
            this.gbMoveAxis.Controls.Add(this.gbJogMove);
            this.gbMoveAxis.Controls.Add(this.gbCommandMove);
            this.gbMoveAxis.Controls.Add(this.btnZeroPosition);
            this.gbMoveAxis.Controls.Add(this.gbSelectAxis);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionCaption);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionValue);
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(643, 12);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.gbMoveAxis.Size = new System.Drawing.Size(300, 724);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";
            // 
            // gbJogMove
            // 
            this.gbJogMove.BackColor = System.Drawing.Color.White;
            this.gbJogMove.Controls.Add(this.btnNegativeJog);
            this.gbJogMove.Controls.Add(this.btnPositiveJog);
            this.gbJogMove.Controls.Add(this.tbAxisMoveStepSizeValue);
            this.gbJogMove.Controls.Add(this.lblAxisMoveStepSizeCaption);
            this.gbJogMove.Controls.Add(this.rbJogMoveMode);
            this.gbJogMove.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbJogMove.Location = new System.Drawing.Point(11, 420);
            this.gbJogMove.Name = "gbJogMove";
            this.gbJogMove.Size = new System.Drawing.Size(281, 296);
            this.gbJogMove.TabIndex = 10;
            this.gbJogMove.TabStop = false;
            this.gbJogMove.Text = "Jog Move";
            // 
            // btnNegativeJog
            // 
            this.btnNegativeJog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnNegativeJog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnNegativeJog.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnNegativeJog.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnNegativeJog.CustomForeColor = System.Drawing.Color.Black;
            this.btnNegativeJog.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnNegativeJog.ForeColor = System.Drawing.Color.Black;
            this.btnNegativeJog.ImageSize = new System.Drawing.Size(45, 45);
            this.btnNegativeJog.Location = new System.Drawing.Point(6, 201);
            this.btnNegativeJog.Name = "btnNegativeJog";
            this.btnNegativeJog.Size = new System.Drawing.Size(111, 50);
            this.btnNegativeJog.TabIndex = 12;
            this.btnNegativeJog.TabStop = false;
            this.btnNegativeJog.Text = "- Jog";
            this.btnNegativeJog.UseVisualStyleBackColor = false;
            // 
            // btnPositiveJog
            // 
            this.btnPositiveJog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPositiveJog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnPositiveJog.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnPositiveJog.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnPositiveJog.CustomForeColor = System.Drawing.Color.Black;
            this.btnPositiveJog.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnPositiveJog.ForeColor = System.Drawing.Color.Black;
            this.btnPositiveJog.ImageSize = new System.Drawing.Size(45, 45);
            this.btnPositiveJog.Location = new System.Drawing.Point(164, 201);
            this.btnPositiveJog.Name = "btnPositiveJog";
            this.btnPositiveJog.Size = new System.Drawing.Size(111, 50);
            this.btnPositiveJog.TabIndex = 11;
            this.btnPositiveJog.TabStop = false;
            this.btnPositiveJog.Text = "+ Jog";
            this.btnPositiveJog.UseVisualStyleBackColor = false;
            // 
            // tbAxisMoveStepSizeValue
            // 
            this.tbAxisMoveStepSizeValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbAxisMoveStepSizeValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.tbAxisMoveStepSizeValue.Location = new System.Drawing.Point(185, 120);
            this.tbAxisMoveStepSizeValue.Multiline = true;
            this.tbAxisMoveStepSizeValue.Name = "tbAxisMoveStepSizeValue";
            this.tbAxisMoveStepSizeValue.Size = new System.Drawing.Size(90, 35);
            this.tbAxisMoveStepSizeValue.TabIndex = 10;
            this.tbAxisMoveStepSizeValue.Text = "000.000";
            this.tbAxisMoveStepSizeValue.WordWrap = false;
            // 
            // lblAxisMoveStepSizeCaption
            // 
            this.lblAxisMoveStepSizeCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lblAxisMoveStepSizeCaption.BorderWidth = 1;
            this.lblAxisMoveStepSizeCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblAxisMoveStepSizeCaption.Location = new System.Drawing.Point(90, 120);
            this.lblAxisMoveStepSizeCaption.Name = "lblAxisMoveStepSizeCaption";
            this.lblAxisMoveStepSizeCaption.Size = new System.Drawing.Size(90, 35);
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
            this.rbJogMoveMode.Size = new System.Drawing.Size(269, 87);
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
            this.gbCommandMove.Size = new System.Drawing.Size(283, 191);
            this.gbCommandMove.TabIndex = 8;
            this.gbCommandMove.TabStop = false;
            this.gbCommandMove.Text = "Command Move";
            // 
            // gbDestinationMoveMode
            // 
            this.gbDestinationMoveMode.BackColor = System.Drawing.Color.White;
            this.gbDestinationMoveMode.Controls.Add(this.btnDestinationMove);
            this.gbDestinationMoveMode.Controls.Add(this.lblAxisMovePositionCaption);
            this.gbDestinationMoveMode.Controls.Add(this.tbAxisMovePositionValue);
            this.gbDestinationMoveMode.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDestinationMoveMode.Location = new System.Drawing.Point(6, 115);
            this.gbDestinationMoveMode.Name = "gbDestinationMoveMode";
            this.gbDestinationMoveMode.Size = new System.Drawing.Size(271, 70);
            this.gbDestinationMoveMode.TabIndex = 9;
            this.gbDestinationMoveMode.TabStop = false;
            this.gbDestinationMoveMode.Text = "Destination Position";
            // 
            // btnDestinationMove
            // 
            this.btnDestinationMove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDestinationMove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnDestinationMove.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btnDestinationMove.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnDestinationMove.CustomForeColor = System.Drawing.Color.Black;
            this.btnDestinationMove.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnDestinationMove.ForeColor = System.Drawing.Color.Black;
            this.btnDestinationMove.ImageSize = new System.Drawing.Size(45, 45);
            this.btnDestinationMove.Location = new System.Drawing.Point(183, 25);
            this.btnDestinationMove.Name = "btnDestinationMove";
            this.btnDestinationMove.Size = new System.Drawing.Size(84, 35);
            this.btnDestinationMove.TabIndex = 11;
            this.btnDestinationMove.TabStop = false;
            this.btnDestinationMove.Text = "Move";
            this.btnDestinationMove.UseVisualStyleBackColor = false;
            // 
            // lblAxisMovePositionCaption
            // 
            this.lblAxisMovePositionCaption.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(208)))), ((int)(((byte)(206)))), ((int)(((byte)(206)))));
            this.lblAxisMovePositionCaption.BorderWidth = 1;
            this.lblAxisMovePositionCaption.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this.lblAxisMovePositionCaption.Location = new System.Drawing.Point(4, 24);
            this.lblAxisMovePositionCaption.Name = "lblAxisMovePositionCaption";
            this.lblAxisMovePositionCaption.Size = new System.Drawing.Size(80, 35);
            this.lblAxisMovePositionCaption.TabIndex = 9;
            this.lblAxisMovePositionCaption.Text = "Move Pos.";
            this.lblAxisMovePositionCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tbAxisMovePositionValue
            // 
            this.tbAxisMovePositionValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbAxisMovePositionValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.tbAxisMovePositionValue.Location = new System.Drawing.Point(89, 25);
            this.tbAxisMovePositionValue.Multiline = true;
            this.tbAxisMovePositionValue.Name = "tbAxisMovePositionValue";
            this.tbAxisMovePositionValue.Size = new System.Drawing.Size(90, 35);
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
            this.rbCommandMoveMode.Size = new System.Drawing.Size(271, 87);
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
            this.btnZeroPosition.Location = new System.Drawing.Point(237, 179);
            this.btnZeroPosition.Name = "btnZeroPosition";
            this.btnZeroPosition.Size = new System.Drawing.Size(55, 35);
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
            this.gbSelectAxis.Size = new System.Drawing.Size(283, 150);
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
            this.lblAxisPositionValue.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblAxisPositionValue.ForeColor = System.Drawing.Color.Lime;
            this.lblAxisPositionValue.Location = new System.Drawing.Point(103, 179);
            this.lblAxisPositionValue.Name = "lblAxisPositionValue";
            this.lblAxisPositionValue.Size = new System.Drawing.Size(130, 35);
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
            // AxispositonListBoxItemsView
            // 
            this.AxispositonListBoxItemsView.BorderWidth = 2;
            this.AxispositonListBoxItemsView.GroupName = "Axis Positions";
            this.AxispositonListBoxItemsView.Location = new System.Drawing.Point(949, 12);
            this.AxispositonListBoxItemsView.Name = "AxispositonListBoxItemsView";
            this.AxispositonListBoxItemsView.SelectedIndex = -1;
            this.AxispositonListBoxItemsView.Size = new System.Drawing.Size(303, 724);
            this.AxispositonListBoxItemsView.TabIndex = 11;
            // 
            // button_Test
            // 
            this.button_Test.Location = new System.Drawing.Point(530, 15);
            this.button_Test.Name = "button_Test";
            this.button_Test.Size = new System.Drawing.Size(75, 23);
            this.button_Test.TabIndex = 12;
            this.button_Test.Text = "Test";
            this.button_Test.UseVisualStyleBackColor = true;
            this.button_Test.Click += new System.EventHandler(this.button_Test_Click);
            // 
            // InputCassetteLifterUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.AxispositonListBoxItemsView);
            this.Controls.Add(this.gbMoveAxis);
            this.Controls.Add(this.gbDigitalIO);
            this.Controls.Add(this.gbPositionTeaching);
            this.Name = "InputCassetteLifterUnit_Config";
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
                InitializeDigitalIO();            // ★ Digital IO 초기화 추가
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitializeUI error: " + ex.Message);
            }
        }

        // ===== Digital IO 초기화 (Cassette Lifter 관련 IO 자동 필터) =====
        private void InitializeDigitalIO()
        {
            try
            {
                if (inputPropertyCollectionView == null)
                    return;

                var eq = Equipment.Instance;
                var scan = eq?.DioScan;
                var unitIO = eq?.UnitIO;
                if (scan == null || unitIO == null)
                {
                    inputPropertyCollectionView.SetProperties(new PropertyCollection());
                    return;
                }

                _ioInputs.Clear();

                var hardInputs = new[]
                {
                    new { No = 1, Name = "WAFER LIFTER CASSETTE CHECK 0", Disp = "X016" },
                    new { No = 2, Name = "WAFER LIFTER CASSETTE CHECK 1", Disp = "X017" },
                    new { No = 3, Name = "WAFER LIFTER RING JUT CHECK",   Disp = "X018" },
                    new { No = 4, Name = "WAFER MAPPING",                 Disp = "X019" }
                };

                // 모듈명 매핑
                Func<string, Tuple<string,string>> resolve = disp =>
                {
                    if (unitIO?.Modules == null) return new Tuple<string,string>(null, disp);
                    foreach (var m in unitIO.Modules)
                    {
                        if (m?.Inputs == null) continue;
                        foreach (var ch in m.Inputs)
                        {
                            if (string.Equals(ch.DisplayNo, disp, StringComparison.OrdinalIgnoreCase))
                                return new Tuple<string,string>(m.ModuleName, ch.DisplayNo);
                        }
                    }
                    return new Tuple<string,string>(null, disp);
                };

                var pc = new PropertyCollection { ShowNoColumn = true, IsInputParameter = false };
                pc.Add(new TitleOnlyProperty("No", "Name", "State"));

                foreach (var item in hardInputs)
                {
                    var map = resolve(item.Disp);
                    bool cur = false;
                    if (map.Item1 != null) scan.TryGetInput(map.Item1, map.Item2, out cur);
                    // PropertyState(번호, 표시 문자열, 초기 상태)
                    string nameCell = item.Disp + " " + item.Name; // 첫 토큰이 키(Xnnn)
                    var ps = new PropertyState(item.No.ToString(), nameCell, cur);
                    pc.Add(ps);
                    _ioInputs.Add(new _IoRef { Module = map.Item1, Disp = map.Item2, Prop = ps });
                }

                inputPropertyCollectionView.SetProperties(pc);
                // 출력 영역 비움
                outputPropertyCollectionView?.SetProperties(new PropertyCollection());

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
                        inputPropertyCollectionView.SetStateByKey(disp, value);
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
                // Equipment에서 CassetteLoadingElevator Unit 가져오기
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "InputCassetteLifter";

                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var inputCassetteLifter = unit as InputCassetteLifter;
                    // TeachingPositions 멤버를 직접 사용하여 Position 이름 리스트 추출
                    if (inputCassetteLifter?.TeachingPositions != null && inputCassetteLifter.TeachingPositions.Count > 0)
                    {
                        var positionNames = inputCassetteLifter.TeachingPositions.Select(tp => tp.Name).ToArray();
                        positionListBoxItemsView?.SetItems(positionNames);
                        Console.WriteLine($"✅ TeachingPositions를 listBoxItemsView에 설정 완료: {positionNames.Length}개 항목");
                        Console.WriteLine($"   설정된 항목들: {string.Join(", ", positionNames)}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ TeachingPositions에 Position 항목이 없습니다.");
                        positionListBoxItemsView?.SetItems();
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
            if (positionListBoxItemsView != null)
            {
                // 기존 이벤트 핸들러 제거 (중복 방지)
                positionListBoxItemsView.ItemSelected -= OnPositionItemSelected;

                // 새 이벤트 핸들러 등록
                positionListBoxItemsView.ItemSelected += OnPositionItemSelected;

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

                // ★ 선택된 TeachingPosition의 축 이름들을 JogControl에 전달하여 필터링 표시
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "InputCassetteLifter";
                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var lifter = unit as InputCassetteLifter;
                    if (lifter != null && selectedIndex >= 0 && selectedIndex < lifter.InputCassetteLifterConfig.TeachingPositions.Count)
                    {
                        var tp = lifter.InputCassetteLifterConfig.TeachingPositions[selectedIndex];
                        if (jogControl != null && tp != null && tp.AxisPositions != null)
                        {
                            jogControl.SetTeachingAxisList(tp.AxisPositions.Keys);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position Item 선택 처리 중 오류: {ex.Message}");
            }
        }

        private void ShowTeachingPositionInPropertyCollectionView(int selectedIndex)
        {
            // Equipment에서 InputCassetteLifter Unit 가져오기
            var equipment = Equipment.Instance;
            const string UNIT_NAME = "InputCassetteLifter";
            if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
            {
                var inputCassetteLifter = unit as InputCassetteLifter;
                var config = inputCassetteLifter?.InputCassetteLifterConfig;
                if (config?.TeachingPositions != null && selectedIndex >= 0 && selectedIndex < config.TeachingPositions.Count)
                {
                    var tp = config.TeachingPositions[selectedIndex];
                    var editorProperties = new PropertyCollection();
                    editorProperties.Add(new TitleOnlyProperty($"Teaching Position: {tp.Name}"));
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
                    positionPropertyCollectionView?.SetProperties(editorProperties);
                }
            }
        }

        private void InitializeTeachingPositionList()
        {
            // Equipment에서 InputCassetteLifter Unit 가져오기
            var equipment = Equipment.Instance;
            const string UNIT_NAME = "InputCassetteLifter";
            if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
            {
                var inputCassetteLifter = unit as InputCassetteLifter;
                var config = inputCassetteLifter?.InputCassetteLifterConfig;
                if (config?.TeachingPositions != null)
                {
                    var positionNames = config.TeachingPositions.Select(tp => tp.Name).ToArray();
                    positionListBoxItemsView.SetItems(positionNames);
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
                const string UNIT_NAME = "InputCassetteLifter";
                var equipment = Equipment.Instance;
                if (!equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var lifter = unit as InputCassetteLifter;
                if (lifter == null)
                {
                    MessageBox.Show("Unit 형식 오류", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 선택된 Teaching Position 인덱스
                int selIndex = -1;
                try
                {
                    var pi = positionListBoxItemsView.GetType().GetProperty("SelectedIndex");
                    if (pi != null)
                    {
                        object val = pi.GetValue(positionListBoxItemsView, null);
                        if (val is int) selIndex = (int)val;
                    }
                }
                catch { selIndex = -1; }

                if (selIndex < 0 || selIndex >= lifter.InputCassetteLifterConfig.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var tp = lifter.InputCassetteLifterConfig.TeachingPositions[selIndex];

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
                    if (axis == null && lifter.Axes.TryGetValue(axisKey, out var directAxis)) axis = directAxis;
                    if (axis == null)
                    {
                        // Name 매칭 시도
                        foreach (var aPair in lifter.Axes)
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
                    if (axis == null && lifter.Axes.TryGetValue(kv.Key, out var directAxis)) axis = directAxis;
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
            try
            {
                const string UNIT_NAME = "InputCassetteLifter";
                var equipment = Equipment.Instance;
                if (!equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    MessageBox.Show("Unit을 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var lifter = unit as InputCassetteLifter;
                if (lifter == null)
                {
                    MessageBox.Show("Unit 형식이 올바르지 않습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 현재 선택된 Teaching Position 인덱스
                int selIndex = -1;
                try
                {
                    // ListBoxItemsView에 SelectedIndex 프로퍼티가 있다고 가정
                    var pi = positionListBoxItemsView.GetType().GetProperty("SelectedIndex");
                    if (pi != null)
                    {
                        object val = pi.GetValue(positionListBoxItemsView, null);
                        if (val is int) selIndex = (int)val;
                    }
                }
                catch { selIndex = -1; }

                if (selIndex < 0 || selIndex >= lifter.TeachingPositions.Count)
                {
                    MessageBox.Show("선택된 Teaching Position이 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // 에디터(PropertyCollectionView)에 입력된 값 적용(안전 차원)
                positionPropertyCollectionView?.Apply();

                var props = positionPropertyCollectionView?.GetCurrentProperties();
                if (props == null || props.Count == 0)
                {
                    MessageBox.Show("편집할 데이터가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var target = lifter.TeachingPositions[selIndex];

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
                lifter.InputCassetteLifterConfig.SetTeachingPosition(new TeachingPosition(target.Name, new Dictionary<string, double>(target.AxisPositions), target.Description) { ExtraInfo = new Dictionary<string, object>(target.ExtraInfo) });

                // 저장 후 재로드 & 재바인딩 (선택적으로 최신 반영)
                lifter.InputCassetteLifterConfig.LoadAndBindAxes(Equipment.Instance.AxisManager);
                lifter.TeachingPositions.Clear();
                foreach (var tp in lifter.InputCassetteLifterConfig.TeachingPositions)
                    lifter.TeachingPositions.Add(tp);

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

        #endregion

        private ListBoxItemsView AxispositonListBoxItemsView;
        private Button button_Test;
    }
}