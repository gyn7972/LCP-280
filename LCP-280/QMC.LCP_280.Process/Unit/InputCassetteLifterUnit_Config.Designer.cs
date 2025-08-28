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
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.button_Test = new System.Windows.Forms.Button();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.gbJogMove = new System.Windows.Forms.GroupBox();
            this.tbAxisMoveStepSizeValue = new System.Windows.Forms.TextBox();
            this.gbCommandMove = new System.Windows.Forms.GroupBox();
            this.gbDestinationMoveMode = new System.Windows.Forms.GroupBox();
            this.tbAxisMovePositionValue = new System.Windows.Forms.TextBox();
            this.gbSelectAxis = new System.Windows.Forms.GroupBox();
            this.AxispositonListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btnNegativeJog = new QMC.Common.IndividualMenuButton();
            this.btnPositiveJog = new QMC.Common.IndividualMenuButton();
            this.lblAxisMoveStepSizeCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.rbJogMoveMode = new QMC.Common.RadioButtonView();
            this.btnDestinationMove = new QMC.Common.IndividualMenuButton();
            this.lblAxisMovePositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.rbCommandMoveMode = new QMC.Common.RadioButtonView();
            this.btnZeroPosition = new QMC.Common.IndividualMenuButton();
            this.lblAxisPositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lblAxisPositionValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.inputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.positionListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.btnMovePosition = new QMC.Common.IndividualMenuButton();
            this.rbTeachingMoveMode = new QMC.Common.RadioButtonView();
            this.positionPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
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
            this.Text = "InputCassetteLifter Unit Configuration";
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
                    var inputCassetteLifter = unit as InputCassetteLifter;
                    if (inputCassetteLifter?.InputCassetteLifterConfig?.PropertyPosition != null)
                    {
                        var propertyPosition = inputCassetteLifter.InputCassetteLifterConfig.PropertyPosition;

                        // PropertyPosition에서 Position Title들을 추출하여 ListBox에 설정
                        var positionTitles = propertyPosition.GetPropertyTitles();

                        if (positionTitles.Length > 0)
                        {
                            // listBoxItemsView에 Position Title들 설정
                            positionListBoxItemsView?.SetItems(positionTitles);

                            Console.WriteLine($"✅ PropertyPosition을 listBoxItemsView에 설정 완료: {positionTitles.Length}개 항목");
                            Console.WriteLine($"   설정된 항목들: {string.Join(", ", positionTitles)}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ PropertyPosition에 Position 항목이 없습니다.");
                            positionListBoxItemsView?.SetItems();
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
                // Equipment에서 CassetteLoadingElevator Unit 가져오기
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "InputCassetteLifter";

                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var inputCassetteLifter = unit as InputCassetteLifter;
                    if (inputCassetteLifter?.InputCassetteLifterConfig?.PropertyPosition != null)
                    {
                        var propertyPosition = inputCassetteLifter.InputCassetteLifterConfig.PropertyPosition;
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
                                positionPropertyCollectionView?.SetProperties(editorProperties);

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

        private ListBoxItemsView AxispositonListBoxItemsView;
        private Button button_Test;
    }
}