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
        private PropertyCollectionView propertyCollectionView;
        private IOPropertyCollectionView inputPropertyCollectionView;
        private IOPropertyCollectionView outputPropertyCollectionView;
        private ListBoxItemsView positionlistBoxItemsView;

        // Axis 목록 (추가)
        private ListBoxItemsView axisListBoxItemsView;

        // Position / Editor 버튼
        private IndividualMenuButton btnSave;
        private IndividualMenuButton btnCancel;
        private IndividualMenuButton btnMovePosition;

        private RadioButtonView radioButtonView;

        // Actual Position 표시 라벨 (추가)
        private CustomBorderLabel lblAxisPositionCaption;
        private CustomBorderLabel lblAxisPositionValue;
        private CustomBorderLabel lblAxisPositionUnit;

        // 원시 값 등 추가표시(선택) - 예시
        private CustomBorderLabel lblAxisPositionRaw;

        // 그룹박스들
        private GroupBox gbTeachingMove;
        private GroupBox gbPositionTeaching;
        private GroupBox gbDigitalIO;

        // Move Axis 외곽 그룹박스 (추가)
        private GroupBox gbMoveAxis;
        private GroupBox gbSelectAxis;
        private GroupBox gbCommandMove;
        private GroupBox gbDestinationPosition;

        private System.ComponentModel.IContainer components = null;

        // 내부 상태
        private AxisDefinition _currentAxis;
        private PropertyPosition _currentPositionItem;
        private readonly Dictionary<string, AxisDefinition> _axisMap = new Dictionary<string, AxisDefinition>(StringComparer.OrdinalIgnoreCase);

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
            this.radioButtonView = new QMC.Common.RadioButtonView();
            this.gbPositionTeaching = new System.Windows.Forms.GroupBox();
            this.positionlistBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.propertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.btnSave = new QMC.Common.IndividualMenuButton();
            this.btnCancel = new QMC.Common.IndividualMenuButton();
            this.gbDigitalIO = new System.Windows.Forms.GroupBox();
            this.inputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();
            this.outputPropertyCollectionView = new QMC.Common.IOPropertyCollectionView();

            // ===== Move Axis 영역 추가 시작 =====
            this.gbMoveAxis = new System.Windows.Forms.GroupBox();
            this.gbSelectAxis = new System.Windows.Forms.GroupBox();
            this.axisListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.lblAxisPositionCaption = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lblAxisPositionValue = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lblAxisPositionUnit = new QMC.Common.CustomControl.CustomBorderLabel();
            this.lblAxisPositionRaw = new QMC.Common.CustomControl.CustomBorderLabel();
            this.gbCommandMove = new System.Windows.Forms.GroupBox();
            this.gbDestinationPosition = new System.Windows.Forms.GroupBox();
            // ===== Move Axis 영역 추가 끝 =====

            this.gbTeachingMove.SuspendLayout();
            this.gbPositionTeaching.SuspendLayout();
            this.gbDigitalIO.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbTeachingMove
            // 
            this.gbTeachingMove.BackColor = System.Drawing.Color.White;
            this.gbTeachingMove.Controls.Add(this.btnMovePosition);
            this.gbTeachingMove.Controls.Add(this.radioButtonView);
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
            this.btnMovePosition.Click += new EventHandler(this.btnMovePosition_Click);
            // 
            // radioButtonView
            // 
            this.radioButtonView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.radioButtonView.GroupName = "Move Mode";
            this.radioButtonView.Location = new System.Drawing.Point(13, 20);
            this.radioButtonView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.radioButtonView.Name = "radioButtonView";
            this.radioButtonView.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.radioButtonView.SelectedIndex = -1;
            this.radioButtonView.Size = new System.Drawing.Size(171, 106);
            this.radioButtonView.TabIndex = 5;
            // 
            // gbPositionTeaching
            // 
            this.gbPositionTeaching.BackColor = System.Drawing.Color.White;
            this.gbPositionTeaching.Controls.Add(this.positionlistBoxItemsView);
            this.gbPositionTeaching.Controls.Add(this.btnSave);
            this.gbPositionTeaching.Controls.Add(this.btnCancel);
            this.gbPositionTeaching.Controls.Add(this.gbTeachingMove);
            this.gbPositionTeaching.Controls.Add(this.propertyCollectionView);
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
            this.positionlistBoxItemsView.Location = new System.Drawing.Point(9, 24);
            this.positionlistBoxItemsView.Name = "positionlistBoxItemsView";
            this.positionlistBoxItemsView.SelectedIndex = -1;
            this.positionlistBoxItemsView.Size = new System.Drawing.Size(257, 323);
            this.positionlistBoxItemsView.TabIndex = 2;
            this.positionlistBoxItemsView.ItemSelected += new EventHandler<int>(this.OnPositionItemSelected);
            // 
            // propertyCollectionView
            // 
            this.propertyCollectionView.GroupName = "Editor";
            this.propertyCollectionView.Location = new System.Drawing.Point(279, 24);
            this.propertyCollectionView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.propertyCollectionView.Name = "propertyCollectionView";
            this.propertyCollectionView.Size = new System.Drawing.Size(326, 169);
            this.propertyCollectionView.TabIndex = 0;
            this.propertyCollectionView.TextBoxFont = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
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
            // gbDigitalIO
            // 
            this.gbDigitalIO.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.gbDigitalIO.BackColor = System.Drawing.Color.White;
            this.gbDigitalIO.Controls.Add(this.inputPropertyCollectionView);
            this.gbDigitalIO.Controls.Add(this.outputPropertyCollectionView);
            this.gbDigitalIO.Font = new System.Drawing.Font("맑은 고딕", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.gbDigitalIO.Location = new System.Drawing.Point(21, 434);
            this.gbDigitalIO.Name = "gbDigitalIO";
            this.gbDigitalIO.Size = new System.Drawing.Size(608, 290);
            this.gbDigitalIO.TabIndex = 9;
            this.gbDigitalIO.TabStop = false;
            this.gbDigitalIO.Text = "Digital I/O";
            // 
            // inputPropertyCollectionView
            // 
            this.inputPropertyCollectionView.GroupName = "Input";
            this.inputPropertyCollectionView.Location = new System.Drawing.Point(10, 25);
            this.inputPropertyCollectionView.Margin = new System.Windows.Forms.Padding(4);
            this.inputPropertyCollectionView.Name = "inputPropertyCollectionView";
            this.inputPropertyCollectionView.Size = new System.Drawing.Size(290, 254);
            this.inputPropertyCollectionView.TabIndex = 1;
            // 
            // outputPropertyCollectionView
            // 
            this.outputPropertyCollectionView.GroupName = "Output";
            this.outputPropertyCollectionView.Location = new System.Drawing.Point(308, 25);
            this.outputPropertyCollectionView.Margin = new System.Windows.Forms.Padding(4);
            this.outputPropertyCollectionView.Name = "outputPropertyCollectionView";
            this.outputPropertyCollectionView.Size = new System.Drawing.Size(290, 254);
            this.outputPropertyCollectionView.TabIndex = 1;
            // 
            // ===== Move Axis 그룹 구성 =====
            // gbMoveAxis
            this.gbMoveAxis.BackColor = System.Drawing.Color.White;
            this.gbMoveAxis.Font = new System.Drawing.Font("맑은 고딕", 10F);
            this.gbMoveAxis.Location = new System.Drawing.Point(640, 12);
            this.gbMoveAxis.Name = "gbMoveAxis";
            this.gbMoveAxis.Size = new System.Drawing.Size(600, 361);
            this.gbMoveAxis.TabIndex = 10;
            this.gbMoveAxis.TabStop = false;
            this.gbMoveAxis.Text = "Move Axis";
            // 
            // gbSelectAxis
            // 
            this.gbSelectAxis.BackColor = System.Drawing.Color.White;
            this.gbSelectAxis.Font = new System.Drawing.Font("맑은 고딕", 9F);
            this.gbSelectAxis.Text = "Select Axis";
            this.gbSelectAxis.Location = new System.Drawing.Point(15, 25);
            this.gbSelectAxis.Name = "gbSelectAxis";
            this.gbSelectAxis.Size = new System.Drawing.Size(250, 150);
            this.gbSelectAxis.TabStop = false;
            // 
            // axisListBoxItemsView
            // 
            this.axisListBoxItemsView.GroupName = "";
            this.axisListBoxItemsView.Location = new System.Drawing.Point(8, 18);
            this.axisListBoxItemsView.Name = "axisListBoxItemsView";
            this.axisListBoxItemsView.SelectedIndex = -1;
            this.axisListBoxItemsView.Size = new System.Drawing.Size(234, 124);
            this.axisListBoxItemsView.TabIndex = 0;
            this.axisListBoxItemsView.ItemSelected += new System.EventHandler<int>(this.OnAxisSelected);
            // 
            // lblAxisPositionCaption
            // 
            this.lblAxisPositionCaption.Text = "Position";
            this.lblAxisPositionCaption.TextAlign = ContentAlignment.MiddleCenter;
            this.lblAxisPositionCaption.Location = new System.Drawing.Point(15, 185);
            this.lblAxisPositionCaption.Size = new System.Drawing.Size(90, 30);
            this.lblAxisPositionCaption.BorderColor = Color.Black;
            // 
            // lblAxisPositionValue
            // 
            this.lblAxisPositionValue.Text = "000.000";
            this.lblAxisPositionValue.TextAlign = ContentAlignment.MiddleCenter;
            this.lblAxisPositionValue.Location = new System.Drawing.Point(105, 185);
            this.lblAxisPositionValue.Size = new System.Drawing.Size(160, 30);
            this.lblAxisPositionValue.BorderColor = Color.Black;
            this.lblAxisPositionValue.BackColor = Color.Black;
            this.lblAxisPositionValue.ForeColor = Color.Lime;
            this.lblAxisPositionValue.Font = new Font("Consolas", 14F, FontStyle.Bold);
            // 
            // lblAxisPositionUnit
            // 
            this.lblAxisPositionUnit.Text = "mm";
            this.lblAxisPositionUnit.TextAlign = ContentAlignment.MiddleCenter;
            this.lblAxisPositionUnit.Location = new System.Drawing.Point(265, 185);
            this.lblAxisPositionUnit.Size = new System.Drawing.Size(50, 30);
            this.lblAxisPositionUnit.BorderColor = Color.Black;
            this.lblAxisPositionUnit.BackColor = Color.White;
            // 
            // lblAxisPositionRaw (옵션)
            // 
            this.lblAxisPositionRaw.Text = "0";
            this.lblAxisPositionRaw.TextAlign = ContentAlignment.MiddleCenter;
            this.lblAxisPositionRaw.Location = new System.Drawing.Point(315, 185);
            this.lblAxisPositionRaw.Size = new System.Drawing.Size(50, 30);
            this.lblAxisPositionRaw.BorderColor = Color.Black;
            this.lblAxisPositionRaw.BackColor = Color.FromArgb(217, 217, 217);
            // 
            // gbCommandMove (Move Mode/Absolute/Relative 등 확장 여지)
            // 
            this.gbCommandMove.Text = "Command Move";
            this.gbCommandMove.Location = new System.Drawing.Point(370, 25);
            this.gbCommandMove.Size = new System.Drawing.Size(210, 90);
            this.gbCommandMove.BackColor = Color.White;
            // 
            // gbDestinationPosition
            // 
            this.gbDestinationPosition.Text = "Destination Position";
            this.gbDestinationPosition.Location = new System.Drawing.Point(370, 120);
            this.gbDestinationPosition.Size = new System.Drawing.Size(210, 95);
            this.gbDestinationPosition.BackColor = Color.White;
            // 
            // Move Axis 그룹에 컨트롤 추가
            this.gbMoveAxis.Controls.Add(this.gbSelectAxis);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionCaption);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionValue);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionUnit);
            this.gbMoveAxis.Controls.Add(this.lblAxisPositionRaw);
            this.gbMoveAxis.Controls.Add(this.gbCommandMove);
            this.gbMoveAxis.Controls.Add(this.gbDestinationPosition);
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
            this.Load += new EventHandler(this.CassetteLoadingElevatorUnit_Config_Load);

            this.gbTeachingMove.ResumeLayout(false);
            this.gbPositionTeaching.ResumeLayout(false);
            this.gbDigitalIO.ResumeLayout(false);
            this.gbMoveAxis.ResumeLayout(false);
            this.gbSelectAxis.ResumeLayout(false);
            this.ResumeLayout(false);

            // Actual Position Timer
            _axisPosTimer = new Timer();
            _axisPosTimer.Interval = 200;
            _axisPosTimer.Tick += (s, e) => UpdateAxisActualPosition();
            _axisPosTimer.Start();
        }

        #endregion

        #region Axis / Position 초기화 로직 (추가)

        private void CassetteLoadingElevatorUnit_Config_Load(object sender, EventArgs e)
        {
            InitializeUI();
            SetAxisDefinitionsToAxisListBox();
        }

        /// <summary>
        /// CassetteElevator + WaferTransferArm 의 AxisDefinition DisplayName 을 axisListBoxItemsView 에 설정
        /// </summary>
        private void SetAxisDefinitionsToAxisListBox()
        {
            _axisMap.Clear();
            var equipment = Equipment.Instance;
            var list = new List<string>();

            if (equipment.Units.TryGetValue("CassetteLoadingElevator", out var unitObj))
            {
                var loadUnit = unitObj as CassetteLoadingElevator;

                var ce = loadUnit?.CassetteElevator;
                if (ce?.Axes != null)
                {
                    foreach (var a in ce.Axes)
                    {
                        if (!_axisMap.ContainsKey(a.DisplayName))
                        {
                            _axisMap.Add(a.DisplayName, a);
                            list.Add(a.DisplayName);
                        }
                    }
                }

                var wta = loadUnit?.WaferTransferArm;
                if (wta?.Axes != null)
                {
                    foreach (var a in wta.Axes)
                    {
                        if (!_axisMap.ContainsKey(a.DisplayName))
                        {
                            _axisMap.Add(a.DisplayName, a);
                            list.Add(a.DisplayName);
                        }
                    }
                }
            }

            axisListBoxItemsView.SetItems(list.ToArray());
        }

        private void OnAxisSelected(object sender, int index)
        {
            var items = axisListBoxItemsView.GetItems();
            if (items == null || index < 0 || index >= items.Length) return;
            var name = items[index];
            if (_axisMap.TryGetValue(name, out var axis))
            {
                _currentAxis = axis;
                var titles = axis.PositionItems.Select(p => p.Title).ToArray();
                positionlistBoxItemsView.SetItems(titles);
                _currentPositionItem = null;
                propertyCollectionView.SetProperties(null);
                UpdateAxisActualPosition();
            }
        }

        private void OnPositionItemSelected(object sender, int index)
        {
            if (_currentAxis == null) return;
            if (index < 0 || index >= _currentAxis.PositionItems.Count) return;

            _currentPositionItem = _currentAxis.PositionItems[index];
            var axisName = _currentAxis.MotionAxis.Name;

            var posProp = _currentPositionItem.GetDoubleProperties()
                                              .FirstOrDefault(p => p.Title == axisName);

            var editor = new PropertyCollection();
            editor.Add(new TitleOnlyProperty("Position (Abs, mm)"));
            if (posProp != null)
                editor.Add(new DoubleProperty(posProp.Title, posProp.Value));

            propertyCollectionView.SetProperties(editor);
        }

        private void UpdateAxisActualPosition()
        {
            if (_currentAxis?.MotionAxis == null)
            {
                lblAxisPositionValue.Text = "---";
                return;
            }

            try
            {
                double act = _currentAxis.MotionAxis.GetActualPosition();
                lblAxisPositionValue.Text = act.ToString("0.000");
                lblAxisPositionUnit.Text = _currentAxis.MotionAxis.Unit ?? "mm";
            }
            catch (Exception ex)
            {
                lblAxisPositionValue.Text = "ERR";
            }
        }

        private void btnMovePosition_Click(object sender, EventArgs e)
        {
            if (_currentAxis == null || _currentPositionItem == null) return;
            var axisName = _currentAxis.MotionAxis.Name;
            var posProp = _currentPositionItem.GetDoubleProperties()
                                              .FirstOrDefault(p => p.Title == axisName);
            if (posProp == null) return;

            string err;
            if (!_currentAxis.MotionAxis.MoveAbs(posProp.Value, 50, 500, 500, 5000, out err))
                MessageBox.Show("Move 실패: " + err);
        }

        #endregion

        #region 기존 InitializeUI 재사용

        private void InitializeUI()
        {
            try
            {
                InitializeRadioButtonView();
                // 기존 Position list 초기화 대신 Axis 선택 후 채우도록 지연
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show($"UI 초기화 오류: {ex.Message}");
#endif
            }
        }

        private void InitializeRadioButtonView()
        {
            try
            {
                radioButtonView?.SetOptions(true, "Fine", "Coarse");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RadioButtonView 오류: {ex.Message}");
            }
        }

        #endregion

        #region Save / Cancel


      #region Save / Cancel

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_currentAxis == null || _currentPositionItem == null) return;

            var editorProps = propertyCollectionView?.GetCurrentProperties();
            if (editorProps == null) return;

            var axisName = _currentAxis.MotionAxis.Name;
            var edited = editorProps.OfType<DoubleProperty>().FirstOrDefault(p => p.Title == axisName);
            if (edited == null) return;

            var original = _currentPositionItem.GetDoubleProperties()
                                               .FirstOrDefault(p => p.Title == axisName);
            if (original != null)
            {
                original.Value = edited.Value;
                Console.WriteLine($"[Save] {_currentPositionItem.Title} = {original.Value:0.000}");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (positionlistBoxItemsView.SelectedIndex >= 0)
                OnPositionItemSelected(positionlistBoxItemsView, positionlistBoxItemsView.SelectedIndex);
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

    }
}