using QMC.Common;
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
        private PropertyCollectionView propertyCollectionView;
        private IOPropertyCollectionView IOPropertyCollectionView;
        private ListBoxItemsView listBoxItemsView;
        
        // 🚀 Position Editor 버튼들 추가
        private Button btnSave;
        private Button btnCancel;
        
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
            this.propertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.IOPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.listBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // propertyCollectionView
            // 
            this.propertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.propertyCollectionView.GroupName = "Editor";
            this.propertyCollectionView.Location = new System.Drawing.Point(257, 22);
            this.propertyCollectionView.Name = "propertyCollectionView";
            this.propertyCollectionView.Size = new System.Drawing.Size(266, 200);
            this.propertyCollectionView.TabIndex = 0;
            this.propertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // IOPropertyCollectionView
            // 
            this.IOPropertyCollectionView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.IOPropertyCollectionView.GroupName = "Recipe Settings";
            this.IOPropertyCollectionView.Location = new System.Drawing.Point(899, 99);
            this.IOPropertyCollectionView.Name = "IOPropertyCollectionView";
            this.IOPropertyCollectionView.Size = new System.Drawing.Size(280, 610);
            this.IOPropertyCollectionView.TabIndex = 1;
            this.IOPropertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            // 
            // listBoxItemsView
            // 
            this.listBoxItemsView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxItemsView.BorderWidth = 2;
            this.listBoxItemsView.GroupName = "Position Items";
            this.listBoxItemsView.Location = new System.Drawing.Point(12, 22);
            this.listBoxItemsView.Name = "listBoxItemsView";
            this.listBoxItemsView.SelectedIndex = -1;
            this.listBoxItemsView.Size = new System.Drawing.Size(200, 610);
            this.listBoxItemsView.TabIndex = 2;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(257, 240);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 30);
            this.btnSave.TabIndex = 3;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(437, 240);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CassetteLoadingElevatorUnit_Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1191, 740);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.propertyCollectionView);
            this.Controls.Add(this.IOPropertyCollectionView);
            this.Controls.Add(this.listBoxItemsView);
            this.Name = "CassetteLoadingElevatorUnit_Config";
            this.Text = "CassetteLoadingElevator Unit Configuration";
            this.ResumeLayout(false);

        }

        #endregion

        #region UI 초기화 및 Position Item 관리

        private void InitializeUI()
        {
            try
            {
                // 🚀 PropertyPosition을 사용하여 Position Item들을 listBoxItemsView에 설정
                SetPropertyPositionToListBox();
                
                // 🚀 Position Item 선택 이벤트 연결
                SetupPositionItemSelectionEvent();
            }
            catch (Exception ex)
            {
                // 디버그 모드에서만 오류 표시
#if DEBUG
                MessageBox.Show($"커스텀 컴포넌트 초기화 오류: {ex.Message}");
#endif
            }
        }

        /// <summary>
        /// 🚀 PropertyPosition을 사용하여 Position Item들을 listBoxItemsView에 설정
        /// </summary>
        private void SetPropertyPositionToListBox()
        {
            try
            {
                // Equipment에서 CassetteLoadingElevator Unit 가져오기
                var equipment = Equipment.Instance;
                const string UNIT_NAME = "CassetteLoadingElevator";

                if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                {
                    var cassetteUnit = unit as CassetteLoadingElevator;
                    if (cassetteUnit?.CassetteElevator?.Config?.PropertyPosition != null)
                    {
                        var propertyPosition = cassetteUnit.CassetteElevator.Config.PropertyPosition;
                        
                        // PropertyPosition에서 Position Title들을 추출하여 ListBox에 설정
                        var positionTitles = propertyPosition.GetPropertyTitles();
                        
                        if (positionTitles.Length > 0)
                        {
                            // listBoxItemsView에 Position Title들 설정
                            listBoxItemsView?.SetItems(positionTitles);
                            
                            Console.WriteLine($"✅ PropertyPosition을 listBoxItemsView에 설정 완료: {positionTitles.Length}개 항목");
                            Console.WriteLine($"   설정된 항목들: {string.Join(", ", positionTitles)}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ PropertyPosition에 Position 항목이 없습니다.");
                            listBoxItemsView?.SetItems();
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
            if (listBoxItemsView != null)
            {
                // 기존 이벤트 핸들러 제거 (중복 방지)
                listBoxItemsView.ItemSelected -= OnPositionItemSelected;
                
                // 새 이벤트 핸들러 등록
                listBoxItemsView.ItemSelected += OnPositionItemSelected;
                
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
                    if (cassetteUnit?.CassetteElevator?.Config?.PropertyPosition != null)
                    {
                        var propertyPosition = cassetteUnit.CassetteElevator.Config.PropertyPosition;
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
                                propertyCollectionView?.SetProperties(editorProperties);
                                
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

        #endregion

        #region Save/Cancel 버튼 이벤트 핸들러

        /// <summary>
        /// 🚀 Save 버튼 클릭 이벤트 - Editor의 변경 내용을 PropertyPosition에 적용
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // PropertyCollectionView의 변경사항을 적용
                propertyCollectionView?.Apply();
                
                // 현재 선택된 Position Item의 값을 PropertyPosition에 저장
                if (listBoxItemsView.SelectedIndex >= 0)
                {
                    var equipment = Equipment.Instance;
                    const string UNIT_NAME = "CassetteLoadingElevator";

                    if (equipment.Units.TryGetValue(UNIT_NAME, out var unit))
                    {
                        var cassetteUnit = unit as CassetteLoadingElevator;
                        if (cassetteUnit?.CassetteElevator?.Config?.PropertyPosition != null)
                        {
                            var propertyPosition = cassetteUnit.CassetteElevator.Config.PropertyPosition;
                            var positionTitles = propertyPosition.GetPropertyTitles();
                            
                            if (listBoxItemsView.SelectedIndex < positionTitles.Length)
                            {
                                var selectedTitle = positionTitles[listBoxItemsView.SelectedIndex];
                                
                                // 🚀 Editor에서 편집된 값을 가져오기
                                var editorProperties = propertyCollectionView?.GetCurrentProperties();
                                var editedProperty = editorProperties?.Where(p => p.Title == selectedTitle)?.FirstOrDefault();
                                
                                if (editedProperty is DoubleProperty editedDoubleProp)
                                {
                                    // 🚀 PropertyPosition의 원본 Property에 편집된 값 적용
                                    var originalProperty = propertyPosition.GetPropertyByTitle(selectedTitle) as DoubleProperty;
                                    if (originalProperty != null)
                                    {
                                        // Editor → PropertyPosition (올바른 방향)
                                        editedDoubleProp.Value = originalProperty.Value; 

                                        // PropertyPosition → Config 동기화
                                        cassetteUnit.CassetteElevator.Config.SyncFromPropertyPosition();
                                        
                                        Console.WriteLine($"✅ Position 값 저장: {selectedTitle} = {editedDoubleProp.Value:F3} mm");
                                        Console.WriteLine($"   PropertyPosition 업데이트: {originalProperty.Value:F3}");
                                        
                                        // Config 값 확인
                                        var config = cassetteUnit.CassetteElevator.Config;
                                        if (selectedTitle == nameof(config.LifterZLoadingPosition))
                                        {
                                            Console.WriteLine($"   Config.LifterZLoadingPosition: {config.LifterZLoadingPosition:F3}");
                                        }
                                        
                                        MessageBox.Show($"Position 값이 저장되었습니다.\n{selectedTitle}: {editedDoubleProp.Value:F3} mm", 
                                                      "저장 완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"⚠️ PropertyPosition에서 원본 Property를 찾을 수 없습니다: {selectedTitle}");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"⚠️ Editor에서 편집된 Property를 찾을 수 없습니다: {selectedTitle}");
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Position Item을 선택해주세요.", "저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position 저장 중 오류: {ex.Message}");
                MessageBox.Show($"Position 저장 중 오류가 발생했습니다:\n{ex.Message}", 
                              "저장 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 🚀 Cancel 버튼 클릭 이벤트 - Editor의 변경 내용을 취소하고 원래 값으로 복원
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                // 현재 선택된 Position Item을 다시 로드하여 원래 값으로 복원
                if (listBoxItemsView.SelectedIndex >= 0)
                {
                    OnPositionItemSelected(listBoxItemsView, listBoxItemsView.SelectedIndex);
                    
                    Console.WriteLine("✅ Position 편집 취소 - 원래 값으로 복원");
                    MessageBox.Show("편집 내용이 취소되었습니다.", "취소 완료", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // 선택된 항목이 없으면 Editor를 비움
                    propertyCollectionView?.SetProperties(null);
                    Console.WriteLine("✅ Editor 초기화 완료");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Position 취소 중 오류: {ex.Message}");
                MessageBox.Show($"편집 취소 중 오류가 발생했습니다:\n{ex.Message}", 
                              "취소 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}