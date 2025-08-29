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

        private RadioButtonView rbTeachingMoveMode;

        private GroupBox gbTeachingMove;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;
        private GroupBox gbMoveAxis;

        private JogControl jogControl;

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
            this.button_Test = new System.Windows.Forms.Button();
            this.positionListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.positionPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.inputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.AxispositonListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.gbTeachingMove.SuspendLayout();
            this.gbPositionTeaching.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
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
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(643, 12);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.gbMoveAxis.Size = new System.Drawing.Size(300, 724);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";

            // ⬇️ [추가 시작] JogControl을 그룹박스 안에 Dock=Fill 로 부착
            this.jogControl = new QMC.LCP_280.Process.Unit.JogControl();
            this.jogControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jogControl.Margin = new System.Windows.Forms.Padding(0);
            this.jogControl.Name = "jogControl";
            this.gbMoveAxis.Controls.Add(this.jogControl);

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