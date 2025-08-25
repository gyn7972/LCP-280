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
    partial class Motion_Setup
    {

        // Axis 목록 (추가)
        private ListBoxItemsView axisListBoxItemsView;
        private GroupBox gbAxisProperty;
        private GroupBox gbAxisPositions;

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
            this.gbAxisProperty = new System.Windows.Forms.GroupBox();
            this.speedListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.configurationListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.gbAxisPositions = new System.Windows.Forms.GroupBox();
            this.motorStateIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.motorIoPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.positionVelocityPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.selectAxisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.gbAxisProperty.SuspendLayout();
            this.gbAxisPositions.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbAxisProperty
            // 
            this.gbAxisProperty.BackColor = System.Drawing.Color.White;
            this.gbAxisProperty.Controls.Add(this.speedListBoxItemsView);
            this.gbAxisProperty.Controls.Add(this.configurationListBoxItemsView);
            this.gbAxisProperty.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbAxisProperty.Location = new System.Drawing.Point(643, 12);
            this.gbAxisProperty.Name = "gbAxisProperty";
            this.gbAxisProperty.Size = new System.Drawing.Size(609, 722);
            this.gbAxisProperty.TabIndex = 10;
            this.gbAxisProperty.TabStop = false;
            this.gbAxisProperty.Text = "Axis Property";
            // 
            // speedListBoxItemsView
            // 
            this.speedListBoxItemsView.BorderWidth = 2;
            this.speedListBoxItemsView.GroupName = "Speed";
            this.speedListBoxItemsView.Location = new System.Drawing.Point(313, 27);
            this.speedListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.speedListBoxItemsView.Name = "speedListBoxItemsView";
            this.speedListBoxItemsView.SelectedIndex = -1;
            this.speedListBoxItemsView.Size = new System.Drawing.Size(290, 688);
            this.speedListBoxItemsView.TabIndex = 1;
            // 
            // configurationListBoxItemsView
            // 
            this.configurationListBoxItemsView.BorderWidth = 2;
            this.configurationListBoxItemsView.GroupName = "Configuration";
            this.configurationListBoxItemsView.Location = new System.Drawing.Point(6, 25);
            this.configurationListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.configurationListBoxItemsView.Name = "configurationListBoxItemsView";
            this.configurationListBoxItemsView.SelectedIndex = -1;
            this.configurationListBoxItemsView.Size = new System.Drawing.Size(290, 690);
            this.configurationListBoxItemsView.TabIndex = 0;
            // 
            // gbAxisPositions
            // 
            this.gbAxisPositions.BackColor = System.Drawing.Color.White;
            this.gbAxisPositions.Controls.Add(this.motorStateIoPropertyCollectionView);
            this.gbAxisPositions.Controls.Add(this.motorIoPropertyCollectionView);
            this.gbAxisPositions.Controls.Add(this.positionVelocityPropertyCollectionView);
            this.gbAxisPositions.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbAxisPositions.Location = new System.Drawing.Point(326, 12);
            this.gbAxisPositions.Name = "gbAxisPositions";
            this.gbAxisPositions.Size = new System.Drawing.Size(305, 722);
            this.gbAxisPositions.TabIndex = 13;
            this.gbAxisPositions.TabStop = false;
            this.gbAxisPositions.Text = "Axis Status";
            // 
            // motorStateIoPropertyCollectionView
            // 
            this.motorStateIoPropertyCollectionView.GroupName = "Motor State";
            this.motorStateIoPropertyCollectionView.Location = new System.Drawing.Point(6, 495);
            this.motorStateIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.motorStateIoPropertyCollectionView.Name = "motorStateIoPropertyCollectionView";
            this.motorStateIoPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
            this.motorStateIoPropertyCollectionView.TabIndex = 2;
            // 
            // motorIoPropertyCollectionView
            // 
            this.motorIoPropertyCollectionView.GroupName = "Motor I/O";
            this.motorIoPropertyCollectionView.Location = new System.Drawing.Point(6, 265);
            this.motorIoPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.motorIoPropertyCollectionView.Name = "motorIoPropertyCollectionView";
            this.motorIoPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
            this.motorIoPropertyCollectionView.TabIndex = 1;
            // 
            // positionVelocityPropertyCollectionView
            // 
            this.positionVelocityPropertyCollectionView.GroupName = "Position & Velocity";
            this.positionVelocityPropertyCollectionView.Location = new System.Drawing.Point(6, 25);
            this.positionVelocityPropertyCollectionView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.positionVelocityPropertyCollectionView.Name = "positionVelocityPropertyCollectionView";
            this.positionVelocityPropertyCollectionView.Size = new System.Drawing.Size(293, 220);
            this.positionVelocityPropertyCollectionView.TabIndex = 0;
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
            // selectAxisListBoxItemsView
            // 
            this.selectAxisListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.selectAxisListBoxItemsView.BorderWidth = 2;
            this.selectAxisListBoxItemsView.GroupName = "Select Axis";
            this.selectAxisListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.selectAxisListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.selectAxisListBoxItemsView.Name = "selectAxisListBoxItemsView";
            this.selectAxisListBoxItemsView.SelectedIndex = -1;
            this.selectAxisListBoxItemsView.Size = new System.Drawing.Size(305, 722);
            this.selectAxisListBoxItemsView.TabIndex = 2;
            // 
            // Motion_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.gbAxisPositions);
            this.Controls.Add(this.gbAxisProperty);
            this.Controls.Add(this.selectAxisListBoxItemsView);
            this.Name = "Motion_Setup";
            this.Text = "Motion Setup";
            this.gbAxisProperty.ResumeLayout(false);
            this.gbAxisPositions.ResumeLayout(false);
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
                            selectAxisListBoxItemsView?.SetItems(positionTitles);

                            Console.WriteLine($"✅ PropertyPosition을 listBoxItemsView에 설정 완료: {positionTitles.Length}개 항목");
                            Console.WriteLine($"   설정된 항목들: {string.Join(", ", positionTitles)}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ PropertyPosition에 Position 항목이 없습니다.");
                            selectAxisListBoxItemsView?.SetItems();
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
            if (selectAxisListBoxItemsView != null)
            {
                // 기존 이벤트 핸들러 제거 (중복 방지)
                selectAxisListBoxItemsView.ItemSelected -= OnPositionItemSelected;

                // 새 이벤트 핸들러 등록
                selectAxisListBoxItemsView.ItemSelected += OnPositionItemSelected;

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
                                //selectAxisropertyCollectionView?.SetProperties(editorProperties);

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

        private ListBoxItemsView selectAxisListBoxItemsView;
        private IOPropertyCollectionView motorStateIoPropertyCollectionView;
        private IOPropertyCollectionView motorIoPropertyCollectionView;
        private PropertyCollectionView positionVelocityPropertyCollectionView;
        private ListBoxItemsView speedListBoxItemsView;
        private ListBoxItemsView configurationListBoxItemsView;
    }
}