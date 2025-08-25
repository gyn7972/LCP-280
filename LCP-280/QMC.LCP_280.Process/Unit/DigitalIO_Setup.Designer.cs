using QMC.Common;
using QMC.Common.CustomControl;
using QMC.Common.Motions;
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
    /// <summary>
    /// Motion_Setup (refactored: minimal-change, safer wiring, clearer structure)
    /// </summary>
    partial class DigitalIO_Setup
    {
        // -------- Fields
        private readonly Equipment equipment = Equipment.Instance;

        // UI
        private ListBoxItemsView dioModuleListBoxItemsView;
        private IndividualMenuButton btn_Save_Setup_Ouput;
        private IndividualMenuButton btn_Save_Setup_Input;

        private System.ComponentModel.IContainer components = null;

        // Data
        private PropertyCollection _editorPropertiesConfig;
        private PropertyCollection _editorPropertiesSpeed;

        private IOPropertyCollectionView inputIOPropertyCollectionView;
        private IOPropertyCollectionView outputIOPropertyCollectionView;
        private PropertyCollectionView inputpropertyCollectionView;
        private PropertyCollectionView outputpropertyCollectionView;


        // Timers
        private Timer _axisPosTimer;

        // -------- Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_axisPosTimer != null)
                {
                    _axisPosTimer.Tick -= AxisPosTimer_Tick;
                    _axisPosTimer.Dispose();
                    _axisPosTimer = null;
                }

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Designer (trimmed & corrected)
        private void InitializeComponent()
        {
            this.btn_Save_Setup_Input = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Setup_Ouput = new QMC.Common.IndividualMenuButton();
            this.dioModuleListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.inputIOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputIOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.inputpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.outputpropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.SuspendLayout();
            // 
            // btn_Save_Setup_Input
            // 
            this.btn_Save_Setup_Input.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Input.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Input.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Input.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Input.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Input.Location = new System.Drawing.Point(672, 687);
            this.btn_Save_Setup_Input.Name = "btn_Save_Setup_Input";
            this.btn_Save_Setup_Input.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Input.TabIndex = 5;
            this.btn_Save_Setup_Input.TabStop = false;
            this.btn_Save_Setup_Input.Text = "Save";
            this.btn_Save_Setup_Input.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Input.Click += new System.EventHandler(this.btn_Save_Setup_Input_Property_Click);
            // 
            // btn_Save_Setup_Ouput
            // 
            this.btn_Save_Setup_Ouput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Ouput.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Ouput.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Ouput.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Ouput.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Ouput.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Ouput.Location = new System.Drawing.Point(1141, 687);
            this.btn_Save_Setup_Ouput.Name = "btn_Save_Setup_Ouput";
            this.btn_Save_Setup_Ouput.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Ouput.TabIndex = 4;
            this.btn_Save_Setup_Ouput.TabStop = false;
            this.btn_Save_Setup_Ouput.Text = "Save";
            this.btn_Save_Setup_Ouput.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Ouput.Click += new System.EventHandler(this.btn_Save_Setup_Output_Property_Click);
            // 
            // dioModuleListBoxItemsView
            // 
            this.dioModuleListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.dioModuleListBoxItemsView.BorderWidth = 2;
            this.dioModuleListBoxItemsView.GroupName = "DIO Module";
            this.dioModuleListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.dioModuleListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.dioModuleListBoxItemsView.Name = "dioModuleListBoxItemsView";
            this.dioModuleListBoxItemsView.SelectedIndex = -1;
            this.dioModuleListBoxItemsView.Size = new System.Drawing.Size(305, 722);
            this.dioModuleListBoxItemsView.TabIndex = 2;
            // 
            // inputIOPropertyCollectionView
            // 
            this.inputIOPropertyCollectionView.GroupName = "Digital Input";
            this.inputIOPropertyCollectionView.Location = new System.Drawing.Point(323, 12);
            this.inputIOPropertyCollectionView.Name = "inputIOPropertyCollectionView";
            this.inputIOPropertyCollectionView.Size = new System.Drawing.Size(460, 722);
            this.inputIOPropertyCollectionView.TabIndex = 11;
            // 
            // outputIOPropertyCollectionView
            // 
            this.outputIOPropertyCollectionView.GroupName = "Digital Output";
            this.outputIOPropertyCollectionView.Location = new System.Drawing.Point(792, 12);
            this.outputIOPropertyCollectionView.Name = "outputIOPropertyCollectionView";
            this.outputIOPropertyCollectionView.Size = new System.Drawing.Size(460, 722);
            this.outputIOPropertyCollectionView.TabIndex = 12;
            // 
            // inputpropertyCollectionView
            // 
            this.inputpropertyCollectionView.GroupName = "Property";
            this.inputpropertyCollectionView.Location = new System.Drawing.Point(333, 540);
            this.inputpropertyCollectionView.Name = "inputpropertyCollectionView";
            this.inputpropertyCollectionView.Size = new System.Drawing.Size(323, 187);
            this.inputpropertyCollectionView.TabIndex = 13;
            // 
            // outputpropertyCollectionView
            // 
            this.outputpropertyCollectionView.GroupName = "Property";
            this.outputpropertyCollectionView.Location = new System.Drawing.Point(802, 540);
            this.outputpropertyCollectionView.Name = "outputpropertyCollectionView";
            this.outputpropertyCollectionView.Size = new System.Drawing.Size(323, 187);
            this.outputpropertyCollectionView.TabIndex = 14;
            // 
            // DigitalIO_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.outputpropertyCollectionView);
            this.Controls.Add(this.btn_Save_Setup_Ouput);
            this.Controls.Add(this.btn_Save_Setup_Input);
            this.Controls.Add(this.inputpropertyCollectionView);
            this.Controls.Add(this.dioModuleListBoxItemsView);
            this.Controls.Add(this.inputIOPropertyCollectionView);
            this.Controls.Add(this.outputIOPropertyCollectionView);
            this.Name = "DigitalIO_Setup";
            this.Text = "Motion Setup";
            this.ResumeLayout(false);

        }
        #endregion

        // -------- Public/Init
        private void InitializeUI()
        {
            try
            {
                WireAxisSelectionEvent();
                BindAxisList();
                InitializeStatusTimer();     // 실제 위치 주기 갱신 (필요 시)
                InitializeRadioButtonView();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"InitializeUI error: {ex}");
            }
        }

        // -------- Binding
        /// <summary>
        /// Axis 목록 바인딩 (UNIT_NAME 기준)
        /// </summary>
        private void BindAxisList()
        {
            try
            {

            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"BindAxisList error: {ex}");
                dioModuleListBoxItemsView?.SetItems();
            }
        }

        // -------- Events
        private void WireAxisSelectionEvent()
        {
            if (dioModuleListBoxItemsView == null) return;

            // 중복 구독 방지
            dioModuleListBoxItemsView.ItemSelected -= OnPositionItemSelected;
            dioModuleListBoxItemsView.ItemSelected += OnPositionItemSelected;
        }

        /// <summary>
        /// Select Axis 리스트에서 항목 선택 시 속성 에디터 구성
        /// </summary>
        private void OnPositionItemSelected(object sender, int selectedIndex)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"OnPositionItemSelected error: {ex}");
            }
        }

        // -------- Builders
        private PropertyCollection BuildConfigProperties(MotionAxis axis)
        {
            return null;
        }
        private PropertyCollection BuildSpeedProperties(MotionAxis axis)
        {
            return null;
        }

        // -------- Axis status polling (optional)
        private void InitializeStatusTimer()
        {

        }

        private void AxisPosTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                UpdateAxisActualPosition();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"AxisPosTimer_Tick error: {ex}");
            }
        }

        // -------- Stubs / Overrides
        private void UpdateAxisActualPosition()
        {
            // TODO: 실제 위치/속도/상태 갱신 바인딩
            // positionVelocityPropertyCollectionView.SetProperties(...); 등
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            // TODO: 선택 포지션 이동 구현
        }

        private void InitializeRadioButtonView()
        {
            try
            {
                // TODO: 라디오버튼 초기화/바인딩
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"RadioButtonView 오류: {ex}");
            }
        }

        //private void btn_Save_Setup_Motion_Configuration_Click(object sender, EventArgs e)
        //{
        //    // TODO: _editorPropertiesConfig → axis.Setup 반영 & 저장
        //}

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // TODO: 변경 취소 로직
        }

        // --- Paint / Resize (keep)
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }
    }
}
