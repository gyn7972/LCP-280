using QMC.Common;
using QMC.Common.Cameras;
using QMC.Common.Cameras.HIKVISION;
using QMC.Common.CustomControl;
using QMC.Common.Motions;
using QMC.Common.Vision;
using QMC.LCP_280.Process;
using QMC.LCP_280.Process.Unit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace QMC.LCP_280.Process.Unit
{
    /// <summary>
    /// Motion_Setup (refactored: minimal-change, safer wiring, clearer structure)
    /// </summary>
    partial class Vision_Setup
    {
        private readonly Equipment equipment = Equipment.Instance;

        private Common.Vision.VisionImageViewer visionImageViewer;

        private ListBoxItemsView cameraListBoxItemsView;
        private ListBoxItemsView iluminatorListBoxItemsView;
        private ListBoxItemsView iluminatorChannelListBoxItemsView;

        private PropertyCollectionView cameraPropertyCollectionView;
        private PropertyCollectionView illuminatorPropertyCollectionView;

        private IndividualMenuButton btn_Save_Setup_Cylinder;
        private IndividualMenuButton individualMenuButton1;
        private IndividualMenuButton btn_Camera_Setup;
        private IndividualMenuButton btn_Illuninator_Setup;
        private IndividualMenuButton btn_Off_Illuminator;
        private IndividualMenuButton btn_On_Illuminator;
        private IndividualMenuButton btn_Save_Camera_Setup;
        private IndividualMenuButton btn_Save_Illuninator_Setup;

        private GroupBox gbIlluminatorControl;

        // Vision_Setup 클래스 내부 필드
        private CameraSwitch _camSwitch;
        private List<string> _cameraNames;

        private PropertyCollection _editorPropertiesConfig;
        private PropertyCollection _editorPropertiesSpeed;

        private Timer _axisPosTimer;

        private System.ComponentModel.IContainer components = null;

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
            this.components = new System.ComponentModel.Container();
            this.cameraListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.cameraPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.btn_Save_Setup_Cylinder = new QMC.Common.IndividualMenuButton();
            this.visionImageViewer = new QMC.Common.Vision.VisionImageViewer();
            this.iluminatorListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.illuminatorPropertyCollectionView = new QMC.Common.PropertyCollectionView();
            this.individualMenuButton1 = new QMC.Common.IndividualMenuButton();
            this.iluminatorChannelListBoxItemsView = new QMC.Common.ListBoxItemsView();
            this.btn_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Illuninator_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Off_Illuminator = new QMC.Common.IndividualMenuButton();
            this.btn_On_Illuminator = new QMC.Common.IndividualMenuButton();
            this.gbIlluminatorControl = new System.Windows.Forms.GroupBox();
            this.btn_Save_Camera_Setup = new QMC.Common.IndividualMenuButton();
            this.btn_Save_Illuninator_Setup = new QMC.Common.IndividualMenuButton();
            this.cameraPropertyCollectionView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).BeginInit();
            this.illuminatorPropertyCollectionView.SuspendLayout();
            this.gbIlluminatorControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // cameraListBoxItemsView
            // 
            this.cameraListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cameraListBoxItemsView.BorderWidth = 2;
            this.cameraListBoxItemsView.GroupName = "Camera";
            this.cameraListBoxItemsView.Location = new System.Drawing.Point(12, 12);
            this.cameraListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.cameraListBoxItemsView.Name = "cameraListBoxItemsView";
            this.cameraListBoxItemsView.SelectedIndex = -1;
            this.cameraListBoxItemsView.Size = new System.Drawing.Size(212, 675);
            this.cameraListBoxItemsView.TabIndex = 2;
            // 
            // cameraPropertyCollectionView
            // 
            this.cameraPropertyCollectionView.Controls.Add(this.btn_Save_Setup_Cylinder);
            this.cameraPropertyCollectionView.GroupName = "Property";
            this.cameraPropertyCollectionView.Location = new System.Drawing.Point(229, 482);
            this.cameraPropertyCollectionView.Name = "cameraPropertyCollectionView";
            this.cameraPropertyCollectionView.Size = new System.Drawing.Size(400, 215);
            this.cameraPropertyCollectionView.TabIndex = 13;
            // 
            // btn_Save_Setup_Cylinder
            // 
            this.btn_Save_Setup_Cylinder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Setup_Cylinder.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Setup_Cylinder.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Setup_Cylinder.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Setup_Cylinder.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Setup_Cylinder.Location = new System.Drawing.Point(330, 312);
            this.btn_Save_Setup_Cylinder.Name = "btn_Save_Setup_Cylinder";
            this.btn_Save_Setup_Cylinder.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Setup_Cylinder.TabIndex = 5;
            this.btn_Save_Setup_Cylinder.TabStop = false;
            this.btn_Save_Setup_Cylinder.Text = "Save";
            this.btn_Save_Setup_Cylinder.UseVisualStyleBackColor = false;
            this.btn_Save_Setup_Cylinder.Click += new System.EventHandler(this.btn_Save_Setup_Cylinder_Click);
            // 
            // visionImageViewer
            // 
            this.visionImageViewer.BackColor = System.Drawing.Color.Black;
            this.visionImageViewer.FrameRate = 1D;
            this.visionImageViewer.InputImage = null;
            this.visionImageViewer.IsViewCustomizedImage = false;
            this.visionImageViewer.Location = new System.Drawing.Point(229, 22);
            this.visionImageViewer.Name = "visionImageViewer";
            this.visionImageViewer.OperatingType = QMC.Common.Vision.VisionImageViewer.OperatingTypes.Center;
            this.visionImageViewer.Simulated = false;
            this.visionImageViewer.Size = new System.Drawing.Size(400, 445);
            this.visionImageViewer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.visionImageViewer.TabIndex = 16;
            this.visionImageViewer.TabStop = false;
            this.visionImageViewer.UpdateDelayTime = 80;
            this.visionImageViewer.VisibleCrossLine = true;
            this.visionImageViewer.Camera = null;
            this.visionImageViewer.CameraSwitch = null;
            this.visionImageViewer.SizeMode = PictureBoxSizeMode.CenterImage;
            this.visionImageViewer.SuspendDisplay();

            // 
            // iluminatorListBoxItemsView
            // 
            this.iluminatorListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.iluminatorListBoxItemsView.BorderWidth = 2;
            this.iluminatorListBoxItemsView.GroupName = "Illuminator";
            this.iluminatorListBoxItemsView.Location = new System.Drawing.Point(634, 15);
            this.iluminatorListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorListBoxItemsView.Name = "iluminatorListBoxItemsView";
            this.iluminatorListBoxItemsView.SelectedIndex = -1;
            this.iluminatorListBoxItemsView.Size = new System.Drawing.Size(212, 675);
            this.iluminatorListBoxItemsView.TabIndex = 17;
            // 
            // illuminatorPropertyCollectionView
            // 
            this.illuminatorPropertyCollectionView.Controls.Add(this.individualMenuButton1);
            this.illuminatorPropertyCollectionView.GroupName = "Property";
            this.illuminatorPropertyCollectionView.Location = new System.Drawing.Point(851, 327);
            this.illuminatorPropertyCollectionView.Name = "illuminatorPropertyCollectionView";
            this.illuminatorPropertyCollectionView.Size = new System.Drawing.Size(400, 247);
            this.illuminatorPropertyCollectionView.TabIndex = 18;
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
            this.individualMenuButton1.Location = new System.Drawing.Point(330, 312);
            this.individualMenuButton1.Name = "individualMenuButton1";
            this.individualMenuButton1.Size = new System.Drawing.Size(100, 40);
            this.individualMenuButton1.TabIndex = 5;
            this.individualMenuButton1.TabStop = false;
            this.individualMenuButton1.Text = "Save";
            this.individualMenuButton1.UseVisualStyleBackColor = false;
            // 
            // iluminatorChannelListBoxItemsView
            // 
            this.iluminatorChannelListBoxItemsView.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.iluminatorChannelListBoxItemsView.BorderWidth = 2;
            this.iluminatorChannelListBoxItemsView.GroupName = "Channel";
            this.iluminatorChannelListBoxItemsView.Location = new System.Drawing.Point(851, 15);
            this.iluminatorChannelListBoxItemsView.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.iluminatorChannelListBoxItemsView.Name = "iluminatorChannelListBoxItemsView";
            this.iluminatorChannelListBoxItemsView.SelectedIndex = -1;
            this.iluminatorChannelListBoxItemsView.Size = new System.Drawing.Size(400, 303);
            this.iluminatorChannelListBoxItemsView.TabIndex = 19;
            // 
            // btn_Camera_Setup
            // 
            this.btn_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Camera_Setup.Location = new System.Drawing.Point(123, 706);
            this.btn_Camera_Setup.Name = "btn_Camera_Setup";
            this.btn_Camera_Setup.Size = new System.Drawing.Size(100, 40);
            this.btn_Camera_Setup.TabIndex = 20;
            this.btn_Camera_Setup.TabStop = false;
            this.btn_Camera_Setup.Text = "Set up";
            this.btn_Camera_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Illuninator_Setup
            // 
            this.btn_Illuninator_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Illuninator_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Illuninator_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Illuninator_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Illuninator_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Illuninator_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Illuninator_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Illuninator_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Illuninator_Setup.Location = new System.Drawing.Point(746, 706);
            this.btn_Illuninator_Setup.Name = "btn_Illuninator_Setup";
            this.btn_Illuninator_Setup.Size = new System.Drawing.Size(100, 40);
            this.btn_Illuninator_Setup.TabIndex = 21;
            this.btn_Illuninator_Setup.TabStop = false;
            this.btn_Illuninator_Setup.Text = "Set up";
            this.btn_Illuninator_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Off_Illuminator
            // 
            this.btn_Off_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Off_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Off_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Off_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Off_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Off_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Off_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_Off_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Off_Illuminator.Location = new System.Drawing.Point(229, 40);
            this.btn_Off_Illuminator.Name = "btn_Off_Illuminator";
            this.btn_Off_Illuminator.Size = new System.Drawing.Size(150, 50);
            this.btn_Off_Illuminator.TabIndex = 1;
            this.btn_Off_Illuminator.TabStop = false;
            this.btn_Off_Illuminator.Text = "Off";
            this.btn_Off_Illuminator.UseVisualStyleBackColor = false;
            // 
            // btn_On_Illuminator
            // 
            this.btn_On_Illuminator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_On_Illuminator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_On_Illuminator.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_On_Illuminator.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_On_Illuminator.CustomForeColor = System.Drawing.Color.Black;
            this.btn_On_Illuminator.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_On_Illuminator.ForeColor = System.Drawing.Color.Black;
            this.btn_On_Illuminator.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_On_Illuminator.Location = new System.Drawing.Point(22, 40);
            this.btn_On_Illuminator.Name = "btn_On_Illuminator";
            this.btn_On_Illuminator.Size = new System.Drawing.Size(150, 50);
            this.btn_On_Illuminator.TabIndex = 0;
            this.btn_On_Illuminator.TabStop = false;
            this.btn_On_Illuminator.Text = "On";
            this.btn_On_Illuminator.UseVisualStyleBackColor = false;
            // 
            // gbIlluminatorControl
            // 
            this.gbIlluminatorControl.BackColor = System.Drawing.Color.White;
            this.gbIlluminatorControl.Controls.Add(this.btn_Off_Illuminator);
            this.gbIlluminatorControl.Controls.Add(this.btn_On_Illuminator);
            this.gbIlluminatorControl.Location = new System.Drawing.Point(852, 626);
            this.gbIlluminatorControl.Name = "gbIlluminatorControl";
            this.gbIlluminatorControl.Size = new System.Drawing.Size(400, 108);
            this.gbIlluminatorControl.TabIndex = 22;
            this.gbIlluminatorControl.TabStop = false;
            this.gbIlluminatorControl.Text = "Control";
            // 
            // btn_Save_Camera_Setup
            // 
            this.btn_Save_Camera_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Camera_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Camera_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Camera_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Camera_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Camera_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Camera_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Camera_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Camera_Setup.Location = new System.Drawing.Point(529, 704);
            this.btn_Save_Camera_Setup.Name = "btn_Save_Camera_Setup";
            this.btn_Save_Camera_Setup.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Camera_Setup.TabIndex = 23;
            this.btn_Save_Camera_Setup.TabStop = false;
            this.btn_Save_Camera_Setup.Text = "Save";
            this.btn_Save_Camera_Setup.UseVisualStyleBackColor = false;
            // 
            // btn_Save_Illuninator_Setup
            // 
            this.btn_Save_Illuninator_Setup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Illuninator_Setup.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btn_Save_Illuninator_Setup.CustomBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(217)))), ((int)(((byte)(217)))));
            this.btn_Save_Illuninator_Setup.CustomFont = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Illuninator_Setup.CustomForeColor = System.Drawing.Color.Black;
            this.btn_Save_Illuninator_Setup.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btn_Save_Illuninator_Setup.ForeColor = System.Drawing.Color.Black;
            this.btn_Save_Illuninator_Setup.ImageSize = new System.Drawing.Size(45, 45);
            this.btn_Save_Illuninator_Setup.Location = new System.Drawing.Point(1148, 579);
            this.btn_Save_Illuninator_Setup.Name = "btn_Save_Illuninator_Setup";
            this.btn_Save_Illuninator_Setup.Size = new System.Drawing.Size(100, 40);
            this.btn_Save_Illuninator_Setup.TabIndex = 24;
            this.btn_Save_Illuninator_Setup.TabStop = false;
            this.btn_Save_Illuninator_Setup.Text = "Save";
            this.btn_Save_Illuninator_Setup.UseVisualStyleBackColor = false;
            // 
            // Vision_Setup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1264, 752);
            this.Controls.Add(this.btn_Save_Illuninator_Setup);
            this.Controls.Add(this.btn_Save_Camera_Setup);
            this.Controls.Add(this.gbIlluminatorControl);
            this.Controls.Add(this.btn_Illuninator_Setup);
            this.Controls.Add(this.btn_Camera_Setup);
            this.Controls.Add(this.iluminatorChannelListBoxItemsView);
            this.Controls.Add(this.iluminatorListBoxItemsView);
            this.Controls.Add(this.illuminatorPropertyCollectionView);
            this.Controls.Add(this.visionImageViewer);
            this.Controls.Add(this.cameraListBoxItemsView);
            this.Controls.Add(this.cameraPropertyCollectionView);
            this.Name = "Vision_Setup";
            this.Text = "Motion Setup";
            this.cameraPropertyCollectionView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.visionImageViewer)).EndInit();
            this.illuminatorPropertyCollectionView.ResumeLayout(false);
            this.gbIlluminatorControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        // -------- Public/Init
        private void InitializeUI()
        {
            try
            {
                BinVisionList();
                WireAxisSelectionEvent();
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
        private void BinVisionList()
        {
            try
            {
                // 1) 좌측 리스트 바인딩
                _cameraNames = Equipment.Instance.Cameras.Keys.ToList();
                cameraListBoxItemsView.SetItems(_cameraNames.ToArray());
                //cameraListBoxItemsView.ItemSelected += CameraListBoxItemsView_ItemSelected;

                // 2) CameraSwitch 구성
                _camSwitch = new CameraSwitch();
                foreach (var cam in Equipment.Instance.Cameras.Values)
                    _camSwitch.Cameras.Add(cam);

                // 3) 뷰어에 스위치 연결 (Camera는 비워둬도 OK)
                visionImageViewer.CameraSwitch = _camSwitch;
                visionImageViewer.FrameRate = 30;

                // 4) 기본 선택
                if (_camSwitch.Cameras.Count > 0)
                {
                    _camSwitch.Change(0);
                    cameraListBoxItemsView.SelectedIndex = 0;
                    ResetViewerForCameraChange(); // 첫 프레임 즉시 표시
                    visionImageViewer.ResumeDisplay();
                }

                //// 1) 좌측 리스트 아이템 구성 (Equipment.Cameras: 이름→Camera)
                //_cameraNames = Equipment.Instance.Cameras.Keys.ToList();
                //cameraListBoxItemsView.SetItems(_cameraNames.ToArray());
                ////cameraListBoxItemsView.ItemSelected += CameraListBoxItemsView_ItemSelected;

                //// 2) 스위처 구성
                //_camSwitch = new CameraSwitch();
                //foreach (var cam in Equipment.Instance.Cameras.Values)
                //    _camSwitch.Cameras.Add(cam);

                //// 3) 뷰어에 스위처 연결
                //visionImageViewer.CameraSwitch = _camSwitch;  // 뷰어가 AfterChange를 구독하고 버퍼/스케일 재설정함
                //visionImageViewer.FrameRate = 10;

                //// 4) 기본 선택 (0번)
                //if (_camSwitch.Cameras.Count > 0)
                //{
                //    _camSwitch.Change(0);
                //    cameraListBoxItemsView.SelectedIndex = 0;
                //}

                //// 교차선은 마지막에 켜기 (아래 2번 수정과 세트)
                //visionImageViewer.VisibleCrossLine = true;

                //// ※ 디자이너에서 호출한 SuspendDisplay()가 있으면 여기서 해제
                //visionImageViewer.ResumeDisplay();
            }
            catch (Exception ex)
            {
                Log.Write("LCP-280", $"BindVisionList error: {ex}");
                cameraListBoxItemsView?.SetItems();
            }
        }

        // -------- Events
        private void WireAxisSelectionEvent()
        {
            if (cameraListBoxItemsView == null) return;

            // 중복 구독 방지
            cameraListBoxItemsView.ItemSelected -= OnVisionItemSelected;
            cameraListBoxItemsView.ItemSelected += OnVisionItemSelected;
        }

        /// <summary>
        /// Select Axis 리스트에서 항목 선택 시 속성 에디터 구성
        /// </summary>
        private void OnVisionItemSelected(object sender, int selectedIndex)
        {
            if (_camSwitch == null) return;
            if (selectedIndex < 0 || selectedIndex >= _camSwitch.Cameras.Count) return;

            // 1) 이전 Live stop (있으면)
            try { visionImageViewer.CurrentCamera?.StopLive(); } catch { }

            // 2) 깜빡임 방지: 잠시 정지
            visionImageViewer.SuspendDisplay();

            // 3) 카메라 체인지
            _camSwitch.Change(selectedIndex);

            // 4) 버퍼/스케일/크로스라인 리셋 + 첫 프레임 스냅샷으로 즉시 표시
            ResetViewerForCameraChange();

            //선택해서 읽자. 너무 느리다.
            //string strTemp = cameraListBoxItemsView.SelectedItemName;
            //if (Equipment.Instance.Cameras.TryGetValue(strTemp, out var cam))
            //{
            //    var hikCam = cam as HIKGigECamera;  // 다운캐스팅 (HIK 전용 기능 쓰려면)

            //    if (hikCam != null)
            //    {
            //        int ret = hikCam.ConnectAndGetProperties("", out var props);
            //        if (ret == 0)
            //        {
            //            //hikCam.ConnectAndSyncConfig(strTemp);
            //            //lblCamInfo.Text = $"{props.ModelName} ({props.SerialNo}) {props.Width}x{props.Height}";
            //            Log.Write("LCP-280", $"Camera connected: {props.ModelName} ({props.SerialNo}) {props.Width}x{props.Height}");
            //        }
            //        else
            //        {
            //            //lblCamInfo.Text = "Camera not connected";
            //            Log.Write("LCP-280", $"Camera not connected (ret={ret})");
            //        }
            //    }
            //}
            //else
            //{
            //    Console.WriteLine("Camera not found");
            //}

            // 5) 새 라이브 시작
            try { visionImageViewer.CurrentCamera?.StartLive(); } 
            catch (Exception ex)
            {
                Log.Write(ex);
            }


            // 6) 다시 표시
            visionImageViewer.ResumeDisplay();
            visionImageViewer.Display();

            //if (_camSwitch == null) return;
            //if (selectedIndex < 0 || selectedIndex >= _camSwitch.Cameras.Count) return;

            //// 이전 카메라가 Live 중이면 드라이버에 맞춰 정리(필요 시)
            //try 
            //{ 
            //    _camSwitch.Cameras[_camSwitch.SelectCameraIndex]?.StopLive(); 
            //} 
            //catch  (Exception ex)
            //{
            //    Log.Write(ex);
            //}

            //_camSwitch.Change(selectedIndex); // 뷰어가 AfterChange에서 버퍼/스케일/센터를 다시 맞춤
        }

        private void ResetViewerForCameraChange()
        {
            var cam = visionImageViewer.CurrentCamera;
            if (cam == null) return;

            // (a) 백버퍼 재생성
            //var ctx = BufferedGraphicsManager.Current;
            //ctx.MaximumBuffer = visionImageViewer.Size;
            //var old = GetViewerBufferedGraphics();           // 내부 필드 접근이 어려우면 생략 가능
            //var gfx = ctx.Allocate(visionImageViewer.CreateGraphics(),
            //                       new Rectangle(Point.Empty, visionImageViewer.Size));
            //SetViewerBufferedGraphics(gfx, old);             // 동일: 접근 어려우면 생략 가능

            // (b) 스케일/센터 리셋
            visionImageViewer.Scale.Wheel = 1.0;
            visionImageViewer.Scale.SetMousePoint(new Point(cam.Resolution.Width / 2, cam.Resolution.Height / 2));
            visionImageViewer.Scale.MoveCenter(new Size(cam.Resolution.Width, cam.Resolution.Height));

            // (c) 크로스라인 재구성
            visionImageViewer.InitCrossLine();
            visionImageViewer.ShowCrossLine(visionImageViewer.VisibleCrossLine);

            // (d) 첫 프레임 스냅샷으로 즉시 표시
            cam.GrabSync(out var snap);
            if (snap != null)
                visionImageViewer.SetImageNDisplay(snap);
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
